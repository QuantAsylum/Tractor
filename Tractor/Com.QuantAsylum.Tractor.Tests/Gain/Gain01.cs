﻿using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.Tests.GainTests
{
    /// <summary>
    /// This test will check the gain
    /// </summary>
    [Serializable]
    public class Gain01 : TestBase, ITest
    {           
        public float Freq = 1000;
        public float OutputLevel = -30;

        public float MinimumOKGain = -10.5f;
        public float MaximumOKGain = -9.5f;

        public Gain01() : base()
        {
            TestType = TestTypeEnum.LevelGain;
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

        public override string GetTestDescription()
        {
            return "Measures the gain at a specified frequency and amplitude. Results must be within a given window to 'pass'.";
        }
    }
}
