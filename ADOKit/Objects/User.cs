using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class User
    {

        public string directoryAlias { get; set; }
        public string displayName { get; set; }
        public string principalName { get; set; }

        public string descriptor { get; set; }


        public User(string directoryAlias, string displayName, string principalName, string descriptor)
        {
            this.directoryAlias = directoryAlias;
            this.displayName = displayName;
            this.principalName = principalName;
            this.descriptor = descriptor;
        }


    }
}
