using Com.QuantAsylum.Tractor.Dialogs;
using Com.QuantAsylum.Tractor.TestManagers;
using System.IO;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    public class AuditionA01 : UiTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "WAV File Name", MaxLength = 512, IsFileName = true)]
        public string AuditionFileName = "";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Output Level (0..1)", MinValue = 0, MaxValue = 1)]
        public float AuditionAmplitude = 0.2f;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Operator Instruction", MaxLength = 128)]
        public string OperatorInstruction = "";

        public AuditionA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Operator;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();

            if (File.Exists(AuditionFileName) == false)
            {
                throw new FileLoadException("Specified audition file doesn't exist");
            }

            try
            {
                ((IAudioAnalyzer)Tm.TestClass).AuditionStart(AuditionFileName, AuditionAmplitude, true);

                DlgAudition dlg = new DlgAudition((IAudioAnalyzer)Tm.TestClass, AuditionAmplitude, OperatorInstruction);

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tr.Pass = true;
                }
                else
                {
                    tr.Pass = false;
                }
            }
            catch
            {

            }

            ((IAudioAnalyzer)Tm.TestClass).AuditionStop();
        }

        public override string GetTestDescription()
        {
            return "Allows operator to audition a wave file. This can be helpful for checking volume and tone controls.";
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.AudioAnalyzer;
            }
        }

    }
}
