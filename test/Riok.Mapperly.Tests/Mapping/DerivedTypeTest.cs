using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class DerivedTypeTest
{
    [Fact]
    public Task WithAbstractBaseClassShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            public partial B Map(A src);
            """,
            "abstract class A { public string BaseValue { get; set; } }",
            "abstract class B { public string BaseValue { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class ASubType2 : A { public string Value2 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }",
            "class BSubType2 : B { public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithAbstractBaseClassAndNonGenericInterfaceShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType(typeof(ASubType1), typeof(BSubType1))]
            [MapDerivedType(typeof(ASubType2), typeof(BSubType2))]
            public partial B Map(A src);
            """,
            "abstract class A { public string BaseValue { get; set; } }",
            "abstract class B { public string BaseValue { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class ASubType2 : A { public string Value2 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }",
            "class BSubType2 : B { public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInterfaceShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            [MapDerivedType<AImpl2, BImpl2>]
            public partial B Map(A src);
            """,
            "interface A { string BaseValue { get; set; } }",
            "interface B { string BaseValue { get; set; }}",
            "class AImpl1 : A { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class AImpl2 : A { public string BaseValue { get; set; } public string Value2 { get; set; } }",
            "class BImpl1 : B { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class BImpl2 : B { public string BaseValue { get; set; } public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInterfaceSourceNullableShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            [MapDerivedType<AImpl2, BImpl2>]
            public partial B Map(A? src);
            """,
            "interface A { string BaseValue { get; set; } }",
            "interface B { string BaseValue { get; set; }}",
            "class AImpl1 : A { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class AImpl2 : A { public string BaseValue { get; set; } public string Value2 { get; set; } }",
            "class BImpl1 : B { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class BImpl2 : B { public string BaseValue { get; set; } public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInterfaceSourceAndTargetNullableShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            [MapDerivedType<AImpl2, BImpl2>]
            public partial B? Map(A? src);
            """,
            "interface A { string BaseValue { get; set; } }",
            "interface B { string BaseValue { get; set; }}",
            "class AImpl1 : A { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class AImpl2 : A { public string BaseValue { get; set; } public string Value2 { get; set; } }",
            "class BImpl1 : B { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class BImpl2 : B { public string BaseValue { get; set; } public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithObjectShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBody(
            """
            [MapDerivedType<string, int>]
            [MapDerivedType<int, string>]
            [MapDerivedType<IEnumerable<string>, IEnumerable<int>>]
            public partial object Map(object src);
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void NotAssignableTargetTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            public partial B Map(A src);
            """,
            "interface A {}",
            "interface B {}",
            "class AImpl1 : A { }",
            "class BImpl1 { }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.DerivedTargetTypeIsNotAssignableToReturnType,
                "Derived target type BImpl1 is not assignable to return type B"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task WithEnumerableOfInterfaceShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial IEnumerable<B> Map(IEnumerable<A> source);

            [MapDerivedType<AImpl1, BImpl1>]
            [MapDerivedType<AImpl2, BImpl2>]
            private partial B Map(A src);
            """,
            "interface A { string BaseValue { get; set; } }",
            "interface B { string BaseValue { get; set; }}",
            "class AImpl1 : A { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class AImpl2 : A { public string BaseValue { get; set; } public string Value2 { get; set; } }",
            "class BImpl1 : B { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class BImpl2 : B { public string BaseValue { get; set; } public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithInterfacePropertyShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial B Map(A source);

            [MapDerivedType<AImpl1, BImpl1>]
            [MapDerivedType<AImpl2, BImpl2>]
            private partial BIntf Map(AIntf src);
            """,
            "class A { public AIntf Value { get; set; } }",
            "class B { public BIntf Value { get; set; } }",
            "interface AIntf { string BaseValue { get; set; } }",
            "interface BIntf { string BaseValue { get; set; }}",
            "class AImpl1 : AIntf { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class AImpl2 : AIntf { public string BaseValue { get; set; } public string Value2 { get; set; } }",
            "class BImpl1 : BIntf { public string BaseValue { get; set; } public string Value1 { get; set; } }",
            "class BImpl2 : BIntf { public string BaseValue { get; set; } public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithBaseTypeConfigShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            [MapProperty(nameof(A.BaseValueA), nameof(B.BaseValueB)]
            public partial B Map(A src);
            """,
            "abstract class A { public string BaseValueA { get; set; } }",
            "abstract class B { public string BaseValueB { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class ASubType2 : A { public string Value2 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }",
            "class BSubType2 : B { public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithBaseTypeConfigAndSeparateMethodShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            [MapProperty(nameof(A.BaseValueA), nameof(B.BaseValueB)]
            public partial B Map(A src);

            [MapperIgnoreSource(nameof(A.BaseValueA)]
            [MapperIgnoreTarget(nameof(B.BaseValueB)]
            public partial BSubType1 Map(ASubType1 src);
            """,
            "abstract class A { public string BaseValueA { get; set; } }",
            "abstract class B { public string BaseValueB { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class ASubType2 : A { public string Value2 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }",
            "class BSubType2 : B { public string Value2 { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void NotAssignableSourceTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            public partial B Map(A src);
            """,
            "interface A {}",
            "interface B {}",
            "class AImpl1 { }",
            "class BImpl1 : B { }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.DerivedSourceTypeIsNotAssignableToParameterType,
                "Derived source type AImpl1 is not assignable to parameter type A"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DuplicatedSourceTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            [MapDerivedType<AImpl1, BImpl2>]
            public partial B Map(A src);
            """,
            "interface A {}",
            "interface B {}",
            "class AImpl1 : A { }",
            "class BImpl1 : B { }",
            "class BImpl2 : B { }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.DerivedSourceTypeDuplicated,
                "Derived source type AImpl1 is specified multiple times, a source type may only be specified once"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void NotMappableShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBody(
            """
            [MapDerivedType<Version, int>]
            public partial object Map(object src);
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                "Could not create mapping from System.Version to int. Consider implementing the mapping manually."
            )
            .HaveAssertedAllDiagnostics();
    }
}
