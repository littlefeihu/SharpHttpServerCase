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
    /// <summary>
    ///     Main class that lets you auto update applications by setting some static fields and executing its Start method.
    /// </summary>
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
        public static Action<bool> UpdateChanged;
        /// <summary>
        ///     Set it to folder path where you want to download the update file. If not provided then it defaults to Temp folder.
        /// </summary>
        public static String DownloadPath;

        /// <summary>
        ///     Set the Application Title shown in Update dialog. Although AutoUpdater.NET will get it automatically, you can set this property if you like to give custom Title.
        /// </summary>
        public static String AppTitle;

        /// <summary>
        ///     URL of the xml file that contains information about latest version of the application.
        /// </summary>
        public static String AppCastURL;

        /// <summary>
        ///     Opens the download url in default browser if true. Very usefull if you have portable application.
        /// </summary>
        public static bool OpenDownloadPage;

        /// <summary>
        ///     If this is true users can see the skip button.
        /// </summary>
        public static Boolean ShowSkipButton = true;

        /// <summary>
        ///     If this is true users can see the Remind Later button.
        /// </summary>
        public static Boolean ShowRemindLaterButton = true;

        /// <summary>
        ///     If this is true users see dialog where they can set remind later interval otherwise it will take the interval from
        ///     RemindLaterAt and RemindLaterTimeSpan fields.
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
        ///     Set this to false if your application doesn't need administrator privileges to replace the old version.
        /// </summary>
        public static bool RunUpdateAsAdmin = true;

        ///<summary>
        ///     Set this to true if you want to ignore previously assigned Remind Later and Skip settings. It will also hide Remind Later and Skip buttons.
        /// </summary>
        public static bool Mandatory;

        /// <summary>
        ///     Set Proxy server to use for all the web requests in AutoUpdater.NET.
        /// </summary>
        public static WebProxy Proxy;

        /// <summary>
        ///     Set if RemindLaterAt interval should be in Minutes, Hours or Days.
        /// </summary>
        public static RemindLaterFormat RemindLaterTimeSpan = RemindLaterFormat.Days;

        /// <summary>
        ///     A delegate type to handle how to exit the application after update is downloaded.
        /// </summary>
        public delegate void ApplicationExitEventHandler();

        /// <summary>
        ///     An event that developers can use to exit the application gracefully.
        /// </summary>
        public static event ApplicationExitEventHandler ApplicationExitEvent;

        /// <summary>
        ///     A delegate type for hooking up update notifications.
        /// </summary>
        /// <param name="args">An object containing all the parameters recieved from AppCast XML file. If there will be an error while looking for the XML file then this object will be null.</param>
        public delegate void CheckForUpdateEventHandler(UpdateInfoEventArgs args);

        /// <summary>
        ///     An event that clients can use to be notified whenever the update is checked.
        /// </summary>
        public static event CheckForUpdateEventHandler CheckForUpdateEvent;

        /// <summary>
        ///     A delegate type for hooking up parsing logic.
        /// </summary>
        /// <param name="args">An object containing the AppCast file received from server.</param>
        public delegate void ParseUpdateInfoHandler(ParseUpdateInfoEventArgs args);

        /// <summary>
        ///     An event that clients can use to be notified whenever the AppCast file needs parsing.
        /// </summary>
        public static event ParseUpdateInfoHandler ParseUpdateInfoEvent;

        #endregion

        /// <summary>
        ///     Start checking for new version of application and display dialog to the user if update is available.
        /// </summary>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
        public static void Start(Assembly myAssembly = null)
        {
            Start(AppCastURL, myAssembly);
        }

        /// <summary>
        /// Start checking for new version of application and display dialog to the user if update is available.
        /// </summary>
        /// <param name="appCast">URL of the xml file that contains information about latest version of the application.</param>
        /// <param name="myAssembly">Assembly to use for version checking.</param>
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
            var updateForm = new UpdateForm();
            if (updateForm.ShowDialog().Equals(DialogResult.OK))
            {
                if (NeedRestart)
                {
                    Exit();
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
        /// Detects and exits all instances of running assembly, including current.
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
                        // Current process should be same as processes created by other instances of the application so it should be able to access modules of other instances. 
                        // This means this is not the process we are looking for so we can safely skip this.
                        continue;
                    }

                    if (process.Id != currentProcess.Id &&
                        currentProcess.MainModule.FileName == processPath) //get all instances of assembly except current
                    {
                        if (process.CloseMainWindow())
                        {
                            process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds); //give some time to process message
                        }
                        if (!process.HasExited)
                        {
                            process.Kill(); //TODO show UI message asking user to close program himself instead of silently killing it
                        }
                    }
                }

                if (IsWinFormsApplication)
                {
                    MethodInvoker methodInvoker = Application.Exit;
                    methodInvoker.Invoke();
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
        ///     Opens the Download window that download the update and execute the installer when download completes.
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
