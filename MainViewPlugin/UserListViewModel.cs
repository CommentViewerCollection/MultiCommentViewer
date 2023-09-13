using CommunityToolkit.Mvvm.Input;
using Mcv.MainViewPlugin;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiCommentViewer.ViewModels
{
    class UserListViewModel : ViewModelBase
    {
        public ICommand ShowUserInfoCommand { get; }
        private readonly ObservableCollection<IMcvCommentViewModel> _comments;
        private readonly IMainViewPluginOptions _options;
        private readonly IAdapter _adapter;
        private readonly IUserViewModelProvider _userProvider;

        public ObservableCollection<UserViewModel> Users { get; }
        public Brush CommentListBackground => new SolidColorBrush(_options.CommentListBackColor);
        public Brush CommentListBorderBrush => new SolidColorBrush(_options.CommentListBorderColor);
        public UserListViewModel()
        {
            if (DesignModeUtils.IsDesignMode)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public UserListViewModel(ObservableCollection<UserViewModel> uvms, ObservableCollection<IMcvCommentViewModel> comments, IMainViewPluginOptions options, IAdapter adapter, IUserViewModelProvider userProvider)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            //TODO:本来はModelを受けとりたいところだけど、現状ではMainViewModelがModelの責務も持ってしまっている
            Users = uvms;
            _comments = comments;
            _options = options;
            _adapter = adapter;
            _userProvider = userProvider;
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.CommentListBackColor):
                        RaisePropertyChanged(nameof(CommentListBackground));
                        break;
                    case nameof(options.CommentListBorderColor):
                        RaisePropertyChanged(nameof(CommentListBorderBrush));
                        break;
                }
            };
        }
        public UserViewModel? SelectedUser { get; set; }
        private void ShowUserInfo()
        {
            var user = SelectedUser;
            if (user is null)
            {
                return;
            }
            var userId = user.UserId;
            MainViewModel.ShowUserInfo(userId, _comments, _options, _adapter, _userProvider);
        }
    }
}
