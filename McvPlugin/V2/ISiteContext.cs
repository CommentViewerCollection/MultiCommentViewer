using System.Windows.Controls;

namespace Mcv.PluginV2;

public interface ISiteContext
{
    string DisplayName { get; }
    IOptionsTabPage TabPanel { get; }
    void SaveOptions(string path, IIo io);
    void LoadOptions(string path, IIo io);
    void LoadOptions(string rawOptions);
    string GetSiteOptions();
    ICommentProvider CreateCommentProvider();
    /// <summary>
    /// inputがこのサイトの入力値に適合しているか
    /// 形式が正しいかを判定するだけで、実際に存在するかは関知しない
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    bool IsValidInput(string input);
    UserControl GetCommentPostPanel(ICommentProvider commentProvider);
}

