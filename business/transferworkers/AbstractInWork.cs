using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;
using AFileUtils = AryxDevLibrary.utils.FileUtils;

namespace TwoStageFileTransfer.business.transferworkers
{
    internal abstract class AbstractInWork : AbstractWork
    {
        public int LastPartDone { get; set; }

        
        public InWorkOptions InWorkOptions { get; }

        protected AbstractInWork(InWorkOptions inWorkOptions)
        {
            InWorkOptions = inWorkOptions;
        }

        public abstract void DoTransfert();

        protected abstract long CalculatePartFileMaxLenght();


        protected string WriteTransferExchangeFile(string sourceName, long sourceLength, long partFileMaxLenght, string sha1, Action<TsftFile> moreActionOnTsft = null)
        {

            TsftFile tsftFile = new TsftFile()
            {
                FileLenght = sourceLength,
                Sha1Hash = sha1
            };
            tsftFile.Source.OriginalDirectory = InWorkOptions.Source.Directory.FullName;
            tsftFile.Source.OriginalFilename = sourceName;

            tsftFile.TempDir.Type = TransferTypes.WindowsFolder;
            tsftFile.TempDir.Path = InWorkOptions.Target;

            tsftFile.TempDir.RegularPartFileLenght = partFileMaxLenght;
            tsftFile.TempDir.AwaitedParts = (long)Math.Ceiling((double)(sourceLength / partFileMaxLenght));

            moreActionOnTsft?.Invoke(tsftFile);

            XmlSerializer serializer = new XmlSerializer(tsftFile.GetType());
            String xml;
            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings() { Indent = false }))
                {
                    serializer.Serialize(writer, tsftFile);
                    xml = sww.ToString();
                }
            }

            return StringCipher.Encrypt(xml, "test");


        }





    }
}
