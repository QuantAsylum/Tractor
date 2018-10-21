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

        /// <summary>
        /// Keeps track if the TestManager currently loaded has changed
        /// </summary>
        bool TmIsDirty = false;

        TestBase SelectedTb;

        /// <summary>
        /// A workaround...sometimes we don't want programmatic changes to 
        /// the treeview to trigger more code. So, we set this true.
        /// </summary>
        bool IgnoreTreeViewBeforeSelects = false;

        public Form1()
        {
            This = this;
            InitializeComponent();

            label3.Text = "";
            Tm = new QATestManager();
            Tm.SetCallbacks(StartEditing, DoneEditing, CancelEditing);
        }

        /// <summary>
        /// Called when user begins editing a test
        /// </summary>
        internal void StartEditing()
        {
            RunTestsBtn.Enabled = false;
            MoveUpBtn.Enabled = false;
            MoveDownBtn.Enabled = false;
            DeleteBtn.Enabled = false;
            treeView1.Enabled = false;
            menuStrip1.Enabled = false;
            AddTestBtn.Enabled = false;
        }

        /// <summary>
        /// Called when user finishes editing a test
        /// </summary>
        internal void DoneEditing()
        {
            treeView1.Enabled = true;
            menuStrip1.Enabled = true;
            AddTestBtn.Enabled = true;
            TmIsDirty = true;
            SetTreeviewControls();
        }

        /// <summary>
        /// Called when user cancels editing a test
        /// </summary>
        internal void CancelEditing()
        {
            treeView1.Enabled = true;
            menuStrip1.Enabled = true;
            AddTestBtn.Enabled = true;
            RePopulateTreeView();
            SetTreeviewControls();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Constants.DataFilePath);
            Directory.CreateDirectory(Constants.TestLogsPath);


            Text = Constants.TitleBarText + " " + Constants.Version.ToString("0.00") + Constants.VersionSuffix;

            DefaultTreeview();

            SetTreeviewControls();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Make sure power is off
            Tm.DutSetPowerState(false);

            // Here, the data is clean. Check if we need to save the current TestManager data
            if (TmIsDirty || ((SelectedTb != null) && SelectedTb.IsDirty == true))
            {
                if (MessageBox.Show("Do you want to save the current test plan?", "Changes not saved", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    saveTestPlanToolStripMenuItem_Click(null, null);
                }
            }
        }

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

            if ((test as TestBase).RunTest)
                root.Checked = true;

            treeView1.Nodes.Add(root);
        }

        /// <summary>
        /// Populates the treeview based on the data in the TestManager. This tries to keep the current node
        /// selected unless another ID to highlight is presented
        /// </summary>
        internal void RePopulateTreeView(string highlightId = "", bool ignoreBeforeSelects = false)
        {
            if ((highlightId == "") && (treeView1.SelectedNode != null))
            {
                var tn = treeView1.Nodes.Cast<TreeNode>().First(o => o.Text.Contains(treeView1.SelectedNode.Text));
                highlightId = tn.Text;
            }

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
        /// Called after a node has been selected. This will update the UI in the 
        /// righthand panel
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
        /// Called after a checkbox in the treeview has been checked (or unchecked)
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
                TmIsDirty = true;
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

        /// <summary>
        /// Pops a modal dlg where the user is given a chance to review the tests and then execute them
        /// over and over if desired. This doesn't return until the user closes the dlg
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTestBtn_Click(object sender, EventArgs e)
        {
            if (Tm.TestList.Count == 0)
                return;

            DlgTestRun dlg = new DlgTestRun(Tm, TestRunCallback, Constants.TestLogsPath);

            this.Visible = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {

            }
            else
            {

            }
            this.Visible = true;
        }
      
        // Called when current tests are done running. Right now, we don't use this.
        public void TestRunCallback()
        {

        }

        /// <summary>
        /// Called when user wants to exit app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (TmIsDirty)
            //{
            //    MessageBox.Show("Changes not saved", "Save data?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //    {

            //    }
            //}

            Close();
        }

        /// <summary>
        /// Deletes the currently selected test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            Tm.TestList.RemoveAt(treeView1.SelectedNode.Index);
            TmIsDirty = true;
            RePopulateTreeView();

            SetTreeviewControls();
        }

        /// <summary>
        /// Loads a test plan from the file system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadTestPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Constants.DataFilePath;
            ofd.Filter = "Test Profile files (*.tp)|*.tp|All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Tm.TestList = (List<TestBase>)SerDes.Deserialize(typeof(List<TestBase>), File.ReadAllText(ofd.FileName));
                TmIsDirty = false;

                foreach (TestBase test in Tm.TestList)
                {
                    test.SetTestManager(Tm);
                }
                RePopulateTreeView();
            }
        }

        /// <summary>
        /// Saves a test plan to the file system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveTestPlanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = Constants.DataFilePath;
            sfd.Filter = "Test Profile files (*.tp)|*.tp|All files (*.*)|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, SerDes.Serialize(Tm.TestList));
                TmIsDirty = false;
            }
        }

        /// <summary>
        /// Moves a test up in the treeview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTb != null)
            {
                int index = Tm.TestList.IndexOf(SelectedTb);

                if (index == 0)
                    return;

                Tm.TestList.RemoveAt(index);
                Tm.TestList.Insert(index - 1, SelectedTb);
                TmIsDirty = true;
                RePopulateTreeView(SelectedTb.Name);
            }
        }

        /// <summary>
        /// Moves a test down in the treeview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownBtn_Click(object sender, EventArgs e)
        {
            if (SelectedTb != null)
            {
                int index = Tm.TestList.IndexOf(SelectedTb);

                if (index == Tm.TestList.Count - 1)
                    return;

                Tm.TestList.RemoveAt(index);
                Tm.TestList.Insert(index + 1, SelectedTb);
                TmIsDirty = true;
                RePopulateTreeView(SelectedTb.Name);
            }
        }
    }
}
