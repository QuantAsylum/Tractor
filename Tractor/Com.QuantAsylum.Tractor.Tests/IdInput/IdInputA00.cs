using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.Dialogs;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will prompt the user to enter a serial number or other identifier
    /// </summary>
    [Serializable]
    public class IdInputA00 : UiTest
    {

        [ObjectEditorAttribute(Index = 200, MaxLength = 32)]
        public string Id = "";

        public IdInputA00() : base()
        {
            Name = "IdInputA00";
            _TestType = TestTypeEnum.User;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            DlgInput dlg = new DlgInput("Enter serial number");

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Id = dlg.Result;
                if (Id != "")
                {
                    Tm.LocalStash.Add("SerialNumber", Id);
                    tr.Pass = true;
                    tr.StringValue[0] = Id;
                }
            }
        }

        public override string GetTestDescription()
        {
            return "Prompts the user to enter a serial number. This function returns 'pass = true' if the user " +
                   "entered something, but no further qualification is performed on the input string. " + 
                   "If the user hit cancel or if the string is empty, then 'pass = false' is returned";
        }
    }
}
