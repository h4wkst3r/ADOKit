using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class Team
    {

        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string project { get; set; }
        public string projID { get; set; }


        public Team(string id, string name, string description, string project, string projID)
        {

            this.id = id;
            this.name = name;
            this.description = description;
            this.project = project;
            this.projID = projID;
 
        }

    }
}
