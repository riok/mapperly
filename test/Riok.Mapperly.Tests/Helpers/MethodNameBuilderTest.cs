using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class MethodNameBuilderTest
{
    [Fact]
    public void ShouldGenerateUniqueMethodNames()
    {
        var builder = new MethodNameBuilder();
        builder.Reserve("MapToA");
        builder.Build(NewMethodMappingMock("A")).ShouldBe("MapToA1");
        builder.Build(NewMethodMappingMock("A")).ShouldBe("MapToA2");
        builder.Build(NewMethodMappingMock("B")).ShouldBe("MapToB");
        builder.Build(NewMethodMappingMock("B")).ShouldBe("MapToB1");
    }

    private MethodMapping NewMethodMappingMock(string targetTypeName)
    {
        var targetTypeMock = Substitute.For<ITypeSymbol>();
        targetTypeMock.Name.Returns(targetTypeName);
        targetTypeMock.NullableAnnotation.Returns(NullableAnnotation.NotAnnotated);

        return new MockedMethodMapping(targetTypeMock);
    }

    private class MockedMethodMapping : MethodMapping
    {
        public MockedMethodMapping(ITypeSymbol t)
            : base(t, t) { }

        public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx) => [];
    }
}
