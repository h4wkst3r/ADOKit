using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using ADOKit.Objects;

namespace ADOKit.Utilities
{
    class BuildUtils
    {

        // get listing of builds
        public static async Task<List<Build>> getBuilds(string credentials, string url, string project)
        {

            // this is the list of URLs to return
            List<Build> projectBuilds = new List<Build>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {


                // web request to get all build definitions for a project
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/" + project + "/_apis/build/builds?api-version=7.1");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                    // if cookie was provided
                    if (credentials.ToLower().Contains("userauthentication="))
                    {
                        webRequest.Headers.Add("Cookie", "AadAuthenticationSet=false; " + credentials);

                    }

                    // otherwise PAT was provided
                    else
                    {
                        webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                    }


                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // parse the JSON output and display results
                    dynamic jsonResult = JsonConvert.DeserializeObject(content);

                    string buildURL = "";
                    int buildID = 0;
                    string buildDefName = "";

                    // read the json results, first all groups
                    foreach (var build in jsonResult.value)
                    {
                        buildID = build.id;
                        buildURL = build.url;
                        buildDefName = build.definition.name;

                        projectBuilds.Add(new Build(buildID, buildURL, buildDefName));
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }


            return projectBuilds;

        }

        // get listing of build logs
        public static async Task<List<BuildLog>> getBuildLogs(string credentials, string url, string project, int buildID)
        {
            // this is the list of log URLs to return
            List<BuildLog> buildLogs = new List<BuildLog>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {


                // web request to get all build definitions for a project
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create($"{url}/{project}/_apis/build/builds/{buildID.ToString()}/logs?api-version=7.1");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                    // if cookie was provided
                    if (credentials.ToLower().Contains("userauthentication="))
                    {
                        webRequest.Headers.Add("Cookie", "AadAuthenticationSet=false; " + credentials);

                    }

                    // otherwise PAT was provided
                    else
                    {
                        webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                    }


                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // parse the JSON output and display results
                    dynamic jsonResult = JsonConvert.DeserializeObject(content);

                    string logURL = "";
                    int logID = 0;
                    int lineCount = 0;

                    // read the json results, first all groups
                    foreach (var log in jsonResult.value)
                    {
                        logID = log.id;
                        logURL = log.url;
                        lineCount = log.lineCount;

                        buildLogs.Add(new BuildLog(buildID, logID, logURL, lineCount));
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }

            return buildLogs;
        }

        // Downloads log to a file
        public static async Task downloadLog(string credentials, string project, string outfolder, BuildLog log)
        {
            
            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                // web request to download a single log
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(log.logURL);
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                    // if cookie was provided
                    if (credentials.ToLower().Contains("userauthentication="))
                    {
                        webRequest.Headers.Add("Cookie", "AadAuthenticationSet=false; " + credentials);

                    }

                    // otherwise PAT was provided
                    else
                    {
                        webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                    }

                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();


                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                 
                    // store to file with the filename containing all useful infos
                    string filePath = $"{outfolder}/{project}_{log.buildID}_{log.logID}.txt";
                    System.IO.File.WriteAllText(filePath, content);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }

        }



        // Get content of a given build log
        public static async Task<string> getLogFileContent(string credentials, string project, BuildLog log)
        {
            string outputContent = "";

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                // web request to download a single log
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(log.logURL);
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                    // if cookie was provided
                    if (credentials.ToLower().Contains("userauthentication="))
                    {
                        webRequest.Headers.Add("Cookie", "AadAuthenticationSet=false; " + credentials);

                    }

                    // otherwise PAT was provided
                    else
                    {
                        webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                    }

                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                    outputContent = content;
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }

            return outputContent;

        }
    }
}
