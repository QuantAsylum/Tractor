using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Com.QuantAsylum.Tractor.Settings;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using Com.QuantAsylum.Tractor.Tests.GainTests;
using Com.QuantAsylum.Tractor.Ui.Extensions;
using Com.QuantAsylum.Tractor.Dialogs;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Tractor
{
    public partial class Form1 : Form
    {
        internal static Form1 This;

        TestManager Tm;

        TestBase SelectedTb;

        bool IgnoreTreeViewBeforeSelects = false;

        public Form1()
        {
            This = this;
            InitializeComponent();

            label3.Text = "";
            Tm = new QATestManager();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Constants.DataFilePath);
            Directory.CreateDirectory(Constants.TestLogsPath);


            Text = Constants.TitleBarText;

            DefaultTreeview();

            SetTreeviewControls();
        }

        // This method is needed by treeview extensions
        //void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        //{
        //    if (e.Node.Level == 1) e.Node.HideCheckBox();
        //    e.DrawDefault = true;
        //}

        /// <summary>
        /// Sets treeview defaults
        /// </summary>
        private void DefaultTreeview()
        {
            treeView1.CheckBoxes = true;
            treeView1.HideSelection = false;
        }

        /// <summary>
        /// Adds a node to the treeview
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="test"></param>
        private void TreeViewAdd(string testName, TestBase test)
        {
            TreeNode root = new TreeNode();
            root.Text = testName + "   [" + (test as TestBase).GetTestName() + "]";
            //TreeNode sub = new TreeNode();
            //sub.Text = "Test: " + (test as ITest).GetTestName();
            //root.Nodes.Add(sub);

            if ((test as TestBase).RunTest)
                root.Checked = true;

            treeView1.Nodes.Add(root);
        }

        /// <summary>
        /// Populates the treeview based on the data in the TestManager
        /// </summary>
        internal void RePopulateTreeView(string highlightId = "", bool ignoreBeforeSelects = false)
        {
            bool oldIgnore = IgnoreTreeViewBeforeSelects;

            IgnoreTreeViewBeforeSelects = ignoreBeforeSelects;
            treeView1.Nodes.Clear();

            for (int i = 0; i < Tm.TestList.Count(); i++)
            {
                TreeViewAdd((Tm.TestList[i] as TestBase).Name, Tm.TestList[i]);
            }

            treeView1.ExpandAll();

            if (highlightId != "")
            {
                TreeNode[] tn = treeView1.Nodes.Cast<TreeNode>().Where(o => o.Text.Contains(highlightId)).ToArray();

                if (tn.Length > 0)
                    treeView1.SelectedNode = tn[0];
            }
            else
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }

            IgnoreTreeViewBeforeSelects = oldIgnore;


        }

        /// <summary>
        /// Given a string such as "abctest0 [xyz]" this returns abctest0
        /// </summary>
        /// <param name="testString"></param>
        /// <returns></returns>
        private string GetTestName(string testString)
        {
            string[] toks = testString.Split('[', ']');

            return toks[0].Trim();
        }

        /// <summary>
        /// Given a string such as "abctest0 [xyz]" this returns xyz
        /// </summary>
        /// <param name="testString"></param>
        /// <returns></returns>
        private string GetTestClass(string testString)
        {
            string[] toks = testString.Split('[', ']');

            return toks[1].Trim();
        }


        /// <summary>
        /// Called before a selection is made in the treeview. This routine can cancel it if needed.
        /// Here, we use this as a way to determine if user changes need to be made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (IgnoreTreeViewBeforeSelects)
                return;

            if (SelectedTb != null && SelectedTb.IsDirty)
            {
                IgnoreTreeViewBeforeSelects = true;

                if (MessageBox.Show("The selected test profile has changed. Save changes to current Test?", "Unsaved Changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string error;
                    if (SelectedTb.SaveChanges(out error) == false)
                    {
                        if (MessageBox.Show("Incorrect data has been entered: " + error + Environment.NewLine + Environment.NewLine + "Press Retry to continue editing. Press Cancel to revert to previous values", "Invalid Data", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            //Form1.This.RePopulateTreeView(this.Name, true);
                            
                        }
                    }
                }
                else
                {
                    SelectedTb.IsDirty = false;
                }

                IgnoreTreeViewBeforeSelects = false;
            }
        }

        /// <summary>
        /// Called after a node has been selected. This will update the UI in the 
        /// right panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ClearEditFields();

            if (e.Node.Level != 0)
                return;

            ClearEditFields();
            TestBase tb = Tm.TestList.Find(o => o.Name == GetTestName(e.Node.Text));
            SelectedTb = tb;
            tb.PopulateUI(tableLayoutPanel1);
            label3.Text = tb.GetTestDescription();

            SetTreeviewControls();
        }

        /// <summary>
        /// Called after a checkbox has been checked (or unchecked)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TestBase tb = Tm.TestList.Find(o => o.Name == GetTestName(e.Node.Text));
            SelectedTb = tb;
            tb.RunTest = e.Node.Checked;
        }

        /// <summary>
        /// Sets button on/off states depending on treeview state
        /// </summary>
        private void SetTreeviewControls()
        {
            if (treeView1.Nodes.Count == 0)
                RunTestsBtn.Enabled = false;
            else
                RunTestsBtn.Enabled = true;

            if (treeView1.SelectedNode == null)
            {
                MoveUpBtn.Enabled = false;
                MoveDownBtn.Enabled = false;
                DeleteBtn.Enabled = false;
                return;
            }

            if (treeView1.SelectedNode != null)
                DeleteBtn.Enabled = true;

            if (treeView1.SelectedNode.Index == 0)
                MoveUpBtn.Enabled = false;
            else
                MoveUpBtn.Enabled = true;

            if (treeView1.SelectedNode.Index == treeView1.Nodes.Count - 1)
            {
                MoveDownBtn.Enabled = false;
            }
            else
            {
                MoveDownBtn.Enabled = true;
            }


        }

        /// <summary>
        /// Add Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            DlgAddTest dlg = new DlgAddTest(Tm);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //string className = (string)(dlg.comboBox1.Items[dlg.comboBox1.SelectedIndex]);
                string className = dlg.GetSelectedTestName();

                TestBase testInst = CreateTestInstance(className);
                testInst.Tm = Tm;
                (testInst as TestBase).Name = dlg.textBox1.Text;
                Tm.TestList.Add(testInst as TestBase);

                TreeViewAdd(dlg.textBox1.Text, CreateTestInstance(className));
            }

            SetTreeviewControls();
        }

        /// <summary>
        /// Creates an instance based on the classname. This is used when the user
        /// specifies a particular test they'd like to run. That test name is mapped
        /// to a class, and then an instance of that class is created
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private TestBase CreateTestInstance(string className)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var type = assembly.GetTypes().First(t => t.Name == className);

            return (TestBase)Activator.CreateInstance(type);
        }

        private void ClearEditFields()
        {
            for (int i = tableLayoutPanel1.Controls.Count - 1; i >= 0; --i)
                tableLayoutPanel1.Controls[i].Dispose();

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearEditFields();

            if (Tm.TestList.Count > 0)
                (Tm.TestList[0] as TestBase).PopulateUI(tableLayoutPanel1);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("data.xml", SerDes.Serialize(Tm.TestList));
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tm.TestList = (List<TestBase>)SerDes.Deserialize(typeof(List<TestBase>), File.ReadAllText("data.xml"));
            RePopulateTreeView();
        }

        /// <summary>
        /// Pops a modal dlg where the user is given a chance to review the tests and then execute them
        /// over and over if desired. This doesn't return until the user closes the dlg
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RustTestBtn_Click(object sender, EventArgs e)
        {
            if (Tm.TestList.Count == 0)
                return;

            DlgTestRun dlg = new DlgTestRun(Tm, TestRunCallback, Constants.TestLogsPath);

            if (dlg.ShowDialog() == DialogResult.OK)
            {

            }
            else
            {

            }
        }

        // Called when current tests are done running
        public void TestRunCallback()
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            Tm.TestList.RemoveAt(treeView1.SelectedNode.Index);
            //if (treeView1.SelectedNode != null)
            //    treeView1.Nodes.Remove(treeView1.SelectedNode);
            RePopulateTreeView();

            SetTreeviewControls();
        }

        //private void saveTestProfileToolStripMenuItem_Click(object sender, EventArgs e)
        //{

        //}



        private void loadTestPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Constants.DataFilePath;
            ofd.Filter = "Test Profile files (*.tp)|*.tp|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Tm.TestList = (List<TestBase>)SerDes.Deserialize(typeof(List<TestBase>), File.ReadAllText(ofd.FileName));

                foreach (TestBase test in Tm.TestList)
                {
                    test.SetTestManager(Tm);
                }
                RePopulateTreeView();
            }
        }

        private void saveTestPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Constants.DataFilePath;
            sfd.Filter = "Test Profile files (*.tp)|*.tp|All files (*.*)|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, SerDes.Serialize(Tm.TestList));
            }
        }

        private void MoveUpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTb != null)
            {
                int index = Tm.TestList.IndexOf(SelectedTb);

                if (index == 0)
                    return;

                Tm.TestList.RemoveAt(index);
                Tm.TestList.Insert(index - 1, SelectedTb);
                RePopulateTreeView(SelectedTb.Name);
            }
        }

        private void MoveDownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTb != null)
            {
                int index = Tm.TestList.IndexOf(SelectedTb);

                if (index == Tm.TestList.Count - 1)
                    return;

                Tm.TestList.RemoveAt(index);
                Tm.TestList.Insert(index + 1, SelectedTb);
                RePopulateTreeView(SelectedTb.Name);
            }
        }
    }
}
