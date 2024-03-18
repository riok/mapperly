using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Tests.Helpers;

public class InlineExpressionRewriterTest
{
    [Theory]
    [InlineData("source + \"fooBar\"")]
    [InlineData(
        "new string(source.Select(char.ToUpper).ToArray())",
        true,
        "new string(global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Select(source, char.ToUpper)))"
    )]
    [InlineData("typeof(Test).ToString()", true, "typeof(global::System.Type).ToString()")]
    [InlineData("FooBar(source)", true, "global::Test.FooBar(source)")]
    [InlineData("OtherTest.FooBar(source)", true, "global::OtherNamespace.OtherTest.FooBar(source)")]
    [InlineData("new List<TestRecord>().ToString()", true, "new List<global::TestRecord>().ToString()")]
    [InlineData("new TestRecord[2].ToString()", true, "new global::TestRecord[2].ToString()")]
    [InlineData("base.ToString()", false)] // CS0831
    [InlineData("(1,2).ToString()", false)] // CS8143
    [InlineData("((string?)source)?.ToString()!", false)] // CS8072
    [InlineData("source switch { _ => \"fooBar\" }", false)] // CS8514
    [InlineData("throw new Exception()", false)] // CS8188
    [InlineData(
        "(new TestRecord(10) with { Value = 20 }).ToString()",
        false,
        "(new global::TestRecord(10) with { Value = 20 }).ToString()"
    )] // CS8849
    [InlineData("new List<int>()[1..3].ToString()", false)] // CS8792
    [InlineData("((List<int>) [1,2,3]).ToString()", false)] // CS9175
    public void RewriteExpression(string expression, bool canBeInlined = true, string? inlinedExpression = null)
    {
        var (result, inlineOk) = Rewrite(
            $$"""
            using System;
            using System.Linq;
            using OtherNamespace;

            public class Test
            {
                public string MapExpression(string source)
                    => {{expression}};

                private static string FooBar(string s)
                    => s + "fooBar";
            }

            public record TestRecord(int Value);

            namespace OtherNamespace
            {
                public class OtherTest
                {
                    public static string FooBar(string s)
                        => s + "otherFooBar";
                }
            }
            """
        );
        inlineOk.Should().Be(canBeInlined);
        result.Should().Be(inlinedExpression ?? expression);
    }

    private (string Result, bool CanBeInlined) Rewrite([StringSyntax(StringSyntax.CSharp)] string source)
    {
        var compilation = TestHelper.BuildCompilation(source);
        var bodyNode = compilation
            .SyntaxTrees.Single()
            .GetRoot()
            .DescendantNodes()
            .OfType<ArrowExpressionClauseSyntax>()
            .Single(x => x.Parent is MethodDeclarationSyntax { Identifier.Text: "MapExpression" });
        var model = compilation.GetSemanticModel(bodyNode.SyntaxTree);
        var rewriter = new InlineExpressionRewriter(model, _ => null);
        var result = rewriter.Visit(bodyNode.Expression);
        return (result.ToFullString(), rewriter.CanBeInlined);
    }
}
