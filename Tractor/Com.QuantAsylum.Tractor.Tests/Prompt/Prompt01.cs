using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Com.QuantAsylum.Tractor.Dialogs;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Dialogs;
using System.Drawing;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will prompt the user to enter a serial number or other identifier
    /// </summary>
    [Serializable]
    public class Prompt01 : TestBase
    {
        public string Id { get; set; }

        public string PromptMessage = "???";
        public string BitmapFile = "";
        public bool ShowFailButton = true;

        public Prompt01() : base()
        {
            Name = "Prompt";
            RetryCount = 1;
            TestType = TestTypeEnum.User;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Bitmap bmp = null;
            try
            {
                bmp = new Bitmap(BitmapFile);
            }
            catch
            {

            }

            DlgPrompt dlg = new DlgPrompt(PromptMessage, ShowFailButton, bmp);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tr.Pass = true;
            }

            
            return;
        }

        public override string GetTestDescription()
        {
            return "Instructs the user to complete an action. 'Pass = true' is always returned.";
        }

        public override bool IsRunnable()
        {
            return true;
        }
    }
}
