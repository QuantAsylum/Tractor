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

namespace Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Power01 : TestBase
    {
        public bool PowerState;

        public float MinimumPassCurrent = 0.005f;
        public float MaximumPassCurrent = 0.01f;

        public Power01() : base()
        {
            TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();
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

        public override bool CheckValues(out string s)
        {
            s = "";

            return true;
        }

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1}A", MinimumPassCurrent, MaximumPassCurrent);
        }

        public override string GetTestDescription()
        {
            return "Sets the power state of the QA450 and measures the current";
        }

        public override bool IsRunnable()
        {
            if ((Tm.TestClass is ICurrentMeter) &&
                (Tm.TestClass is IPowerSupply))
            {
                return true;
            }

            return false;
        }
    }
}
