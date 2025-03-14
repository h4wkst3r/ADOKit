using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ADOKit.Modules.Recon
{
    class ListOrgs
    {
        public static async Task execute(string credential, Dictionary<string, string> args)
        {

            // Determine the mode: "aad" if specified, otherwise default.
            string mode = args.ContainsKey("mode") ? args["mode"].ToLower() : "";

            // Determine which aex endpoint to use based on operator input.
            string aexEndpoint = args.ContainsKey("endpoint") ? args["endpoint"].ToLower() : ""; 



            // do some error check. endpoint should not be defined and the only supported mode is aad.
            // sanity checks
            if(string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(aexEndpoint))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Custom endpoint not supported for default mode.");
                Console.WriteLine("");
                return;
            }

            if(!string.IsNullOrEmpty(mode) && mode != "aad")
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Unsupported mode.");
                Console.WriteLine("");
                return;
            }


            if (string.IsNullOrEmpty(aexEndpoint))
            {
                aexEndpoint = "aexprodcus1"; // default AEX endpoint for AAD mode; ensure it matches the "aexEndpoint" variable in getAadOrgs for consistent logging and traffic.
            }


            // Choose the appropriate host based on mode.
            string host = (mode == "aad") ? $"https://{aexEndpoint}.vsaex.visualstudio.com" : "https://app.vssps.visualstudio.com";


            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("listorgs", credential, host, "", "", "", ""));

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // check if credentials provided are valid
            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (Utilities.JwtUtils.isValid(credential))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");

                try
                {

                    List<Objects.Org> orgList;


                    if (mode == "aad")
                    {
                        // create table header
                        //The given organization name is not valid. Organization names must start with a letter or number, followed by letters, numbers or hyphens, and must end with a letter or number.
                        string tableHeader = string.Format("{0,40} | {1,40} | {2,50} | {3,40}", "Organization ID", "Organization Name", "URL", "Owner");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all organizations access token has access to in the Azure DevOps instance
                        orgList = await Utilities.OrgUtils.getAadOrgs(credential, aexEndpoint);

                        // iterate through the list of orgs and list them
                        foreach (Objects.Org org in orgList)
                        {

                            Console.WriteLine("{0,40} | {1,40} | {2,50} | {3,40}", org.orgID, org.orgName, org.url, org.owner);

                        }

                        //Console.WriteLine("");

                    }
                    else
                    {
                        // create table header
                        //The given organization name is not valid. Organization names must start with a letter or number, followed by letters, numbers or hyphens, and must end with a letter or number.
                        string tableHeader = string.Format("{0,40} | {1,40}", "Organization ID", "Organization Name");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all organizations access token has access to in the Azure DevOps instance
                        orgList = await Utilities.OrgUtils.getAccessibleOrgs(credential);

                        // iterate through the list of orgs and list them
                        foreach (Objects.Org org in orgList)
                        {

                            Console.WriteLine("{0,40} | {1,40}", org.orgID, org.orgName);

                        }

                        Console.WriteLine("");
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

            }

            // if creds not valid, display message and return
            else
            {
                Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                Console.WriteLine("");
                return;
            }
        }
    }
}
