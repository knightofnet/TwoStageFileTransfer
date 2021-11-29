using System;
using System.IO;
using TwoStageFileTransfer.business;
using TwoStageFileTransfer.dto;
using TwoStageFileTransfer.utils;

namespace TwoStageFileTransfer
{
    class Program
    {
        static void Main(string[] args)
        {

            AppArgsParser argsParser = new AppArgsParser();
            AppArgs appArgs = argsParser.ParseDirect(args);

            // args = new string[3] {"in", @"C:\Users\ARyx\Desktop\Badger2018-08-06am.7z", @"C:\Users\ARyx\Desktop\destination"};
            // args = new string[3] {"out", @"C:\Users\ARyx\Desktop\destination\Badger2018-08-06am.7z.29445130.part0", @"C:\Users\ARyx\Desktop\destination\final"};
            // args = new string[3] { "out", @"C:\Users\ARyx\Desktop\destination", @"C:\Users\ARyx\Desktop\destination\final" };

            Console.WriteLine("Source: {0}", appArgs.Source);
            Console.WriteLine("Target: {0}", appArgs.Target);

            Console.WriteLine();

            if (appArgs.Direction == "IN")
            {
                Console.WriteLine("Mode IN");

                InToOutWork w = new InToOutWork();
                w.Source = new FileInfo(appArgs.Source);
                w.Target = appArgs.Target;
                w.BufferSize = appArgs.BufferSize;

                w.MaxTransfertLength = (long) (FileUtils.GetAvailableSpace(w.Target, 20 * 1024 * 1024) * 0.9);
                Console.WriteLine("Max size used: {0}", AryxDevLibrary.utils.FileUtils.HumanReadableSize(w.MaxTransfertLength));

                w.DoTransfert();

            } else if (appArgs.Direction == "OUT")
            {

                Console.WriteLine("Mode OUT");

                OutToFileWork o = new OutToFileWork();
                o.Source = new FileInfo(appArgs.Source);
                o.Target = appArgs.Target;
                o.BufferSize = appArgs.BufferSize;

                o.DoTransfert();

            }


        }
    }
}
