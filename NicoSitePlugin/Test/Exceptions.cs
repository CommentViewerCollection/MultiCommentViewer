using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Test
{
    [Serializable]
    public class GetPostKeyFailedException : Exception
    {
        public string Response { get; }
        public GetPostKeyFailedException(string res)
        {
            Response = res;
        }
    }
}
