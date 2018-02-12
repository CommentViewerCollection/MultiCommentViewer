using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        void OnCommentReceived(ICommentData commentData);
        //connectionが追加されたり削除されたりしたら通知される仕組みが欲しい
        //接続、切断情報も。
        void OnLoaded();
        void OnClosing();
        void ShowSettingView();
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
    }
    public interface ICommentData
    {
        /// <summary>
        /// コメントID
        /// </summary>
        string Id { get; }        
        string UserId { get; }
        string Nickname { get; }
        string Comment { get; }
        bool IsNgUser { get; }
    }
}
