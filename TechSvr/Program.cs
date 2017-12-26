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

            Boolean createdNew;
            Mutex instance = new Mutex(true, "TechSvr", out createdNew);
            if (createdNew)
            {
                TechSvrApplication.Instance.StartUp();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var mainform = new TechSvrForm(args);
                mainform.Run();

                var printfrm = new PrintForm();
                printfrm.IsMdiContainer = true;
                printfrm.WindowState = FormWindowState.Maximized;
                printfrm.TopMost = true;

                TechSvrApplication.Instance.SetPrintForm(printfrm);

                Application.Run(mainform);
                instance.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("已经启动了一个程序");
                Application.Exit();
            }

        }
    }
}
