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
    class ListBuildLogs
    {
        public static async Task execute(string credential, string url, string project)
        {
            // Generate module header
            Console.WriteLine(ArgUtils.GenerateHeader("listbuildlogs", credential, url, "", project, "", ""));

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

                    // if user wants to list build logs for all projects
                    if (project.ToLower().Equals("all"))
                    {
                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,10} | {2,30} | {3,10} | {4,50}", "Project", "Build ID", "Build Name", "Num Logs", "URL");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all projects in the Azure DevOps instance
                        List<Objects.Project> projectList = await ProjectUtils.getAllProjects(credential, url);

                        // iterate through the list of projects and list all builds for each project
                        foreach (Objects.Project proj in projectList)
                        {
                            List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, proj.projectName);

                            // in each project, get the list of builds (each pipeline run)
                            foreach (Objects.Build build in buildList)
                            {
                                List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, proj.projectName, build.ID);
                                Console.WriteLine("{0,30} | {1,10} | {2,30} | {3,10} | {4,50}", proj.projectName, build.ID, build.buildDefinitionName, logList.Count, build.URL);

                            }

                        }

                    }
                    // if user wants to only list build logs for a certain project
                    else
                    {
                        // create table header
                        string tableHeader = string.Format("{0,10} | {1,30} | {2,10} | {3,50}", "Build ID", "Build Name", "Num Logs", "URL");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get the list of builds (each pipeline run)
                        List<Objects.Build> buildList = await BuildUtils.getBuilds(credential, url, project);

                        foreach (Objects.Build build in buildList)
                        {
                            List<Objects.BuildLog> logList = await BuildUtils.getBuildLogs(credential, url, project, build.ID);
                            Console.WriteLine("{0,10} | {1,30} | {2,10} | {3,50}", build.ID, build.buildDefinitionName, logList.Count, build.URL);
                        }

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
