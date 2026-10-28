using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Riok.Mapperly.Tests.Mapping;

public class ExternalMappingReferenceTest
{
    [Theory]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    public void NamedEnumMappingFromAnotherAssembly(bool asCompilationReference, bool retarget, bool fallback)
    {
        using var contractsV1 = BuildContracts("1.0.0.0");
        using var contractsV2 = BuildContracts("2.0.0.0");
        var mapperSource = $$"""
            #define MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME
            using Riok.Mapperly.Abstractions;

            public enum DomainUnit { Ton, Kilogram, Kilometers }

            [Mapper]
            public static partial class SharedEnumUnitMapper
            {
                [MapEnum(EnumMappingStrategy.ByName{{(fallback ? ", FallbackValue = SharedUnit.NotDefined" : "")}})]
                [MapperIgnoreSourceValue(DomainUnit.Kilometers)]
                [MapperIgnoreTargetValue(SharedUnit.Pound)]
                [MapperIgnoreTargetValue(SharedUnit.NotDefined)]
                public static partial SharedUnit MapUnit(DomainUnit source);
            }
            """;
        var mapperCompilation = TestHelper
            .BuildCompilation(mapperSource)
            .WithAssemblyName("Mappers")
            .AddReferences(contractsV1.MetadataReference);
        CSharpGeneratorDriver
            .Create(new MapperGenerator())
            .RunGeneratorsAndUpdateCompilation(mapperCompilation, out var generatedMapperCompilation, out _);
        generatedMapperCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

        using var mapperAssembly = asCompilationReference
            ? TestAssembly.CreateAsCompilationReference(generatedMapperCompilation)
            : new TestAssembly(generatedMapperCompilation);
        var source = """
            using Riok.Mapperly.Abstractions;

            public class DomainSlip { public DomainUnit Unit { get; set; } }
            public class SharedSlip { public SharedUnit Unit { get; set; } }

            [Mapper]
            [UseStaticMapper(typeof(SharedEnumUnitMapper))]
            public partial class Mapper
            {
                [MapProperty(nameof(DomainSlip.Unit), nameof(SharedSlip.Unit), Use = nameof(@SharedEnumUnitMapper.MapUnit))]
                public partial SharedSlip Map(DomainSlip source);
            }
            """;
        var contracts = retarget ? contractsV2 : contractsV1;
        var compilation = TestHelper
            .BuildCompilation(source)
            .WithReferences(mapperCompilation.References)
            .ReplaceReference(contractsV1.MetadataReference, contracts.MetadataReference)
            .AddReferences(mapperAssembly.MetadataReference);

        // Verify that the test actually exercises attributes with and without source syntax.
        var referencedMethod = compilation
            .GetTypeByMetadataName("SharedEnumUnitMapper")!
            .GetMembers("MapUnit")
            .OfType<IMethodSymbol>()
            .Single();
        var enumAttribute = referencedMethod.GetAttributes().Single(x => x.AttributeClass!.Name == "MapEnumAttribute");
        (enumAttribute.ApplicationSyntaxReference == null).ShouldBe(!asCompilationReference || retarget);

        var driver = CSharpGeneratorDriver
            .Create(new MapperGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        outputCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        driver
            .GetRunResult()
            .GeneratedTrees.Single()
            .ToString()
            .ShouldContain("target.Unit = global::SharedEnumUnitMapper.MapUnit(source.Unit);");
    }

    private static TestAssembly BuildContracts(string version)
    {
        var compilation = TestHelper
            .BuildCompilation(
                $$"""
                [assembly: System.Reflection.AssemblyVersion("{{version}}")]
                public enum SharedUnit { NotDefined, Ton, Kilogram, Pound }
                """
            )
            .WithAssemblyName("Contracts");
        return new TestAssembly(compilation);
    }
}
