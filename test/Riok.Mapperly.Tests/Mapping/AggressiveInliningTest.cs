namespace Riok.Mapperly.Tests.Mapping;

public class AggressiveInliningTest
{
    [Fact]
    public Task DebugGeneratedCode()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper(AggressiveInliningTypes = AggressiveInliningTypes.ValueTypes)]
            public partial class MyMapper
            {
                public partial int ToInt(double value);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AllTypesShouldAddMethodImpl()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper(AggressiveInliningTypes = AggressiveInliningTypes.All)]
            public partial class MyMapper
            {
                public partial int ToInt(double value);
                public partial string MapToString(MyClass source);
            }

            public class MyClass { }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceTypesShouldAddMethodImpl()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper(AggressiveInliningTypes = AggressiveInliningTypes.ReferenceTypes)]
            public partial class MyMapper
            {
                public partial int ToInt(double value);
                public partial string MapToString(MyClass source);
            }

            public class MyClass { }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NoneShouldNotAddMethodImpl()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper(AggressiveInliningTypes = AggressiveInliningTypes.None)]
            public partial class MyMapper
            {
                public partial int ToInt(double value);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DefaultShouldNotAddMethodImpl()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class MyMapper
            {
                public partial int ToInt(double value);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ValueTypesShouldAddMethodImpl()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper(AggressiveInliningTypes = AggressiveInliningTypes.ValueTypes)]
            public partial class MyMapper
            {
                public partial int ToInt(double value);
                public partial long ToLong(int value);
                public partial string MapToString(MyClass source);
            }

            public class MyClass { }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
