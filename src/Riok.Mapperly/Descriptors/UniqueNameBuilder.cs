namespace Riok.Mapperly.Descriptors;

public class UniqueNameBuilder
{
    private readonly HashSet<string> _usedNames = new();

    internal void Reserve(string name)
        => _usedNames.Add(name);

    internal string Build(string name)
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
