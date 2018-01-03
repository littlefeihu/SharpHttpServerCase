using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TechSvr.Utils
{
    public class DirectoryManage
    {
        public static string GetINIFullPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.INI_FileName);
        }

        /// <summary>
        /// 根据SysType获取FRX的文件路径
        /// </summary>
        /// <param name="frxName"></param>
        /// <param name="SysType"></param>
        /// <returns></returns>
        public static string GetFrxFullPath(string frxName, string SysType)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Plugins\FastPrint");
            if (!string.IsNullOrEmpty(SysType))
            {
                dir = Path.Combine(dir, SysType, "Template");
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.Combine(dir, frxName);
        }
    }
}
