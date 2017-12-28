using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechSvr.Utils;

namespace TechSvr.Plugin.ReadCard
{
    public class ReadCardCommand : ICommand
    {
        public string Name { get { return "readcard"; } }

        public string Excute(string input)
        {
            return "readcard";
        }
    }
}
