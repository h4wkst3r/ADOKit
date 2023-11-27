using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class PatToken
    {

        public string displayName { get; set; }
        public string validTo { get; set; }
        public string scope { get; set; }
        public string tokenContent { get; set; }
        public string authID { get; set; }
        public string isValid { get; set; }


        public PatToken(string authID,string displayName, string validTo, string scope, string tokenContent, string isValid)
        {
            this.authID = authID;
            this.displayName = displayName;
            this.validTo = validTo;
            this.scope = scope;
            this.tokenContent = tokenContent;
            this.isValid = isValid;
        }

    }
}
