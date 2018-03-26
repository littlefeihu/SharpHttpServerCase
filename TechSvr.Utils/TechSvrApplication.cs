using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TechSvr.Utils;

namespace TechSvr.Utils
{
    public class TechSvrApplication
    {
        public static TechSvrApplication Instance = new TechSvrApplication();
        public event Action<string> OutputLog;
        List<ICommand> Commands = new List<ICommand>();
        private TechSvrApplication()
        {

        }

        /// <summary>
        /// 程序启动
        /// </summary>
        public void StartUp()
        {
            LoadPlugIn();
        }

        public void LoadPlugIn()
        {
            Commands.Clear();
            Commands.AddRange(PlugInLoader.Load());
        }


        public void ShowToUI(string msg, string logname = "Log")
        {
            WhiteLog(msg, logname, true);
        }
        public void Log(string msg, string logname = "Log")
        {
            WhiteLog(msg, logname, false);
        }
        private void WhiteLog(string msg, string logname, bool showinApp = true)
        {
            LogFactory.GetLogger(logname).Info(msg);
            if (showinApp)
            {
                if (OutputLog != null)
                    OutputLog(msg);
            }
        }

        /// <summary>
        /// 根据名称获取对应的命令插件
        /// </summary>
        /// <param name="msgtype"></param>
        /// <returns></returns>
        public ICommand GetCommand(string msgtype)
        {
            var cmd = Commands.FirstOrDefault(o => o.Name.Equals(msgtype, StringComparison.CurrentCultureIgnoreCase));

            if (cmd == null)
            {
                #region 第一次为找到 尝试重新加载一次插件列表
                LoadPlugIn();

                cmd = Commands.FirstOrDefault(o => o.Name.Equals(msgtype, StringComparison.CurrentCultureIgnoreCase));
                if (cmd == null)
                {
                    //仍未找到,则抛出异常
                    throw new Exception("对应的插件未找到，msgtype：" + msgtype);
                }
                #endregion
            }

            return cmd;
        }
        public int MaxRecordCount
        {
            get
            {
                var recordCount = INIHelper.ReadInteger(Constants.INI_MAXRecordCount, DirectoryManage.GetINIFullPath());
                if (recordCount == 0)
                {
                    recordCount = 1000;
                }
                return recordCount;
            }
        }


        Form _mainFrm;
        /// <summary>
        /// 打印窗口
        /// </summary>
        public System.Windows.Forms.Form MainFrm
        {
            get
            {
                return _mainFrm;
            }
        }

        /// <summary>
        /// 设置打印窗口
        /// </summary>
        /// <param name="printFrm"></param>
        public void SetMainFrm(Form mainFrm)
        {
            _mainFrm = mainFrm;
        }
    }
}
