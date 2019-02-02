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

namespace Com.QuantAsylum.Tractor.Tests.NoiseFloors
{
    /// <summary>
    /// Measures the noise floor without any weighting applied, from 20 to 20KHz
    /// </summary>
    [Serializable]
    public class NoiseFloor02 : TestBase
    {
        public int ProgrammableLoadImpedance = 8;
        public float MinimumPassLevel = -200;
        public float MaximumPassLevel = -105;


        public int AnalyzerInputRange = 6;

        public NoiseFloor02() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            // Disable generators
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(false, -60, 1000);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(false, -60, 1000);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();


            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(20, 20000, out tr.Value[0], out tr.Value[1]);

            if (LeftChannel)
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dBV";
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dBV";
            else
                tr.StringValue[1] = "SKIP";

            if (LeftChannel && tr.Value[0] > MinimumPassLevel && tr.Value[0] < MaximumPassLevel && RightChannel && tr.Value[1] > MinimumPassLevel && tr.Value[1] < MaximumPassLevel)
                tr.Pass = true;
            else if (!LeftChannel && RightChannel && tr.Value[1] > MinimumPassLevel && tr.Value[1] < MaximumPassLevel)
                tr.Pass = true;
            else if (!RightChannel && LeftChannel && tr.Value[0] > MinimumPassLevel && tr.Value[0] < MaximumPassLevel)
                tr.Pass = true;

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
            return string.Format("{0:N1}...{1:N1} dBV", MinimumPassLevel, MaximumPassLevel);
        }

        public override string GetTestDescription()
        {
            return "Measures the noise floor (residual noise) with A-Weighting applied. If the resulting measurement is " +
                   "within the specified limits, then 'pass = true' is returned.";
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
