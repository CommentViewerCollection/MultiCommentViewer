using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
namespace NicoSitePlugin.Test2
{
    class CommentPostPanelViewModel : ViewModelBase
    {
        private readonly NicoCommentProvider3 _nicoCommentProvider;
        private readonly ILogger _logger;
        public bool Is184 { get; set; }
        public string Comment { get; set; }
        public ICommand PostCommentCommand { get; }
        private async void PostComment()
        {
            var is184 = Is184;
            var comment = Comment;
            var mail = is184 ? "184" : "";
            try
            {
                await _nicoCommentProvider.PostCommentAsync(comment, mail);
            }catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        /// <summary>
        /// デザイナ用
        /// </summary>
        public CommentPostPanelViewModel()
        {
            if (IsInDesignMode)
            {

            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public CommentPostPanelViewModel(NicoCommentProvider3 nicoCommentProvider, ILogger logger)
        {
            _nicoCommentProvider = nicoCommentProvider;
            _logger = logger;
            PostCommentCommand = new RelayCommand(PostComment);

        }
    }
}
