using SitePlugin;

namespace ShowRoomSitePlugin
{
    public class ShowRoomOptionsTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public System.Windows.Controls.UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly ShowRoomOptionsPanel _panel;
        public ShowRoomOptionsTabPage(string displayName, ShowRoomOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
}
