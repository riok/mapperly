using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

internal class MethodNameBuilder
{
    private const string MethodNamePrefix = "MapTo";
    private readonly HashSet<string> _usedNames = new();

    internal void Reserve(string name)
        => _usedNames.Add(name);

    internal string Build(MethodMapping mapping)
    {
        var i = 0;
        var prefix = MethodNamePrefix + mapping.TargetType.NonNullable().Name;
        var name = prefix;
        while (!_usedNames.Add(name))
        {
            i++;
            name = prefix + i;
        }

        return name;
    }
}
