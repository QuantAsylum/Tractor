using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    [Serializable]
    public class Gain : TestBase, ITest
    {           
        public float Freq = 1000;
        public float OutputLevel = -10;

        public float MinimumOKGain = -10.5f;
        public float MaximumOKGain = -9.5f;

        public Gain() : base()
        {
            Name = "GainTest";
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (TestManager.AudioAnalyzer == null)
                return;

            TestManager.AudioAnalyzer.SetGenerator(QA401.GenType.Gen1, true, OutputLevel, Freq);
            TestManager.AudioAnalyzer.SetGenerator(QA401.GenType.Gen2, false, OutputLevel, Freq);
            TestManager.AudioAnalyzer.RunSingle();

            while (TestManager.AudioAnalyzer.GetAcquisitionState() == QA401.AcquisitionState.Busy)
            {

            }

            // Compute the total RMS around the freq of interest
            value[0] = (float)TestManager.AudioAnalyzer.ComputePowerDB(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.LeftIn), Freq * 0.98, Freq * 1.02 );
            value[1] = (float)TestManager.AudioAnalyzer.ComputePowerDB(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.RightIn), Freq * 0.98, Freq * 1.02);

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
            return "Measures the gain at a specified frequency";
        }

        /// <summary>
        /// This must return the name of the class. BUGBUG fix this
        /// </summary>
        /// <returns></returns>
        public override string TestName()
        {
            return "Gain";
        }
    }
}
