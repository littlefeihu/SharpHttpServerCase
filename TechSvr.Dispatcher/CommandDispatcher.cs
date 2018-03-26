using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TechSvr.Utils;
using SharpHttpServer.Net.Http;
using TechSvr.Utils.DTO;

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
        public static string CheckDispatch(HttpListenerRequest request)
        {
            return new ResposeMessage { message = "service is running", type = ResultType.SUCCESS.ToString(), data = "", messageCode = MessageCode.information.ToString() }.ToJson();
        }
        /// <summary>
        ///  命令分发
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string Dispatch(HttpListenerRequest request)
        {
            var executeResult = "";
            var msgtype = "";
            try
            {
                var provider = new RequestDataProvider(request);
                var input = provider.BuildInputArgs();
                msgtype = input.MsgType;

                if (string.IsNullOrEmpty(msgtype))
                {
                    throw new Exception("msgtype 不能为空");
                }

                CmdExecuting(provider, input);

                executeResult = TechSvrApplication.Instance.GetCommand(msgtype).Excute(input).ToJson();

                CmdExecuted(msgtype, executeResult);
            }
            catch (Exception ex)
            {
                TechSvrApplication.Instance.Log("已处理请求：" + msgtype + ",执行失败,错误信息:" + ex.Message, msgtype);
                throw ex;
            }

            return executeResult;
        }

        /// <summary>
        /// 命令执行前 日志记录
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="input"></param>
        private static void CmdExecuting(RequestDataProvider provider, InputArgs input)
        {
            TechSvrApplication.Instance.ShowToUI("接收到请求：" + input.MsgType, input.MsgType);
            TechSvrApplication.Instance.Log("QueryString参数：" + provider.QueryString, input.MsgType);

            if (!string.IsNullOrEmpty(input.PostBody))
            {
                TechSvrApplication.Instance.Log("PostBody参数：" + input.PostBody, input.MsgType);
            }
        }
        /// <summary>
        /// 命令执行后 日志记录
        /// </summary>
        /// <param name="msgtype"></param>
        /// <param name="excuteResult"></param>
        private static void CmdExecuted(string msgtype, string excuteResult)
        {
            TechSvrApplication.Instance.ShowToUI("已处理请求：" + msgtype + ",执行成功", msgtype);
            TechSvrApplication.Instance.Log("已处理请求：" + msgtype + ",执行成功,返回结果：" + excuteResult, msgtype);
        }
    }
}
