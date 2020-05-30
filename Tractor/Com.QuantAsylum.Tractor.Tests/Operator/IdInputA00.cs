using Com.QuantAsylum.Tractor.Dialogs;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will prompt the user to enter a serial number or other identifier
    /// </summary>
    [Serializable]
    public class IdInputA00 : UiTestBase
    {

        [ObjectEditorAttribute(Index = 200, DisplayText = "Prompt Message", MaxLength = 32)]
        public string Msg = "";

        public IdInputA00() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Operator;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            DlgInput dlg = new DlgInput(Msg);

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string serialNumber = dlg.Result.Trim().ToUpper();
                if (serialNumber != "")
                {
                    Tm.LocalStash.Add("SerialNumber", serialNumber);
                    tr.Pass = true;
                    tr.StringValue[0] = serialNumber;
                }
            }
        }

        public override string GetTestDescription()
        {
            return "Prompts the user to enter some information as a string. This function returns 'pass = true' if the user " +
                   "entered something, it will be converted to upper case and leading and trailing spaces will be removed, " +
                   "but no further qualification is performed on the input string. " + 
                   "If the user hit cancel or if the string is empty, then 'pass = false' is returned";
        }
    }
}
