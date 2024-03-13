using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UseMapperTest
{
    [Fact]
    public void UseMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(_otherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UsePropertyMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private OtherMapper otherMapper { get; } = new();
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(otherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void StaticMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(_otherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public Task ReferenceHandling()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            private partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source, [ReferenceHandler] IReferenceHandler refHandler) => new BExternal(); }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceHandlingEnabledNoParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            private partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void IgnorePrivateMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { private BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseGeneratedMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "[Mapper] partial class OtherMapper { public partial BExternal ToBExternal(AExternal source); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(_otherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void IgnoreInvalidSignature()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public void NotAMappingMethod(AExternal source) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void PreferInternalMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            private partial BExternal MapInternal(AExternal source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapInternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void PreferInternalImplementedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();
            partial B Map(A source);
            private BExternal MapInternal(AExternal source) => new BExternal();
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapInternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void NullableFieldShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper? _otherMapper;
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ExternalMapperMemberCannotBeNullable,
                "The used mapper member Mapper._otherMapper cannot be nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void DisabledNullableShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper;
            partial B Map(A source);
            """,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveMapMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::B(_otherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseMapperWithDisabledAutoUserMappingsExplicitlyMarkedMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper;
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { [UserMapping(Default = true)] public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(_otherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseMapperWithDisabledAutoUserMappingsExplicitlyMarkedButIgnored()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper;
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { [UserMapping(Ignore = true)] public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseMapperWithExplicitlyIgnoredMappingMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper;
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "record A(AExternal Value);",
            "record B(BExternal Value);",
            "record AExternal();",
            "record BExternal();",
            "class OtherMapper { [UserMapping(Ignore = true)] public BExternal ToBExternal(AExternal source) => new BExternal(); }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }
}
