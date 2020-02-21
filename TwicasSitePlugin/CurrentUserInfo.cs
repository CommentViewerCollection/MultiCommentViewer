using System;
using Common;
using System.Windows.Threading;
using SitePlugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Http;
using ryu_s.BrowserCookie;
using SitePluginCommon;

namespace TwicasSitePlugin
{
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
