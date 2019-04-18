using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tractor.Com.QuantAsylum.Tractor.Tests
{
    public class ObjectEditorAttribute : Attribute
    {
        public int Index { get; set; } = int.MaxValue;
        public string DisplayText { get; set; } = "Default";
        public string FormatString { get; set; } = "0.0";
        public double MinValue { get; set; } = int.MinValue;
        public double MaxValue { get; set; } = int.MaxValue;
        public bool MustBePowerOfTwo { get; set; } = false;
        public int[] ValidInts { get; set; } = null;
        public int MaxLength { get; set; } = 64;
        public bool Hide { get; set; } = false;

        /// <summary>
        /// The indicated index must be LESS than the current index
        /// </summary>
        public int MustBeGreaterThanIndex { get; set; } = -1;

        /// <summary>
        /// The indicated index must be LESS than the current index
        /// </summary>
        public int MustBeGreaterThanOrEqualIndex { get; set; } = -1;
    }

    public class ObjectEditorSpacer
    {

    }

    public class ObjectEditor
    {
        bool _IsDirty = false;

        internal Button OkButton;
        internal Button CancelButton;

        TestBase ObjectToEdit;
        TestBase BackupObjectToEdit;
        TableLayoutPanel Tlp;
        Form1 ParentForm;
        Action NowEditingCallback;

        public ObjectEditor(Form1 parentForm, TestBase obj, TableLayoutPanel tlp, Action nowEditingCallback)
        {
            NowEditingCallback = nowEditingCallback;
            ObjectToEdit = obj;
            BackupObjectToEdit = (AudioTestBase)obj.ShallowCopy();
            Tlp = tlp;
            ParentForm = parentForm;
            PopulateUi();
        }

        private void PopulateUi()
        {
            Tlp.SuspendLayout();

            Type t = ObjectToEdit.GetType();
            FieldInfo[] f = t.GetFields();

            f = f.OrderBy(m => m.GetCustomAttribute<ObjectEditorAttribute>() == null ? -1 : m.GetCustomAttribute<ObjectEditorAttribute>().Index).ToArray();

            Tlp.Controls.Clear();
            Tlp.ColumnStyles.Clear();
            Tlp.RowStyles.Clear();
            Tlp.ColumnCount = 3;
            Tlp.RowCount = f.Length;

            Tlp.ColumnCount = 3;
            int row = -1;

            foreach (FieldInfo fi in f)
            {
                ++row;
                object o = fi.GetValue(ObjectToEdit);

                if (row == 0)
                    Tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                if (fi.GetCustomAttribute<ObjectEditorAttribute>().Hide)
                    continue;

                if ((o is ObjectEditorSpacer) == false)
                {
                    var attr = fi.GetCustomAttribute<ObjectEditorAttribute>();
                    Tlp.Controls.Add(new Label() { Text = fi.GetCustomAttribute<ObjectEditorAttribute>().DisplayText, Anchor = AnchorStyles.Right, AutoSize = true }, 0, row);
                    Tlp.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 2, row);
                }

                if (o is int)
                {
                    int value = (int)fi.GetValue(ObjectToEdit);
                    TextBox tb = new TextBox() { Text = value.ToString(), Anchor = AnchorStyles.Left, AutoSize = true };
                    tb.TextChanged += ValueChanged;
                    Tlp.Controls.Add(tb, 1, row);
                }
                if (o is uint)
                {
                    uint value = (uint)fi.GetValue(ObjectToEdit);
                    TextBox tb = new TextBox() { Text = value.ToString(), Anchor = AnchorStyles.Left, AutoSize = true };
                    tb.TextChanged += ValueChanged;
                    Tlp.Controls.Add(tb, 1, row);
                }
                else if (o is double || o is float)
                {
                    double value = (float)fi.GetValue(ObjectToEdit);
                    TextBox tb = new TextBox() { Text = value.ToString(fi.GetCustomAttribute<ObjectEditorAttribute>().FormatString), Anchor = AnchorStyles.Left };
                    tb.TextChanged += ValueChanged;
                    Tlp.Controls.Add(tb, 1, row);
                }
                else if (o is string)
                {
                    string value = (string)fi.GetValue(ObjectToEdit);
                    TextBox tb = new TextBox() { Text = value, Anchor = AnchorStyles.Left };
                    tb.TextChanged += ValueChanged;
                    Tlp.Controls.Add(tb, 1, row);
                }
                else if (o is bool)
                {
                    bool value = (bool)fi.GetValue(ObjectToEdit);
                    CheckBox tb = new CheckBox() { Checked = value, Anchor = AnchorStyles.Left };
                    tb.TextChanged += ValueChanged;
                    Tlp.Controls.Add(tb, 1, row);
                }
                else if (o is ObjectEditorSpacer)
                {
                    Tlp.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 1, row);
                }
            }

            ++row;

            OkButton = new Button() { Text = "OK", Anchor = AnchorStyles.Right, AutoSize = true, Enabled = true };
            OkButton.Click += UpdateBtn_Click;
            OkButton.Enabled = false;
            //OkButton.DialogResult = DialogResult.OK;
            //ParentForm.AcceptButton = OkButton;
            Tlp.Controls.Add(OkButton, 0, row);

            CancelButton = new Button() { Text = "Cancel", Anchor = AnchorStyles.Left, AutoSize = true, Enabled = true };
            CancelButton.Click += CancelBtn_Click;
            CancelButton.Enabled = false;
            //ParentForm.CancelButton = CancelButton;
            Tlp.Controls.Add(CancelButton, 1, row);

            // This just helps with Y spacing...
            Tlp.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Right }, 0, ++row);
            _IsDirty = false;

            Tlp.ResumeLayout();
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            NowEditingCallback?.Invoke();
            OkButton.Enabled = true;
            CancelButton.Enabled = true;
            _IsDirty = true;
        }

        public bool IsDirty
        {
            get { return _IsDirty; }
        }

        bool IsPowerOfTwo(uint x)
        {
            return (x & (x - 1)) == 0;
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public bool VerifyChanges(bool commit = false)
        {
            bool retVal = true;

            Type t = ObjectToEdit.GetType();
            FieldInfo[] f = t.GetFields();
            f = f.OrderBy(m => m.GetCustomAttribute<ObjectEditorAttribute>() == null ? -1 : m.GetCustomAttribute<ObjectEditorAttribute>().Index).ToArray();

            try
            {
                for (int i = 0; i < Tlp.RowCount; i++)
                {
                    Debug.WriteLine("Field {0}", f[i]);
                    int[] ValidInts = f[i].GetCustomAttribute<ObjectEditorAttribute>().ValidInts;

                    int MustBeGreaterThanIndex = f[i].GetCustomAttribute<ObjectEditorAttribute>().MustBeGreaterThanIndex;
                    double MustBeGreaterThanValue = 0;
                    if (MustBeGreaterThanIndex != -1)
                    {
                        FieldInfo fi = f.FirstOrDefault(o => o.GetCustomAttribute<ObjectEditorAttribute>().Index == MustBeGreaterThanIndex);
                        MustBeGreaterThanValue = Convert.ToDouble(Tlp.GetControlFromPosition(1, Array.IndexOf(f, fi)).Text);
                    }

                    int MustBeGreaterThanOrEqualIndex = f[i].GetCustomAttribute<ObjectEditorAttribute>().MustBeGreaterThanOrEqualIndex;
                    double MustBeGreaterThanOrEqualValue = 0;
                    if (MustBeGreaterThanOrEqualIndex != -1)
                    {
                        FieldInfo fi = f.FirstOrDefault(o => o.GetCustomAttribute<ObjectEditorAttribute>().Index == MustBeGreaterThanOrEqualIndex);
                        MustBeGreaterThanOrEqualValue = Convert.ToDouble(Tlp.GetControlFromPosition(1, Array.IndexOf(f, fi)).Text);
                    }

                    if (f[i].GetCustomAttribute<ObjectEditorAttribute>().Hide)
                        continue;

                    if ( (f[i].GetValue(ObjectToEdit) is double) || (f[i].GetValue(ObjectToEdit) is float) )
                    {
                        bool valueOk = true;
                        string errMsg = "";

                        if (float.TryParse(Tlp.GetControlFromPosition(1, i).Text, out float result) == false)
                        {
                            valueOk = false;
                            errMsg = "Value is not a double";

                        }
                        else if (result < f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue)
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue;
                        }
                        else if (result > f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue)
                        {
                            valueOk = false;
                            errMsg = "Value must be <=" + f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue;
                        }
                        else if (MustBeGreaterThanIndex != -1 && (result <= MustBeGreaterThanValue))
                        {
                            valueOk = false;
                            errMsg = "Value must be > " + MustBeGreaterThanValue;
                        }
                        else if (MustBeGreaterThanOrEqualIndex != -1 && (result < MustBeGreaterThanOrEqualValue))
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + MustBeGreaterThanOrEqualValue;
                        }

                        if (valueOk)
                        {
                            if (commit)
                                f[i].SetValue(ObjectToEdit, result);

                            Tlp.GetControlFromPosition(2, i).Text = errMsg;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(2, i).Text = errMsg;
                            retVal = false;
                        }
                    }
                    else if (f[i].GetValue(ObjectToEdit) is int)
                    {
                        bool valueOk = true;
                        string errMsg = "";

                        if (int.TryParse(Tlp.GetControlFromPosition(1, i).Text, out int result) == false)
                        {
                            valueOk = false;
                            errMsg = "Value is not an integer";

                        }
                        else if (f[i].GetCustomAttribute<ObjectEditorAttribute>().MustBePowerOfTwo ? !IsPowerOfTwo(result) : false)
                        {
                            valueOk = false;
                            errMsg = "Value is not a power of 2";
                        }
                        else if (ValidInts == null ? false : !ValidInts.Contains(result))
                        {
                            valueOk = false;
                            errMsg = "Value must be " + string.Join(" or ", ValidInts);
                        }
                        else if (result < f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue)
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue;
                        }
                        else if (result > f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue)
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue;
                        }
                        else if (MustBeGreaterThanIndex != -1 && (result <= MustBeGreaterThanValue))
                        {
                            valueOk = false;
                            errMsg = "Value must be > " + MustBeGreaterThanValue;
                        }
                        else if (MustBeGreaterThanOrEqualIndex != -1 && (result < MustBeGreaterThanOrEqualValue))
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + MustBeGreaterThanOrEqualValue;
                        }

                        if (valueOk)
                        {
                            if (commit)
                                f[i].SetValue(ObjectToEdit, result);

                            Tlp.GetControlFromPosition(2, i).Text = errMsg;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(2, i).Text = errMsg;
                            retVal = false;
                        }
                    }
                    else if (f[i].GetValue(ObjectToEdit) is uint)
                    {
                        bool valueOk = true;
                        string errMsg = "";

                        if (uint.TryParse(Tlp.GetControlFromPosition(1, i).Text, out uint result) == false)
                        {
                            valueOk = false;
                            errMsg = "Value is not a uint";

                        }
                        else if (f[i].GetCustomAttribute<ObjectEditorAttribute>().MustBePowerOfTwo ? !IsPowerOfTwo(result) : false)
                        {
                            valueOk = false;
                            errMsg = "Value is not a power of 2";
                        }
                        else if (result < f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue)
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue;
                        }
                        else if (result > f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue)
                        {
                            valueOk = false;
                            errMsg = "Value must be <= " + f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue;
                        }
                        else if (MustBeGreaterThanIndex != -1 && (result <= MustBeGreaterThanValue))
                        {
                            valueOk = false;
                            errMsg = "Value must be > " + MustBeGreaterThanValue;
                        }
                        else if (MustBeGreaterThanOrEqualIndex != -1 && (result < MustBeGreaterThanOrEqualValue))
                        {
                            valueOk = false;
                            errMsg = "Value must be >= " + MustBeGreaterThanOrEqualValue;
                        }

                        if (valueOk)
                        {
                            if (commit)
                                f[i].SetValue(ObjectToEdit, result);

                            Tlp.GetControlFromPosition(2, i).Text = errMsg;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(2, i).Text = errMsg;
                            retVal = false;
                        }
                    }
                    else if (f[i].GetValue(ObjectToEdit) is string)
                    {
                        if (commit)
                        {
                            string s = Tlp.GetControlFromPosition(1, i).Text;
                            f[i].SetValue(ObjectToEdit, s.Substring(0, Math.Min(s.Length, f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxLength)));
                        }

                        Tlp.GetControlFromPosition(2, i).Text = "";
                    }
                    else if (f[i].GetValue(ObjectToEdit) is bool)
                    {
                        if (commit)
                            f[i].SetValue(ObjectToEdit, ((CheckBox)Tlp.GetControlFromPosition(1, i)).Checked);

                        Tlp.GetControlFromPosition(2, i).Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to parse attribute in AutobuilUi.cs: " + ex.Message);
                return false;
            }

            return retVal;
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (VerifyChanges(true))
            {
                ParentForm.AcceptChanges();
                OkButton.Enabled = false;
                CancelButton.Enabled = false;
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            OkButton.Enabled = false;
            CancelButton.Enabled = false;
            ObjectToEdit = BackupObjectToEdit;
            BackupObjectToEdit = (AudioTestBase)ObjectToEdit.ShallowCopy();
            PopulateUi();
            ParentForm.AbandonChanges();
        }
    }
}
