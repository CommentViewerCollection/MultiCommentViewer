namespace Plugin
{
    public interface IConnectionStatus
    {
        string Name { get; }
        string Guid { get; }
        //bool CanPostComment{get;}//接続中で且つ大抵のサイトではログイン済みである必要がある

    }
}
