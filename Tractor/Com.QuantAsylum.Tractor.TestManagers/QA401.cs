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

        public double GetVersion()
        {
            return Qa401.GetVersion();
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
                //TcpChannelInst = new TcpChannel();
                TcpChannelInst = (TcpChannel)Helper.GetChannel();

                ChannelServices.RegisterChannel(TcpChannelInst, false);

                Type requiredType = typeof(QA401Interface);

                Qa401 = (QA401Interface)Activator.GetObject(requiredType, "tcp://localhost:9401/QuantAsylumQA401Server");
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
            if (length < 2048 || length > 65536 || (length & (length - (uint)1)) != 0 )
                throw new Exception("Invalid FFT size set in QA401.cs SetFftLength. Length was " + length);

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

        public void SetMuting(bool muteLeft, bool muteRight)
        {
            if (muteLeft == true && muteRight == true)
                throw new Exception("Both channels cannot be muted in Qa401.cs SetMuting()");

            if (muteLeft)
            {
                Qa401.SetMuting(Muting.MuteLeft);
            }
            else if (muteRight)
            {
                Qa401.SetMuting(Muting.MuteRight);
            }
            else
            {
                Qa401.SetMuting(Muting.MuteNone);
            }
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

        public void SetOffsets(double inputOffset, double outputOffset)
        {
            Qa401.SetOffsets(inputOffset, outputOffset);
        }

        public void DoFrAquisition(float ampLevel_Dbv, double windowSec, int smoothingDenominator)
        {
            Qa401.RemotingRunSingleFrExpoChirp(ampLevel_Dbv, windowSec, smoothingDenominator);

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

        public void TestMask(string maskFile, bool testL, bool testR, bool testMath, out bool passLeft, out bool passRight, out bool passMath)
        {
            Qa401.TestMask(maskFile, testL, testR, testMath, out passLeft, out passRight, out passMath);
        }

        public void SetYLimits(int yMax, int yMin)
        {
            Qa401.SetYLimits(yMax, yMin);
        }

        public void AddMathToDisplay()
        {
            Qa401.AddMathToDisplay();
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

        //public PointD[] GetData(ChannelEnum channel)
        //{
        //    QuantAsylum.QA401.PointD[] dataIn;
        //    PointD[] dataOut;

        //    switch (channel)
        //    {
        //        case ChannelEnum.Left:
        //            dataIn = Qa401.GetData(ChannelType.LeftIn);
        //            break;
        //        case ChannelEnum.Right:
        //            dataIn = Qa401.GetData(ChannelType.RightIn);
        //            break;
        //        default:
        //            throw new ArgumentException("Invalid arguement in GetData()");
        //    }

        //    dataOut = new PointD[dataIn.Length];

        //    return dataOut = MarshallToPointD(dataIn);
        //}

        public Bitmap GetBitmap()
        {
            byte[] imgArray = Qa401.GetBitmapBytes();

            MemoryStream ms = new MemoryStream(imgArray);
            return (Bitmap)Image.FromStream(ms);
        }

        //public double ComputeRms(PointD[] data, float startFreq, float stopFreq)
        //{
        //    return Qa401.ComputePowerDb(MarshallToQAPointD(data), startFreq, stopFreq);
        //}

        public void ComputeRms(double startFreq, double stopFreq, out double rmsDbL, out double rmsDbR)
        {
            Qa401.ComputeRmsDb(startFreq, stopFreq, out rmsDbL, out rmsDbR);
        }

        public void ComputePeakDb(double startFreq, double stopFreq, out double PeakDbL, out double PeakDbR)
        {
            Qa401.GetPeakDb(startFreq, stopFreq, out PeakDbL, out PeakDbR);
        }

        public void ComputeThdPct(double fundamental, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdPct(fundamental, stopFreq, out thdPctL, out thdPctR);
        }

        public void ComputeThdnPct(double fundamental, double startFreq, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Qa401.ComputeThdnPct(fundamental, startFreq, stopFreq, out thdPctL, out thdPctR);
        }

        public bool LRVerifyPhase(int bufferOffset)
        {
            return Qa401.LRVerifyPhase(bufferOffset);
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




        /// <summary>
        /// Returns true if the QA401 application is running. 
        /// </summary>
        /// <returns></returns>
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
                            if (IsAppAlreadyRunning() == true)
                            {
                                // At this point we've launched the app and we've determiend
                                // it's running. Now we must wait to ensure it has been configured
                                // which could take up to 20 seconds. BUGBUG: Should add a another 
                                // global mutex to QA401 app to determine that FPGA is configured
                                Thread.Sleep(20000);
                                return true;
                            }

                            // Wait 1 second and try again
                            Thread.Sleep(1000);
                        }

                        // Here we couldn't confirm the app had launched
                        return false;
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
            static int tcpPort = 4300;

            public static IChannel GetChannel()
            {
                return GetChannel(tcpPort++, false);
            }

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
