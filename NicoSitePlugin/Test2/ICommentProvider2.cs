using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NicoSitePlugin.Old;

namespace NicoSitePlugin.Test2
{
    interface ICommentProvider
    {
        event EventHandler<TicketReceivedEventArgs> TicketReceived;
        event EventHandler<ChatReceivedEventArgs> CommentReceived;
        event EventHandler<InitialChatsReceivedEventArgs> InitialCommentsReceived;

        void Add(IEnumerable<RoomInfo> newRooms);
        void Disconnect();
        Task ReceiveAsync();
        Task SendAsync(RoomInfo roomInfo, string str);
    }
}