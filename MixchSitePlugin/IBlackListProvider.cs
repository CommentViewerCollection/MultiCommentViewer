using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenrecSitePlugin
{
    interface IBlackListProvider
    {
        event EventHandler<List<string>> Received;

        void Disconnect();
        Task ReceiveAsync(string movieId, Context context);
    }
}