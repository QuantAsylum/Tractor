using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain at a given impedance level
    /// </summary>
    [Serializable]
    public class Gain02 : TestBase, ITest
    {           
        public float Freq = 1000;
        public float OutputLevel = -30;

        public float MinimumOKGain = -10.5f;
        public float MaximumOKGain = -9.5f;

        public int OutputImpedance = 8;
        public int InputRange = 6;

        private int[] AllowedInputRanges;
        private int[] AllowedLoadImpedances;

        public Gain02() : base()
        {
            TestType = TestTypeEnum.LevelGain;
        }

        public override void DoTest(out float[] value, out bool pass)
        {
            value = new float[2] { float.NaN, float.NaN };
            pass = false;

            if (Tm == null)
                return;

            AllowedInputRanges = Tm.GetInputRanges();
            AllowedLoadImpedances = Tm.GetImpedances();

            Tm.LoadSetImpedance(OutputImpedance);

            Tm.AudioGenSetGen1(true, OutputLevel, Freq);
            Tm.AudioGenSetGen2(false, OutputLevel, Freq);
            Tm.RunSingle();

            while (Tm.AnalyzerIsBusy())
            {
                float current = Tm.DutGetCurrent();
                Debug.WriteLine("Current: " + current);
            }

            TestResultBitmap = Tm.GetBitmap();

            // Compute the total RMS around the freq of interest
            value[0] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Left), Freq * 0.98f, Freq * 1.02f );
            value[1] = (float)Tm.ComputeRms(Tm.GetData(ChannelEnum.Right), Freq * 0.98f, Freq * 1.02f);

            if (LeftChannel && value[0] > MinimumOKGain && value[0] < MaximumOKGain && RightChannel && value[1] > MinimumOKGain && value[1] < MaximumOKGain)
                pass = true;
            else if (!LeftChannel && RightChannel && value[1] > MinimumOKGain && value[1] < MaximumOKGain)
                pass = true;
            else if (!RightChannel && LeftChannel && value[0] > MinimumOKGain && value[0] < MaximumOKGain)
                pass = true;

            return;
        }

        public override bool CheckValues(out string s)
        {
            s = "";
            if (AllowedLoadImpedances.Contains(OutputImpedance) == false)
            {
                s = "Output impedance must be: " + string.Join(" ", AllowedLoadImpedances);
                return false;
            }

            if (AllowedInputRanges.Contains(InputRange) == false)
            {
                s = "Input range not supported. Must be: " + string.Join(" ", AllowedInputRanges);
                return false;
            }

            return true;
        }

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude at a specified impedance level. Results must be within a given window to 'pass'.";
        }
    }
}
