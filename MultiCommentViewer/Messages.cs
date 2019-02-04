using System;
using System.Collections.Generic;
using SitePlugin;
using System.Windows.Controls;

namespace MultiCommentViewer
{
    class ShowOptionsViewMessage: GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public IEnumerable<IOptionsTabPage> Tabs { get; }
        public ShowOptionsViewMessage(IEnumerable<IOptionsTabPage> tabs)
        {
            Tabs = tabs;
        }
    }
    class ShowUserViewMessage : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public UserViewModel Uvm { get; }
        public ShowUserViewMessage(UserViewModel uvm)
        {
            Uvm = uvm;
        }
    }
    class SetPostCommentPanel : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public UserControl Panel { get; }
        public SetPostCommentPanel(UserControl panel)
        {
            Panel = panel;
        }
    }
}
