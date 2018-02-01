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
        internal NicoSiteOptions Clone()
        {
            return (NicoSiteOptions)this.MemberwiseClone();
        }
        internal void Set(NicoSiteOptions changedOptions)
        {
            var properties = changedOptions.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.SetMethod != null)
                {
                    property.SetValue(this, property.GetValue(changedOptions));
                }
            }
        }
    }
}
