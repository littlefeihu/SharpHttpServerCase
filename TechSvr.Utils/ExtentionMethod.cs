using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace TechSvr.Utils
{
    public static class ExtentionMethod
    {
        public static string ToString(this NameValueCollection nameValueCollection)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var key in nameValueCollection.AllKeys)
            {
                stringBuilder.Append(nameValueCollection[key]);
            }
            return stringBuilder.ToString();
        }
    }
}
