using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.THDs
{
    [Serializable]
    public class Thd01 : TestBase
    {
        public float Freq = 1000;
        public float OutputLevel = -30;

        public float MinimumOKTHD = -110;
        public float MaximumOKTHD = -100;

        public int InputRange = 6;

        public Thd01() : base()
        {
            Name = "THD01";
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            ((IComposite)Tm.TestClass).SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(InputRange);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).RunSingle();

            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {

            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            tr.Value[0] = (float)((IAudioAnalyzer)Tm.TestClass).ComputeThdPct(((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Left), Freq, 20000);
            tr.Value[1] = (float)((IAudioAnalyzer)Tm.TestClass).ComputeThdPct(((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Right), Freq, 20000);

            // Convert to db
            tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
            tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
                if ((tr.Value[0] < MinimumOKTHD) || (tr.Value[0] > MaximumOKTHD))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
                if ((tr.Value[1] < MinimumOKTHD) || (tr.Value[1] > MaximumOKTHD))
                    passRight = false;
            }
            else
                tr.StringValue[1] = "SKIP";

            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;

            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumOKTHD, MaximumOKTHD);
        }

        public override string GetTestDescription()
        {
            return "Measures THD at a given frequency and amplitude. Results must be within a given window to 'pass'.";
        }

        public override bool IsRunnable()
        {
            if ((Tm.TestClass is IAudioAnalyzer) &&
                 (Tm.TestClass is IProgrammableLoad))
            {
                return true;
            }

            return false;
        }
    }
}
