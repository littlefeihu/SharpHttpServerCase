using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.GetMachineInfo
{
    public class GetMachineInfoCommand : ICommand
    {
        static GetMachineInfoCommand()
        {
            InitMachineInfo();
        }
        public string Name { get { return "Computer"; } }

        private static object MachineInfo { get; set; }

        private static void InitMachineInfo()
        {
            MachineInfo = new
            {
                COMPUTERNAME = MachineInfoHelper.GetComputerName(),
                IPADDRESS = MachineInfoHelper.GetUserIP(),
                MACADDRESS = MachineInfoHelper.GetMAC(),
                CPU = MachineInfoHelper.GetCPUID()
            };
        }
        public ResposeMessage Excute(InputArgs input)
        {
            return new ResposeMessage
            {
                type = ResultType.SUCCESS.ToString(),
                message = "成功获取机器信息",
                data = MachineInfo,
                messageCode = MessageCode.information.ToString(),
            };
        }
    }
}
