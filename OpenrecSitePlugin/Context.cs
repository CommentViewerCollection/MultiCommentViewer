namespace OpenrecSitePlugin
{
    public class Context
    {
        public string Uuid { get; set; } = "";
        //public string Token { get; set; } = "";
        //public string Random { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public override string ToString()
        {
            //return $"Uuid={Uuid}, Token={Token}, Random={Random}";
            return $"Uuid={Uuid}, AccessToken={AccessToken}";
        }
    }
}
