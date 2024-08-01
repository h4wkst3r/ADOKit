using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Recon
{
    class Creds
    {
        // cred search query and array for all the terms to be searched
        private static string credSearchString = "pw OR pwd OR passwrod OR password OR \\\"-----BEGIN PGP PRIVATE KEY BLOCK-----\\\" OR \\\"-----BEGIN EC PRIVATE KEY-----\\\" OR \\\"-----BEGIN DSA PRIVATE KEY-----\\\" OR \\\"-----BEGIN OPENSSH PRIVATE KEY-----\\\" OR \\\"-----BEGIN RSA PRIVATE KEY-----\\\" OR ANSIBLE_VAULT OR AWS_ACCESS_KEY_ID OR AWS_SECRET_ACCESS_KEY OR ACCESS_TOKEN OR API_KEY OR Authorization OR db_password";
        private static string[] credSearchStringArray = { "pwd", "pw", "passwrod", "password", "-----BEGIN PGP PRIVATE KEY BLOCK-----", "-----BEGIN EC PRIVATE KEY-----", "-----BEGIN DSA PRIVATE KEY-----", "-----BEGIN OPENSSH PRIVATE KEY-----", "-----BEGIN RSA PRIVATE KEY-----", "ANSIBLE_VAULT", "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY", "ACCESS_TOKEN", "API_KEY", "Authorization", "db_password" };


        public static async Task execute(string credential, string url)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("creds", credential, url, credSearchString, "", "", ""));

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
                    // the list to hold the code search results
                    List<Objects.CodeResult> codeList = await Utilities.CodeUtils.searchCode(credential, url, credSearchString);

                    // go through all the matching code items, and print the line that matched including a full URL to the code
                    int matchCount = 0;
                    foreach (Objects.CodeResult result in codeList)
                    {
                        // go through search cred string that is being searched from the results
                        foreach (string searchItem in credSearchStringArray)
                        {
                            string codeSnippetMatching = Utilities.CodeUtils.getMatchingCodeSnippet(result.codeContents, searchItem);
                            if (codeSnippetMatching != "")
                            {
                                Console.WriteLine("\n[>] URL: " + result.fullURL);

                                // get each of the matching lines and print them
                                string[] codeContentLines = codeSnippetMatching.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                                foreach (string matchItem in codeContentLines)
                                {
                                    if (matchItem != "")
                                    {
                                        Console.WriteLine("    |_ " + matchItem.Trim());
                                        matchCount++;
                                    }
                                }



                            }

                        }                    

                    }

                    Console.WriteLine("");
                    Console.WriteLine("[*] Match count : " + matchCount);
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
