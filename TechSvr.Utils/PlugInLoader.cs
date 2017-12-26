using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TechSvr.Utils
{
    public class PlugInLoader
    {
        /// <summary>
        /// 加载插件DLL
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ICommand> Load()
        {
            var pluginDirctory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            var dllFiles = Directory.GetFiles(pluginDirctory, "*.dll", SearchOption.AllDirectories);
            foreach (var dllFile in dllFiles)
            {
                Assembly assembly = Assembly.LoadFile(dllFile);
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var interf in type.GetInterfaces())
                    {
                        if (interf == typeof(ICommand))
                        {
                            var cmd = (ICommand)Activator.CreateInstance(type);

                            yield return cmd;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

        }

    }
}
