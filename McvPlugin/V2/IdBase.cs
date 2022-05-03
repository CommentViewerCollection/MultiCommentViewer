namespace Mcv.PluginV2;

public abstract class IdBase
{
    private readonly string _guid;

    public IdBase(Guid guid)
    {
        _guid = guid.ToString();
    }
    public IdBase(string s)
    {
        _guid = s;
    }
    public override bool Equals(object? obj)
    {
        if (obj is not IdBase b)
        {
            return false;
        }
        return _guid.Equals(b._guid);
    }
    public override int GetHashCode()
    {
        return _guid.GetHashCode();
    }
    public override string ToString()
    {
        return _guid.ToString();
    }
}
