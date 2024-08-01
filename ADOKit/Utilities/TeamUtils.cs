using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using ADOKit.Objects;

namespace ADOKit.Utilities
{
    class TeamUtils
    {

        // get a list of all teams
        public static async Task<List<Team>> getAllTeams(string credentials, string url)
        {
            // this is the list of teams to return
            List<Team> teamList = new List<Team>();

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // parse the JSON output and display results
            JsonTextReader jsonResult;

            string propName = "";
            string name = "";
            string description = "";
            string project = "";
            string id = "";
            string projID = "";


            try
            {

                // web request to get all teams
                string contToken = "";
                string content = "";

                HttpWebRequest webRequest = null;

                // loop until we don't have a continuationToken in the response. if we do, fetch more 
                do
                {
                    content = ""; // empty our buffer 

                    if (contToken != "")
                    {
                        webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/teams?api-version=7.2-preview.3&continuationToken=" + contToken);
                    }
                    else
                    {
                        webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/teams?api-version=7.2-preview.3");
                    }

                    if (webRequest != null)
                    {

                        // set header values
                        webRequest.Method = "GET";
                        webRequest.ContentType = "application/json";
                        webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                        // if cookie was provided
                        if (credentials.ToLower().Contains("userauthentication="))
                        {
                            webRequest.Headers.Add("Cookie", "AadAuthenticationSet=false; " + credentials);
                        }
                        // otherwise PAT was provided
                        else
                        {
                            webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                        }



                        // get web response
                        HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                        var reader = new StreamReader(myWebResponse.GetResponseStream());
                        content += reader.ReadToEnd();

                        // if there's a continuationToken header we need to send the request to get
                        // a bit more ... and a bit more until there isn't a continuation token 

                        contToken = "";
                        // get the X-ms-continuationtoken header value
                        for (int i = 0; i < myWebResponse.Headers.Count; i++)
                        {

                            // grab the header value
                            if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-ms-continuationtoken"))
                            {
                                contToken = myWebResponse.Headers[i];
                            }
                        }

                    }

                    // parse the JSON output and display results
                    jsonResult = new JsonTextReader(new StringReader(content));

                    propName = "";
                    name = "";
                    description = "";
                    project = "";
                    id = "";
                    projID = "";


                    // read the json results 
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the team to the list if it has all the needed attributes
                                if (name != "" && description != "" && project != "" && id != "" && projID != "")
                                {
                                    if (!doesTeamAlreadyExistInOurList(id, teamList))
                                    {
                                        teamList.Add(new Team(id, name, description, project, projID));

                                    }
                                    name = "";
                                    description = "";
                                    project = "";
                                    id = "";
                                }
                                break;
                            case "StartArray":
                                break;
                            case "EndArray":
                                break;
                            case "PropertyName":
                                propName = jsonResult.Value.ToString();
                                break;
                            case "String":

                                // grab the team id
                                if (propName.ToLower().Equals("id"))
                                {
                                    id = jsonResult.Value.ToString();

                                }

                                // grab the project id
                                if (propName.ToLower().Equals("projectid"))
                                {
                                    projID = jsonResult.Value.ToString();

                                }

                                // grab the name
                                if (propName.ToLower().Equals("name"))
                                {
                                    name = jsonResult.Value.ToString();
                                }

                                // grab the description
                                if (propName.ToLower().Equals("description"))
                                {
                                    description = jsonResult.Value.ToString();
                                }

                                // grab the project
                                if (propName.ToLower().Equals("projectname"))
                                {
                                    project = jsonResult.Value.ToString();
                                }

                                break;
                            case "Boolean":
                                break;
                            case "Date":
                                break;
                            default:
                                break;

                        }

                    }
                } while (contToken != "");

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("[-] ERROR: " + ex.StackTrace);
                Console.WriteLine("");
            }


