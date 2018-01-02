using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SharpHttpServer.Net.Http
{
    public class HttpServer : IDisposable
    {
        private readonly Router router;
        private bool isStarted;

        private HttpListener listener;

        private int port;

        public string Hostname { get; set; } = "localhost";

        public string Scheme { get; set; } = "http";

        public bool IsStarted
        {

            get
            {
                return isStarted;
            }
            private set
            {
                isStarted = value;
                if (StatusChanged != null)
                    StatusChanged(isStarted, BaseUrl);
            }
        }

        /// <summary>
        /// 服务状态变化事件
        /// </summary>
        public event Action<bool, string> StatusChanged;
        /// <summary>
        /// 命令执行出错事件
        /// </summary>

        public event Action<Exception> CmdErrored;

        public HttpServer(int port)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            router = new Router();

            this.port = port;
        }


        public void SetPort(int port)
        {
            this.port = port;
        }
        public string BaseUrl
        {
            get { return BuildUri(); }
        }

        public void Run()
        {
            listener = new HttpListener();
            router.GetAllRoutes()
                .ToList()
                .ForEach(path =>
                {
                    string query = "";
                    if (path.Contains("?"))
                    {
                        query = path.Substring(path.IndexOf("?") + 1);
                        path = path.Substring(0, path.IndexOf("?"));
                    }
                    if (!path.EndsWith("/"))
                        path += "/";
                    listener.Prefixes.Add(BuildUri(path, query));
                });

            listener.Start();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                IsStarted = true;
                Console.WriteLine("Webserver running...");
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                Func<HttpListenerRequest, string> handler = router.FindHandler(ctx.Request);
                                if (handler == null)
                                    Respond404(ctx);
                                else
                                    ProcessRequest(ctx, handler);
                            }
                            catch (Exception ex)
                            {
                                if (CmdErrored != null)
                                    CmdErrored(ex);

                                Respond500(ctx);
                            } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();
            IsStarted = false;
        }

        public RequestHandlerRegistrator Get
        {
            get { return router.GetRegistrator(HttpMethod.Get); }
        }

        public RequestHandlerRegistrator Post
        {
            get { return router.GetRegistrator(HttpMethod.Post); }
        }

        public RequestHandlerRegistrator Put
        {
            get { return router.GetRegistrator(HttpMethod.Put); }
        }

        public RequestHandlerRegistrator Delete
        {
            get { return router.GetRegistrator(HttpMethod.Delete); }
        }

        public RequestHandlerRegistrator Options
        {
            get { return router.GetRegistrator(HttpMethod.Options); }
        }

        public RequestHandlerRegistrator Patch
        {
            get { return router.GetRegistrator(HttpMethod.Patch); }
        }

        public RequestHandlerRegistrator Head
        {
            get { return router.GetRegistrator(HttpMethod.Head); }
        }

        public RequestHandlerRegistrator Connect
        {
            get { return router.GetRegistrator(HttpMethod.Connect); }
        }

        public RequestHandlerRegistrator Trace
        {
            get { return router.GetRegistrator(HttpMethod.Trace); }
        }

        public void ServeStatic(DirectoryInfo directory, string path = "")
        {
            router.ServeStatic(directory, path);
        }

        private string BuildUri(string path = "", string query = "")
        {
            return new UriBuilder(Scheme, Hostname, port, path, query).ToString();
        }

        private void ProcessRequest(HttpListenerContext ctx, Func<HttpListenerRequest, string> handler)
        {
            try
            {
                string response = handler(ctx.Request);
                Respond200(ctx, response);
            }
            catch (Exception)
            {
                Respond500(ctx);
            }
        }

        public void Respond200(HttpListenerContext ctx, string content)
        {
            ctx.Response.Headers.Add("Access-Control-Allow-Origin: *");
            ctx.Response.Headers.Add("Content-type", "text/html;charset=UTF-8");
            ctx.Response.StatusCode = 200;
            ctx.Response.StatusDescription = "The request was fulfilled.";
            byte[] buf = Encoding.UTF8.GetBytes(content);
            ctx.Response.ContentLength64 = buf.Length;
            ctx.Response.ContentEncoding = Encoding.UTF8;
            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
        }

        public void Respond404(HttpListenerContext ctx)
        {
            ctx.Response.Headers.Add("Access-Control-Allow-Origin: *");
            ctx.Response.Headers.Add("Content-type", "text/html;charset=UTF-8");
            ctx.Response.StatusCode = 404;
            ctx.Response.StatusDescription = "The server has not found anything matching the URI given.";
        }

        public void Respond500(HttpListenerContext ctx)
        {
            ctx.Response.Headers.Add("Access-Control-Allow-Origin: *");
            ctx.Response.Headers.Add("Content-type", "text/html;charset=UTF-8");
            ctx.Response.StatusCode = 500;
            ctx.Response.StatusDescription = "The server encountered an unexpected condition which prevented it from fulfilling the request.";

        }

        #region IDisposable Support

        private bool disposed = false; // To detect redundant calls


        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (listener.IsListening)
                        Stop();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
