using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    // this class is meant to hold Repo objects
    class Repo
    {

            public string repoName { get; set; }
            public string repoURL { get; set; }

            public Repo(string repoName, string repoURL)
            {
                this.repoName = repoName;
                this.repoURL = repoURL;
            }

        
    }
}
