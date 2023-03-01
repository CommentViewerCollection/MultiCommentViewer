using System;
using System.Collections.Generic;
using SitePlugin;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MultiCommentViewer
{
    internal class SetAddingCommentDirectionItems
    {
        public bool IsTop { get; set; }
    }
    internal class SetAddingCommentDirection : ValueChangedMessage<SetAddingCommentDirectionItems>
    {
        public SetAddingCommentDirection(SetAddingCommentDirectionItems value) : base(value)
        {
        }
    }

    class ShowOptionsViewMessageItems
    {
        public IEnumerable<IOptionsTabPage> Tabs { get; }
        public ShowOptionsViewMessageItems(IEnumerable<IOptionsTabPage> tabs)
        {
            Tabs = tabs;
        }
    }
    class ShowOptionsViewMessage : ValueChangedMessage<ShowOptionsViewMessageItems>
    {
        public ShowOptionsViewMessage(ShowOptionsViewMessageItems value) : base(value)
        {
        }
    }
    class ShowUserViewMessageItems
    {
        public UserViewModel Uvm { get; }
        public ShowUserViewMessageItems(UserViewModel uvm)
        {
            Uvm = uvm;
        }
    }
    class ShowUserViewMessage : ValueChangedMessage<ShowUserViewMessageItems>
    {
        public ShowUserViewMessage(ShowUserViewMessageItems value) : base(value)
        {
        }
    }
    class ShowUserListViewMessageItems
    {
        public ObservableCollection<UserViewModel> UserViewModels { get; }
        public MainViewModel MainVm { get; }
        public IOptions Options { get; }

        public ShowUserListViewMessageItems(ObservableCollection<UserViewModel> userViewModels, MainViewModel mainVm, IOptions options)
        {
            UserViewModels = userViewModels;
            MainVm = mainVm;
            Options = options;
        }
    }
    class ShowUserListViewMessage : ValueChangedMessage<ShowUserListViewMessageItems>
    {
        public ShowUserListViewMessage(ShowUserListViewMessageItems value) : base(value)
        {
        }
    }
    class SetPostCommentPanelItems
    {
        public UserControl Panel { get; }
        public SetPostCommentPanelItems(UserControl panel)
        {
            Panel = panel;
        }
    }
    class SetPostCommentPanel : ValueChangedMessage<SetPostCommentPanelItems>
    {
        public SetPostCommentPanel(SetPostCommentPanelItems value) : base(value)
        {
        }
    }
    class SetRawMessagePostPanelItems
    {
        public UserControl Panel { get; }
        public SetRawMessagePostPanelItems(UserControl panel)
        {
            Panel = panel;
        }
    }
    class SetRawMessagePostPanel : ValueChangedMessage<SetRawMessagePostPanelItems>
    {
        public SetRawMessagePostPanel(SetRawMessagePostPanelItems value) : base(value)
        {
        }
    }
}
