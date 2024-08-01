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
    class PatUtils
    {
        public static async Task<List<PatToken>> getUserPats(string credentials, string url)
        {
            // this is the list of PAT's to return
            List<PatToken> patList = new List<PatToken>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                url = url.Replace("dev.azure.com", "vssps.dev.azure.com");

                // web request to get all PATs for a user
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/Token/SessionTokens?api-version=7.0-preview.1");
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

                                // add the pat to our list as long as it has display name, validTo and scope
                                if (authID != "" && displayName != "" && validTo != "" && scope != "" && isValid)
                                {

                                    if (!doesPatAlreadyExistInList(authID, patList))
                                    {

                                        patList.Add(new PatToken(authID, displayName, validTo, scope, tokenContent, isValid.ToString()));
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


                                break;
                            case "Boolean":
                                // grab whether the PAT is still active/valid
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


            return patList;
        }

        // remove a personal access token
        public static async Task<bool> deletePAT(string credentials, string url, string patID)
        {

            bool success = false;

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                url = url.Replace("dev.azure.com", "vssps.dev.azure.com");

                // web request to remove a PAT
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/Token/SessionTokens/" + patID);
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
                    if (statusCode.ToLower().Equals("ok")){
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


        // create a personal access token
        public static async Task<PatToken> createPAT(string credentials, string url)
        {

            // this is the PAT to return
            PatToken tokenToReturn = null;


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                // web request to create a PAT
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

                        // create random token name
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[8];

                        for (int i = 0; i < 8; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string personalAccessTokenName = new string(chars);
                        personalAccessTokenName = "ADOKit-" + personalAccessTokenName;

                        // create the validTo for the token for 1 year from now
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
                        string dateTokenGoodTil = year + "-" + month + "-" + day + "T00:00:00.000Z";


                        string json = "{\"contributionIds\":[\"ms.vss-token-web.personal-access-token-issue-session-token-provider\"],\"dataProviderContext\":{\"properties\":{\"displayName\":\"" + personalAccessTokenName + "\",\"validTo\":\"" + dateTokenGoodTil + "\",\"scope\":\"app_token\",\"targetAccounts\":[]}}}}}";

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

                                    tokenToReturn = new PatToken(authID, displayName, validTo, scope, tokenContent, isValid.ToString());

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

                                // grab the token value
                                if (propName.ToLower().Equals("token"))
                                {
                                    tokenContent = jsonResult.Value.ToString();
                                }


                                break;
                            case "Boolean":
                                // grab whether the PAT is still active/valid
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


            return tokenToReturn;
        }


        // determine whether we already have a PAT in our list by the given unique GUID for that PAT
        public static bool doesPatAlreadyExistInList(string guid, List<PatToken> patList)
        {
            bool doesItExist = false;

            foreach (Objects.PatToken pat in patList)
            {
                if (pat.authID.Equals(guid))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }



    }
}
