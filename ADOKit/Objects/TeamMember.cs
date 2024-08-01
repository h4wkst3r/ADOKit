using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class TeamMember
    {

        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }


        public TeamMember(string id, string displayName, string uniqueName)
        {
            this.id = id;
            this.displayName = displayName;
            this.uniqueName = uniqueName;


        }

    }
}
