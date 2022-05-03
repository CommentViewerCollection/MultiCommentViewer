using System.Threading.Tasks;
namespace Mcv.PluginV2;

public interface IIo
{
    string? ReadFile(string path);
    Task<string?> ReadFileAsync(string path);
    void WriteFile(string path, string s);
    Task WriteFileAsync(string path, string s);
}
