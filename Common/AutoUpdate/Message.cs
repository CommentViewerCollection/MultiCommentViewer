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
        public ShowUpdateDialogMessage(bool isUpdateExists, Version currentVersion, AutoUpdate.LatestVersionInfo latestInfo, ILogger logger)
        {
            this.IsUpdateExists = isUpdateExists;
            CurrentVersion = currentVersion;
            LatestVersionInfo = latestInfo;
            Logger = logger;
        }
    }
}
