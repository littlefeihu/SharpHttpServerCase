using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpHttpServer.Net.Http
{
    public class RequestHandlerRegistrator
    {
        public RequestHandlerRegistrator(HttpMethod httpMethod)
        {
            this.httpMethod = httpMethod;
        }

        public HttpMethod HttpMethod
        {
            get { return httpMethod; }
        }
        private readonly HttpMethod httpMethod;

        public Dictionary<string, Func<HttpListenerRequest, string>> Handlers
        {
            get { return handlers; }
        }
        private readonly Dictionary<string, Func<HttpListenerRequest, string>> handlers = new Dictionary<string, Func<HttpListenerRequest, string>>();

        public Func<HttpListenerRequest, string> this[string path]
        {
            get { return handlers[path]; }
            set { handlers[path] = value; }
        }
    }
}
