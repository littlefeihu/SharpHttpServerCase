using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdaterDotNET.Util
{
    public class WebHelper
    {
        public static string GetURL(Uri baseUri, String url)
        {
            if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                Uri uri = new Uri(baseUri, url);

                if (uri.IsAbsoluteUri)
                {
                    url = uri.AbsoluteUri;
                }
            }

            return url;
        }
    }
}