            return teamList;
        }


        // get a list of team members for a given team
        public static async Task<List<TeamMember>> getTeamMembers(string credentials, string url, string projectID, string teamID)
        {
            // this is the list of team members to return
            List<TeamMember> teamMemberList = new List<TeamMember>();

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // parse the JSON output and display results
            JsonTextReader jsonResult;

            string propName = "";
            string id = "";
            string displayName = "";
            string uniqueName = "";


            try
            {

                // web request to get all team members for a given team
                string contToken = "";
                string content = "";

                HttpWebRequest webRequest = null;

                // loop until we don't have a continuationToken in the response. if we do, fetch more 
                do
                {
                    content = ""; // empty our buffer 

                    if (contToken != "")
                    {
                        webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/projects/" + projectID + "/teams/" + teamID + "/members?api-version=7.2-preview.2&continuationToken=" + contToken);
                    }
                    else
                    {
                        webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/_apis/projects/" + projectID + "/teams/" + teamID + "/members?api-version=7.2-preview.2");
                    }

                    if (webRequest != null)
                    {

                        // set header values
                        webRequest.Method = "GET";
                        webRequest.ContentType = "application/json";
                        webRequest.UserAgent = "ADOKit-21e233d4334f9703d1a3a42b6e2efd38";

                        // if cookie was provided
                        if (credentials.ToLower().Contains("userauthentication="))
                        {
                            webRequest.Headers.Add("Cookie", "AadAuthenticationSet=false; " + credentials);
                        }
                        // otherwise PAT was provided
                        else
                        {
                            webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(":" + credentials)));
                        }



                        // get web response
                        HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();

                        var reader = new StreamReader(myWebResponse.GetResponseStream());
                        content += reader.ReadToEnd();

                        // if there's a continuationToken header we need to send the request to get
                        // a bit more ... and a bit more until there isn't a continuation token 

                        contToken = "";
                        // get the X-ms-continuationtoken header value
                        for (int i = 0; i < myWebResponse.Headers.Count; i++)
                        {

                            // grab the header value
                            if (myWebResponse.Headers.Keys[i].ToString().ToLower().Equals("x-ms-continuationtoken"))
                            {
                                contToken = myWebResponse.Headers[i];
                            }
                        }

                    }

                    // parse the JSON output and display results
                    jsonResult = new JsonTextReader(new StringReader(content));

                    propName = "";
                    id = "";
                    displayName = "";
                    uniqueName = "";


                    // read the json results 
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the team member to the list if it has all the needed attributes
                                if (id != "" && displayName != "" && uniqueName != "")
                                {
                                    if (!doesTeamMemberAlreadyExistInOurList(id, teamMemberList))
                                    {
                                        teamMemberList.Add(new TeamMember(id, displayName, uniqueName));

                                    }
                                    id = "";
                                    displayName = "";
                                    uniqueName = "";
                                }
                                break;
                            case "StartArray":
                                break;
                            case "EndArray":
                                break;
                            case "PropertyName":
                                propName = jsonResult.Value.ToString();
                                break;
                            case "String":

                                // grab the team member id
                                if (propName.ToLower().Equals("id"))
                                {
                                    id = jsonResult.Value.ToString();

                                }

                                // grab the display name
                                if (propName.ToLower().Equals("displayname"))
                                {
                                    displayName = jsonResult.Value.ToString();

                                }

                                // grab the unique name
                                if (propName.ToLower().Equals("uniquename"))
                                {
                                    uniqueName = jsonResult.Value.ToString();
                                }

                                break;
                            case "Boolean":
                                break;
                            case "Date":
                                break;
                            default:
                                break;

                        }

                    }
                } while (contToken != "");

            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("[-] ERROR: " + ex.StackTrace);
                Console.WriteLine("");
            }


            return teamMemberList;
        }


        // determine whether we already have a team in our list by the given unique id
        public static bool doesTeamAlreadyExistInOurList(string id, List<Team> teamList)
        {
            bool doesItExist = false;

            foreach (Objects.Team team in teamList)
            {
                if (team.id.ToLower().Equals(id.ToLower()))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }


        // determine whether we already have a team member in our list by the given unique id
        public static bool doesTeamMemberAlreadyExistInOurList(string id, List<TeamMember> teamMemberList)
        {
            bool doesItExist = false;

            foreach (Objects.TeamMember member in teamMemberList)
            {
                if (member.id.ToLower().Equals(id.ToLower()))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }


    }
}
