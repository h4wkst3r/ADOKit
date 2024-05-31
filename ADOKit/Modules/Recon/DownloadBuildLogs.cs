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
    class DownloadBuildLogs
    {
        public static async Task execute(string credential, string url, string project, string outfolder, bool reallyDownload)
        {
            // Generate module header
            Console.WriteLine(ArgUtils.GenerateHeader("downloadbuildlogs", credential, url, "", project, "", ""));

            // Verifies that the output folder exists
            if (!Directory.Exists(outfolder + "/"))
            {
                throw new DirectoryNotFoundException($"The folder '{outfolder}' does not exist.");
            }

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
                    // count the number of build logs and lines
                    int totalBuildLogs = 0;
                    int totalLines = 0;

                    // if user wants to dump build logs for all projects
                    if (project.ToLower().Equals("all"))
                    {

                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,10} | {2,30} | {3,10} | {4,50}", "Project", "Build ID", "Build def. name", "# logs", "URL");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all projects in the Azure DevOps instance
                        List<Objects.Project> projectList = await ProjectUtils.getAllProjects(credential, url);

                        // iterate through the list of projects and get all builds for each project
                        foreach (Objects.Project proj in projectList)
                        {
                            List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, proj.projectName);

                            // in each project, get the list of builds (each pipeline run)
                            foreach (Objects.Build build in buildList)
                            {
                                List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, proj.projectName, build.ID);
                                Console.WriteLine("{0,30} | {1,10} | {2,30} | {3,10} | {4,50}", proj.projectName, build.ID, build.buildDefinitionName, logList.Count, build.URL);
                                totalBuildLogs += logList.Count;

                                // finally, for each log, download it and store in the output folder
                                foreach (Objects.BuildLog log in logList)
                                {
                                    if (reallyDownload)
                                    {
                                        await BuildUtils.downloadLog(credential, proj.projectName, outfolder, log);
                                    }
                                    else
                                    {
                                        totalLines += log.lineCount;
                                    }
                                }

                            }

                        }

                    }
                    // if user wants to only dump build logs for a certain project
                    else
                    {
                        // create table header
                        string tableHeader = string.Format("{0,10} | {1,30} | {2,10} | {3,50}", "Build ID", "Build def. name", "# logs", "URL");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get the list of builds (each pipeline run)
                        List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, project);
                        foreach (Objects.Build build in buildList)
                        {
                            List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, project, build.ID);
                            Console.WriteLine("{0,10} | {1,30} | {2,10} | {3,50}", build.ID, build.buildDefinitionName, logList.Count, build.URL);
                            totalBuildLogs += logList.Count;

                            // finally, for each log, download it and store in the output folder
                            foreach (Objects.BuildLog log in logList)
                            {
                                if (reallyDownload)
                                {
                                    await BuildUtils.downloadLog(credential, project, outfolder, log);
                                }
                                else
                                {
                                    totalLines += log.lineCount;
                                }
                            }

                        }

                    }

                    Console.WriteLine("");
                    if (!reallyDownload)
                    {
                        Console.WriteLine("[!] WARNING: If you really want to download the logs, use the /reallydownload flag.");
                        Console.WriteLine($"[!] WARNING: This will download {totalBuildLogs} log files totalling {totalLines} lines of log.");
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
