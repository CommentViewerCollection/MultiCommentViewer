using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MultiCommentViewer.Views.Mirrativ
{
    class RawMessagePostPanelViewModel : ObservableObject
    {
        private readonly ICommentProvider _cp;
        private string _input;

        public string Input
        {
            get
            {
                return _input;
            }
            set
            {
                _input = value;
                OnPropertyChanged();
            }
        }
        public ICommand PostCommand { get; }
        public RawMessagePostPanelViewModel(ICommentProvider cp)
        {
            _cp = cp;
            PostCommand = new RelayCommand(Post);
        }
        private void Post()
        {
            var raw = Input;
            _cp.SetMessage(raw);
            Input = "";
        }
    }
}
