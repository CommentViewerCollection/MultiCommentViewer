using GalaSoft.MvvmLight.Messaging;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YouTubeLiveCommentViewer
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
    //class ShowUserViewMessage : MessageBase
    //{
    //    public UserViewModel Uvm { get; }
    //    public ShowUserViewMessage(UserViewModel uvm)
    //    {
    //        Uvm = uvm;
    //    }
    //}
    class SetPostCommentPanel : MessageBase
    {
        public UserControl Panel { get; }
        public SetPostCommentPanel(UserControl panel)
        {
            Panel = panel;
        }
    }
}
