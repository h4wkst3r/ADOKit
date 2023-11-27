using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    // this is a class to represent results of code objects
    class CodeResult
    {

        public string filePath { get; set; }
        public string projectName { get; set; }
        public string projectID { get; set; }
        public string repoName { get; set; }
        public string repoID { get; set; }

        public string fullURL { get; set; }

        public string codeContents { get; set; }


        public CodeResult(string filePath, string projectName, string projectID, string repoName, string repoID, string fullURL, string codeContents)
        {
            this.filePath = filePath;
            this.projectName = projectName;
            this.projectID = projectID;
            this.repoName = repoName;
            this.repoID = repoID;
            this.fullURL = fullURL;
            this.codeContents = codeContents;

        }
    }
}
