using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AryxDevLibrary.utils;
using TwoStageFileTransfer.constant;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.exceptions;
using TwoStageFileTransfer.utils;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.utils;
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

        protected abstract void TestFilesNotAlreadyExist(FileInfo source, string target, long chunkSize,
            bool exceptionIfExists = false);

        protected abstract void IncludeMoreThingsInTsftFile(TsftFile tsftFile);


        protected TsftFileSecured GetTransferExchangeFileContent(string sourceName, long sourceLength, long partFileMaxLenght,
            string sha1, string inPassphrase = null,  Action<TsftFile> moreActionOnTsft = null)
        {
            try
            {
                TsftFile tsftFile = new TsftFile()
                {
                    FileLenght = sourceLength,
                    Sha1Hash = sha1
                };
                tsftFile.Source.OriginalDirectory = InWorkOptions.Source.Directory.FullName;
                tsftFile.Source.OriginalFilename = sourceName;

                tsftFile.TempDir.Type = TransferTypes.Windows;
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

                string passPhrase = inPassphrase ;
                return new TsftFileSecured()
                    { PassPhrase = passPhrase, SecureContent = StringCipher.Encrypt(xml, passPhrase) };
            }
            catch (Exception ex)
            {
                _log.Error($"Error while creating TSFT file: {ex.Message}");
                throw new AppException($"Error while creating TSFT file: {ex.Message}", ex, EnumExitCodes.KO_IN);
            }


        }

        protected void MainTestFilesNotAlreadyExist(FileInfo source, string target, long chunkSize,
            bool exceptionIfExists = false)
        {
            try
            {
                TestFilesNotAlreadyExist(source, target, chunkSize, exceptionIfExists);
            }
            catch (Exception ex)
            {
                throw new AppException($"Error when testing or deleting already existing files: {ex.Message}", ex,
                    EnumExitCodes.KO_IN);
            }
        }

    }
}
