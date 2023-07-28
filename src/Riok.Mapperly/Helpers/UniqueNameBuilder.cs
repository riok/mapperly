namespace Riok.Mapperly.Helpers;

public class UniqueNameBuilder
{
    private readonly HashSet<string> _usedNames;
    private readonly UniqueNameBuilder? _parentScope;

    public UniqueNameBuilder()
    {
        _usedNames = new HashSet<string>(StringComparer.Ordinal);
    }

    private UniqueNameBuilder(UniqueNameBuilder parentScope)
    {
        _usedNames = new HashSet<string>(StringComparer.Ordinal);
        _parentScope = parentScope;
    }

    public void Reserve(string name) => _usedNames.Add(name);

    public UniqueNameBuilder NewScope() => new(this);

    public string New(string name)
    {
        var i = 0;
        var uniqueName = name;
        while (Contains(uniqueName))
        {
            i++;
            uniqueName = name + i;
        }

        _usedNames.Add(uniqueName);

        return uniqueName;
    }

    private bool Contains(string name)
    {
        if (_usedNames.Contains(name))
            return true;

        if (_parentScope != null)
            return _parentScope.Contains(name);

        return false;
    }
}
