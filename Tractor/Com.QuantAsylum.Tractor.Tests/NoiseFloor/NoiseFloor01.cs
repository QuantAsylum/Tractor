using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

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

            if (Tm == null)
                return;

            // Disable generators
            Tm.AudioGenSetGen1(false, -60, 1000);
            Tm.AudioGenSetGen1(false, -60, 1000);
            Tm.RunSingle();

            while (Tm.AnalyzerIsBusy())
            {
                Thread.Sleep(20);
            }

            TestResultBitmap = Tm.GetBitmap();

            value[0] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Left), 20, 20000);
            value[1] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Right), 20, 20000);

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
