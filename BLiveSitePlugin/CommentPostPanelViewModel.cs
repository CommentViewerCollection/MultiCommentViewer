using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
namespace BLiveSitePlugin
{
    class CommentPostPanelViewModel : ViewModelBase
    {
        public ICommand PostCommentCommand { get; }

        #region Input
        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                if (_input == value) return;
                _input = value;
                RaisePropertyChanged();
            }
        }
        #endregion //Input

        #region CanPostComment
        private bool _canPostComment;
        public bool CanPostComment
        {
            get { return _canPostComment; }
            set
            {
                if (_canPostComment == value) return;
                _canPostComment = value;
                RaisePropertyChanged();
            }
        }
        #endregion //CanPostComment
        private void PostComment(string str)
        {

        }

        private readonly CommentProvider _commentProvider;
        private readonly ILogger _logger;


        #region Ctors
        public CommentPostPanelViewModel()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {

            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public CommentPostPanelViewModel(CommentProvider commentProvider, ILogger logger)
        {
            _commentProvider = commentProvider;
            _logger = logger;
            PostCommentCommand = new RelayCommand(()=> PostComment(Input));

            Input = "コメント投稿は未対応です";
            CanPostComment = false;
        }
        #endregion
    }
}
