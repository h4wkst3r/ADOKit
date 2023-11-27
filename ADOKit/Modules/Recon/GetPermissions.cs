using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Recon
{
    class GetPermissions
    {
        public static async Task execute(string credential, string url, string project)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("getpermissions", credential, url, "", project, "", ""));

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
                    string tableHeader = string.Format("{0,50} | {1,50} | {2,50}", "UPN", "Display Name", "Description");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get a listing of groups that have permissions to a given project
                    List<Objects.Group> groupList = await Utilities.GroupUtils.getGroupPermissionsForProject(credential, url, project);

                    // iterate through the list of groups and list them
                    foreach (Objects.Group group in groupList)
                    {

                        Console.WriteLine("{0,50} | {1,50} | {2,50}", group.principalName, group.displayName, group.description);

                    }

                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Listing group members for each group that has permissions to this project");
                    Console.WriteLine("");

                    // list all group members for each group
                    foreach (Objects.Group group in groupList)
                    {

                        Console.WriteLine("");
                        Console.WriteLine("");
                        Console.WriteLine("GROUP NAME: " + group.principalName);
                        Console.WriteLine("");

                        // get group member list based on the group descriptor
                        List<Objects.GroupMember> groupMemberList = await Utilities.GroupUtils.getGroupMembers(credential, url, group.descriptor);

                        // create table header for listing group members
                        string tableHeaderMembers = string.Format("{0,70} | {1,50} | {2,50}", "Group", "Mail Address", "Display Name");
                        Console.WriteLine(tableHeaderMembers);
                        Console.WriteLine(new String('-', tableHeaderMembers.Length));

                        // go through each group member in the group and list them
                        foreach (Objects.GroupMember member in groupMemberList)
                        {
                            Console.WriteLine("{0,70} | {1,50} | {2,50}", group.principalName, member.mailAddress, member.displayName);
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
