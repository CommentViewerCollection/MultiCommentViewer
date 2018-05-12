using GalaSoft.MvvmLight.Messaging;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TwicasCommentViewer
{
    internal class MainViewCloseMessage : MessageBase { }
    internal class SetAddingCommentDirection : MessageBase
    {
        public bool IsTop { get; set; }
    }
    class ShowOptionsViewMessage : MessageBase
    {
        public IEnumerable<IOptionsTabPage> Tabs { get; }
        public ShowOptionsViewMessage(IEnumerable<IOptionsTabPage> tabs)
        {
            Tabs = tabs;
        }
    }
    class ShowUserViewMessage : MessageBase
    {
        public  ViewModel.UserViewModel Uvm { get; }
        public ShowUserViewMessage(ViewModel.UserViewModel uvm)
        {
            Uvm = uvm;
        }
    }
    class ShowUserListViewMessage : MessageBase
    {
        public ObservableCollection<ViewModel.UserViewModel> UserViewModels { get; }
        public ShowUserListViewMessage(ObservableCollection<ViewModel.UserViewModel> userViewModels)
        {
            UserViewModels = userViewModels;
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
