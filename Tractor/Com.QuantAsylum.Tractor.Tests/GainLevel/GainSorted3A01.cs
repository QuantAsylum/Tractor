using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain is in one of 3 ranges. Note the 'N' suffix in the class name indicates this supports and
    /// operator notification
    /// </summary>
    [Serializable]
    public class GainSorted3A01N : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        //[ObjectEditorAttribute(Index = 220, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        //public float ExternalAnalyzerInputGain = 0;

        [ObjectEditorAttribute(Index = 225, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        //---------------

        [ObjectEditorAttribute(Index = 227, DisplayText = "")]
        public ObjectEditorSpacer s0;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Gain to Pass Sort 0 (dB)", MinValue = -100, MaxValue = 100)]
        public float MinimumPassSort0 = -15f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Gain to Pass Sort 0 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 230)]
        public float MaximumPassSort0 = -5f;

        [ObjectEditorAttribute(Index = 245, DisplayText = "Operator Message Sort 0", MaxLength = 20)]
        public string Sort0Message = "Sort 0";

        //---------------

        [ObjectEditorAttribute(Index = 249, DisplayText = "")]
        public ObjectEditorSpacer s1;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Minimum Gain to Pass Sort 1 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanOrEqualIndex = 240)]
        public float MinimumPassSort1 = -5f; 

        [ObjectEditorAttribute(Index = 260, DisplayText = "Maximum Gain to Pass Sort 1 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 250)]
        public float MaximumPassSort1 = 5f;

        [ObjectEditorAttribute(Index = 265, DisplayText = "Operator Message Sort 1", MaxLength = 20)]
        public string Sort1Message = "Sort 1";

        //---------------

        [ObjectEditorAttribute(Index = 269, DisplayText = "")]
        public ObjectEditorSpacer s2;

        [ObjectEditorAttribute(Index = 270, DisplayText = "Minimum Gain to Pass Sort 2 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanOrEqualIndex = 260)]
        public float MinimumPassSort2 = 5f;

        [ObjectEditorAttribute(Index = 280, DisplayText = "Maximum Gain to Pass Sort 2 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 270)]
        public float MaximumPassSort2 = 15f;

        [ObjectEditorAttribute(Index = 285, DisplayText = "Operator Message Sort 2", MaxLength = 20)]
        public string Sort2Message = "Sort 2";

        //---------------


        public GainSorted3A01N() : base()
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

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);

            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            // Compute the total RMS around the freq of interest
            ((IAudioAnalyzer)Tm.TestClass).ComputePeakDb(TestFrequency * 0.90f, TestFrequency * 1.10f, out tr.Value[0], out tr.Value[1]);
            tr.Value[0] = tr.Value[0] - AnalyzerOutputLevel;
            tr.Value[1] = tr.Value[1] - AnalyzerOutputLevel;

            bool passLeft = false, passRight = false;

            if (LeftChannel)
            {
                passLeft = CheckChannel(0, tr);
            }
            else
            {
                tr.StringValue[1] = "SKIP";
            }

            if (RightChannel)
            {
                passRight = CheckChannel(1, tr);
            }
            else
            {
                tr.StringValue[1] = "SKIP";
            }


            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;

            return;
        }


        bool CheckChannel(int channel, TestResult tr)
        {
            bool pass = false;

            tr.StringValue[channel] = tr.Value[channel].ToString("0.00") + " dB";
            if ((tr.Value[channel] >= MinimumPassSort0) && (tr.Value[channel] <= MaximumPassSort0))
            {
                pass = true;
                tr.OperatorMessage = Sort0Message;
            }
            else if ((tr.Value[channel] >= MinimumPassSort1) && (tr.Value[channel] <= MaximumPassSort1))
            {
                pass = true;
                tr.OperatorMessage = Sort1Message;
            }
            else if ((tr.Value[channel] >= MinimumPassSort2) && (tr.Value[channel] <= MaximumPassSort2))
            {
                pass = true;
                tr.OperatorMessage = Sort2Message;
            }

            return pass;
        }

        public override string GetTestLimits()
        {
            //return string.Format("{0:N1}...{1:N1} dB", MinimumPassGain, MaximumPassGain);
            return "";
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude on the left channel. The results must fall into 1 of 3 ranges " +
                "to be considered a 'pass'. If the measured gain falls outside of these 3 ranges, it is considered a 'fail'. The operator " +
                "will be prompted in the pass/fail screen with a message corresponding to a particular gain range. This allows performance sorting by an operator.";
        }

        public override bool IsRunnable()
        {
            if (Tm.TestClass is IAudioAnalyzer)
            {
                return true;
            }

            return false;
        }

       
    }
}
