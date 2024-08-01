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
    class GetBuildLogs
    {
        public static async Task execute(string credential, string url, string project)
        {
            // Generate module header
            Console.WriteLine(ArgUtils.GenerateHeader("getbuildlogs", credential, url, "", project, "", ""));

            // set outfolder to our current directory
            string outfolder = Environment.CurrentDirectory + "\\ADOKit-" + Utilities.FileUtils.generateRandomName();
            Directory.CreateDirectory(outfolder);

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

                    // if user wants to dump build logs for all projects
                    if (project.ToLower().Equals("all"))
                    {

                        // get a listing of all projects in the Azure DevOps instance
                        List<Objects.Project> projectList = await ProjectUtils.getAllProjects(credential, url);

                        // iterate through the list of projects and get all builds for each project
                        Console.WriteLine("[*] INFO: Downloading build logs to: " + outfolder);
                        Console.WriteLine("");
                        foreach (Objects.Project proj in projectList)
                        {
                            List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, proj.projectName);

                            // in each project, get the list of builds (each pipeline run)
                            foreach (Objects.Build build in buildList)
                            {
                                List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, proj.projectName, build.ID);
                                totalBuildLogs += logList.Count;

                                // finally, for each log, download it and store in the output folder
                                foreach (Objects.BuildLog log in logList)
                                {

                                    await BuildUtils.downloadLog(credential, proj.projectName, outfolder, log);

                                }

                            }

                        }

                    }
                    // if user wants to only dump build logs for a certain project
                    else
                    {

                        // get the list of builds (each pipeline run)
                        List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, project);

                        Console.WriteLine("[*] INFO: Downloading build logs to: " + outfolder);
                        Console.WriteLine("");
                        foreach (Objects.Build build in buildList)
                        {
                            List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, project, build.ID);
                            totalBuildLogs += logList.Count;

                            // finally, for each log, download it and store in the output folder                        
                            foreach (Objects.BuildLog log in logList)
                            {
                                await BuildUtils.downloadLog(credential, project, outfolder, log);

                            }

                        }

                    }

                    Console.WriteLine("[+] SUCCESS: Build log files downloaded to: " + outfolder);
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
