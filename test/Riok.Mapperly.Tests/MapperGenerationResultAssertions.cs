using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public class MapperGenerationResultAssertions
{
    private readonly MapperGenerationResult _mapper;

    public MapperGenerationResultAssertions(MapperGenerationResult mapper)
    {
        _mapper = mapper;
    }

    public MapperGenerationResultAssertions HaveDiagnostics()
    {
        _mapper.Diagnostics.Should().NotBeEmpty();
        return this;
    }

    public MapperGenerationResultAssertions NotHaveDiagnostics(IReadOnlySet<DiagnosticSeverity> allowedDiagnosticSeverities)
    {
        _mapper.Diagnostics.FirstOrDefault(d => !allowedDiagnosticSeverities.Contains(d.Severity)).Should().BeNull();
        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostics(DiagnosticDescriptor descriptor, params string[] descriptions)
    {
        var diags = _mapper.Diagnostics.Where(d => descriptor.Equals(d.Descriptor));
        diags.Select(d => d.GetMessage()).Should().BeEquivalentTo(descriptions, o => o.WithStrictOrdering());
        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostic(DiagnosticMatcher diagnosticMatcher)
    {
        var diag = _mapper.Diagnostics.FirstOrDefault(diagnosticMatcher.MatchesDescriptor);
        var foundIds = string.Join(", ", _mapper.Diagnostics.Select(x => x.Descriptor.Id));
        diag.Should().NotBeNull($"No diagnostic with id {diagnosticMatcher.Descriptor.Id} found, found diagnostic ids: {foundIds}");
        diagnosticMatcher.EnsureMatches(diag!);
        return this;
    }

    public MapperGenerationResultAssertions HaveSingleMethodBody([StringSyntax(StringSyntax.CSharp)] string mapperMethodBody)
    {
        _mapper.Methods.Single().Value.Body.Should().Be(mapperMethodBody.ReplaceLineEndings());
        return this;
    }

    public MapperGenerationResultAssertions HaveMethodCount(int count)
    {
        _mapper.Methods.Should().HaveCount(count);
        return this;
    }

    public MapperGenerationResultAssertions AllMethodsHaveBody(string mapperMethodBody)
    {
        mapperMethodBody = mapperMethodBody.ReplaceLineEndings().Trim();
        foreach (var method in _mapper.Methods.Values)
        {
            method.Body.Should().Be(mapperMethodBody);
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveMethods(params string[] methodNames)
    {
        foreach (var methodName in methodNames)
        {
            _mapper.Methods.Keys.Should().Contain(methodName);
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveOnlyMethods(params string[] methodNames)
    {
        HaveMethods(methodNames);
        HaveMethodCount(methodNames.Length);
        return this;
    }

    public MapperGenerationResultAssertions HaveMethodBody(string methodName, [StringSyntax(StringSyntax.CSharp)] string mapperMethodBody)
    {
        _mapper.Methods[methodName].Body.Should().Be(mapperMethodBody.ReplaceLineEndings().Trim(), $"Method: {methodName}");
        return this;
    }

    public MapperGenerationResultAssertions HaveMapMethodBody([StringSyntax(StringSyntax.CSharp)] string mapperMethodBody) =>
        HaveMethodBody(TestSourceBuilder.DefaultMapMethodName, mapperMethodBody);
}
