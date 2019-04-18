using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Tractor.Com.QuantAsylum.Tractor.Database
{
    class AuditData
    {
        public string ProductId;
        public string SerialNumber;
        public string SessionName;
        public string Channel;
        public string TestGroup;
        public string TestFile;
        public string TestFileMd5;
        public string Name;
        public DateTime Time;
        public bool PassFail;
        public string ResultString;
        public float Result;
        public string TestLimits;
        public string Email;
    }

    class QueryData
    {
        public string ProductId = "";
        public string TestGroup = "";
        public string SerialNumber = "";
        public string TestName = "";
        public string TestSession = "";
        public bool IncludeLeft = false;
        public bool IncludeRight = false;
    }

    /// <summary>
    /// The Audit Database is a database that lives in the cloud. It exists to provide 
    /// easy and reliable out-of-the-box storage of test data. 
    /// </summary>
    static class AuditDb
    {
        // Web service in cloud
        static string Url = "https://qatestcloud1.azurewebsites.net";
        //static string Url = "http://localhost:7071";

        // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        static HttpClient Client = new HttpClient();

        static int _AuditQueueDepth;

        static public int AuditQueueDepth
        {
            get { return _AuditQueueDepth; }
        }

        static public void StartBackgroundWorker()
        {
            var task = new Task(() => BackgroundWorker(), TaskCreationOptions.LongRunning);
            task.Start();
        }

        static public string CheckService()
        {
            try
            {
                var response = Client.GetAsync(Url + "/api/CheckService").Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                    string result = (string)jsSerializer.DeserializeObject(content);
                    var r = jsSerializer.Deserialize<string>(result);
                    Log.WriteLine(LogType.Database, string.Format("Checkservice(). Response: " + r));
                    return r;
                }
            }
            catch (Exception ex)
            {

            }

            throw new HttpRequestException("CheckService() failed");
        }

        static public void SubmitAuditData(AuditData d)
        {
            string json = new JavaScriptSerializer().Serialize(d);
            File.WriteAllText(Constants.AuditPath + @"\" + Guid.NewGuid().ToString() + ".cache", json);
            Log.WriteLine(LogType.Database, string.Format("Submitted AudioData to Filesystem"));
        }

        /// <summary>
        /// Returns a list of test groups (newest first) associated with the specified product id
        /// and serial number
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        static public List<string> QueryGroupsBySerialNumber(string pid, string sn)
        {
            try
            {
                QueryData d = new QueryData() { ProductId = pid, SerialNumber = sn };

                var json = new JavaScriptSerializer().Serialize(d);
                var body = new StringContent(json, Encoding.UTF8, "application/json");
                var response = Client.PostAsync(Url + "/api/QueryGroups", body).Result;

                string content = response.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                string result = (string)jsSerializer.DeserializeObject(content);
                var r = jsSerializer.Deserialize<List<string>>(result);
                return r;
            }
            catch (Exception ex)
            {

            }

            return new List<string> { "An error occured in QueryGroupsBySerialNumber()" };
        }

        /// <summary>
        /// Returns a formatted string of all the tests in this group
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        static public string QueryTestsByGroup(string pid, string group)
        {
            try
            {
                QueryData d = new QueryData() { ProductId = pid, TestGroup = group };

                var json = new JavaScriptSerializer().Serialize(d);
                var body = new StringContent(json, Encoding.UTF8, "application/json");
                var response = Client.PostAsync(Url + "/api/QueryTests", body).Result;

                string content = response.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                string result = (string)jsSerializer.DeserializeObject(content);
                List<AuditData> ad = jsSerializer.Deserialize<List<AuditData>>(result);

                StringBuilder sb = new StringBuilder();
                if (ad.Count > 0)
                {
                    sb.AppendLine("Unit: " + ad[0].SerialNumber);
                    sb.AppendLine("Date: " + ad[0].Time.ToString());
                    for (int i = 0; i < ad.Count; i++)
                    {
                        sb.AppendFormat("{0}[{1}] {2} [{3}]  {4}" + Environment.NewLine, ad[i].Name, ad[i].Channel, ad[i].ResultString, ad[i].TestLimits, ad[i].PassFail);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {

            }

            return "An error occurred in QueryTestByGroup()";
        }

        static public List<string> QueryTestNames(string pid)
        {
            try
            {
                QueryData d = new QueryData() { ProductId = pid };

                var json = new JavaScriptSerializer().Serialize(d);
                var body = new StringContent(json, Encoding.UTF8, "application/json");
                var response = Client.PostAsync(Url + "/api/QueryTestNames", body).Result;

                string content = response.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                string result = (string)jsSerializer.DeserializeObject(content);
                List<string> vals = jsSerializer.Deserialize<List<string>>(result);

                return vals;
            }
            catch (Exception ex)
            {

            }

            return new List<string> { "An error occured in QueryTestNames()" };
        }

        /// <summary>
        /// Returns a formatted string of the statistics for this particular test
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        static public string QueryStatsByTest(string pid, string testName, string testSession, bool includeLeft, bool includeRight)
        {
            if (includeLeft == false && includeRight == false)
                return "Nothing to do";

            try
            {

                QueryData d = new QueryData() { ProductId = pid, TestName = testName, TestSession = testSession, IncludeLeft = includeLeft, IncludeRight = includeRight };

                var json = new JavaScriptSerializer().Serialize(d);
                var body = new StringContent(json, Encoding.UTF8, "application/json");
                var response = Client.PostAsync(Url + "/api/QueryResults", body).Result;

                string content = response.Content.ReadAsStringAsync().Result;
                JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
                string result = (string)jsSerializer.DeserializeObject(content);
                List<double> vals = jsSerializer.Deserialize<List<double>>(result);

                StringBuilder sb = new StringBuilder();
                Maths.StdDev(vals, out double avg, out double stdDev);
                sb.AppendFormat("Total Data Points: {0}" + Environment.NewLine, vals.Count());
                sb.AppendFormat("Mean: {0:0.00}" + Environment.NewLine, avg);
                // BUGBUG: Should convert to linear first?
                sb.AppendFormat("StdDev: {0:0.00}" + Environment.NewLine, stdDev);
                sb.AppendFormat("Min: {0:0.00}" + Environment.NewLine, vals.Min());
                sb.AppendFormat("Max: {0:0.00}" + Environment.NewLine, vals.Max());

                return sb.ToString();
            }
            catch (Exception ex)
            {
               
            }

            return "An error occurred. ";
        }

        static public string QueryTestNamesByProductId(string pid)
        {

            QueryData d = new QueryData() { ProductId = pid};

            var json = new JavaScriptSerializer().Serialize(d);
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            var response = Client.PostAsync(Url + "/api/QueryResults", body).Result;

            string content = response.Content.ReadAsStringAsync().Result;
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            string result = (string)jsSerializer.DeserializeObject(content);
            List<double> vals = jsSerializer.Deserialize<List<double>>(result);

            StringBuilder sb = new StringBuilder();
            //if (ad.Count > 0)
            //{
            //    sb.AppendLine("Unit: " + ad[0].SerialNumber);
            //    sb.AppendLine("Date: " + ad[0].Time.ToString());
            //    for (int i = 0; i < ad.Count; i++)
            //    {
            //        sb.AppendFormat("{0}[{1}] {2} [{3}]  {4}" + Environment.NewLine, ad[i].Name, ad[i].Channel, ad[i].ResultString, ad[i].TestLimits, ad[i].PassFail);
            //    }
            //}

            return sb.ToString();
        }

        /// <summary>
        /// Returns a 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        //static public List<string> QueryBySerialNumber(string pid, string serialNumber)
        //{
        //    QueryData d = new QueryData() { ProductId = pid, SerialNumber = serialNumber };

        //    var json = new JavaScriptSerializer().Serialize(d);
        //    var body = new StringContent(json, Encoding.UTF8, "application/json");
        //    var response = Client.PostAsync(Url + "/api/QueryTest", body).Result;

        //    string content = response.Content.ReadAsStringAsync().Result;
        //    JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
        //    string result = (string)jsSerializer.DeserializeObject(content);
        //    var r = jsSerializer.Deserialize<List<string>>(result);
        //    return r;
        //}

        /// <summary>
        /// Submits data to the cloud. Returns true if successful
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        static bool SubmitAuditDataToCloud(AuditData d)
        {
            var json = new JavaScriptSerializer().Serialize(d);
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            var response = Client.PostAsync(Url + "/api/AddTest", body).Result;
            Log.WriteLine(LogType.Database, string.Format("PostAsync() to cloud finished. Response: " + response.StatusCode.ToString()));

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        static void BackgroundWorker()
        {
            const int fileLimit = 500;
            while (true)
            {
                try
                {
                    string[] filePaths = Directory.GetFiles(Constants.AuditPath, "*.cache", SearchOption.TopDirectoryOnly);

                    _AuditQueueDepth = filePaths.Length;
                    if (filePaths.Length > fileLimit)
                    {
                        for (int i = 0; i <= fileLimit; i++)
                        {
                            SubmitFileToServer(filePaths[i]);
                            Thread.Sleep(1100);
                        }
                    }
                    else if (filePaths.Length >= 1)
                    {
                        SubmitFileToServer(filePaths[0]);
                        Thread.Sleep(1100);
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine(LogType.Database, "AuditDb BackgroundWorker exception: " + ex.Message);
                }

            }
        }

        /// <summary>
        /// Reads a file from the filesystem, converts it to an Audit, and pushes to cloud
        /// </summary>
        /// <param name="fileName"></param>
        static void SubmitFileToServer(string fileName)
        {
            try
            {
                string s = File.ReadAllText(fileName);
                AuditData d = (AuditData)new JavaScriptSerializer().Deserialize(s, typeof(AuditData));
                if (SubmitAuditDataToCloud(d))
                {
                    File.Delete(fileName);
                    Log.WriteLine(LogType.Database, "AuditDb BackgroundWorker successfully submitted a file to cloud");
                }
                else
                {
                    Log.WriteLine(LogType.Database, "AuditDb BackgroundWorker failed to submit a file to cloud");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Database, "AuditDb BackgroundWorker exception: " + ex.Message);
            }
        }
    }
}
