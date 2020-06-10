using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.Tests;

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// Enables or disables power to device
    /// </summary>
    [Serializable]
    public class PowerA14 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Power Enabled")]
        public bool PowerState;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Minimum Current to Pass (A)", MinValue = 0, MaxValue = 15, FormatString = "0.000")]
        public float MinimumPassCurrent = 0.005f;

        [ObjectEditorAttribute(Index = 220, DisplayText = "Maximum Current to Pass (A)", MinValue = 0, MaxValue = 15, FormatString = "0.000", MustBeGreaterThanIndex = 210)]
        public float MaximumPassCurrent = 0.01f;

        public PowerA14() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
            SetupBaseTests();

            ((IPowerSupply)Tm.TestClass).SetSupplyState(PowerState);
            Thread.Sleep(1200);
            float current = ((ICurrentMeter)Tm.TestClass).GetDutCurrent(3);


            if ((current > MinimumPassCurrent) && (current < MaximumPassCurrent))
                tr.Pass = true;

            tr.Value[0] = current;
            tr.StringValue[0] = current.ToString("0.000") + "A";
            tr.StringValue[1] = "SKIP";

            return;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N3}...{1:N3}A", MinimumPassCurrent, MaximumPassCurrent);
        }

        public override string GetTestDescription()
        {
            return "Sets the power state on the QA450 and measures the current in that state.";
        }

        public override bool IsRunnable()
        {
            int val = HardwareMask & ((int)HardwareTypes.PowerSupply | (int)HardwareTypes.CurrentMeter);
            return ((val & 0x14) == 0x14);
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.PowerSupply | (int)HardwareTypes.CurrentMeter;
            }
        }
    }
}
