using Mcv.PluginV2;
using System.Collections.Generic;

namespace Mcv.Core;

class UserCommentCountManager
{
    private readonly Dictionary<ConnectionId, Dictionary<string, int>> _userCommentCountDict = new();

    internal void AddCommentCount(ConnectionId connId, string userId)
    {
        var dict = _userCommentCountDict[connId];
        if (dict.TryGetValue(userId, out var n))
        {
            dict[userId] = n + 1;
        }
        else
        {
            dict.Add(userId, 1);
        }
    }
    internal bool IsFirstComment(ConnectionId connId, string userId)
    {
        var dict = _userCommentCountDict[connId];
        if (dict.TryGetValue(userId, out var n))
        {
            return n <= 1;
        }
        else
        {
            return true;
        }
    }

    internal void AddConnection(ConnectionId connId)
    {
        _userCommentCountDict.Add(connId, new Dictionary<string, int>());
    }
}
