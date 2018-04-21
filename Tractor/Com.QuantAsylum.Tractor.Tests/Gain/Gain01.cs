﻿using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (TestManager.QA401 == null)
                return;

            TestManager.QA401.SetGenerator(QA401.GenType.Gen1, true, OutputLevel, Freq);
            TestManager.QA401.SetGenerator(QA401.GenType.Gen2, false, OutputLevel, Freq);
            TestManager.QA401.RunSingle();

            while (TestManager.QA401.GetAcquisitionState() == QA401.AcquisitionState.Busy)
            {

            }

            TestResultBitmap = CaptureBitmap(TestManager.QA401.GetBitmapBytes());

            // Compute the total RMS around the freq of interest
            value[0] = (float)TestManager.QA401.ComputePowerDB(TestManager.QA401.GetData(QA401.ChannelType.LeftIn), Freq * 0.98, Freq * 1.02 );
            value[1] = (float)TestManager.QA401.ComputePowerDB(TestManager.QA401.GetData(QA401.ChannelType.RightIn), Freq * 0.98, Freq * 1.02);

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
