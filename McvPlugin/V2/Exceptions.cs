using System.Runtime.Serialization;
namespace Mcv.PluginV2;

[Serializable]
public class ChromeCookiesFileNotFoundException : Exception
{
    public ChromeCookiesFileNotFoundException(string message)
        : base(message)
    { }

    public ChromeCookiesFileNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    { }

    protected ChromeCookiesFileNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}
[Serializable]
public class FirefoxProfileIniNotFoundException : Exception
{
    public FirefoxProfileIniNotFoundException(string message)
        : base(message)
    { }

    public FirefoxProfileIniNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    { }

    protected FirefoxProfileIniNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}
