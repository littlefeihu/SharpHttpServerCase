using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TechSvr.Utils;

namespace TechSvr.Plugin.CTest
{
    public class CTestCommand : ICommand
    {
        public string Name
        {
            get { return "CTest"; }
        }

        public string Excute(InputArgs input)
        {
            string resultmsg = "";
            try
            {

                string dllname = "com_PrintReport";
                string inputContext = "";
                IntPtr filepathptr = Marshal.StringToHGlobalAnsi(Directory.GetCurrentDirectory() + "\\" + "CPPTransferModule");
                IntPtr dllnameptr = Marshal.StringToHGlobalAnsi(dllname);
                IntPtr paramptr = Marshal.StringToHGlobalAnsi(inputContext);

                IntPtr resultcontent = DllTransfer.CommonMethodC(filepathptr, dllnameptr, paramptr);
                string resultstr = Marshal.PtrToStringAnsi(resultcontent);

            }
            catch (Exception ex)
            {

                throw;
            }
            return resultmsg;
        }
    }
}
