using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechSvr.Utils
{
    //  msgtype：消息类型，用于表达需要处理的事项；
    //	infname：接口dll名称，用于动态调用dll来处理业务；
    //	inftype：接口类型，比如：Delphi、C#、C++、PB等；
    //	validateid：验证码，用于验证当前接口是否可用；
    //	data：接口数据：支持XML和JSON格式
    public class Constants
    {
        #region 参数名称
        public const string QueryString_MsgType = "msgtype";
        public const string QueryString_InfName = "infname";
        public const string QueryString_InfType = "inftype";
        public const string QueryString_ValidateId = "validateid";
        public const string QueryString_Data = "data";
        public const string PostBody_Data = "postbody";
        public const string QueryString_SysType = "systype";
        #endregion


        #region INI 文件
        public const string INI_FileName = "TechSvr.ini";
        public const string INI_SectionName = "ServiceProfile";
        public const string INI_ServicePort = "Port";
        public const string INI_MAXRecordCount = "MaxRecordCount";
        public const string INI_Log = "Log";


        #endregion



        public const string MainDataTable = "mainDataTable";
        public const string SubDataTable = "subDataTable";
    }
}
