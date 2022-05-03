namespace Mcv.PluginV2;

/// <summary>
/// 情報の種類。
/// デバッグ情報や軽微なエラー情報が必要無い場合があるため分類する。
/// </summary>
/// <remarks>大小比較ができるように</remarks>
public enum InfoType
{
    /// <summary>
    /// 無し
    /// </summary>
    None,
    /// <summary>
    /// 致命的なエラーがあった場合だけ。必要最小限の情報
    /// </summary>
    Error,
    /// <summary>
    /// 
    /// </summary>
    Notice,
    /// <summary>
    /// 例外全て
    /// </summary>
    Debug,
}
public static class InfoTypeRelatedOperations
{
    /// <summary>
    /// 文字列をInfoTypeに変換する。
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    /// <remarks>InfoTypeをEnumではなくclassにしてこのメソッドもそこに含めたほうが良いかも</remarks>
    public static InfoType ToInfoType(string s)
    {
        if (!Enum.TryParse(s, out InfoType type))
        {
            type = InfoType.Notice;
        }
        return type;
    }
}
