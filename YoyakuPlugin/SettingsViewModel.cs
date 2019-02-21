using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Globalization;
using System.Collections.Concurrent;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;
using System.Net;
using Plugin;

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
                        RegisteredUsers.Clear();
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
                        if(SelectedUser != null)
                        {
                            RegisteredUsers.Remove(SelectedUser);
                            SelectedUser = null;
                        }
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
                        CallNPeople(5);
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
                        CallNPeople(4);
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
                        CallNPeople(3);
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
                        CallNPeople(2);
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
                        CallNPeople(1);
                    });
                }
                return _call1People;
            }
        }
        /// <summary>
        /// 登録済みのN人を呼び出す
        /// </summary>
        /// <param name="n"></param>
        private void CallNPeople(int n)
        {
            var toCall = new List<User>();

            //N人をピックアップ
            lock (RegisteredUsers)
            {
                foreach (var user in RegisteredUsers)
                {
                    if (toCall.Count >= n)
                        break;
                    if (!user.HadCalled)
                        toCall.Add(user);
                }
            }

            //呼び出しコメント
            if (toCall.Count > 0)
            {
                var names = string.Join("、", toCall.Select(user => user.Name + "さん"));
                var s = Call_Message.Replace("$name", names);
                WriteComment(s);
            }

            //呼出済にする
            foreach (var called in toCall)
            {
                called.HadCalled = true;
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
                        WriteComment(Explain_Message);
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
                        //呼出済のユーザを削除する
                        var toRemove = new List<User>();
                        lock (RegisteredUsers)
                        {
                            foreach (var user in RegisteredUsers)
                            {
                                if (user.HadCalled)
                                    toRemove.Add(user);
                            }
                            foreach (var removeUser in toRemove)
                            {
                                RegisteredUsers.Remove(removeUser);
                            }
                        }
                    });
                }
                return _removeCalledUsersCommand;
            }
        }
        private RelayCommand _saveAndShowListCommand;
        public ICommand SaveAndShowListCommand
        {
            get
            {
                if (_saveAndShowListCommand == null)
                {
                    _saveAndShowListCommand = new RelayCommand(() =>
                    {
                        Save();
                        IsListSelected = true;
                    });
                }
                return _saveAndShowListCommand;
            }
        }

        private RelayCommand _loadSettingsCommand;
        public ICommand LoadSettingsCommand
        {
            get
            {
                if (_loadSettingsCommand == null)
                {
                    _loadSettingsCommand = new RelayCommand(() =>
                    {
                        Load();
                    });
                }
                return _loadSettingsCommand;
            }
        }
        private RelayCommand _defaultCommand;
        public ICommand DefaultCommand
        {
            get
            {
                if (_defaultCommand == null)
                {
                    _defaultCommand = new RelayCommand(() =>
                    {
                        _options.Reset();
                    });
                }
                return _defaultCommand;
            }
        }
        #endregion

        private ObservableCollection<User> _registeredUsers = new ObservableCollection<User>();
        public ObservableCollection<User> RegisteredUsers { get { return _registeredUsers; } }
        public User SelectedUser { get; set; }
        public bool IsEnabled
        {
            get { return _options.IsEnabled; }
            set
            {
                _options.IsEnabled = value;
                RaisePropertyChanged();
            }
        }
        public string Title
        {
            get { return "予約管理plugin for やります！アンコちゃん　のオマージュプラグイン"; }
        }
        private bool _isListSelected;
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
        private string _reserved_Se;
        public string Reserved_Se
        {
            get { return _reserved_Se; }
            set
            {
                _reserved_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _reserved_Message;
        public string Reserved_Message
        {
            get { return _reserved_Message; }
            set
            {
                _reserved_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _delete_Se;
        public string Delete_Se
        {
            get { return _delete_Se; }
            set
            {
                _delete_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _delete_Message;
        public string Delete_Message
        {
            get { return _delete_Message; }
            set
            {
                _delete_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _call_Se;
        public string Call_Se
        {
            get { return _call_Se; }
            set
            {
                _call_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _call_Message;
        public string Call_Message
        {
            get { return _call_Message; }
            set
            {
                _call_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _alreadyReserved_Se;
        public string AlreadyReserved_Se
        {
            get { return _alreadyReserved_Se; }
            set
            {
                _alreadyReserved_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _alreadyReserved_Message;
        public string AlreadyReserved_Message
        {
            get { return _alreadyReserved_Message; }
            set
            {
                _alreadyReserved_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _handleNameNotSubscribed_Se;
        public string HandleNameNotSubscribed_Se
        {
            get { return _handleNameNotSubscribed_Se; }
            set
            {
                _handleNameNotSubscribed_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _handleNameNotSubscribed_Message;
        public string HandleNameNotSubscribed_Message
        {
            get { return _handleNameNotSubscribed_Message; }
            set
            {
                _handleNameNotSubscribed_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _notReserved_Se;
        public string NotReserved_Se
        {
            get { return _notReserved_Se; }
            set
            {
                _notReserved_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _notReserved_Message;
        public string NotReserved_Message
        {
            get { return _notReserved_Message; }
            set
            {
                _notReserved_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _deleteByOther_Se;
        public string DeleteByOther_Se
        {
            get { return _deleteByOther_Se; }
            set
            {
                _deleteByOther_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _deleteByOther_Message;
        public string DeleteByOther_Message
        {
            get { return _deleteByOther_Message; }
            set
            {
                _deleteByOther_Message = value;
                RaisePropertyChanged();
            }
        }
        private string _explain_Se;
        public string Explain_Se
        {
            get { return _explain_Se; }
            set
            {
                _explain_Se = value;
                RaisePropertyChanged();
            }
        }
        private string _explain_Message;
        public string Explain_Message
        {
            get { return _explain_Message; }
            set
            {
                _explain_Message = value;
                RaisePropertyChanged();
            }
        }

        private void WriteComment(string comment)
        {
            _host.PostCommentToAll(comment);
        }
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        public void SetComment(string userId, string nickname, string comment)
        {
            //"/yoyaku"
            //"/torikeshi"
            //"/kakunin"

            if (string.IsNullOrEmpty(userId)) return;

            if (IsYoyakuCommand(comment))
            {
                lock (RegisteredUsers)
                {
                    if (FindUser(userId) == null)
                    {
                        //未登録なら登録する
                        _dispatcher.Invoke(() =>
                        {
                            var user = new User { Date = GetCurrentDateTime(), Id = userId, Name = nickname, HadCalled = false };
                            RegisteredUsers.Add(user);
                            WriteComment(Reserved_Message.Replace("$name", nickname));
                        });
                    }
                }
            }
            else if (IsTorikeshiCommand(comment))
            {
                lock (RegisteredUsers)
                {
                    var user = FindUser(userId);
                    if (user != null)
                    {
                        //登録済みなら取り消す
                        _dispatcher.Invoke(() =>
                        {
                            RegisteredUsers.Remove(user);
                            WriteComment(Delete_Message.Replace("$name", nickname));
                        });
                    }
                }
            }
            else if (IsKakuninCommand(comment))
            {
                //未登録の場合は何も表示されない。
                lock (RegisteredUsers)
                {
                    var i = 0;
                    foreach (var user in RegisteredUsers)
                    {
                        if (user.Id == userId && !user.HadCalled)
                        {
                            WriteComment($"ただいまの予約人数は{RegisteredUsers.Count}名です。{nickname}さんは、{i + 1}番目です");
                            break;
                        }
                        if (!user.HadCalled)
                            i++;
                    }
                }
            }
        }
        public void SetComment(ICommentData commentData)
        {
            //"/yoyaku"
            //"/torikeshi"
            //"/kakunin"

            if (IsYoyakuCommand(commentData))
            {
                if (FindUser(commentData.UserId) == null)
                {
                    //未登録なら登録する
                    _dispatcher.Invoke(() =>
                    {
                        var user = new User { Date = DateTime.Now, Id = commentData.UserId, Name = commentData.Nickname, HadCalled = false };
                        RegisteredUsers.Add(user);
                        WriteComment(Reserved_Message.Replace("$name", commentData.Nickname));
                    });
                }
            }
            else if (IsTorikeshiCommand(commentData))
            {
                var user = FindUser(commentData.UserId);
                if (user != null)
                {
                    //登録済みなら取り消す
                    _dispatcher.Invoke(() =>
                    {
                        RegisteredUsers.Remove(user);
                        WriteComment(Delete_Message.Replace("$name", commentData.Nickname));
                    });
                }
            }
            else if (IsKakuninCommand(commentData))
            {
                //未登録の場合は何も表示されない。
                var userId = commentData.UserId;
                lock (RegisteredUsers)
                {
                    var i = 0;
                    foreach (var user in RegisteredUsers)
                    {
                        if (user.Id == userId && !user.HadCalled)
                        {
                            WriteComment($"ただいまの予約人数は{RegisteredUsers.Count}名です。{commentData.Nickname}さんは、{i + 1}番目です");
                            break;
                        }
                        if (!user.HadCalled)
                            i++;
                    }
                }
            }
        }
        private User FindUser(string userId)
        {
            lock (RegisteredUsers)
            {
                foreach (var user in RegisteredUsers)
                {
                    if (user.Id == userId)
                        return user;
                }
            }
            return null;
        }
        private bool IsYoyakuCommand(ICommentData data)
        {
            return data.Comment.StartsWith("/yoyaku") || data.Comment.StartsWith("/予約");
        }
        private bool IsTorikeshiCommand(ICommentData data)
        {
            return data.Comment.StartsWith("/torikeshi") || data.Comment.StartsWith("/取消");
        }
        private bool IsKakuninCommand(ICommentData data)
        {
            return data.Comment.StartsWith("/kakunin") || data.Comment.StartsWith("/確認");
        }
        private bool IsYoyakuCommand(string comment)
        {
            return comment.StartsWith("/yoyaku") || comment.StartsWith("/予約");
        }
        private bool IsTorikeshiCommand(string comment)
        {
            return comment.StartsWith("/torikeshi") || comment.StartsWith("/取消");
        }
        private bool IsKakuninCommand(string comment)
        {
            return comment.StartsWith("/kakunin") || comment.StartsWith("/確認");
        }
        private void Save()
        {
            _options.Reserved_Se = Reserved_Se;
            _options.Reserved_Message = Reserved_Message;
            _options.Delete_Se = Delete_Se;
            _options.Delete_Message = Delete_Message;
            _options.Call_Se = Call_Se;
            _options.Call_Message = Call_Message;
            _options.AlreadyReserved_Se = AlreadyReserved_Se;
            _options.AlreadyReserved_Message = AlreadyReserved_Message;
            _options.HandleNameNotSubscribed_Se = HandleNameNotSubscribed_Se;
            _options.HandleNameNotSubscribed_Message = HandleNameNotSubscribed_Message;
            _options.NotReserved_Se = NotReserved_Se;
            _options.NotReserved_Message = NotReserved_Message;
            _options.DeleteByOther_Se = DeleteByOther_Se;
            _options.DeleteByOther_Message = DeleteByOther_Message;
            _options.Explain_Se = Explain_Se;
            _options.Explain_Message = Explain_Message;
        }
        private void Load()
        {
            Reserved_Se = _options.Reserved_Se;
            Reserved_Message = _options.Reserved_Message;
            Delete_Se = _options.Delete_Se;
            Delete_Message = _options.Delete_Message;
            Call_Se = _options.Call_Se;
            Call_Message = _options.Call_Message;
            AlreadyReserved_Se = _options.AlreadyReserved_Se;
            AlreadyReserved_Message = _options.AlreadyReserved_Message;
            HandleNameNotSubscribed_Se = _options.HandleNameNotSubscribed_Se;
            HandleNameNotSubscribed_Message = _options.HandleNameNotSubscribed_Message;
            NotReserved_Se = _options.NotReserved_Se;
            NotReserved_Message = _options.NotReserved_Message;
            DeleteByOther_Se = _options.DeleteByOther_Se;
            DeleteByOther_Message = _options.DeleteByOther_Message;
            Explain_Se = _options.Explain_Se;
            Explain_Message = _options.Explain_Message;
        }
        private readonly IPluginHost _host;
        private readonly IOptions _options;
        private readonly Dispatcher _dispatcher;
        public SettingsViewModel()
        {
            if (IsInDesignMode)
            {
                _options = new DynamicOptions();
                IsEnabled = true;
                IsListSelected = false;
            }
        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public SettingsViewModel(IPluginHost host, IOptions options, Dispatcher dispatcher)
        {
            _host = host;
            _options = options;
            _dispatcher = dispatcher;
            IsListSelected = true;
            Load();
        }
    }
    public class User : ViewModelBase
    {
        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                RaisePropertyChanged();
            }
        }
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        private bool _hasCalled;
        /// <summary>
        /// 呼び出し済みか
        /// </summary>
        public bool HadCalled
        {
            get { return _hasCalled; }
            set
            {
                _hasCalled = value;
                RaisePropertyChanged();
            }
        }
        public override string ToString()
        {
            return $"{Name} id={Id}";
        }
        public override bool Equals(object obj)
        {
            if (!(obj is User user))
                return false;
            if (this.Id == null) return false;
            return Id.Equals(user.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
