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
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.IMDTests
{
    /// <summary>
    /// This test performs an ITU IMD test. The user may select an output level. Each generator will
    /// be set to this level, giving an combined output that is 6 dB higher. The amplitude at 1 KHz
    /// will be measured and reported, relative to the combined amplitude. For example, if the amplitude
    /// is specified at -3 dBV, then the reference amplitude is 0 dBV, and the response at 1 KHz is 
    /// referenced to that.
    /// </summary>
    [Serializable]
    public class ImdA01 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Minimum Level to Pass (dB)", MinValue = -200, MaxValue = 100)]
        public float MinimumPassLevel = -250;

        [ObjectEditorAttribute(Index = 205, DisplayText = "Maximum Level to Pass (dB)", MinValue = -200, MaxValue = 100, MustBeGreaterThanIndex = 200)]
        public float MaximumPassLevel = -105;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -50, MaxValue = 6)]
        public float AnalyzerOutputLevel = -10;

        [ObjectEditorAttribute(Index = 215, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        public ImdA01() : base()
        {
            Name = "ImdA01";
            _TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);

            // The RMS of two distinct but equal tones is 3 dBV above level of the tones
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel - 3, 19000);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(true, AnalyzerOutputLevel - 3, 20000);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(18995, 19005, out double l1, out double r1);
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(995, 1005, out double l2, out double r2);

            if (LeftChannel)
            {
                tr.Value[0] = l2 - l1;
            }

            if (RightChannel)
            {
                tr.Value[1] = r2 - r1;
            }

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
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

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dBc", MinimumPassLevel, MaximumPassLevel);
        }

        public override string GetTestDescription()
        {
            return "Performs IMD ITU Test. This test generates dual tones at 19 and 20 KHz, each with a level 3 dB below the specified amplitude. " +
                   "The combined amplitude of the tones will be the value specified in the test parameters. The resultant mixing product at 1 KHz " +
                   "is measured, and the amplitude relative to the specified amplitude is computed. If the amplitude relative to the specified " +
                   "output amplitude is within the specified window limits, then the test is considered to pass.";
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.AudioAnalyzer;
            }
        }

    }
}
