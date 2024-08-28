using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UnsafeAccessorTest
{
    [Fact]
    public Task PrivateProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int _value { get; set; } }",
            "class B { private int _value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ProtectedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { protected int value { get; set; } }",
            "class B { protected int value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateNestedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("nested.value", "value")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private C nested { get; set; } }",
            "class B { private int value { get; set; } }",
            "class C { private int value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateExistingTargetProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial void Map(A source, B dest);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int value { get; set; } }",
            "class B { private int value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateExistingTargetEnumerableProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A source, B dest);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private List<int> value { get; } }",
            "class B { private List<int> value { get;} }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateNestedNullableProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("nested.value", "value")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private C? nested { get; set; } }",
            "class B { private int value { get; set; } }",
            "class C { private int? value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateNestedNullablePropertyShouldInitialize()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("nested.value", "nested.value")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private C nested { get; set; } }",
            "class B { private D nested { get; set; } }",
            "class C { private int value { get; set; } }",
            "class D { private int value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ManualUnflattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { private C? Value { get; set; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (target.GetValue() == null)
                {
                    target.SetValue(new global::C());
                }
                target.GetValue().Id = source.MyValueId;
                target.GetValue().Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public void ManualUnflattenedPropertyNullablePathWithWithObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory]
            C CreateMyC() => new C();

            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { private C? Value { get; set; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                if (target.GetValue() == null)
                {
                    target.SetValue(CreateMyC());
                }
                target.GetValue().Id = source.MyValueId;
                target.GetValue().Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public Task ManualUnflattenedPropertyNullablePathWithPrivateConstructor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.Default with
            {
                IncludedMembers = MemberVisibility.All,
                IncludedConstructors = MemberVisibility.All,
            },
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { private C? Value { get; set; } }",
            "class C { private C() {} public string Id { get; set; } public string Id2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void PropertyWithPrivateSetter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source); ",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; private set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.SetValue(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void PropertyWithProtectedSetterWhenDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All & ~MemberVisibility.Protected),
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; protected set; } }"
        );

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics).Should().HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task PrivateField()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int value }",
            "class B { private int value }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ManualUnflattenedFieldNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { private C? Value }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.GetValue() ??= new global::C();
                target.GetValue().Id = source.MyValueId;
                target.GetValue().Id2 = source.MyValueId2;
                return target;
                """
            );
    }

    [Fact]
    public Task ManualUnflattenedFieldNullablePathPrivateConstructor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("MyValueId", "Value.Id")]
            [MapProperty("MyValueId2", "Value.Id2")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.Default with
            {
                IncludedMembers = MemberVisibility.All,
                IncludedConstructors = MemberVisibility.All,
            },
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { private C? Value }",
            "class C { private C() {} public string Id { get; set; } public string Id2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task GeneratedMethodShouldNotHaveConflictingName()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int value { get; set; } public void GetValue() { } public void GetValue1() { } }",
            "class B { private int value { get; set; } public void Setvalue() { } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void InitPrivatePropertyShouldNotMap()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int value { get; set; } }",
            "class B { private int value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToInitOnlyMemberPath)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void RequiredPrivateSetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.AllAccessible),
            "class A { public int Value { get; set; } }",
            "class B { public required int Value { get; private set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void QueryablePrivateToPrivatePropertyShouldNotGenerate()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int Value { get; set; } }",
            "class B { private int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void QueryablePrivateToPublicPropertyShouldNotGenerate()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapFromWriteOnlyMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void UnmappedMembersShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int _value1 { get; set; } }",
            "class B { private int _value2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void IgnoreUnmappedMembers()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreSource("_value1")]
            [MapperIgnoreTarget("_value2")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private int _value1 { get; set; } }",
            "class B { private int _value2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void MemberVisibilityPublic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.Public),
            "class A { public int PublicValue { get; set; } private int _privateValue { get; set; } internal int InternalValue { get; set; } }",
            "class B { public int PublicValue { get; set; } private int _privateValue { get; set; } internal int InternalValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.PublicValue = source.PublicValue;
                return target;
                """
            );
    }

    [Fact]
    public Task PrivateConstructor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.WithConstructorVisibility(MemberVisibility.All),
            "class A { public int IntValue { get; set; } private string _stringValue { get; set; } }",
            "class B { private B(int intValue) {} private string _stringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateConstructorAndMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            TestSourceBuilderOptions.Default with
            {
                IncludedMembers = MemberVisibility.All,
                IncludedConstructors = MemberVisibility.All,
            },
            "class A { private int _intValue { get; set; } private string _stringValue { get; set; } }",
            "class B { private B(int _intValue) {} private string _stringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void MemberVisibilityAllWithoutPublic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All & ~MemberVisibility.Public),
            "class A { public int PublicValue { get; set; } private int _privateValue { get; set; } internal int InternalValue { get; set; } }",
            "class B { public int PublicValue { get; set; } private int _privateValue { get; set; } internal int InternalValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.SetPrivateValue(source.GetPrivateValue());
                target.InternalValue = source.InternalValue;
                return target;
                """
            );
    }

    [Fact]
    public Task AssemblyDefaultShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaultsAttribute(IncludedMembers = MemberVisibility.All)]
            [Mapper()]
            public partial class MyMapper
            {
                private partial B Map(A source);
            }

            class A { private int value { get; set; } }

            class B { private int value { get; set; } }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassAttributeShouldOverrideAssemblyDefault()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaultsAttribute(IncludedMembers = MemberVisibility.All)]
            [Mapper(IncludedMembers = MemberVisibility.AllAccessible)]
            public partial class MyMapper
            {
                private partial B Map(A value);
            }

            class A { private int privateValue { get; set; } public int PublicValue { get; set; } }

            class B { private int privateValue { get; set; } public int PublicValue { get; set; } }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapperInNestedClassShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            class A { private int value { get; set; } }

            class B { private int value { get; set; } }

            public static partial class CarFeature
            {
                public static partial class Mappers
                {
                    [Mapper(IncludedMembers = MemberVisibility.All)]
                    public partial class CarMapper
                    {
                        public partial B Map(A value);
                    }
                }
            },
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
