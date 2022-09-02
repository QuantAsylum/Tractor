using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.THDs
{
    [Serializable]
    public class ThdB03 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float Freq = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float OutputLevel = -30;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum THD to Pass (dB)", MinValue = -100, MaxValue = 10)]
        public float MinimumOKThd = -110;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum THD to Pass (dB)", MinValue = -100, MaxValue = 10, MustBeGreaterThanIndex = 230)]
        public float MaximumOKThd = -100;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Load Impedance (ohms)", ValidInts = new int[] { 8, 4 })]
        public int LoadImpedance = 8;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Analyzer Input Range")]
        public AudioAnalyzerInputRanges AnalyzerInputRange = new AudioAnalyzerInputRanges() { InputRange = 6 };

        public ThdB03() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange.InputRange);
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(LoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            // Get THD in dB
            ((IAudioAnalyzer)Tm.TestClass).ComputeThdPct(Freq, 20000, out tr.Value[0], out tr.Value[1]);
            tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
            tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);

            // Compute peak
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(Freq * 0.98f, Freq * 1.02f, out double peakLDbv, out double peakRDbv);

            // Convert to volts
            double leftVrms = (float)Math.Pow(10, peakLDbv / 20);
            double rightVrms = (float)Math.Pow(10, peakRDbv / 20);

            // Convert to watts
            double leftWatts = (leftVrms * leftVrms) / LoadImpedance;
            double rightWatts = (rightVrms * rightVrms) / LoadImpedance;

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = string.Format("{0:N1} dB @ {1:N1} Watts", tr.Value[0], leftWatts);
                if ((tr.Value[0] < MinimumOKThd) || (tr.Value[0] > MaximumOKThd))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = string.Format("{0:N1} dB @ {1:N1} Watts", tr.Value[1], rightWatts);
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

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumOKThd, MaximumOKThd);
        }

        public override string GetTestDescription()
        {
            return "Measures THD at a given frequency and amplitude at a given load with power indicated. Results must be withing the specified window to pass.";
        }

        public override bool IsRunnable()
        {
            if (Tm.TestClass is IAudioAnalyzer && Tm.TestClass is IProgrammableLoad)
            {
                return true;
            }

            return false;
        }
    }
}
