using SitePlugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TestSitePlugin
{
    public class TestSiteContext : ISiteContext
    {
        private readonly ICommentOptions _options;

        public Guid Guid => new Guid("609B4057-A5CE-49BA-A30F-211C4DFE838E");
        public string DisplayName => "Test";
        public IOptionsTabPage TabPanel { get; }

        public ICommentProvider CreateCommentProvider()
        {
            return new TestCommentProvider(_options, new TestUserStore());
        }

        public System.Windows.Controls.UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var testCommentProvider = commentProvider as TestCommentProvider;
            Debug.Assert(testCommentProvider != null);
            if (testCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(testCommentProvider);
            var panel = new CommentPostPanel
            {
                DataContext = vm
            };
            return panel;
        }

        public void Init()
        {
        }

        public bool IsValidInput(string input)
        {
            return true;
        }

        public void LoadOptions(string path, IIo io)
        {
        }

        public void Save()
        {
        }

        public void SaveOptions(string path, IIo io)
        {
        }
        public TestSiteContext(ICommentOptions options)
        {
            _options = options;
        }
    }
}
