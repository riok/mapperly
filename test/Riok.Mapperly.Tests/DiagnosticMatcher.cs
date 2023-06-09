using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public record DiagnosticMatcher(DiagnosticDescriptor Descriptor, string? Message = null)
{
    public bool MatchesDescriptor(Diagnostic diagnostic) => Descriptor.Equals(diagnostic.Descriptor);

    public void EnsureMatches(Diagnostic diagnostic)
    {
        diagnostic.Descriptor.Id.Should().Be(Descriptor.Id);

        if (Message != null)
        {
            diagnostic.GetMessage().Should().Be(Message, $"Message for descriptor id {Descriptor.Id} does not match");
        }
    }
}
