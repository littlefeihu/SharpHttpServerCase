using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TechSvr.Utils;
using SharpHttpServer.Net.Http;

namespace TechSvr.Dispatcher
{
    //  msgtype：消息类型，用于表达需要处理的事项；
    //	infname：接口dll名称，用于动态调用dll来处理业务；
    //	inftype：接口类型，比如：Delphi、C#、C++、PB等；
    //	validateid：验证码，用于验证当前接口是否可用；
    //	data：接口数据：支持XML和JSON格式
    //  调用示例：
    //http://localhost:8080/lis?msgtype=PRINT&infname=Com_PrintReport.dll&inftype=Delphi&validateid=uey94379230027739jjsyu0203821392323&data={XMLData或Json}

    public class CommandDispatcher
    {
        /// <summary>
        ///  命令分发
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string Dispatch(HttpListenerRequest request)
        {
            var parameters = new RequestDataProvider(request).GetParams();

            var msgtype = parameters.Get(Constants.QueryString_MsgType);
            var infname = parameters.Get(Constants.QueryString_InfName);
            var inftype = parameters.Get(Constants.QueryString_InfType);
            var validateid = parameters.Get(Constants.QueryString_ValidateId);
            var data = parameters.Get(Constants.QueryString_Data);

            //查询字符串中取不到值 则尝试从RequestBody中获取数据
            if (string.IsNullOrEmpty(data))
            {
                data = parameters.Get(Constants.PostBody_Data);
            }

            TechSvrApplication.Instance.WhiteLog("请求类型：" + msgtype);
            var cmd = TechSvrApplication.Instance.GetCommand(msgtype);

            return cmd.Excute(data);
        }
    }
}
