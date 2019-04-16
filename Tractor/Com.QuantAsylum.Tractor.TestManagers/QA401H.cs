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

namespace Com.QuantAsylum.Tractor.TestManagers
{
    class QA401H : IInstrument, IAudioAnalyzer
    {
        static HttpClient Client = new HttpClient();
        static string RootUrl;

        public QA401H()
        {
            SetRootUrl("http://localhost:9401");
        }

        void SetRootUrl(string rootUrl)
        {
            RootUrl = rootUrl;
            Client = new HttpClient
            {
                BaseAddress = new Uri(RootUrl)
            };
        }

        public void AudioAnalyzerSetTitle(string s)
        {
            return;
        }

        public void AudioGenSetGen1(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            PutSync(string.Format("/Settings/AudioGen/1/{0}/{1}/{2}", isOn ? 1 : 0, freq_Hz, ampLevel_dBV));
        }

        public void AudioGenSetGen2(bool isOn, float ampLevel_dBV, float freq_Hz)
        {
            PutSync(string.Format("/Settings/AudioGen/2/{0}/{1}/{2}", isOn ? 1 : 0, freq_Hz, ampLevel_dBV));
        }

        public void AuditionSetVolume(double volume)
        {
            throw new NotImplementedException();
        }

        public void AuditionStart(string fileName, double volume, bool repeat)
        {
            throw new NotImplementedException();
        }

        public void AuditionStop()
        {
            throw new NotImplementedException();
        }

        public void ComputeRms(double startFreq, double stopFreq, out double rmsDbvL, out double rmsDbvR)
        {
            Dictionary<string, object> d = GetSync(string.Format("/RmsDbv/{0}/{1}", startFreq, stopFreq));
            rmsDbvL = Convert.ToDouble(d["Left"]);
            rmsDbvR = Convert.ToDouble(d["Right"]);
        }

        public void ComputePeak(double startFreq, double stopFreq, out double PeakDbvL, out double PeakDbvR)
        {
            Dictionary<string, object> d = GetSync(string.Format("/PeakDbv/{0}/{1}", startFreq, stopFreq));
            PeakDbvL = Convert.ToDouble(d["Left"]);
            PeakDbvR = Convert.ToDouble(d["Right"]);
        }

        public void ComputeThdPct(double fundamental, double stopFreq, out double thdPctL, out double thdPctR)
        {
            Dictionary<string, object> d = GetSync(string.Format("/ThdPct/{0}/{1}", fundamental, stopFreq));
            thdPctL = Convert.ToDouble(d["Left"]);
            thdPctR = Convert.ToDouble(d["Right"]);
        }

        public bool ConnectToDevice()
        {
            return true;
        }

        public Bitmap GetBitmap()
        {
            return new Bitmap(128, 128);
        }

        public PointD[] GetData(ChannelEnum channel)
        {
            throw new NotImplementedException();
        }

        public int[] GetInputRanges()
        {
            return new int[] { 6, 26 };
        }

        public bool IsConnected()
        {
            return Convert.ToBoolean(GetSync("/Status/Connection", "Value"));
        }

        public bool IsRunning()
        {
            return true;
        }

        public void LaunchApplication()
        {
            throw new NotImplementedException();
        }

        public void DoAcquisition()
        {
            PostSync("/Acquisition");
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

        public void SetFftLength(uint length)
        {
            PutSync(string.Format("/Settings/BufferSize/{0}", length));
        }

        public void SetInputRange(int attenLevel_dB)
        {
            PutSync(string.Format("/Settings/Input/Max/{0}", attenLevel_dB));
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
                json = "{{}}";

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
