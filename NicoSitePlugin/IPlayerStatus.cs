using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin
{
    public interface IPlayerStatus
    {
        string Title { get; }
        string Description { get; }
        ProviderType ProviderType { get; }
        long BaseTime { get; }
        long OpenTime { get; }
        long StartTime { get; }
        long? EndTime { get; }
        string UserId { get; }
        string Nickname { get; }
        string RoomLabel { get; }
        int RoomSeetNo { get; }
        bool IsJoin { get; }
        IMs Ms { get; }
        IMs[] MsList { get; }

    }
    public interface IPlayerStatusError
    {

    }
    public static class API
    {
        public static Task<IPlayerStatusResponse> GetPlayerStatusAsync(string live_id)
        {
            throw new NotImplementedException();
        }
    }
    public interface IPlayerStatusResponse
    {
        bool Success { get; }
        IPlayerStatus PlayerStatus { get; }
        IPlayerStatusError Error { get; }
    }
    public class IPlayerStatusResponseTest : IPlayerStatusResponse
    {
        #region Properties
        public bool Success { get { return _ps != null; } }
        public IPlayerStatus PlayerStatus { get; }
        public IPlayerStatusError Error { get; }
        #endregion

        #region Ctors
        public IPlayerStatusResponseTest(IPlayerStatus playerStatus)
        {
            _ps = playerStatus;
        }
        public IPlayerStatusResponseTest(IPlayerStatusError error)
        {
            _error = error;
        }
        #endregion //Ctors

        #region Fields
        private readonly IPlayerStatus _ps;
        private readonly IPlayerStatusError _error;
        #endregion //Fields
    }
    public class MsTest:IMs
    {
        public string Addr { get; set; }
        public string Thread { get; set; }
        public int Port { get; set; }
        public MsTest() { }
        public MsTest(string addr, string thread, int port)
        {
            Addr = addr;
            Thread = thread;
            Port = port;
        }
    }
    public interface IMs
    {
        string Addr { get; }
        string Thread { get; }
        int Port { get; }
    }
    public enum ProviderType
    {
        Unknown,
        Community,
        Channel,
        Official,
    }
}
