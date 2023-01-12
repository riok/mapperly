namespace Riok.Mapperly.Helpers;

public class UniqueNameBuilder
{
    private readonly HashSet<string> _usedNames;

    public UniqueNameBuilder()
    {
        _usedNames = new HashSet<string>();
    }

    public UniqueNameBuilder(IEnumerable<string> usedNames)
    {
        _usedNames = new HashSet<string>(usedNames);
    }

    public void Reserve(string name)
        => _usedNames.Add(name);

    public UniqueNameBuilder NewScope()
        => new(_usedNames);

    public string New(string name)
    {
        var i = 0;
        var uniqueName = name;
        while (!_usedNames.Add(uniqueName))
        {
            i++;
            uniqueName = name + i;
        }

        return uniqueName;
    }
}
