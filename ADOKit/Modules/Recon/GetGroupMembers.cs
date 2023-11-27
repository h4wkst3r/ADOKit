using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace ADOKit.Modules.Recon
{
    class GetGroupMembers
    {

        public static async Task execute(string credential, string url, string groupName)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("getgroupmembers", credential, url, "", "", groupName, ""));

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
                    string tableHeader = string.Format("{0,70} | {1,50} | {2,50}", "Group", "Mail Address", "Display Name");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get a listing of all groups in the Azure DevOps instance
                    List<Objects.Group> groupList = await Utilities.GroupUtils.getAllGroups(credential, url);

                    // iterate through the list of groups
                    foreach (Objects.Group group in groupList)
                    {
                        // get any groups that match what is being searched for
                        if (group.displayName.ToLower().Contains(groupName.ToLower()))
                        {

                            // get group member list based on the group descriptor
                            List<Objects.GroupMember> groupMemberList = await Utilities.GroupUtils.getGroupMembers(credential, url, group.descriptor);

                            // go through each group member in the group and list them
                            foreach (Objects.GroupMember member in groupMemberList)
                            {
                                Console.WriteLine("{0,70} | {1,50} | {2,50}", group.principalName, member.mailAddress, member.displayName);
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
