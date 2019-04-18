using Com.QuantAsylum.Tractor.Database;
using Com.QuantAsylum.Tractor.Settings;
using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Database;

namespace Com.QuantAsylum.Tractor.Dialogs
{
    partial class DlgSettings : Form
    {
        AppSettings Settings;

        bool CanClose = true;

        public DlgSettings(AppSettings settings)
        {
            InitializeComponent();
            Settings = settings;
            label5.Text = "";
            label7.Text = "";
        }

        private void DlgSettings_Load(object sender, EventArgs e)
        {
            foreach (Type testType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                            .Where(mytype => /*(mytype.GetInterfaces().Contains(typeof(IComposite))) ||*/
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
            textBox4.Text = Settings.ProductId.ToString();
            textBox5.Text = Settings.AuditDbSessionName;
            checkBox4.Checked = Settings.UseAuditDb;
            textBox6.Text = Settings.AuditDbEmail;
        }

        private void DlgSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CanClose == false)
                e.Cancel = true;
        }

        // OK Button clicked
        private void button1_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(textBox4.Text, out Guid guidResult))
            {
                Settings.TestClass = comboBox1.Text;
                Settings.AbortOnFailure = checkBox1.Checked;
                Settings.UseDb = checkBox3.Checked;
                Settings.DbConnectString = textBox2.Text;
                Settings.DbSessionName = textBox3.Text;
                Settings.ProductId = guidResult;
                Settings.AuditDbSessionName = textBox5.Text;
                Settings.UseAuditDb = checkBox4.Checked;
                Settings.AuditDbEmail = textBox6.Text;
                return;
            }

            CanClose = false;

        }

        // Cancel button clicked
        private void button2_Click(object sender, EventArgs e)
        {
            CanClose = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label5.Text = "Wait...";
            label5.Update();
            if (Db.OpenExisting(textBox2.Text))
                label5.Text = "OK";
            else
                label5.Text = "Error";
        }

        // Delete database
        private void button4_Click(object sender, EventArgs e)
        {
            label5.Text = "Wait...";
            label5.Update();
            if (Db.DeleteExisting(textBox2.Text))
                label5.Text = "OK";
            else
                label5.Text = "Error";
        }

        // Create database
        private void button5_Click(object sender, EventArgs e)
        {
            label5.Text = "Wait...";
            label5.Update();
            if (Db.CreateNew(textBox2.Text))
                label5.Text = "OK";
            else
                label5.Text = "Error";
        }

        // Test connection to audit database
        private void button6_Click(object sender, EventArgs e)
        {
            label7.Text = "Wait...";
            label7.Update();

            try
            {
                if (double.TryParse(AuditDb.CheckService(), out double result))
                {
                    if (result >= Constants.RequiredWebserviceVersion)
                    {
                        label7.Text = string.Format("Connection successful. Webservice version: {0:0.00}", result);
                    }
                    else
                    {
                        label7.Text = string.Format("Bad version. Needed {0:0.00} but found {1:0.00}", Constants.RequiredWebserviceVersion, result);
                    }
                }
                else
                {
                    label7.Text = "Connection failed. Service or internet connection may be down";
                }
            }
            catch (Exception ex)
            {
                label7.Text = ex.Message.Substring(0, Math.Min(100, ex.Message.Length-1));
            }
        }

        // Generate new Product ID
        private void button7_Click(object sender, EventArgs e)
        {
            textBox4.Text = Guid.NewGuid().ToString();
            label7.Text = "";
        }

        // Save Product ID to file
        private void button8_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(textBox4.Text, out Guid guidResult))
            {
                try
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.InitialDirectory = Constants.PidPath;
                    sfd.DefaultExt = "pid";
                    sfd.Filter = "pid files (*.pid) | *.pid";
                    sfd.CheckPathExists = true;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(sfd.FileName, guidResult.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogType.Error, "An exception occured saving a PID to a file: " + ex.Message);
                }
            }
            else
            {
                label7.Text = "Bad GUID. Please correct and try again";
            }

        }

        // Load Product Id from a file
        private void button9_Click(object sender, EventArgs e)
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
                        textBox4.Text = guidResult.ToString();
                        return;
                    }

                    MessageBox.Show("Failed to load file because the GUID couldn't be parsed");
                    
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, "An exception occured load a PID fron a file: " + ex.Message);
            }
        }

        
    }
}
