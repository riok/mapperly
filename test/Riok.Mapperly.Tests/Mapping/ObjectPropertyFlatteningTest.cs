using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectPropertyFlatteningTest
{
    [Fact]
    public void ManualFlattenedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"Value.Id\", \"MyValueId\")] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string MyValueId { get; set; } }",
            "class C { public string Id { get; set; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.MyValueId = source.Value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.ValueId = source.Value.Id;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value.Id = source.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedManuallyConfiguredPropertyShouldOnlySetOnce()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"ValueId\", \"Value.Id\")] partial A Map(B source);",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value.Id = source.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C? Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value ??= new();
                target.Value.Id = source.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyShouldPreferFlattened()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C Value { get; set; } public string ValueId { get; set; } }",
            "class B { public C Value { get; set; } public string ValueId { get; set; } }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                target.ValueId = source.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyMultiple()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } public int ValueValueId { get; set; } }",
            "class C { public string Id { get; set; } public D Value { get; set; } }",
            "class D { public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value.Id = source.ValueId;
                target.Value.Value.Id = source.ValueValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyMultipleShouldPreferFlattened()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C Value { get; set; } public string ValueId { get; set; } }",
            "class B { public string ValueId { get; set; } public string ValueValue { get; set; } }",
            "class C { public string Id { get; set; } public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.ValueId = source.ValueId;
                target.Value.Value = source.ValueValue;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyMultipleShouldPreferAndRewriteFlattened()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C Value { get; set; } public string ValueId { get; set; } }",
            "class B { public C Value { get; set; } public string ValueValue { get; set; } }",
            "class C { public string Id { get; set; } public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                target.ValueId = source.Value.Id;
                target.Value.Value = source.ValueValue;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyMultipleShouldPreferFlattenedNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C? Value { get; set; } public string ValueId { get; set; } }",
            "class B { public string ValueId { get; set; } public string ValueValue { get; set; } }",
            "class C { public string Id { get; set; } public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value ??= new();
                target.ValueId = source.ValueId;
                target.Value.Value = source.ValueValue;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyMultipleNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public C? Value { get; set; } }",
            "class B { public string ValueId { get; set; } public int ValueValueId { get; set; } }",
            "class C { public string Id { get; set; } public D? Value { get; set; } }",
            "class D { public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value ??= new();
                target.Value.Value ??= new();
                target.Value.Id = source.ValueId;
                target.Value.Value.Id = source.ValueValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoUnflattenedPropertyAvailableShouldPreferShortened()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } public string ValueId { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C { public string Id { get; set; }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.Value.Id = source.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedPropertyAvailableShouldPreferNonFlattened()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } public string ValueId { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.ValueId = source.ValueId;
                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.ValueId = source.Value.Id;
                }

                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedMultiplePropertiesNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public string ValueId { get; set; } public string ValueName { get; set; } }",
            "class C { public Guid Id { get; set; } public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.ValueId = source.Value.Id.ToString();
                    target.ValueName = source.Value.Name;
                }

                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedPropertyNullableValueTypePath()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "public class A { public C Id { get; set; } }",
            "public class B { public int IdValue { get; set; } }",
            "public class C { public int? Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Id.Value != null)
                {
                    target.IdValue = source.Id.Value.Value;
                }

                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedPropertyNullableValueTypePathShouldResolve()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "public class A { public C? Prop { get; set; } }",
            "public class B { public string PropInteger { get; set; } }",
            "public struct C { public int Integer { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Prop != null)
                {
                    target.PropInteger = source.Prop.Value.Integer.ToString();
                }

                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedPropertyNullableValueTypePathShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "public record A(int? Id);",
            "public record B { public int IdValue { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedMultiplePropertiesPathDisabledNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } public string ValueName { get; set; } }",
            "class C { public Guid Id { get; set; } public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::B();
                if (source.Value != null)
                {
                    target.ValueId = source.Value.Id.ToString();
                }

                target.ValueName = source.Value?.Name;
                return target;
                """
            );
    }

    [Fact]
    public void AutoFlattenedPropertyPathDisabledNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueName { get; set; } }",
            "class C { public string Name { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::B();
                target.ValueName = source.Value?.Name;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C { public string Id { get; set; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value.Id = source.MyValueId;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyShouldIgnoreAuto()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"ValueId2\", \"Value.Id\")] partial A Map(B source);",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } public string ValueId2 }",
            "class C { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value.Id = source.ValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\"), MapProperty($\"MyValueId2\", \"Value.Id2\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value ??= new();
                target.Value.Id = source.MyValueId;
                target.Value.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public Task ManualUnflattenedPropertyNullablePathNoParameterlessCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManualUnflattenedPropertySourcePropertyNotFoundShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueIdXXX\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManualUnflattenedPropertyTargetPropertyPathWriteOnlyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManualUnflattenedPropertyTargetPropertyNotFoundShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.IdXXX\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ManualNestedPropertyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"Value1.Value1.Id1\", \"Value2.Value2.Id2\")]"
                + "[MapProperty(\"Value1.Value1.Id10\", \"Value2.Value2.Id20\")]"
                + "[MapProperty(new[] { \"Value1\", \"Id100\" }, new[] { \"Value2\", \"Id200\" })]"
                + "partial B Map(A source);",
            "class A { public C? Value1 { get; set; } }",
            "class B { public E? Value2 { get; set; } }",
            "class C { public D? Value1 { get; set; } public string Id100 { get; set; } }",
            "class D { public string Id1 { get; set; } public string Id10 { get; set; } }",
            "class E { public F? Value2 { get; set; } public string Id200 { get; set; } }",
            "class F { public string Id2 { get; set; } public string Id20 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value1 != null)
                {
                    target.Value2 ??= new();
                    if (source.Value1?.Value1 != null)
                    {
                        target.Value2.Value2 ??= new();
                        target.Value2.Value2.Id2 = source.Value1.Value1.Id1;
                        target.Value2.Value2.Id20 = source.Value1.Value1.Id10;
                    }

                    target.Value2.Id200 = source.Value1.Id100;
                }

                return target;
                """
            );
    }
}
