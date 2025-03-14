using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;
using ADOKit.Objects;

namespace ADOKit.Utilities
{
    class OrgUtils
    {
        // Retrieve memberId
        public static async Task<string> getMemberId(string credentials)
        {
            string memberId = "";

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // parse the JSON output and display results
            JsonTextReader jsonResult;

            try
            {

                string url = "https://app.vssps.visualstudio.com";
                string content = "";

                // web request to get memberId
                HttpWebRequest webRequest = null;
                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/profile/profiles/me?api-version=7.1");


                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";
                    webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));

                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                }


                // parse the JSON output and display results
                jsonResult = new JsonTextReader(new StringReader(content));
                string propName = "";
                // read the json results
                while (jsonResult.Read())
                {
                    switch (jsonResult.TokenType.ToString())
                    {
                        case "StartObject":
                            break;
                        case "EndObject":
                            break;
                        case "StartArray":
                            break;
                        case "EndArray":
                            break;
                        case "PropertyName":
                            propName = jsonResult.Value.ToString();
                            break;
                        case "String":
                            // grab the memberId
                            if (propName.ToLower().Equals("publicalias"))
                            {
                                memberId = jsonResult.Value.ToString();
                            }
                            break;
                        case "Integer":
                            break;
                        default:
                            break;

                    }

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("[-] ERROR: " + ex.StackTrace);
                Console.WriteLine("");
            }



            return memberId;
        }


        // Retrieve the list of all Azure DevOps organizations accessible with the current access token.
        public static async Task<List<Org>> getAccessibleOrgs(string credentials)
        {
            // this is the list of orgs to return
            List<Org> orgList = new List<Org>();


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

	        // parse the JSON output and display results
	        JsonTextReader jsonResult;
	    
	        string orgName = "";
            string orgID = "";
            string content = "";
            string propName = "";

            string url = "https://app.vssps.visualstudio.com";

            try
            {
                // Retrieve memberId/publicAlias via REST API
                string memberId = await getMemberId(credentials); 

                // Web request to get Organizations we have access to.
                // https://learn.microsoft.com/en-us/rest/api/azure/devops/account/accounts/list?view=azure-devops-rest-7.1&tabs=HTTP
                HttpWebRequest webRequest = null;
                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + $"/_apis/accounts?memberId={memberId}?api-version=7.1");


                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";
                    webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));

                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                }

                // parse the JSON output and display results
                jsonResult = new JsonTextReader(new StringReader(content));
                
                // read the json results
                while (jsonResult.Read())
                {
                    //Console.WriteLine($"Token: {jsonResult.TokenType}, Value: {jsonResult.Value}");
                    switch (jsonResult.TokenType.ToString())
                    {
                        case "StartObject":
                            break;
                        case "EndObject":
                            
                            if (orgID != "" && orgName != "")
                            {
                                orgList.Add(new Org(orgName, orgID));
                                orgID = "";
                                orgName = "";
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
                            // grab the org id (AccountId)
                            if (propName.ToLower().Equals("accountid"))
                            {
                                orgID = jsonResult.Value.ToString();
                            }
                            // grab the org name (AccountName)
                            if (propName.ToLower().Equals("accountname"))
                            {
                                orgName = jsonResult.Value.ToString();
                            }
                            break;
                        case "Integer":
                            break;
                        default:
                            break;

                    }

                }

            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("[-] ERROR: " + ex.StackTrace);
                Console.WriteLine("");
            }

            return orgList;
        }


        // Retrieve the list of all Azure DevOps organizations registered in Azure Active Directory.
        public static async Task<List<Org>> getAadOrgs(string credentials, string aexEndpoint)
        {
            // this is the list of orgs to return
            List<Org> orgList = new List<Org>();

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Extract tenant id (tid) claim from the JWT.
            string tenantID = JwtUtils.getTenantID(credentials);
            if (string.IsNullOrEmpty(tenantID))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Tenant ID (tid) not found in the token.");
                Console.WriteLine("");

                return orgList;
            }




            /*
            
            Construct the endpoint URL.
            The endpoint was selected at random, valid endpoints include:
            aexprodcus1, aexprodeus21, aexprodea1, aexprodweu1, aexprodeau1, aexprodsin1, etc.
            Additional endpoints may be found via Google dorks or by inspecting the "X-VSS-DeploymentAffinity" cookie from aex.dev.azure.com.
            
            */

            string endpoint = "";
            if (string.IsNullOrEmpty(aexEndpoint))
            {
                aexEndpoint = "aexprodcus1"; // default, if operator doesn't supply custom endpoint.
            }
            else
            {
                endpoint = aexEndpoint;
            }
            
            string url = $"https://{endpoint}.vsaex.visualstudio.com";

            try
            {
                HttpWebRequest webRequest = null;
                webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + $"/_apis/EnterpriseCatalog/Organizations?tenantId={tenantID}");

                // set header values
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";
                //webRequest.Accept = "application/octet-stream;api-version=5.2-preview.1;excludeUrls=true;enumsAsNumbers=true;msDateFormat=true;noArrayWrap=true"; //Not necessary during testing. YMMV.
                webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));

                string content = "";
                if (webRequest != null)
                {

                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                }

                // parse the CSV output and display results
                using (StringReader sr = new StringReader(content))
                {
                    string line;
                    bool headerParsed = false;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Skip the header line.
                        if (!headerParsed)
                        {
                            headerParsed = true;
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Split the CSV line.
                        string[] parts = line.Split(',');
                        // Expect at least 4 parts: Org Id, Org Name, Url, Owner.
                        if (parts.Length >= 4)
                        {
                            string orgId = parts[0].Trim();
                            string orgName = parts[1].Trim();
                            string orgUrl = parts[2].Trim();
                            string orgOwner = parts[3].Trim();

                            orgList.Add(new Org(orgName, orgId, orgUrl, orgOwner));
                        }
                    }
                }


            }
            catch (WebException webEx)
            {
                if (webEx.Response is HttpWebResponse httpResponse && httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: 401 Unauthorized. Specify a valid aex endpoint.");
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: " + webEx.Message);
                    Console.WriteLine("[-] ERROR: " + webEx.StackTrace);
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("[-] ERROR: " + ex.StackTrace);
                Console.WriteLine("");
            }

            return orgList;
        }
    }
}
