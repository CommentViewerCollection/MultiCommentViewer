using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SitePlugin;

namespace Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        void OnMessageReceived(IMessage message, IMessageMetadata messageMetadata);
        void OnCommentReceived(ICommentData commentData);
        //connectionが追加されたり削除されたりしたら通知される仕組みが欲しい
        //接続、切断情報も。
        void OnLoaded();
        void OnClosing();
        void ShowSettingView();
        void OnTopmostChanged(bool isTopmost);
        IPluginHost Host { get; set; }
    }
    public interface IPluginHost
    {
        /// <summary>
        /// 設定を保存するディレクトリの絶対パス
        /// </summary>
        string SettingsDirPath { get; }
        double MainViewLeft { get; }
        double MainViewTop { get; }
        bool IsTopmost { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginName"></param>
        /// <param name="s">serialized options</param>
        void SaveOptions(string path, string s);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns>serialized options</returns>
        string LoadOptions(string path);
        /// <summary>
        /// 全ての接続中のConnectionにコメントを投稿する
        /// </summary>
        void PostCommentToAll(string comment);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="comment"></param>
        void PostComment(string guid, string comment);
        IEnumerable<IConnectionStatus> GetAllConnectionStatus();
    }
    public interface ICommentData
    {
        string ThumbnailUrl { get; }
        int ThumbnailWidth { get; }
        int ThumbnailHeight { get; }
        /// <summary>
        /// コメントID
        /// </summary>
        string Id { get; }        
        string UserId { get; }
        string Nickname { get; }
        string Comment { get; }
        bool IsNgUser { get; }
        bool IsFirstComment { get; }
        string SiteName { get; }
        bool Is184 { get; }
    }
}
