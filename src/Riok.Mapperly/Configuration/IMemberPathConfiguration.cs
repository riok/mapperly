namespace Riok.Mapperly.Configuration;

public interface IMemberPathConfiguration
{
    string RootName { get; }

    string FullName { get; }

    int PathCount { get; }
}
