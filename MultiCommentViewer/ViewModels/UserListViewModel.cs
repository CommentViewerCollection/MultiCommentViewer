using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace MultiCommentViewer.ViewModels
{
    public class UserListViewModel : ViewModelBase
    {
        public ICommand ShowUserInfoCommand { get; }
        private readonly MainViewModel _mainVm;

        public ObservableCollection<UserViewModel> Users { get; }

        public UserListViewModel()
        {

        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public UserListViewModel(ObservableCollection<UserViewModel> uvms, MainViewModel mainVm)
        {
            //TODO:本来はModelを受けとりたいところだけど、現状ではMainViewModelがModelの責務も持ってしまっている
            Users = uvms;
            _mainVm = mainVm;
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
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
