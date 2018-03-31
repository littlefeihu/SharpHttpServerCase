using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TechSvr.Utils
{
    public class AutoUpdateMonitor
    {
        static EventWaitHandle evh = new EventWaitHandle(false, 0, "AutoUpdateHappened");
        static Thread updateMonitorThread;
        public static Action AutoUpdateHappened;
        /// <summary>
        /// 检测自动升级的事件发生
        /// </summary>
        public static void StartMonitor()
        {
            updateMonitorThread = new Thread(() =>
            {
                while (true)
                {
                    //等待事件发生
                    evh.WaitOne();
                    if (AutoUpdateHappened != null)
                    {
                        AutoUpdateHappened();
                    }
                }
            });
            updateMonitorThread.Start();

        }


    }
}
