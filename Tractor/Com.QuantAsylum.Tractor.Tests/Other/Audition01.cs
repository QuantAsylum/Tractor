using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.Dialogs;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    public class Audition01 : TestBase
    {
        public string AuditionFileName = "";
        public float AuditionAmplitude = 0.2f;
        public string OperatorInstruction = "";

        public Audition01() : base()
        {
            RetryCount = 1;
            TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            ((IInstrument)Tm.TestClass).SetToDefaults();

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
            return "Allows operator to audition a wave file.";
        }

        public override bool IsRunnable()
        {
            return true;
        }

    }
}
