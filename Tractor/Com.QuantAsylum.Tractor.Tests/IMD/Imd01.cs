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

namespace Tractor.Com.QuantAsylum.Tractor.Tests.IMDTests
{
    /// <summary>
    /// This test performs an ITU IMD test. The user may select an output level. Each generator will
    /// be set to this level, giving an combined output that is 6 dB higher. The amplitude at 1 KHz
    /// will be measured and reported, relative to the combined amplitude. For example, if the amplitude
    /// is specified at -6 dBV, then the reference amplitude is 0 dBV, and the response at 1 KHz is 
    /// referenced to that.
    /// </summary>
    [Serializable]
    public class Imd01 : TestBase, ITest
    {
        public float MinimumLevelDbc = -200;
        public float MaximumLevelDbc = -105;

        /// <summary>
        /// This is the output level for each tone.
        /// </summary>
        public float OutputLevelDBV = -10;

        public Imd01() : base()
        {
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (Tm == null)
                return;

            Tm.AudioGenSetGen1(true, OutputLevelDBV - 6, 19000);
            Tm.AudioGenSetGen1(true, OutputLevelDBV - 6, 20000);
            Tm.RunSingle();

            while (Tm.AnalyzerIsBusy())
            {
                Thread.Sleep(20);
            }

            TestResultBitmap = Tm.GetBitmap();

            value[0] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Left), 995, 1005);
            value[1] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Right), 995, 1005);

            value[0] = -(OutputLevelDBV + 6 - value[0]);
            value[1] = -(OutputLevelDBV + 6 - value[1]);

            if (LeftChannel && value[0] > MinimumLevelDbc && value[0] < MaximumLevelDbc && RightChannel && value[1] > MinimumLevelDbc && value[1] < MaximumLevelDbc)
                pass = true;
            else if (!LeftChannel && RightChannel && value[1] > MinimumLevelDbc && value[1] < MaximumLevelDbc)
                pass = true;
            else if (!RightChannel && LeftChannel && value[0] > MinimumLevelDbc && value[0] < MaximumLevelDbc)
                pass = true;

            return;
        }

        public override string GetTestDescription()
        {
            return "Performs IMD ITU Test. This test generates dual tones at 19 and 20 KHz, each with a level 6 dB below the specified amplitude. " +
                   "The combined amplitude of the tones will be the value specified in the test parameters. The resultant mixing product at 1 KHz " +
                   "is measured, and the amplitude below the specified output amplitude is computed. If that amplitude relative to the output amplitude " +
                   "is within the specified window limits, then 'pass = true' is returned. ";
        }

    }
}
