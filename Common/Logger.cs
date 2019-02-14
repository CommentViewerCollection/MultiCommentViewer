using System;

namespace Common
{
    public interface ILogger
    {
        string GetExceptions();
        void LogException(Exception ex, string message = "", string detail = "");
    }
}
