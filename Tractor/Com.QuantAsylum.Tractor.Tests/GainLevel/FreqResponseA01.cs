using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain
    /// </summary>
    [Serializable]
    public class FreqResponseA01 : AudioTestBase
    {

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 215, DisplayText = "Mask File Name", IsFileName = true, MaxLength = 512)]
        public string MaskFileName = "";

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        public FreqResponseA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
            if (FftSize < 32768)
                FftSize = 32768;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize); 
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);

            ((IAudioAnalyzer)Tm.TestClass).DoFrAquisition(AnalyzerOutputLevel);
            ((IAudioAnalyzer)Tm.TestClass).TestMask(MaskFileName, out bool passLeft, out bool passRight);

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            if (LeftChannel)
            {
                tr.StringValue[0] = passLeft.ToString();
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = passRight.ToString();
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
            return "---";
        }

        public override string GetTestDescription()
        {
            return "Measures the frequency response using a chirp and compares to a mask. NOTE: FFT should be >32768.";
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
