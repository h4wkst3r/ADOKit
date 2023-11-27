using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOKit.Objects
{

    // this class is meant to hold File objects
    class File
    {

        public string filePath { get; set; }
        public string fileURL { get; set; }

        public File(string filePath, string fileURL)
        {
            this.filePath = filePath;
            this.fileURL = fileURL;
        }

    }
}
