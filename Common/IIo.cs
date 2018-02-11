using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IIo
    {
        string ReadFile(string path);
        Task<string> ReadFileAsync(string path);
        void WriteFile(string path, string s);
        Task WriteFileAsync(string path, string s);
    }
}
