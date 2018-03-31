using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using AutoUpdaterDotNET.Properties;
using AutoUpdaterDotNET.Util;
using Microsoft.Win32;

namespace AutoUpdaterDotNET
{
    public static class AutoUpdater
    {

        #region 局部变量

        private static System.Timers.Timer _remindLaterTimer;

        internal static String ChangelogURL;

        internal static String DownloadURL;

        internal static String InstallerArgs;

        internal static String RegistryLocation;

        internal static String Checksum;

        internal static String HashingAlgorithm;

        internal static Version CurrentVersion;

        internal static Version InstalledVersion;

        internal static bool IsWinFormsApplication;

        internal static bool Running;

        /// <summary>
        /// 是否需要重启
        /// </summary>
        internal static bool NeedRestart = false;

        #endregion


        #region 公共属性
        /// <summary>
        /// 检查服务端是否有更新之后，触发该事件
        /// </summary>
        public static Action<bool> UpdateChanged;
        /// <summary>
        ///   下载路径
        /// </summary>
        public static String DownloadPath;

        /// <summary>
        ///  主程序标题
        /// </summary>
        public static String AppTitle;

        /// <summary>
        ///   XML地址
        /// </summary>
        public static String AppCastURL;

        /// <summary>
        ///   是否打开下载页面
        /// </summary>
        public static bool OpenDownloadPage;

        /// <summary>
        ///   显示跳过按钮
        /// </summary>
        public static Boolean ShowSkipButton = true;

        /// <summary>
        ///  显示稍后提醒按钮
        /// </summary>
        public static Boolean ShowRemindLaterButton = true;

        /// <summary>
        ///   设置稍后提醒信息
        /// </summary>
        public static Boolean LetUserSelectRemindLater = true;

        /// <summary>
        ///     Remind Later interval after user should be reminded of update.
        /// </summary>
        public static int RemindLaterAt = 2;

        ///<summary>
        ///     AutoUpdater.NET will report errors if this is true.
        /// </summary>
        public static bool ReportErrors = false;

        /// <summary>
        ///   是否需要管理员权限
        /// </summary>
        public static bool RunUpdateAsAdmin = true;

        ///<summary>
        ///   强制更新（不显示UpdateForm更新提醒窗口，直接下载更新包进行更新）
        /// </summary>
        public static bool Mandatory;

        /// <summary>
        /// 网络代理
        /// </summary>
        public static WebProxy Proxy;

        /// <summary>
        ///   稍后提醒间隔
        /// </summary>
        public static RemindLaterFormat RemindLaterTimeSpan = RemindLaterFormat.Days;

        /// <summary>
        ///  主程序退出事件
        /// </summary>
        public delegate void ApplicationExitEventHandler();

        /// <summary>
        ///   主程序退出事件
        /// </summary>
        public static event ApplicationExitEventHandler ApplicationExitEvent;

        /// <summary>
        ///   检查更新委托
        /// </summary>
        /// <param name="args"></param>
        public delegate void CheckForUpdateEventHandler(UpdateInfoEventArgs args);

        /// <summary>
        ///    检查更新事件
        /// </summary>
        public static event CheckForUpdateEventHandler CheckForUpdateEvent;

        /// <summary>
        /// 服务器XML解析委托
        /// </summary>
        /// <param name="args"></param>
        public delegate void ParseUpdateInfoHandler(ParseUpdateInfoEventArgs args);

        /// <summary>
        /// 服务器XML解析事件
        /// </summary>
        public static event ParseUpdateInfoHandler ParseUpdateInfoEvent;

        #endregion

        /// <summary>
        /// 开始检查服务端是否有更新可用
        /// </summary>
        /// <param name="myAssembly">当前主程序.</param>
        public static void Start(Assembly myAssembly = null)
        {
            Start(AppCastURL, myAssembly);
        }

        /// <summary>
        ///开始检查服务端是否有更新可用
        /// </summary>
        /// <param name="appCast">服务器XML文件路径</param>
        /// <param name="myAssembly">.</param>
        public static void Start(String appCast, Assembly myAssembly = null)
        {
            if (Mandatory && _remindLaterTimer != null)
            {
                _remindLaterTimer.Stop();
                _remindLaterTimer.Close();
                _remindLaterTimer = null;
            }
            if (!Running && _remindLaterTimer == null)
            {
                Running = true;

                AppCastURL = appCast;

                IsWinFormsApplication = Application.MessageLoop;

                var backgroundWorker = new BackgroundWorker();

                backgroundWorker.DoWork += BackgroundWorkerDoWork;

                backgroundWorker.RunWorkerCompleted += BackgroundWorkerOnRunWorkerCompleted;

                backgroundWorker.RunWorkerAsync(myAssembly ?? Assembly.GetEntryAssembly());
            }
        }

