using Com.QuantAsylum.Tractor.Tests.GainTests;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tractor;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;
using Tractor.Com.QuantAsylum.Tractor.Tests.IMDTests;
using Tractor.Com.QuantAsylum.Tractor.Tests.NoiseFloors;
using Tractor.Com.QuantAsylum.Tractor.Tests.THDs;
using Tractor.Com.QuantAsylum.Tractor.Tests.Other;

namespace Com.QuantAsylum.Tractor.Tests
{
    public class TestResult
    {
       
        /// <summary>
        /// An array of doubles that represent the test results. Index 0 = Left, Index 1 = Right
        /// </summary>
        public double[] Value;

        /// <summary>
        /// Results formatted for display. This allows each test to show whatever number of 
        /// significant digits makes sense OR to display an error code
        /// </summary>
        public string[] StringValue;

        /// <summary>
        /// Indicates if the test overall passed
        /// </summary>
        public bool Pass;

        public TestResult(int count)
        {
            Pass = false;
            Value = new double[count];
            StringValue = new string[count];

            for (int i=0; i<count; i++)
            {
                Value[i] = double.NaN;
                StringValue[i] = "---";
            }
        }
    }

    /// <summary>
    /// Base class for handling the display and update of data used by each test. 
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Gain01))]
    [System.Xml.Serialization.XmlInclude(typeof(Gain02))]
    [System.Xml.Serialization.XmlInclude(typeof(Imd01))]
    [System.Xml.Serialization.XmlInclude(typeof(Imd02))]
    [System.Xml.Serialization.XmlInclude(typeof(NoiseFloor01))]
    [System.Xml.Serialization.XmlInclude(typeof(NoiseFloor02))]
    [System.Xml.Serialization.XmlInclude(typeof(IdInput01))]
    [System.Xml.Serialization.XmlInclude(typeof(Thd01))]
    [System.Xml.Serialization.XmlInclude(typeof(Thd02))]
    [System.Xml.Serialization.XmlInclude(typeof(Prompt01))]
    [System.Xml.Serialization.XmlInclude(typeof(Impedance01))]
    public class TestBase
    {

        internal bool RunTest { get; set; } = true;

        /// <summary>
        /// Returns the user-assigned name for the test. This name must be unique among
        /// all the tests
        /// </summary>
        public string Name = "Placeholder";

        /// <summary>
        /// This is to permit grouping of tests by type. The type should be 
        /// one of the following:
        /// User Input
        /// Level
        /// Frequency Response
        /// Phase
        /// Crosstalk 
        /// SNR
        /// Distortion
        /// 
        /// Level measures the absolute level of a signal
        /// Frequency Response measures amplitude relative to a reference amplitude
        /// Phase measures the phase relationship between two signals
        /// Crosstalk meausures the leakage between channels
        /// SNR measures the ratio between the signal and the noise
        /// Distortion measures THD or THD + N
        /// </summary>
        internal enum TestTypeEnum { Unspecified, User, LevelGain, FrequencyResponse, Phase, CrossTalk, SNR, Distortion, Other };
        internal TestTypeEnum TestType = TestTypeEnum.Unspecified;

        public int RetryCount = 2;
        public bool LeftChannel = true;
        public bool RightChannel = true;

        internal Bitmap TestResultBitmap { get; set; }

        internal TestManager Tm;

        TableLayoutPanel TLPanel;
        
        internal bool IsDirty = false;
        internal Button OkButton;

        public void SetTestManager(TestManager tm)
        {
            Tm = tm;
        }

        internal void PopulateUI(TableLayoutPanel p)
        {
            // Right now, this is using reflection to find the publics. These names are unsorted and 
            // a single word (no space). It would be better in the future to use display name
            // See https://stackoverflow.com/questions/5499459/how-to-get-displayattribute-of-a-property-by-reflection/5499578#5499578
            TLPanel = p;

            Type t = GetType();

            FieldInfo[] f = t.GetFields();

            TLPanel.RowCount = f.Length;
            int row = 0;
            foreach (FieldInfo fi in f)
            {
                object obj = fi.GetValue(this);

                TLPanel.Controls.Add(new Label() { Text = UnCamelCase(fi.Name), Anchor = AnchorStyles.Right, AutoSize = true }, 0, row);

                if (obj is float)
                {
                    float value = (float)fi.GetValue(this);
                    TextBox tb = new TextBox() { Text = value.ToString("0.0"), Anchor = AnchorStyles.Left, AutoSize = true };
                    tb.TextChanged += Tb_TextChanged;
                    TLPanel.Controls.Add(tb, 1, row);
                }
                else if (obj is string)
                {
                    string value = (string)fi.GetValue(this);
                    TextBox tb = new TextBox() { Text = value, Anchor = AnchorStyles.Left, AutoSize = true };
                    tb.TextChanged += Tb_TextChanged;
                    TLPanel.Controls.Add(tb, 1, row);
                }
                else if (obj is bool)
                {
                    bool value = (bool)fi.GetValue(this);
                    CheckBox cb = new CheckBox() { Checked = value, Anchor = AnchorStyles.Left, AutoSize = true };
                    cb.CheckedChanged += Cb_CheckedChanged;
                    TLPanel.Controls.Add(cb, 1, row);
                }
                else if (obj is int)
                {
                    int value = (int)fi.GetValue(this);
                    TextBox tb = new TextBox() { Text = value.ToString(), Anchor = AnchorStyles.Left, AutoSize = true };
                    tb.TextChanged += Tb_TextChanged;
                    TLPanel.Controls.Add(tb, 1, row);
                }

                ++row;
            }
            ++row;

            OkButton = new Button() { Text = "Update", Anchor = (AnchorStyles.Top | AnchorStyles.Right), AutoSize = true, Enabled = false};
            OkButton.Click += UpdateBtn_Click;
            p.Controls.Add(OkButton, 0, row++);

            //Label label = new Label() { Text = this.GetTestDescription(), Size = new Size(TLPanel.Width / 2, 200) };
            //p.Controls.Add(label, 0, row++);

        }

        private void Cb_CheckedChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            OkButton.Enabled = true;
        }

        private void Tb_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            OkButton.Enabled = true;
        }

        public bool SaveChanges(out string errorCode)
        {
            Type t = GetType();
            errorCode = "";

            try
            {

                for (int i = 0; i < TLPanel.RowCount; i++)
                {
                    string fieldName = TLPanel.GetControlFromPosition(0, i).Text;
                    fieldName = ReCamelCase(fieldName);
                    FieldInfo fi = t.GetField(fieldName);

                    if (fi.GetValue(this) is float)
                    {
                        fi.SetValue(this, Convert.ToSingle(TLPanel.GetControlFromPosition(1, i).Text));
                    }
                    else if (fi.GetValue(this) is string)
                    {
                        fi.SetValue(this, TLPanel.GetControlFromPosition(1, i).Text);
                    }
                    else if (fi.GetValue(this) is bool)
                    {
                        fi.SetValue(this, ((CheckBox)(TLPanel.GetControlFromPosition(1, i))).Checked);
                    }
                    if (fi.GetValue(this) is int)
                    {
                        fi.SetValue(this, Convert.ToInt32(TLPanel.GetControlFromPosition(1, i).Text));
                    }
                }
            }
            catch(Exception ex)
            {
                // Failed to parse and thus failed to save
                errorCode = ex.Message;
                return false;
            }

            //string s;

            //if (CheckValues(out s) == false)
            //{
            //    MessageBox.Show(s);
            //}

            IsDirty = false;
            return true;
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            string error;
            if (SaveChanges(out error) == false)
            {
                if (MessageBox.Show("Incorrect data has been entered: " + error + Environment.NewLine + Environment.NewLine + "Press Retry to continue editing. Press Cancel to revert to previous values", "Invalid Data", MessageBoxButtons.RetryCancel) == DialogResult.Retry)
                {

                }
                else
                {
                    Form1.This.RePopulateTreeView(this.Name, true);
                }
            }            
        }

        public virtual bool CheckValues(out string s)
        {
            s = "";
            return true;
        }

        public virtual string GetTestDescription()
        {
            throw new NotImplementedException();
        }

        public virtual string GetTestName()
        {
            return this.GetType().Name;
        }

        internal virtual TestTypeEnum GetTestType()
        {
            return TestType;
        }

        public virtual void DoTest(string title, out TestResult testResult)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Because the remoting interface can only pass common data types that know how to 
        /// serialize themselves, images are passed as byte arrays. This helper will take
        /// a byte array and save it to the property
        /// </summary>
        /// <param name="imgArray"></param>
        public Bitmap CaptureBitmap(byte[] imgArray)
        {
            MemoryStream ms = new MemoryStream(imgArray);
            return (Bitmap)Image.FromStream(ms);
        }

        /// <summary>
        /// Converts CamelStringData into Camel String Data
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string UnCamelCase(string s)
        {
            return Regex.Replace(
                    Regex.Replace(
                        s,
                        @"(\P{Ll})(\P{Ll}\p{Ll})",
                        "$1 $2"
                    ),
                    @"(\p{Ll})(\P{Ll})",
                    "$1 $2"
                );
        }

        public string ReCamelCase(string s)
        {
            return s.Replace(" ", string.Empty);
        }
    }
}
