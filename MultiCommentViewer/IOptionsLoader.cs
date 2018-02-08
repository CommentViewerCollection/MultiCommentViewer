using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
namespace MultiCommentViewer
{
    public interface IOptionsSerializer
    {
        IOptions DeserializeOptions(string optionsStr);
        string SerializeOptions(IOptions options);
    }
}
