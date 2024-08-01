using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Recon
{
    class GetTeamMembers
    {

        public static async Task execute(string credential, string url, string search)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("getteammembers", credential, url, search, "", "", ""));

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
                    string tableHeader = string.Format("{0,50} | {1,50} | {2,50}", "Team Name", "Username", "User Display Name");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get a listing of all teams in the Azure DevOps instance
                    List<Objects.Team> teamList = await Utilities.TeamUtils.getAllTeams(credential, url);

                    // iterate through the list of teams and list them
                    foreach (Objects.Team team in teamList)
                    {
                        // if team name matches the team we are looking for
                        if (team.name.ToLower().Contains(search.ToLower()))
                        {

                            List<Objects.TeamMember> teamMemberList = await Utilities.TeamUtils.getTeamMembers(credential, url, team.projID, team.id);

                            // display each team member
                            foreach(Objects.TeamMember member in teamMemberList)
                            {
                                Console.WriteLine("{0,50} | {1,50} | {2,50}", team.name, member.uniqueName, member.displayName);
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
