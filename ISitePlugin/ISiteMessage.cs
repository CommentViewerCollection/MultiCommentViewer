namespace SitePlugin
{
    public interface ISiteMessage : IValueChanged
    {
        string Raw { get; }
        SiteType SiteType { get; }
    }
}
