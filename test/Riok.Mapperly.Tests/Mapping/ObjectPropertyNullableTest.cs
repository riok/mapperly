using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyNullableTest
{
    [Fact]
    public void NullableIntToNullableIntProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int? Value { get; set; } }",
            "class B { public int? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void NullableIntToNonNullableIntProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int? Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableIntToNonNullableIntPropertyWithNoNullAssignment()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
            },
            "class A { public int? Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableIntToNonNullableIntPropertyWithNoNullAssignmentThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
                ThrowOnPropertyMappingNullMismatch = true,
            },
            "class A { public int? Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value.Value;
                }
                else
                {
                    throw new global::System.ArgumentNullException(nameof(source.Value));
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableStringToNonNullableStringProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string? Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNonNullableClassProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableStringToNullableStringProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string? Value { get; set; } }",
            "class B { public string? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void NullableStringToNullableStringPropertyWithNoNullAssignment()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
            },
            "class A { public string? Value { get; set; } }",
            "class B { public string? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableStringToNullableStringPropertyWithNoNullAssignmentAndThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
                ThrowOnPropertyMappingNullMismatch = true,
            },
            "class A { public string? Value { get; set; } }",
            "class B { public string? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value;
                }
                else
                {
                    throw new global::System.ArgumentNullException(nameof(source.Value));
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToSameNullableClass()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void NonNullableClassToNullableClassProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = MapToD(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNullableClassProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                else
                {
                    target.Value = null;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNullableClassPropertyWithNoNullAssignment()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNullableClassPropertyWithNoNullAssignmentThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
                ThrowOnPropertyMappingNullMismatch = true,
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                else
                {
                    throw new global::System.ArgumentNullException(nameof(source.Value));
                }
                return target;
                """
            );
    }

    [Fact]
    public void DisabledNullableClassPropertyToNonNullableProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "#nullable disable\n class A { public C Value { get; set; } }\n#nullable enable",
            "class B { public D Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassPropertyToDisabledNullableProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "#nullable disable\n class B { public D Value { get; set; } }\n#nullable enable",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                else
                {
                    target.Value = null;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableValueTypeToOtherNullableValueType()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public float? Value { get; set; } }",
            "class B { public decimal? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = new decimal(source.Value.Value);
                }
                else
                {
                    target.Value = null;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNonNullableClassPropertyThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnPropertyMappingNullMismatch = true,
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value of A to the target property Value of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                else
                {
                    throw new global::System.ArgumentNullException(nameof(source.Value));
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableNestedMembersShouldInitialize()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "SubValue.Value")]
            public partial B Map(A a)
            """,
            "class A { public C? Value { get; set; } }",
            "class B { public D? SubValue { get; set; } }",
            "class C { public int V { get; set; } }",
            "class D { public E? Value { get; set; } }",
            "class E { public int V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.SubValue ??= new global::D();
                if (a.Value != null)
                {
                    target.SubValue.Value = MapToE(a.Value);
                }
                else
                {
                    target.SubValue.Value = null;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableNestedMembersShouldInitializeWithNoNullAssignment()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("NullableValue1", "V.NullableValue1")]
            [MapProperty("NullableValue2", "V.NullableValue2")]
            public partial B Map(A a)
            """,
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
            },
            "class A { public int? NullableValue1 { get; set; } public int? NullableValue2 { get; set; } }",
            "class B { public C? V { get; set; } }",
            "class C { public int? NullableValue1 { get; set; } public int? NullableValue2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (a.NullableValue1 != null)
                {
                    target.V ??= new global::C();
                    target.V.NullableValue1 = a.NullableValue1.Value;
                }
                if (a.NullableValue2 != null)
                {
                    target.V ??= new global::C();
                    target.V.NullableValue2 = a.NullableValue2.Value;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableNestedMembersShouldInitializeWithNoNullAssignmentOutsideContainer()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("NullableValue1", "V.NullableValue1")]
            [MapProperty("Value2", "V.Value2")]
            public partial B Map(A a)
            """,
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
            },
            "class A { public int? NullableValue1 { get; set; } public int Value2 { get; set; } }",
            "class B { public C? V { get; set; } }",
            "class C { public int? NullableValue1 { get; set; } public int? Value2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (a.NullableValue1 != null)
                {
                    target.V ??= new global::C();
                    target.V.NullableValue1 = a.NullableValue1.Value;
                }
                target.V ??= new global::C();
                target.V.Value2 = a.Value2;
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNullableClassPropertyThrowShouldSetNull()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnPropertyMappingNullMismatch = true,
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                }
                else
                {
                    target.Value = null;
                }
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNullableClassFlattenedPropertyThrow()
    {
        // the flattened property is not nullable
        // therefore if source.Value is null
        // an exception should be thrown
        // instead of assigning null to target.Value.
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "Value")]
            [MapProperty("Value.Flattened", "ValueFlattened")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.Default with
            {
                ThrowOnPropertyMappingNullMismatch = true,
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } public string ValueFlattened { get; set; } }",
            "class C { public string V { get; set; } public string Flattened { get; set; } }",
            "class D { public string V { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member Flattened on the mapping source type C is not mapped to any member on the mapping target type D"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Value.Flattened of A to the target property ValueFlattened of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = MapToD(source.Value);
                    target.ValueFlattened = source.Value.Flattened;
                }
                else
                {
                    throw new global::System.ArgumentNullException(nameof(source.Value));
                }
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseUserImplementedMappingWithDisabledNullability()
    {
        var mapperBody = TestSourceBuilder.CSharp(
            """
            partial B Map(A source);
            D UserImplementedMap(C source) => new D();
            """
        );

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C {}",
            "class D {}"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::B();
                target.StringValue = source.StringValue;
                target.NestedValue = UserImplementedMap(source.NestedValue);
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseUserImplementedMappingWithNullableValueType()
    {
        var mapperBody = TestSourceBuilder.CSharp(
            """
            partial NotNullableType? To(TypeWithNullableProperty? y);
            public Wrapper Map(double? source) => source.HasValue ? new() { Test = source.Value } : new();
            """
        );

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "public class Wrapper { public double Test { get; set; } }",
            "public class TypeWithNullableProperty { public double? Test { get; set; } }",
            "public class NotNullableType { public Wrapper Test { get; set; } = new(); }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (y == null)
                    return default;
                var target = new global::NotNullableType();
                target.Test = Map(y.Test);
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseUserImplementedMappingWithNonNullableValueType()
    {
        var mapperBody = TestSourceBuilder.CSharp(
            """
            partial NotNullableType? To(TypeWithNullableProperty? y);
            public Wrapper Map(double source) => new() { Test = source.Value };
            """
        );

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "public class Wrapper { public double Test { get; set; } }",
            "public class TypeWithNullableProperty { public double? Test { get; set; } }",
            "public class NotNullableType { public Wrapper Test { get; set; } = new(); }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property Test of TypeWithNullableProperty to the target property Test of NotNullableType which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                if (y == null)
                    return default;
                var target = new global::NotNullableType();
                if (y.Test != null)
                {
                    target.Test = Map(y.Test.Value);
                }
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseUserImplementedMappingWithNonNullableValueTypeAndNullableValueTypeShouldChooseCorrect()
    {
        var mapperBody = TestSourceBuilder.CSharp(
            """
            partial NotNullableType? To(TypeWithNullableProperty? y);
            public Wrapper MapNonNullable(double source) => new() { Test = source.Value };
            public Wrapper MapNullable(double? source) => source.HasValue ? new() { Test = source.Value } : new();
            """
        );

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "public class Wrapper { public double Test { get; set; } }",
            "public class TypeWithNullableProperty { public double? Test { get; set; } public double Test2 { get; set; } }",
            "public class NotNullableType { public Wrapper Test { get; set; } = new(); public Wrapper Test2 { get; set; } = new(); }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (y == null)
                    return default;
                var target = new global::NotNullableType();
                target.Test = MapNullable(y.Test);
                target.Test2 = MapNonNullable(y.Test2);
                return target;
                """
            );
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInNestedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set;} }",
            "class B { public D Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInGenericProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public IEnumerable<C> Value { get; set;} }",
            "class B { public IReadOnlyCollection<D> Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInArrayProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C[] Value { get; set;} }",
            "class B { public D[] Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task NullableIntWithAdditionalFlattenedValueToNonNullableIntProperties()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Nested { get; set; } }",
            "class B { public D Nested { get; set; } public int NestedValue2 { set; } }",
            "class C { public int? Value1 { get; } public int? Value2 { get; } }",
            "class D { public int Value1 { set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void NullableDirectiveEnabledTargetWithSameNullableRefTypeAsPropertyAndInEnumerable()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Value { get; set; } public string[] Descriptions { get; set; } }",
            "#nullable disable\n class B { public string Value { get; set; } public string[] Descriptions { get; set; } }\n#nullable enable"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.Descriptions = (string?[])source.Descriptions;
                return target;
                """
            );
    }

    [Fact]
    public Task NullableToNullablePropertyWithAnotherNullableToNonNullableMappingShouldDirectAssign()
    {
        // see https://github.com/riok/mapperly/issues/1089
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial ADest? Map(A? source);
            public static partial BDest MapToDestinationB(B source);
            """,
            "public record A(int? Prop);",
            "public record B(List<int?> Prop);",
            "public record ADest(int? Prop);",
            "public record BDest(List<int> Prop);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ClassToRecordNoAccessibleSourceCtorShouldNotDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            """
            public class A
            {
                private A(C value)
                {
                    Value = value;
                }

                public C Value { get; }
            }
            """,
            "public record B(C Value);",
            "public record C(string StringValue);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public Task MixedNullableContextsWithDerivedTypesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            public partial BBase Map(ABase src);
            """,
            """
            #nullable disable
            public abstract record BBase
            {
                public List<BBase> Objects { get; init; } = [];
            }

            public record B : BBase;

            #nullable enable
            public abstract record ABase
            {
                public List<ABase> Objects { get; init; } = [];
            }

            public record A: ABase;
            """
        );
        return TestHelper.VerifyGenerator(source);
    }
}
