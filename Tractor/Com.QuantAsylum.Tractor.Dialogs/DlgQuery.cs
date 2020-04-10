using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tractor.Com.QuantAsylum.Tractor.Database;

namespace Tractor.Com.QuantAsylum.Tractor.Dialogs
{
    public partial class DlgQuery : Form
    {
        List<string> TestGroups = new List<string>();
        int TestGroupIndex = 0;

        public DlgQuery()
        {
            InitializeComponent();
        }


        private void DlgQuery_Load(object sender, EventArgs e)
        {
            textBox2.Text = Form1.AppSettings.ProductId.ToString();
            textBox4.Text = Form1.AppSettings.ProductId.ToString();
            UpdateButtonState();
        }

        private void DlgQuery_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void UpdateButtonState()
        {
            if (TestGroups.Count == 0 || TestGroups.Count == 1)
            {
                button10.Enabled = false;
                button13.Enabled = false;
                button11.Enabled = false;
                button12.Enabled = false;
                return;
            }

            button10.Enabled = true;
            button13.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;


            if (TestGroupIndex == 0)
            {
                button10.Enabled = false;
                button11.Enabled = false;
            }

            if (TestGroupIndex == TestGroups.Count - 1)
            {
                button12.Enabled = false;
                button13.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadGuid(textBox2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                TestGroups = AuditDb.QueryGroupsBySerialNumber(textBox2.Text, textBox1.Text);

                UpdateButtonState();
                if (TestGroups.Count > 0)
                {
                    var s = AuditDb.QueryTestsByGroup(textBox2.Text, TestGroups[0]);
                    textBox3.Text = s;
                    return;
                }

                textBox3.Text = "No results found.";
            }
            catch (Exception ex)
            {
                string s = "Web service call failed exception: " + ex.Message;
                Log.WriteLine(LogType.Database, s);
                MessageBox.Show(s);
            }
        }

        // Newest button
        private void button10_Click(object sender, EventArgs e)
        {
            TestGroupIndex = 0;
            var s = AuditDb.QueryTestsByGroup(textBox2.Text, TestGroups[TestGroupIndex]);
            textBox3.Text = s;
            UpdateButtonState();
        }

        // Newer
        private void button11_Click(object sender, EventArgs e)
        {
            if (--TestGroupIndex < 0)
                TestGroupIndex = 0;

            var s = AuditDb.QueryTestsByGroup(textBox2.Text, TestGroups[TestGroupIndex]);
            textBox3.Text = s;
            UpdateButtonState(); ;

        }

        // Older
        private void button12_Click(object sender, EventArgs e)
        {
            if (++TestGroupIndex == TestGroups.Count)
                TestGroupIndex = TestGroups.Count- 1;

            var s = AuditDb.QueryTestsByGroup(textBox2.Text, TestGroups[TestGroupIndex]);
            textBox3.Text = s;
            UpdateButtonState(); ;
        }

        // Oldest
        private void button13_Click(object sender, EventArgs e)
        {
            TestGroupIndex = TestGroups.Count - 1;
            var s = AuditDb.QueryTestsByGroup(textBox2.Text, TestGroups[TestGroupIndex]);
            textBox3.Text = s;
            UpdateButtonState(); ;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2.Focus();
                button2.PerformClick();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            textBox6.Clear();
            List<string> names = AuditDb.QueryTestNames(textBox4.Text);
            names.Sort();
            comboBox1.DataSource = names;
        }

        // Query button
        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;

            string testName = comboBox1.GetItemText(comboBox1.SelectedItem);

            if (testName == null || testName == "")
                return;

            textBox6.Text = AuditDb.QueryStatsByTest(textBox4.Text, comboBox1.GetItemText(comboBox1.SelectedItem), textBox5.Text, checkBox1.Checked, checkBox2.Checked);

        }

        //private void comboBox1_TextUpdate(object sender, EventArgs e)
        //{

        //}

        //private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        //{

        //}

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button4.Focus();
                button4.PerformClick();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadGuid(textBox4);
        }

        private void LoadGuid(TextBox tb)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Constants.PidPath;
                ofd.DefaultExt = "pid";
                ofd.Filter = "pid files (*.pid) | *.pid";
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(ofd.FileName);

                    if (Guid.TryParse(lines[0], out Guid guidResult))
                    {
                        tb.Text = guidResult.ToString();
                        return;
                    }

                    MessageBox.Show("Failed to load file because the GUID couldn't be parsed");

                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, "An exception occured loading a PID fron a file: " + ex.Message);
            }
        }

      
    }
}
