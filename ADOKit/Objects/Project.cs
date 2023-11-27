using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{

    // this class is meant to hold Project objects
    class Project
    {

        public string projectName { get; set; }
        public string projectURL { get; set; }
        public string visibility { get; set; }

        public Project(string projectName, string projectURL, string visibility)
        {
            this.projectName = projectName;
            this.projectURL = projectURL;
            this.visibility = visibility;
        }



    }
}
