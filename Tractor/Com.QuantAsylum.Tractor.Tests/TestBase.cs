using Com.QuantAsylum.Tractor.Tests.GainTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests.IMDTests;
using Tractor.Com.QuantAsylum.Tractor.Tests.NoiseFloors;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// Base class for handling the display and update of data used by each test. 
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Gain))]
    [System.Xml.Serialization.XmlInclude(typeof(IMD_ITU))]
    [System.Xml.Serialization.XmlInclude(typeof(NoiseFloor))]
    public class TestBase 
    {
        /// <summary>
        /// Determines if the test will be run or not
        /// </summary>
        public bool RunTest { get; set; } = true;

        /// <summary>
        /// Returns the user-assigned name for the test. This name must be unique among
        /// all the tests
        /// </summary>
        public string Name = "Placeholder";

        public bool LeftChannel = true;
        public bool RightChannel = true;

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
                TLPanel.Controls.Add(new Label() { Text = fi.Name, Anchor = AnchorStyles.Right, AutoSize = true }, 0, row);
                object obj = fi.GetValue(this);

                if (fi.GetValue(this) is float)
                {
                    float value = (float)fi.GetValue(this);
                    TLPanel.Controls.Add(new TextBox() { Text = value.ToString("0.0"), Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }
                else if (fi.GetValue(this) is string)
                {
                    string value = (string)fi.GetValue(this);
                    TLPanel.Controls.Add(new TextBox() { Text = value, Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }
                else if (fi.GetValue(this) is bool)
                {
                    bool value = (bool)fi.GetValue(this);
                    TLPanel.Controls.Add(new CheckBox() { Checked = value, Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }

                ++row;
            }
            ++row;

            Button okBtn = new Button() { Text = "Update", Anchor = AnchorStyles.None, AutoSize = false, };
            okBtn.Click += UpdateBtn_Click;
            Button cancelBtn = new Button() { Text = "Cancel", Anchor = AnchorStyles.None, AutoSize = false };
            cancelBtn.Click += CancelBtn_Click;

            p.Controls.Add(okBtn, 0, row);
            p.Controls.Add(cancelBtn, 1, row);

        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            Type t = GetType();

            for (int i=0; i<TLPanel.RowCount; i++)
            {
                string FieldName = TLPanel.GetControlFromPosition(0, i).Text;
                FieldInfo fi = t.GetField(FieldName);

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
            }

            Form1.This.RePopulateTreeView(this.Name);
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public virtual string TestDescription()
        {
            throw new NotImplementedException();
        }

        public virtual string TestName()
        {
            throw new NotImplementedException();
        }

        public virtual void DoTest(out float[] value, out bool pass)
        {
            throw new NotImplementedException();
        }
    }
}
