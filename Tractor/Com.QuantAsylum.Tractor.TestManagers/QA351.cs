using Com.QuantAsylum.Tractor.TestManagers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Tractor.Com.QuantAsylum.Tractor.TestManagers
{
    class QA351 : IInstrument, IVoltMeter
    {
        static HttpClient Client = new HttpClient();

        static string RootUrl;

        public QA351()
        {
            SetRootUrl("http://localhost:9351");
        }
        void SetRootUrl(string rootUrl)
        {
            RootUrl = rootUrl;
            Client = new HttpClient
            {
                BaseAddress = new Uri(RootUrl)
            };
        }

        public bool ConnectToDevice(out string result)
        {
            // Nothing special to do for REST device
            result = "";
            return true;
        }

        public void CloseConnection()
        {

        }

        public double GetVersion()
        {
            string result = GetSync(RootUrl + "/Status/Version", "Value");
            return Convert.ToDouble(result);
        }

        public bool IsConnected()
        {
            // Do a version read and see if the correct version comes back
            try
            {
                double current = GetVersion();
                return true;
            }
            catch
            {

            }

            return false;
        }

        public bool IsRunning()
        {
            return IsConnected();
        }

        public void LaunchApplication()
        {

        }

        public void SetToDefaults()
        {

        }

        public float GetVoltage(int channel)
        {
            if (channel < 0 || channel > 1)
                throw new Exception("Invalid channel specified. The QA351 can support channel numbers of 0 (Main) and 1 (Aux)");

            string result = GetSync(RootUrl + $"/Volts/{channel}", "Value");
            float  val = Convert.ToSingle(result);
            return val;
        }

        /*******************************************************************/
        /*********************** HELPERS for REST **************************/
        /*******************************************************************/

        private void PutSync(string url)
        {
            PutSync(url, "", 0);
        }

        /// <summary>
        /// Synchronous PUT. This will throw an exception of the PUT fails for some reason
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="value"></param>
        private void PutSync(string url, string token, int value)
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
        }

        /// <summary>
        /// Synchronous GET. This will throw an exception if the GET fails for some reason
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetSync(string url, string token)
        {
            string content;

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = Client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            content = response.Content.ReadAsStringAsync().Result;
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            var result = jsSerializer.DeserializeObject(content);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict = (Dictionary<string, object>)result;

            return dict[token].ToString();
        }
    }
}
