using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TechSvr.Utils;

namespace TechSvr.Controls
{
    public partial class LogControl : UserControlBase
    {
        public LogControl()
        {
            InitializeComponent();
            TechSvrApplication.Instance.LogOutput += Instance_LogOutput;
        }

        int logCount = 0;
        private void Instance_LogOutput(string msg)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                if (logCount >= TechSvrApplication.Instance.MaxRecordCount)
                {
                    //日志记录超过规定的最大值则 清空日志
                    logCount = 0;
                    ClearLog();
                }
                this.txtLog.Text = DateTime.Now.ToString() + "," + msg + "\r\n" + this.txtLog.Text;
                logCount += 1;

            }));

        }

        public void ClearLog()
        {
            this.txtLog.Text = "";
        }
    }
}
