using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace MultiCommentViewer.ViewModels
{
    public class UserListViewModel : ViewModelBase
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
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
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
                        RaisePropertyChanged(nameof(CommentListBackground));
                        break;
                    case nameof(options.CommentListBorderColor):
                        RaisePropertyChanged(nameof(CommentListBorderBrush));
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
