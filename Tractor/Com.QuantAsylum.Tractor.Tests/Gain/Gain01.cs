using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain
    /// </summary>
    [Serializable]
    public class Gain01 : TestBase
    {           
        public float TestFrequency = 1000;
        public float AnalyzerOutputLevel = -30;

        public float ExternalAnalyzerInputGain = 0;

        public float MinimumPassGain = -10.5f;
        public float MaximumPassGain = -9.5f;

        public int AnalyzerInputRange = 6;

        public Gain01() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels
            tr = new TestResult(2);

            ((IComposite)Tm).SetToDefaults();
            ((IAudioAnalyzer)Tm).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm).SetInputRange(AnalyzerInputRange);

            ((IAudioAnalyzer)Tm).AudioGenSetGen1(true, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm).AudioGenSetGen2(false, AnalyzerOutputLevel, TestFrequency);
            ((IAudioAnalyzer)Tm).RunSingle();

            while (((IAudioAnalyzer)Tm).AnalyzerIsBusy())
            {
                Thread.Sleep(50);
            }

            TestResultBitmap = ((IAudioAnalyzer)Tm).GetBitmap();

            // Compute the total RMS around the freq of interest
            tr.Value[0] = (float)((IAudioAnalyzer)Tm).ComputeRms(((IAudioAnalyzer)Tm).GetData(ChannelEnum.Left), TestFrequency * 0.98f, TestFrequency * 1.02f ) + ExternalAnalyzerInputGain - AnalyzerOutputLevel;
            tr.Value[1] = (float)((IAudioAnalyzer)Tm).ComputeRms(((IAudioAnalyzer)Tm).GetData(ChannelEnum.Right), TestFrequency * 0.98f, TestFrequency * 1.02f) + ExternalAnalyzerInputGain - AnalyzerOutputLevel;

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.00") + " dB";
                if ((tr.Value[0] < MinimumPassGain) || (tr.Value[0] > MaximumPassGain))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.00") + " dB";
                if ((tr.Value[1] < MinimumPassGain) || (tr.Value[1] > MaximumPassGain))
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

        public override string GetTestLimits()
        {
            return string.Format("{0:N1}...{1:N1} dB", MinimumPassGain, MaximumPassGain);
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude. Results must be within a given window to 'pass'.";
        }

        public override bool IsRunnable()
        {
            if (Tm.TestClass is IAudioAnalyzer)
            {
                return true;
            }

            return false;
        }
    }
}
