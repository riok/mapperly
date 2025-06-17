using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests.Helpers;

public class MemberNamingUtilTest
{
    [Theory]
    [InlineData("OneTwoThree", "oneTwoThree")]
    [InlineData("oneTwoThree", "oneTwoThree")]
    [InlineData("one_two_three", "oneTwoThree")]
    [InlineData("ONE_TWO_THREE", "oneTwoThree")]
    [InlineData("OneTWOThree", "oneTwoThree")]
    [InlineData("One1Two12Three123", "one1Two12Three123")]
    [InlineData("One1TWO12Three123", "one1Two12Three123")]
    public void ToCamelCaseTest(string input, string expected) => input.ToCamelCase().ShouldBe(expected);

    [Theory]
    [InlineData("OneTwoThree", "OneTwoThree")]
    [InlineData("oneTwoThree", "OneTwoThree")]
    [InlineData("one_two_three", "OneTwoThree")]
    [InlineData("ONE_TWO_THREE", "OneTwoThree")]
    [InlineData("OneTWOThree", "OneTwoThree")]
    [InlineData("One1Two12Three123", "One1Two12Three123")]
    [InlineData("One1TWO12Three123", "One1Two12Three123")]
    public void ToPascalCaseTest(string input, string expected) => input.ToPascalCase().ShouldBe(expected);

    [Theory]
    [InlineData("OneTwoThree", "one_two_three")]
    [InlineData("oneTwoThree", "one_two_three")]
    [InlineData("one_two_three", "one_two_three")]
    [InlineData("ONE_TWO_THREE", "one_two_three")]
    [InlineData("OneTWOThree", "one_two_three")]
    [InlineData("One1Two12Three123", "one1_two12_three123")]
    [InlineData("One1TWO12Three123", "one1_two12_three123")]
    public void ToSnakeCaseTest(string input, string expected) => input.ToSnakeCase().ShouldBe(expected);

    [Theory]
    [InlineData("OneTwoThree", "ONE_TWO_THREE")]
    [InlineData("oneTwoThree", "ONE_TWO_THREE")]
    [InlineData("one_two_three", "ONE_TWO_THREE")]
    [InlineData("ONE_TWO_THREE", "ONE_TWO_THREE")]
    [InlineData("OneTWOThree", "ONE_TWO_THREE")]
    [InlineData("One1Two12Three123", "ONE1_TWO12_THREE123")]
    [InlineData("One1TWO12Three123", "ONE1_TWO12_THREE123")]
    public void ToUpperSnakeCaseTest(string input, string expected) => input.ToUpperSnakeCase().ShouldBe(expected);

    [Theory]
    [InlineData("OneTwoThree", "one-two-three")]
    [InlineData("oneTwoThree", "one-two-three")]
    [InlineData("one_two_three", "one-two-three")]
    [InlineData("ONE_TWO_THREE", "one-two-three")]
    [InlineData("OneTWOThree", "one-two-three")]
    [InlineData("One1Two12Three123", "one1-two12-three123")]
    [InlineData("One1TWO12Three123", "one1-two12-three123")]
    public void ToKebabCaseTest(string input, string expected) => input.ToKebabCase().ShouldBe(expected);

    [Theory]
    [InlineData("OneTwoThree", "ONE-TWO-THREE")]
    [InlineData("oneTwoThree", "ONE-TWO-THREE")]
    [InlineData("one_two_three", "ONE-TWO-THREE")]
    [InlineData("ONE_TWO_THREE", "ONE-TWO-THREE")]
    [InlineData("OneTWOThree", "ONE-TWO-THREE")]
    [InlineData("One1Two12Three123", "ONE1-TWO12-THREE123")]
    [InlineData("One1TWO12Three123", "ONE1-TWO12-THREE123")]
    public void ToUpperKebabCaseTest(string input, string expected) => input.ToUpperKebabCase().ShouldBe(expected);
}
