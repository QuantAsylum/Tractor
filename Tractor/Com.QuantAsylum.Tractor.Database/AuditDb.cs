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

    /// <summary>
    /// The Audit Database is a database that lives in the cloud. It exists to provide 
    /// easy and reliable out-of-the-box storage of test data. 
    /// </summary>
    static class AuditDb
    {
        // Web service in cloud
        static string Url = "https://qatestcloud1.azurewebsites.net";

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

        static public bool CheckService()
        {
            var response = Client.GetAsync(Url + "/api/CheckService").Result;
            Log.WriteLine(LogType.Database, string.Format("Checkservice(). Response: " + response.StatusCode.ToString()));

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        static public void SubmitAuditData(AuditData d)
        {
            string json = new JavaScriptSerializer().Serialize(d);
            File.WriteAllText(Constants.AuditPath + @"\" + Guid.NewGuid().ToString() + ".cache", json);
            Log.WriteLine(LogType.Database, string.Format("Submitted AudioData to Filesystem"));
        }

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
