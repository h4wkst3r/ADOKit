using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Recon
{
    class SearchFile
    {

        public static async Task execute(string credential, string url, string search)
        {

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("searchfile", credential, url, search, "", "", ""));

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
                    string tableHeader = string.Format("{0,100}", "File URL");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get a listing of all projects in the Azure DevOps instance
                    List<Objects.Project> projectList = await Utilities.ProjectUtils.getAllProjects(credential, url);

                    // iterate through the list of projects and get all repos for each project
                    foreach (Objects.Project proj in projectList)
                    {
                        // the project URL returned from the projects API will be a GUID, so we need to translate that to the URL of the actual repo
                        string projectURL = await Utilities.ProjectUtils.translateGUIProjectURL(credential, proj.projectURL);

                        // get a list of all repos for a given project
                        List<string> repos = await Utilities.RepoUtils.getAllReposWithGUIDURL(credential, projectURL);

                        // get all the repos for the project
                        foreach (string repo in repos)
                        {

                            // get all files in repo and if any match based on file name being searched, display the path to file name
                            List<Objects.File> fileList = await Utilities.FileUtils.getAllFilesInRepo(credential, repo);

                            foreach (Objects.File file in fileList)
                            {
                                if (file.filePath.ToLower().Contains(search.ToLower()))
                                {
                                    Console.WriteLine(file.fileURL);

                                }

                            }

                        }

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
