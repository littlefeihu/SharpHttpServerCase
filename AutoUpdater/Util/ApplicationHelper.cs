using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AutoUpdaterDotNET.Util
{
    public class ApplicationHelper
    {
        public static Version GetInstallVersion()
        {
            XmlDocument receivedAppCastDocument = new XmlDocument();
            var techManifestXML = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TechManifest.xml");
            receivedAppCastDocument.Load(techManifestXML);
            try
            {
                var installVersion = new Version(receivedAppCastDocument.SelectSingleNode("//item/version").InnerText);
                return installVersion;
            }
            catch (Exception ex)
            {
                throw new Exception("TechManifest.xml 版本解析出错");
            }
        }

        public static Attribute GetAttribute(Assembly assembly, Type attributeType)
        {
            object[] attributes = assembly.GetCustomAttributes(attributeType, false);
            if (attributes.Length == 0)
            {
                return null;
            }
            return (Attribute)attributes[0];
        }

    }
}
