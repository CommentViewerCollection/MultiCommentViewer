using System;

namespace Mcv.PluginV2;

public interface ILogger
{
    string GetExceptions();
    void LogException(Exception ex, string message = "", string detail = "");
}
