using GalaSoft.MvvmLight;
using SitePlugin;
//TODO:過去コメントの取得


namespace MultiCommentViewer
{
    public class MetadataViewModel : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }
        private string _elapsed;
        public string Elapsed
        {
            get { return _elapsed; }
            set
            {
                _elapsed = value;
                RaisePropertyChanged();
            }
        }
        private string _currentViewers;
        public string CurrentViewers
        {
            get { return _currentViewers; }
            set
            {
                _currentViewers = value;
                RaisePropertyChanged();
            }
        }
        private string _totalViewers;
        public string TotalViewers
        {
            get { return _totalViewers; }
            set
            {
                _totalViewers = value;
                RaisePropertyChanged();
            }
        }
        private string _active;
        public string Active
        {
            get { return _active; }
            set
            {
                _active = value;
                RaisePropertyChanged();
            }
        }
        private string _others;
        public string Others
        {
            get { return _others; }
            set
            {
                _others = value;
                RaisePropertyChanged();
            }
        }

        public string ConnectionName => _connectionName.Name;
        private readonly ConnectionName _connectionName;
        public MetadataViewModel(ConnectionName connectionName)
        {
            _connectionName = connectionName;
            Title = "-";
            Elapsed = "-";
            CurrentViewers = "-";
            TotalViewers = "-";
            Active = "-";
            Others = "-";
            _connectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_connectionName.Name):
                        base.RaisePropertyChanged(nameof(ConnectionName));
                        break;
                }
            };
        }
    }
}
