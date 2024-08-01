using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;
using ADOKit.Utilities;
using System.Threading;
using System.Net.Http;

namespace ADOKit.Modules.Recon
{
    class SearchBuildLogs
    {

        public static async Task execute(string credential, string url, string project, string search)
        {
            // Generate module header
            Console.WriteLine(ArgUtils.GenerateHeader("searchbuildlogs", credential, url, search, project, "", ""));

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // check if credentials provided are valid
            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (await WebUtils.credsValid(credential, url))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");

                try
                {
                    // count the number of build logs and matches
                    int totalBuildLogs = 0;
                    int matchCount = 0;

                    // if user wants to search build logs for all projects
                    if (project.ToLower().Equals("all"))
                    {
                        // get a listing of all projects in the Azure DevOps instance
                        List<Objects.Project> projectList = await ProjectUtils.getAllProjects(credential, url);

                        // iterate through the list of projects and get all builds for each project
                        Console.WriteLine("[*] INFO: Searching build logs");
                        foreach (Objects.Project proj in projectList)
                        {
                            List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, proj.projectName);

                            // in each project, get the list of builds (each pipeline run)
                            foreach (Objects.Build build in buildList)
                            {
                                List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, proj.projectName, build.ID);
                                totalBuildLogs += logList.Count;

                                // finally, for each log, grab the content and search for your search string
                                foreach (Objects.BuildLog log in logList)
                                {

                                    // get the log content
                                    string logContent = await BuildUtils.getLogFileContent(credential, proj.projectName, log);

                                    // look for matching snipper
                                    string codeSnippetMatching = Utilities.CodeUtils.getMatchingCodeSnippet(logContent, search);
                                    if (codeSnippetMatching != "")
                                    {
                                        Console.WriteLine("\n\n[>] URL: " + log.logURL);
                                        Console.WriteLine("[>] Project: " + proj.projectName);

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

                        }

                    }
                    // if user wants to only search build logs for a certain project
                    else
                    {

                        // get the list of builds (each pipeline run)
                        List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, project);

                        Console.WriteLine("[*] INFO: Searching build logs");
                        foreach (Objects.Build build in buildList)
                        {
                            List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, project, build.ID);
                            totalBuildLogs += logList.Count;

                            // finally, for each log, download it and store in the output folder                        
                            foreach (Objects.BuildLog log in logList)
                            {
                                string logContent = await BuildUtils.getLogFileContent(credential, project, log);


                                // look for matching snipper
                                string codeSnippetMatching = Utilities.CodeUtils.getMatchingCodeSnippet(logContent, search);
                                if (codeSnippetMatching != "")
                                {
                                    Console.WriteLine("\n\n[>] URL: " + log.logURL);
                                    Console.WriteLine("[>] Project: " + project);

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
