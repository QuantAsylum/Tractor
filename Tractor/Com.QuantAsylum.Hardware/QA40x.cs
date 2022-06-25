using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using Tractor;

namespace Com.QuantAsylum.Hardware
{
    class QA40x : IInstrument, IAudioAnalyzer
    {
        static HttpClient Client = new HttpClient();
        static string RootUrl;

        public QA40x()
        {
            SetRootUrl("http://localhost:9402");
        }

        void SetRootUrl(string rootUrl)
        {
            RootUrl = rootUrl;
            Client = new HttpClient
            {
                BaseAddress = new Uri(RootUrl)
            };
        }

        public void AudioAnalyzerSetTitle(string title)
        {
            title = title.Substring(0, title.Length > 100 ? 100 : title.Length);
            title = WebUtility.UrlEncode(title);
            PutSync($"/Settings/Title/{title}");
        }

        public void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            PutSync(string.Format("/Settings/AudioGen/Gen1/{0}/{1}/{2}", isOn ? "On" : "Off", freq_Hz, ampLevel_dBV));
        }

        public void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            PutSync(string.Format("/Settings/AudioGen/Gen2/{0}/{1}/{2}", isOn ? "On" : "Off", freq_Hz, ampLevel_dBV));
        }

        public void SetMuting(bool muteLeft, bool muteRight)
        {
            if (muteLeft && muteRight)
            {
                throw new Exception("Both left and right channels cannot be muted in QA40x.cs SetMuting");
            }

            if (muteLeft)
            {
                PutSync("/Settings/Muting/MuteLeft");
            }
            else if (muteRight)
            {
                PutSync("/Settings/Muting/MuteRight");
            }
            else
            {
                PutSync("/Settings/Muting/MuteNone");
            }
        }

        public void AuditionSetVolume(double volume)
        {
            PutSync($"/AuditionVolume/{volume:0.000}");
        }

        public void AuditionStart(string fileName, double volume, bool repeat)
        {
            fileName = WebUtility.UrlEncode(fileName);
            PostSync($"/AuditionStart/{fileName}/{8}/{volume}/{repeat}");
        }

        public void AuditionStop()
        {
            PostSync($"/AuditionStop");
        }

        public void ComputeRms(double startFreq, double stopFreq, out double rmsDbvL, out double rmsDbvR)
        {
            Dictionary<string, object> d = GetSync($"/RmsDbv/{startFreq}/{stopFreq}");
            rmsDbvL = Convert.ToDouble(d["Left"]);
            rmsDbvR = Convert.ToDouble(d["Right"]);
        }

        public void ComputePeakDb(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR)
        {
            Dictionary<string, object> d = GetSync($"/PeakDbv/{startFreq}/{stopFreq}");
            PeakDbvL = Convert.ToDouble(d["Left"]);
            PeakDbvR = Convert.ToDouble(d["Right"]);
        }

        public void ComputeThdPct(double fundamental, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Dictionary<string, object> d = GetSync($"/ThdPct/{fundamental}/{stopFreq}");
            thdPctL = Convert.ToDouble(d["Left"]);
            thdPctR = Convert.ToDouble(d["Right"]);
        }

        public void ComputeThdnPct(double fundamental, double minFreq, double maxFreq, out double thdnPctL, out double thdnPctR)
        {
            Dictionary<string, object> d = GetSync($"/ThdnPct/{fundamental}/{minFreq}/{maxFreq}");
            thdnPctL = Convert.ToDouble(d["Left"]);
            thdnPctR = Convert.ToDouble(d["Right"]);
        }

        public bool ConnectToDevice(out string result)
        {
            result = "";

            if (IsConnected() == false)
            {
                result = $"Unable to connect to the QA40x. Is the QA40x application running? Is it version {Constants.RequiredQa40xVersion:0.000} or later?";
                return false;
            }

            return true;
        }

        public void CloseConnection()
        {

        }

        public Bitmap GetBitmap()
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(RootUrl + "/Graph/Frequency/In/0");
            Bitmap bitmap = new Bitmap(stream);
            stream.Flush();
            stream.Close();

            return bitmap;
        }

        public PointD[] GetData(ChannelEnum channel)
        {
            throw new NotImplementedException();
        }

        public int[] GetInputRanges()
        {
            return new int[] { 0, 6, 12, 18, 24, 30, 36, 42 };
        }

        public bool IsConnected()
        {
            try
            {
                if(Convert.ToBoolean(GetSync("/Status/Connection", "Value")))
                {
                    if (Convert.ToDouble(GetSync("/Status/Version", "Value")) >= Constants.RequiredQa40xVersion)
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        public bool IsRunning()
        {
            return true;
        }

        public void LaunchApplication()
        {
            //throw new NotImplementedException();
        }

        public void DoAcquisition()
        {
            PostSync("/Acquisition");
        }

        public void DoFrAquisition(float ampLevl_dBV, double windowSec, int smoothingDenominator)
        {
            PutSync($"/Settings/ExpoChirpGen/{ampLevl_dBV:0.00}/{windowSec:0.00}/{smoothingDenominator}/{false}");
            PutSync($"/Settings/OutputSource/ExpoChirp");
            PostSync("/Acquisition");
        }

        public void AddMathToDisplay()
        {
            throw new NotImplementedException();
        }



        void DoAcquisitionAsync()
        {
            PostSync("/AcquisitionAsync");
        }

        public bool AnalyzerIsBusy()
        {
            string s = GetSync("AcquisitionBusy", "Value");
            return Convert.ToBoolean(s);
        }

        public void TestMask(string maskFile, bool testL, bool testR, bool testMath, out bool passLeft, out bool passRight, out bool passMath)
        {
            passMath = false;
            //maskFile = Path.GetFileName(maskFile);
            maskFile = WebUtility.UrlEncode(maskFile);
            Dictionary<string, object> d =  GetSync($"/MaskTest/{maskFile}/{testL}/{testR}");

            passLeft = Convert.ToBoolean(d["PassLeft"]);
            passRight = Convert.ToBoolean(d["PassRight"]);
        }

        public void SetYLimits(int yMax, int yMin)
        {

        }

        public bool LRVerifyPhase(int bufferOffset)
        {
            throw new NotImplementedException();
        }



        public void SetFftLength(uint length)
        {
            PutSync(string.Format("/Settings/BufferSize/{0}", length));
        }

        public void SetInputRange(int attenLevel_dB)
        {
            PutSync(string.Format("/Settings/Input/Max/{0}", attenLevel_dB));
        }

        public void SetOffsets(double inputOffset, double outputOffset)
        {
            if (inputOffset != 0 || outputOffset != 0)
            {
                throw new InvalidOperationException("Exception in QA40x.cs SetOffsets(): QA40x can only accept in/out offsets of 0");
            }
        }

        public void SetToDefaults()
        {
            PutSync("/Settings/Default");
        }

        /*******************************************************************/
        /*********************** HELPERS for REST **************************/
        /*******************************************************************/

        static private void PutSync(string url)
        {
            PutSync(url, "", 0);
        }

        /// <summary>
        /// Synchronous PUT. This will throw an exception of the PUT fails for some reason
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="value"></param>
        static private void PutSync(string url, string token, int value)
        {
            string json;

            if (token != "")
                json = string.Format("{{\"{0}\":{1}}}", token, value);
            else
                json = "{{}}";

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // We make the PutAsync synchronous via the .Result
            var response = Client.PutAsync(url, content).Result;

            // Throw an exception if not successful
            response.EnsureSuccessStatusCode();
            response.Dispose();
        }

        static private void PostSync(string url)
        {
            PostSync(url, "", 0);
        }

        static private void PostSync(string url, string token, int value)
        {
            string json;

            if (token != "")
                json = string.Format("{{\"{0}\":{1}}}", token, value);
            else
                json = "";

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // We make the PutAsync synchronous via the .Result
            var response = Client.PostAsync(url, content).Result;

            // Throw an exception if not successful
            response.EnsureSuccessStatusCode();
            response.Dispose();
        }

        static private Dictionary<string, object> GetSync(string url)
        {
            string content;

            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            content = response.Content.ReadAsStringAsync().Result;
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(content);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict = (Dictionary<string, object>)result;
            response.Dispose();

            return dict;
        }

        static private string GetSync(string url, string token)
        {
            Dictionary<string, object> dict = GetSync(url);
            return dict[token].ToString();
        }

        void IAudioAnalyzer.DoAcquisitionAsync()
        {
            throw new NotImplementedException();
        }

        
    }
}
