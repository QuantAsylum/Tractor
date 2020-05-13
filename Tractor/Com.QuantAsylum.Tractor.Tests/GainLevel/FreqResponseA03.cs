using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// Performs a swept test of gain using an expo sweep and compares the result to a specified mask file.
    /// </summary>
    [Serializable]
    public class FreqResponseA03 : AudioTestBase
    {

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 212, DisplayText = "Smoothing (1/N), N=", MinValue = 1, MaxValue = 96)]
        public int SmoothingDenominator = 12;

        [ObjectEditorAttribute(Index = 215, DisplayText = "Mask File Name", IsFileName = true, MaxLength = 512)]
        public string MaskFileName = "";

        [ObjectEditorAttribute(Index = 250, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Load Impedance (ohms)", ValidInts = new int[] { 8, 4 })]
        public int ProgrammableLoadImpedance = 8;

        public FreqResponseA03() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IAudioAnalyzer)Tm.TestClass).DoFrAquisition(AnalyzerOutputLevel, 0, SmoothingDenominator);
            ((IAudioAnalyzer)Tm.TestClass).TestMask(MaskFileName, LeftChannel, RightChannel, false, out bool passLeft, out bool passRight, out _);

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
            return "Measures the frequency response into the specified load using a chirp and compares to a mask.";
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
