using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tractor.Com.QuantAsylum.Tractor.Dialogs;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will prompt the user to enter a serial number or other identifier
    /// </summary>
    [Serializable]
    public class Prompt01 : TestBase, ITest
    {
        public string Id { get; set; }

        public string PromptMessage = "???";

        public Prompt01() : base()
        {
            Name = "Prompt";
            TestType = TestTypeEnum.User;
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            MessageBox.Show(PromptMessage);

            pass = true;
            value = new float[] { 0, 0 };
            return;
        }

        public override string GetTestDescription()
        {
            return "Instructs the user to complete an action. 'Pass = true' is always returned.";
        }
    }
}
