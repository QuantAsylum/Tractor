using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.Dialogs;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    public class AuditionA01 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "File Name:", MaxLength = 128)]
        public string AuditionFileName = "";

        [ObjectEditorAttribute(Index = 210, DisplayText = "Output Level (0..1)", MinValue = 0, MaxValue = 1)]
        public float AuditionAmplitude = 0.2f;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Operator Instruction:", MaxLength = 128)]
        public string OperatorInstruction = "";

        public AuditionA01() : base()
        {
            RetryCount = 1;
            Name = "AuditionA01";
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();

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
