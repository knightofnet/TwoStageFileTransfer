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
        private readonly FileInfo _normalFile;

        private readonly FileInfo _tempFile;

        public bool IsNormalFile { get; set; }

        public FileInfo File => IsNormalFile ? _normalFile : _tempFile;

        public string FileName => IsNormalFile ? _normalFile.Name : _tempFile.Name;

        public string FullName => IsNormalFile ? _normalFile.FullName : _tempFile.FullName;

        public String TempName { get; private set; }

        public AppFile(string directory, string filename)
        {
            IsNormalFile = false;
            _normalFile = new FileInfo(Path.Combine(directory, filename));
            _tempFile = new FileInfo(Path.Combine(directory, "~" + filename));
        }

        public FileInfo GetNormalFileInfo()
        {
            return _normalFile;
        }

        public FileInfo GetTempFileInfo()
        {
            return _tempFile;
        }


        public void MoveToNormal()
        {
            _tempFile.MoveTo(_normalFile.FullName);
        }
    }
}
