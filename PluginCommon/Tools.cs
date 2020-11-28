﻿using SitePlugin;
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
                comment = superchat.CommentItems.ToText();
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
            else if (message is NicoSitePlugin.INicoItem nicoItem)
            {
                comment = nicoItem.Text;
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
            //else if (message is MixerSitePlugin.IMixerMessage mixerMessage)
            //{
            else if (message is MixerSitePlugin.IMixerComment mixerComment)
            {
                comment = mixerComment.CommentItems.ToText();
                name = mixerComment.UserName;
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
            return (name, comment);
        }
    }
}
