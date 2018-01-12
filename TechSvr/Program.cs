using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TechSvr.ModuleForm;
using TechSvr.Utils;

namespace TechSvr
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //处理未捕获的异常   
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常   
            Application.ThreadException += Application_ThreadException;
            //处理非UI线程异常   
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Boolean createdNew;
            Mutex instance = new Mutex(true, "TechSvr", out createdNew);
            if (createdNew)
            {
                TechSvrApplication.Instance.StartUp();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var mainform = new TechSvrForm(args);
                mainform.Run();
                Application.Run(mainform);
                instance.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("已经启动了一个程序");
                Application.Exit();
            }

        }
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            if (ex != null)
            {
                TechSvrApplication.Instance.WhiteLog(ex.Message);
            }

            MessageBox.Show("系统出现未知异常");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                TechSvrApplication.Instance.WhiteLog(ex.Message);
            }

            MessageBox.Show("系统出现未知异常");

        }

    }
}
