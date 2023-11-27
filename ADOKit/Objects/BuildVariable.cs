using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class BuildVariable
    {

        public string name { get; set; }
        public string value { get; set; }

        public BuildVariable(string name, string value)
        {
            this.name = name;
            this.value = value;
        }


    }
}
