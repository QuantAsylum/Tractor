using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using static Com.QuantAsylum.Tractor.Tests.AudioTestBase;
using Com.QuantAsylum.Tractor.Settings;
using static Com.QuantAsylum.Tractor.Tests.TestBase;

namespace Tractor
{
    partial class DlgAddTest : Form
    {
        class ComboItem
        {
            internal string DisplayName;
            internal string Description;

            public override string ToString()
            {
                return DisplayName;
            }
        }

        TestManager Tm;

        public DlgAddTest(TestManager tm)
        {
            InitializeComponent();
            Tm = tm;
        }

        private void DlgAddTest_Load(object sender, EventArgs e)
        {
            // Populate the filter listbox with filter options
            foreach (var name in Enum.GetNames(typeof(TestTypeEnum)))
            {
                if (name == TestTypeEnum.Unspecified.ToString())
                    continue;

                comboBox2.Items.Add(name);
            }
            comboBox2.SelectedIndex = 0;

            PopulateListBox();
        }

        public string GetSelectedTestName()
        {
            ComboItem ci = (ComboItem)comboBox1.Items[comboBox1.SelectedIndex];
            return ci.DisplayName;
        }

        private void DlgAddTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                return;

            if (comboBox1.SelectedIndex == -1)
            {
                e.Cancel = true;
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            string s = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            textBox1.Text = Form1.AppSettings.FindUniqueName(s);

            ComboItem ci = (ComboItem)comboBox1.Items[comboBox1.SelectedIndex];
            label4.Text = ci.Description;
        }

        /// <summary>
        /// Populates the test listbox based on the selected item in the filter list box
        /// </summary>
        private void PopulateListBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Text = "";

            // Find all the classes in this assembly that implement ITest

            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.IsSubclassOf(typeof(AudioTestBase))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as AudioTestBase;

            string filter = comboBox2.Text;

            // Add them to the combobox. This is our list of options
            foreach (var instance in instances)
            {
                if ((instance as AudioTestBase).TestType.ToString() == filter)
                {
                    comboBox1.Items.Add(new ComboItem() { DisplayName = instance.GetTestName(), Description = instance.GetTestDescription() });
                }


            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateListBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
