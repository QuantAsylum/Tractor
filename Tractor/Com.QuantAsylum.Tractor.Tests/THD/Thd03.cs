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
    public class Thd03 : TestBase
    {
        public float Freq = 1000;
        public float OutputLevel = -30;

        public float MinimumOKThd = -110;
        public float MaximumOKThd = -100;

        public int LoadImpedance = 8;
        public int InputRange = 6;
        public int ExtGain = -6;

        public Thd03() : base()
        {
            Name = "THD03";
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetInstrumentsToDefault();
            Tm.AudioAnalyzerSetTitle(title);
            Tm.SetInputRange(InputRange);
            Tm.LoadSetImpedance(LoadImpedance); Thread.Sleep(Constants.QA450RelaySettle);

            Tm.AudioGenSetGen1(true, OutputLevel, Freq);
            Tm.AudioGenSetGen2(false, OutputLevel, Freq);
            Tm.RunSingle();

            while (Tm.AnalyzerIsBusy())
            {
                Thread.Sleep(25);
            }

            TestResultBitmap = Tm.GetBitmap();

            
            //tr.Value[1] = (float)Tm.ComputeThdPct(Tm.GetData(ChannelEnum.Right), Freq, 20000);

            // Convert to db
            //tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
            //tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                // Get dbV out
                float wattsOut = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Left), Freq * 0.98f, Freq * 1.02f) - ExtGain;
                // Get volts out
                wattsOut = (float)Math.Pow(10, wattsOut / 20);
                // Get watts out
                wattsOut = (wattsOut * wattsOut) / LoadImpedance;

                tr.Value[0] = (float)Tm.ComputeThdPct(Tm.GetData(ChannelEnum.Left), Freq, 20000);
                // Convert to dB
                tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
                tr.StringValue[0] = string.Format("{0:N1} dB @ {1:N1}W", tr.Value[0], wattsOut);
                if ((tr.Value[0] < MinimumOKThd) || (tr.Value[0] > MaximumOKThd))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                // Get dbV out
                float wattsOut = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Right), Freq * 0.98f, Freq * 1.02f) - ExtGain;
                // Get volts out
                wattsOut = (float)Math.Pow(10, wattsOut / 20);
                // Get watts out
                wattsOut = (wattsOut * wattsOut) / LoadImpedance;

                tr.Value[1] = (float)Tm.ComputeThdPct(Tm.GetData(ChannelEnum.Right), Freq, 20000);
                // Convert to dB
                tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);
                tr.StringValue[1] = string.Format("{0:N1} dB @ {1:N1}W", tr.Value[1], wattsOut);
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
            return "Measures THD at a given frequency and amplitude at a given load";
        }
    }
}
