using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public class TestAssembly : IDisposable
{
    private readonly MemoryStream _data = new();

    internal TestAssembly(Compilation compilation)
    {
        compilation.Emit(_data).Success.Should().BeTrue();

        _data.Seek(0, SeekOrigin.Begin);
        MetadataReference = MetadataReference.CreateFromStream(_data);
    }

    public MetadataReference MetadataReference { get; }

    public void Dispose() => _data.Dispose();
}
