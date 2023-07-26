namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
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
            .GenerateMapper(source)
            .Should()
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
                AllowNullPropertyAssignment = false
            },
            "class A { public int? Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
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
                ThrowOnPropertyMappingNullMismatch = true
            },
            "class A { public int? Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.Value != null)
                {
                    target.Value = source.Value.Value;
                }
                else
                {
                    throw new System.ArgumentNullException(nameof(source.Value.Value));
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
            .GenerateMapper(source)
            .Should()
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
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
                AllowNullPropertyAssignment = false
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
                ThrowOnPropertyMappingNullMismatch = true
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
                    throw new System.ArgumentNullException(nameof(source.Value));
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
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
    public void NullableClassToNullableClassPropertyWithNoNullAssignment()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
                ThrowOnPropertyMappingNullMismatch = true
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
                    throw new System.ArgumentNullException(nameof(source.Value));
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
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
    public void NullableClassPropertyToDisabledNullableProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "#nullable disable\n class B { public D Value { get; set; } }\n#nullable enable",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
    public void NullableClassToNonNullableClassPropertyThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnPropertyMappingNullMismatch = true
            },
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }"
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
                    throw new System.ArgumentNullException(nameof(source.Value));
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
            .GenerateMapper(source)
            .Should()
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
}
