using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TechSvr.Controls;
using TechSvr.ModuleForm;
using TechSvr.Utils;

namespace TechSvr
{
    public partial class TechSvrForm : Form
    {
        private ContextMenu notifyiconMnu;
        private List<Server> MyServers = new List<Server>();
        System.Timers.Timer timer = null;
        public TechSvrForm(string[] args)
        {
            InitializeComponent();
            Initializenotifyicon();
            this.SizeChanged += TechSvrForm_SizeChanged;
            InitServer();
            LoadLogControl();
            TechSvrApplication.Instance.SetMainFrm(this);

            StartServer();
            CheckUpdate();
            AutoUpdater.UpdateChanged += (isUpdateAvailable) =>
            {
                if (!isUpdateAvailable)
                {
                    MessageBox.Show("已经是最新版本");
                }
            };
        }

        private void CheckUpdate()
        {
            AutoUpdater.Mandatory = true;

            var minutes = int.Parse(System.Configuration.ConfigurationManager.AppSettings[Constants.AutoCheckUpdateInterval].ToString());
            timer = new System.Timers.Timer
            {
                Interval = minutes * 60 * 1000,
                SynchronizingObject = this
            };
            timer.Elapsed += delegate
            {
                AutoUpdater.Start(System.Configuration.ConfigurationManager.AppSettings[Constants.CheckUpdateUrl].ToString());
            };
            timer.Start();
        }
        private void StartServer()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Run();
            });
        }
        private void LoadLogControl()
        {
            this.logControl1.AutoScroll = true;
            this.logControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logControl1.Location = new System.Drawing.Point(3, 3);
            this.logControl1.Name = "logControl1";
            this.logControl1.Size = new System.Drawing.Size(708, 405);
            this.logControl1.TabIndex = 0;
            this.tabPage2.Controls.Add(this.logControl1);
        }

        private void InitServer()
        {
            MyServers.Clear();

            foreach (var port in Ports)
            {
                var MyServer = new Server(int.Parse(port));
                MyServer.StatusChanged += MyServer_StatusChanged;
                MyServer.CmdErrored += MyServer_CmdErrored;
                MyServers.Add(MyServer);
            }
        }

        public List<string> Ports
        {
            get
            {
                var portsStr = INIHelper.ReadString(Constants.INI_ServicePort, DirectoryManage.GetINIFullPath());

                if (string.IsNullOrEmpty(portsStr))
                {
                    portsStr = System.Configuration.ConfigurationManager.AppSettings["port"];
                }

                return portsStr.Split(',').OrderBy(o => o).ToList();
            }
        }



        public void Run()
        {
            foreach (var server in MyServers)
            {
                server.Run();
            }
        }
        public void Stop()
        {
            foreach (var server in MyServers)
            {
                server.Stop();
            }
        }
        #region 内部事件
        private void MyServer_CmdErrored(Exception ex)
        {
            TechSvrApplication.Instance.ShowToUI(ex.ToString());
        }

        private void TechSvrForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.ShowInTaskbar = false;
            }
        }
        // 重写OnClosing使点击关闭按键时窗体能够缩进托盘

        protected override void OnClosing(CancelEventArgs e)
        {

            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            e.Cancel = true;
        }

        private void MyServer_StatusChanged(bool isStarted, string baseUrl)
        {
            string statusChangeText = "";
            if (isStarted)
            {
                statusChangeText = "服务已启动," + baseUrl;
            }
            else
            {
                statusChangeText = "服务已关闭," + baseUrl;
            }

            while (!this.IsHandleCreated)
            {//窗口句柄 未创建 则等待
                Thread.Sleep(1000);
            }
            this.Invoke(new MethodInvoker(() =>
            {
                启动服务ToolStripMenuItem.Enabled = !isStarted;
                关闭服务ToolStripMenuItem.Enabled = isStarted;
                lblServiceStatus.Text = statusChangeText;
            }));
            TechSvrApplication.Instance.ShowToUI(statusChangeText);
        }


        private void 清空日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logControl1.ClearLog();
        }

        private void 启动服务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void 关闭服务ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApplition();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowForm();
        }
        private void notifyIcon1_showfrom(object sender, System.EventArgs e)
        {
            ShowForm();
        }

        private void ExitSelect(object sender, System.EventArgs e)
        {
            ExitApplition();
        }
        #endregion

        #region 私有方法

        private void Initializenotifyicon()
        {
            //定义一个MenuItem数组，并把此数组同时赋值给ContextMenu对象
            MenuItem[] mnuItms = new MenuItem[3];
            mnuItms[0] = new MenuItem();
            mnuItms[0].Text = "显示窗口";
            mnuItms[0].Click += new System.EventHandler(this.notifyIcon1_showfrom);

            mnuItms[1] = new MenuItem("-");

            mnuItms[2] = new MenuItem();
            mnuItms[2].Text = "退出";
            mnuItms[2].Click += new System.EventHandler(this.ExitSelect);
            mnuItms[2].DefaultItem = true;

            notifyiconMnu = new ContextMenu(mnuItms);
            notifyIcon1.ContextMenu = notifyiconMnu;
            //为托盘程序加入设定好的ContextMenu对象
        }




        private void ExitApplition()
        {
            if (MessageBox.Show("确认退出吗？", "退出系统", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                System.Environment.Exit(0);
            }
        }

        public void ShowForm()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
            }
        }

        public void HideForm()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                notifyIcon1.Visible = true;
            }
        }

        #endregion

        /// <summary>
        /// 端口设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var originPorts = Ports;
            var portSettingForm = new PortSettingForm(string.Join(",", Ports));

            portSettingForm.ShowDialog();


            //端口增加或减少 都会重新启动服务
            if (originPorts.Except(Ports).Count() > 0 || Ports.Except(originPorts).Count() > 0)
            {
                Stop();

                InitServer();

                if (!MyServers.Any(o => o.IsStarted))
                {
                    Run();
                }
            }

        }



        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var outputRecordCountForm = new OutputRecordCountForm(TechSvrApplication.Instance.MaxRecordCount);
            outputRecordCountForm.ShowDialog();
        }

        private void 检查更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            AutoUpdater.Start(System.Configuration.ConfigurationManager.AppSettings["checkupdateurl"].ToString());

        }
    }
}
