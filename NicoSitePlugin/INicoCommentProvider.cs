using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
namespace NicoSitePlugin
{
    public interface INicoCommentProvider : ICommentProvider
    {
        Task PostCommentAsync(string text, string mail);
    }
}
