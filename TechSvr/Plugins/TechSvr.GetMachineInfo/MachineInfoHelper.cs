using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;

namespace TechSvr.Plugin.GetMachineInfo
{
    public class MachineInfoHelper
    {
        /// <summary>  
        /// 获取本地IP  
        /// </summary>  
        /// <returns></returns>  
        public static string GetUserIP()
        {
            string ip = "";
            string strHostName = Dns.GetHostName(); //得到本机的主机名  
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName); //取得本机IP  
            if (ipEntry.AddressList.Length > 0)
            {
                ip = ipEntry.AddressList[1].ToString();
            }
            return ip;
        }

        /// <summary>  
        /// 获取CPU编号  
        /// </summary>  
        /// <returns>返回一个字符串类型</returns>  
        public static string GetCPUID()
        {
            try
            {
                //需要在解决方案中引用System.Management.DLL文件  
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                string strCpuID = null;
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                return strCpuID;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>  
        /// 获取网卡的MAC地址  
        /// </summary>  
        /// <returns>返回一个string</returns>  
        public static string GetMAC()
        {
            try
            {
                string stringMAC = "";
                ManagementClass MC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection MOC = MC.GetInstances();
                foreach (ManagementObject MO in MOC)
                {
                    if ((bool)MO["IPEnabled"] == true)
                    {
                        stringMAC += MO["MACAddress"].ToString();
                    }
                }
                return stringMAC;
            }
            catch
            {
                return "";
            }
        }

        public static string GetComputerName()
        {
            try
            {
                return System.Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return "unknow";
            }
        }
    }
}
