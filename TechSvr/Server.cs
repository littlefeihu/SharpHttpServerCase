
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TechSvr.Dispatcher;

namespace TechSvr
{
    public class Server : SharpHttpServer.Net.Http.HttpServer
    {
        public Server(int port)
            : base(port)
        {
            Get["/"] = _ => "你好,服务正在运行";

            Get["/api/command"] = CommandDispatcher.Dispatch;
            Post["/api/command"] = CommandDispatcher.Dispatch;
        }

    }
}
