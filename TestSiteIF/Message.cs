using SitePlugin;
using System;

namespace TestSitePlugin
{
    public enum TestMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
    }

    public interface ITestMessage : ISiteMessage
    {
        TestMessageType TestMessageType { get; }
    }
    public interface ITestConnected : ITestMessage
    {
        string Text { get; }
    }
    public interface ITestDisconnected : ITestMessage
    {
        string Text { get; }
    }
    public interface ITestComment : ITestMessage
    {
        string UserName { get; }
        string Text { get; }
        DateTime PostedAt { get; }
    }
}