using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechSvr.Utils
{
    public interface ICommand
    {
        /// <summary>
        /// 命令执行
        /// </summary>
        /// <param name="input">入参为string类型，格式为：XML或JSON</param>
        /// <returns></returns>
        string Excute(string input);

        string Name { get; }
    }
}
