namespace NicoSitePlugin.Test
{
    internal class NicoSiteOptions : INicoSiteOptions
    {
        public bool CanUploadPlayerStatus { get; set; }
        public int OfficialRoomsRetrieveCount { get; set; }
        public NicoSiteOptions()
        {
            OfficialRoomsRetrieveCount = 3;

            CanUploadPlayerStatus = true;
        }
    }
}
