using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    interface IAudioAnalyzer
    {
        void SetFftLength(uint length);
        void AudioAnalyzerSetTitle(string s);

        void SetMuting(bool muteLeft, bool muteRight);

        void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz);
        void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz);

        int[] GetInputRanges();
        void SetInputRange(int attenLevel_dB);

        void SetOffsets(double inputOffset, double outputOffset);

        void DoAcquisition();
        void DoFrAquisition(float ampLevl_dBV, double windowSec, int smoothingDenominator);
        void DoAcquisitionAsync();
        bool AnalyzerIsBusy();

        void TestMask(string maskFile, bool testL, bool testR, bool testMath, out bool passLeft, out bool passRight, out bool passMath);

        void SetYLimits(int yMax, int yMin);

        void AddMathToDisplay();

        void ComputeRms(double startFreq, double stopFreq, out double RmsDbvL, out double RmsDbvR);
        void ComputePeakDb(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR);
        void ComputeThdPct(double fundamental, double stopFreq, out double ThdPctL, out double ThdPctR);
        void ComputeThdnPct(double fundamental, double startFreq, double stopFreq, out double thdPctL, out double thdPctR);

        bool LRVerifyPhase(int bufferOffset);

        void AuditionStart(string fileName, double volume, bool repeat);
        void AuditionSetVolume(double volume);
        void AuditionStop();

        PointD[] GetData(ChannelEnum channel);
        Bitmap GetBitmap();
    }
}
