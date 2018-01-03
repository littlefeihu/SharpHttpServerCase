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
            var excuteResult = "";
            var msgtype = "";
            try
            {
                var provider = new RequestDataProvider(request);
                var input = provider.BuildInputArgs();
                msgtype = input.MsgType;
                var queryString = provider.QueryString;
                var postBody = input.PostBody;
                TechSvrApplication.Instance.WhiteLog("接收到请求：" + msgtype);
                TechSvrApplication.Instance.WhiteLog("QueryString参数：" + provider.QueryString, false);

                if (!string.IsNullOrEmpty(input.PostBody))
                {
                    TechSvrApplication.Instance.WhiteLog("PostBody参数：" + input.PostBody, false);
                }
                var cmd = TechSvrApplication.Instance.GetCommand(msgtype);

                excuteResult = cmd.Excute(input);
                TechSvrApplication.Instance.WhiteLog("已处理请求：" + msgtype + ",执行成功");
            }
            catch (Exception ex)
            {
                TechSvrApplication.Instance.WhiteLog("已处理请求：" + msgtype + ",执行失败,错误信息:" + ex.Message);
                throw;
            }

            return excuteResult;
        }
    }
}
