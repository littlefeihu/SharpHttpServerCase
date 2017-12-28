using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TechSvr.Utils;

namespace TechSvr.Plugin.DelphiPrint
{
    public class DelphiPrintCommand : ICommand
    {
        public string Name
        {
            get { return "DelphiPrint"; }
        }

        public string Excute(string input)
        {
            string resultmsg = "";
            try
            {
                string dllname = "com_PrintReport";
                string inputContext = input;
                string filepathdel = Path.Combine(Directory.GetCurrentDirectory(), @"Plugins\DelphiPrint\");
                IntPtr filepathdelptr = Marshal.StringToHGlobalAnsi(filepathdel);
                IntPtr dllnameptr = Marshal.StringToHGlobalAnsi(dllname);
                IntPtr inputcontentptr = Marshal.StringToHGlobalAnsi(inputContext);//需要传入的信息

                IntPtr resultptr = DllTransfer.CommonMethodD(filepathdelptr, dllnameptr, inputcontentptr);//Delphi中接口入参形式
                resultmsg = Marshal.PtrToStringAnsi(resultptr);//dll返回信息
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            return resultmsg;
        }
    }
}
