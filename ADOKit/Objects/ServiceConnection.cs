using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{
    class ServiceConnection
    {

        public string name { get; set; }
        public string type { get; set; }
        public string id { get; set; }

        public ServiceConnection(string name, string type, string id)
        {
            this.name = name;
            this.type = type;
            this.id = id;
        }
    }
}
