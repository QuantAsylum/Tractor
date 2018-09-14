using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

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

            if (Tm == null)
                return;

            Tm.AudioGenSetGen1(true, OutputLevel, Freq);
            Tm.AudioGenSetGen2(false, OutputLevel, Freq);
            Tm.RunSingle();

            while (Tm.AnalyzerIsBusy())
            {

            }

            TestResultBitmap = Tm.GetBitmap();

            value[0] = (float)Tm.ComputeThdPct(Tm.GetData(ChannelEnum.Left), Freq, 20000);
            value[1] = (float)Tm.ComputeThdPct(Tm.GetData(ChannelEnum.Right), Freq, 20000);

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
