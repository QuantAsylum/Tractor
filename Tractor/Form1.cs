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

namespace Tractor
{
    public partial class Form1 : Form
    {
        internal static Form1 This;

        public Form1()
        {
            This = this;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // These two lines needed by treeview extensions
            //this.treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //this.treeView1.DrawNode += new DrawTreeNodeEventHandler(treeView1_DrawNode);

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
            root.Text = testName + "   [" + (test as ITest).GetTestName() + "]";
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
        internal void RePopulateTreeView(string highlightId = "")
        {
            treeView1.Nodes.Clear();

            for (int i=0; i<TestManager.TestList.Count; i++)
            {
                TreeViewAdd((TestManager.TestList[i] as TestBase).Name, TestManager.TestList[i]);
            }

            treeView1.ExpandAll();

            if (highlightId != "")
            {
                TreeNode[] tn = treeView1.Nodes.Cast<TreeNode>().Where(o => o.Text == highlightId).ToArray();

                if (tn.Length == 1)
                    tn[0].ForeColor = Color.LightSalmon;
                
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
            TestBase tb = TestManager.TestList.Find(o => o.Name == GetTestName(e.Node.Text));
            tb.PopulateUI(tableLayoutPanel1);

            SetTreeviewControls();
        }

        /// <summary>
        /// Called after a checkbox has been checked (or unchecked)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TestBase tb = TestManager.TestList.Find(o => o.Name == GetTestName(e.Node.Text));
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

            

        private void button1_Click(object sender, EventArgs e)
        {
            DlgAddTest dlg = new DlgAddTest();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //string className = (string)(dlg.comboBox1.Items[dlg.comboBox1.SelectedIndex]);
                string className = dlg.GetSelectedTestName();
                
                TestBase testInst = CreateTestInstance(className);
                (testInst as TestBase).Name = dlg.textBox1.Text;
                TestManager.TestList.Add(testInst as TestBase);

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

            if (TestManager.TestList.Count > 0)
                (TestManager.TestList[0] as TestBase).PopulateUI(tableLayoutPanel1);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText("data.xml", SerDes.Serialize(TestManager.TestList));
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TestManager.TestList = (List<TestBase>)SerDes.Deserialize(typeof(List<TestBase>), File.ReadAllText("data.xml"));
            RePopulateTreeView();
        }

        /// <summary>
        /// Pops a modal dlg where the user is given a chance to review the tests and then execute them
        /// over and over if desired. This doesn't return until the user closes the dlg
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (TestManager.TestList.Count == 0)
                return;

            DlgTestRun dlg = new DlgTestRun(@"d:\trash\testruns");

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                
            }
            else
            {

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
                treeView1.Nodes.Remove(treeView1.SelectedNode);

            SetTreeviewControls();
        }
    }
}
