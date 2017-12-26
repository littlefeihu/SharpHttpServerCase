using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpHttpServer.Net.Http
{
    public static class HttpListenerRequestExtensions
    {
        public static HttpMethod GetHttpMethod(this HttpListenerRequest request)
        {
            return (HttpMethod)Enum.Parse(typeof(HttpMethod), request.HttpMethod, ignoreCase: true);
        }
    }
}
