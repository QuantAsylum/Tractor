using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.THDs
{
    [Serializable]
    public class Thd02 : TestBase
    {
        public float Freq = 1000;
        public float OutputLevel = -30;

        public float MinimumOKThd = -110;
        public float MaximumOKThd = -100;

        public int OutputImpedance = 8;
        public int InputRange = 6;

        public Thd02() : base()
        {
            Name = "THD02";
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            ((IComposite)Tm.TestClass).SetToDefaults();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(InputRange);
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(OutputImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).RunSingle();

            while (((IAudioAnalyzer)Tm).AnalyzerIsBusy())
            {
                Thread.Sleep(25);
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            tr.Value[0] = (float)((IAudioAnalyzer)Tm.TestClass).ComputeThdPct(((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Left), Freq, 20000);
            tr.Value[1] = (float)((IAudioAnalyzer)Tm).ComputeThdPct(((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Right), Freq, 20000);

            // Convert to db
            tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
            tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
                if ((tr.Value[0] < MinimumOKThd) || (tr.Value[0] > MaximumOKThd))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
                if ((tr.Value[1] < MinimumOKThd) || (tr.Value[1] > MaximumOKThd))
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

        public override string GetTestLimitsString()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumOKThd, MaximumOKThd);
        }

        public override string GetTestDescription()
        {
            return "Measures THD at a given frequency and amplitude at a given load with power indicated";
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
