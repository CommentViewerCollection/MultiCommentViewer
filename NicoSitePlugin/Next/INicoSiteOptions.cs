namespace NicoSitePlugin.Next
{
    public interface INicoSiteOptions
    {
        //公式放送では自分の部屋を入れて何部屋取得するか
        //"アリーナ:123456"等のコメントIDの表示形式。カスタマイズできるようにしたい
        //放送者コメント等の文字色、背景色
        int OfficialRoomsRetrieveCount { get; set; }
        /// <summary>
        /// 接続時に取得する直近の過去コメントの数
        /// </summary>
        int ResNum { get; set; }
    }
}
