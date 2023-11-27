using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Privesc
{
    class GetServiceConnections
    {

        public static async Task execute(string credential, string url, string project)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("getserviceconnections", credential, url, "", project, "", ""));

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


                    // if user wants to get service connections for all projects
                    if (project.ToLower().Equals("all"))
                    {

                        // create table header
                        string tableHeader = string.Format("{0,25} | {1,60} | {2,20} | {3,60}", "Project", "Connection Name", "Connection Type", "ID");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all projects in the Azure DevOps instance
                        List<Objects.Project> projectList = await Utilities.ProjectUtils.getAllProjects(credential, url);

                        // iterate through the list of projects
                        foreach (Objects.Project proj in projectList)
                        {

                            // get all service connections for a given project
                            List<Objects.ServiceConnection> serviceConnections = await Utilities.ServiceConnectionUtils.getServiceConnectionsForProject(credential, url, proj.projectName);


                            // go through each service connection and print info
                            foreach (Objects.ServiceConnection connection in serviceConnections)
                            {

                                Console.WriteLine("{0,25} | {1,60} | {2,20} | {3,60}", proj.projectName, connection.name, connection.type, connection.id);

                            }



                        }

                        Console.WriteLine("");


                    }

                    // if user wants to only get service connections for a certain project
                    else
                    {
                        // create table header
                        string tableHeader = string.Format("{0,60} | {1,20} | {2,60}", "Connection Name", "Connection Type", "ID");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get all service connections for a given project
                        List<Objects.ServiceConnection> serviceConnections = await Utilities.ServiceConnectionUtils.getServiceConnectionsForProject(credential, url, project);

                        // go through each service connection and print info
                        foreach (Objects.ServiceConnection connection in serviceConnections)
                        {

                            Console.WriteLine("{0,60} | {1,20} | {2,60}", connection.name, connection.type, connection.id);

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
