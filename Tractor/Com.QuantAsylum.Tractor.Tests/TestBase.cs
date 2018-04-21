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
using Tractor.Com.QuantAsylum.Tractor.Tests.IMDTests;
using Tractor.Com.QuantAsylum.Tractor.Tests.NoiseFloors;
using Tractor.Com.QuantAsylum.Tractor.Tests.THDs;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// Base class for handling the display and update of data used by each test. 
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Gain01))]
    [System.Xml.Serialization.XmlInclude(typeof(Gain03))]
    [System.Xml.Serialization.XmlInclude(typeof(Imd01))]
    [System.Xml.Serialization.XmlInclude(typeof(NoiseFloor01))]
    [System.Xml.Serialization.XmlInclude(typeof(IdInput01))]
    [System.Xml.Serialization.XmlInclude(typeof(Thd01))]
    [System.Xml.Serialization.XmlInclude(typeof(Prompt01))]
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
        internal enum TestTypeEnum { Unspecified, User, LevelGain, FrequencyResponse, Phase, CrossTalk, SNR, Distortion };
        internal TestTypeEnum TestType = TestTypeEnum.Unspecified;

        public bool LeftChannel = true;
        public bool RightChannel = true;

        internal Bitmap TestResultBitmap { get; set; }

        TableLayoutPanel TLPanel;

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

                //if (obj is TestTypeEnum)
                //    continue;

                TLPanel.Controls.Add(new Label() { Text = UnCamelCase(fi.Name), Anchor = AnchorStyles.Right, AutoSize = true }, 0, row);

                if (obj is float)
                {
                    float value = (float)fi.GetValue(this);
                    TLPanel.Controls.Add(new TextBox() { Text = value.ToString("0.0"), Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }
                else if (obj is string)
                {
                    string value = (string)fi.GetValue(this);
                    TLPanel.Controls.Add(new TextBox() { Text = value, Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }
                else if (obj is bool)
                {
                    bool value = (bool)fi.GetValue(this);
                    TLPanel.Controls.Add(new CheckBox() { Checked = value, Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }
                else if (obj is int)
                {
                    int value = (int)fi.GetValue(this);
                    TLPanel.Controls.Add(new TextBox() { Text = value.ToString(), Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }


                ++row;
            }
            ++row;

            Button okBtn = new Button() { Text = "Update", Anchor = AnchorStyles.None, AutoSize = false, };
            okBtn.Click += UpdateBtn_Click;
            //Button cancelBtn = new Button() { Text = "Cancel", Anchor = AnchorStyles.None, AutoSize = false };
            //cancelBtn.Click += CancelBtn_Click;

            p.Controls.Add(okBtn, 0, row);
            //p.Controls.Add(cancelBtn, 1, row);

        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            Type t = GetType();

            for (int i=0; i<TLPanel.RowCount; i++)
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

            string s;

            if (CheckValues(out s) == false)
            {
                MessageBox.Show(s);
            }

            Form1.This.RePopulateTreeView(this.Name);
        }

        //private void CancelBtn_Click(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

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

        public virtual void DoTest(out float[] value, out bool pass)
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
