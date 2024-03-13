using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class InstantiableMapperWithStaticMethodsTest
{
    [Fact]
    public Task StaticPartialMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(int Value);
            record B(int Value);

            [Mapper]
            public partial class Mapper
            {
                static partial B Map(A source);
            }
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

            public static partial class CarFeature
            {
                public partial class Mappers
                {
                    [Mapper]
                    public partial class CarMapper
                    {
                        static partial int ToInt(double value);
                    }
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NestedMappingShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class CarMapper
            {
                static partial C MapToC(A value);
            }

            public record A(B Value1);
            public record B(int Value2);

            public record C(D Value1);
            public record D(int Value2);
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MappingWithUserMappingShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class CarMapper
            {
                static partial D MapToD(A value);

                static F MapToF(C value) => new F(value.Value3);
            }

            public record A(B Value1);
            public record B(C Value2);
            public record C(int Value3);

            public record D(E Value1);
            public record E(F Value2);
            public record F(int Value3);
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldUseSimpleObjectFactory()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class CarMapper
            {
                [ObjectFactory]
                static B CreateB() => new B();

                static partial B Map(A a);
            }

            class A { public string StringValue { get; set; } }
            class B { public string StringValue { get; set; } }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UseStaticGenericMapperStaticMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper {
                public static BExternal ToBExternal(AExternal source) => new BExternal();
            }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                static partial B Map(A source);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IgnoreInstanceMethodFromStaticMapper()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal(int ExternalValue);
            record BExternal(int ExternalValue);

            class OtherMapper {
                public BExternal ToBExternal(AExternal source) => new BExternal();
            }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                static partial B Map(A source);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void MixedStaticMethodWithPartialInstanceMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            static partial B Map(A source);
            partial string Map2(int source);
            """,
            "record A(int Value);",
            "record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.MixingStaticPartialWithInstanceMethod)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MixedStaticPartialMethodWithNonStaticFactory()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class CarMapper
            {
                [ObjectFactory]
                B CreateB() => new B();

                public static partial B Map(A a);
            }

            class A { public string StringValue { get; set; } }
            class B { public string StringValue { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidObjectFactorySignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MixedStaticMethodWithInstanceMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            static partial B Map(A source);
            private string Map2(int source);
            """,
            "record A(int Value);",
            "record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MixingStaticPartialWithInstanceMethod,
                "Mapper class Mapper contains 'static partial' methods. Use either only instance methods or only static methods."
            )
            .HaveAssertedAllDiagnostics();
    }
}
