using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor;

namespace Com.QuantAsylum.Tractor.Tests.IMDTests
{
    /// <summary>
    /// This test performs an ITU IMD test. The user may select an output level. Each generator will
    /// be set to this level, giving an combined output that is 6 dB higher. The amplitude at 1 KHz
    /// will be measured and reported, relative to the combined amplitude. For example, if the amplitude
    /// is specified at -6 dBV, then the reference amplitude is 0 dBV, and the response at 1 KHz is 
    /// referenced to that.
    /// </summary>
    [Serializable]
    public class Imd02 : TestBase
    {
        public float MinimumPassLevel = -200;
        public float MaximumPassLevel = -105;

        public int AnalyzerInputRange = 6;
        public int ProgrammableLoadImpedance = 8;

        /// <summary>
        /// This is the output level for each tone.
        /// </summary>
        public float AnalyzerOutputLevel = -10;

        public Imd02() : base()
        {
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);

            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel - 6, 19000);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(true, AnalyzerOutputLevel - 6, 20000);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(18995, 19005, out double toneRmsL, out double toneRmsR);
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(990, 1010, out double productLeft, out double productRight);
            tr.Value[0] = toneRmsL + 6 - productLeft;
            tr.Value[1] = toneRmsR + 6 - productRight;

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = "1 KHz mixing product is " + tr.Value[0].ToString("0.0") + " dBc from combined amplitude of 19 and  ";
                if ((tr.Value[0] < MinimumPassLevel) || (tr.Value[0] > MaximumPassLevel))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
                if ((tr.Value[1] < MinimumPassLevel) || (tr.Value[1] > MaximumPassLevel))
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
            return string.Format("{0:N1}...{1:N1} dBc", MinimumPassLevel, MaximumPassLevel);
        }

        public override string GetTestDescription()
        {
            return  "Performs IMD ITU Test. This test generates dual tones at 19 and 20 KHz, each with a level 3 dB below the specified amplitude " +
                    "and at the specified load. The combined amplitude of the tones will be the value specified in the test parameters. The resultant " +
                    "mixing product at 1 KHz is measured, and the amplitude relative to the specified amplitude is computed. If the amplitude " +
                    "relative to the specified output amplitude is within the specified window limits, then the test is considered to pass.";
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
