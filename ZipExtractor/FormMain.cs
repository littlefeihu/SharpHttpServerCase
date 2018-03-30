using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor
{
    public partial class FormMain : Form
    {
        private BackgroundWorker _backgroundWorker;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 3)
            {
                //解压升级包的临时目录
                var tempDirectory = args[1];
                ///主程序完整路径
                var main_exeFullName = args[2];
                //检查是否需要重新启动
                var needRestart = "true".Equals(args[3], StringComparison.InvariantCultureIgnoreCase);
                if (needRestart)
                {
                    foreach (var process in Process.GetProcesses())
                    {
                        try
                        {
                            if (process.MainModule.FileName.Equals(main_exeFullName))
                            {
                                labelInformation.Text = @"Waiting for application to Exit...";
                                process.WaitForExit();
                            }
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                    }
                }

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
                    try
                    {
                        var path = Path.GetDirectoryName(main_exeFullName);

                        #region 更新之前备份TechSVR的程序文件
                        var bakDirectory = Path.Combine(path, Constants.Bak_TechSvr + DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒ffff"));
                        if (!Directory.Exists(bakDirectory))
                        {
                            Directory.CreateDirectory(bakDirectory);

                            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                            {
                                if (!file.Contains(Constants.Bak_TechSvr))
                                {

                                    var destFile = file.Replace(path, bakDirectory);
                                    var destDirectory = Path.GetDirectoryName(destFile);
                                    if (!Directory.Exists(destDirectory))
                                    {
                                        Directory.CreateDirectory(destDirectory);
                                    }
                                    File.Copy(file, destFile, true);
                                }
                            }
                        }
                        #endregion

                        #region 解压升级包到主程序的安装目录
                        // Open an existing zip file for reading.
                        using (ZipStorer zip = ZipStorer.Open(tempDirectory, FileAccess.Read))
                        {
                            // Read the central directory collection.
                            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                            for (var index = 0; index < dir.Count; index++)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    eventArgs.Cancel = true;
                                    zip.Close();
                                    return;
                                }
                                ZipStorer.ZipFileEntry entry = dir[index];
                                zip.ExtractFile(entry, Path.Combine(path, entry.FilenameInZip));
                                _backgroundWorker.ReportProgress((index + 1) * 100 / dir.Count, string.Format(Resources.CurrentFileExtracting, entry.FilenameInZip));
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("安装出错," + ex.ToString());
                    }
                };

                _backgroundWorker.ProgressChanged += (o, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    labelInformation.Text = eventArgs.UserState.ToString();
                };

                _backgroundWorker.RunWorkerCompleted += (o, eventArgs) =>
                {
                    if (!eventArgs.Cancelled)
                    {
                        labelInformation.Text = @"Finished";
                        try
                        {   //如果需要重新启动主程序，则由此开始启动程序
                            if (needRestart)
                            {
                                ProcessStartInfo processStartInfo = new ProcessStartInfo(main_exeFullName);
                                if (args.Length > 4)
                                {
                                    processStartInfo.Arguments = args[4];
                                }
                                Process.Start(processStartInfo);
                            }
                        }
                        catch (Win32Exception exception)
                        {
                            if (exception.NativeErrorCode != 1223)
                                throw;
                        }
                        Application.Exit();
                    }
                };
                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();
        }
    }
}
