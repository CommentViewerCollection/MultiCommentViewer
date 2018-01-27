using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using NicoSitePlugin;
using System.Xml.Serialization;
using SitePlugin;

namespace MultiCommentViewer.Test.NicoLive
{
    class NicoSiteContext : INicoSiteContext
    {
        public Guid Guid { get { return new Guid("581C2292-02CE-48DB-88BD-006EBA4D531E"); } }

        public string DisplayName { get { return "ニコ生"; } }

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new NicoOptionsPanel();
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            throw new NotImplementedException();
        }

        public INicoCommentProvider CreateNicoCommentProvider()
        {
            throw new NotImplementedException();
        }

        public INicoSiteOptions GetNicoSiteOptions()
        {
            throw new NotImplementedException();
        }

        public void LoadOptions(string path)
        {
            throw new NotImplementedException();
        }

        public void SaveOptions(string path)
        {
            throw new NotImplementedException();
        }
        private readonly IOptions _options;
        public NicoSiteContext(IOptions options)
        {
            _options = options;
        }
    }
    public class PlayerStatusTest : IPlayerStatus
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public ProviderType ProviderType { get; set; }

        public long BaseTime { get; set; }

        public long OpenTime { get; set; }

        public long StartTime { get; set; }

        public long? EndTime { get; set; }

        public string UserId { get; set; }

        public string Nickname { get; set; }

        public string RoomLabel { get; set; }

        public int RoomSeetNo { get; set; }

        public bool IsJoin { get; set; }

        public IMs Ms { get; set; }

        public IMs[] MsList { get; set; }
    }
    public static class Tools
    {
        public static IPlayerStatus Parse(string playerStatus_xml)
        {
            var serializer = new XmlSerializer(typeof(Xml2CSharp.Getplayerstatus));
            var bytes = System.Text.Encoding.UTF8.GetBytes(playerStatus_xml);
            Xml2CSharp.Getplayerstatus ps;
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                ps = (Xml2CSharp.Getplayerstatus)serializer.Deserialize(ms);
            }

            var msList = ps.Ms_list?.Ms.Select(ms => new MsTest(ms.Addr, ms.Thread, int.Parse(ms.Port))).Cast<IMs>().ToArray();
            return new PlayerStatusTest
            {
                Title = ps.Stream.Title,
                BaseTime = long.Parse(ps.Stream.Base_time),
                Description = ps.Stream.Description,
                EndTime = long.Parse(ps.Stream.End_time),
                IsJoin = ps.User.Is_join == "1",
                Ms = new MsTest(ps.Ms.Addr, ps.Ms.Thread, int.Parse(ps.Ms.Port)),
                Nickname = ps.User.Nickname,
                OpenTime = long.Parse(ps.Stream.Open_time),
                StartTime = long.Parse(ps.Stream.Start_time),
                ProviderType = Convert(ps.Stream.Provider_type),
                UserId = ps.User.User_id,
                RoomSeetNo = int.Parse(ps.User.Room_seetno),
                RoomLabel = ps.User.Room_label,
                MsList = msList,
            };
        }
        public static ProviderType Convert(string providerType)
        {
            ProviderType type;
            switch (providerType)
            {
                case "channel":
                    type = ProviderType.Channel;
                    break;
                case "community":
                    type = ProviderType.Community;
                    break;
                case "official":
                    type = ProviderType.Official;
                    break;
                default:
                    type = ProviderType.Unknown;
                    break;
            }
            return type;
        }
    }
}
