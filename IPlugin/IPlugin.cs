using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        string Description { get; }
        void ReceiveComment(ICommentData commentData);
        //connectionが追加されたり削除されたりしたら通知される仕組みが欲しい
        //接続、切断情報も。

    }

    public interface IHost
    {
        /// <summary>
        /// 設定を保存するディレクトリの絶対パス
        /// </summary>
        string SettingsDirPath { get; }

    }
    public interface ICommentData
    {
        /// <summary>
        /// コメントID
        /// </summary>
        string Id { get; }        
        string UserId { get; }
        string Name { get; }
        string Message { get; }

    }
}
