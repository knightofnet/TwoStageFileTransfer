using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AryxDevLibrary.utils;

namespace TwoStageFileTransferGUI.utils
{
    internal static class AppUtils
    {
        public static string GetValidFilepathFromClipboard()
        {
            
            string fileName = null;

            try
            {
                if (Clipboard.ContainsFileDropList())
                {
                    var filesArray = Clipboard.GetFileDropList();
                    fileName = filesArray[0];
                }
                else if (Clipboard.ContainsText())
                {
                    string rawFileName = Clipboard.GetText();
                    if (FileUtils.IsAFile(rawFileName))
                    {
                        fileName = rawFileName;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO (nothing)
            }

            return fileName;
        }
    }
}
