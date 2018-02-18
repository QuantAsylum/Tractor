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

namespace Com.QuantAsylum.Tractor.Dialogs
{
    public partial class DlgTestRun : Form
    {
        enum ColText { TEST = 0, ENABLED = 1, TARGET = 2, L = 3, R = 4, PASSFAIL = 5};

        bool Abort = false;

        string ReportDirectory = "";

        public DlgTestRun(string directory)
        {
            InitializeComponent();
            ReportDirectory = directory;
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

            dataGridView1.RowCount = TestManager.TestList.Count;

            for (int i=0; i<TestManager.TestList.Count; i++)
            {
                dataGridView1[(int)ColText.TEST, i].Value = TestManager.TestList[i].Name;
                dataGridView1[(int)ColText.ENABLED, i].Value = TestManager.TestList[i].RunTest;
            }

            dataGridView1.Refresh(); 
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
            Start();
        }

        private void Start()
        {
            if (TestManager.AudioAnalyzer == null)
                TestManager.ConnectQA401();

            if (IsConnected() == false) return;

            ClearPassFailColumn();

            Abort = false;

            dataGridView1[(int)ColText.PASSFAIL, 0].Value = "Starting...";
            dataGridView1.Refresh();

            // Set everthing to defaults by specifying an empty settings file
            TestManager.AudioAnalyzer.SetToDefault("");
            TestManager.AudioAnalyzer.SetLog(true);
            TestManager.AudioAnalyzer.SetInputAtten(QA401.InputAttenState.NoAtten);
            TestManager.AudioAnalyzer.SetBufferLength(16384);
            TestManager.AudioAnalyzer.SetUnits(QA401.UnitsType.dBV);
            
            Thread.Sleep(500);

            new Thread(() =>
            {
                HtmlWriter html = new HtmlWriter(ReportDirectory);

                for (int i = 0; i < TestManager.TestList.Count; i++)
                {
                    float[] value = new float[2] { float.NaN, float.NaN };
                    bool pass = false;

                    if (Abort)
                    {
                        dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = "Aborted..."; });
                        break;
                    }

                    // In here, we're running in another thread and so we need to switch from 
                    // that thread back to the UI threa to update the UI. 
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = "Running..."; });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1.Refresh(); });

                    TestManager.TestList[i].DoTest(out value, out pass);

                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.L, i].Value = value[0].ToString("0.0"); });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.R, i].Value = value[1].ToString("0.0"); });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1[(int)ColText.PASSFAIL, i].Value = pass ? "Pass" : "Fail"; });
                    dataGridView1.Invoke((MethodInvoker)delegate { dataGridView1.Refresh(); });

                    if (i==0 && (TestManager.TestList[0] is  IdInput01))
                    {
                        html.AddHeading1(string.Format("Test ID: {0}", (TestManager.TestList[0] as IdInput01).Id));
                    }
                    else
                    {
                        html.AddParagraph(string.Format("<B>Test Name:</B> {0}  <B>Result L:</B> {1}   <B>Result R:</B> {2} <B>P/F</B>: {3} <B>Image:</B> {4}", 
                            TestManager.TestList[i].GetTestName(), 
                            TestManager.TestList[i].LeftChannel ? value[0].ToString("0.00") : "---",
                            TestManager.TestList[i].RightChannel ? value[1].ToString("0.00") : "---",
                            pass == true ? "PASS" : "<mark>FAIL</mark>",
                            TestManager.TestList[i].TestResultBitmap == null ? "[No Image]" : html.ImageLink("Screen", TestManager.TestList[i].TestResultBitmap)
                            ));
                    }


                    //if (TestManager.TestList[i].TestResultBitmap != null)
                    //{
                    //    html.AddImageLink("Screen", TestManager.TestList[i].TestResultBitmap);
                    //}

                    if (IsConnected() == false) return;
                }

                html.Render();
            }).Start();
        }

        private bool IsConnected()
        {
            if (TestManager.AudioAnalyzer == null)
            {
                MessageBox.Show("Unable to connect to the QA40x Audio Analyzer. Is it connected and the QA40x application running?");
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
    }
}
