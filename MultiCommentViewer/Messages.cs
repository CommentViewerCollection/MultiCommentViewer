using System;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using SitePlugin;
namespace MultiCommentViewer
{
    class ShowOptionsViewMessage: MessageBase
    {
        public IEnumerable<IOptionsTabPage> Tabs { get; }
        public ShowOptionsViewMessage(IEnumerable<IOptionsTabPage> tabs)
        {
            Tabs = tabs;
        }
    }
    class ShowUserViewMessage : MessageBase
    {
        public UserViewModel Uvm { get; }
        public ShowUserViewMessage(UserViewModel uvm)
        {
            Uvm = uvm;
        }
    }
}
