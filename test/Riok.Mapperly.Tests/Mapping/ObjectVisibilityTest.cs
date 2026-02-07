using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectVisibilityTest
{
    [Fact]
    public void PrivateToPublicShouldIgnore()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { private string Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void PublicToPrivateShouldIgnore()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Value { get; set; } }",
            "class B { private string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void InternalToInternalShouldMap()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { internal string Value { get; set; } }",
            "class B { internal string Value { get; set; } }"
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
    public void InternalOtherAssemblyToInternalShouldIgnore()
    {
        var aSource = TestSourceBuilder.SyntaxTree("namespace A; public class A { internal string Value { get; set; } }");
        using var aAssembly = TestHelper.BuildAssembly("A", aSource);

        var source = TestSourceBuilder.Mapping("A.A", "B", "class B { internal string Value { get; set; } }");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics, [aAssembly])
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void InternalToInternalOtherAssemblyShouldIgnore()
    {
        var aSource = TestSourceBuilder.SyntaxTree(
            """
            namespace A;
            public class A { public string PublicValue { get; set; } internal string InternalValue { get; set; } private string PrivateValue { get; set; } }
            """
        );

        using var aAssembly = TestHelper.BuildAssembly("A", aSource);

        var source = TestSourceBuilder.Mapping(
            "A.A",
            "B",
            "class B { public string PublicValue { get; set; } internal string InternalValue { get; set; } private string PrivateValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics, [aAssembly])
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.PublicValue = source.PublicValue;
                return target;
                """
            );
    }

    [Fact]
    public void InternalOtherAssemblyWithGrantedVisibilityToInternalShouldMap()
    {
        var aSource = TestSourceBuilder.SyntaxTree(
            """
            [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

            namespace A;
            public class A { public string PublicValue { get; set; } internal string InternalValue { get; set; } private string PrivateValue { get; set; } }
            """
        );

        using var aAssembly = TestHelper.BuildAssembly("A", aSource);

        var source = TestSourceBuilder.Mapping(
            "A.A",
            "B",
            "class B { public string PublicValue { get; set; } internal string InternalValue { get; set; } private string PrivateValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.Default, [aAssembly])
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.PublicValue = source.PublicValue;
                target.InternalValue = source.InternalValue;
                return target;
                """
            );
    }

    [Fact]
    public void InternalClassOtherAssemblyWithGrantedVisibilityToInternalShouldMap()
    {
        var aSource = TestSourceBuilder.SyntaxTree(
            """
            [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

            namespace A;
            internal class A { public string PublicValue { get; set; } internal string InternalValue { get; set; } private string PrivateValue { get; set; } }
            """
        );

        using var aAssembly = TestHelper.BuildAssembly("A", aSource);

        var source = TestSourceBuilder.Mapping(
            "A.A",
            "B",
            "class B { public string PublicValue { get; set; } internal string InternalValue { get; set; } private string PrivateValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.Default, [aAssembly])
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.PublicValue = source.PublicValue;
                target.InternalValue = source.InternalValue;
                return target;
                """
            );
    }

    [Fact]
    public Task PrivateInTheSameClassShouldMap()
    {
        var source = TestSourceBuilder.CSharp(
            """
            public partial class A
            {
                private string _value;

                [Riok.Mapperly.Abstractions.Mapper(UseDeepCloning = true)]
                internal partial class Mapper
                {
                    public partial A Clone(A source);
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateFieldPublicClassOtherAssemblyShouldMap()
    {
        var aSource = TestSourceBuilder.SyntaxTree(
            """
            namespace A;
            public class A { private int Value; }
            """
        );

        using var aAssembly = TestHelper.BuildAssembly("A", aSource);

        var source = TestSourceBuilder.Mapping(
            "A.A",
            "B",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class B { public int Value; }"
        );

        return TestHelper.VerifyGenerator(source, additionalAssemblies: [aAssembly]);
    }

    [Fact]
    public Task PrivatePropertyPublicClassOtherAssemblyShouldMap()
    {
        var aSource = TestSourceBuilder.SyntaxTree(
            """
            namespace A;
            public class A
            {
                private int PrivateProp { get; set; }
                public int PrivateGetterProp { private get; set; }
                public int PrivateSetterProp { get; private set; }
            }
            """
        );

        using var aAssembly = TestHelper.BuildAssembly("A", aSource);

        var source = TestSourceBuilder.Mapping(
            "A.A",
            "B",
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            """
            class B
            {
                public int PrivateProp { get; set; }
                public int PrivateGetterProp { get; set; }
                public int PrivateSetterProp { get; set; }
            }
            """
        );

        return TestHelper.VerifyGenerator(source, additionalAssemblies: [aAssembly]);
    }

    [Fact]
    public Task PropertyInTheSameClassShouldIgnoreBackingField()
    {
        var source = TestSourceBuilder.CSharp(
            """
            public partial class A
            {
                public string Value { get; set; }

                [Riok.Mapperly.Abstractions.Mapper(UseDeepCloning = true)]
                internal partial class Mapper
                {
                    public partial A Clone(A source);
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
