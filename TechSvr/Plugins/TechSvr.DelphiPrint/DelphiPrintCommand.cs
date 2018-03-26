using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.DelphiPrint
{
    public class DelphiPrintCommand : ICommand
    {
        public string Name
        {
            get { return "DelphiPrint"; }
        }

        public ResposeMessage Excute(InputArgs input)
        {
            string resultmsg = "";
            try
            {
                string dllname = "com_PrintReport";
                string inputContext = input.Data;
                string filepathdel = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "DelphiPrint");
                IntPtr filepathdelptr = Marshal.StringToHGlobalAnsi(filepathdel);
                IntPtr dllnameptr = Marshal.StringToHGlobalAnsi(dllname);
                IntPtr inputcontentptr = Marshal.StringToHGlobalAnsi(inputContext);

                IntPtr resultptr = DllTransfer.CommonMethodD(filepathdelptr, dllnameptr, inputcontentptr);
                resultmsg = Marshal.PtrToStringAnsi(resultptr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
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
