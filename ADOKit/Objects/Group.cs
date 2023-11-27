using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class Group
    {
        public string principalName { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string descriptor { get; set; }


        public Group(string principalName, string displayName, string description, string descriptor)
        {
            this.principalName = principalName;
            this.displayName = displayName;
            this.description = description;
            this.descriptor = descriptor;
        }

    }
}
