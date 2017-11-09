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

namespace Tractor
{
    public partial class DlgAddTest : Form
    {
        public DlgAddTest()
        {
            InitializeComponent();
        }

        private void DlgAddTest_Load(object sender, EventArgs e)
        {
            // Find all the classes in this assembly that implement ITest
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(ITest))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as ITest;

            // Add them to the combobox. This is our list of options
            foreach (var instance in instances)
            {
                comboBox1.Items.Add(instance.TestName()); 
            }

            comboBox1.SelectedIndex = 0;
        }

        private void DlgAddTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                e.Cancel = true;
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            string s = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            textBox1.Text = TestManager.FindUniqueName(s);
        }
    }
}
