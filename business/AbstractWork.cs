using AryxDevLibrary.utils.logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer.business
{
    abstract class AbstractWork
    {
        protected static Logger _log = Logger.LastLoggerInstance ;

        public FileInfo Source { get; internal set; }
        public string Target { get; internal set; }

        public int BufferSize { get; internal set; }


        public static string CalculculateSourceSha1(FileInfo file, string compSha1 = null)
        {
            Console.Write("Calculate SHA1... ", _log);
            _log.Info("Calculate SHA1");
            DateTime start = DateTime.Now;

            string sha1 = FileUtils.GetSha1Hash(file);
            Console.WriteLine("Done.");



            if (compSha1 != null)
            {
                LogUtils.WriteConsole(string.Format("SHA1 match: {0}", sha1.ToUpper().Equals(compSha1?.ToUpper()) ? "OK" : "KO"), _log);
            }

            TimeSpan duration = DateTime.Now - start;
            _log.Info("Calculate Sha1 > Done ({0})", duration.ToString("hh\\:mm\\:ss\\.ffff"));

            return sha1;
        }
    }
}
