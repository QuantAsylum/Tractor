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
    public class Thd01 : TestBase, ITest
    {
        public float Freq = 1000;
        public float OutputLevel = -30;

        public float MinimumOKTHD = -110;
        public float MaximumOKTHD = -100;

        public Thd01() : base()
        {
            Name = "THD01";
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (TestManager.QA401 == null)
                return;

            TestManager.QA401.SetGenerator(QA401.GenType.Gen1, true, OutputLevel, Freq);
            TestManager.QA401.SetGenerator(QA401.GenType.Gen2, false, OutputLevel, Freq);
            TestManager.QA401.RunSingle();

            while (TestManager.QA401.GetAcquisitionState() == QA401.AcquisitionState.Busy)
            {

            }

            TestResultBitmap = CaptureBitmap(TestManager.QA401.GetBitmapBytes());

            value[0] = (float)TestManager.QA401.ComputeTHDPct(TestManager.QA401.GetData(QA401.ChannelType.LeftIn), Freq, 20000);
            value[1] = (float)TestManager.QA401.ComputeTHDPct(TestManager.QA401.GetData(QA401.ChannelType.RightIn), Freq, 20000);

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

        public override string GetTestDescription()
        {
            return "Measures THD at a given frequency and amplitude. Results must be within a given window to 'pass'.";
        }
    }
}
