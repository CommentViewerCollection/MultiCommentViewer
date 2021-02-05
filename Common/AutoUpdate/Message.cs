using System;
using GalaSoft.MvvmLight.Messaging;
namespace Common.AutoUpdate
{
    [Obsolete]
    public class ShowUpdateDialogMessage : MessageBase
    {
        public Version CurrentVersion { get; }
        public LatestVersionInfo LatestVersionInfo { get; }
        public bool IsUpdateExists { get; private set; }
        public ILogger Logger { get; }
        public string UserAgent { get; }

        public ShowUpdateDialogMessage(bool isUpdateExists, Version currentVersion, AutoUpdate.LatestVersionInfo latestInfo, ILogger logger, string userAgent)
        {
            this.IsUpdateExists = isUpdateExists;
            CurrentVersion = currentVersion;
            LatestVersionInfo = latestInfo;
            Logger = logger;
            UserAgent = userAgent;
        }
    }
}
