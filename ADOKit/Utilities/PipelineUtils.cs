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
    class PipelineUtils
    {
        public static async Task<List<string>> getBuildDefinitionURLs(string credentials, string url, string project)
        {

            // this is the list of URLs to return
            List<string> buildDefURLs = new List<string>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {


                // web request to get all build definitions for a project
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/" + project + "/_apis/build/definitions?api-version=5.0");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
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


                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string propName = "";
                    string buildDefURL = "";


                    // read the json results
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the URL to our list
                                if (buildDefURL != "" && buildDefURL.Contains("/_apis/build/Definitions"))
                                {

                                    buildDefURLs.Add(buildDefURL);
                                    buildDefURL = "";

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
                                // grab the url
                                if (propName.ToLower().Equals("href"))
                                {
                                    buildDefURL = jsonResult.Value.ToString();

                                }
                                break;
                            case "Boolean":
                                break;
                            case "Date":
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


            return buildDefURLs;

        }


        // get a list of build variables from a project build definition
        public static async Task<List<BuildVariable>> getBuildVarsOrSecrets(string credentials, string buildDefURL, bool getSecrets)
        {

            // this is the list of build variables to return
            List<BuildVariable> buildVariables = new List<BuildVariable>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {


                // web request to get all build variables for a build definition
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(buildDefURL);
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
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


                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // parse the JSON output and display results
                    dynamic jsonResult = JsonConvert.DeserializeObject(content);

                    string pipelineName = jsonResult.name;
                    string variableName = "";
                    string variableValue = "";

                    // read the json results if there are some variables
                    if (jsonResult.variables != null)
                    {
                        foreach (var variable in jsonResult.variables)
                        {
                            variableName = variable.Name;
                            variableValue = variable.Value.value;
                            // don't add the same variable multiple times or empty vars
                            if (variableValue != "" && !doesBuildVariableAlreadyExist(pipelineName, variableName, variableValue, buildVariables))
                            {
                                // only input either the var or the secret, not both
                                if (variable.Value.isSecret == "true" && getSecrets)
                                {
                                    buildVariables.Add(new BuildVariable(variableName, variableValue, pipelineName));
                                }
                                else if (variable.Value.isSecret != "true" && !getSecrets)
                                {
                                    buildVariables.Add(new BuildVariable(variableName, variableValue, pipelineName));
                                }
                            }

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


            return buildVariables;

        }

        // determine whether we already have that build variable
        public static bool doesBuildVariableAlreadyExist(string pipelineName, string name, string value, List<BuildVariable> buildList)
        {
            bool doesItExist = false;

            foreach (Objects.BuildVariable variable in buildList)
            {
                if (variable.pipelineName.Equals(pipelineName) && variable.name.Equals(name) && variable.value.Equals(value))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }


        public static async Task<List<GroupVariable>> getVariableGroups(string credentials, string url, string project)
        {

            // this is the list of group IDs to return
            List<GroupVariable> groupList = new List<GroupVariable>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                // web request to get all variable groups for a project
                // https://learn.microsoft.com/en-us/rest/api/azure/devops/distributedtask/variablegroups/get-variable-groups
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/" + project + "/_apis/distributedtask/variablegroups?api-version=7.2-preview.2");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
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


                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    dynamic jsonResult = JsonConvert.DeserializeObject(content);

                    string groupName = "";
                    string variableName = "";
                    string variableValue = "";

                    // read the json results, first all groups
                    foreach (var group in jsonResult.value)
                    {
                        groupName = group.name;

                        // then all variables for this group
                        foreach(var variable in group.variables)
                        {
                            variableName = variable.Name;
                            variableValue = variable.Value.isSecret=="true" ? "[HIDDEN]" : variable.Value.value;
                            groupList.Add(new GroupVariable(variableName, variableValue, groupName));
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


            return groupList;

        }

    }
}
