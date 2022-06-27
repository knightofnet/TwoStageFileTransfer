using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using TwoStageFileTransferCore.constant;
using TwoStageFileTransferCore.dto;
using TwoStageFileTransferCore.exceptions;

namespace TwoStageFileTransferCore.utils
{
    public static class CommonAppUtils
    {
        public static Uri ToUri(this string str)
        {
            return new Uri(str);
        }

        public static string GetTransferSpeed(long localBytesRead, DateTime timeStart)
        {
            long diffTime = (long)(DateTime.Now - timeStart).TotalSeconds;
            if (diffTime == 0) return string.Empty;

            return "~" + AryxDevLibrary.utils.FileUtils.HumanReadableSize(localBytesRead / diffTime) + "/s [last part]";
        }


        public static TsftFile DecryptTsft(AppArgs appArgs, string tsftFilePath, bool isThrowException = true)
        {
            try
            {
                if (appArgs.TsftPassphrase == null)
                {
                    throw new Exception("Passphrase not set");
                }

                String configFile = File.ReadAllText(tsftFilePath, Encoding.UTF8);
                configFile = StringCipher.Decrypt(configFile, appArgs.TsftPassphrase);

                TsftFile tsftFile;
                using (TextReader reader = new StringReader(configFile))
                {
                    tsftFile = (TsftFile)new XmlSerializer(typeof(TsftFile)).Deserialize(reader);
                }

                return tsftFile;
            }
            catch (CryptographicException ex)
            {
                if (isThrowException)
                {
                    throw new CommonAppException(
                        $"Can't decrypt TSFT {appArgs.Source}. Check the input. If not, you can enter it " +
                        $"with the input parameter -{CmdArgsOptions.OptTsftFilePassPhrase.ShortOpt}.", ex, CommonAppExceptReason.ErrorPreparingTreatment);
                }

                return null;
            }
        }

        public static T CreateInstance<T>(string nextPageName)
        {
            Type t = Type.GetType(nextPageName);
            return (T) Activator.CreateInstance(t);
        }
    }
}
