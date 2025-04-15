using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyFlatteningTest
{
    [Fact]
    public void ManualFlattenedPropertyWithFullNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(@A.Value.Id), nameof(B.MyValueId))] partial B Map(A source);",
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
    public void ManualFlattenedPropertyWithFullNameOfSourceAndWrongType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(@D.Value.Id), nameof(D.MyValueId))] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string MyValueId { get; set; } }",
            "class C { public string Id { get; set; }",
            "class D { public string? MyValueId { get; set; } public C? Value { get; set; } }"
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
    public void ManualFlattenedPropertyWithFullNameOfNamespacedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(@My.A.Value.Id), nameof(B.MyValueId))] partial B Map(My.A source);",
            "namespace My { class A { public C Value { get; set; } } } ",
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
    public void ManualFlattenedPropertyWithFullNameOfNestedSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(@My.Nested.A.Value.Id), nameof(B.MyValueId))] partial B Map(My.Nested.A source);",
            "namespace My { public class Nested { public class A { public C Value { get; set; } } } }",
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
    public void ManualFlattenedPropertyWithSourceArray()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(new string[]{nameof(C.Value), nameof(C.Value.Id)}, nameof(B.MyValueId))] partial B Map(A source);",
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
    public void ManualFlattenedPropertyWithSourceAndTargetArray()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(new string[]{nameof(C.Value), nameof(C.Value.Id)}, new string[] {nameof(B.MyValueId)})] partial B Map(A source);",
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
    public void ManualFlattenedPropertyWithInterpolatedNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapProperty($"{nameof(C.Value)}.{nameof(C.Value.Id)}", nameof(B.MyValueId))] partial B Map(A source);""",
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
            "class C { public string Id { get; set; }"
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member Value on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveAssertedAllDiagnostics()
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value.Id of A to the target property ValueId of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value.Id of A to the target property ValueId of B which is not nullable"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value.Name of A to the target property ValueName of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Id.Value of A to the target property IdValue of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
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
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Prop.Integer of A to the target property PropInteger of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
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
                else
                {
                    target.ValueId = null;
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
    public void ManualUnflattenedPropertyWithFullNameOfTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.MyValueId), nameof(@B.Value.Id))] partial B Map(A source);",
            "class A { public string MyValueId { get; set; }  }",
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
    public void ManualUnflattenedPropertyWithTargetArray()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(C.MyValueId), new string[] { nameof(B.Value), nameof(B.Value.Id) })] partial B Map(A source);",
            "class A { public string MyValueId { get; set; }  }",
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
    public void ManualUnflattenedPropertyInterpolatedFullNameOfTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapProperty(nameof(A.MyValueId), $"{nameof(B.Value)}.{nameof(B.Value.Id)}")] partial B Map(A source);""",
            "class A { public string MyValueId { get; set; }  }",
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
    public void ManualUnflattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"MyValueId\", \"Value.Id\"), MapProperty(\"MyValueId2\", \"Value.Id2\")] partial B Map(A source);",
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
                target.Value ??= new global::C();
                target.Value.Id = source.MyValueId;
                target.Value.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyReadOnlyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
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
    public void ManualUnflattenedPropertyDeepNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"MyValueId\", \"Value.Nested.Id\"), MapProperty(\"MyValueId2\", \"Value.Nested.Id2\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public D? Nested { get; set; } }",
            "class D { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value ??= new global::C();
                target.Value.Nested ??= new global::D();
                target.Value.Nested.Id = source.MyValueId;
                target.Value.Nested.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyDeepNullablePathObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory]
            C CreateMyC() => new C();

            [ObjectFactory]
            D CreateMyD() => new D();

            [MapProperty("MyValueId", "Value.Nested.Id")]
            [MapProperty("MyValueId2", "Value.Nested.Id2")]
            partial B Map(A source);
            """,
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public D? Nested { get; set; } }",
            "class D { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value ??= CreateMyC();
                target.Value.Nested ??= CreateMyD();
                target.Value.Nested.Id = source.MyValueId;
                target.Value.Nested.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public Task ManualUnflattenedPropertyNullablePathNoParameterlessCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"MyValueId\", \"Value.Id\")] private partial B Map(A source);",
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
            "[MapProperty(\"MyValueIdXXX\", \"Value.Id\")] private partial B Map(A source);",
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
            "[MapProperty($\"MyValueId\", \"Value.Id\")] private partial B Map(A source);",
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
            "[MapProperty(\"MyValueId\", \"Value.IdXXX\")] private partial B Map(A source);",
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
                    target.Value2 ??= new global::E();
                    target.Value2.Id200 = source.Value1.Id100;
                    if (source.Value1.Value1 != null)
                    {
                        target.Value2.Value2 ??= new global::F();
                        target.Value2.Value2.Id2 = source.Value1.Value1.Id1;
                        target.Value2.Value2.Id20 = source.Value1.Value1.Id10;
                    }
                }
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyNullablePathShouldNotNullInitialize()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            [MapProperty("Value", "Value")]
            public partial B Map(A source);
            """,
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } public C Value { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.Value.Id = source.MyValueId;
                target.Value.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyNullablePathShouldNullInitialize()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            [MapProperty("Value", "Value")]
            public partial B Map(A source);
            """,
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } public C? Value { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.Value ??= new global::C();
                target.Value.Id = source.MyValueId;
                target.Value.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyDeepNullablePathShouldNotNullInitialize()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "My.Value.Id")]
            [MapProperty("MyValueId2", "My.Value.Id2")]
            [MapProperty("My", "My")]
            [MapProperty("Value", "My.Value")]
            public partial B Map(A source);
            """,
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } public C My { get; set; }  public D Value { get; set; } }",
            "class B { public C? My { get; set; } }",
            "class C { public D? Value { get; set; } }",
            "class D { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.My = source.My;
                target.My.Value = source.Value;
                target.My.Value.Id = source.MyValueId;
                target.My.Value.Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void NotNullablePropertyOnTargetWithNoSuppressNullMismatchDiagnosticShouldGiveWarningRMG089()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"IdA\", \"IdB\")] private partial B Map(A source);",
            "class A { public int? IdA { get; set; } }",
            "class B { public int IdB { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property IdA of A to the target property IdB of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.IdA != null)
                {
                    target.IdB = source.IdA.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NotNullablePropertyOnTargetWithSuppressNullMismatchDiagnosticFalseShouldGiveWarningRMG089()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"IdA\", \"IdB\", SuppressNullMismatchDiagnostic = false)] private partial B Map(A source);",
            "class A { public int? IdA { get; set; } }",
            "class B { public int IdB { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property IdA of A to the target property IdB of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.IdA != null)
                {
                    target.IdB = source.IdA.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NotNullablePropertyOnTargetWithSuppressNullMismatchDiagnosticTrueShouldIgnoreWarningRMG089()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"IdA\", \"IdB\", SuppressNullMismatchDiagnostic = true)] private partial B Map(A source);",
            "class A { public int? IdA { get; set; } }",
            "class B { public int IdB { get; set; } }"
        );

        var mapperGenerationResult = TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics);

        mapperGenerationResult.Diagnostics.Should().BeEmpty();
        mapperGenerationResult
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.IdA != null)
                {
                    target.IdB = source.IdA.Value;
                }
                return target;
                """
            );
    }
}
