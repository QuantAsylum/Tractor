using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System.Threading;
using Tractor.Com.QuantAsylum.Tractor.HTML;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Dialogs
{
    partial class DlgTestRun : Form
    {
        enum ColText { TEST = 0, ENABLED = 1, TARGET = 2, L = 3, R = 4, PASSFAIL = 5};

        bool Abort = false;
        bool Pause = false;

        string ReportDirectory = "";

        TestManager Tm;

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
            dataGridView1.ReadOnly = true;

            dataGridView1.ColumnCount = Enum.GetValues(typeof(ColText)).Cast<int>().Max()+1;
            dataGridView1.Columns[(int)ColText.TEST].HeaderText = "Test";
            dataGridView1.Columns[(int)ColText.ENABLED].HeaderText = "Enabled";
            dataGridView1.Columns[(int)ColText.TARGET].HeaderText = "Target";
            dataGridView1.Columns[(int)ColText.L].HeaderText = "Measured L";
            dataGridView1.Columns[(int)ColText.R].HeaderText = "Measured R";
            dataGridView1.Columns[(int)ColText.PASSFAIL].HeaderText = "Pass/Fail";

            dataGridView1.RowCount = Tm.TestList.Count;

            for (int i=0; i<Tm.TestList.Count; i++)
            {
                dataGridView1[(int)ColText.TEST, i].Value = Tm.TestList[i].Name;
                dataGridView1[(int)ColText.ENABLED, i].Value = Tm.TestList[i].RunTest;
            }

            dataGridView1.Refresh();

            IntPtr handle = dataGridView1.Handle;
        }

        private void ClearPassFailColumn()
        {
            for (int i=0; i<dataGridView1.RowCount; i++)
            {
                dataGridView1[(int)ColText.PASSFAIL, i].Value = "";
            }

            dataGridView1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CloseBtn.Enabled = false;
            PauseBtn.Enabled = true;
            StartBtn.Enabled = false;
            StopBtn.Enabled = true;
            Start();
        }

        private void Start()
        {
            if (Tm.AllConnected() == false)
                Tm.ConnectToDevices();

            ClearPassFailColumn();

            Abort = false;

            dataGridView1[(int)ColText.PASSFAIL, 0].Value = "Starting...";
            dataGridView1.Refresh();

            // Set everthing to defaults by specifying an empty settings file
            //string s = Tm.QA401.GetName();
            //s = QATestManager.QA450.GetName();
            Tm.AudioAnalyzerSetDefaults();
            //Tm.QA401.SetLog(true);
            Tm.SetInputRange(Tm.GetInputRanges()[0]);
            Tm.AudioAnalyzerSetFftLength(16384);
            //QATestManager.QA401.SetUnits(QA401.UnitsType.dBV);
            
            Thread.Sleep(500);

            new Thread(() =>
            {
                Thread.CurrentThread.Name = "Test Runner";

                HtmlWriter html = new HtmlWriter(ReportDirectory);

                for (int i = 0; i < Tm.TestList.Count; i++)
                {
                    float[] value = new float[2] { float.NaN, float.NaN };
                    bool pass = false;

                    if (Abort)
                    {
                        dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = "Aborted..."; });
                        break;
                    }

                    while (Pause)
                        Thread.Sleep(500);

                    Tm.TestList[i].DoTest(out value, out pass);

                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.L, i].Value = float.IsNaN(value[0]) ? "---" : value[0].ToString("0.000"); });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.R, i].Value = float.IsNaN(value[1]) ? "---" : value[1].ToString("0.000"); });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = pass ? "Pass" : "Fail"; });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1.Refresh(); });

                    if (i==0 && (Tm.TestList[0] is  IdInput01))
                    {
                        html.AddHeading1(string.Format("Test ID: {0}", (Tm.TestList[0] as IdInput01).Id));
                    }
                    else
                    {
                        html.AddParagraph(string.Format("<B>Test Name:</B> {0}  <B>Result L:</B> {1}   <B>Result R:</B> {2} <B>P/F</B>: {3} <B>Image:</B> {4}", 
                            Tm.TestList[i].GetTestName(), 
                            Tm.TestList[i].LeftChannel ? value[0].ToString("0.00") : "---",
                            Tm.TestList[i].RightChannel ? value[1].ToString("0.00") : "---",
                            pass == true ? "PASS" : "<mark>FAIL</mark>",
                            Tm.TestList[i].TestResultBitmap == null ? "[No Image]" : html.ImageLink("Screen", Tm.TestList[i].TestResultBitmap)
                            ));
                    }

                    if (IsConnected() == false) return;
                }

                html.Render();
                this.Invoke(((MethodInvoker)delegate { TestPastFinished(); }));

            }).Start();
        }

        private void TestPastFinished()
        {
            StartBtn.Enabled = true;
            PauseBtn.Enabled = false;
            StopBtn.Enabled = false;
            CloseBtn.Enabled = true;
            RunCompleteCallback?.Invoke();
        }

        private bool IsConnected()
        {
            if (Tm.AllConnected() == false)
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
    }
}
