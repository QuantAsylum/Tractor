using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Com.QuantAsylum.Tractor.Tests;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using Tractor.Com.QuantAsylum.Tractor.TestManagers;
using Tractor.Com.QuantAsylum.Hardware;
using System.Drawing;
using System.IO;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    class QATestManager : TestManager
    {
        QA401Interface QA401;

        override public void ConnectToDevices()
        {
            QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA401);
            QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA450);

            if (QAConnectionManager.ConnectToDevices(out QA401))
            {
                if (QA401 == null)
                {

                }
            }
        }

        override public bool AllConnected()
        {
            if (QA401 != null && QA401.IsConnected() && QA450.IsConnected())
                return true;

            return false;
        }

        public override string GetProfileName()
        {
            return "QA401, QA450";
        }

        public override void SetInstrumentsToDefault()
        {
            QA401.SetToDefault("");
            QA450.SetToDefault();
        }

        public override void DutSetDefault()
        {
            
        }

        public override void DutSetPowerState(bool powerEnable)
        {
            QA450.SetDutPower(true);
        }

        public override bool DutGetPowerState()
        {
            return QA450.GetDutPower();
        }

        public override float DutGetCurrent()
        {
            return QA450.GetCurrent();
        }

        public override void DutSetVoltage(float voltage_V)
        {
            throw new NotImplementedException();
        }

        public override float DutGetVoltage()
        {
            throw new NotImplementedException();
        }

        public override float DutGetTemperature()
        {
            throw new NotImplementedException();
        }

        public override void LoadSetDefault()
        {
            throw new NotImplementedException();
        }

        public override int[] GetImpedances()
        {
            return new int[] { 0, 4, 8 };
        }

        public override void LoadSetImpedance(int impedance)
        {
            if (GetImpedances().Contains(impedance) == false)
                throw new ArgumentException("Impedance value is not supported");

            QA450.SetImpedance(impedance);
        }

        public override int LoadGetImpedance()
        {
            return QA450.GetImpedance();
        }

        public override float LoadGetTemperature()
        {
            throw new NotImplementedException();
        }

        public override void AudioAnalyzerSetDefaults()
        {
            QA401.SetToDefault("");
        }

        public override void AudioAnalyzerSetFftLength(uint length)
        {
            QA401.SetBufferLength(length);
        }

        public override void AudioAnalyzerSetTitle(string s)
        {
            QA401.SetTitle(s);
        }

        public override void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            QA401.SetGenerator(QuantAsylum.QA401.GenType.Gen1, isOn, ampLevel_dBV, freq_Hz);
        }

        public override void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            QA401.SetGenerator(QuantAsylum.QA401.GenType.Gen2, isOn, ampLevel_dBV, freq_Hz);
        }

        public override int[] GetInputRanges()
        {
            return new int[] { 6, 26 };
        }

        public override void SetInputRange(int attenLevel_dB)
        {
            switch (attenLevel_dB)
            {
                case 6: 
                    QA401.SetInputAtten(QuantAsylum.QA401.InputAttenState.NoAtten);
                    break;
                
                case 26:
                    QA401.SetInputAtten(QuantAsylum.QA401.InputAttenState.dB20);
                    break;
                default:
                     throw new ArgumentException("Attenuation value is not supported");
            }
        }

        public override void RunSingle()
        {
            QA401.RunSingle();
        }

        public override bool AnalyzerIsBusy()
        {
            return (QA401.GetAcquisitionState() == QuantAsylum.QA401.AcquisitionState.Busy);
        }

        public override PointD[] GetData(ChannelEnum channel)
        {
            QuantAsylum.QA401.PointD[] dataIn;
            PointD[] dataOut;

            switch (channel)
            {
                case ChannelEnum.Left:
                    dataIn = QA401.GetData(QuantAsylum.QA401.ChannelType.LeftIn);
                    break;
                case ChannelEnum.Right:
                    dataIn = QA401.GetData(QuantAsylum.QA401.ChannelType.RightIn);
                    break;
                default:
                    throw new ArgumentException("Invalid arguement in GetData()");
            }

            dataOut = new PointD[dataIn.Length];

            return dataOut = MarshallToPointD(dataIn);
        }

        public override Bitmap GetBitmap()
        {
            byte[] imgArray = QA401.GetBitmapBytes();

            MemoryStream ms = new MemoryStream(imgArray);
            return (Bitmap)Image.FromStream(ms);
        }

        public override double ComputeRms(PointD[] data, float startFreq, float stopFreq)
        {
            return QA401.ComputePowerDB(MarshallToQAPointD(data), startFreq, stopFreq);
        }

        public override double ComputeThdPct(PointD[] data, float fundamental, float stopFreq)
        {
            return QA401.ComputeTHDPct(MarshallToQAPointD(data), fundamental, stopFreq);
        }

        private QuantAsylum.QA401.PointD[] MarshallToQAPointD(PointD[] dataIn)
        {
            QuantAsylum.QA401.PointD[] dataOut = new QuantAsylum.QA401.PointD[dataIn.Length];

            for (int i=0; i<dataOut.Length; i++)
            {
                dataOut[i] = new QuantAsylum.QA401.PointD { X = dataIn[i].X, Y = dataIn[i].Y };
            }

            return dataOut;
        }

        private PointD[] MarshallToPointD(QuantAsylum.QA401.PointD[] dataIn)
        {
            PointD[] dataOut = new PointD[dataIn.Length];

            for (int i = 0; i < dataOut.Length; i++)
            {
                dataOut[i] = new PointD { X = dataIn[i].X, Y = dataIn[i].Y };
            }

            return dataOut;
        }
    }
}
