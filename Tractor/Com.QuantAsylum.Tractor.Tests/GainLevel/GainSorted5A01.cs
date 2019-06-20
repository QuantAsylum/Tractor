using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain is in one of 5 ranges. Note the 'N' suffix in the class name indicates this supports and
    /// operator notification
    /// </summary>
    [Serializable]
    public class GainSorted5A01N : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue =-100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        public float ExternalAnalyzerInputGain = 0;

        [ObjectEditorAttribute(Index = 225, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        //---------------

        [ObjectEditorAttribute(Index = 229, DisplayText = "")]
        public ObjectEditorSpacer s0;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum Gain to Pass Sort 0 (dB)", MinValue = -100, MaxValue = 100)]
        public float MinimumPassSort0 = -15f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum Gain to Pass Sort 0 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 230)]
        public float MaximumPassSort0 = -10f;

        [ObjectEditorAttribute(Index = 245, DisplayText = "Operator Message Sort 0", MaxLength = 20)]
        public string Sort0Message = "Sort 0";

        //---------------

        [ObjectEditorAttribute(Index = 249, DisplayText = "")]
        public ObjectEditorSpacer s1;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Minimum Gain to Pass Sort 1 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanOrEqualIndex = 240)]
        public float MinimumPassSort1 = -10f;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Maximum Gain to Pass Sort 1 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 250)]
        public float MaximumPassSort1 = -5f;

        [ObjectEditorAttribute(Index = 265, DisplayText = "Operator Message Sort 1", MaxLength = 20)]
        public string Sort1Message = "Sort 1";

        //---------------

        [ObjectEditorAttribute(Index = 269, DisplayText = "")]
        public ObjectEditorSpacer s2;

        [ObjectEditorAttribute(Index = 270, DisplayText = "Minimum Gain to Pass Sort 2 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanOrEqualIndex = 260)]
        public float MinimumPassSort2 = -5f;

        [ObjectEditorAttribute(Index = 280, DisplayText = "Maximum Gain to Pass Sort 2 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 270)]
        public float MaximumPassSort2 = -1f;

        [ObjectEditorAttribute(Index = 285, DisplayText = "Operator Message Sort 2", MaxLength = 20)]
        public string Sort2Message = "Sort 2";

        //---------------

        [ObjectEditorAttribute(Index = 289, DisplayText = "")]
        public ObjectEditorSpacer s3;

        [ObjectEditorAttribute(Index = 290, DisplayText = "Minimum Gain to Pass Sort 3 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanOrEqualIndex = 260)]
        public float MinimumPassSort3 = -1f;

        [ObjectEditorAttribute(Index = 300, DisplayText = "Maximum Gain to Pass Sort 3 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 270)]
        public float MaximumPassSort3 = 5f;

        [ObjectEditorAttribute(Index = 305, DisplayText = "Operator Message Sort 3", MaxLength = 20)]
        public string Sort3Message = "Sort 3";

        //---------------

        [ObjectEditorAttribute(Index = 319, DisplayText = "")]
        public ObjectEditorSpacer s4;

        [ObjectEditorAttribute(Index = 320, DisplayText = "Minimum Gain to Pass Sort 4 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanOrEqualIndex = 260)]
        public float MinimumPassSort4 = 5f;

        [ObjectEditorAttribute(Index = 325, DisplayText = "Maximum Gain to Pass Sort 4 (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 270)]
        public float MaximumPassSort4 = 10f;

        [ObjectEditorAttribute(Index = 330, DisplayText = "Operator Message Sort 4", MaxLength = 20)]
        public string Sort4Message = "Sort 4";

        //---------------


        public GainSorted5A01N() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize); 
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);

            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            // Compute the total RMS around the freq of interest
            ((IAudioAnalyzer)Tm.TestClass).ComputePeak(TestFrequency * 0.90f, TestFrequency * 1.10f, out tr.Value[0], out tr.Value[1]);
            tr.Value[0] = tr.Value[0] + ExternalAnalyzerInputGain - AnalyzerOutputLevel;
            tr.Value[1] = tr.Value[1] + ExternalAnalyzerInputGain - AnalyzerOutputLevel;

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
            else if ((tr.Value[channel] >= MinimumPassSort3) && (tr.Value[channel] <= MaximumPassSort3))
            {
                pass = true;
                tr.OperatorMessage = Sort3Message;
            }
            else if ((tr.Value[channel] >= MinimumPassSort4) && (tr.Value[channel] <= MaximumPassSort4))
            {
                pass = true;
                tr.OperatorMessage = Sort4Message;
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
            return "Measures the gain at a specified frequency and amplitude. The results must fall into 1 of 5 ranges " +
                "to be considered a 'pass'. If the measured gain falls outside of these 5 ranges, it is considered a 'fail'. The operator " +
                "will be prompted in the pass/fail screen with a message corresponding to a particular gain range. This allows performance sorting by an operator.";
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
