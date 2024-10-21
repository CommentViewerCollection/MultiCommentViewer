using SitePlugin;
using System;
using System.Collections.Generic;

namespace PluginCommon
{
    public interface MessageStatus
    {

    }
    public static class Tools
    {
        public static string ToText(this IEnumerable<IMessagePart> parts)
        {
            string s = "";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part is IMessageText text)
                    {
                        s += text;
                    }
                }
            }
            return s;
        }
        public static (string name, string comment) GetData(ISiteMessage message)
        {
            string name = null;
            string comment = null;

            //if (message is YouTubeLiveSitePlugin.IYouTubeLiveMessage ytMessage)
            //{
            if (message is YouTubeLiveSitePlugin.IYouTubeLiveComment ytComment)
            {
                comment = ytComment.CommentItems.ToText();
                name = ytComment.NameItems.ToText();
            }
            else if (message is YouTubeLiveSitePlugin.IYouTubeLiveSuperchat superchat)
            {
                var s = superchat.PurchaseAmount;
                var text = superchat.CommentItems.ToText();
                if (!string.IsNullOrEmpty(text))
                {
                    s += Environment.NewLine + text;
                }
                comment = s;
                name = superchat.NameItems.ToText();
            }
            //}
            //else if (message is NicoSitePlugin.INicoMessage nicoMessage)
            //{
            else if (message is NicoSitePlugin.INicoComment nicoComment)
            {
                comment = nicoComment.Text;
                name = nicoComment.UserName;
            }
            else if (message is NicoSitePlugin.INicoGift nicoItem)
            {
                comment = nicoItem.Text;
                //name = nicoItem.;
            }
            else if (message is NicoSitePlugin.INicoAd nicoAd)
            {
                comment = nicoAd.Text;
                //name = nicoItem.;
            }
            else if (message is NicoSitePlugin.INicoSpi nicoSpi)
            {
                comment = nicoSpi.Text;
                //name = nicoItem.;
            }
            else if (message is NicoSitePlugin.INicoEmotion nicoEmotion)
            {
                comment = nicoEmotion.Content;
                //name = nicoItem.;
            }
            //}
            //else if (message is TwitchSitePlugin.ITwitchMessage twMessage)
            //{
            else if (message is TwitchSitePlugin.ITwitchComment twComment)
            {
                comment = twComment.CommentItems.ToText();
                name = twComment.DisplayName;
            }
            //}
            //else if (message is TwicasSitePlugin.ITwicasMessage casMessage)
            //{
            else if (message is TwicasSitePlugin.ITwicasComment casComment)
            {
                comment = casComment.CommentItems.ToText();
                name = casComment.UserName;
            }
            else if (message is TwicasSitePlugin.ITwicasKiitos casKiitos)
            {
                comment = casKiitos.CommentItems.ToText();
                name = casKiitos.UserName;
            }
            else if (message is TwicasSitePlugin.ITwicasItem casItem)
            {
                comment = casItem.CommentItems.ToText();
                name = casItem.UserName;
            }
            //}
            //else if (message is WhowatchSitePlugin.IWhowatchMessage wwMessage)
            //{
            else if (message is WhowatchSitePlugin.IWhowatchComment wwComment)
            {
                comment = wwComment.Comment;
                name = wwComment.UserName;
            }
            else if (message is WhowatchSitePlugin.IWhowatchItem wwItem)
            {
                comment = wwItem.Comment;
                name = wwItem.UserName;
            }
            //}
            //else if (message is OpenrecSitePlugin.IOpenrecMessage opMessage)
            //{
            else if (message is OpenrecSitePlugin.IOpenrecComment opComment)
            {
                comment = opComment.MessageItems.ToText();
                name = opComment.NameItems.ToText();
            }
            //}
            //else if (message is BLiveSitePlugin.IBLiveMessage opMessage)
            //{
            else if (message is BLiveSitePlugin.IBLiveComment blComment)
            {
                comment = blComment.MessageItems.ToText();
                name = blComment.NameItems.ToText();
            }
            //}
            //else if (message is MixchSitePlugin.IMixchMessage opMessage)
            //{
            else if (message is MixchSitePlugin.IMixchMessage mxMessage)
            {
                comment = mxMessage.MessageItems.ToText();
                name = mxMessage.NameItems.ToText();
            }
            //}
            //else if (message is MirrativSitePlugin.IMirrativMessage mrMessage)
            //{
            else if (message is MirrativSitePlugin.IMirrativComment mrComment)
            {
                comment = mrComment.Text;
                name = mrComment.UserName;
            }
            else if (message is MirrativSitePlugin.IMirrativJoinRoom mrJoin)
            {
                comment = mrJoin.Text;
                name = mrJoin.UserName;
            }
            //}
            //else if (message is LineLiveSitePlugin.ILineLiveMessage lineLiveMessage)
            //{
            else if (message is LineLiveSitePlugin.ILineLiveComment llComment)
            {
                comment = llComment.Text;
                name = llComment.DisplayName;
            }
            //}
            //else if (message is PeriscopeSitePlugin.IPeriscopeMessage psMessage)
            //{
            else if (message is PeriscopeSitePlugin.IPeriscopeComment psComment)
            {
                comment = psComment.Text;
                name = psComment.DisplayName;
            }
            else if (message is ShowRoomSitePlugin.IShowRoomComment srComment)
            {
                comment = srComment.Text;
                name = srComment.UserName;
            }
            //}
            //else if (message is MildomSitePlugin.IMildomMessage mildomMessage)
            //{
            else if (message is MildomSitePlugin.IMildomComment mildomComment)
            {
                comment = mildomComment.CommentItems.ToText();
                name = mildomComment.UserName;
            }
            //}
            else if (message is BigoSitePlugin.IBigoComment bgComment)
            {
                comment = bgComment.Message;
                name = bgComment.Name;
            }
            return (name, comment);
        }
    }
}
