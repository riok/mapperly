namespace Riok.Mapperly.Helpers;

public class UniqueNameBuilder()
{
    private readonly HashSet<string> _usedNames = new(StringComparer.Ordinal);
    private readonly UniqueNameBuilder? _parentScope;

    private UniqueNameBuilder(UniqueNameBuilder parentScope)
        : this()
    {
        _parentScope = parentScope;
    }

    public void Reserve(string name) => _usedNames.Add(name);

    public void Reserve(IEnumerable<string> names) => _usedNames.AddRange(names);

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

    public string New(string name, IEnumerable<string> reservedNames)
    {
        var scope = NewScope();
        scope.Reserve(reservedNames);
        var uniqueName = scope.New(name);
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
