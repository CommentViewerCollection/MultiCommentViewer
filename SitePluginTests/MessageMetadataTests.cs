using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineLiveSitePlugin;
using MirrativSitePlugin;
using Moq;
using NicoSitePlugin;
using NUnit.Framework;
using OpenrecSitePlugin;
using SitePlugin;
using TwicasSitePlugin;
using TwitchSitePlugin;
using WhowatchSitePlugin;
using YouTubeLiveSitePlugin;
using YouTubeLiveSitePlugin.Test2;

namespace SitePluginTests
{
    [TestFixture]
    class MessageMetadataNgUserTests
    {
        public abstract class MetadataFactory
        {
            public abstract IMessageMetadata CreateMetadata(ICommentOptions options, IUser user);
        }
        class LineLiveMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<ILineLiveComment>();
                var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
                var userMock = new Mock<IUser>();
                return new LineLiveSitePlugin.MessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class MirrativMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<IMirrativComment>();
                var siteOptionsMock = new Mock<IMirrativSiteOptions>();
                var userMock = new Mock<IUser>();
                return new MirrativSitePlugin.CommentMessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class NicoMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<INicoComment>();
                var siteOptionsMock = new Mock<INicoSiteOptions>();
                var userMock = new Mock<IUser>();
                return new NicoSitePlugin.MessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class OpenrecMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<IOpenrecComment>();
                var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
                var userMock = new Mock<IUser>();
                return new OpenrecSitePlugin.MessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class TwicasMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<ITwicasComment>();
                var siteOptionsMock = new Mock<ITwicasSiteOptions>();
                var userMock = new Mock<IUser>();
                return new TwicasSitePlugin.MessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class TwitchMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<ITwitchComment>();
                var siteOptionsMock = new Mock<ITwitchSiteOptions>();
                var userMock = new Mock<IUser>();
                return new TwitchSitePlugin.MessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class WhowatchMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<IWhowatchComment>();
                var siteOptionsMock = new Mock<IWhowatchSiteOptions>();
                var userMock = new Mock<IUser>();
                return new WhowatchSitePlugin.MessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }
        class YouTubeLiveMetadataFactory : MetadataFactory
        {
            public override IMessageMetadata CreateMetadata(ICommentOptions options, IUser user)
            {
                var messageMock = new Mock<IYouTubeLiveComment>();
                var siteOptionsMock = new Mock<IYouTubeLiveSiteOptions>();
                var userMock = new Mock<IUser>();
                return new YouTubeLiveSitePlugin.Test2.YouTubeLiveMessageMetadata(messageMock.Object, options, siteOptionsMock.Object, user, null, false);
            }
        }

