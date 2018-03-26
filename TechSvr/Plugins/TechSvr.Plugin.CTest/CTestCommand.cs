using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.CTest
{
    public class CTestCommand : ICommand
    {
        public string Name
        {
            get { return "CTest"; }
        }

        public ResposeMessage Excute(InputArgs input)
        {
            string resultmsg = "";

            string dllname = "com_PrintReport";
            string inputContext = "";
            IntPtr filepathptr = Marshal.StringToHGlobalAnsi(Directory.GetCurrentDirectory() + "\\" + "CPPTransferModule");
            IntPtr dllnameptr = Marshal.StringToHGlobalAnsi(dllname);
            IntPtr paramptr = Marshal.StringToHGlobalAnsi(inputContext);

            IntPtr resultcontent = DllTransfer.CommonMethodC(filepathptr, dllnameptr, paramptr);
            string resultstr = Marshal.PtrToStringAnsi(resultcontent);


            return new ResposeMessage
            {
                type = ResultType.SUCCESS.ToString(),
                message = resultmsg,
                messageCode = MessageCode.information.ToString(),
                data = ""
            };
        }
    }
}
