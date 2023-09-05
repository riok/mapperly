namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class UseStaticMapperTest
{
    [Fact]
    public void UseStaticGenericMapperStaticMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(global::OtherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseStaticTypeOfMapperStaticMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper(typeof(OtherMapper))]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(global::OtherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseStaticGenericMapperStaticMethodInStaticMapper()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            static class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(global::OtherMapper.ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public Task ReferenceHandling()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source, [ReferenceHandler] IReferenceHandler refHandler) => new BExternal(); }

            [Mapper(UseReferenceHandling = true)]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceHandlingEnabledNoParameter()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper(UseReferenceHandling = true)]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void IgnoreInstanceMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void IgnorePrivateMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { private BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UseGeneratedMapper()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            [Mapper]
            partial class OtherMapper { public partial BExternal ToBExternal(AExternal source); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void IgnoreInvalidSignature()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            public class OtherMapper { public void NotAMappingMethod(AExternal source) {} }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void PreferInternalMapping()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);

                private partial BExternal MapInternal(AExternal source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapInternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void PreferInternalImplementedMapping()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);

                private BExternal MapInternal(AExternal source) = new BExternal();
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(MapInternal(source.Value));
                return target;
                """
            );
    }
}
