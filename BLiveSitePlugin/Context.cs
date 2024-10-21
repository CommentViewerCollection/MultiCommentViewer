namespace BLiveSitePlugin
{
    public class Context
    {
        public string Uuid { get; }
        public string AccessToken { get; }
        public Context(string uuid, string accessToken)
        {
            Uuid = uuid;
            AccessToken = accessToken;
        }
        public override string ToString()
        {
            return $"Uuid={Uuid}, AccessToken={AccessToken}";
        }
    }
}
