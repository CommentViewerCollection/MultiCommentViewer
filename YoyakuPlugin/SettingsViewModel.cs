using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;

namespace OpenrecYoyakuPlugin
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Commands
        private RelayCommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new RelayCommand(() =>
                    {
                        _model.RemoveAllUsers();
                    });
                }
                return _clearCommand;
            }
        }
        private RelayCommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(() =>
                    {
                        _model.RemoveCurrentUser();
                    });
                }
                return _deleteCommand;
            }
        }
        private RelayCommand _call5People;
        public ICommand Call5People
        {
            get
            {
                if (_call5People == null)
                {
                    _call5People = new RelayCommand(() =>
                    {
                        _model.CallNPeople(5);
                    });
                }
                return _call5People;
            }
        }
        private RelayCommand _call4People;
        public ICommand Call4People
        {
            get
            {
                if (_call4People == null)
                {
                    _call4People = new RelayCommand(() =>
                    {
                        _model.CallNPeople(4);
                    });
                }
                return _call4People;
            }
        }
        private RelayCommand _call3People;
        public ICommand Call3People
        {
            get
            {
                if (_call3People == null)
                {
                    _call3People = new RelayCommand(() =>
                    {
                        _model.CallNPeople(3);
                    });
                }
                return _call3People;
            }
        }
        private RelayCommand _call2People;
        public ICommand Call2People
        {
            get
            {
                if (_call2People == null)
                {
                    _call2People = new RelayCommand(() =>
                    {
                        _model.CallNPeople(2);
                    });
                }
                return _call2People;
            }
        }
        private RelayCommand _call1People;
        public ICommand Call1People
        {
            get
            {
                if (_call1People == null)
                {
                    _call1People = new RelayCommand(() =>
                    {
                        _model.CallNPeople(1);
                    });
                }
                return _call1People;
            }
        }
        private RelayCommand _showExplainCommand;
        public ICommand ShowExplainCommand
        {
            get
            {
                if (_showExplainCommand == null)
                {
                    _showExplainCommand = new RelayCommand(() =>
                    {
                        _model.WriteExplain();
                    });
                }
                return _showExplainCommand;
            }
        }
        private RelayCommand _removeCalledUsersCommand;
        public ICommand RemoveCalledUsersCommand
        {
            get
            {
                if (_removeCalledUsersCommand == null)
                {
                    _removeCalledUsersCommand = new RelayCommand(() =>
                    {
                        _model.RemoveCalledUsers();
                    });
                }
                return _removeCalledUsersCommand;
            }
        }
        #endregion
        public bool Topmost
        {
            get
            {
                return _topmost;
            }
            set
            {
                if (_topmost == value) return;
                _topmost = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<User> RegisteredUsers { get; } = new ObservableCollection<User>();
        public User SelectedUser
        {
            get => _model.CurrentUser;
            set => _model.CurrentUser = value;
        }
        public bool IsEnabled
        {
            get { return _model.IsEnabled; }
            set { _model.IsEnabled = value; }
        }
        public string Title
        {
            get { return "予約管理plugin for やります！アンコちゃん　のオマージュプラグイン"; }
        }
        private bool _isListSelected;
        private bool _topmost;

        public bool IsListSelected
        {
            get { return _isListSelected; }
            set
            {
                _isListSelected = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ListVisibility));
                RaisePropertyChanged(nameof(SettingsVisibility));
            }
        }
        public Visibility ListVisibility
        {
            get { return IsListSelected ? Visibility.Visible : Visibility.Hidden; }
        }
        public Visibility SettingsVisibility
        {
            get { return IsListSelected ? Visibility.Hidden : Visibility.Visible; }
        }
        public string ReserveCommandPattern
        {
            get { return _model.ReserveCommandPattern; }
            set { _model.ReserveCommandPattern = value; }
        }
        public string DeleteCommandPattern
        {
            get { return _model.DeleteCommandPattern; }
            set { _model.DeleteCommandPattern = value; }
        }
        public string Reserved_Se
        {
            get { return _model.Reserved_Se; }
            set { _model.Reserved_Se = value; }
        }
        public string Reserved_Message
        {
            get { return _model.Reserved_Message; }
            set { _model.Reserved_Message = value; }
        }
        public string Delete_Se
        {
            get { return _model.Delete_Se; }
            set { _model.Delete_Se = value; }
        }
        public string Delete_Message
        {
            get { return _model.Delete_Message; }
            set { _model.Delete_Message = value; }
        }
        public string Call_Se
        {
            get { return _model.Call_Se; }
            set { _model.Call_Se = value; }
        }
        public string Call_Message
        {
            get { return _model.Call_Message; }
            set { _model.Call_Message = value; }
        }
        public string AlreadyReserved_Se
        {
            get { return _model.AlreadyReserved_Se; }
            set { _model.AlreadyReserved_Se = value; }
        }
        public string AlreadyReserved_Message
        {
            get { return _model.AlreadyReserved_Message; }
            set { _model.AlreadyReserved_Message = value; }
        }
        public string HandleNameNotSubscribed_Se
        {
            get { return _model.HandleNameNotSubscribed_Se; }
            set { _model.HandleNameNotSubscribed_Se = value; }
        }
        public string HandleNameNotSubscribed_Message
        {
            get { return _model.HandleNameNotSubscribed_Message; }
            set { _model.HandleNameNotSubscribed_Message = value; }
        }
        public string NotReserved_Se
        {
            get { return _model.NotReserved_Se; }
            set { _model.NotReserved_Se = value; }
        }
        public string NotReserved_Message
        {
            get { return _model.NotReserved_Message; }
            set { _model.NotReserved_Message = value; }
        }
        public string DeleteByOther_Se
        {
            get { return _model.DeleteByOther_Se; }
            set { _model.DeleteByOther_Se = value; }
        }
        public string DeleteByOther_Message
        {
            get { return _model.DeleteByOther_Message; }
            set { _model.DeleteByOther_Message = value; }
        }
        public string Explain_Se
        {
            get { return _model.Explain_Se; }
            set { _model.Explain_Se = value; }
        }
        public string Explain_Message
        {
            get { return _model.Explain_Message; }
            set { _model.Explain_Message = value; }
        }
        public double DateWidth
        {
            get => _model.DateWidth;
            set => _model.DateWidth = value;
        }
        public double IdWidth
        {
            get => _model.IdWidth;
            set => _model.IdWidth = value;
        }
        public double NameWidth
        {
            get => _model.NameWidth;
            set => _model.NameWidth = value;
        }
        public double CalledWidth
        {
            get => _model.CalledWidth;
            set => _model.CalledWidth = value;
        }
        public string TestPattern
        {
            get => _model.TestPattern;
            set => _model.TestPattern = value;
        }
        public string TestComment
        {
            get => _model.TestComment;
            set => _model.TestComment = value;
        }
        public string TestResult
        {
            get
            {
                return _model.TestResult;
            }
        }
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        private readonly Model _model;
        private readonly Dispatcher _dispatcher;
        public SettingsViewModel()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                var options = new DynamicOptions();
                _model = new Model(options, null)
                {
                    TestPattern = "/yoyaku",
                    TestComment = "/yoyaku",
                };
                IsEnabled = true;
                IsListSelected = false;
            }
        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        internal SettingsViewModel(Model model, Dispatcher dispatcher)
        {
            _model = model;
            _dispatcher = dispatcher;
            IsListSelected = true;
            model.PropertyChanged += Model_PropertyChanged;
            _model.UsersListChanged += Model_UsersListChanged;
            RegisteredUsers.CollectionChanged += RegisteredUsers_CollectionChanged;
        }

        private void RegisteredUsers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"Action={e.Action} index {e.OldStartingIndex}->{e.NewStartingIndex}");
            _model.ChangeRegisteredUsers(e.Action, e.OldStartingIndex, e.NewStartingIndex);
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_model.TestResult):
                    RaisePropertyChanged(nameof(TestResult));
                    break;
            }
        }
        private static readonly object _lockObj = new object();
        private void Model_UsersListChanged(object sender, EventArgs e)
        {
            //変更があるたびに全削除して入れ直す
            _dispatcher.Invoke(() =>
            {
                lock (_lockObj)
                {
                    RegisteredUsers.Clear();
                    foreach (var user in _model.RegisteredUsers)
                    {
                        RegisteredUsers.Add(user);
                    }
                }
            });
        }
    }
}
