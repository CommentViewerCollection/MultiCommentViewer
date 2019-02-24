using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NicoSitePlugin.Low.GetPlayerStatus
{
    [XmlRoot(ElementName = "press")]
    public class Press
    {
        [XmlElement(ElementName = "display_lines")]
        public string Display_lines { get; set; }
        [XmlElement(ElementName = "display_time")]
        public string Display_time { get; set; }
        [XmlElement(ElementName = "style_conf")]
        public string Style_conf { get; set; }
    }

    [XmlRoot(ElementName = "que")]
    public class Que
    {
        [XmlAttribute(AttributeName = "vpos")]
        public string Vpos { get; set; }
        [XmlAttribute(AttributeName = "mail")]
        public string Mail { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "quesheet")]
    public class Quesheet
    {
        [XmlElement(ElementName = "que")]
        public List<Que> Que { get; set; }
    }

    [XmlRoot(ElementName = "stream")]
    public class Stream
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "provider_type")]
        public string Provider_type { get; set; }
        [XmlElement(ElementName = "default_community")]
        public string Default_community { get; set; }
        [XmlElement(ElementName = "international")]
        public string International { get; set; }
        [XmlElement(ElementName = "is_owner")]
        public string Is_owner { get; set; }
        [XmlElement(ElementName = "owner_id")]
        public string Owner_id { get; set; }
        [XmlElement(ElementName = "owner_name")]
        public string Owner_name { get; set; }
        [XmlElement(ElementName = "is_reserved")]
        public string Is_reserved { get; set; }
        [XmlElement(ElementName = "is_niconico_enquete_enabled")]
        public string Is_niconico_enquete_enabled { get; set; }
        [XmlElement(ElementName = "watch_count")]
        public string Watch_count { get; set; }
        [XmlElement(ElementName = "comment_count")]
        public string Comment_count { get; set; }
        [XmlElement(ElementName = "base_time")]
        public string Base_time { get; set; }
        [XmlElement(ElementName = "open_time")]
        public string Open_time { get; set; }
        [XmlElement(ElementName = "start_time")]
        public string Start_time { get; set; }
        [XmlElement(ElementName = "end_time")]
        public string End_time { get; set; }
        [XmlElement(ElementName = "is_rerun_stream")]
        public string Is_rerun_stream { get; set; }
        [XmlElement(ElementName = "bourbon_url")]
        public string Bourbon_url { get; set; }
        [XmlElement(ElementName = "full_video")]
        public string Full_video { get; set; }
        [XmlElement(ElementName = "after_video")]
        public string After_video { get; set; }
        [XmlElement(ElementName = "before_video")]
        public string Before_video { get; set; }
        [XmlElement(ElementName = "kickout_video")]
        public string Kickout_video { get; set; }
        [XmlElement(ElementName = "twitter_tag")]
        public string Twitter_tag { get; set; }
        [XmlElement(ElementName = "danjo_comment_mode")]
        public string Danjo_comment_mode { get; set; }
        [XmlElement(ElementName = "infinity_mode")]
        public string Infinity_mode { get; set; }
        [XmlElement(ElementName = "archive")]
        public string Archive { get; set; }
        [XmlElement(ElementName = "press")]
        public Press Press { get; set; }
        [XmlElement(ElementName = "plugin_delay")]
        public string Plugin_delay { get; set; }
        [XmlElement(ElementName = "plugin_url")]
        public string Plugin_url { get; set; }
        [XmlElement(ElementName = "plugin_urls")]
        public string Plugin_urls { get; set; }
        [XmlElement(ElementName = "allow_netduetto")]
        public string Allow_netduetto { get; set; }
        [XmlElement(ElementName = "ng_scoring")]
        public string Ng_scoring { get; set; }
        [XmlElement(ElementName = "is_nonarchive_timeshift_enabled")]
        public string Is_nonarchive_timeshift_enabled { get; set; }
        [XmlElement(ElementName = "is_timeshift_reserved")]
        public string Is_timeshift_reserved { get; set; }
        [XmlElement(ElementName = "timeshift_time")]
        public string Timeshift_time { get; set; }
        [XmlElement(ElementName = "quesheet")]
        public Quesheet Quesheet { get; set; }
        [XmlElement(ElementName = "picture_url")]
        public string Picture_url { get; set; }
        [XmlElement(ElementName = "thumb_url")]
        public string Thumb_url { get; set; }
        [XmlElement(ElementName = "is_priority_prefecture")]
        public string Is_priority_prefecture { get; set; }
    }

    [XmlRoot(ElementName = "twitter_info")]
    public class Twitter_info
    {
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }
        [XmlElement(ElementName = "screen_name")]
        public string Screen_name { get; set; }
        [XmlElement(ElementName = "followers_count")]
        public string Followers_count { get; set; }
        [XmlElement(ElementName = "is_vip")]
        public string Is_vip { get; set; }
        [XmlElement(ElementName = "profile_image_url")]
        public string Profile_image_url { get; set; }
        [XmlElement(ElementName = "after_auth")]
        public string After_auth { get; set; }
        [XmlElement(ElementName = "tweet_token")]
        public string Tweet_token { get; set; }
    }

    [XmlRoot(ElementName = "user")]
    public class User
    {
        [XmlElement(ElementName = "user_id")]
        public string User_id { get; set; }
        [XmlElement(ElementName = "nickname")]
        public string Nickname { get; set; }
        [XmlElement(ElementName = "is_premium")]
        public string Is_premium { get; set; }
        [XmlElement(ElementName = "userAge")]
        public string UserAge { get; set; }
        [XmlElement(ElementName = "userSex")]
        public string UserSex { get; set; }
        [XmlElement(ElementName = "userDomain")]
        public string UserDomain { get; set; }
        [XmlElement(ElementName = "userPrefecture")]
        public string UserPrefecture { get; set; }
        [XmlElement(ElementName = "userLanguage")]
        public string UserLanguage { get; set; }
        [XmlElement(ElementName = "room_label")]
        public string Room_label { get; set; }
        [XmlElement(ElementName = "room_seetno")]
        public string Room_seetno { get; set; }
        [XmlElement(ElementName = "is_join")]
        public string Is_join { get; set; }
        [XmlElement(ElementName = "twitter_info")]
        public Twitter_info Twitter_info { get; set; }
    }

    [XmlRoot(ElementName = "rtmp")]
    public class Rtmp
    {
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
        [XmlElement(ElementName = "ticket")]
        public string Ticket { get; set; }
        [XmlAttribute(AttributeName = "is_fms")]
        public string Is_fms { get; set; }
    }

    [XmlRoot(ElementName = "ms")]
    public class Ms
    {
        [XmlElement(ElementName = "addr")]
        public string Addr { get; set; }
        [XmlElement(ElementName = "port")]
        public string Port { get; set; }
        [XmlElement(ElementName = "thread")]
        public string Thread { get; set; }
    }
    [XmlRoot(ElementName = "tid_list")]
    public class Tid_list
    {
        [XmlElement(ElementName = "tid")]
        public List<string> Tid { get; set; }
    }
    [XmlRoot(ElementName = "ms_list")]
    public class Ms_list
    {
        [XmlElement(ElementName = "ms")]
        public List<Ms> Ms { get; set; }
    }

    [XmlRoot(ElementName = "twitter")]
    public class Twitter
    {
        [XmlElement(ElementName = "live_enabled")]
        public string Live_enabled { get; set; }
        [XmlElement(ElementName = "vip_mode_count")]
        public string Vip_mode_count { get; set; }
        [XmlElement(ElementName = "live_api_url")]
        public string Live_api_url { get; set; }
    }

    [XmlRoot(ElementName = "dialog_image")]
    public class Dialog_image
    {
        [XmlElement(ElementName = "oidashi")]
        public string Oidashi { get; set; }
    }

    [XmlRoot(ElementName = "player")]
    public class Player
    {
        [XmlElement(ElementName = "qos_analytics")]
        public string Qos_analytics { get; set; }
        [XmlElement(ElementName = "dialog_image")]
        public Dialog_image Dialog_image { get; set; }
        [XmlElement(ElementName = "is_notice_viewer_balloon_enabled")]
        public string Is_notice_viewer_balloon_enabled { get; set; }
        [XmlElement(ElementName = "error_report")]
        public string Error_report { get; set; }
    }

    [XmlRoot(ElementName = "marquee")]
    public class Marquee
    {
        [XmlElement(ElementName = "category")]
        public string Category { get; set; }
        [XmlElement(ElementName = "game_key")]
        public string Game_key { get; set; }
        [XmlElement(ElementName = "game_time")]
        public string Game_time { get; set; }
        [XmlElement(ElementName = "force_nicowari_off")]
        public string Force_nicowari_off { get; set; }
    }

    [XmlRoot(ElementName = "getplayerstatus")]
    public class RootObject
    {
        [XmlElement(ElementName = "stream")]
        public Stream Stream { get; set; }
        [XmlElement(ElementName = "user")]
        public User User { get; set; }
        [XmlElement(ElementName = "rtmp")]
        public Rtmp Rtmp { get; set; }
        [XmlElement(ElementName = "ms")]
        public Ms Ms { get; set; }
        [XmlElement(ElementName = "tid_list")]
        public Tid_list Tid_list { get; set; }
        [XmlElement(ElementName = "ms_list")]
        public Ms_list Ms_list { get; set; }
        [XmlElement(ElementName = "twitter")]
        public Twitter Twitter { get; set; }
        [XmlElement(ElementName = "player")]
        public Player Player { get; set; }
        [XmlElement(ElementName = "marquee")]
        public Marquee Marquee { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

}