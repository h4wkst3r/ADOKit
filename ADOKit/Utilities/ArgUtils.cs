using System;
using System.Collections.Generic;

namespace ADOKit.Utilities
{
    /// <summary>
    /// Simple class to parse command line arguments
    /// </summary>
    internal static class ArgUtils
    {

        private const char _VALUE_SEPARATOR = ':';

        /**
        * Parse the arguments
        * 
        * */
        public static Dictionary<string, string> ParseArguments(IEnumerable<string> args)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string arg in args)
            {
  
                string[] parts = arg.Split(new char[] { _VALUE_SEPARATOR }, 2);
                if (parts.Length == 2)
                {
                    result[parts[0].ToLower().Substring(1)] = parts[1];
                }
                else
                {
                    result[parts[0].ToLower().Substring(1)] = "";
                }
            }
            return result;
        }

        /**
        * Generate module header
        * 
        * */
        public static string GenerateHeader(string module, string credential, string url, string searchTerm, string project, string group, string user)
        {
            string output = String.Empty;
            string delim = "==================================================";
            string authType = "";

            if (credential.ToLower().StartsWith("userauthentication="))
            {
                authType = "User Authentication Cookie";

            }
            else if (credential.ToLower().StartsWith("eyj0"))
            {
                authType = "Azure Access Token";

            }
            else
            {
                authType = "API Key";
            }

            output += "\n" + delim + "\n";
            output += "Module:\t\t" + module + "\n";
            output += "Auth Type:\t" + authType + "\n";
            if (searchTerm != "")
            {
                output += "Search Term:\t" + searchTerm + "\n";
            }
            if (project != "")
            {
                output += "Project:\t" + project + "\n";
            }
            if (group != "")
            {
                output += "Group:\t" + group + "\n";
            }
            if (user != "")
            {
                output += "User:\t" + user + "\n";
            }
            output += "Target URL:\t" + url + "\n\n";
            output += "Timestamp:\t" + DateTime.Now + "\n";
            output += delim + "\n";

            return output;
        }


        /**
        * print help
        * 
        * */
        public static void HelpMe()
        {
            Console.Write("\nPlease read the README page for proper usage of the tool.\n\n");


        } // end print help method


    }
}
