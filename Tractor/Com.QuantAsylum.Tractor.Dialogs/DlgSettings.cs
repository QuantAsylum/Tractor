using Com.QuantAsylum.Tractor.Settings;
using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Com.QuantAsylum.Tractor.Dialogs
{
    partial class DlgSettings : Form
    {
        AppSettings Settings;

        public DlgSettings(AppSettings settings)
        {
            InitializeComponent();
            Settings = settings;
        }

        private void DlgSettings_Load(object sender, EventArgs e)
        {
            foreach (Type testType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                            .Where(  mytype => (mytype.GetInterfaces().Contains(typeof(IComposite))) ||                           
                                              (mytype.GetInterfaces().Contains(typeof(IInstrument)))   
                            ))
            {
                comboBox1.Items.Add(testType.FullName);
            }

            if (comboBox1.Items.Contains(Settings.TestClass))
            {
                comboBox1.SelectedText = Settings.TestClass;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.TestClass = comboBox1.Text;
        }
    }
}
