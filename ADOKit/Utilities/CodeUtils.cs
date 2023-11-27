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
    class CodeUtils
    {

        // get matching code for a given search term and return a list of CodeResult objects
        public static async Task<List<Objects.CodeResult>> searchCode(string credentials, string url, string searchText)
        {

            // this is the list of repos to return
            List<Objects.CodeResult> codeList = new List<Objects.CodeResult>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {


                // web request to search a project for code of interest
                url = url.Replace("dev.azure.com","almsearch.dev.azure.com"); // switch to the searching endpoint
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/search/codeAdvancedQueryResults?api-version=7.0-preview"); // use this undocumented REST API to do the searching
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                    // if cookie was provided
                    if (credentials.ToLower().Contains("userauthentication="))
                    {
                        webRequest.Headers.Add("Cookie", "X-VSS-UseRequestRouting=True; " + credentials);

                    }

                    // otherwise PAT was provided
                    else
                    {
                        webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                    }


                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        string json = "{\"searchText\": \"" + searchText + "\", \"skipResults\":0,\"takeResults\":1000,\"isInstantSearch\":true}";

                        streamWriter.Write(json);
                    }



                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string propName = "";
                    string filePath = "";
                    string projectName = "";
                    string projectID = "";
                    string repoName = "";
                    string repoID = "";

                    // read the json results
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the code result to our list as long as all fields are filled out
                                if (filePath != "" && projectName != "" && projectID != "" && repoName != "" && repoID != "")
                                {
                                    // form the full URL where the code was found since it doesn't include it in the REST API response
                                    string fullURL = url + "/" + projectName + "/_git/" + repoName + "?path=" + filePath;
                                    fullURL = fullURL.Replace("almsearch.dev.azure.com", "dev.azure.com");

                                    // only add to our code result list if we don't already have it
                                    if (!doesFileAlreadyExistInOurList(fullURL, codeList))
                                    {
                                        // form the URL needed to grab the raw file contents for the matching file for our code search
                                        string urlToGetCodeContents = url + "/" + projectName + "/_apis/git/repositories/" + repoID + "/items?path=" + filePath;
                                        urlToGetCodeContents = urlToGetCodeContents.Replace("almsearch.dev.azure.com", "dev.azure.com");
                                        string codeContents = await Utilities.FileUtils.getFileContents(credentials, urlToGetCodeContents);

                                        // only add to list if there is indeed code at the URL that came up in results. just another verification method.
                                        if (codeContents.Trim() != "")
                                        {
                                            codeList.Add(new CodeResult(filePath, projectName, projectID, repoName, repoID, fullURL, codeContents));
                                        }
                                    }
                                    
                                }
                                break;
                            case "StartArray":
                                break;
                            case "EndArray":
                                break;
                            case "PropertyName":
                                propName = jsonResult.Value.ToString();
                                break;
                            case "String":

                                // grab the path for code match
                                if (propName.ToLower().Equals("path"))
                                {
                                    filePath = jsonResult.Value.ToString();

                                }

                                // grab the project name for the code match
                                if (propName.ToLower().Equals("project"))
                                {
                                    projectName = jsonResult.Value.ToString();

                                }

                                // grab the project ID for the code match
                                if (propName.ToLower().Equals("projectid"))
                                {
                                    projectID = jsonResult.Value.ToString();

                                }

                                // grab the repo name for the code match
                                if (propName.ToLower().Equals("repository"))
                                {
                                    repoName = jsonResult.Value.ToString();

                                }

                                // grab the repo ID for the code match
                                if (propName.ToLower().Equals("repositoryid"))
                                {
                                    repoID = jsonResult.Value.ToString();

                                }

                                break;
                            case "Integer":
                                break;
                            default:
                                break;

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }

            return codeList;


        }

        // get the matching line of a code snippet that was searched
        public static string getMatchingCodeSnippet(string codeContents, string searchTerm)
        {
            string matchToReturn = "";
            string[] codeContentLines = codeContents.Split(new string[] { "\r\n", "\r", "\n" },StringSplitOptions.None);

            foreach(string line in codeContentLines)
            {
                if (line.ToLower().Contains(searchTerm.ToLower()))
                {
                    matchToReturn += line +"\r\n";
                }
            }

            return matchToReturn;

        }


        // determine whether we already have a code result in our list by the given unique URL for that file
        public static bool doesFileAlreadyExistInOurList(string url, List<Objects.CodeResult> codeList)
        {
            bool doesItExist = false;

            foreach (Objects.CodeResult result in codeList)
            {
                if (result.fullURL.ToLower().Equals(url.ToLower()))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }



    }
}
