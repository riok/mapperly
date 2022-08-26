using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public record DiagnosticMatcher(
    DiagnosticDescriptor Descriptor,
    string? Message = null)
{
    public bool Matches(Diagnostic diagnostic)
    {
        if (!diagnostic.Descriptor.Equals(Descriptor))
            return false;

        if (Message != null)
        {
            diagnostic.GetMessage().Should().Be(Message);
        }

        return true;
    }
}
