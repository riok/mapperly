using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class DerivedExistingTargetTypeTest
{
    [Fact]
    public Task WithAbstractBaseClassShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            public partial void Map(A src, B trg);
            """,
            "abstract class A { public string BaseValue { get; set; } }",
            "abstract class B { public string BaseValue { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class ASubType2 : A { public string Value1 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }",
            "class BSubType2 : B { public string Value1 { get; set; } }"
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
            public partial void Map(A src, B trg);
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
            public partial void Map(A src, B trg);
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
            public partial void Map(A? src, B trg);
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
            public partial void Map(A? src, B? trg);
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
    public void NotAssignableTargetTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<AImpl1, BImpl1>]
            public partial void Map(A src, B trg);
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
    public Task WithBaseTypeConfigShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<ASubType1, BSubType1>]
            [MapDerivedType<ASubType2, BSubType2>]
            [MapProperty(nameof(A.BaseValueA), nameof(B.BaseValueB)]
            public partial void Map(A src, B trg);
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
            public partial void Map(A src, B trg);
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
            public partial void Map(A src, B trg);
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
            public partial void Map(object src, object trg);
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
