using System;
using System.IO;
using TwoStageFileTransfer.business;

namespace TwoStageFileTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            
            // args = new string[3] {"in", @"C:\Users\ARyx\Desktop\Badger2018-08-06am.7z", @"C:\Users\ARyx\Desktop\destination"};
            // args = new string[3] {"out", @"C:\Users\ARyx\Desktop\destination\Badger2018-08-06am.7z.29445130.part0", @"C:\Users\ARyx\Desktop\destination\final"};
            // args = new string[3] { "out", @"C:\Users\ARyx\Desktop\destination", @"C:\Users\ARyx\Desktop\destination\final" };

            if (args[0] == "in" && args.Length == 3)
            {
                Console.WriteLine("Mode 1");

                InToOutWork w = new InToOutWork();
                w.FileInput = new FileInfo(args[1]);
                w.Target = args[2];

                w.DoTransfert(20 * 1024 * 1024);





            } else if (args[0] == "out" && args.Length == 3)
            {

                Console.WriteLine("Mode 2");

                OutToFileWork o = new OutToFileWork();
                o.FirstFile = new FileInfo(args[1]);
                o.Target = args[2];

                o.DoTransfert();

            }


        }
    }
}
