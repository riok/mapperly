using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UserMethodAdditionalParameterForwardingTest
{
    [Fact]
    public void MapValueUseMethodWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetValue))]
            partial B Map(A src, int ctx);
            private int GetValue(int ctx) => ctx * 2;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = GetValue(ctx);
                return target;
                """
            );
    }

    [Fact]
    public void MapValueUseMethodWithMultipleAdditionalParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(Combine))]
            partial B Map(A src, int first, int second);
            private int Combine(int first, int second) => first + second;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = Combine(first, second);
                return target;
                """
            );
    }

    [Fact]
    public void MapValueUseMethodWithZeroParamsStillWorks()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetDefault))]
            partial B Map(A src);
            private int GetDefault() => 42;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = GetDefault();
                return target;
                """
            );
    }

    [Fact]
    public void NestedMappingWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, int ctx);
            private partial BNested MapNested(ANested src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapNested(src.Nested, ctx);
                return target;
                """
            );
    }

    [Fact]
    public void NestedMappingFallsBackToParameterlessWhenNoMatchingUserMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapToBNested(src.Nested);
                return target;
                """
            )
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void ParameterUsedByBothPropertyAndNestedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("CtxValue", Use = nameof(GetCtx))]
            partial B Map(A src, int ctx);
            private int GetCtx(int ctx) => ctx;
            private partial BNested MapNested(ANested src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } public int CtxValue { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapNested(src.Nested, ctx);
                target.CtxValue = GetCtx(ctx);
                return target;
                """
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapValueUseMethodWithUnsatisfiableParametersShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetValue))]
            partial B Map(A src);
            private int GetValue(int ctx) => ctx * 2;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueMethodParametersUnsatisfied,
                "The method GetValue referenced by MapValue has parameters that cannot be matched from the mapping's additional parameters"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MapPropertyUseWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Result), Use = nameof(Transform))]
            partial B Map(A src, int multiplier);
            private partial int Transform(int value, int multiplier);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Result { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Result = Transform(src.Value, multiplier);
                return target;
                """
            );
    }

    [Fact]
    public void MapPropertyUseWithMultipleAdditionalParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Result), Use = nameof(Transform))]
            partial B Map(A src, int multiplier, int offset);
            private partial int Transform(int value, int multiplier, int offset);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Result { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Result = Transform(src.Value, multiplier, offset);
                return target;
                """
            );
    }

    [Fact]
    public void MapPropertyUseWithUnsatisfiableParametersShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Result), Use = nameof(Transform))]
            partial B Map(A src);
            private partial int Transform(int value, int multiplier);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Result { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NamedMappingParametersUnsatisfied,
                "The named mapping Transform has additional parameters that cannot be matched from the caller's scope"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void ExistingTargetWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial void Update(A src, [MappingTarget] B target, int ctx);
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int Ctx { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "Update",
                """
                target.StringValue = src.StringValue;
                target.Ctx = ctx;
                """
            );
    }

    [Fact]
    public void ExistingTargetWithNestedMappingForwarding()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial void Update(A src, [MappingTarget] B target, int ctx);
            private partial BNested MapNested(ANested src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "Update",
                """
                target.Nested = MapNested(src.Nested, ctx);
                """
            );
    }

    [Fact]
    public void ExistingTargetWithMapValueUse()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetValue))]
            partial void Update(A src, [MappingTarget] B target, int ctx);
            private int GetValue(int ctx) => ctx * 2;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "Update",
                """
                target.StringValue = src.StringValue;
                target.IntValue = GetValue(ctx);
                """
            );
    }

    [Fact]
    public void MultipleUseMethodsConsumingSameParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetDouble))]
            [MapValue("OtherValue", Use = nameof(GetTriple))]
            partial B Map(A src, int ctx);
            private int GetDouble(int ctx) => ctx * 2;
            private int GetTriple(int ctx) => ctx * 3;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } public int OtherValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = GetDouble(ctx);
                target.OtherValue = GetTriple(ctx);
                return target;
                """
            );
    }

    [Fact]
    public void ParameterMappedToPropertyAndConsumedByNestedMapping()
    {
        // The parameter 'ctx' is mapped to the target property B.Ctx directly by name,
        // AND also forwarded to a nested mapping method MapNested.
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, int ctx);
            private partial BNested MapNested(ANested src, int ctx);
            """,
            """
            class A { public ANested Nested { get; set; } }
            class B { public BNested Nested { get; set; } public int Ctx { get; set; } }
            class ANested { public int ValueA { get; set; } }
            class BNested { public int ValueA { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapNested(src.Nested, ctx);
                target.Ctx = ctx;
                return target;
                """
            );
    }

    [Fact]
    public void ReferenceHandlingWithAdditionalParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int ctx);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int Ctx { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var refHandler = new global::Riok.Mapperly.Abstractions.ReferenceHandling.PreserveReferenceHandler();
                if (refHandler.TryGetReference<global::A, global::B>(src, out var existingTargetReference))
                    return existingTargetReference;
                var target = new global::B();
                refHandler.SetReference<global::A, global::B>(src, target);
                target.StringValue = src.StringValue;
                target.Ctx = ctx;
                return target;
                """
            );
    }

    [Fact]
    public void DeepNestingForwardsParametersThroughMultipleLevels()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial C Map(A src, int ctx);
            private partial CInner MapInner(AInner src, int ctx);
            private partial CDeep MapDeep(ADeep src, int ctx);
            """,
            """
            class A { public AInner Inner { get; set; } }
            class C { public CInner Inner { get; set; } }
            class AInner { public ADeep Deep { get; set; } }
            class CInner { public CDeep Deep { get; set; } public int Ctx { get; set; } }
            class ADeep { public int Value { get; set; } }
            class CDeep { public int Value { get; set; } public int Ctx { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::C();
                target.Inner = MapInner(src.Inner, ctx);
                return target;
                """
            )
            .HaveMethodBody(
                "MapInner",
                """
                var target = new global::CInner();
                target.Deep = MapDeep(src.Deep, ctx);
                target.Ctx = ctx;
                return target;
                """
            )
            .HaveMethodBody(
                "MapDeep",
                """
                var target = new global::CDeep();
                target.Value = src.Value;
                target.Ctx = ctx;
                return target;
                """
            );
    }

    [Fact]
    public void MapPropertyFromSourceUseWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.IsLiked), Use = nameof(GetIsLiked))]
            partial B Map(A src, int currentUserId);
            private static bool GetIsLiked(A record, int currentUserId) => record.Likes.Any(l => l.UserId == currentUserId);
            """,
            """
            class A { public List<Like> Likes { get; set; } }
            class B { public bool IsLiked { get; set; } }
            class Like { public int UserId { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.IsLiked = GetIsLiked(src, currentUserId);
                return target;
                """
            );
    }

    [Fact]
    public void MapValueUseMethodWithVerbatimIdentifierParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("IntValue", Use = nameof(GetValue))]
            partial B Map(A src, int @class);
            private int GetValue(int @class) => @class * 2;
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                target.IntValue = GetValue(@class);
                return target;
                """
            );
    }

    [Fact]
    public void UnusedParameterWithNoConsumerShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int unusedParam);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.AdditionalParameterNotMapped,
                "The additional mapping method parameter unusedParam of the method Map is not mapped"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void ExternalMapperMethodWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UseMapper]
            private readonly OtherMapper _otherMapper = new();

            [MapProperty(nameof(A.Value), nameof(B.Result), Use = nameof(@_otherMapper.Transform))]
            partial B Map(A src, int multiplier);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Result { get; set; } }",
            "class OtherMapper { public int Transform(int value, int multiplier) => value * multiplier; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Result = _otherMapper.Transform(src.Value, multiplier);
                return target;
                """
            );
    }

    [Fact]
    public void ExternalStaticMapperMethodWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Result), Use = nameof(@OtherMapper.Transform))]
            partial B Map(A src, int multiplier);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Result { get; set; } }",
            "static class OtherMapper { public static int Transform(int value, int multiplier) => value * multiplier; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Result = global::OtherMapper.Transform(src.Value, multiplier);
                return target;
                """
            );
    }
}
