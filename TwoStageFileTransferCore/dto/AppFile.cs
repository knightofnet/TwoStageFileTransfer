using System.IO;

namespace TwoStageFileTransferCore.dto
{
    public class AppFile
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
