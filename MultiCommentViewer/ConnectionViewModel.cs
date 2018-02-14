using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using SitePlugin;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Common;

namespace MultiCommentViewer
{
    public class ConnectionViewModel:ViewModelBase
    {
        public ConnectionName ConnectionName => _connectionName;
        public string Name
        {
            get { return _connectionName.Name; }
            set { _connectionName.Name = value; }
         }
        public bool IsSelected { get; set; }
        public ObservableCollection<SiteViewModel> Sites { get; }
        public ObservableCollection<BrowserViewModel> Browsers { get; }
        private SiteViewModel _selectedSite;
        private ICommentProvider _commentProvider=null;
        private readonly ConnectionName _connectionName;
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public SiteViewModel SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                //一番最初は_commentProviderはnull
                var before = _commentProvider;
                if (before != null)
                {
                    Debug.Assert(before.CanConnect, "接続中に変更はできない");
                    before.CanConnectChanged -= CommentProvider_CanConnectChanged;
                    before.CanDisconnectChanged -= CommentProvider_CanDisconnectChanged;
                    before.CommentReceived -= CommentProvider_CommentReceived;
                    before.MetadataUpdated -= CommentProvider_MetadataUpdated;
                }
                _selectedSite = value;
                var next = _commentProvider = _selectedSite.Site.CreateCommentProvider(_connectionName);
                next.CanConnectChanged += CommentProvider_CanConnectChanged;
                next.CanDisconnectChanged += CommentProvider_CanDisconnectChanged;
                next.CommentReceived += CommentProvider_CommentReceived;
                next.MetadataUpdated += CommentProvider_MetadataUpdated;

                RaisePropertyChanged();
            }
        }
        
        private void CommentProvider_MetadataUpdated(object sender, IMetadata e)
        {
            MetadataReceived?.Invoke(this, e);//senderはConnection
        }
        public event EventHandler<RenamedEventArgs> Renamed;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataReceived;
        private void CommentProvider_CommentReceived(object sender, ICommentViewModel e)
        {
            CommentReceived?.Invoke(sender, e);
        }

        private void CommentProvider_CanDisconnectChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanDisconnect));
        }

        private void CommentProvider_CanConnectChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanConnect));
        }

        private BrowserViewModel _selectedBrowser;
        public BrowserViewModel SelectedBrowser
        {
            get { return _selectedBrowser; }
            set
            {
                _selectedBrowser = value;
                RaisePropertyChanged();
            }
        }
        public bool CanConnect
        {
            get { return _commentProvider.CanConnect; }
        }
        public bool CanDisconnect
        {
            get { return _commentProvider.CanDisconnect; }
        }
        public string Input { get; set; }
        


        private async void Connect()
        {
            try
            {
                var input = Input;
                var browser = SelectedBrowser.Browser;
                await _commentProvider.ConnectAsync(input, browser);


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Disconnect()
        {
            try
            {
                _commentProvider.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        string _beforeName;
        private readonly ILogger _logger;
        public ConnectionViewModel(ConnectionName connectionName, IEnumerable<SiteViewModel> sites, IEnumerable<BrowserViewModel> browsers, ILogger logger)
        {
            _logger = logger;
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _beforeName = _connectionName.Name;
            _connectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_connectionName.Name):
                        var newName = _connectionName.Name;
                        Renamed?.Invoke(this, new RenamedEventArgs(_beforeName, newName));
                        _beforeName = newName;
                        break;
                }
            };

            if(sites == null)
            {
                throw new ArgumentNullException(nameof(sites));
            }
            Sites = new ObservableCollection<SiteViewModel>(sites);
            if(Sites.Count > 0)
            {
                SelectedSite = Sites[0];
            }

            Browsers = new ObservableCollection<BrowserViewModel>(browsers);
            if(Browsers.Count > 0)
            {
                SelectedBrowser = Browsers[0];
            }
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);            
        }
    }
    public class RenamedEventArgs : EventArgs
    {
        public string NewValue { get; }
        public string OldValue { get; }
        public RenamedEventArgs(string oldValue, string newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
