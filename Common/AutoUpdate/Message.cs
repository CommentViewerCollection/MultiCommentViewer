using System;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Common.AutoUpdate
{
    public class ShowUpdateDialogMessageItems
    {
        public Version CurrentVersion { get; }
        public LatestVersionInfo LatestVersionInfo { get; }
        public bool IsUpdateExists { get; private set; }
        public ILogger Logger { get; }
        public string UserAgent { get; }
        public ShowUpdateDialogMessageItems(bool isUpdateExists, Version currentVersion, AutoUpdate.LatestVersionInfo latestInfo, ILogger logger, string userAgent)
        {
            IsUpdateExists = isUpdateExists;
            CurrentVersion = currentVersion;
            LatestVersionInfo = latestInfo;
            Logger = logger;
            UserAgent = userAgent;
        }
    }
    public class ShowUpdateDialogMessage : ValueChangedMessage<ShowUpdateDialogMessageItems>
    {
        public ShowUpdateDialogMessage(ShowUpdateDialogMessageItems value) : base(value) { }
    }
}
