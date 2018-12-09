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

        void RunSingle();
        bool AnalyzerIsBusy();

        double ComputeRms(PointD[] data, float startFreq, float stopFreq);
        double ComputeThdPct(PointD[] data, float fundamental, float stopFreq);

        PointD[] GetData(ChannelEnum channel);
        Bitmap GetBitmap();
    }
}
