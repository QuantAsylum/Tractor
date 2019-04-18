using Com.QuantAsylum.Tractor.TestManagers;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class ImpedanceA03 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float TestFrequency = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float AnalyzerOutputLevel = -30;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Minimum Gain to Pass (dB)", MinValue = -100, MaxValue = 100)]
        public float MinimumPassImpedance = 0.01f;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Maximum Gain to Pass (dB)", MinValue = -100, MaxValue = 100, MustBeGreaterThanIndex = 220)]
        public float MaximumPassImpedance = 0.2f;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int AnalyzerInputRange = 6;

        public ImpedanceA03() : base()
        {
            Name = "ImpedanceA03";
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            double[] vOut4 = new double[2] { double.NaN, double.NaN };
            double[] vOut8 = new double[2] { double.NaN, double.NaN };

            Tm.SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(AnalyzerInputRange);

            // First, we make 8 ohm measurement
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(8);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            // Grab the 8 ohm levels
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(TestFrequency * 0.98f, TestFrequency * 1.02f, out vOut8[0], out vOut8[1]);

            // Now make 4 ohm meausrement
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(4);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            // Grab the 4 ohm circuit levels
            ((IAudioAnalyzer)Tm.TestClass).ComputeRms(TestFrequency * 0.98f, TestFrequency * 1.02f, out vOut4[0], out vOut4[1]);

            // Compute impedance
            for (int i = 0; i < 2; i++)
            {
                if (!double.IsNaN(vOut4[i]) && !double.IsNaN(vOut8[i]))
                    tr.Value[i] = CalcImpedance(vOut4[i], vOut8[i]);
            }

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0000") + " ohms";
                if ((tr.Value[0] < MinimumPassImpedance) || (tr.Value[0] > MaximumPassImpedance))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0000") + " ohms";
                if ((tr.Value[1] < MinimumPassImpedance) || (tr.Value[1] > MaximumPassImpedance))
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

        /// <summary>
        /// Calculates the output impedance given two measurements: Amplitude at 4 ohms and amplitude at 8 ohms.
        /// We have two equations and two unknowns:
        /// Vout4 = Vin * 4/(R+4)
        /// Vout8 = Vin * 8/(R+8)
        /// Vin is the same for both, and R is the same for both
        /// Vin is the input of the amp BEFORE the amp's output impedance
        /// R is the amp output impedance
        /// To solve, re-arrange eq 1 in terms of Vin = Vout1 * (R+4)/4
        /// Then sub that into equation 2 for Vin. And then go to Wolfram and
        /// ask wolfram to solve for R. That yields R = 8*(Vout4 - Vout8)/(2*Vout4 - Vout8)
        /// </summary>
        /// <param name="openCircuitLevel"></param>
        /// <param name="loadedCircuitLevel"></param>
        /// <param name="impedance"></param>
        /// <returns></returns>
        double CalcImpedance(double vOut4, double vOut8)
        {
            // Convert levels to linear
            vOut4 = (float)Math.Pow(10, vOut4 / 20);
            vOut8 = (float)Math.Pow(10, vOut8 / 20);
            return - 8 * (vOut4 - vOut8)/(2*vOut4 - vOut8);
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} Ohms", MinimumPassImpedance, MaximumPassImpedance);
        }

        public override string GetTestDescription()
        {
            return "Measures the output impedance of an amplifier using an " +
                   "open-circuit measurement and a measurement at a " +
                   "specified load.";
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
