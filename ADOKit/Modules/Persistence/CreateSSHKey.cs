using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace ADOKit.Modules.Persistence
{
    class CreateSSHKey
    {

        public static async Task execute(string credential, string url, string sshKey)
        {

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("createsshkey", credential, url, "", "", "", ""));

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
                    string tableHeader = string.Format("{0,40} | {1,30} | {2,30} | {3,30} | {4,25}", "SSH Key ID", "Name", "Scope", "Valid Until", "Public SSH Key");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get the org ID
                    string orgID = await Utilities.WebUtils.getOrgID(credential, url);

                    // create the SSH Key and display it
                    Objects.SshKey theKey = await Utilities.SshKeyUtils.createSSHKey(credential, url, sshKey, orgID);
                    Console.WriteLine("{0,40} | {1,30} | {2,30} | {3,30} | {4,25}", theKey.authID, theKey.displayName, theKey.scope, theKey.validTo, "..." + theKey.keyContent.Substring(theKey.keyContent.Length - 20, 20));
                    Console.WriteLine("");



                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Unable to add SSH key: " + ex.Message);
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
