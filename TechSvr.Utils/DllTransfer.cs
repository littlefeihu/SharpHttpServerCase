using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TechSvr.Utils
{
    /// <summary>
    /// 调用其他其他语言辅助类
    /// </summary>
    public class DllTransfer
    {
        /// <summary>
        /// 调用C++接口
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="dllname"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [DllImport("CPPTransfer.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CommonMethodC(IntPtr filepath, IntPtr dllname, IntPtr param);

        /// <summary>
        /// 调用delphi接口
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="dllname"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        [DllImport("CDelTransfer.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CommonMethodD(IntPtr filepath, IntPtr dllname, IntPtr param);
    }
}
