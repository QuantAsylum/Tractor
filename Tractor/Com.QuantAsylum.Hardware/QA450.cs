using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Tractor.Com.QuantAsylum.Hardware
{
    /// <summary>
    /// Implents a simple REST interface to the QA450
    /// </summary>
    static class QA450
    {
        static HttpClient Client = new HttpClient();

        static string RootUrl;

        static QA450()
        {
            SetRootUrl("http://localhost:9450");
        }

        static void SetRootUrl(string rootUrl)
        {
            RootUrl = rootUrl;
            Client = new HttpClient
            {
                BaseAddress = new Uri(RootUrl)
            };
        }

        static public void SetImpedance(int impedance)
        {
            if (impedance == 4)
            {
                PutSync("/impedance", "Value", 4);
            }
            else if (impedance == 8)
            {
                PutSync("/impedance", "Value", 8);
            }
            else if (impedance == 0)
            {
                PutSync("/impedance", "Value", 0);
            }
            else
                throw new NotImplementedException("Bad value in SetImpedance()");
        }

        static public int GetImpedance()
        {
            string result = GetSync(RootUrl + "/impedance", "Value");
            return Convert.ToInt32(result);
        }

        static public float GetCurrent()
        {
            string result = GetSync(RootUrl + "/current", "Value");
            return Convert.ToSingle(result);
        }

        static public bool IsConnected()
        {
            string result = GetSync(RootUrl + "/connection", "Value");
            return Convert.ToBoolean(result);
        }

        static public void SetDutPower(bool powerEnable)
        {
            if (powerEnable)
                PutSync("/dutpower", "Value", 1);
            else
                PutSync("/dutpower", "Value", 0);
        }

        static public bool GetDutPower()
        {
            string result = GetSync(RootUrl + "/dutpower", "Value");
            return Convert.ToBoolean(result);
        }

        /// <summary>
        /// Synchronous PUT. This will throw an exception of the PUT fails for some reason
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="value"></param>
        static private void PutSync(string url, string token, int value)
        {
            string json = string.Format("{{\"{0}\":{1}}}", token, value);
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
        static private string GetSync(string url, string token)
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

            return dict[token].ToString();
        }
    }
}
