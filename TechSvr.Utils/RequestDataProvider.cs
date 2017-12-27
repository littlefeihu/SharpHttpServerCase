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
                              
        public NameValueCollection GetParams()
        {
            string queryString = _request.RawUrl.Substring(_request.RawUrl.IndexOf("?") + 1);
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
    }
}
