using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ADOKit.Modules.Recon
{
    class Check
    {

        public static async Task execute(string credential, string url)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("check", credential, url, "", "", "", ""));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Checking if organization provided uses Azure DevOps");
                Console.WriteLine("");

                // web request to check auth and whether organization uses Azure DevOps
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url);
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                    // get web response
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                    // if we didn't go to exception, then the organization exists and we can proceed to check credentials provided
                    Console.WriteLine("[+] SUCCESS: Organization provided exists in Azure DevOps");
                    Console.WriteLine("");

                    // check if credentials provided are valid
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Checking credentials provided");
                    Console.WriteLine("");

                    // if creds valid, then provide message
                    if (await Utilities.WebUtils.credsValid(credential, url))
                    {
                        Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                        Console.WriteLine("");
                    }

                    // if creds not valid, display message
                    else
                    {
                        Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                        Console.WriteLine("");
                    }

                } // end web request

            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: Organization provided does not exist in Azure DevOps");
                Console.WriteLine("");
            }


        }

    }

}
