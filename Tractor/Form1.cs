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
            this.treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.treeView1.DrawNode += new DrawTreeNodeEventHandler(treeView1_DrawNode);

            DefaultTreeview();
        }

        // This method is needed by treeview extensions
        void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Level == 1) e.Node.HideCheckBox();
            e.DrawDefault = true;
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
            root.Text = testName;
            TreeNode sub = new TreeNode();
            sub.Text = "Test: " + (test as ITest).TestName();
            root.Nodes.Add(sub);

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

            string id = e.Node.Text;

            ClearEditFields();
            TestBase tb = TestManager.TestList.Find(o => o.Name == id);
            tb.PopulateUI(tableLayoutPanel1);
        }

        /// <summary>
        /// Called after a checkbox has been checked (or unchecked)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            string id = e.Node.Text;

            //List<TestBase> list = (List<TestBase>)(TestManager.TestList);
            TestBase tb = TestManager.TestList.Find(o => o.Name == id);
            tb.RunTest = e.Node.Checked;
        }

            

        private void button1_Click(object sender, EventArgs e)
        {
            DlgAddTest dlg = new DlgAddTest();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string className = (string)(dlg.comboBox1.Items[dlg.comboBox1.SelectedIndex]);
                TestBase testInst = CreateTestInstance(className);
                (testInst as TestBase).Name = dlg.textBox1.Text;
                TestManager.TestList.Add(testInst as TestBase);

                TreeViewAdd(dlg.textBox1.Text, CreateTestInstance(className));
            }
        }

        /// <summary>
        /// Creates an instance based on the classname
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (TestManager.TestList.Count == 0)
                return;

            DlgTestRun dlg = new DlgTestRun();

            if (dlg.ShowDialog() == DialogResult.OK)
            {

            }
        }
    }
}