        private static readonly List<MetadataFactory> _sourceList = new List<MetadataFactory>
        {
            new LineLiveMetadataFactory(),
            new MirrativMetadataFactory(),
            new NicoMetadataFactory(),
            new OpenrecMetadataFactory(),
            new TwicasMetadataFactory(),
            new TwitchMetadataFactory(),
            new WhowatchMetadataFactory(),
            new YouTubeLiveMetadataFactory(),
        };
        [Test, TestCaseSource(nameof(_sourceList))]
        public void NGユーザに指定されたら非表示になる(MetadataFactory sitePlugin)
        {
            var optionsMock = new Mock<ICommentOptions>();
            var userMock = new Mock<IUser>();

            var options = optionsMock.Object;
            var user = userMock.Object;
            var metadata = sitePlugin.CreateMetadata(options, user);
            var isVisible = true;
            metadata.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(metadata.IsVisible):
                        isVisible = metadata.IsVisible;
                        break;
                }
            };
            //NGユーザに指定されたら非表示になる
            userMock.Setup(u => u.IsNgUser).Returns(true);
            userMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(user.IsNgUser)));
            Assert.IsFalse(isVisible);
        }
    }
    [TestFixture]
    partial class MessageMetadataTests
    {
        abstract class MetadataFactory
        {
            public abstract Mock<ICommentOptions> OptionsMock { get; }
            public abstract IMessageMetadata CreateMetadata();
        }
        class OpenrecMetadataFactory : MetadataFactory
        {
            public OpenrecMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<IOpenrecComment>();
                var siteOptionsMock = new Mock<IOpenrecSiteOptions>();
                var userMock = new Mock<IUser>();
                return new OpenrecSitePlugin.MessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        class LineLiveMetadataFactory : MetadataFactory
        {
            public LineLiveMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<ILineLiveComment>();
                var siteOptionsMock = new Mock<ILineLiveSiteOptions>();
                var userMock = new Mock<IUser>();
                return new LineLiveSitePlugin.MessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        class MirrativMetadataFactory : MetadataFactory
        {
            public MirrativMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<IMirrativComment>();
                var siteOptionsMock = new Mock<IMirrativSiteOptions>();
                var userMock = new Mock<IUser>();
                return new MirrativSitePlugin.CommentMessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        class NicoMetadataFactory : MetadataFactory
        {
            public NicoMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<INicoComment>();
                var siteOptionsMock = new Mock<INicoSiteOptions>();
                var userMock = new Mock<IUser>();
                return new NicoSitePlugin.MessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        //class TwicasMetadataFactory : MetadataFactory
        //{
        //    public TwicasMetadataFactory()
        //    {
        //        OptionsMock = new Mock<ICommentOptions>();
        //        OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
        //    }

        //    public override Mock<ICommentOptions> OptionsMock { get; }
        //    public override IMessageMetadata CreateMetadata()
        //    {
        //        var messageMock = new Mock<ITwicasComment>();
        //        var siteOptionsMock = new Mock<ITwicasSiteOptions>();
        //        var userMock = new Mock<IUser>();
        //        return new TwicasSitePlugin.MessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
        //    }
        //}
        class TwitchMetadataFactory : MetadataFactory
        {
            public TwitchMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<ITwitchComment>();
                var siteOptionsMock = new Mock<ITwitchSiteOptions>();
                var userMock = new Mock<IUser>();
                return new TwitchSitePlugin.MessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        class WhowatchMetadataFactory : MetadataFactory
        {
            public WhowatchMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<IWhowatchComment>();
                var siteOptionsMock = new Mock<IWhowatchSiteOptions>();
                var userMock = new Mock<IUser>();
                return new WhowatchSitePlugin.MessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        class YouTubeLiveMetadataFactory : MetadataFactory
        {
            public YouTubeLiveMetadataFactory()
            {
                OptionsMock = new Mock<ICommentOptions>();
                OptionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);
            }

            public override Mock<ICommentOptions> OptionsMock { get; }
            public override IMessageMetadata CreateMetadata()
            {
                var messageMock = new Mock<IYouTubeLiveComment>();
                var siteOptionsMock = new Mock<IYouTubeLiveSiteOptions>();
                var userMock = new Mock<IUser>();
                return new YouTubeLiveSitePlugin.Test2.YouTubeLiveMessageMetadata(messageMock.Object, OptionsMock.Object, siteOptionsMock.Object, userMock.Object, null, false);
            }
        }
        /// <summary>
        /// OptionsのIsUserNameWrappingを変更した時にIsNameWrappingが連動して変更されるか
        /// </summary>
        [Test]
        public void IsNameWrappingTest()
        {
            var list = new List<MetadataFactory>
            {

                new LineLiveMetadataFactory(),
                new MirrativMetadataFactory(),
                new NicoMetadataFactory(),
                new OpenrecMetadataFactory(),
                //new TwicasMetadataFactory(),
                new TwitchMetadataFactory(),
                new WhowatchMetadataFactory(),
                new YouTubeLiveMetadataFactory(),
            };
            foreach (var sitePlugin in list)
            {
                Check(sitePlugin);
            }
        }
        private void Check(MetadataFactory sitePlugin)
        {
            var optionsMock = sitePlugin.OptionsMock;
            optionsMock.Setup(s => s.IsUserNameWrapping).Returns(false);

            var options = optionsMock.Object;
            var userMock = new Mock<IUser>();
            var metadata = sitePlugin.CreateMetadata();

            //変更前
            Assert.IsFalse(metadata.IsNameWrapping);

            //変更
            optionsMock.Setup(s => s.IsUserNameWrapping).Returns(true);

            //変更通知
            optionsMock.Raise(o => o.PropertyChanged += null, new PropertyChangedEventArgs(nameof(options.IsUserNameWrapping)));

            //テスト
            Assert.IsTrue(metadata.IsNameWrapping);
        }
    }
}
