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
        }

        private void DlgSettings_Load(object sender, EventArgs e)
        {
            foreach (Type testType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                            .Where(mytype => (mytype.GetInterfaces().Contains(typeof(IInstrument)))
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
            textBox5.Text = Settings.SessionName;
            textBox9.Text = Settings.ProductId.ToString();

            checkBox5.Checked = Settings.UseCsvLog;
            textBox7.Text = Settings.CsvFileName;

            checkBox4.CheckedChanged -= checkBox4_CheckedChanged;
            checkBox4.Checked = Settings.UseAuditDb;
            checkBox4.CheckedChanged += checkBox4_CheckedChanged;

            textBox6.Text = Settings.AuditDbEmail;
            checkBox2.Checked = Settings.LockTestScreen;
            textBox1.Text = Settings.Password;
        }

        private void DlgSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CanClose == false)
                e.Cancel = true;
        }

        // OK Button clicked
        private void button1_Click(object sender, EventArgs e)
        {
            CanClose = true;

            if (Guid.TryParse(textBox9.Text, out Guid guidResult))
            {
                Settings.TestClass = comboBox1.Text;
                Settings.SessionName = textBox5.Text;
                Settings.ProductId = guidResult;

                Settings.AbortOnFailure = checkBox1.Checked;

                Settings.UseDb = checkBox3.Checked;
                Settings.DbConnectString = textBox2.Text;

                Settings.UseCsvLog = checkBox5.Checked;
                Settings.CsvFileName = textBox7.Text.Trim();
               
                Settings.UseAuditDb = checkBox4.Checked;
                Settings.AuditDbEmail = textBox6.Text;
                Settings.Password = textBox1.Text.Trim();

                if (Settings.Password == "")
                {
                    Settings.LockTestScreen = false;
                }
                else
                {
                    Settings.LockTestScreen = checkBox2.Checked;
                }

                if (Settings.CsvFileName != "")
                {
                    string f = Path.Combine(Constants.CsvLogsPath, Settings.CsvFileName);

                    if (Path.HasExtension(f) == false)
                    {
                        f = f + ".csv";
                        Settings.CsvFileName = Path.GetFileName(f);
                    }

                    if (File.Exists(f) == false)
                    {
                        try
                        {
                            File.CreateText(f).Close();
                            File.Delete(f);
                        }
                        catch (Exception ex)
                        {
                            label5.Text = "CSV File Name isn't valid: " + ex.Message;
                            CanClose = false;
                        }
                    }
                }

                if (Settings.CsvFileName == "" && Settings.UseCsvLog)
                {
                    label5.Text = "CSV file name not specified";
                    CanClose = false;
                }
                
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
            label5.Text = "Wait...";
            label5.Update();

            try
            {
                if (double.TryParse(AuditDb.CheckService(), out double result))
                {
                    if (result >= Constants.RequiredWebserviceVersion)
                    {
                        label5.Text = string.Format("Connection successful. Webservice version: {0:0.00}", result);
                    }
                    else
                    {
                        label5.Text = string.Format("Bad version. Needed {0:0.00} but found {1:0.00}", Constants.RequiredWebserviceVersion, result);
                    }
                }
                else
                {
                    label5.Text = "Connection failed. Service or internet connection may be down";
                }
            }
            catch (Exception ex)
            {
                label5.Text = ex.Message.Substring(0, Math.Min(100, ex.Message.Length-1));
            }
        }

        // Generate new Product ID
        private void button7_Click(object sender, EventArgs e)
        {
            textBox9.Text = Guid.NewGuid().ToString();
            label5.Text = "";
        }

        // Save Product ID to file
        private void button8_Click(object sender, EventArgs e)
        {
            if (Guid.TryParse(textBox9.Text, out Guid guidResult))
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
                label5.Text = "Bad GUID. Please correct and try again";
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
                        textBox9.Text = guidResult.ToString();
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

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                MessageBox.Show("By checking the Cloud Database option, Tractor will save raw test data (no bitmap graphics) " +
                    "to a Microsoft Azure website run by QuantAsylum USA LLC. The product ID is your secret key to this " +
                    "data. If you lose this key, it will be very difficult to recover your data. Anyone with this key " +
                    "would theoretically be able to submit tests and view tests on your behalf. Retrieving data from " +
                    "the Cloud Database will be supported in this application only--there is no way currently for you to " +
                    "programmatically access the data. Data maybe deleted after a period of time (6 months) as " +
                    "part of housecleaning operations. You may contact us to request an extension to that period. Under " +
                    "no circumstances can we be responsible for costs or damages due to lost data. Finally, make sure you " +
                    "fill in the email address field below so that we know who to contact before data might be deleted. If " +
                    "there is no valid email address, we will assume it was a test account and can be safely deleted at any " +
                    "time. ", "Important!");
            }
        }
    }
}
