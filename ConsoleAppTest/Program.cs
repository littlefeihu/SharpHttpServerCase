using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"D:\合肥研发中心\040 LIS\03 LIS6.0\70 技术探究\TechSvr\TechSvr\bin\Debug";
            var bakDirectory = Path.Combine(path, "Bak_TechSvr" + DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒ffff"));
            Directory.CreateDirectory(bakDirectory);

            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (!file.Contains("Bak_TechSvr"))
                {
                    var destFile = file.Replace(path, bakDirectory);
                    var destDirectory = Path.GetDirectoryName(destFile);
                    if (!Directory.Exists(destDirectory))
                    {
                        Directory.CreateDirectory(destDirectory);
                    }
                    Console.WriteLine(file);
                    File.Copy(file, destFile, true);
                }
            }

            Console.ReadKey();
        }
    }
}
