using Com.QuantAsylum.Tractor.Database;
using Com.QuantAsylum.Tractor.HTML;
using Com.QuantAsylum.Tractor.Settings;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Database;
using Tractor.Com.QuantAsylum.Tractor.Dialogs;

namespace Com.QuantAsylum.Tractor.Dialogs
{
    partial class DlgTestRun : Form
    {
        enum ColText { TEST = 0, ENABLED = 1, TARGET = 2, L = 3, R = 4, PASSFAIL = 5 };

        bool Abort = false;
        bool Pause = false;

        string ReportDirectory = "";
        string CsvLogDirectory = "";

        TestManager Tm;

        DateTime TestStartTime;

        public delegate void TestRunComplete();
        public TestRunComplete RunCompleteCallback;

        public DlgTestRun(TestManager tm, TestRunComplete runCompleteCallback, string reportDir, string csvLogDir)
        {
            InitializeComponent();
            ReportDirectory = reportDir;
            CsvLogDirectory = csvLogDir;
            Tm = tm;
            RunCompleteCallback = runCompleteCallback;
        }

        private void DlgReporting_Load(object sender, EventArgs e)
        {
            statusStrip1.Items.Add(new ToolStripStatusLabel("---"));
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 12);
            dataGridView1.RowTemplate.MinimumHeight = 25;
            dataGridView1.ReadOnly = true;

            dataGridView1.ColumnCount = Enum.GetValues(typeof(ColText)).Cast<int>().Max() + 1;
            dataGridView1.Columns[(int)ColText.TEST].HeaderText = "Test";
            dataGridView1.Columns[(int)ColText.ENABLED].HeaderText = "Enabled";
            dataGridView1.Columns[(int)ColText.TARGET].HeaderText = "Limits";
            dataGridView1.Columns[(int)ColText.L].HeaderText = "Measured L";
            dataGridView1.Columns[(int)ColText.R].HeaderText = "Measured R";
            dataGridView1.Columns[(int)ColText.PASSFAIL].HeaderText = "Pass/Fail";

            dataGridView1.RowCount = Form1.AppSettings.TestList.Count;

            for (int i = 0; i < Form1.AppSettings.TestList.Count; i++)
            {
                dataGridView1[(int)ColText.TEST, i].Value = Form1.AppSettings.TestList[i].Name;
                dataGridView1[(int)ColText.ENABLED, i].Value = Form1.AppSettings.TestList[i].RunTest;
            }

            dataGridView1.Refresh();

            IntPtr handle = dataGridView1.Handle;
        }

