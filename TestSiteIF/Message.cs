using SitePlugin;

namespace TestSitePlugin
{
    public enum TestMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
    }

    public interface ITestMessage : IMessage
    {
        TestMessageType TestMessageType { get; }
    }
    public interface ITestConnected : ITestMessage
    {
    }
    public interface ITestDisconnected : ITestMessage
    {
    }
    public interface ITestComment : ITestMessage, IMessageComment
    {
    }
}