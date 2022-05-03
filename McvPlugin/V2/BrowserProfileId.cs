namespace Mcv.PluginV2;

public class BrowserProfileId
{
    private readonly Guid _guid;

    public BrowserProfileId(Guid guid)
    {
        _guid = guid;
    }
    public override string ToString()
    {
        return _guid.ToString();
    }
    public override bool Equals(object? obj)
    {
        if (obj is not BrowserProfileId b)
        {
            return false;
        }
        return _guid.Equals(b._guid);
    }
    public override int GetHashCode()
    {
        return _guid.GetHashCode();
    }
}
