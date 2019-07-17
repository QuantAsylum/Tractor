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
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace Com.QuantAsylum.Tractor.TestManagers
{
    class QA401 : IInstrument, IAudioAnalyzer
    {
        QA401Interface Qa401;
        TcpChannel TcpChannelInst;

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

        public bool ConnectToDevice(out string result)
        {
            result = "";

            if (Qa401 != null)
                return true;

            if (IsAppAlreadyRunning() == false)
            {
                LaunchApplication();
            }

            try
            {
                TcpChannelInst = new TcpChannel();
                ChannelServices.RegisterChannel(TcpChannelInst, false);

                Type requiredType = typeof(QA401Interface);

                Qa401 = (QA401Interface)Activator.GetObject(requiredType, "tcp://localhost:9401/QuantAsylumQA401Server");

                if (Qa401.GetVersion() < 1.78)
                {
                    System.Windows.Forms.MessageBox.Show("You are running an older version of the QA Analyzer application that is required by this version of Tractor. Please upgrade to a more recent version.", "Version Notice");
                }

                return true;
            }
            catch (Exception ex)
            {
                result = ex.Message;
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

        public void CloseConnection()
        {
            ChannelServices.UnregisterChannel(TcpChannelInst);
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

        public void DoFrAquisition(float ampLevel_Dbv)
        {
            Qa401.RemotingRunSingleFrExpoChirp(ampLevel_Dbv);

            while (AnalyzerIsBusy())
            {
                Thread.Sleep(50);
            }
        }

       

        public void DoAcquisition()
        {
            Qa401.RunSingle();

            while (AnalyzerIsBusy())
            {
                Thread.Sleep(50);
            }
        }

        public void DoAcquisitionAsync()
        {
            Qa401.RunSingle();
        }

        public bool AnalyzerIsBusy()
        {
            return (Qa401.GetAcquisitionState() == AcquisitionState.Busy);
        }

        public void TestMask(string maskFile, out bool passLeft, out bool passRight)
        {
            Qa401.ApplyMask(maskFile, out passLeft, out passRight);
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

        public void ComputeRms(double startFreq, double stopFreq, out double rmsDbvL, out double rmsDbvR)
        {
            Qa401.ComputeRms(startFreq, stopFreq, out rmsDbvL, out rmsDbvR);
        }

        public void ComputePeak(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR)
        {
            Qa401.GetPeak(startFreq, stopFreq, out PeakDbvL, out PeakDbvR);
        }

        public void ComputeThdPct(double fundamental, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdPct(fundamental, stopFreq, out thdPctL, out thdPctR);
        }

        public void ComputeThdnPct(double fundamental, double startFreq, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdNPct(fundamental, startFreq, stopFreq, out thdPctL, out thdPctR);
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




        bool IsAppAlreadyRunning()
        {
            System.Threading.Mutex mutex;

            mutex = new System.Threading.Mutex(false, MutexName);

            if (mutex.WaitOne(1))
            {
                // Here we were able to grab ownership. The app must not be running
                mutex.ReleaseMutex();
                mutex.Dispose();
                return false;
          
            }

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

                        // Wait up to 20 seconds for the app to get running
                        for (int i=0; i<20; i++)
                        {
                            if (IsAppAlreadyRunning())
                            {
                                Thread.Sleep(1000);
                                return true;
                            }

                            Thread.Sleep(1000);
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(LogType.Error, "Exception inside LaunchApp(): " + ex.Message);
                    }
                }
            }

            return false;
        }

       

        /// <summary>
        /// This class is only needed IF we want to connect to multiple devices
        /// </summary>
        public class Helper
        {
            public static IChannel GetChannel(int tcpPort, bool isSecure)
            {
                BinaryServerFormatterSinkProvider serverProv =
                    new BinaryServerFormatterSinkProvider();
                serverProv.TypeFilterLevel = TypeFilterLevel.Full;
                IDictionary propBag = new Hashtable();
                propBag["port"] = tcpPort;
                propBag["typeFilterLevel"] = TypeFilterLevel.Full;
                propBag["name"] = Guid.NewGuid().ToString();
                if (isSecure)
                {
                    propBag["secure"] = isSecure;
                    propBag["impersonate"] = false;
                }
                return new TcpChannel(
                    propBag, null, serverProv);
            }
        }

    }
}
