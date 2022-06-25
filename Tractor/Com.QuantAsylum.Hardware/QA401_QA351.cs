using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor;
using Tractor.Com.QuantAsylum.Hardware;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;

namespace Com.QuantAsylum.Hardware
{
    class QA401_QA351 : /* IComposite, */IInstrument, IAudioAnalyzer, IVoltMeter
    {
        QA401 Qa401;
        QA351 Qa351;

        public QA401_QA351()
        {
            Qa401 = new QA401();
            Qa351 = new QA351();
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

        public void SetOffsets(double inputOffset, double outputOffset)
        {
            Qa401.SetOffsets(inputOffset, outputOffset);
        }

        public void DoAcquisition()
        {
            Qa401.DoAcquisition();
        }

        public void DoFrAquisition(float ampLevl_dBV, double windowSec, int smoothingDenominator)
        {
            Qa401.DoFrAquisition(ampLevl_dBV, windowSec, smoothingDenominator);
        }

        public void AddMathToDisplay()
        {
            throw new  NotImplementedException();
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

        public void SetMuting(bool muteLeft, bool muteRight)
        {
            Qa401.SetMuting(muteLeft, muteRight);
        }

        public void ComputeRms(double startFreq, double stopFreq, out double rmsDbvL, out double rmsDbvR)
        {
            Qa401.ComputeRms(startFreq, stopFreq, out rmsDbvL, out rmsDbvR);
        }

        public void ComputePeakDb(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR)
        {
            Qa401.ComputePeakDb(startFreq, stopFreq, out PeakDbvL, out PeakDbvR);
        }

        public void ComputeThdPct(double fundamental, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdPct(fundamental, stopFreq, out thdPctL, out thdPctR);
        }

        public void ComputeThdnPct(double fundamental, double startFreq, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdnPct(fundamental, startFreq, stopFreq, out thdPctL, out thdPctR);
        }

        public void TestMask(string maskFile, bool testL, bool testR, bool testMath, out bool passLeft, out bool passRight, out bool passMath)
        {
            Qa401.TestMask(maskFile, testL, testR, testMath, out passLeft, out passRight, out passMath);
        }

        public void SetYLimits(int yMax, int yMin)
        {
            Qa401.SetYLimits(yMax, yMin);
        }

        public bool LRVerifyPhase(int bufferOffset)
        {
            return Qa401.LRVerifyPhase(bufferOffset);
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

        public void SetToDefaults()
        {
            Qa401.SetToDefaults();
            Qa351.SetToDefaults();
        }

        public bool IsConnected()
        {
            if (Qa401.IsConnected() && Qa351.IsConnected())
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

            bool qa401Ok = false, qa351Ok = false;

            if (Qa401.IsConnected() == true)
            {
                qa401Ok = true;
            }
            else
            {
                qa401Ok = Qa401.ConnectToDevice(out result);
            }

            if (qa401Ok)
            {
                if (Qa401.GetVersion() >= Constants.RequiredQa401Version)
                {
                    // Version OK
                }
                else
                {
                    qa401Ok = false;
                    result += "The QA401 application version was not current. Must be >= " + Constants.RequiredQa401Version.ToString("0.00") + ". ";
                }
            }
            else
            {
                result += "Unable to connect to the QA401. ";
            }

            qa351Ok = Qa351.IsConnected();

            if (qa351Ok)
            {
                if (Qa351.GetVersion() >= Constants.RequiredQa351Version)
                {
                    // Version OK
                }
                else
                {
                    qa351Ok = false;
                    result += "The QA351 application version was not current. Must be >= " + Constants.RequiredQa351Version.ToString("0.000") + ". ";
                }
            }
            else
            {
                qa351Ok = false;
                result += "Unable to connect to the QA351. ";
            }

            if (qa401Ok && qa351Ok)
            {
                return true;
            }

            result += "The testing will now stop. ";
            return false;
        }

        public void CloseConnection()
        {
            if (Qa401.IsConnected())
            {
                Qa401.CloseConnection();
            }

            if (Qa351.IsConnected())
            {
                Qa351.CloseConnection();
            }
        }

        public void DoAcquisitionAsync()
        {
            Qa401.DoAcquisitionAsync();
            //throw new NotImplementedException();
        }

        public float GetVoltage(int channel)
        {
            return Qa351.GetVoltage(channel);
        }
    }
}
