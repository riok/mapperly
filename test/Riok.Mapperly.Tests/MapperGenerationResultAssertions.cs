using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests;

public class MapperGenerationResultAssertions
{
    private readonly MapperGenerationResult _mapper;
    private readonly HashSet<Diagnostic> _notAssertedDiagnostics;

    public MapperGenerationResultAssertions(MapperGenerationResult mapper)
    {
        _mapper = mapper;
        _notAssertedDiagnostics = new HashSet<Diagnostic>(_mapper.Diagnostics);
    }

    public MapperGenerationResultAssertions HaveDiagnostics()
    {
        _mapper.Diagnostics.Should().NotBeEmpty();
        return this;
    }

    public MapperGenerationResultAssertions HaveAssertedAllDiagnostics()
    {
        if (_notAssertedDiagnostics.Count == 0)
            return this;

        var assertions = _notAssertedDiagnostics.GroupBy(x => x.Descriptor.Id).Select(BuildDiagnosticAssertions);
        Assert.Fail(
            $"""
            {_notAssertedDiagnostics.Count} not asserted diagnostics found.
            Code to assert missing diagnostics:
            .{string.Join(Environment.NewLine + ".", assertions)}
            """
        );
        return this;
    }

    private string BuildDiagnosticAssertions(IGrouping<string, Diagnostic> diagnosticGroup)
    {
        var diagnosticDescriptorFieldName = typeof(DiagnosticDescriptors)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(x => (x.Name, Value: x.GetValue(null)))
            .Where(x => x.Value is DiagnosticDescriptor && ((DiagnosticDescriptor)x.Value!).Id.Equals(diagnosticGroup.Key))
            .Select(x => x.Name)
            .Single();
        var diagnosticDescriptorAccess = $"{typeof(DiagnosticDescriptors).FullName}.{diagnosticDescriptorFieldName}";
        if (!diagnosticGroup.Skip(1).Any())
        {
            return $"{nameof(HaveDiagnostic)}({diagnosticDescriptorAccess}, \"{diagnosticGroup.First().GetMessage()}\")";
        }

        return $"""
            {nameof(HaveDiagnostics)}(
                {diagnosticDescriptorAccess},
                {string.Join($",{Environment.NewLine}    ", diagnosticGroup.Select(y => $"\"{y.GetMessage()}\""))}
            )
            """;
    }

    public MapperGenerationResultAssertions OnlyHaveDiagnosticSeverities(IReadOnlySet<DiagnosticSeverity> allowedDiagnosticSeverities)
    {
        _mapper.Diagnostics.FirstOrDefault(d => !allowedDiagnosticSeverities.Contains(d.Severity)).Should().BeNull();
        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostics(DiagnosticDescriptor descriptor, params string[] messages)
    {
        var diagnostics = GetDiagnostics(descriptor);
        var max = Math.Min(diagnostics.Count, messages.Length);
        for (var i = 0; i < max; i++)
        {
            var diagnostic = diagnostics[i];
            diagnostic.GetMessage().Should().Be(messages[i]);
            _notAssertedDiagnostics.Remove(diagnostic);
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostic(DiagnosticDescriptor descriptor)
    {
        foreach (var diagnostic in GetDiagnostics(descriptor))
        {
            _notAssertedDiagnostics.Remove(diagnostic);
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostic(DiagnosticDescriptor descriptor, string message)
    {
        var diagnostics = GetDiagnostics(descriptor);
        var matchedDiagnostic = diagnostics.FirstOrDefault(x => x.GetMessage().Equals(message));
        if (matchedDiagnostic != null)
        {
            _notAssertedDiagnostics.Remove(matchedDiagnostic);
            return this;
        }

        var matchingIdDiagnostic =
            _notAssertedDiagnostics.FirstOrDefault(x => x.Descriptor.Equals(descriptor))
            ?? _mapper.Diagnostics.First(x => x.Descriptor.Equals(descriptor));
        matchingIdDiagnostic.GetMessage().Should().Be(message, $"message of {descriptor.Id} should match");
        return this;
    }

    public MapperGenerationResultAssertions HaveSingleMethodBody([StringSyntax(StringSyntax.CSharp)] string mapperMethodBody)
    {
        switch (_mapper.Methods.Count)
        {
            case 0:
                Assert.Fail("No generated method found");
                break;
            case 1:
                _mapper.Methods.First().Value.Body.Should().Be(mapperMethodBody.ReplaceLineEndings());
                break;
            default:
                Assert.Fail($"Found multiple methods ({_mapper.Methods.Count}): {string.Join(", ", _mapper.Methods.Select(x => x.Key))}");
                break;
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveMethodCount(int count)
    {
        _mapper.Methods.Should().HaveCount(count);
        return this;
    }

    public MapperGenerationResultAssertions AllMethodsHaveBody([StringSyntax(LanguageNames.CSharp)] string mapperMethodBody)
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

    public MapperGenerationResultAssertions HaveMapMethodWithGenericConstraints(
        string methodName,
        [StringSyntax(StringSyntax.CSharp)] string? constraintClauses
    )
    {
        _mapper.Methods[methodName].ConstraintClauses.Should().Be(constraintClauses);
        return this;
    }

    public MapperGenerationResultAssertions HaveMapMethodWithGenericConstraints(
        [StringSyntax(StringSyntax.CSharp)] string? constraintClauses
    ) => HaveMapMethodWithGenericConstraints(TestSourceBuilder.DefaultMapMethodName, constraintClauses);

    private IReadOnlyList<Diagnostic> GetDiagnostics(DiagnosticDescriptor descriptor)
    {
        if (_mapper.DiagnosticsByDescriptorId.TryGetValue(descriptor.Id, out var diagnostics))
            return diagnostics;

        var foundIds = _mapper.Diagnostics.Count == 0 ? "<none>" : string.Join(", ", _mapper.Diagnostics.Select(x => x.Descriptor.Id));
        throw new Exception($"No diagnostic with id {descriptor.Id} found, found diagnostic ids: {foundIds}");
    }
}
