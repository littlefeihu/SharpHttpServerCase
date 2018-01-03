using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechSvr.Utils.DTO
{
    /// <summary>
    /// 命令执行结果
    /// </summary>
    public class CmdResult
    {
        /// <summary>
        /// 接口执行标志 0-失败 1-成功
        /// </summary>
        public int RESULTCODE { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MESSAGE { get; set; }
        /// <summary>
        /// 插件执行返回数据
        /// </summary>
        public object DATA { get; set; }

    }
}
