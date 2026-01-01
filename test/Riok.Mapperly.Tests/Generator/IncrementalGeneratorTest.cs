using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Tests.Generator.IncrementalGeneratorTestHelper;

namespace Riok.Mapperly.Tests.Generator;

public class IncrementalGeneratorTest
{
    [Fact]
    public void AddingUnrelatedTypeDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("string", "string");

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        var compilation2 = compilation1.AddSyntaxTrees(TestSourceBuilder.SyntaxTree("struct MyValue {}"));
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached);
    }

    [Fact]
    public void AddingNewMapperDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );

        var secondary = TestSourceBuilder.SyntaxTree(
            """
            using Riok.Mapperly.Abstractions;

            namespace Test.B
            {
                [Mapper]
                internal partial class BarFooMapper
                {
                    internal partial string BarToFoo(string value);
                }
            }
            """
        );

        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var compilation2 = compilation1.AddSyntaxTrees(secondary);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached, 0);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.New, 1);
    }

    [Fact]
    public void AppendingUnrelatedTypeDoesNotRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("string", "string");
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var newTree = syntaxTree.WithRootAndOptions(
            syntaxTree.GetCompilationUnitRoot().AddMembers(ParseMemberDeclaration("struct Foo {}")!),
            syntaxTree.Options
        );

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached);
    }

    [Fact]
    public void ModifyingMappedTypeDoesRegenerateOriginal()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "record A(int Value);", "record B(int Value);");
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        var compilation2 = ReplaceRecord(compilation1, "A", "record A(string Value)");
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedSource);
    }

    [Fact]
    public void ModifyingMapperDoesRegenerateOriginal()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        var classDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .Members.OfType<ClassDeclarationSyntax>()
            .Single(x => x.Identifier.Text == TestSourceBuilderOptions.DefaultMapperClassName);
        var member = ParseMemberDeclaration("internal partial int BarToBaz(int value);")!;
        var updatedClass = classDeclaration.AddMembers(member);

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(classDeclaration, updatedClass);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedSourceAndDiagnostics);
    }

    [Fact]
    public void ModifyingMapperDiagnosticsOnlyDoesRegenerateDiagnosticsOnly()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnoreSource(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }"
        );
        var syntaxTree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);

        var classDeclaration = syntaxTree
            .GetCompilationUnitRoot()
            .Members.OfType<ClassDeclarationSyntax>()
            .Single(x => x.Identifier.Text == TestSourceBuilderOptions.DefaultMapperClassName);
        var member = ParseMemberDeclaration("[MapperIgnoreSource(\"not_found_updated\")] partial B Map(A source);")!;
        var updatedClass = classDeclaration.WithMembers(new SyntaxList<MemberDeclarationSyntax>(member));

        var newRoot = syntaxTree.GetCompilationUnitRoot().ReplaceNode(classDeclaration, updatedClass);
        var newTree = syntaxTree.WithRootAndOptions(newRoot, syntaxTree.Options);

        var compilation2 = compilation1.ReplaceSyntaxTree(compilation1.SyntaxTrees.First(), newTree);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedDiagnostics);
    }

    [Fact]
    public void MapperDefaultsChangeShouldRegenerateAffectedMapper()
    {
        var mapperSource = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaults(EnumMappingStrategy = EnumMappingStrategy.ByValueCheckDefined)]

            [Mapper]
            public partial class AffectedMapper
            {
                public partial E2 Map(E1 source);
            }

            [Mapper]
            public partial class UnaffectedMapper
            {
                public partial B Map(A source);
            }

            record A(string Value);
            record B(string Value);
            enum E1 { value, value2 }
            enum E2 { Value }
            """
        );

        var syntaxTree1 = CSharpSyntaxTree.ParseText(mapperSource, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree1);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        var compilationUnit1 = (CompilationUnitSyntax)syntaxTree1.GetRoot();
        var attribute = Attribute(
            IdentifierName("MapperDefaults"),
            ParseAttributeArgumentList("(EnumMappingStrategy = EnumMappingStrategy.ByValue)")
        );
        var attributeList = AttributeList(SingletonSeparatedList(attribute));
        var compilationUnit2 = compilationUnit1.WithAttributeLists(new SyntaxList<AttributeListSyntax>(attributeList));
        var syntaxTree2 = syntaxTree1.WithRootAndOptions(compilationUnit2, syntaxTree1.Options);
        var compilation2 = compilation1.ReplaceSyntaxTree(syntaxTree1, syntaxTree2);
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Modified, 0);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.ModifiedDefaults, 1);
    }

    [Fact]
    public void AddingNewMapperDoesntRegenerateStaticMappers()
    {
        var mapperSource = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: UseStaticMapper(typeof(StaticMapper))]

            public static class StaticMapper
            {
                public static E2 Map(E1 source) => E2.Value;
            }

            [Mapper]
            public partial class Mapper
            {
                public partial B Map(A source);
            }

            record A(E1 Value);
            record B(E2 Value);
            enum E1 { value1, value2 }
            enum E2 { Value1, Value2 }
            """
        );

        var syntaxTree = CSharpSyntaxTree.ParseText(mapperSource, CSharpParseOptions.Default);
        var compilation1 = TestHelper.BuildCompilation(syntaxTree);

        var driver1 = TestHelper.GenerateTracked(compilation1);
        AssertRunReasons(driver1, IncrementalGeneratorRunReasons.New);

        var compilation2 = compilation1.AddSyntaxTrees(
            TestSourceBuilder.SyntaxTree(
                """
                using Riok.Mapperly.Abstractions;

                [Mapper]
                public partial class MyOtherMapper
                {
                    public partial B Map(A source);
                }
                """
            )
        );
        var driver2 = driver1.RunGenerators(compilation2);
        AssertRunReasons(driver2, IncrementalGeneratorRunReasons.Cached);
    }
}
