namespace NicoSitePlugin.Next
{
    public enum ErrorCode
    {
        /// <summary>
        /// 未確認のエラーコード
        /// </summary>
        unknown,
        /// <summary>
        /// 
        /// </summary>
        closed,
        notfound,
        notlogin,
        /// <summary>
        /// コミュ限
        /// </summary>
        require_community_member,
        /// <summary>
        /// 予約
        /// </summary>
        comingsoon,
        /// <summary>
        /// 満員
        /// </summary>
        full,
        /// <summary>
        /// なにこれ？有料放送かな？
        /// </summary>
        noauth,
        /// <summary>
        /// 配信者が削除
        /// </summary>
        deletedbyuser,
        /// <summary>
        /// 運営削除
        /// </summary>
        deletedbyvisor,
        /// <summary>
        /// 高負荷
        /// </summary>
        block_now_count_overflow,
        unknown_error,
        tsarchive,
        usertimeshift,
        violated,
        timeshiftfull,
        userliveslotfull,
        maintenance,
    }
}
