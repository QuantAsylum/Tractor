using Com.QuantAsylum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Com.QuantAsylum.Tractor.TestManagers;
using Tractor;
using static Com.QuantAsylum.QA401;
using System.Diagnostics;
using System.Threading;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    class QA401 : IInstrument, IAudioAnalyzer
    {
        QA401Interface Qa401;

        string MutexName = "Global\\QA401MutexCheck";

        public bool IsRunning()
        {
            return IsAppAlreadyRunning();
        }

        public void LaunchApplication()
        {
            LaunchApp("QAAnalyzer.exe");
        }

        public void SetToDefaults()
        {
            Qa401.SetToDefault("");
        }

        public bool ConnectToDevice()
        {
            Qa401 = null;

            if (IsAppAlreadyRunning() == false)
            {
                LaunchApplication();
            }

            try
            {
                // QA401 first
                TcpChannel tcpChannel = (TcpChannel)Helper.GetChannel(4401, false);
                //TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel, false);

                Type requiredType = typeof(QA401Interface);

                Qa401 = (QA401Interface)Activator.GetObject(requiredType, "tcp://localhost:9401/QuantAsylumQA401Server");

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, "Exception in ConnectToDevice() " + ex.Message);
            }

            return false;
        }

        public bool IsConnected()
        {
            if (Qa401 == null)
                return false;

            try
            {
                return Qa401.IsConnected();
            }
            catch
            {

            }

            return false;
        }

        public void SetFftLength(uint length)
        {
            Qa401.SetBufferLength(length);
        }

        public void AudioAnalyzerSetTitle(string s)
        {
            Qa401.SetTitle(s);
        }


        public void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            Qa401.SetGenerator(QuantAsylum.QA401.GenType.Gen1, isOn, ampLevel_dBV, freq_Hz);
        }

        public void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            Qa401.SetGenerator(QuantAsylum.QA401.GenType.Gen2, isOn, ampLevel_dBV, freq_Hz);
        }

        public int[] GetInputRanges()
        {
            return new int[] { 6, 26 };
        }

        public void SetInputRange(int attenLevel_dB)
        {
            switch (attenLevel_dB)
            {
                case 6:
                    Qa401.SetInputAtten(InputAttenState.NoAtten);
                    break;

                case 26:
                    Qa401.SetInputAtten(InputAttenState.dB20);
                    break;
                default:
                    throw new ArgumentException("Attenuation value is not supported");
            }
        }

        public void RunSingle()
        {
            Qa401.RunSingle();
        }

        public bool AnalyzerIsBusy()
        {
            return (Qa401.GetAcquisitionState() == AcquisitionState.Busy);
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

        public PointD[] GetData(ChannelEnum channel)
        {
            QuantAsylum.QA401.PointD[] dataIn;
            PointD[] dataOut;

            switch (channel)
            {
                case ChannelEnum.Left:
                    dataIn = Qa401.GetData(ChannelType.LeftIn);
                    break;
                case ChannelEnum.Right:
                    dataIn = Qa401.GetData(ChannelType.RightIn);
                    break;
                default:
                    throw new ArgumentException("Invalid arguement in GetData()");
            }

            dataOut = new PointD[dataIn.Length];

            return dataOut = MarshallToPointD(dataIn);
        }

        public Bitmap GetBitmap()
        {
            byte[] imgArray = Qa401.GetBitmapBytes();

            MemoryStream ms = new MemoryStream(imgArray);
            return (Bitmap)Image.FromStream(ms);
        }

        public double ComputeRms(PointD[] data, float startFreq, float stopFreq)
        {
            return Qa401.ComputePowerDB(MarshallToQAPointD(data), startFreq, stopFreq);
        }

        public double ComputeThdPct(PointD[] data, float fundamental, float stopFreq)
        {
            return Qa401.ComputeTHDPct(MarshallToQAPointD(data), fundamental, stopFreq);
        }

        private QuantAsylum.QA401.PointD[] MarshallToQAPointD(PointD[] dataIn)
        {
            QuantAsylum.QA401.PointD[] dataOut = new QuantAsylum.QA401.PointD[dataIn.Length];

            for (int i = 0; i < dataOut.Length; i++)
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

        /************************************************************/
        /********************HELPER FUNCTIONS************************/
        /************************************************************/



        System.Threading.Mutex AlreadyRunningMutex;
        bool IsAppAlreadyRunning()
        {
            bool createdNew;

            // Try to create a named mutex. All we really care about is whether or not we were
            // able to create it. If another app was running, then the mutex would already exist
            // and we would fail to create it. 
            AlreadyRunningMutex = new System.Threading.Mutex(true, MutexName, out createdNew);

            // If we were able to create a new mutex, then we also own it given the 
            // 'true' parameter in the c'tor
            if (createdNew)
            {
                // Release ownership
                AlreadyRunningMutex.ReleaseMutex();
                AlreadyRunningMutex.Dispose();
                AlreadyRunningMutex = null;
                return false;
            }

            // Here we weren't able to create a new mutex. The hardware is in use
            return true;
        }

        /// <summary>
        /// Launches the named application. Returns true if the app was successfully launched
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool LaunchApp(string name)
        {
            if (IsAppAlreadyRunning())
                return true;

            List<string> InstallDirSearchPaths = new List<string>();

            InstallDirSearchPaths.Add(@"c:\program files\quantasylum\qa401\");
            InstallDirSearchPaths.Add(@"c:\program files (x86)\quantasylum\qa401\");
            InstallDirSearchPaths.Add(@"d:\program files\quantasylum\qa401\");
            InstallDirSearchPaths.Add(@"d:\program files (x86)\quantasylum\qa401\");
            InstallDirSearchPaths.Add(@"e:\program files\quantasylum\qa401\");
            InstallDirSearchPaths.Add(@"e:\program files (x86)\quantasylum\qa401\");

            foreach (string dir in InstallDirSearchPaths)
            {
                string pathToExe = dir + name;
                if (File.Exists(pathToExe))
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.WorkingDirectory = Path.GetDirectoryName(pathToExe);
                        psi.FileName = pathToExe;
                        System.Diagnostics.Process.Start(psi);

                        // Wait up to 5 seconds for the app to get running
                        for (int i=0; i<5; i++)
                        {
                            if (IsAppAlreadyRunning())
                                return true;

                            Thread.Sleep(1000);
                        }

                        return true;
                    }
                    catch
                    {

                    }
                }
            }

            return false;
        }

    }
}