        private static void BackgroundWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (!runWorkerCompletedEventArgs.Cancelled)
            {
                if (runWorkerCompletedEventArgs.Result is DateTime)
                {
                    SetTimer((DateTime)runWorkerCompletedEventArgs.Result);
                }
                else
                {

                    var args = runWorkerCompletedEventArgs.Result as UpdateInfoEventArgs;
                    if (CheckForUpdateEvent != null)
                    {
                        CheckForUpdateEvent(args);
                    }
                    else
                    {
                        if (args != null)
                        {
                            if (args.IsUpdateAvailable)
                            {
                                if (!IsWinFormsApplication)
                                {
                                    Application.EnableVisualStyles();
                                }
                                if (Thread.CurrentThread.GetApartmentState().Equals(ApartmentState.STA))
                                {
                                    ShowUpdateForm();
                                }
                                else
                                {
                                    Thread thread = new Thread(ShowUpdateForm);
                                    thread.CurrentCulture = thread.CurrentUICulture = CultureInfo.CurrentCulture;
                                    thread.SetApartmentState(ApartmentState.STA);
                                    thread.Start();
                                    thread.Join();
                                }
                                return;
                            }
                            else
                            {
                                if (ReportErrors)
                                {
                                    MessageBox.Show(Resources.UpdateUnavailableMessage, Resources.UpdateUnavailableCaption,
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                        else
                        {
                            if (ReportErrors)
                            {
                                MessageBox.Show(
                                    Resources.UpdateCheckFailedMessage,
                                    Resources.UpdateCheckFailedCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            Running = false;
        }


        private static void ShowUpdateForm()
        {

            if (Mandatory)
            {
                if (DownloadUpdate())
                {
                    if (NeedRestart)
                    {
                        Exit();
                    }
                }
            }
            else
            {
                var updateForm = new UpdateForm();
                if (updateForm.ShowDialog().Equals(DialogResult.OK))
                {
                    if (NeedRestart)
                    {
                        Exit();
                    }
                }
            }
        }

        private static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Cancel = true;
            Assembly mainAssembly = e.Argument as Assembly;

            var companyAttribute =
                (AssemblyCompanyAttribute)ApplicationHelper.GetAttribute(mainAssembly, typeof(AssemblyCompanyAttribute));
            if (string.IsNullOrEmpty(AppTitle))
            {
                var titleAttribute =
                    (AssemblyTitleAttribute)ApplicationHelper.GetAttribute(mainAssembly, typeof(AssemblyTitleAttribute));
                AppTitle = titleAttribute != null ? titleAttribute.Title : mainAssembly.GetName().Name;
            }

            string appCompany = companyAttribute != null ? companyAttribute.Company : "";

            RegistryLocation = !string.IsNullOrEmpty(appCompany)
                ? $@"Software\{appCompany}\{AppTitle}\AutoUpdater"
                : $@"Software\{AppTitle}\AutoUpdater";

            //InstalledVersion = mainAssembly.GetName().Version;
            InstalledVersion = ApplicationHelper.GetInstallVersion();
            var webRequest = WebRequest.Create(AppCastURL);
            webRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            if (Proxy != null)
            {
                webRequest.Proxy = Proxy;
            }
            WebResponse webResponse;

            try
            {
                webResponse = webRequest.GetResponse();
            }
            catch (Exception)
            {
                e.Cancel = false;
                return;
            }
            UpdateInfoEventArgs args;
            using (Stream appCastStream = webResponse.GetResponseStream())
            {
                if (appCastStream != null)
                {
                    if (ParseUpdateInfoEvent != null)
                    {
                        using (StreamReader streamReader = new StreamReader(appCastStream))
                        {
                            string data = streamReader.ReadToEnd();
                            ParseUpdateInfoEventArgs parseArgs = new ParseUpdateInfoEventArgs(data);
                            ParseUpdateInfoEvent(parseArgs);
                            args = parseArgs.UpdateInfo;
                        }
                    }
                    else
                    {
                        XmlDocument receivedAppCastDocument = new XmlDocument();

                        try
                        {
                            receivedAppCastDocument.Load(appCastStream);

                            XmlNodeList appCastItems = receivedAppCastDocument.SelectNodes("item");

                            args = new UpdateInfoEventArgs();

                            if (appCastItems != null)
                            {
                                foreach (XmlNode item in appCastItems)
                                {
                                    XmlNode appCastVersion = item.SelectSingleNode("version");

                                    try
                                    {
                                        CurrentVersion = new Version(appCastVersion?.InnerText);
                                    }
                                    catch (Exception)
                                    {
                                        CurrentVersion = null;
                                    }

                                    args.CurrentVersion = CurrentVersion;

                                    XmlNode appCastChangeLog = item.SelectSingleNode("changelog");

                                    args.ChangelogURL = appCastChangeLog?.InnerText;

                                    XmlNode appCastUrl = item.SelectSingleNode("url");

                                    args.DownloadURL = appCastUrl?.InnerText;

                                    if (Mandatory.Equals(false))
                                    {
                                        XmlNode mandatory = item.SelectSingleNode("mandatory");

                                        Boolean.TryParse(mandatory?.InnerText, out Mandatory);
                                    }

                                    args.Mandatory = Mandatory;

                                    XmlNode appArgs = item.SelectSingleNode("args");
                                    XmlNode restart = item.SelectSingleNode("restart");
                                    if (restart != null && restart.InnerText.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        NeedRestart = true;
                                    }

                                    args.InstallerArgs = appArgs?.InnerText;

                                    XmlNode checksum = item.SelectSingleNode("checksum");

                                    args.HashingAlgorithm = checksum?.Attributes["algorithm"]?.InnerText;

                                    args.Checksum = checksum?.InnerText;
                                }
                            }
                        }
                        catch (XmlException)
                        {
                            e.Cancel = false;
                            webResponse.Close();
                            return;
                        }
                    }
                }
                else
                {
                    e.Cancel = false;
                    webResponse.Close();
                    return;
                }
            }

            if (args.CurrentVersion == null || string.IsNullOrEmpty(args.DownloadURL))
            {
                webResponse.Close();
                if (ReportErrors)
                {
                    throw new InvalidDataException();
                }
                return;
            }

            CurrentVersion = args.CurrentVersion;
            ChangelogURL = args.ChangelogURL = WebHelper.GetURL(webResponse.ResponseUri, args.ChangelogURL);
            DownloadURL = args.DownloadURL = WebHelper.GetURL(webResponse.ResponseUri, args.DownloadURL);
            Mandatory = args.Mandatory;
            InstallerArgs = args.InstallerArgs ?? String.Empty;
            HashingAlgorithm = args.HashingAlgorithm ?? "MD5";
            Checksum = args.Checksum ?? String.Empty;

            webResponse.Close();

            if (Mandatory)
            {
                ShowRemindLaterButton = false;
                ShowSkipButton = false;
            }
            else
            {
                using (RegistryKey updateKey = Registry.CurrentUser.OpenSubKey(RegistryLocation))
                {
                    if (updateKey != null)
                    {
                        object skip = updateKey.GetValue("skip");
                        object applicationVersion = updateKey.GetValue("version");
                        if (skip != null && applicationVersion != null)
                        {
                            string skipValue = skip.ToString();
                            var skipVersion = new Version(applicationVersion.ToString());
                            if (skipValue.Equals("1") && CurrentVersion <= skipVersion)
                                return;
                            if (CurrentVersion > skipVersion)
                            {
                                using (RegistryKey updateKeyWrite = Registry.CurrentUser.CreateSubKey(RegistryLocation))
                                {
                                    if (updateKeyWrite != null)
                                    {
                                        updateKeyWrite.SetValue("version", CurrentVersion.ToString());
                                        updateKeyWrite.SetValue("skip", 0);
                                    }
                                }
                            }
                        }

                        object remindLaterTime = updateKey.GetValue("remindlater");

                        if (remindLaterTime != null)
                        {
                            DateTime remindLater = Convert.ToDateTime(remindLaterTime.ToString(),
                                CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat);

                            int compareResult = DateTime.Compare(DateTime.Now, remindLater);

                            if (compareResult < 0)
                            {
                                e.Cancel = false;
                                e.Result = remindLater;
                                return;
                            }
                        }
                    }
                }
            }

            args.IsUpdateAvailable = CurrentVersion > InstalledVersion;
            args.InstalledVersion = InstalledVersion;
            if (UpdateChanged != null && !args.IsUpdateAvailable)
            {
                UpdateChanged(args.IsUpdateAvailable);
            }
            e.Cancel = false;
            e.Result = args;
        }

        /// <summary>
        /// 退出当前主程序
        /// </summary>
        private static void Exit()
        {
            if (ApplicationExitEvent != null)
            {
                ApplicationExitEvent();
            }
            else
            {
                var currentProcess = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    string processPath;
                    try
                    {
                        processPath = process.MainModule.FileName;
                    }
                    catch (Win32Exception)
                    {
                        continue;
                    }

                    if (process.Id != currentProcess.Id &&
                        currentProcess.MainModule.FileName == processPath)
                    {
                        if (process.CloseMainWindow())
                        {
                            process.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds);
                        }
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                }

                if (IsWinFormsApplication)
                {
                    Environment.Exit(0);
                }
#if NETWPF
                else if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        System.Windows.Application.Current.Shutdown()));
                }
#endif
                else
                {
                    Environment.Exit(0);
                }
            }
        }



        internal static void SetTimer(DateTime remindLater)
        {
            TimeSpan timeSpan = remindLater - DateTime.Now;

            var context = SynchronizationContext.Current;

            _remindLaterTimer = new System.Timers.Timer
            {
                Interval = (int)timeSpan.TotalMilliseconds,
                AutoReset = false
            };

            _remindLaterTimer.Elapsed += delegate
            {
                _remindLaterTimer = null;
                if (context != null)
                {
                    try
                    {
                        context.Send(state => Start(), null);
                    }
                    catch (InvalidAsynchronousStateException)
                    {
                        Start();
                    }
                }
                else
                {
                    Start();
                }
            };

            _remindLaterTimer.Start();
        }

        /// <summary>
        ///   打开下载窗口，开始下载更新包
        /// </summary>
        public static bool DownloadUpdate()
        {
            var downloadDialog = new DownloadUpdateDialog(DownloadURL);

            try
            {
                return downloadDialog.ShowDialog().Equals(DialogResult.OK);
            }
            catch (TargetInvocationException)
            {
            }
            return false;
        }
    }
}
