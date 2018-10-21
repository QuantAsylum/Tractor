using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Gain02 : TestBase
    {           
        public float Freq = 1000;
        public float OutputLevel = -30;
        public float ExtGain = 0;

        public float MinimumPassGain = -10.5f;
        public float MaximumPassGain = -9.5f;

        public int OutputImpedance = 8;
        public int InputRange = 6;

        public Gain02() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetInstrumentsToDefault();
            Tm.SetInputRange(InputRange);
            Tm.AudioAnalyzerSetTitle(title);

            Tm.LoadSetImpedance(OutputImpedance); Thread.Sleep(Constants.QA450RelaySettle);

            Tm.AudioGenSetGen1(true, OutputLevel, Freq);
            Tm.AudioGenSetGen2(false, OutputLevel, Freq);
            Tm.RunSingle();

            while (Tm.AnalyzerIsBusy())
            {
                float current = Tm.DutGetCurrent();
                Debug.WriteLine("Current: " + current);
            }

            TestResultBitmap = Tm.GetBitmap();

            // Compute the total RMS around the freq of interest
            tr.Value[0] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Left), Freq * 0.97f, Freq * 1.03f);
            tr.Value[0] = tr.Value[0] - OutputLevel - ExtGain;
            tr.Value[1] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Right), Freq * 0.97f, Freq * 1.03f);
            tr.Value[1] = tr.Value[1] - OutputLevel - ExtGain;

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.00") + " dB";
                if ((tr.Value[0] < MinimumPassGain) || (tr.Value[0] > MaximumPassGain))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.00") + " dB";
                if ((tr.Value[1] < MinimumPassGain) || (tr.Value[1] > MaximumPassGain))
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

        public override bool CheckValues(out string s)
        {
            s = "";
            if (Tm.GetImpedances().Contains(OutputImpedance) == false)
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

        public override string GetTestLimitsString()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumPassGain, MaximumPassGain);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude at a specified impedance level. Results must be within a given window to 'pass'.";
        }
    }
}
