using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor;

namespace Com.QuantAsylum.Tractor.Tests.IMDTests
{
    /// <summary>
    /// This test performs an ITU IMD test. The user may select an output level. Each generator will
    /// be set to this level, giving an combined output that is 6 dB higher. The amplitude at 1 KHz
    /// will be measured and reported, relative to the combined amplitude. For example, if the amplitude
    /// is specified at -6 dBV, then the reference amplitude is 0 dBV, and the response at 1 KHz is 
    /// referenced to that.
    /// </summary>
    [Serializable]
    public class Imd02 : TestBase
    {
        public float MinimumLevelDbc = -200;
        public float MaximumLevelDbc = -105;

        public int InputRange = 6;
        public int OutputImpedance = 8;

        /// <summary>
        /// This is the output level for each tone.
        /// </summary>
        public float OutputLevelDBV = -10;

        public Imd02() : base()
        {
            TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            ((IComposite)Tm).SetToDefaults();
            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(InputRange);

            ((IProgrammableLoad)Tm.TestClass).SetImpedance(OutputImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, OutputLevelDBV - 6, 19000);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(true, OutputLevelDBV - 6, 20000);
            ((IAudioAnalyzer)Tm.TestClass).RunSingle();

            while (((IAudioAnalyzer)Tm.TestClass).AnalyzerIsBusy())
            {
                Thread.Sleep(20);
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();

            if (LeftChannel)
            {
                PointD[] data = ((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Left);
                tr.Value[0] = ((IAudioAnalyzer)Tm.TestClass).ComputeRms(data, 18995, 19005);
                tr.Value[0] = ((IAudioAnalyzer)Tm.TestClass).ComputeRms(data, 995, 1005) - tr.Value[0];
            }

            if (RightChannel)
            {
                PointD[] data = ((IAudioAnalyzer)Tm.TestClass).GetData(ChannelEnum.Right);
                tr.Value[1] = ((IAudioAnalyzer)Tm.TestClass).ComputeRms(data, 18995, 19005);
                tr.Value[1] = ((IAudioAnalyzer)Tm.TestClass).ComputeRms(data, 995, 1005) - tr.Value[1];
            }

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
                if ((tr.Value[0] < MinimumLevelDbc) || (tr.Value[0] > MaximumLevelDbc))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
                if ((tr.Value[1] < MinimumLevelDbc) || (tr.Value[1] > MaximumLevelDbc))
                    passRight = false;
            }
            else
                tr.StringValue[1] = "SKIP";


            if (LeftChannel && RightChannel)
                tr.Pass = passLeft && passRight;
            else if (LeftChannel)
                tr.Pass = passLeft;
            else if (RightChannel)
                tr.Pass = passRight;

            return;
        }

        public override string GetTestLimitsString()
        {
            return string.Format("{0:N1}...{1:N1} dBc", MinimumLevelDbc, MaximumLevelDbc);
        }

        public override string GetTestDescription()
        {
            return "Performs IMD ITU Test. This test generates dual tones at 19 and 20 KHz, each with a level 6 dB below the specified amplitude. " +
                   "The combined amplitude of the tones will be the value specified in the test parameters. The resultant mixing product at 1 KHz " +
                   "is measured, and the amplitude below the specified output amplitude is computed. If that amplitude relative to the output amplitude " +
                   "is within the specified window limits, then 'pass = true' is returned. ";
        }

        public override bool IsRunnable()
        {
            if ((Tm.TestClass is IAudioAnalyzer) &&
                 (Tm.TestClass is IProgrammableLoad))
            {
                return true;
            }

            return false;
        }

    }
}
