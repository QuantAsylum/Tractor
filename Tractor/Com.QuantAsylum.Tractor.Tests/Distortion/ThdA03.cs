using Com.QuantAsylum;
using Com.QuantAsylum.Tractor.TestManagers;
using Com.QuantAsylum.Tractor.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tractor.Com.QuantAsylum.Tractor.Tests.THDs
{
    [Serializable]
    public class ThdA03 : AudioTestBase
    {
        [ObjectEditorAttribute(Index = 200, DisplayText = "Test Frequency (Hz)", MinValue = 10, MaxValue = 20000)]
        public float Freq = 1000;

        [ObjectEditorAttribute(Index = 210, DisplayText = "Analyzer Output Level (dBV)", MinValue = -100, MaxValue = 6)]
        public float OutputLevel = -30;

        [ObjectEditorAttribute(Index = 230, DisplayText = "Minimum THD to Pass (dB)", MinValue = -100, MaxValue = 10)]
        public float MinimumOKTHD = -110;

        [ObjectEditorAttribute(Index = 240, DisplayText = "Maximum THD to Pass (dB)", MinValue = -100, MaxValue = 10, MustBeGreaterThanIndex = 230)]
        public float MaximumOKTHD = -100;

        [ObjectEditorAttribute(Index = 250, DisplayText = "Load Impedance (ohms)", ValidInts = new int[] { 8, 4 })]
        public int ProgrammableLoadImpedance = 8;

        [ObjectEditorAttribute(Index = 260, DisplayText = "Analyzer Input Range", ValidInts = new int[] { 6, 26 })]
        public int InputRange = 6;

        public ThdA03() : base()
        {
            Name = this.GetType().Name;
            _TestType = TestTypeEnum.Distortion;
        }

        public override void DoTest(string title, out TestResult tr)
        {
            // Two channels of testing
            tr = new TestResult(2);

            Tm.SetToDefaults();

            ((IAudioAnalyzer)Tm.TestClass).AudioAnalyzerSetTitle(title);
            ((IAudioAnalyzer)Tm.TestClass).SetInputRange(InputRange);
            ((IAudioAnalyzer)Tm.TestClass).SetFftLength(FftSize);
            ((IProgrammableLoad)Tm.TestClass).SetImpedance(ProgrammableLoadImpedance);

            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen1(true, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).AudioGenSetGen2(false, OutputLevel, Freq);
            ((IAudioAnalyzer)Tm.TestClass).DoAcquisition();

            TestResultBitmap = ((IAudioAnalyzer)Tm.TestClass).GetBitmap();
            ((IAudioAnalyzer)Tm.TestClass).ComputeThdPct(Freq, 20000, out tr.Value[0], out tr.Value[1]);

            // Convert to db
            tr.Value[0] = 20 * (float)Math.Log10(tr.Value[0] / 100);
            tr.Value[1] = 20 * (float)Math.Log10(tr.Value[1] / 100);

            bool passLeft = true, passRight = true;

            if (LeftChannel)
            {
                tr.StringValue[0] = tr.Value[0].ToString("0.0") + " dB";
                if ((tr.Value[0] < MinimumOKTHD) || (tr.Value[0] > MaximumOKTHD))
                    passLeft = false;
            }
            else
                tr.StringValue[0] = "SKIP";

            if (RightChannel)
            {
                tr.StringValue[1] = tr.Value[1].ToString("0.0") + " dB";
                if ((tr.Value[1] < MinimumOKTHD) || (tr.Value[1] > MaximumOKTHD))
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
            return string.Format("{0:N1}...{1:N1} dB", MinimumOKTHD, MaximumOKTHD);
        }

        public override string GetTestDescription()
        {
            return "Measures THD at a given frequency and amplitude at a given load. Results must be within a given window to pass.";
        }

        internal override int HardwareMask
        {
            get
            {
                return (int)HardwareTypes.AudioAnalyzer | (int)HardwareTypes.ProgrammableLoad;
            }
        }
    }
}
