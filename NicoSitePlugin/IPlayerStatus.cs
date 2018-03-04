using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace NicoSitePlugin.Old
{
    public interface IPlayerStatus
    {
        string Raw { get; }
        string Title { get; }
        string Description { get; }
        string DefaultCommunity { get; }
        /// <summary>
        /// 配信者のUserID
        /// </summary>
        /// <remarks>名前はownerだけど、実際には配信者のID。コミュニティのオーナーアカウントのIDでは無い</remarks>
        string OwnerId { get; }

        string OwnerName { get; }
        ProviderType ProviderType { get; }
        int BaseTime { get; }
        int OpenTime { get; }
        int StartTime { get; }
        int? EndTime { get; }
        string UserId { get; }
        string Nickname { get; }
        string RoomLabel { get; }
        int RoomSeetNo { get; }
        bool IsJoin { get; }
        string IsPremium { get; }
        IMs Ms { get; }
        IMs[] MsList { get; }

    }
    public interface IPlayerStatusError
    {
        ErrorCode Code { get; }
    }
    public interface IDataSource
    {
        Task<string> Get(string url, CookieContainer cc);
    }
    public interface IPlayerStatusResponse
    {
        bool Success { get; }
        IPlayerStatus PlayerStatus { get; }
        IPlayerStatusError Error { get; }
    }
    public interface IMs
    {
        string Addr { get; }
        string Thread { get; }
        int Port { get; }
    }
}
