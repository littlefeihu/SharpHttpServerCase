
using log4net;
using System;
using System.IO;
using System.Web;

namespace TechSvr.Utils
{
    public class LogFactory
    {
        static LogFactory()
        {

        }

        public static Log GetLogger(string filename = "Log")
        {
            ILog logger = CustomRollingFileLogger.GetCustomLogger(filename, DateTime.Now.ToString("yyyyMMdd"));

            return new Log(logger);
        }


    }
}
