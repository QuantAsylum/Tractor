using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.THDs
{
    [Serializable]
    public class THD : TestBase, ITest
    {
        public float Freq = 1000;
        public float OutputLevel = -10;

        public float MinimumOKTHD = -110;
        public float MaximumOKTHD = -100;

        public THD() : base()
        {
            Name = "THD";
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

            value[0] = (float)TestManager.AudioAnalyzer.ComputeTHDPct(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.LeftIn), Freq, 20000);
            value[1] = (float)TestManager.AudioAnalyzer.ComputeTHDPct(TestManager.AudioAnalyzer.GetData(QA401.ChannelType.RightIn), Freq, 20000);

            // Convert to db
            value[0] = 20 * (float)Math.Log10(value[0] / 100);
            value[1] = 20 * (float)Math.Log10(value[1] / 100);

            if (LeftChannel && value[0] > MinimumOKTHD && value[0] < MaximumOKTHD && RightChannel && value[1] > MinimumOKTHD && value[1] < MaximumOKTHD)
                pass = true;
            else if (!LeftChannel && RightChannel && value[1] > MinimumOKTHD && value[1] < MaximumOKTHD)
                pass = true;
            else if (!RightChannel && LeftChannel && value[0] > MinimumOKTHD && value[0] < MaximumOKTHD)
                pass = true;

            return;
        }

        public override string TestDescription()
        {
            return "Measures THD";
        }

        /// <summary>
        /// This must return the name of the class. 
        /// </summary>
        /// <returns></returns>
        public override string TestName()
        {
            return "THD";
        }
    }
}
