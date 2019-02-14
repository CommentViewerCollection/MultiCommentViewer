namespace NicoSitePlugin
{
    public class PlayerStatusResponseTest : IPlayerStatusResponse
    {
        #region Properties
        public bool Success { get { return _ps != null; } }
        public IPlayerStatus PlayerStatus { get { return _ps; } }
        public IPlayerStatusError Error { get { return _error; } }
        #endregion

        #region Ctors
        public PlayerStatusResponseTest(IPlayerStatus playerStatus)
        {
            _ps = playerStatus;
        }
        public PlayerStatusResponseTest(IPlayerStatusError error)
        {
            _error = error;
        }
        #endregion //Ctors

        #region Fields
        private readonly IPlayerStatus _ps;
        private readonly IPlayerStatusError _error;
        #endregion //Fields
    }
}
