using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System.Threading;
using Tractor.Com.QuantAsylum.Tractor.HTML;


namespace Com.QuantAsylum.Tractor.Dialogs
{
    partial class DlgTestRun : Form
    {
        enum ColText { TEST = 0, ENABLED = 1, TARGET = 2, L = 3, R = 4, PASSFAIL = 5 };

        bool Abort = false;
        bool Pause = false;

        bool AbortOnFailure = false;

        string ReportDirectory = "";

        TestManager Tm;

        DateTime TestStartTime;

        public delegate void TestRunComplete();
        public TestRunComplete RunCompleteCallback;

        public DlgTestRun(TestManager tm, TestRunComplete runCompleteCallback, string reportDir)
        {
            InitializeComponent();
            ReportDirectory = reportDir;
            Tm = tm;
            RunCompleteCallback = runCompleteCallback;
        }

        private void DlgReporting_Load(object sender, EventArgs e)
        {
            dataGridView1.DefaultCellStyle.Font = new Font("Arial", 12);
            dataGridView1.RowTemplate.MinimumHeight= 25;
            dataGridView1.ReadOnly = true;

            dataGridView1.ColumnCount = Enum.GetValues(typeof(ColText)).Cast<int>().Max() + 1;
            dataGridView1.Columns[(int)ColText.TEST].HeaderText = "Test";
            dataGridView1.Columns[(int)ColText.ENABLED].HeaderText = "Enabled";
            dataGridView1.Columns[(int)ColText.TARGET].HeaderText = "Limits";
            dataGridView1.Columns[(int)ColText.L].HeaderText = "Measured L";
            dataGridView1.Columns[(int)ColText.R].HeaderText = "Measured R";
            dataGridView1.Columns[(int)ColText.PASSFAIL].HeaderText = "Pass/Fail";

            dataGridView1.RowCount = Tm.TestList.Count;

            for (int i = 0; i < Tm.TestList.Count; i++)
            {
                dataGridView1[(int)ColText.TEST, i].Value = Tm.TestList[i].Name;
                dataGridView1[(int)ColText.ENABLED, i].Value = Tm.TestList[i].RunTest;
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
            AbortOnFailure = AbortCB.Checked;

            AbortCB.Enabled = false;
            CloseBtn.Enabled = false;
            PauseBtn.Enabled = true;
            StartBtn.Enabled = false;
            StopBtn.Enabled = true;
            TestStartTime = DateTime.Now;
            timer1.Enabled = true;
            label1.Text = "Wait...";
            Start();
        }

        private void Start()
        {
            if (((IComposite)Tm.TestClass).IsConnected() == false)
                ((IComposite)Tm.TestClass).ConnectToDevices();

            ClearPassFailResultColumn();

            Abort = false;

            dataGridView1[(int)ColText.PASSFAIL, 0].Value = "Starting...";
            dataGridView1.Refresh();

            // Set everthing to defaults by specifying an empty settings file
            if (Tm.TestClass is IPowerSupply)
            {
                (Tm as IPowerSupply).SetSupplyState(false);
            }

            Thread.Sleep(750);

            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.Name = "Test Runner";

                    HtmlWriter html = new HtmlWriter(ReportDirectory);

                    bool allPassed = true;

                    for (int i = 0; i < Tm.TestList.Count; i++)
                    {
                        dataGridView1[(int)ColText.PASSFAIL, i].Style.BackColor = Color.White;
                    }

                    for (int i = 0; i < Tm.TestList.Count; i++)
                    {
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
                            dataGridView1[(int)ColText.TARGET, i].Value = Tm.TestList[i].GetTestLimitsString();
                            dataGridView1.FirstDisplayedScrollingRowIndex = i > 2 ? i-2 : 0;
                        });

                        while (Pause)
                        {
                            Thread.Sleep(500);
                        }

                        TestResult tr = null;

                        for (int j = 0; j < Tm.TestList[i].RetryCount; j++)
                        {
                            if (j > 0)
                            {
                                dataGridView1.Invoke((MethodInvoker)delegate
                                {
                                    dataGridView1[(int)ColText.PASSFAIL, i].Value = "Retry: " + j.ToString();
                                });
                            }

                            Tm.TestList[i].DoTest(Tm.TestList[i].Name, out tr);

                            if (tr.Pass)
                                break;
                        }

                        if (AbortOnFailure && (tr.Pass == false) )
                        {
                            Abort = true;
                        }

                        if (tr.Pass == false)
                            allPassed = false;

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
                        if (i == 0 && (Tm.TestList[0] is IdInput01))
                        {
                            html.AddHeading2(string.Format("Device SN: {0}", tr.StringValue[0]));
                        }

                        html.AddParagraph(string.Format("<B>Test Name:</B> {0}  <B>Result L:</B> {1}   <B>Result R:</B> {2} <B>P/F</B>: {3} <B>Image:</B> {4}",
                            Tm.TestList[i].Name,
                            tr.StringValue[0],
                            tr.StringValue[1],
                            tr.Pass ? "PASS" : "<mark>FAIL</mark>",
                            Tm.TestList[i].TestResultBitmap == null ? "[No Image]" : html.ImageLink("Screen", Tm.TestList[i].TestResultBitmap)
                            ));

                        if (IsConnected() == false) return;
                    }
                    TimeSpan ts = DateTime.Now.Subtract(TestStartTime);
                    html.AddParagraph(string.Format("Elapsed Time: {0:N1} sec", ts.TotalSeconds));

                    html.Render();
                    this.Invoke(((MethodInvoker)delegate { TestPassFinished(allPassed); }));
                }
                catch (Exception ex)
                {
                    
                }

                if (Tm.TestClass is IPowerSupply)
                {
                    (Tm.TestClass as IPowerSupply).SetSupplyState(false);
                }

            }).Start();
        }

        private void TestPassFinished(bool allTestPassed)
        {
            if (Tm.TestClass is IPowerSupply)
            {
                (Tm.TestClass as IPowerSupply).SetSupplyState(false);
            }

            timer1.Enabled = false;

            DlgPassFail dlg = new DlgPassFail(allTestPassed ? "PASS" : "FAIL", allTestPassed);
            dlg.ShowDialog();

            AbortCB.Enabled = false;
            StartBtn.Enabled = true;
            PauseBtn.Enabled = false;
            StopBtn.Enabled = false;
            CloseBtn.Enabled = true;
            RunCompleteCallback?.Invoke();
        }

        private bool IsConnected()
        {
            if (((IComposite)Tm.TestClass).IsConnected() == false)
            {
                MessageBox.Show("Unable to connect to the equipment. Is it connected and the QA40x application running?");
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
            Close();
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
    }
}
