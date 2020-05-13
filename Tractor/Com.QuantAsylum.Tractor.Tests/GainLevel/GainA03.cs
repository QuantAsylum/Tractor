using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class GainA03 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        //[ObjectEditorAttribute(Index = 220, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        //public float ExternalAnalyzerInputGain = 0;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Gain to Pass (dB)", MinValue = -100, MaxValue = 100)]
        public float MinimumPassGain = -10.5f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Gain to Pass (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 230)]
        public float MaximumPassGain = -9.5f;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Load Impedance (ohms)", ValidInts = new int[] { 8, 4 })]
        public int ProgrammableLoadImpedance = 8;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        public GainA03() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

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
            ((IAudioAnalyzer)Tm.TestClass).ComputePeakDb(TestFrequency * 0.90f, TestFrequency * 1.10f, out tr.Value[0], out tr.Value[1]);
            tr.Value[0] = tr.Value[0] - AnalyzerOutputLevel;
            tr.Value[1] = tr.Value[1] - AnalyzerOutputLevel;

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

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumPassGain, MaximumPassGain);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude at a specified load impedance. Results must be within a given window to pass.";
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.AudioAnalyzer | (int)HardwareTypes.ProgrammableLoad;
            }
        }
    }
}
