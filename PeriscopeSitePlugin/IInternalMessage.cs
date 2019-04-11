namespace PeriscopeSitePlugin
{
    internal interface IInternalMessage
    {
        InternalMessageType MessageType { get; }
        string Raw { get; }
    }
}
