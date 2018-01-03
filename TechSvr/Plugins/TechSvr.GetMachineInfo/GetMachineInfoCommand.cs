using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechSvr.Utils;
using TechSvr.Utils.DTO;

namespace TechSvr.Plugin.GetMachineInfo
{
    public class GetMachineInfoCommand : ICommand
    {
        public string Name { get { return "Computer"; } }

        public string Excute(InputArgs input)
        {
            return new CmdResult
            {
                RESULTCODE = 1,
                MESSAGE = "成功获取机器信息",
                DATA = new
                {
                    COMPUTERNAME = MachineInfoHelper.GetComputerName(),
                    IPADDRESS = MachineInfoHelper.GetUserIP(),
                    MACADDRESS = MachineInfoHelper.GetMAC(),
                    CPU = MachineInfoHelper.GetCPUID()
                }
            }.ToJson();
        }
    }
}
