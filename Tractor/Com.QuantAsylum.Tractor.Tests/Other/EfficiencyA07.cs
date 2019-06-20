using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class EfficiencyA07 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Amp Voltage", MinValue = 5, MaxValue = 55)]
        public float AmplifierSupplyVoltage = 51;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -15;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Pre-analyzer Input Gain (dB)", MinValue = -100, MaxValue = 100)]
        public float ExternalAnalyzerInputGain = -6;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Minimum Efficiency to Pass (dB)", MinValue = 20, MaxValue = 100)]
        public float MinimumPassEfficiency = 80;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Maximum Efficiency to Pass (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 240)]
        public float MaximumPassEfficiency = 90;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Load Impedance (ohms)", ValidInts = new int[] { 8, 4 })]
        public int ProgrammableLoadImpedance = 8;

        [ObjectEditorAttribute(Index = 270, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 26;

        public EfficiencyA07() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisitionAsync();

            float current = 0;
            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {
                float c = ((ICurrentMeter)Tm.TestClass).GetDutCurrent();
                if (c > current)
                {
                    current = c;
                }

                Log.WriteLine(LogType.General, "Current: " + current.ToString("0.000"));
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            // Get dBV out and adjust based on input gains
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(TestFrequency * 0.98f, TestFrequency * 1.02f, out double peakLDbv, out double peakRDbv);
            peakLDbv -= ExternalAnalyzerInputGain;
            peakRDbv -= ExternalAnalyzerInputGain;

            // Convert to Volts RMS
            double leftVrms = (double)Math.Pow(10, peakLDbv / 20);
            double rightVrms = (double)Math.Pow(10, peakRDbv / 20);

            // Convert to watts
            double wattsL = leftVrms * leftVrms / ProgrammableLoadImpedance;
            double wattsR = rightVrms * rightVrms / ProgrammableLoadImpedance;

            double wattsInPerChannel = AmplifierSupplyVoltage * current / 2;
            tr.Value[0] = 100 * wattsL / wattsInPerChannel;
            tr.Value[1] = 100 * wattsR / wattsInPerChannel;

            bool passLeft = true, passRight = true;
    
            if (LeftChannel)
            {
                tr.StringValue[0] = string.Format("{0:N1}% @ {1:N2} W out", tr.Value[0], wattsL);
                if ((tr.Value[0] < MinimumPassEfficiency) || (tr.Value[0] > MaximumPassEfficiency))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = string.Format("{0:N1}% @ {1:N2} W out", tr.Value[1], wattsR);
                if ((tr.Value[1] < MinimumPassEfficiency) || (tr.Value[1] > MaximumPassEfficiency))
                    passLeft = false;
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
            return string.Format("{0:N1}...{1:N1}%", MinimumPassEfficiency, MaximumPassEfficiency);
        }

        public override string GetTestDescription()
        {
            return "Measures the efficiency of the amplifier under test. This is done by measuring amplifier output power " +
                "and measuring amplifier input power (supply voltage and supply current). The ratio of these measurements " +
                "is the efficiency, expressed in % (0..100)";
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.AudioAnalyzer | (int)HardwareTypes.ProgrammableLoad | (int)HardwareTypes.CurrentMeter;
            }
        }
    }
}
