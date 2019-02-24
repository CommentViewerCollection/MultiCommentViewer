﻿using System.ComponentModel;

namespace TwitchSitePlugin
{
    interface ITwitchSiteOptions: INotifyPropertyChanged
    {
        bool NeedAutoSubNickname { get; }
    }
}
