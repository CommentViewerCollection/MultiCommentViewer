using CommentViewer.Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentViewer.Plugin
{
    public static class Tools
    {
        public static string GetSiteName(ISiteMessage message)
        {
            //各サイトのサービス名
            //YouTubeLive:youtubelive
            //ニコ生:nicolive
            //Twitch:twitch
            //Twicas:twicas
            //ふわっち:whowatch
            //OPENREC:openrec
            //Mirrativ:mirrativ
            //LINELIVE:linelive
            //Periscope:periscope
            //Mixer:mixer
            //Mildom:mildom
            //NimoTV:nimotv

            string siteName;
            switch (message)
            {
                case YouTubeLiveSitePlugin.IYouTubeLiveMessage _:
                    siteName = "youtubelive";
                    break;
                case NicoSitePlugin.INicoMessage _:
                    siteName = "nicolive";
                    break;
                case TwitchSitePlugin.ITwitchMessage _:
                    siteName = "twitch";
                    break;
                case TwicasSitePlugin.ITwicasMessage _:
                    siteName = "twicas";
                    break;
                case WhowatchSitePlugin.IWhowatchMessage _:
                    siteName = "whowatch";
                    break;
                case OpenrecSitePlugin.IOpenrecMessage _:
                    siteName = "openrec";
                    break;
                case MirrativSitePlugin.IMirrativMessage _:
                    siteName = "mirrativ";
                    break;
                case LineLiveSitePlugin.ILineLiveMessage _:
                    siteName = "linelive";
                    break;
                case PeriscopeSitePlugin.IPeriscopeMessage _:
                    siteName = "periscope";
                    break;
                case MixerSitePlugin.IMixerMessage _:
                    siteName = "mixer";
                    break;
                case MildomSitePlugin.IMildomMessage _:
                    siteName = "mildom";
                    break;
                default:
                    siteName = "unknown";
                    break;
            }
            return siteName;
        }
    }
}
