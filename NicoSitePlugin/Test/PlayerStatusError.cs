namespace NicoSitePlugin.Test
{
    public class PlayerStatusError : IPlayerStatusError
    {
        public ErrorCode Code { get; set; }
        public PlayerStatusError(ErrorCode code)
        {
            Code = code;
        }
    }
}
