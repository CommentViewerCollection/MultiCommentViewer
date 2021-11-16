using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Concurrent;
using System.Linq;
using SitePluginCommon;
using System.Net.Http;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace YouTubeLiveSitePlugin.Test2
{
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
