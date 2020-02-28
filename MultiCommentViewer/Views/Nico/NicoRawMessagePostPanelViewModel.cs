using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MultiCommentViewer.Views.Nico
{
    class NicoRawMessagePostPanelViewModel : ViewModelBase
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
                RaisePropertyChanged();
            }
        }
        public ICommand PostCommand { get; }
        public NicoRawMessagePostPanelViewModel(ICommentProvider cp)
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
