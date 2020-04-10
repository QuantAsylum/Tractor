using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        public bool IsFileName { get; set; } = false;

        public bool FileNameCanBeEmpty = false;

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

    class FileLoadButton : Button
    {
        public TextBox FileNameTextBox;
    }

    public class ObjectEditor
    {
        bool _IsDirty = false;

        Button OkButton;
        Button CancelButton;

        TestBase ObjectToEdit;
        TestBase BackupObjectToEdit;
        TableLayoutPanel Tlp;
        Form1 ParentForm;
        Action NowEditingCallback;

        public ObjectEditor(Form1 parentForm, TestBase obj, TableLayoutPanel tlp, Action nowEditingCallback)
        {
            NowEditingCallback = nowEditingCallback;
            ObjectToEdit = obj;
            BackupObjectToEdit = (TestBase)obj.ShallowCopy();
            Tlp = tlp;
            ParentForm = parentForm;
            PopulateUi();
        }

        private void PopulateUi()
        {
            Tlp.SuspendLayout();

            Tlp.AutoScroll = true;

            Type t = ObjectToEdit.GetType();
            FieldInfo[] f = t.GetFields();

            f = f.OrderBy(m => m.GetCustomAttribute<ObjectEditorAttribute>() == null ? -1 : m.GetCustomAttribute<ObjectEditorAttribute>().Index).ToArray();

            Tlp.Controls.Clear();
            Tlp.ColumnStyles.Clear();
            Tlp.RowStyles.Clear();
            Tlp.ColumnCount = 4;  // COL0 = label, COL1 = data field, COL3 = button if needed, COL4 = error
            Tlp.RowCount = f.Length;

            int row = -1;

            _IsDirty = false;

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
                    //var attr = fi.GetCustomAttribute<ObjectEditorAttribute>();
                    Tlp.Controls.Add(new Label() { Text = fi.GetCustomAttribute<ObjectEditorAttribute>().DisplayText, Anchor = AnchorStyles.Right, AutoSize = true }, 0, row);
                    Tlp.Controls.Add(new Label() { Text = "", Anchor = AnchorStyles.Left, AutoSize = true }, 3, row);
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
                    bool isFileName = (bool)fi.GetCustomAttribute<ObjectEditorAttribute>().IsFileName;
                    bool canBeEmpty = (bool)fi.GetCustomAttribute<ObjectEditorAttribute>().FileNameCanBeEmpty;
                    TextBox tb = new TextBox() { Text = value, Anchor = AnchorStyles.Left };
                    tb.TextChanged += ValueChanged;
                    Tlp.Controls.Add(tb, 1, row);

                    if (isFileName)
                    {
                        FileLoadButton b = new FileLoadButton() { Text = "Browse" };
                        b.Click += BrowseFile;
                        b.FileNameTextBox = tb;
                        Tlp.Controls.Add(b, 2, row);

                        if (canBeEmpty == false &&  File.Exists(tb.Text) == false)
                        {
                            _IsDirty = true;
                        }
                    }
                }
                else if (o is bool)
                {
                    bool value = (bool)fi.GetValue(ObjectToEdit);
                    CheckBox tb = new CheckBox() { Checked = value, Anchor = AnchorStyles.Left };
                    tb.CheckedChanged += ValueChanged;
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


            Tlp.ResumeLayout();

            // Some fields (such as file names) need to be selected before closing
            if (_IsDirty)
            {
                ValueChanged(null, null);
            }
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            NowEditingCallback?.Invoke();
            OkButton.Enabled = true;
            CancelButton.Enabled = true;
            _IsDirty = true;
        }

        private void BrowseFile(object sender, EventArgs e)
        {
            NowEditingCallback?.Invoke();
            OkButton.Enabled = true;
            CancelButton.Enabled = true;
            _IsDirty = true;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Constants.MaskFiles;
            ofd.CheckFileExists = true;
            ofd.Filter = "All Files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileLoadButton b = sender as FileLoadButton;
                b.FileNameTextBox.Text = ofd.FileName;
            }
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

                    if ((f[i].GetValue(ObjectToEdit) is double) || (f[i].GetValue(ObjectToEdit) is float))
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

                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Black;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Red;
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
                            int min = (int)(f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue);
                            int max = (int)(f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue);

                            StringBuilder sb = new StringBuilder();
                            for (int pwr = (int)Math.Log(min, 2); pwr <= (int)Math.Log(max, 2); pwr++)
                            {
                                sb.Append($"{(int)Math.Pow(2, pwr)} ");
                            }
                            errMsg = $"Value is not a power of 2. Value must be one of the following [{sb.ToString()}]";
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

                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Black;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Red;
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
                            int min = (int)(f[i].GetCustomAttribute<ObjectEditorAttribute>().MinValue);
                            int max = (int)(f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxValue);
 
                            StringBuilder sb = new StringBuilder();
                            for (int pwr = (int)Math.Log(min, 2); pwr <= (int)Math.Log(max, 2); pwr++)
                            {
                                sb.Append($"{(int)Math.Pow(2, pwr)} ");
                            }
                            errMsg = $"Value is not a power of 2. Value must be one of the following [{sb.ToString()}]";
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

                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Black;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Red;
                            retVal = false;
                        }
                    }
                    else if (f[i].GetValue(ObjectToEdit) is string)
                    {
                        bool valueOk = true;
                        string errMsg = "";

                        if (f[i].GetCustomAttribute<ObjectEditorAttribute>().IsFileName)
                        {
                            if (f[i].GetCustomAttribute<ObjectEditorAttribute>().FileNameCanBeEmpty == false)
                            {
                                if (File.Exists(Tlp.GetControlFromPosition(1, i).Text) == false)
                                {
                                    valueOk = false;
                                    errMsg = "File does not exist";
                                }
                            }
                        }

                        if (valueOk)
                        {
                            if (commit)
                            {
                                string s = Tlp.GetControlFromPosition(1, i).Text.Trim();
                                f[i].SetValue(ObjectToEdit, s.Substring(0, Math.Min(s.Length, f[i].GetCustomAttribute<ObjectEditorAttribute>().MaxLength)));
                            }

                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Black;
                        }
                        else
                        {
                            Tlp.GetControlFromPosition(3, i).Text = errMsg;
                            Tlp.GetControlFromPosition(0, i).ForeColor = Color.Red;
                            retVal = false;
                        }
                    }
                    else if (f[i].GetValue(ObjectToEdit) is bool)
                    {
                        if (commit)
                            f[i].SetValue(ObjectToEdit, ((CheckBox)Tlp.GetControlFromPosition(1, i)).Checked);

                        Tlp.GetControlFromPosition(3, i).Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                string s = "Failed to parse attribute in AutobuilUi.cs: " + ex.Message;
                Log.WriteLine(LogType.Error, s);
                MessageBox.Show(s);
                return false;
            }

            return retVal;
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (VerifyChanges(true))
            {
                _IsDirty = false;
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
            BackupObjectToEdit = (TestBase)ObjectToEdit.ShallowCopy();
            PopulateUi();
            ParentForm.AbandonChanges();
        }
    }
}
