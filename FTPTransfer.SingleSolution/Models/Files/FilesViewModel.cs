using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTPTransfer.SingleSolution.Models.Files
{
    public class FileDetails
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public override String ToString()
        {
            return System.IO.Path.Combine(Path, Name);
        }
    }

    public class FilesViewModel
    {
        public List<FileDetails> Files { get; set; }
            = new List<FileDetails>();
    }
}
