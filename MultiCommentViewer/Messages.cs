using System;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using SitePlugin;
using System.Windows.Controls;

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
    class SetPostCommentPanel : MessageBase
    {
        public UserControl Panel { get; }
        public SetPostCommentPanel(UserControl panel)
        {
            Panel = panel;
        }
    }
}
