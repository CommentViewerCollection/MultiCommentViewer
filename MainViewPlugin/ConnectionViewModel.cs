using CommunityToolkit.Mvvm.Input;
using Mcv.PluginV2;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Mcv.MainViewPlugin;

class ConnectionViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _input;
    private SiteViewModel _selectedSite;
    private BrowserViewModel _selectedBrowser;
    private bool _canConnect;
    private bool _canDisconnect;
    public ConnectionViewModel(IAdapter adapter, IConnectionStatus connSt, ObservableCollection<SiteViewModel> sites, ObservableCollection<BrowserViewModel> browsers, SiteViewModel selectedSite, BrowserViewModel selectedBrowser, ConnectionName connName)
    {
        Id = connSt.Id;
        _adapter = adapter;
        //SetInput(connSt.Input);
        Sites = sites;
        Browsers = browsers;
        ConnectionName = connName;
        SetSelectedSite(selectedSite);
        SetSelectedBrowser(selectedBrowser);
        IsConnected = connSt.IsConnected;
        SetCanConnect(connSt.CanConnect);
        SetCanDisconnect(connSt.CanDisconnect);

        ConnectCommand = new RelayCommand(Connect);
        DisconnectCommand = new RelayCommand(Disconnect);
    }

    public ConnectionId Id { get; }
    private readonly IAdapter _adapter;

    //public string Name
    //{
    //    get
    //    {
    //        return _name;
    //    }
    //    set
    //    {
    //        _name = value;
    //        _adapter.RequestChangeConnectionStatus(new ConnectionStatusDiff(Id)
    //        {
    //            Name = value,
    //        });
    //    }
    //}
    public ObservableCollection<SiteViewModel> Sites { get; }
    public ObservableCollection<BrowserViewModel> Browsers { get; }
    public ConnectionName ConnectionName { get; }

    public SiteViewModel SelectedSite
    {
        get
        {
            return _selectedSite;
        }
        set
        {
            _selectedSite = value;
            _adapter.RequestChangeConnectionStatus(new ConnectionStatusDiff(Id)
            {
                SelectedSite = value.Id,
            });
        }
    }
    public BrowserViewModel SelectedBrowser
    {
        get
        {
            return _selectedBrowser;
        }
        set
        {
            _selectedBrowser = value;
            _adapter.RequestChangeConnectionStatus(new ConnectionStatusDiff(Id)
            {
                SelectedBrowser = value.Id,
            });
        }
    }
    public bool IsConnected { get; }
    public bool IsSelected { get; set; }
    public string Input
    {
        get
        {
            return _input;
        }
        set
        {
            _input = value;
            _adapter.AfterInputChanged(Id, value);
        }
    }
    public bool CanConnect
    {
        get
        {
            return _canConnect;
        }
        set
        {
            _canConnect = value;
            _adapter.RequestChangeConnectionStatus(new ConnectionStatusDiff(Id)
            {
                CanConnect = value,
            });
        }
    }
    public bool CanDisconnect
    {
        get
        {
            return _canDisconnect;
        }
        set
        {
            _canDisconnect = value;
            _adapter.RequestChangeConnectionStatus(new ConnectionStatusDiff(Id)
            {
                CanDisconnect = value,
            });
        }
    }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public bool NeedSave { get; set; }
    public string LoggedInUsername { get; }
    public System.Windows.Media.Color BackColor
    {
        get => ConnectionName.BackColor;
        set
        {
            ConnectionName.BackColor = value;
            RaisePropertyChanged();
        }
    }
    public System.Windows.Media.Color ForeColor
    {
        get => ConnectionName.ForeColor;
        set
        {
            ConnectionName.ForeColor = value;
            RaisePropertyChanged();
        }
    }
    public void SetInput(string input)
    {
        _input = input;
        RaisePropertyChanged(nameof(Input));
    }
    public void SetSelectedSite(SiteViewModel selectedSite)
    {
        _selectedSite = selectedSite;
        RaisePropertyChanged(nameof(SelectedSite));
    }
    public void SetSelectedBrowser(BrowserViewModel selectedBrowser)
    {
        _selectedBrowser = selectedBrowser;
        RaisePropertyChanged(nameof(SelectedBrowser));
    }
    public void SetCanConnect(bool canConnect)
    {
        _canConnect = canConnect;
        RaisePropertyChanged(nameof(CanConnect));
    }
    public void SetCanDisconnect(bool canDisconnect)
    {
        _canDisconnect = canDisconnect;
        RaisePropertyChanged(nameof(CanDisconnect));
    }
    private void Connect()
    {
        _adapter.SetConnectSite(SelectedSite.Id, Id, Input, _selectedBrowser.Id);
    }
    private void Disconnect()
    {
        _adapter.SetDisconectSite(SelectedSite.Id, Id);
    }
}
