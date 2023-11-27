using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Recon
{
    class SearchCode
    {

        public static async Task execute(string credential, string url, string search)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("searchcode", credential, url, search, "", "", ""));

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
                    List<Objects.CodeResult> codeList = await Utilities.CodeUtils.searchCode(credential, url, search);

                    // go through all the matching code items, and print the line that matched including a full URL to the code
                    int matchCount = 0;
                    foreach (Objects.CodeResult result in codeList)
                    {
                        string codeSnippetMatching = Utilities.CodeUtils.getMatchingCodeSnippet(result.codeContents, search);
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
