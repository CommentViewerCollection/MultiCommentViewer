using System;
using System.Collections.Generic;
using SitePlugin;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace MultiCommentViewer
{
    internal class SetAddingCommentDirection : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public bool IsTop { get; set; }
    }
    class ShowOptionsViewMessage : GalaSoft.MvvmLight.Messaging.MessageBase
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
    class ShowUserListViewMessage : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public ObservableCollection<UserViewModel> UserViewModels { get; }
        public MainViewModel MainVm { get; }
        public IOptions Options { get; }

        public ShowUserListViewMessage(ObservableCollection<UserViewModel> userViewModels, MainViewModel mainVm, IOptions options)
        {
            UserViewModels = userViewModels;
            MainVm = mainVm;
            Options = options;
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
    class SetRawMessagePostPanel : GalaSoft.MvvmLight.Messaging.MessageBase
    {
        public UserControl Panel { get; }
        public SetRawMessagePostPanel(UserControl panel)
        {
            Panel = panel;
        }
    }
}
