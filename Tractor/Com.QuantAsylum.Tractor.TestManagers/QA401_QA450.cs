using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    class QA401_QA450 : IComposite, IAudioAnalyzer, IProgrammableLoad, ICurrentMeter, IPowerSupply
    {
        QA401 Qa401;
        QA450 Qa450;

        public QA401_QA450()
        {
            Qa401 = new QA401();
            Qa450 = new QA450();
        }

        //
        // IProgrammableLoad
        //

        public int GetImpedance()
        {
            return Qa450.GetImpedance();
        }

        public void SetImpedance(int impedance)
        {
            // Settling time is signficant and checking impedance is cheap, so 
            // always see if we need to change or not
            if (GetImpedance() != impedance)
            {
                Qa450.SetImpedance(impedance);
                Thread.Sleep(750);
            }
        }

        public int[] GetSupportedImpedances()
        {
            return Qa450.GetSupportedImpedances();
        }

        public float GetLoadTemperature()
        {
            return Qa450.GetLoadTemperature();
        }

        //
        // IPowerSupply
        //


        public void SetSupplyVoltage(float volts)
        {
            throw new NotImplementedException();
        }

        public float GetSupplyVoltage()
        {
            throw new NotImplementedException();
        }

        public void SetSupplyState(bool powerEnable)
        {
            Qa450.SetSupplyState(powerEnable);
        }

        public bool GetSupplyState()
        {
            return Qa450.GetSupplyState();
        }

        //
        // ICurrentMeter
        //
        public float GetDutCurrent(int averages = 1)
        {
            return Qa450.GetDutCurrent(averages);
        }

        //
        // IAudioAnalyzer
        //

        public int[] GetInputRanges()
        {
            return Qa401.GetInputRanges();
        }

        public void SetInputRange(int attenLevel_dB)
        {
            Qa401.SetInputRange(attenLevel_dB);
        }

        public void RunSingle()
        {
            Qa401.RunSingle();
        }

        public void SetFftLength(uint length)
        {
            Qa401.SetFftLength(length);
        }

        public bool AnalyzerIsBusy()
        {
            return Qa401.AnalyzerIsBusy();
        }

        public void AudioAnalyzerSetTitle(string s)
        {
            Qa401.AudioAnalyzerSetTitle(s);
        }

        public void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            Qa401.AudioGenSetGen1(isOn, ampLevel_dBV, freq_Hz);
        }

        public void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            Qa401.AudioGenSetGen2(isOn, ampLevel_dBV, freq_Hz);
        }

        public double ComputeRms(PointD[] data, float startFreq, float stopFreq)
        {
            return Qa401.ComputeRms(data, startFreq, stopFreq);
        }

        public double ComputeThdPct(PointD[] data, float fundamental, float stopFreq)
        {
            return Qa401.ComputeRms(data, fundamental, stopFreq);
        }

        public Bitmap GetBitmap()
        {
            return Qa401.GetBitmap();
        }

        public PointD[] GetData(ChannelEnum channel)
        {
            return Qa401.GetData(channel);
        }

        public void SetToDefaults()
        {
            Qa401.SetToDefaults();
            Qa450.SetToDefaults();
        }

        public bool IsConnected()
        {
            if (Qa401.IsConnected() && Qa450.IsConnected())
                return true;

            return false;
        }

        public bool ConnectToDevices()
        {
            bool r1 = false, r2 = false;

            if (Qa401.IsConnected() == false)
                r1 = Qa401.ConnectToDevice();

            if (Qa450.IsConnected() == false)
                r2 = Qa450.ConnectToDevice();

            if (r1 && r2)
                return true;

            return false;
        }
    }
}
