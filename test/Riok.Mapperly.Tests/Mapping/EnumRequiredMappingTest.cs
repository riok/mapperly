using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumRequiredMappingTest
{
    [Fact]
    public void MapperAttributeRequiredMappingSourceWithUnmappedMember()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Source),
            "enum E1 { V1, V2, V3 }",
            "enum E2 { V1, V2 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member V3 (2) on E1 not found on target enum E2")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("return (global::E2)source;");
    }

    [Fact]
    public void MapperAttributeRequiredMappingSourceByNameWithUnmappedMember()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2",
            TestSourceBuilderOptions.Default with
            {
                RequiredMappingStrategy = RequiredMappingStrategy.Source,
                EnumMappingStrategy = EnumMappingStrategy.ByName,
            },
            "enum E1 { V1, V2, V3 }",
            "enum E2 { V1, V2 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceEnumValueNotMapped, "Enum member V3 (2) on E1 not found on target enum E2")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E1.V1 => global::E2.V1,
                    global::E1.V2 => global::E2.V2,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E1 is not supported"),
                };
                """
            );
    }

    [Fact]
    public void MapperAttributeRequiredMappingTargetWithUnmappedMember()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Target),
            "enum E1 { V1, V2 }",
            "enum E2 { V1, V2, V3 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, "Enum member V3 (2) on E2 not found on source enum E1")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("return (global::E2)source;");
    }

    [Fact]
    public void MapperAttributeRequiredMappingTargetByNameWithUnmappedMember()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2",
            TestSourceBuilderOptions.Default with
            {
                RequiredMappingStrategy = RequiredMappingStrategy.Target,
                EnumMappingStrategy = EnumMappingStrategy.ByName,
            },
            "enum E1 { V1, V2 }",
            "enum E2 { V1, V2, V3 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.TargetEnumValueNotMapped, "Enum member V3 (2) on E2 not found on source enum E1")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                return source switch
                {
                    global::E1.V1 => global::E2.V1,
                    global::E1.V2 => global::E2.V2,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(source), source, "The value of enum E1 is not supported"),
                };
                """
            );
    }

    [Fact]
    public void MapperAttributeRequiredMappingNoneWithUnmappedMember()
    {
        var source = TestSourceBuilder.Mapping(
            "E1",
            "E2",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.None),
            "enum E1 { V1, V2, V3 }",
            "enum E2 { V1, V2, V4 }"
        );

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::E2)source;");
    }

    [Fact]
    public void MethodAttributeRequiredMappingNoneShouldOverrideMapperAttributeWithUnmappedMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperRequiredMapping(RequiredMappingStrategy.None)] public partial E2 Map(E1 source);",
            TestSourceBuilderOptions.WithRequiredMappingStrategy(RequiredMappingStrategy.Source),
            "enum E1 { V1, V2, V3 }",
            "enum E2 { V1, V2, V4 }"
        );

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::E2)source;");
    }
}
