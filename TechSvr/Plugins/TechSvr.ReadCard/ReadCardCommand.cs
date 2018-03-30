using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.ReadCard
{
    public class ReadCardCommand : ICommand
    {
        public string Name { get { return "readcard"; } }

        public ResposeMessage Excute(InputArgs input)
        {

            string resultmsg = "";
            try
            {
                string dllname = "Lis_ReadICCard";
                string inputContext = input.Data;
                string filepathdel = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "ReadCard");
                IntPtr filepathdelptr = Marshal.StringToHGlobalAnsi(filepathdel);
                IntPtr dllnameptr = Marshal.StringToHGlobalAnsi(dllname);
                IntPtr inputcontentptr = Marshal.StringToHGlobalAnsi(inputContext);

                IntPtr resultptr = DllTransfer.CommonMethodD(filepathdelptr, dllnameptr, inputcontentptr);
                resultmsg = Marshal.PtrToStringAnsi(resultptr);

                var pationt = resultmsg.ToObject<Patient>();
                return new ResposeMessage
                {
                    type = ResultType.SUCCESS.ToString(),
                    message = resultmsg,
                    messageCode = MessageCode.information.ToString(),
                    data = pationt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


        }
    }
}
