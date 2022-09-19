using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests;

public class MapperGenerationResultAssertions
{
    private readonly MapperGenerationResult _mapper;

    public MapperGenerationResultAssertions(MapperGenerationResult mapper)
    {
        _mapper = mapper;
    }

    public MapperGenerationResultAssertions NotHaveDiagnostics(IReadOnlySet<DiagnosticSeverity> allowedDiagnosticSeverities)
    {
        _mapper.Diagnostics
            .FirstOrDefault(d => !allowedDiagnosticSeverities.Contains(d.Severity))
            .Should()
            .BeNull();
        return this;
    }

    public MapperGenerationResultAssertions HaveDiagnostic(DiagnosticMatcher diagnosticMatcher)
    {
        _mapper.Diagnostics.FirstOrDefault(diagnosticMatcher.Matches)
            .Should()
            .NotBeNull();
        return this;
    }

    public MapperGenerationResultAssertions HaveSingleMethodBody(string mapperMethodBody)
    {
        _mapper.MethodBodies.Single()
            .Value
            .Should()
            .Be(mapperMethodBody.ReplaceLineEndings());
        return this;
    }

    public MapperGenerationResultAssertions HaveMethodCount(int count)
    {
        _mapper.MethodBodies.Should().HaveCount(count);
        return this;
    }

    public MapperGenerationResultAssertions AllMethodsHaveBody(string mapperMethodBody)
    {
        mapperMethodBody = mapperMethodBody.ReplaceLineEndings();
        foreach (var methodBody in _mapper.MethodBodies.Values)
        {
            methodBody.Should().Be(mapperMethodBody);
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveMethods(params string[] methodNames)
    {
        foreach (var methodName in methodNames)
        {
            _mapper.MethodBodies.Keys.Should().Contain(methodName);
        }

        return this;
    }

    public MapperGenerationResultAssertions HaveMethodBody(string methodName, string mapperMethodBody)
    {
        _mapper.MethodBodies[methodName].Should().Be(mapperMethodBody.ReplaceLineEndings());
        return this;
    }

    public MapperGenerationResultAssertions HaveMapMethodBody(string mapperMethodBody)
        => HaveMethodBody(TestSourceBuilder.DefaultMapMethodName, mapperMethodBody);
}