        private void ClearPassFailResultColumn()
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1[(int)ColText.L, i].Value = "";
                dataGridView1[(int)ColText.R, i].Value = "";
                dataGridView1[(int)ColText.PASSFAIL, i].Value = "";
            }

            dataGridView1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Checking connections...";
            label1.Update();
            if (CheckConnections(out string result))
            {
                CloseBtn.Enabled = false;
                PauseBtn.Enabled = true;
                StartBtn.Enabled = false;
                StopBtn.Enabled = true;
                TestStartTime = DateTime.Now;
                timer1.Enabled = true;
                label1.Text = "Wait...";
                Start();
            }
            else
            {
                MessageBox.Show("The test could not be started: " + result);
                label1.Text = "Failed to start.";
            }
        }

        private bool CheckConnections(out string result)
        {
            ((IInstrument)Tm.TestClass).LaunchApplication();
            bool ok = ((IInstrument)Tm.TestClass).ConnectToDevice(out result);
            if (ok && ((IInstrument)Tm.TestClass).IsConnected())
            {
                return true;
            }

            return false;
        }

        private void Start()
        {
            Abort = false;

            ClearPassFailResultColumn();

            dataGridView1[(int)ColText.PASSFAIL, 0].Value = "Starting...";
            dataGridView1.Refresh();

            if (Tm.TestClass is IPowerSupply)
            {
                (Tm.TestClass as IPowerSupply).SetSupplyState(false);
                Thread.Sleep(750);
            }

            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.Name = "Test Runner";
                    Log.WriteLine(LogType.General, "Test thread started.");

                    if (Form1.AppSettings.UseDb)
                    {
                        Db.OpenExisting(Form1.AppSettings.DbConnectString);
                        Log.WriteLine(LogType.General, "Database opened");
                    }

                    HtmlWriter html = new HtmlWriter(ReportDirectory);

                    bool allPassed = true;
                    string opMessage = "";

                    // Tests that run together (eg left and right channels) are part of the same test group
                    Guid testGroup = Guid.NewGuid();

                    for (int i = 0; i < Form1.AppSettings.TestList.Count; i++)
                    {
                        dataGridView1[(int)ColText.PASSFAIL, i].Style.BackColor = Color.White;
                    }

                    Tm.LocalStash = new Dictionary<string, string>();

                    for (int i = 0; i < Form1.AppSettings.TestList.Count; i++)
                    {
                        if (Form1.AppSettings.TestList[i].RunTest == false)
                        {
                            dataGridView1[(int)ColText.PASSFAIL, i].Value = "Skipped";
                            continue;
                        }

                        if (Abort)
                        {
                            dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = "Aborted..."; });
                            allPassed = false;
                            break;
                        }
                        else
                        {
                            dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = "Running..."; });
                        }

                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            dataGridView1[(int)ColText.TARGET, i].Value = Form1.AppSettings.TestList[i].GetTestLimits();
                            dataGridView1.FirstDisplayedScrollingRowIndex = i > 2 ? i - 2 : 0;
                        });

                        while (Pause)
                        {
                            Thread.Sleep(500);
                        }

                        TestResult tr = null;

                        if (Form1.AppSettings.TestList[i] is AudioTestBase)
                        {
                            for (int j = 0; j < ((AudioTestBase)Form1.AppSettings.TestList[i]).RetryCount; j++)
                            {
                                if (j > 0)
                                {
                                    dataGridView1.Invoke((MethodInvoker)delegate
                                    {
                                        dataGridView1[(int)ColText.PASSFAIL, i].Value = "Retry: " + j.ToString();
                                    });
                                }

                                Form1.AppSettings.TestList[i].DoTest(Form1.AppSettings.TestList[i].Name, out tr);

                                if (tr.Pass)
                                    break;
                            }
                        }
                        else
                        {
                            Form1.AppSettings.TestList[i].DoTest(Form1.AppSettings.TestList[i].Name, out tr);
                        }

                        if (Form1.AppSettings.AbortOnFailure && (tr.Pass == false))
                        {
                            Abort = true;
                        }

                        if (tr.Pass == false)
                        {
                            allPassed = false;
                        }

                        // any test can update the operator message. But only the last test that updates
                        // the operator message will have its result shown.
                        if (tr.OperatorMessage != "")
                        {
                            opMessage = tr.OperatorMessage;
                        }

                        dataGridView1.Invoke((MethodInvoker)delegate
                        {
                            dataGridView1[(int)ColText.L, i].Value = tr.StringValue[0];
                            dataGridView1[(int)ColText.R, i].Value = tr.StringValue[1];
                            dataGridView1[(int)ColText.PASSFAIL, i].Value = tr.Pass ? "PASS" : "FAIL";
                            dataGridView1[(int)ColText.PASSFAIL, i].Style.BackColor = tr.Pass ? Color.Green : Color.Red;
                            dataGridView1.Refresh();
                        });

                        // Delineate new tests
                        if (i == 0)
                        {
                            html.AddParagraph("================================================");
                            html.AddHeading2(string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
                        }

                        // If new test AND first test is serial number, then print it larger
                        if (i == 0 && (Form1.AppSettings.TestList[0] is IdInputA00))
                        {
                            html.AddHeading2(string.Format("Device SN: {0}", tr.StringValue[0]));
                        }

                        html.AddParagraph(string.Format("<B>Test Name:</B> {0}  <B>Result L:</B> {1}   <B>Result R:</B> {2} <B>P/F</B>: {3} <B>Image:</B> {4}",
                            Form1.AppSettings.TestList[i].Name,
                            tr.StringValue[0],
                            tr.StringValue[1],
                            tr.Pass ? "PASS" : "<mark>FAIL</mark>",
                            Form1.AppSettings.TestList[i].TestResultBitmap == null ? "[No Image]" : html.ImageLink("Screen", Form1.AppSettings.TestList[i].TestResultBitmap)
                            ));

                        if (Form1.AppSettings.UseCsvLog) 
                        {
                            string fileName = Path.Combine(Constants.CsvLogsPath, Form1.AppSettings.CsvFileName);

                            if (File.Exists(fileName) == false)
                            {
                                try
                                {
                                    Log.WriteLine(LogType.General, "CSV Log file created: " + fileName);
                                    File.AppendAllText(fileName, GetCsvHeader());
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLine(LogType.Error, "CSV Log File could not be created. " + ex.Message);
                                    throw new InvalidOperationException("Unabled to write to CSV log file. Disable CSV logging to bypass.");
                                }
                            }

                            if (Form1.AppSettings.TestList[i] is AudioTestBase)
                            {
                                if (((AudioTestBase)Form1.AppSettings.TestList[i]).LeftChannel)
                                {
                                    SubmitToCsv(fileName, testGroup, 0, Form1.AppSettings.TestList[i], tr);
                                }
                                if (((AudioTestBase)Form1.AppSettings.TestList[i]).RightChannel)
                                {
                                    SubmitToCsv(fileName, testGroup, 1, Form1.AppSettings.TestList[i], tr);
                                }
                            }
                            else
                            {
                                SubmitToCsv(fileName, testGroup, 0, Form1.AppSettings.TestList[i], tr);
                            }
                        }

                        if (Form1.AppSettings.UseAuditDb)
                        {
                            if (Form1.AppSettings.TestList[i] is AudioTestBase)
                            {
                                if (((AudioTestBase)Form1.AppSettings.TestList[i]).LeftChannel)
                                {
                                    SubmitToAuditDb(testGroup, 0, Form1.AppSettings.TestList[i], tr);
                                }
                                if (((AudioTestBase)Form1.AppSettings.TestList[i]).RightChannel)
                                {
                                    SubmitToAuditDb(testGroup, 1, Form1.AppSettings.TestList[i], tr);
                                }
                            }
                            else
                            {
                                SubmitToAuditDb(testGroup, 0, Form1.AppSettings.TestList[i], tr);
                            }
                        }

                        // Add to database if needed
                        if (Form1.AppSettings.UseDb)
                        {
                            if (Form1.AppSettings.TestList[i] is AudioTestBase)
                            {
                                // Left channel
                                if (((AudioTestBase)Form1.AppSettings.TestList[i]).LeftChannel)
                                {
                                    SubmitToDb(testGroup, 0, Form1.AppSettings.TestList[i], tr);
                                }
                                if (((AudioTestBase)Form1.AppSettings.TestList[i]).RightChannel)
                                {
                                    SubmitToDb(testGroup, 1, Form1.AppSettings.TestList[i], tr);
                                }
                            }
                            else
                            {
                                SubmitToDb(testGroup, 0, Form1.AppSettings.TestList[i], tr);
                            }
                        }

                        if (IsConnected() == false)
                        {
                            Log.WriteLine(LogType.Error, "Equipment is no longer connected. Testing will now stop.");
                            dataGridView1[(int)ColText.PASSFAIL, i].Value = "ERROR";
                            throw new InvalidOperationException("Equipment is no longer connected. Testing will now stop.");
                        }
                    }
                    TimeSpan ts = DateTime.Now.Subtract(TestStartTime);
                    html.AddParagraph(string.Format("Elapsed Time: {0:N1} sec", ts.TotalSeconds));

                    html.Render();
                    this.Invoke(((MethodInvoker)delegate { TestPassFinished(allPassed, opMessage); }));
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogType.Error, ex.Message);
                    this.Invoke(((MethodInvoker)delegate { TestPassFinished(false, "See Log"); }));
                }

                if (Tm.TestClass is IPowerSupply)
                {
                    (Tm.TestClass as IPowerSupply).SetSupplyState(false);
                }

            }).Start();
        }

        private void SubmitToDb(Guid testGroup, int channelIndex, TestBase tb, TestResult tr)
        {
            Test tri = new Test()
            {
                SerialNumber = Tm.LocalStash.ContainsKey("SerialNumber") ? Tm.LocalStash["SerialNumber"] : "0",
                SessionName = Form1.AppSettings.SessionName,
                Name = tb.Name,
                Time = DateTime.Now,
                PassFail = tr.Pass,
                ResultString = tr.StringValue[channelIndex],
                Result = (float)tr.Value[channelIndex],
                TestGroup = testGroup.ToString(),
                TestFile = Form1.SettingsFile,
                TestFileMD5 = ComputeMd5(Form1.AppSettings),
                TestLimits = tb.GetTestLimits(),
                ImageArray = tb.TestResultBitmap != null ? TestResultDatabase.BmpToBytes(tb.TestResultBitmap) : null
            };
            Db.InsertTest(tri);
        }

        private void SubmitToAuditDb(Guid testGroup, int channelIndex, TestBase tb, TestResult tr)
        {
            AuditData d = new AuditData()
            {
                ProductId = Form1.AppSettings.ProductId.ToString(),
                SerialNumber = Tm.LocalStash.ContainsKey("SerialNumber") ? Tm.LocalStash["SerialNumber"] : "0",
                SessionName = Form1.AppSettings.SessionName,
                Channel = (channelIndex == 0 ? "Left" : "Right"),
                Name = tb.Name,
                TestGroup = testGroup.ToString(),
                TestFile = Form1.SettingsFile,
                TestFileMd5 = ComputeMd5(Form1.AppSettings),
                Time = DateTime.Now,
                PassFail = tr.Pass,
                ResultString = tr.StringValue[channelIndex],
                Result = (float)tr.Value[channelIndex],
                TestLimits = tb.GetTestLimits(),
                Email = Form1.AppSettings.AuditDbEmail
            };
            AuditDb.SubmitAuditData(d);
        }

        private string GetCsvHeader()
        {
            string s = "ProductId,SerialNumber,SessionName,Channel,Name,TestGroup,TestFile,TestFileMd5,Time,PassFail,ResultString,Result,TestLimits" + Environment.NewLine;
            return s;
        }

        private void SubmitToCsv(string fileName, Guid testGroup, int channelIndex, TestBase tb, TestResult tr)
        {
            string s =
                Form1.AppSettings.ProductId.ToString() + "," +
                (Tm.LocalStash.ContainsKey("SerialNumber") ? Tm.LocalStash["SerialNumber"] : "0") + "," +
                Form1.AppSettings.SessionName + "," +
                (channelIndex == 0 ? "Left" : "Right") + "," +
                tb.Name + "," +
                testGroup.ToString() + "," +
                Form1.SettingsFile + "," +
                ComputeMd5(Form1.AppSettings) + "," +
                DateTime.Now + "," +
                tr.Pass + "," +
                tr.StringValue[channelIndex] + "," +
                tr.Value[channelIndex].ToString("0.000", CultureInfo.InvariantCulture) + "," +
                tb.GetTestLimits() + "," +
                Environment.NewLine;

                try
                {
                    File.AppendAllText(fileName, s);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogType.Error, $"Failed to write line to CSV file. Filename: {fileName} Message: {ex.Message}");
                    throw new InvalidOperationException("Unabled to write to CSV log file. Disable CSV logging to bypass.");
            }
        }

        private string ComputeMd5(AppSettings settings)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(settings.Serialize());
            byte[] hash = null;

            BinaryFormatter bf = new BinaryFormatter();
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(bytes);
            }

            return Convert.ToBase64String(hash);
        }

        private void TestPassFinished(bool allTestPassed, string operatorMessage)
        {
            if (Tm.TestClass is IPowerSupply)
            {
                (Tm.TestClass as IPowerSupply).SetSupplyState(false);
            }

            timer1.Enabled = false;

            DlgPassFail dlg = new DlgPassFail(allTestPassed ? "PASS" : "FAIL", allTestPassed, operatorMessage);
            dlg.ShowDialog();

            StartBtn.Enabled = true;
            PauseBtn.Enabled = false;
            StopBtn.Enabled = false;
            CloseBtn.Enabled = true;
            RunCompleteCallback?.Invoke();
        }

        private bool IsConnected()
        {
            bool connected = ((IInstrument)Tm.TestClass).IsConnected();

            if (connected == false)
            {
                MessageBox.Show("Unable to connect to the equipment. Is it connected and the QA40x application running?");
                Log.WriteLine(LogType.Error, "Unable to connect to the equipment. Is it connected and the QA40x application running?");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Abort = true;
            timer1.Enabled = false;
        }

        private void DlgTestRun_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void PauseBtn_Click(object sender, EventArgs e)
        {
            Pause = true;
            if (MessageBox.Show("Testing has been paused. Press OK to continue", "Test Paused", MessageBoxButtons.OK) == DialogResult.OK)
            {

            }
            Pause = false;
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            if (Form1.AppSettings.LockTestScreen)
            {
                DlgUnlock dlg = new DlgUnlock();

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (dlg.Password == Form1.AppSettings.Password)
                    {
                        Close();
                    }
                }
            }
            else
            {
                Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now.Subtract(TestStartTime);
            label1.Text = string.Format("Elapsed Time: {0:N1} sec", ts.TotalSeconds);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                this.Opacity = 0.6;
            else
                this.Opacity = 1.0;
        }

        // This timer provides general status updates. It runs all the time
        private void timer2_Tick(object sender, EventArgs e)
        {
            ToolStripStatusLabel lbl = (ToolStripStatusLabel)statusStrip1.Items[0];
            lbl.Text = string.Format("Audit Queue Depth: {0} items", AuditDb.AuditQueueDepth.ToString());
        }
    }
}
