using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.dto.transfer;
using TwoStageFileTransferCore.exceptions;
using TwoStageFileTransferCore.utils;
using AFileUtils = AryxDevLibrary.utils.FileUtils;

namespace TwoStageFileTransferCore.business.transfer
{
    public abstract class AbstractInWork : AbstractWork
    {
        protected long TotalBytesToRead { get; set; }
        protected long TotalBytesRead;

        public int LastPartDone { get; set; }


        public InWorkOptions InWorkOptions { get; }

        protected AbstractInWork(InWorkOptions inWorkOptions)
        {
            InWorkOptions = inWorkOptions;
        }

        public abstract void DoTransfert(IProgressTransfer reporter);

        protected abstract long CalculatePartFileMaxLenght();

        protected abstract void TestFilesNotAlreadyExist(FileInfo source, string target, long chunkSize,
            bool exceptionIfExists = false);

        protected abstract void IncludeMoreThingsInTsftFile(TsftFile tsftFile);


        protected TsftFileSecured GetTransferExchangeFileContent(string sourceName, long sourceLength, long partFileMaxLenght,
            string sha1, string inPassphrase = null, Action<TsftFile> moreActionOnTsft = null)
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

                string passPhrase = inPassphrase;
                return new TsftFileSecured()
                { PassPhrase = passPhrase, SecureContent = StringCipher.Encrypt(xml, passPhrase) };
            }
            catch (Exception ex)
            {
                _log.Error($"Error while creating TSFT file: {ex.Message}");
                throw new CommonAppException($"Error while creating TSFT file: {ex.Message}", ex, CommonAppExceptReason.ErrorInStage);
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
                throw new CommonAppException($"Error when testing or deleting already existing files: {ex.Message}", ex,
                    CommonAppExceptReason.ErrorInStage);
            }
        }

    }
}
