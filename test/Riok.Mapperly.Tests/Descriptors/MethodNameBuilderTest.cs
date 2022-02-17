using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Tests.Descriptors;

public class MethodNameBuilderTest
{
    [Fact]
    public void ShouldGenerateUniqueMethodNames()
    {
        var builder = new MethodNameBuilder();
        builder.Reserve("MapToA");
        builder.Build(NewMethodMappingMock("A"))
            .Should()
            .BeEquivalentTo("MapToA1");
        builder.Build(NewMethodMappingMock("A"))
            .Should()
            .BeEquivalentTo("MapToA2");
        builder.Build(NewMethodMappingMock("B"))
            .Should()
            .BeEquivalentTo("MapToB");
        builder.Build(NewMethodMappingMock("B"))
            .Should()
            .BeEquivalentTo("MapToB1");
    }

    private MethodMapping NewMethodMappingMock(string targetTypeName)
    {
        var targetTypeMock = new Mock<ITypeSymbol>();
        targetTypeMock.Setup(x => x.Name).Returns(targetTypeName);
        targetTypeMock.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);

        return new MockedMethodMapping(targetTypeMock.Object);
    }

    private class MockedMethodMapping : MethodMapping
    {
        public MockedMethodMapping(ITypeSymbol t) : base(t, t)
        {
        }

        public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
            => Enumerable.Empty<StatementSyntax>();
    }
}
