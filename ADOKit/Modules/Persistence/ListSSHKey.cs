using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Persistence
{
    class ListSSHKey
    {

        public static async Task execute(string credential, string url)
        {

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("listsshkey", credential, url, "", "", "", ""));

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

                    // get a listing of all SSH keys for the user being authenticated as
                    List<Objects.SshKey> sshKeyList = await Utilities.SshKeyUtils.getUserSSHKeys(credential, url);


                    // iterate through the list of SSH keys and display info about them
                    foreach (Objects.SshKey key in sshKeyList)
                    {

                        Console.WriteLine("{0,40} | {1,30} | {2,30} | {3,30} | {4,25}", key.authID, key.displayName, key.scope, key.validTo, "..." + key.keyContent.Substring(key.keyContent.Length - 20, 20));

                    }

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
