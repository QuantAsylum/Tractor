using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Efficiency01 : TestBase
    {
        public float AmpVoltage = 51;
        public float MinimumPassEfficiency = 80;
        public float MaximumPassEfficiency = 90;
        public float Freq = 1000;
        public float OutputLevel = -15;
        public float ExtGain = -6;
        public int LoadImpedance = 8;
        public int InputRange = 26;

        public Efficiency01() : base()
        {
            TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetInstrumentsToDefault();
            Tm.LoadSetImpedance(LoadImpedance); Thread.Sleep(Constants.QA450RelaySettle);

            Tm.AudioAnalyzerSetTitle(title);
            Tm.SetInputRange(InputRange);
            Tm.AudioAnalyzerSetFftLength(8192);
            Tm.AudioGenSetGen1(true, OutputLevel, Freq);
            Tm.AudioGenSetGen2(false, OutputLevel, Freq);
            Tm.RunSingle();

            float current = 0;
            while (Tm.AnalyzerIsBusy())
            {
                float c = Tm.DutGetCurrent();
                if (c > current)
                    current = c;
                Debug.WriteLine("Current: " + current);
            }

            TestResultBitmap = Tm.GetBitmap();


            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                // Get dbV out
                float wattsOut = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Left), Freq * 0.98f, Freq * 1.02f) - ExtGain;
                // Get volts out
                wattsOut = (float)Math.Pow(10, wattsOut / 20);
                // Get watts out
                wattsOut = (wattsOut * wattsOut) / LoadImpedance;
                // Since two channels are active, we must divide current by 2
                float wattsIn = AmpVoltage * current/2;
                tr.Value[0] = 100 * wattsOut / wattsIn;
                tr.StringValue[0] = string.Format("{0:N1}% @ {1:N2} W out", tr.Value[0], wattsOut);
                if ((tr.Value[0] < MinimumPassEfficiency) || (tr.Value[0] > MaximumPassEfficiency))
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
                // Since two channels are active, we must divide current by 2
                float wattsIn = AmpVoltage * current/2;
                tr.Value[1] = 100 * wattsOut / wattsIn;
                tr.StringValue[1] = string.Format("{0:N1}% @ {1:N2} W out", tr.Value[1], wattsOut);
                if ((tr.Value[1] < MinimumPassEfficiency) || (tr.Value[1] > MaximumPassEfficiency))
                    passLeft = false;
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
            return string.Format("{0:N1}...{1:N1}%", MinimumPassEfficiency, MaximumPassEfficiency);
        }


        public override bool CheckValues(out string s)
        {
            s = "";
            if (Tm.GetImpedances().Contains(LoadImpedance) == false)
            {
                s = "Output impedance must be: " + string.Join(" ", Tm.GetImpedances());
                return false;
            }

            if (Tm.GetInputRanges().Contains(InputRange) == false)
            {
                s = "Input range not supported. Must be: " + string.Join(" ", Tm.GetInputRanges());
                return false;
            }

            return true;
        }

        public override string GetTestDescription()
        {
            return "Sets the power state of the QA450 and measures the current";
        }
    }
}
