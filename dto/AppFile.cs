using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoStageFileTransfer.dto
{
    class AppFile
    {
     
        public FileInfo File { get; }

        public FileInfo TempFile { get; }


        public AppFile(string directory, string filename)
        {
            File = new FileInfo(Path.Combine(directory, filename));
            TempFile = new FileInfo(Path.Combine(directory, "~" + filename));
        }
        

        public void MoveToNormal()
        {
            TempFile.MoveTo(File.FullName);
        }
    }
}
