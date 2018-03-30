using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using TechSvr.Utils;
namespace TechSvr.Utils
{
    public class RequestDataProvider
    {
        HttpListenerRequest _request;
        public RequestDataProvider(HttpListenerRequest request)
        {
            _request = request;
        }

        private NameValueCollection GetParams()
        {
            string queryString = _request.RawUrl.Substring(_request.RawUrl.IndexOf("?") + 1);
            QueryString = queryString;
            var paraments = HttpUtility.ParseQueryString(queryString);

            if (_request.HasEntityBody)
            {
                using (System.IO.Stream body = _request.InputStream)
                {
                    System.Text.Encoding encoding = _request.ContentEncoding;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(body, System.Text.Encoding.UTF8))
                    {
                        var postBodytr = reader.ReadToEnd();
                        paraments.Add(Constants.PostBody_Data, postBodytr);
                    }
                }
            }
            return paraments;
        }

        public string QueryString
        {
            get; private set;
        }

        /// <summary>
        /// 构建输入参数
        /// </summary>
        public InputArgs BuildInputArgs()
        {
            var parameters = GetParams();
            var postBody = parameters.Get(Constants.PostBody_Data);
            var postParameters = HttpUtility.ParseQueryString(postBody);
            var msgtype = postParameters.Get(Constants.QueryString_MsgType);
            var infname = postParameters.Get(Constants.QueryString_InfName);
            var inftype = postParameters.Get(Constants.QueryString_InfType);
            var validateid = postParameters.Get(Constants.QueryString_ValidateId);
            var data = postParameters.Get(Constants.QueryString_Data);
            var systype = postParameters.Get(Constants.QueryString_SysType);

            if (string.IsNullOrEmpty(msgtype))
            {//如果msgtype不包含值，则尝试反序列化json字符串
                var input = postBody.ToObject<InputArgs>();
                input.SetPostBody(postBody);
                return input;
            }
            return InputArgs.Create(postBody, msgtype, infname, inftype, validateid, data, systype);

        }
    }
}
