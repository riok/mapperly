using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Tests.Helpers;

public class GenericTypeCheckerTest
{
    [Fact]
    public void DirectTypeParameters()
    {
        var result = InferAndCheckTypes(
            "B Test<A, B>(A source)",
            """
            class Source;
            class Target;
            """
        );
        AssertSuccessResult(result, ("A", "Source"), ("B", "Target"));
    }

    [Fact]
    public void NestedInterfaceTypeParameters()
    {
        var result = InferAndCheckTypes(
            "IEnumerable<B> Test<A, B>(IEnumerable<A> source)",
            """
            class Source : IEnumerable<int>;
            class Target : IEnumerable<string>;
            """
        );
        AssertSuccessResult(result, ("A", "Int32"), ("B", "String"));
    }

    [Fact]
    public void AdditionalNonInferrableTypeParameter()
    {
        var result = InferAndCheckTypes(
            "B Test<A, B, C>(A source)",
            """
            class Source;
            class Target;
            """
        );
        result.Success.Should().BeFalse();
        result.FailedArgument.Should().BeNull();
    }

    [Fact]
    public void BindDifferentTypesToTheSameTypeParameter()
    {
        var result = InferAndCheckTypes(
            "void Test<A, B>(A source, B target, B target2)",
            """
            class Source;
            class Target;
            class Target2;
            """,
            buildParameterAndArguments: (m, c) =>

                [
                    (m.Parameters[0].Type, c.GetTypeByMetadataName("Source")!.WithNullableAnnotation(NullableAnnotation.NotAnnotated)),
                    (m.Parameters[1].Type, c.GetTypeByMetadataName("Target")!.WithNullableAnnotation(NullableAnnotation.NotAnnotated)),
                    (m.Parameters[2].Type, c.GetTypeByMetadataName("Target2")!.WithNullableAnnotation(NullableAnnotation.NotAnnotated)),
                ]
        );
        result.Success.Should().BeFalse();
        result.FailedIndex.Should().Be(2);
        result.FailedArgument.Should().NotBeNull();
        result.FailedArgument!.Name.Should().Be("Target2");
        result.FailedParameter.Should().NotBeNull();
        result.FailedParameter!.Name.Should().Be("B");
    }

    [Fact]
    public void NestedBaseTypeParameters()
    {
        var result = InferAndCheckTypes(
            "BaseClass Test<A>(A source)",
            """
            class Source;
            class Target : BaseClass;
            class BaseClass;
            """
        );
        AssertSuccessResult(result, ("A", "Source"));
    }

    [Fact]
    public void TwoParameters()
    {
        var result = InferAndCheckTypes(
            "void Test<A, B>(A source, B target)",
            """
            class Source;
            class Target;
            """
        );
        AssertSuccessResult(result, ("A", "Source"), ("B", "Target"));
    }

    [Fact]
    public void ArrayParameter()
    {
        var result = InferAndCheckTypes(
            "void Test<A, B>(A[] sources, B[] targets)",
            """
            class Source;
            class Target;
            """,
            (s, c) =>
                c.CreateArrayTypeSymbol(s, 1, NullableAnnotation.NotAnnotated).WithNullableAnnotation(NullableAnnotation.NotAnnotated),
            (s, c) => c.CreateArrayTypeSymbol(s, 1, NullableAnnotation.NotAnnotated).WithNullableAnnotation(NullableAnnotation.NotAnnotated)
        );
        AssertSuccessResult(result, ("A", "Source"), ("B", "Target"));
    }

    [Fact]
    public void ArrayElementTypeNullMismatch()
    {
        var result = InferAndCheckTypes(
            "void Test<A, B>(A[] sources, B[] targets) where A : notnull",
            """
            class Source;
            class Target;
            """,
            (s, c) => c.CreateArrayTypeSymbol(s, 1, NullableAnnotation.Annotated).WithNullableAnnotation(NullableAnnotation.NotAnnotated),
            (s, c) => c.CreateArrayTypeSymbol(s, 1, NullableAnnotation.NotAnnotated).WithNullableAnnotation(NullableAnnotation.NotAnnotated)
        );
        result.Success.Should().BeFalse();
        result.FailedIndex.Should().Be(0);
    }

    [Fact]
    public void NonArrayAtArrayParameter()
    {
        var result = InferAndCheckTypes(
            "IEnumerable<B> Test<A, B>(A[] source)",
            """
            class Source : IEnumerable<int>;
            class Target : IEnumerable<string>;
            """
        );
        AssertFailureResult(result, "Source");
    }

