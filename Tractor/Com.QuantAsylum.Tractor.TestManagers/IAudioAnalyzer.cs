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

        void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz);
        void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz);

        int[] GetInputRanges();
        void SetInputRange(int attenLevel_dB);

        void DoAcquisition();
        void DoAcquisitionAsync();
        bool AnalyzerIsBusy();

        void ComputeRms(double startFreq, double stopFreq, out double RmsDbvL, out double RmsDbvR);
        void ComputePeak(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR);
        void ComputeThdPct(double fundamental, double stopFreq, out double ThdPctL, out double ThdPctR);

        void AuditionStart(string fileName, double volume, bool repeat);
        void AuditionSetVolume(double volume);
        void AuditionStop();

        PointD[] GetData(ChannelEnum channel);
        Bitmap GetBitmap();
    }
}
