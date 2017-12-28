using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechSvr.Utils;

namespace TechSvr.Plugin.GetMachineInfo
{
    public class GetMachineInfoCommand : ICommand
    {
        public string Name { get { return "computerinfo"; } }

        public string Excute(string input)
        {
            return "cpu 64";
        }
    }
}
