using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePlugin
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICurrentUserInfo
    {
        string Username { get; }
        //string UserId { get; }
        bool IsLoggedIn { get; }
    }
}
