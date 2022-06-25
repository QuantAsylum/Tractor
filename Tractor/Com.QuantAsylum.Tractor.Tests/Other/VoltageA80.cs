using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using Tractor.Com.QuantAsylum.Tractor.Tests;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// Measures voltage on a specified channel
    /// </summary>
    [Serializable]
    public class VoltageA80 : TestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Channel", MinValue = 0, MaxValue = 15, FormatString = "0.000")]
        public int ChannelIndex;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Minimum Voltage to Pass (V)", MinValue = -100, MaxValue = 100, FormatString = "0.000")]
        public float MinimumPassVoltage = 11.9f;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Maximum Voltage to Pass (V)", MinValue = -100, MaxValue = 100, FormatString = "0.000", MustBeGreaterThanIndex = 210)]
        public float MaximumPassVoltage = 12.1f;

        public VoltageA80() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            //Tm.SetToDefaults();
            //SetupBaseTests();

            float voltage = ((IVoltMeter)Tm.TestClass).GetVoltage(ChannelIndex);

            if ((voltage > MinimumPassVoltage) && (voltage < MaximumPassVoltage))
                tr.Pass = true;

            tr.Value[0] = voltage;
            tr.StringValue[0] = voltage.ToString("0.000") + "A";
            tr.StringValue[1] = "SKIP";

            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N3}...{1:N3}A", MinimumPassVoltage, MaximumPassVoltage);
        }

        public override string GetTestDescription()
        {
            return "Measures the voltage on the specified channel of the voltmeter and verifies it is within the specified limits. " +
                "This test is useful for verifying supply rails during the test.";
        }

        public override bool IsRunnable()
        {
            if (Tm.TestClass is IVoltMeter && Tm.TestClass is IPowerSupply)
            {
                return true;
            }

            return false;
        }
    }
}
