using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixchSitePlugin
{
    interface IBlackListProvider
    {
        event EventHandler<List<string>> Received;

        void Disconnect();
        Task ReceiveAsync(string movieId, Context context);
    }
}
