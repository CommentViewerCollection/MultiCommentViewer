using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiCommentViewer.ViewModels
{
    public class UserListViewModel : ObservableObject
    {
        public ICommand ShowUserInfoCommand { get; }
        private readonly MainViewModel _mainVm;
        private readonly IOptions _options;

        public ObservableCollection<UserViewModel> Users { get; }
        public Brush CommentListBackground => new SolidColorBrush(_options.CommentListBackColor);
        public Brush CommentListBorderBrush => new SolidColorBrush(_options.CommentListBorderColor);
        public UserListViewModel()
        {

        }
        public UserListViewModel(ObservableCollection<UserViewModel> uvms, MainViewModel mainVm, IOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            //TODO:本来はModelを受けとりたいところだけど、現状ではMainViewModelがModelの責務も持ってしまっている
            Users = uvms;
            _mainVm = mainVm;
            _options = options;
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.CommentListBackColor):
                        OnPropertyChanged(nameof(CommentListBackground));
                        break;
                    case nameof(options.CommentListBorderColor):
                        OnPropertyChanged(nameof(CommentListBorderBrush));
                        break;
                }
            };
        }
        public UserViewModel SelectedUser { get; set; }
        private void ShowUserInfo()
        {
            var user = SelectedUser;
            var userId = user.UserId;
            _mainVm.ShowUserInfo(userId);
        }
    }
}
