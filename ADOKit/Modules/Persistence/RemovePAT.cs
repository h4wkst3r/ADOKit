using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Persistence
{
    class RemovePAT
    {

        public static async Task execute(string credential, string url, string patID)
        {

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("removepat", credential, url, "", "", "", ""));

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // check if credentials provided are valid
            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (await Utilities.WebUtils.credsValid(credential, url))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");

                try
                {


                    bool success = await Utilities.PatUtils.deletePAT(credential, url, patID);


                    // if removal of PAT was successful, display message and listing of current PAT's for user
                    if (success)
                    {

                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: PAT with ID " + patID + " was removed successfully.");
                        Console.WriteLine("");

                        // create table header
                        string tableHeader = string.Format("{0,40} | {1,30} | {2,30} | {3,30}", "PAT ID", "Name", "Scope", "Valid Until");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all PAT's for the user being authenticated as
                        List<Objects.PatToken> patList = await Utilities.PatUtils.getUserPats(credential, url);


                        // iterate through the list of PAT's and display info about them
                        foreach (Objects.PatToken token in patList)
                        {

                            Console.WriteLine("{0,40} | {1,30} | {2,30} | {3,30}", token.authID, token.displayName, token.scope, token.validTo);

                        }

                        Console.WriteLine("");

                    }

                    // if not successful, display message
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[-] ERROR: Could not remove the PAT with ID provided.");
                        Console.WriteLine("");
                    }





                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: " + ex.Message);
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
