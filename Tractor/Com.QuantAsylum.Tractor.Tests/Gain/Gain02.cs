using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor;
using Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Gain02 : TestBase
    {           
        public float TestFrequency = 1000;
        public float AnalyzerOutputLevel = -30;
        public float ExternalAnalyzerInputGain = 0;

        public float MinimumPassGain = -10.5f;
        public float MaximumPassGain = -9.5f;

        public int ProgrammableLoadImpedance = 8;
        public int AnalyzerInputRange = 6;

        public Gain02() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);

            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {
                float current = ((ICurrentMeter)Tm.TestClass).GetDutCurrent();
                Log.WriteLine(LogType.General, "Current: " + current);
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            // Compute the total RMS around the freq of interest
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(TestFrequency * 0.97f, TestFrequency * 1.03f, out tr.Value[0], out tr.Value[1]);
            tr.Value[0] = tr.Value[0] - AnalyzerOutputLevel - ExternalAnalyzerInputGain;
            tr.Value[1] = tr.Value[1] - AnalyzerOutputLevel - ExternalAnalyzerInputGain;

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
            if (((IProgrammableLoad)Tm.TestClass).GetSupportedImpedances().Contains(ProgrammableLoadImpedance) == false)
            {
                s = "Output impedance must be: " + string.Join(" ", ((IProgrammableLoad)Tm.TestClass).GetSupportedImpedances());
                return false;
            }

            if (((IAudioAnalyzer)Tm.TestClass).GetInputRanges().Contains(AnalyzerInputRange) == false)
            {
                s = "Input range not supported. Must be: " + string.Join(" ", ((IAudioAnalyzer)Tm.TestClass).GetInputRanges());
                return false;
            }

            return true;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumPassGain, MaximumPassGain);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude at a specified load impedance. Results must be within a given window to 'pass'.";
        }

        public override bool IsRunnable()
        {
            if ((Tm.TestClass is IAudioAnalyzer) &&
                (Tm.TestClass is ICurrentMeter) &&
                (Tm.TestClass is IProgrammableLoad))
            {
                return true;
            }

            return false;
        }
    }
}
