using System.Windows.Controls;

namespace Mcv.PluginV2;

public interface IOptionsTabPage
{
    string HeaderText { get; }
    void Apply();
    void Cancel();
    UserControl TabPagePanel { get; }
}
