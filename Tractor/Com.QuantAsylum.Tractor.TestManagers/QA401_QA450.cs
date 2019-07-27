using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    class QA401_QA450 : /* IComposite, */IInstrument, IAudioAnalyzer, IProgrammableLoad, ICurrentMeter, IPowerSupply
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

        public void DoAcquisition()
        {
            Qa401.DoAcquisition();
        }

        public void DoFrAquisition(float ampLevl_dBV)
        {
            throw new NotImplementedException();
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

        public void ComputeRms(double startFreq, double stopFreq, out double rmsDbvL, out double rmsDbvR)
        {
            Qa401.ComputeRms(startFreq, stopFreq, out rmsDbvL, out rmsDbvR);
        }

        public void ComputePeak(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR)
        {
            Qa401.ComputePeak(startFreq, stopFreq, out PeakDbvL, out PeakDbvR);
        }

        public void ComputeThdPct(double fundamental, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdPct(fundamental, stopFreq, out thdPctL, out thdPctR);
        }

        public void ComputeThdnPct(double fundamental, double startFreq, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdnPct(fundamental, startFreq, stopFreq, out thdPctL, out thdPctR);
        }

        public void TestMask(string maskFile, out bool passLeft, out bool passRight)
        {
            Qa401.TestMask(maskFile, out passLeft, out passRight);
        }

        public void ComputeThdnPct(double fundamental, double stopFreq, out double ThdPctL, out double ThdPctR)
        {
            throw new NotImplementedException();
        }

       

        public void AuditionStart(string fileName, double volume, bool repeat)
        {
            Qa401.AuditionStart(fileName, volume, repeat);
        }

        public void AuditionSetVolume(double volume)
        {
            Qa401.AuditionSetVolume(volume);
        }

        public void AuditionStop()
        {
            Qa401.AuditionStop();
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

        public bool IsRunning()
        {
            return Qa401.IsRunning();
        }

        public void LaunchApplication()
        {
            Qa401.LaunchApplication();
        }

        public bool ConnectToDevice(out string result)
        {
            result = "";

            bool qa401Ok = false, qa450Ok = false;

            if (Qa401.IsConnected() == false)
            {
                qa401Ok = Qa401.ConnectToDevice(out result);
            }

            if (qa401Ok && Qa401.GetVersion() >= Constants.RequiredQa401Version)
            {
                qa401Ok = true;
            }
            else
            {
                if (qa401Ok == false)
                    result += "Unable to connect to the QA401.";
                else
                    result += "The QA401 application version was not current. Must be >= " + Constants.RequiredQa401Version.ToString("0.00");
            }

            qa450Ok = Qa450.IsConnected();
            if (qa450Ok && Qa450.GetVersion() >= Constants.RequiredQa450Version)
            {
                qa450Ok = true;
            }
            else
            {
                if (qa450Ok == false)
                    result += "Unable to connect to the QA450.";
                else
                    result += "The QA450 application version was not current. Must be >= " + Constants.RequiredQa450Version.ToString("0.00");
            }

            if (qa401Ok && qa450Ok)
            {
                return true;
            }

            result += "The testing will now stop.";
            return false;
        }

        public void CloseConnection()
        {
            if (Qa401.IsConnected())
            {
                Qa401.CloseConnection();
            }

            if (Qa450.IsConnected())
            {
                Qa450.CloseConnection();
            }
        }

        public void DoAcquisitionAsync()
        {
            Qa401.DoAcquisitionAsync();
            //throw new NotImplementedException();
        }

        
    }
}
