using Com.QuantAsylum.Tractor.Database;
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
            label5.Text = "";
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

            checkBox1.Checked = Settings.AbortOnFailure;
            checkBox3.Checked = Settings.UseDb;
            textBox2.Text = Settings.DbConnectString;
            textBox3.Text = Settings.DbSessionName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.TestClass = comboBox1.Text;
            Settings.AbortOnFailure = checkBox1.Checked;
            Settings.UseDb = checkBox3.Checked;
            Settings.DbConnectString = textBox2.Text;
            Settings.DbSessionName = textBox3.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label5.Text = "Wait...";
            label5.Update();

            try
            {
                if (Db.OpenExisting(textBox2.Text) == false)
                {
                    label5.Text = "Connection failed.";
                }
                else
                {
                    label5.Text = "OK.";
                }
            }
            catch
            {
                label5.Text = "Connection failed.";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label5.Text = "Wait...";
            label5.Update();

            try
            {
                Db.CreateNew(textBox2.Text, out string outMsg);
                label5.Text = outMsg;
            }
            catch
            {
                label5.Text = "Connection failed.";
            }
        }
    }
}
