using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.NoiseFloors
{
    [Serializable]
    public class NoiseFloor : TestBase, ITest
    {
        public float MinimumOKNoise = -200;
        public float MaximumOKNoise = -105;

        public NoiseFloor() : base()
        {
            Name = "NoiseFloor";
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (TestManager.AudioAnalyzer == null)
                return;

            TestManager.AudioAnalyzer.SetGenerator(QA401.GenType.Gen1, false, -6, 19000);
            TestManager.AudioAnalyzer.SetGenerator(QA401.GenType.Gen2, false, -6, 20000);
            TestManager.AudioAnalyzer.RunSingle();

            while (TestManager.AudioAnalyzer.GetAcquisitionState() == QA401.AcquisitionState.Busy)
            {

            }

            value[0] = (float)TestManager.AudioAnalyzer.ComputePowerDB(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.LeftIn), 20, 20000);
            value[1] = (float)TestManager.AudioAnalyzer.ComputePowerDB(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.RightIn), 20, 20000);

            if (LeftChannel && value[0] > MinimumOKNoise && value[0] < MaximumOKNoise && RightChannel && value[1] > MinimumOKNoise && value[1] < MaximumOKNoise)
                pass = true;
            else if (!LeftChannel && RightChannel && value[1] > MinimumOKNoise && value[1] < MaximumOKNoise)
                pass = true;
            else if (!RightChannel && LeftChannel && value[0] > MinimumOKNoise && value[0] < MaximumOKNoise)
                pass = true;

            return;
        }

        public override string TestDescription()
        {
            return "Measures Noise";
        }

        /// <summary>
        /// This must return the name of the class. 
        /// </summary>
        /// <returns></returns>
        public override string TestName()
        {
            return "NoiseFloor";
        }
    }
}
