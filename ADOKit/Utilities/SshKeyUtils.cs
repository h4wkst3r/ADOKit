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
    class SshKeyUtils
    {


        // get all SSH keys for a user
        public static async Task<List<SshKey>> getUserSSHKeys(string credentials, string url)
        {
            // this is the list of ssh keys to return
            List<SshKey> sshKeyList = new List<SshKey>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                url = url.Replace("dev.azure.com", "vssps.dev.azure.com");

                // web request to get all ssh keys for a user
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/Token/SessionTokens?isPublic=true&includePublicData=true&api-version=7.0-preview.1");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json;api-version=5.0-preview.1";
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
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string propName = "";
                    string displayName = "";
                    string validTo = "";
                    string scope = "";
                    string tokenContent = "";
                    string authID = "";
                    bool isValid = false;

                    // read the json results
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the SSH key to our list as long as it has all needed attributes
                                if (authID != "" && displayName != "" && validTo != "" && scope != "" && isValid)
                                {

                                    if (!doesSSHKeyAlreadyExistInList(authID, sshKeyList))
                                    {

                                        sshKeyList.Add(new SshKey(authID, displayName, validTo, scope, tokenContent, isValid.ToString()));
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

                                // grab the display name
                                if (propName.ToLower().Equals("displayname"))
                                {
                                    displayName = jsonResult.Value.ToString();

                                }

                                // grab the scope
                                if (propName.ToLower().Equals("scope"))
                                {
                                    scope = jsonResult.Value.ToString();
                                }

                                // grab the auth id
                                if (propName.ToLower().Equals("authorizationid"))
                                {
                                    authID = jsonResult.Value.ToString();
                                }

                                // grab the ssh key content
                                if (propName.ToLower().Equals("publicdata"))
                                {
                                    tokenContent = jsonResult.Value.ToString();
                                }



                                break;
                            case "Boolean":
                                // grab whether the SSH key is still active/valid
                                if (propName.ToLower().Equals("isvalid"))
                                {
                                    isValid = (bool)jsonResult.Value;
                                }
                                break;
                            case "Date":
                                // grab the valid to
                                if (propName.ToLower().Equals("validto"))
                                {
                                    validTo = jsonResult.Value.ToString();
                                }
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


            return sshKeyList;
        }



        // create an SSH key for a user
        public static async Task<SshKey> createSSHKey(string credentials, string url, string sshKey, string orgID)
        {

            // this is the PAT to return
            SshKey keyToReturn = null;


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                // web request to create SSH key
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/Contribution/HierarchyQuery");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.Accept = "application/json;api-version=5.0-preview.1";
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


                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        // create random ssh key name
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[8];

                        for (int i = 0; i < 8; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string sshKeyName = new string(chars);
                        sshKeyName = "ADOKit-" + sshKeyName;

                        // create the validTo for the ssh key for 1 year from now
                        DateTime myDateTime = DateTime.Now.AddYears(1);
                        string month = myDateTime.Month.ToString();
                        if (month != "12" && month != "11" && month != "10")
                        {
                            month = "0" + month;
                        }
                        string year = myDateTime.Year.ToString();
                        string day = myDateTime.Day.ToString();
                        if (day.Equals("1") || day.Equals("2") || day.Equals("3") || day.Equals("4") || day.Equals("5") || day.Equals("6") || day.Equals("7") || day.Equals("8") || day.Equals("9"))
                        {
                            day = "0" + day;
                        }
                        string dateKeyGoodTil = year + "-" + month + "-" + day + "T00:00:00.000Z";


                        string json = "{\"contributionIds\":[\"ms.vss-token-web.personal-access-token-issue-session-token-provider\"],\"dataProviderContext\":{\"properties\":{\"displayName\":\"" + sshKeyName + "\",\"publicData\":\"" + sshKey + "\",\"validTo\":\"" + dateKeyGoodTil + "\",\"scope\":\"app_token\",\"isPublic\":true,\"targetAccounts\":[\"" + orgID + "\"]}}}}}";

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
                    string displayName = "";
                    string validTo = "";
                    string scope = "";
                    string tokenContent = "";
                    string authID = "";
                    bool isValid = false;

                    // read the json results
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the pat to our list as long as it has display name, validTo and scope
                                if (authID != "" && displayName != "" && validTo != "" && scope != "" && isValid)
                                {

                                    keyToReturn = new SshKey(authID, displayName, validTo, scope, tokenContent, isValid.ToString());

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

                                // grab the display name
                                if (propName.ToLower().Equals("displayname"))
                                {
                                    displayName = jsonResult.Value.ToString();

                                }

                                // grab the scope
                                if (propName.ToLower().Equals("scope"))
                                {
                                    scope = jsonResult.Value.ToString();
                                }

                                // grab the auth id
                                if (propName.ToLower().Equals("authorizationid"))
                                {
                                    authID = jsonResult.Value.ToString();
                                }

                                // grab the ssh key value
                                if (propName.ToLower().Equals("publicdata"))
                                {
                                    tokenContent = jsonResult.Value.ToString();
                                }


                                break;
                            case "Boolean":
                                // grab whether the ssh key is still active/valid
                                if (propName.ToLower().Equals("isvalid"))
                                {
                                    isValid = (bool)jsonResult.Value;
                                }
                                break;
                            case "Date":
                                // grab the valid to
                                if (propName.ToLower().Equals("validto"))
                                {
                                    validTo = jsonResult.Value.ToString();
                                }
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
                Console.WriteLine("[-] ERROR: Unable to add SSH key" + ex.Message);
                Console.WriteLine("");

            }


            return keyToReturn;
        }


        // remove an SSH key
        public static async Task<bool> deleteSSHKey(string credentials, string url, string sshKeyID)
        {

            bool success = false;

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                url = url.Replace("dev.azure.com", "vssps.dev.azure.com");

                // web request to remove an SSH key
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/Token/SessionTokens/" + sshKeyID + "?isPublic=true");
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "DELETE";
                    webRequest.ContentType = "application/json";
                    webRequest.Accept = "application/json;api-version=5.0-preview.1";
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


                    // get web response and if success set success to true
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                    string statusCode = myWebResponse.StatusCode.ToString();
                    if (statusCode.ToLower().Equals("ok"))
                    {
                        success = true;
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");

            }

            return success;


        }



        // determine whether we already have an SSH Key in our list by the given unique GUID for that SSH key
        public static bool doesSSHKeyAlreadyExistInList(string guid, List<SshKey> sshKeyList)
        {
            bool doesItExist = false;

            foreach (Objects.SshKey key in sshKeyList)
            {
                if (key.authID.Equals(guid))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }




    }
}
