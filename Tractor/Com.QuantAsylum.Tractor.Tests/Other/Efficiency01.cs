using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Efficiency01 : TestBase
    {
        public float AmplifierSupplyVoltage = 51;
        public float MinimumPassEfficiency = 80;
        public float MaximumPassEfficiency = 90;
        public float TestFrequency = 1000;
        public float AnalyzerOutputLevel = -15;
        public float ExternalAnalyzerInputGain = -6;
        public int ProgrammableLoadImpedance = 8;
        public int AnalyzerInputRange = 26;

        public Efficiency01() : base()
        {
            TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(8192);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisitionAsync();

            float current = 0;
            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {
                float c = ((ICurrentMeter)Tm).GetDutCurrent();
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

        public override string GetTestDescription()
        {
            return "Measures the efficiency of the amplifier under test. This is done by measuring amplifier output power " +
                "and measuring amplifier input power (supply voltage and supply current). The ratio of these measurements " +
                "is the efficiency.";
        }

        public override bool IsRunnable()
        {
            if ((Tm.TestClass is IAudioAnalyzer) &&
                (Tm.TestClass is ICurrentMeter) &&
                (Tm.TestClass is IProgrammableLoad))
            {
                return true;
            }

            return false;
        }
    }
}
