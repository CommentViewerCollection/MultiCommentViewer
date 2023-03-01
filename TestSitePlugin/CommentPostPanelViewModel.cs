using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace TestSitePlugin
{
    public class CommentPostPanelViewModel : ObservableObject
    {
        private TestCommentProvider _testCommentProvider;
        private string _input;

        public ICommand PostCommand { get; }

        public string Input
        {
            get
            {
                return _input;
            }
            set
            {
                _input = value;
                OnPropertyChanged();
            }
        }

        async void Post()
        {
            var input = Input;
            Input = "";
            await _testCommentProvider.PostCommentAsync(input);
        }
        public CommentPostPanelViewModel()
        {

        }
        public CommentPostPanelViewModel(TestCommentProvider testCommentProvider)
        {
            this._testCommentProvider = testCommentProvider;
            PostCommand = new RelayCommand(Post);
        }
    }
}
