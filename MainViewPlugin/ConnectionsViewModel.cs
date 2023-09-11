using Mcv.PluginV2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mcv.MainViewPlugin;

class ConnectionsViewModel
{
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

    public void AddConnection(ConnectionViewModel connVm)
    {
        Connections.Add(connVm);
    }
    public void RemoveConnection(ConnectionViewModel connVm)
    {
        Connections.Remove(connVm);
    }
    public List<ConnectionId> GetSelectedConnections()
    {
        return Connections.Where(c => c.IsSelected).Select(c => c.Id).ToList();
    }
    private readonly IAdapter _adapter;
    #region ConnectionsView
    #region ConnectionsViewSelection
    public int ConnectionsViewSelectionDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewSelectionDisplayIndex; }
        set { _adapter.Options.ConnectionsViewSelectionDisplayIndex = value; }
    }
    public double ConnectionsViewSelectionWidth
    {
        get { return _adapter.Options.ConnectionsViewSelectionWidth; }
        set { _adapter.Options.ConnectionsViewSelectionWidth = value; }
    }
    public bool IsShowConnectionsViewSelection
    {
        get { return _adapter.Options.IsShowConnectionsViewSelection; }
        set { _adapter.Options.IsShowConnectionsViewSelection = value; }
    }
    #endregion
    #region ConnectionsViewSite
    public int ConnectionsViewSiteDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewSiteDisplayIndex; }
        set { _adapter.Options.ConnectionsViewSiteDisplayIndex = value; }
    }
    public double ConnectionsViewSiteWidth
    {
        get { return _adapter.Options.ConnectionsViewSiteWidth; }
        set { _adapter.Options.ConnectionsViewSiteWidth = value; }
    }
    public bool IsShowConnectionsViewSite
    {
        get { return _adapter.Options.IsShowConnectionsViewSite; }
        set { _adapter.Options.IsShowConnectionsViewSite = value; }
    }
    #endregion
    #region ConnectionsViewConnectionName
    public int ConnectionsViewConnectionNameDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewConnectionNameDisplayIndex; }
        set { _adapter.Options.ConnectionsViewConnectionNameDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionNameWidth
    {
        get { return _adapter.Options.ConnectionsViewConnectionNameWidth; }
        set { _adapter.Options.ConnectionsViewConnectionNameWidth = value; }
    }
    public bool IsShowConnectionsViewConnectionName
    {
        get { return _adapter.Options.IsShowConnectionsViewConnectionName; }
        set { _adapter.Options.IsShowConnectionsViewConnectionName = value; }
    }
    #endregion
    #region ConnectionsViewInput
    public int ConnectionsViewInputDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewInputDisplayIndex; }
        set { _adapter.Options.ConnectionsViewInputDisplayIndex = value; }
    }
    public double ConnectionsViewInputWidth
    {
        get { return _adapter.Options.ConnectionsViewInputWidth; }
        set { _adapter.Options.ConnectionsViewInputWidth = value; }
    }
    public bool IsShowConnectionsViewInput
    {
        get { return _adapter.Options.IsShowConnectionsViewInput; }
        set { _adapter.Options.IsShowConnectionsViewInput = value; }
    }
    #endregion
    #region ConnectionsViewBrowser
    public int ConnectionsViewBrowserDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewBrowserDisplayIndex; }
        set { _adapter.Options.ConnectionsViewBrowserDisplayIndex = value; }
    }
    public double ConnectionsViewBrowserWidth
    {
        get { return _adapter.Options.ConnectionsViewBrowserWidth; }
        set { _adapter.Options.ConnectionsViewBrowserWidth = value; }
    }
    public bool IsShowConnectionsViewBrowser
    {
        get { return _adapter.Options.IsShowConnectionsViewBrowser; }
        set { _adapter.Options.IsShowConnectionsViewBrowser = value; }
    }
    #endregion
    #region ConnectionsViewConnection
    public int ConnectionsViewConnectionDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewConnectionDisplayIndex; }
        set { _adapter.Options.ConnectionsViewConnectionDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionWidth
    {
        get { return _adapter.Options.ConnectionsViewConnectionWidth; }
        set { _adapter.Options.ConnectionsViewConnectionWidth = value; }
    }
    public bool IsShowConnectionsViewConnection
    {
        get { return _adapter.Options.IsShowConnectionsViewConnection; }
        set { _adapter.Options.IsShowConnectionsViewConnection = value; }
    }
    #endregion
    #region ConnectionsViewDisconnection
    public int ConnectionsViewDisconnectionDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewDisconnectionDisplayIndex; }
        set { _adapter.Options.ConnectionsViewDisconnectionDisplayIndex = value; }
    }
    public double ConnectionsViewDisconnectionWidth
    {
        get { return _adapter.Options.ConnectionsViewDisconnectionWidth; }
        set { _adapter.Options.ConnectionsViewDisconnectionWidth = value; }
    }
    public bool IsShowConnectionsViewDisconnection
    {
        get { return _adapter.Options.IsShowConnectionsViewDisconnection; }
        set { _adapter.Options.IsShowConnectionsViewDisconnection = value; }
    }
    #endregion
    #region ConnectionsViewSave
    public int ConnectionsViewSaveDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewSaveDisplayIndex; }
        set { _adapter.Options.ConnectionsViewSaveDisplayIndex = value; }
    }
    public double ConnectionsViewSaveWidth
    {
        get { return _adapter.Options.ConnectionsViewSaveWidth; }
        set { _adapter.Options.ConnectionsViewSaveWidth = value; }
    }
    public bool IsShowConnectionsViewSave
    {
        get { return _adapter.Options.IsShowConnectionsViewSave; }
        set { _adapter.Options.IsShowConnectionsViewSave = value; }
    }
    #endregion
    #region ConnectionsViewLoggedinUsername
    public int ConnectionsViewLoggedinUsernameDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewLoggedinUsernameDisplayIndex; }
        set { _adapter.Options.ConnectionsViewLoggedinUsernameDisplayIndex = value; }
    }
    public double ConnectionsViewLoggedinUsernameWidth
    {
        get { return _adapter.Options.ConnectionsViewLoggedinUsernameWidth; }
        set { _adapter.Options.ConnectionsViewLoggedinUsernameWidth = value; }
    }
    public bool IsShowConnectionsViewLoggedinUsername
    {
        get { return _adapter.Options.IsShowConnectionsViewLoggedinUsername; }
        set { _adapter.Options.IsShowConnectionsViewLoggedinUsername = value; }
    }
    #endregion
    #region ConnectionsViewConnectionBackground
    public int ConnectionsViewConnectionBackgroundDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewConnectionBackgroundDisplayIndex; }
        set { _adapter.Options.ConnectionsViewConnectionBackgroundDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionBackgroundWidth
    {
        get { return _adapter.Options.ConnectionsViewConnectionBackgroundWidth; }
        set { _adapter.Options.ConnectionsViewConnectionBackgroundWidth = value; }
    }
    public bool IsShowConnectionsViewConnectionBackground
    {
        get { return _adapter.Options.IsEnabledSiteConnectionColor && _adapter.Options.SiteConnectionColorType == SiteConnectionColorType.Connection; }
    }
    #endregion
    #region ConnectionsViewConnectionForeground
    public int ConnectionsViewConnectionForegroundDisplayIndex
    {
        get { return _adapter.Options.ConnectionsViewConnectionForegroundDisplayIndex; }
        set { _adapter.Options.ConnectionsViewConnectionForegroundDisplayIndex = value; }
    }
    public double ConnectionsViewConnectionForegroundWidth
    {
        get { return _adapter.Options.ConnectionsViewConnectionForegroundWidth; }
        set { _adapter.Options.ConnectionsViewConnectionForegroundWidth = value; }
    }
    public bool IsShowConnectionsViewConnectionForeground
    {
        get { return _adapter.Options.IsEnabledSiteConnectionColor && _adapter.Options.SiteConnectionColorType == SiteConnectionColorType.Connection; }
    }
    #endregion
    #endregion

    public double ConnectionColorColumnWidth
    {
        get
        {
            if (_adapter.Options.IsEnabledSiteConnectionColor && _adapter.Options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return 100;
            }
            else
            {
                return 0;
            }
        }
    }
    public System.Windows.Controls.DataGridGridLinesVisibility GridLinesVisibility
    {
        get
        {
            if (_adapter.Options.IsShowHorizontalGridLine && _adapter.Options.IsShowVerticalGridLine)
                return System.Windows.Controls.DataGridGridLinesVisibility.All;
            else if (_adapter.Options.IsShowHorizontalGridLine)
                return System.Windows.Controls.DataGridGridLinesVisibility.Horizontal;
            else if (_adapter.Options.IsShowVerticalGridLine)
                return System.Windows.Controls.DataGridGridLinesVisibility.Vertical;
            else
                return System.Windows.Controls.DataGridGridLinesVisibility.None;
        }
    }
    public ConnectionsViewModel(IAdapter adapter)
    {
        _adapter = adapter;
    }
}
