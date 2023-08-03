using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public class TestAssembly : IDisposable
{
    private readonly MemoryStream _data = new();

    internal TestAssembly(Compilation compilation)
    {
        var result = compilation.Emit(_data);
        Execute.Assertion.ForCondition(result.Success).FailWith(string.Join("\n", result.Diagnostics.Select(x => x.ToString())));

        _data.Seek(0, SeekOrigin.Begin);
        MetadataReference = MetadataReference.CreateFromStream(_data);
    }

    public MetadataReference MetadataReference { get; }

    public void Dispose() => _data.Dispose();
}
