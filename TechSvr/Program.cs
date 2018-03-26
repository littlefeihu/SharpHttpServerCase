using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TechSvr.ModuleForm;
using TechSvr.Utils;

namespace TechSvr
{
    static class Program
    {

        private static string _appLibsDir = "";
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
            //程序集解析事件
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Boolean createdNew;
            Mutex instance = new Mutex(true, "TechSvr", out createdNew);
            if (createdNew)
            {
                TechSvrApplication.Instance.StartUp();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
              
                _appLibsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Lib\");

                AddEnvironmentPaths(_appLibsDir);
                var mainform = new TechSvrForm(args);
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
                TechSvrApplication.Instance.Log(ex.Message);
            }

            MessageBox.Show("系统出现未知异常");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                TechSvrApplication.Instance.Log(ex.Message);
            }

            MessageBox.Show("系统出现未知异常");

        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null, objExecutingAssemblies = null;
            objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            foreach (AssemblyName assmblyName in arrReferencedAssmbNames)
            {
                if (assmblyName.FullName.Substring(0, assmblyName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(",")))
                {
                    string path = System.IO.Path.Combine(_appLibsDir, args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll");
                    assembly = Assembly.LoadFrom(path);
                    break;
                }
            }
            return assembly;
        }

        /// <summary>
        /// 添加DLLImport的DLL
        /// </summary>
        /// <param name="paths"></param>
        static void AddEnvironmentPaths(params string[] paths)
        {
            var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty };

            string newPath = string.Join(Path.PathSeparator.ToString(), path.Concat(paths));

            Environment.SetEnvironmentVariable("PATH", newPath);
        }


    }
}
