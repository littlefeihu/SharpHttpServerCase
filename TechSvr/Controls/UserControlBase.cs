using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TechSvr.Utils;

namespace TechSvr.Controls
{
    public class UserControlBase : UserControl
    {

        /// <summary>
        /// 文件日志
        /// </summary>
        public Log FileLog
        {
            get
            {
                return LogFactory.GetLogger(this.GetType());
            }
        }
    }
}
