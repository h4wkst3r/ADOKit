﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Privesc
{
    class GetPipelineVars
    {

        public static async Task execute(string credential, string url, string project)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("getpipelinevars", credential, url, "", project, "", ""));

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


                    // if user wants to dump pipeline variables for all projects
                    if (project.ToLower().Equals("all"))
                    {

                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,30} | {2,50}", "Project Name", "Pipeline Var Name", "Pipeline Var Value");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all projects in the Azure DevOps instance
                        List<Objects.Project> projectList = await Utilities.ProjectUtils.getAllProjects(credential, url);

                        // iterate through the list of projects
                        foreach (Objects.Project proj in projectList)
                        {

                            // get a listing of all build definition URLs for all pipelines within a project
                            List<string> buildDefinitionURLs = await Utilities.PipelineUtils.getBuildDefinitionURLs(credential, url, proj.projectName);

                            // go through each build definition URL in the project and extract any variables from them
                            foreach (string buildUrl in buildDefinitionURLs)
                            {

                                List<Objects.BuildVariable> variables = await Utilities.PipelineUtils.getBuildVars(credential, buildUrl);
                                foreach (Objects.BuildVariable var in variables)
                                {
                                    Console.WriteLine("{0,30} | {1,30} | {2,50}", proj.projectName, var.name, var.value);
                                }
                            }

                        }

                        Console.WriteLine("");


                    }

                    // if user wants to only dump pipeline variables for a certain project
                    else
                    {
                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,50}", "Pipeline Var Name", "Pipeline Var Value");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all build definition URLs for all pipelines within a project
                        List<string> buildDefinitionURLs = await Utilities.PipelineUtils.getBuildDefinitionURLs(credential, url, project);

                        // go through each build definition URL in the project and extract any variables from them
                        foreach (string buildUrl in buildDefinitionURLs)
                        {

                            List<Objects.BuildVariable> variables = await Utilities.PipelineUtils.getBuildVars(credential, buildUrl);
                            foreach (Objects.BuildVariable var in variables)
                            {
                                Console.WriteLine("{0,30} | {1,50}", var.name, var.value);
                            }
                        }

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
