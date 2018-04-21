using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.NoiseFloors
{
    /// <summary>
    /// Measures the noise floor without any weighting applied, from 20 to 20KHz
    /// </summary>
    [Serializable]
    public class NoiseFloor01 : TestBase, ITest
    {
        public float MinimumOKNoise = -200;
        public float MaximumOKNoise = -105;

        public NoiseFloor01() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (TestManager.QA401 == null)
                return;

            // Disable generators
            TestManager.QA401.SetGenerator(QA401.GenType.Gen1, false, -60, 1000);
            TestManager.QA401.SetGenerator(QA401.GenType.Gen2, false, -60, 1000);
            TestManager.QA401.RunSingle();

            while (TestManager.QA401.GetAcquisitionState() == QA401.AcquisitionState.Busy)
            {
                Thread.Sleep(20);
            }

            TestResultBitmap = CaptureBitmap(TestManager.QA401.GetBitmapBytes());

            value[0] = (float)TestManager.QA401.ComputePowerDB(TestManager.QA401.GetData(QA401.ChannelType.LeftIn), 20, 20000);
            value[1] = (float)TestManager.QA401.ComputePowerDB(TestManager.QA401.GetData(QA401.ChannelType.RightIn), 20, 20000);

            if (LeftChannel && value[0] > MinimumOKNoise && value[0] < MaximumOKNoise && RightChannel && value[1] > MinimumOKNoise && value[1] < MaximumOKNoise)
                pass = true;
            else if (!LeftChannel && RightChannel && value[1] > MinimumOKNoise && value[1] < MaximumOKNoise)
                pass = true;
            else if (!RightChannel && LeftChannel && value[0] > MinimumOKNoise && value[0] < MaximumOKNoise)
                pass = true;

            return;
        }

        public override string GetTestDescription()
        {
            return "Measures the noise floor (residual noise) with A-Weighting applied. If the resulting measurement is " +
                   "within the specified limits, then 'pass = true' is returned.";
        }
    }
}
