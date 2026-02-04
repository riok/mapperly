using Microsoft.CodeAnalysis;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Configuration;

public class BuildConfigurationTest
{
    [Fact]
    public void EnumShouldBeConfigurableByBuild()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "public partial B Map(A source);",
            "class A { public string Value { get; set; } }",
            "class B { public string Value2 { get; set; } }"
        );

        var options = TestHelperOptions.Default with
        {
            AnalyzerConfigOptions = new Dictionary<string, string> { { "build_property.MapperlyRequiredMappingStrategy", "None" } },
        };

        var result = TestHelper.GenerateMapper(source, options);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void BooleanShouldBeConfigurableByBuild()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("public partial A[] Map(A[] source);", "class A { }");

        var options = TestHelperOptions.Default with
        {
            AnalyzerConfigOptions = new Dictionary<string, string> { { "build_property.MapperlyUseDeepCloning", "true" } },
        };

        TestHelper
            .GenerateMapper(source, options)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::A[source.Length];
                for (var i = 0; i < source.Length; i++)
                {
                    target[i] = MapToA(source[i]);
                }
                return target;
                """
            );
    }

    [Fact]
    public void WarningIsReportedForInvalidBuildConfiguration()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("public partial A[] Map(A[] source);", "class A { }");

        var options = TestHelperOptions.Default with
        {
            AllowedDiagnosticSeverities = new HashSet<DiagnosticSeverity> { DiagnosticSeverity.Warning },
            AnalyzerConfigOptions = new Dictionary<string, string>
            {
                { "build_property.MapperlyUseDeepCloning", "NotABool" },
                { "build_property.MapperlyRequiredMappingStrategy", "NotAnEnum" },
            },
        };

        TestHelper
            .GenerateMapper(source, options)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ConfiguredMSBuildOptionInvalid,
                "The MSBuild option MapperlyUseDeepCloning with value NotABool could not be parsed as System.Boolean"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.ConfiguredMSBuildOptionInvalid,
                "The MSBuild option MapperlyRequiredMappingStrategy with value NotAnEnum could not be parsed as Riok.Mapperly.Abstractions.RequiredMappingStrategy"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void AttributeShouldOverrideBuildAllLowercase()
    {
        var source = """

            using Riok.Mapperly.Abstractions;
            using System.Collections.Generic;

            [assembly: MapperDefaults(UseDeepCloning = false)]

            public class A {}

            [Mapper]
            public partial class Mapper
            {
                public partial A[] Map(A[] source);
            }

            """;
        var options = TestHelperOptions.Default with
        {
            AnalyzerConfigOptions = new Dictionary<string, string> { { "build_property.mapperlyusedeepcloning", "true" } },
        };

        TestHelper.GenerateMapper(source, options).Should().HaveMapMethodBody("return source;");
    }

    [Fact]
    public void FlagsEnumShouldBeConfigurableByPipe()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("public partial B Map(A source);", "class A { }", "class B { }");

        var options = TestHelperOptions.Default with
        {
            AnalyzerConfigOptions = new Dictionary<string, string>
            {
                { "build_property.MapperlyRequiredMappingStrategy", "Source | Target" },
            },
        };

        var result = TestHelper.GenerateMapper(source, options);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void FlagsEnumShouldBeConfigurableByComma()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("public partial B Map(A source);", "class A { }", "class B { }");

        var options = TestHelperOptions.Default with
        {
            AnalyzerConfigOptions = new Dictionary<string, string>
            {
                { "build_property.MapperlyRequiredMappingStrategy", "Source, Target" },
            },
        };

        var result = TestHelper.GenerateMapper(source, options);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void EnumShouldBeConfigurableByInt()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("public partial B Map(A source);", "class A { }", "class B { }");

        // RequiredMappingStrategy.Source is 1
        var options = TestHelperOptions.Default with
        {
            AnalyzerConfigOptions = new Dictionary<string, string> { { "build_property.MapperlyRequiredMappingStrategy", "1" } },
        };

        var result = TestHelper.GenerateMapper(source, options);
        result.Diagnostics.ShouldBeEmpty();
    }
}
