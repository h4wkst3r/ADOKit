using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class GroupMember
    {

        public string mailAddress { get; set; }
        public string displayName { get; set; }



        public GroupMember(string mailAddress, string displayName)
        {
            this.mailAddress = mailAddress;
            this.displayName = displayName;
        }

    }
}
