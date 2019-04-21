using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Plugin;
using SitePlugin;

namespace OpenrecYoyakuPlugin
{
    public class Model : INotifyPropertyChanged
    {
        private readonly IOptions _options;
        private readonly IPluginHost _host;
        private User _currentUser;

        public string Reserved_Se
        {
            get { return _options.Reserved_Se; }
            set { _options.Reserved_Se = value; }
        }
        public string Reserved_Message
        {
            get { return _options.Reserved_Message; }
            set { _options.Reserved_Message = value; }
        }
        public string Delete_Se
        {
            get { return _options.Delete_Se; }
            set { _options.Delete_Se = value; }
        }
        public string Delete_Message
        {
            get { return _options.Delete_Message; }
            set { _options.Delete_Message = value; }
        }
        public string Call_Se
        {
            get { return _options.Call_Se; }
            set { _options.Call_Se = value; }
        }
        public string Call_Message
        {
            get { return _options.Call_Message; }
            set { _options.Call_Message = value; }
        }
        public string AlreadyReserved_Se
        {
            get { return _options.AlreadyReserved_Se; }
            set { _options.AlreadyReserved_Se = value; }
        }
        public string AlreadyReserved_Message
        {
            get { return _options.AlreadyReserved_Message; }
            set { _options.AlreadyReserved_Message = value; }
        }
        public string HandleNameNotSubscribed_Se
        {
            get { return _options.HandleNameNotSubscribed_Se; }
            set { _options.HandleNameNotSubscribed_Se = value; }
        }
        public string HandleNameNotSubscribed_Message
        {
            get { return _options.HandleNameNotSubscribed_Message; }
            set { _options.HandleNameNotSubscribed_Message = value; }
        }
        public string NotReserved_Se
        {
            get { return _options.NotReserved_Se; }
            set { _options.NotReserved_Se = value; }
        }
        public string NotReserved_Message
        {
            get { return _options.NotReserved_Message; }
            set { _options.NotReserved_Message = value; }
        }
        public string DeleteByOther_Se
        {
            get { return _options.DeleteByOther_Se; }
            set { _options.DeleteByOther_Se = value; }
        }
        public string DeleteByOther_Message
        {
            get { return _options.DeleteByOther_Message; }
            set { _options.DeleteByOther_Message = value; }
        }
        public string Explain_Se
        {
            get { return _options.Explain_Se; }
            set { _options.Explain_Se = value; }
        }
        public string Explain_Message
        {
            get { return _options.Explain_Message; }
            set { _options.Explain_Message = value; }
        }
        public double DateWidth
        {
            get => _options.DateWidth;
            set => _options.DateWidth = value;
        }
        public double IdWidth
        {
            get => _options.IdWidth;
            set => _options.IdWidth = value;
        }
        public double NameWidth
        {
            get => _options.NameWidth;
            set => _options.NameWidth = value;
        }
        public double CalledWidth
        {
            get => _options.CalledWidth;
            set => _options.CalledWidth = value;
        }
        public Model(IOptions options, IPluginHost host)
        {
            _options = options;
            _host = host;
        }
        /// <summary>
        /// 登録済みのN人を呼び出す
        /// </summary>
        /// <param name="n"></param>
        public void CallNPeople(int n)
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
        public List<User> RegisteredUsers { get; } = new List<User>();
        public void AddUser(User user)
        {
            lock (RegisteredUsers)
            {
                RegisteredUsers.Add(user);
            }
            RaiseUsersListChanged();
        }
        private void RemoveUser(User user)
        {
            lock (RegisteredUsers)
            {
                RegisteredUsers.Remove(user);
            }
            RaiseUsersListChanged();
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
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
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
        public void SetComment(string userId, string name, string comment, IUser user, Guid siteContextGuid)
        {
            //"/yoyaku"
            //"/torikeshi"
            //"/kakunin"

            if (string.IsNullOrEmpty(userId)) return;

            string defaultName;
            if (user != null && !string.IsNullOrEmpty(user.Nickname))
            {
                defaultName = user.Nickname;
            }
            else
            {
                defaultName = name;
            }

            if (IsYoyakuCommand(comment))
            {
                if (FindUser(userId) == null)
                {
                    if(siteContextGuid == Guid.Empty)
                    {
                        Debugger.Break();
                    }
                    //未登録なら登録する
                    var pluginUser = new User(user) { Date = GetCurrentDateTime(), Id = userId, Name = defaultName, HadCalled = false, SitePluginGuid = siteContextGuid };
                    AddUser(pluginUser);
                    WriteComment(Reserved_Message.Replace("$name", defaultName));
                }
            }
            else if (IsTorikeshiCommand(comment))
            {
                var pluginUser = FindUser(userId);
                if (pluginUser != null)
                {
                    //登録済みなら取り消す
                    RemoveUser(pluginUser);
                    WriteComment(Delete_Message.Replace("$name", defaultName));
                }
            }
            else if (IsKakuninCommand(comment))
            {
                //未登録の場合は何も表示されない。
                lock (RegisteredUsers)
                {
                    var i = 0;
                    foreach (var pluginUser in RegisteredUsers)
                    {
                        if (pluginUser.Id == userId && !pluginUser.HadCalled)
                        {
                            WriteComment($"ただいまの予約人数は{RegisteredUsers.Count}名です。{defaultName}さんは、{i + 1}番目です");
                            break;
                        }
                        if (!pluginUser.HadCalled)
                            i++;
                    }
                }
            }
        }
        /// <summary>
        /// 呼び出し済みのユーザを削除する
        /// </summary>
        public void RemoveCalledUsers()
        {
            lock (RegisteredUsers)
            {
                var toRemove = new List<User>();
                foreach (var user in RegisteredUsers)
                {
                    if (user.HadCalled)
                        toRemove.Add(user);
                }
                foreach (var removeUser in toRemove)
                {
                    RemoveUser(removeUser);
                }
            }
            RaiseUsersListChanged();
        }
        public void RemoveAllUsers()
        {
            RegisteredUsers.Clear();
            RaiseUsersListChanged();
        }
        public void RemoveCurrentUser()
        {
            if (CurrentUser != null)
            {
                RemoveUser(CurrentUser);
                CurrentUser = null;
            }
        }
        public void WriteExplain()
        {
            WriteComment(Explain_Message);
        }
        private void RaiseUsersListChanged()
        {
            UsersListChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler UsersListChanged;
        public User CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                _currentUser = value;
                RaisePropertyChanged();
            }
        }
        public bool IsEnabled
        {
            get => _options.IsEnabled;
            set => _options.IsEnabled = value;
        }
        private void WriteComment(string comment)
        {
            _host.PostCommentToAll(comment);
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