    [Fact]
    public void GenericParameter()
    {
        var result = InferAndCheckTypes(
            "void Test<A, B>(IEnumerable<A> sources, IReadOnlyCollection<B> targets)",
            """
            class Source;
            class Target;
            """,
            (s, c) =>
                c.CreateArrayTypeSymbol(s, 1, NullableAnnotation.NotAnnotated).WithNullableAnnotation(NullableAnnotation.NotAnnotated),
            (s, c) => c.CreateArrayTypeSymbol(s, 1, NullableAnnotation.NotAnnotated).WithNullableAnnotation(NullableAnnotation.NotAnnotated)
        );
        AssertSuccessResult(result, ("A", "Source"), ("B", "Target"));
    }

    [Theory]
    [InlineData(false, "new()", "class Source { private Source() {} }")]
    [InlineData(true, "new()", "class Source;")]
    [InlineData(false, "struct", "class Source;")]
    [InlineData(true, "struct", "struct Source;")]
    [InlineData(false, "class", "struct Source;")]
    [InlineData(true, "class", "class Source;")]
    [InlineData(false, "class", "class Source;", NullableAnnotation.Annotated)]
    [InlineData(true, "class?", "class Source;", NullableAnnotation.Annotated)]
    [InlineData(true, "class?", "class Source;", NullableAnnotation.None)]
    [InlineData(true, "class?", "class Source;")]
    [InlineData(true, "notnull", "class Source;")]
    [InlineData(true, "notnull", "struct Source;")]
    [InlineData(false, "notnull", "class Source;", NullableAnnotation.Annotated)]
    [InlineData(true, "notnull", "class Source;", NullableAnnotation.None)]
    [InlineData(true, "BaseClass", "class Source : BaseClass; class BaseClass;")]
    [InlineData(true, "BaseClass", "class Source : BaseClass; class BaseClass;", NullableAnnotation.None, NullableContextOptions.Disable)]
    [InlineData(false, "BaseClass", "class Source; class BaseClass;")]
    [InlineData(true, "BaseClass?", "class Source : BaseClass; class BaseClass;")]
    [InlineData(true, "BaseClass?", "class Source : BaseClass; class BaseClass;", NullableAnnotation.Annotated)]
    [InlineData(true, "BaseClass?", "class Source : BaseClass; class BaseClass;", NullableAnnotation.None)]
    [InlineData(true, "IBase", "class Source : IBase; interface IBase;")]
    [InlineData(true, "IBase", "class Source : IBase; interface IBase;", NullableAnnotation.None, NullableContextOptions.Disable)]
    [InlineData(false, "IBase", "class Source; interface IBase;")]
    [InlineData(true, "IBase?", "class Source : IBase; interface IBase;")]
    [InlineData(true, "IBase?", "class Source : IBase; interface IBase;", NullableAnnotation.Annotated)]
    [InlineData(true, "IBase?", "class Source : IBase; interface IBase;", NullableAnnotation.None)]
    [InlineData(true, "IDtoProvider<T>", "class Source : IDtoProvider<Source>; interface IDtoProvider<T2>;")]
    [InlineData(true, "IDtoProvider<T>?", "class Source : IDtoProvider<Source>; interface IDtoProvider<T2>;")]
    [InlineData(true, "IDtoProvider<T>?", "class Source : IDtoProvider<Source>; interface IDtoProvider<T2>;", NullableAnnotation.Annotated)]
    [InlineData(true, "IDtoProvider<T>", "class Source : IDtoProvider<Source>; interface IDtoProvider<T2>;", NullableAnnotation.None)]
    [InlineData(true, "IDtoProvider<T>?", "class Source : IDtoProvider<Source>; interface IDtoProvider<T2>;", NullableAnnotation.None)]
    [InlineData(
        true,
        "IDtoProvider<T>",
        "class Source : IDtoProvider<Source>; interface IDtoProvider<T2>;",
        NullableAnnotation.None,
        NullableContextOptions.Disable
    )]
    [InlineData(false, "IDtoProvider<T>", "class Source : IDtoProvider<Source?>; interface IDtoProvider<T2>;")]
    [InlineData(true, "IDtoProvider<T?>", "class Source : IDtoProvider<Source?>; interface IDtoProvider<T2>;")]
    [InlineData(true, "DtoProvider<T>", "class Source : DtoProvider<Source>; class DtoProvider<T2>;")]
    [InlineData(
        true,
        "DtoProvider<T>",
        "class Source : DtoProvider<Source>; class DtoProvider<T2>;",
        NullableAnnotation.None,
        NullableContextOptions.Disable
    )]
    [InlineData(false, "DtoProvider<T>", "class Source : DtoProvider<Source?>; class DtoProvider<T2>;")]
    [InlineData(true, "DtoProvider<T?>", "class Source : DtoProvider<Source?>; class DtoProvider<T2>;")]
    [InlineData(true, "DtoProvider<T>", "class Source : DtoProvider<Source>; class DtoProvider<T2>;", NullableAnnotation.None)]
    public void TypeConstraints(
        bool valid,
        [StringSyntax(StringSyntax.CSharp)] string typeConstraints,
        [StringSyntax(StringSyntax.CSharp)] string types,
        NullableAnnotation sourceNullableAnnotation = NullableAnnotation.NotAnnotated,
        NullableContextOptions nullableContextOptions = NullableContextOptions.Enable
    )
    {
        var result = InferAndCheckTypes(
            $"Target Test<T>(T source) where T : {typeConstraints}",
            $$"""
            {{types}}
            class Target;
            """,
            (x, _) => x.WithNullableAnnotation(sourceNullableAnnotation),
            nullableOptions: nullableContextOptions
        );

        if (valid)
        {
            AssertSuccessResult(result, ("T", "Source"));
        }
        else
        {
            AssertFailureResult(result, "Source");
        }
    }

