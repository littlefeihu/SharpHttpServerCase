using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

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
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding))
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
            var msgtype = parameters.Get(Constants.QueryString_MsgType);
            var infname = parameters.Get(Constants.QueryString_InfName);
            var inftype = parameters.Get(Constants.QueryString_InfType);
            var validateid = parameters.Get(Constants.QueryString_ValidateId);
            var data = parameters.Get(Constants.QueryString_Data);
            var systype = parameters.Get(Constants.QueryString_SysType);
            var postBody = parameters.Get(Constants.PostBody_Data);

            //查询字符串中取不到值 则尝试从RequestBody中获取数据
            if (string.IsNullOrEmpty(data))
            {
                data = postBody;
            }
            return InputArgs.Create(postBody, msgtype, infname, inftype, validateid, data, systype);

        }
    }
}
