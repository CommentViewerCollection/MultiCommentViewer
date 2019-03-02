using System;

namespace NicoSitePlugin
{
    public interface IChat
    {
        int? Anonymity { get; }
        DateTime Date { get; }
        long? DateUsec { get; }
        bool IsBsp { get; }
        string Locale { get; }
        string Mail { get; }
        int? No { get; }
        string Origin { get; }
        int? Premium { get; }
        string Raw { get; }
        int? Score { get; }
        string Text { get; }
        string Thread { get; }
        string UserId { get; }
        long? Vpos { get; }
        bool Yourpost { get; }
    }
}