    private static GenericTypeChecker.GenericTypeCheckerResult InferAndCheckTypes(
        [StringSyntax(StringSyntax.CSharp)] string methodSignature,
        [StringSyntax(StringSyntax.CSharp)] string types,
        Func<INamedTypeSymbol, Compilation, ITypeSymbol>? sourceTypeModifier = null,
        Func<INamedTypeSymbol, Compilation, ITypeSymbol>? targetTypeModifier = null,
        Func<IMethodSymbol, Compilation, (ITypeSymbol Parameter, ITypeSymbol Argument)[]>? buildParameterAndArguments = null,
        NullableContextOptions nullableOptions = NullableContextOptions.Enable
    )
    {
        sourceTypeModifier ??= (x, _) => x.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        targetTypeModifier ??= (x, _) => x.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

        var source = TestSourceBuilder.CSharp(
            $$"""
            using System;
            using System.Linq;
            using System.Collections.Generic;

            public class Mapper
            {
                {{methodSignature}}
                    => throw new Exception();
            }

            {{types}}
            """
        );
        var compilation = TestHelper.BuildCompilation(source, TestHelperOptions.Default with { NullableOption = nullableOptions, });
        var nodes = compilation.SyntaxTrees.Single().GetRoot().DescendantNodes();
        var classNode = nodes.OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.Text == "Mapper");
        var methodNode = nodes.OfType<MethodDeclarationSyntax>().Single(x => x.Identifier.Text == "Test");
        var model = compilation.GetSemanticModel(classNode.SyntaxTree);
        var mapperSymbol = model.GetDeclaredSymbol(classNode) ?? throw new NullReferenceException();
        var compilationContext = new CompilationContext(compilation, new WellKnownTypes(compilation), new FileNameBuilder());
        var symbolAccessor = new SymbolAccessor(compilationContext, mapperSymbol);
        var typeChecker = new GenericTypeChecker(symbolAccessor, compilationContext.Types);

        var methodSymbol = model.GetDeclaredSymbol(methodNode) ?? throw new NullReferenceException();
        var parametersAndArguments = buildParameterAndArguments?.Invoke(methodSymbol, compilation);
        if (parametersAndArguments == null)
        {
            var sourceSymbol = sourceTypeModifier(
                compilation.GetTypeByMetadataName("Source") ?? throw new NullReferenceException(),
                compilation
            );
            var targetSymbol = targetTypeModifier(
                compilation.GetTypeByMetadataName("Target") ?? throw new NullReferenceException(),
                compilation
            );

            var targetMethodTypeSymbol = methodSymbol.ReturnsVoid ? methodSymbol.Parameters[1].Type : methodSymbol.ReturnType;
            parametersAndArguments = [(methodSymbol.Parameters[0].Type, sourceSymbol), (targetMethodTypeSymbol, targetSymbol),];
        }

        return typeChecker.InferAndCheckTypes(methodSymbol.TypeParameters, parametersAndArguments);
    }

    private static void AssertFailureResult(GenericTypeChecker.GenericTypeCheckerResult result, string failedArgumentName)
    {
        result.Success.Should().BeFalse();
        result.FailedArgument.Should().NotBeNull();
        result.FailedArgument!.Name.Should().Be(failedArgumentName);
    }

    private static void AssertSuccessResult(
        GenericTypeChecker.GenericTypeCheckerResult result,
        params (string TypeParameterName, string TypeName)[] inferredTypeNames
    )
    {
        result.Success.Should().BeTrue();
        result.FailedArgument.Should().BeNull();

        var inferredTypeNamesDict = inferredTypeNames.ToDictionary(x => x.TypeParameterName, x => x.TypeName);

        foreach (var (typeParameter, inferredType) in result.InferredTypes)
        {
            inferredTypeNamesDict.Remove(typeParameter.Name, out var typeName).Should().BeTrue();
            inferredType.Name.Should().Be(typeName);
        }
    }
}
