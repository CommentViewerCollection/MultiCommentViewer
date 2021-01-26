using System.ComponentModel;
using System.Windows.Media;

namespace NicoSitePlugin
{
    public interface INicoSiteOptions : INotifyPropertyChanged
    {
        //公式放送では自分の部屋を入れて何部屋取得するか
        //"アリーナ:123456"等のコメントIDの表示形式。カスタマイズできるようにしたい
        //放送者コメント等の文字色、背景色
        int OfficialRoomsRetrieveCount { get; set; }
        /// <summary>
        /// 接続時に取得する直近の過去コメントの数
        /// </summary>
        int ResNum { get; set; }
        Color OperatorBackColor { get; set; }
        Color OperatorForeColor { get; set; }
        Color AdBackColor { get; set; }
        Color AdForeColor { get; set; }
        Color ItemBackColor { get; set; }
        Color ItemForeColor { get; set; }
        Color SpiBackColor { get; set; }
        Color SpiForeColor { get; set; }
        Color EmotionBackColor { get; set; }
        Color EmotionForeColor { get; set; }

        /// <summary>
        /// 184コメントを表示するか
        /// </summary>
        bool IsShow184 { get; set; }

        bool IsAutoSetNickname { get; set; }

        bool IsShow184Id { get; set; }
        bool IsAutoGetUsername { get; set; }
    }
}
