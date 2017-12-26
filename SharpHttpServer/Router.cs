using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpHttpServer.Net.Http
{
    class Router
    {
        public Router()
        {
            registrators = new List<RequestHandlerRegistrator>(); ;
            servedStatic = new Dictionary<string, DirectoryInfo>();
        }

        private readonly List<RequestHandlerRegistrator> registrators;

        private readonly Dictionary<string, DirectoryInfo> servedStatic;

        public RequestHandlerRegistrator GetRegistrator(HttpMethod method)
        {
            var res = registrators.FirstOrDefault(r => r.HttpMethod == method);
            if (res == null)
            {
                res = new RequestHandlerRegistrator(method);
                registrators.Add(res);
            }
            return res;
        }

        public IEnumerable<string> GetAllRoutes()
        {
            return registrators
                .SelectMany(r => r.Handlers.Keys)
                .Union(servedStatic.Keys);
        }

        public Func<HttpListenerRequest, string> FindHandler(HttpListenerRequest request)
        {
            Func<HttpListenerRequest, string> res = null;

            var requestMethod = request.GetHttpMethod();
            var registrator = registrators.FirstOrDefault(r => r.HttpMethod == requestMethod);

            if (registrator != null)
            {
                res = registrator.Handlers
                    .Where(kv => kv.Key.Equals(request.Url.AbsolutePath, StringComparison.CurrentCultureIgnoreCase))
                    .Select(kv => kv.Value)
                    .FirstOrDefault();
            }

            if (res == null && requestMethod == HttpMethod.Get)
            {
                string staticMatch = servedStatic.Keys.FirstOrDefault(k => request.Url.AbsolutePath.StartsWith(k));
                if (staticMatch != null)
                {
                    string fileRelPath = request.Url.AbsolutePath.Substring(staticMatch.Length);
                    if (fileRelPath == "" || fileRelPath == "index")
                        fileRelPath = "index.html";
                    string fileAbsPath = Path.Combine(servedStatic[staticMatch].FullName, fileRelPath);
                    if (File.Exists(fileAbsPath))
                        res = _ => File.ReadAllText(fileAbsPath);
                }
            }

            return res;
        }

        public void ServeStatic(DirectoryInfo directory, string path = "")
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (servedStatic.ContainsKey(path))
                throw new ArgumentException("Directory is already served statically at path " + path, "path");

            path = path.Trim();
            if (path == "")
                path = "/";
            else
            {
                if (!path.StartsWith("/"))
                    path = "/" + path;
                if (!path.EndsWith("/"))
                    path += "/";
            }

            servedStatic[path] = directory;
        }
    }
}
