using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class SshKey
    {

        public string displayName { get; set; }
        public string validTo { get; set; }
        public string scope { get; set; }
        public string keyContent { get; set; }
        public string authID { get; set; }
        public string isValid { get; set; }


        public SshKey(string authID, string displayName, string validTo, string scope, string keyContent, string isValid)
        {
            this.authID = authID;
            this.displayName = displayName;
            this.validTo = validTo;
            this.scope = scope;
            this.keyContent = keyContent;
            this.isValid = isValid;
        }

    }
}
