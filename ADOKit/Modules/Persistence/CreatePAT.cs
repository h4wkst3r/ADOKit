using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace ADOKit.Modules.Persistence
{
    class CreatePAT
    {

        public static async Task execute(string credential, string url)
        {

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("createpat", credential, url, "", "", "", ""));

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

                    // create table header
                    string tableHeader = string.Format("{0,40} | {1,30} | {2,30} | {3,30} | {4,50}", "PAT ID", "Name", "Scope", "Valid Until", "Token Value");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // create the PAT and display it
                    Objects.PatToken patToken = await Utilities.PatUtils.createPAT(credential, url);
                    Console.WriteLine("{0,40} | {1,30} | {2,30} | {3,30} | {4,50}", patToken.authID, patToken.displayName, patToken.scope, patToken.validTo, patToken.tokenContent);
                    Console.WriteLine("");



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
