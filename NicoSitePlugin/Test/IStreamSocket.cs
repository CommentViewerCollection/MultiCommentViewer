using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NicoSitePlugin.Old
{
    interface IStreamSocket
    {
        event EventHandler Connected;
        event EventHandler<List<string>> Received;

        Task ConnectAsync();
        void Disconnect();
        void Dispose();
        Task ReceiveAsync();
        Task SendAsync(string s);
    }
}