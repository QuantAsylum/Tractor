using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// Performs a swept test of gain using an expo sweep and compares the result to a specified mask file.
    /// </summary>
    [Serializable]
    public class MicCompareA01 : AudioTestBase
    {

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Windowing (mS) (0=none)", MinValue = 0, MaxValue = 20)]
        public float WindowingMs = 5;

        [ObjectEditorAttribute(Index = 225, DisplayText = "Smoothing (1/N), N=", MinValue = 1, MaxValue = 96)]
        public int SmoothingDenominator = 12;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Mask File Name", IsFileName = true, MaxLength = 512)]
        public string MaskFileName = "";

        [ObjectEditorAttribute(Index = 240, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Check Phase")]
        public bool CheckPhase = false;

        public MicCompareA01() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);

            ((IAudioAnalyzer)Tm.TestClass).DoFrAquisition(AnalyzerOutputLevel, WindowingMs/1000, SmoothingDenominator);
            ((IAudioAnalyzer)Tm.TestClass).TestMask(MaskFileName, false, false, true, out bool passLeft, out bool passRight, out bool passMath);
            ((IAudioAnalyzer)Tm.TestClass).AddMathToDisplay();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            bool passPhase = true;
            int passCount = 0;
            if (CheckPhase) 
            {
                if (((IAudioAnalyzer)Tm.TestClass).LRVerifyPhase((int)FftSize * 1024 / 4)) ++passCount;
                if (((IAudioAnalyzer)Tm.TestClass).LRVerifyPhase((int)FftSize * 1024 / 4 + 300)) ++passCount;
                if (passCount != 2)
                    passPhase = false;
            }


            tr.Pass = passMath && passPhase;

            if (passPhase == false)
            {
                tr.OperatorMessage += "PHASE ";
            }

            if (passMath == false)
            {
                tr.OperatorMessage += "MASK";
            }

            return;
        }

        public override string GetTestLimits()
        {
            return "---";
        }

        public override string GetTestDescription()
        {
            return "Compares a reference microphone (left channel) to a test microphone (right channel). The difference (L-R) is " +
                "displayed and compared to a specified mask.";
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
