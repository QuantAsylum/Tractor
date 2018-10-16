using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.Other
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Power01 : TestBase
    {
        public bool PowerState;

        public float MinimumPassCurrent = 0.01f;
        public float MaximumPassCurrent = 0.01f;

        public Power01() : base()
        {
            TestType = TestTypeEnum.Other;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetInstrumentsToDefault();
            Tm.DutSetPowerState(PowerState);
            Thread.Sleep(1200);
            float current = Tm.DutGetCurrent(3);


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

        public override string GetTestDescription()
        {
            return "Sets the power state of the QA450 and measures the current";
        }
    }
}
