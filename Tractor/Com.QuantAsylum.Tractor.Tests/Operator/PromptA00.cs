using Com.QuantAsylum.Tractor.Dialogs;
using System;
using System.Drawing;
using System.Windows.Forms;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests
{
    /// <summary>
    /// This test will prompt the user to enter a serial number or other identifier
    /// </summary>
    [Serializable]
    public class PromptA00 : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Prompt Message:", MaxLength = 128)]
        public string PromptMessage = "";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Bitmap File Name:", MaxLength = 512, IsFileName = true, FileNameCanBeEmpty = true)]
        public string BitmapFile = "";

        [ObjectEditorAttribute(Index = 220, DisplayText = "Display Fail Button")]
        public bool ShowFailButton = true;

        public PromptA00() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Operator;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Bitmap bmp = null;
            try
            {
                if (BitmapFile.Trim() != "")
                {
                    bmp = new Bitmap(BitmapFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load specified bitmap file {BitmapFile}. Exception: " + ex.Message);
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
            return "Instructs the user to complete an action. A PNG image (512x384) may be specified as an instruction aid, and the operator" +
                "may optionally decide if the action succeeded or failed. If an image is specified, it will be stretched to fit, maintaining" +
                "its current aspect ratio.";
        }
    }
}
