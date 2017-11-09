using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.IMDTests
{
    [Serializable]
    public class IMD_ITU : TestBase, ITest
    {
        public float MinimumOKGain = -200;
        public float MaximumOKGain = -105;

        public IMD_ITU() : base()
        {
            Name = "IMD ITU (19K + 20K)";
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (TestManager.AudioAnalyzer == null)
                return;

            TestManager.AudioAnalyzer.SetGenerator(QA401.GenType.Gen1, true, -6, 19000);
            TestManager.AudioAnalyzer.SetGenerator(QA401.GenType.Gen2, true, -6, 20000);
            TestManager.AudioAnalyzer.RunSingle();

            while (TestManager.AudioAnalyzer.GetAcquisitionState() == QA401.AcquisitionState.Busy)
            {

            }

            value[0] = (float)TestManager.AudioAnalyzer.ComputePowerDB(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.LeftIn), 995, 1005);
            value[1] = (float)TestManager.AudioAnalyzer.ComputePowerDB(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.RightIn), 995, 1005);

            if (LeftChannel && value[0] > MinimumOKGain && value[0] < MaximumOKGain && RightChannel && value[1] > MinimumOKGain && value[1] < MaximumOKGain)
                pass = true;
            else if (!LeftChannel && RightChannel && value[1] > MinimumOKGain && value[1] < MaximumOKGain)
                pass = true;
            else if (!RightChannel && LeftChannel && value[0] > MinimumOKGain && value[0] < MaximumOKGain)
                pass = true;

            return;
        }

        public override string TestDescription()
        {
            return "Performs IMD ITU Test";
        }

        /// <summary>
        /// This must return the name of the class. BUGBUG fix this
        /// </summary>
        /// <returns></returns>
        public override string TestName()
        {
            return "IMD_ITU";
        }
    }
}
