namespace Riok.Mapperly.Tests.Mapping;

public class AggressiveInliningTest
{
    [Fact]
    public Task MapperAttributeWithDefaultsShouldGenerateMethodImpleAttribute()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class MyMapper
            {
                public partial B Map(A a);
            }

            public record A(InnerA Value);
            public record InnerA(int Value);
            public record B(InnerB Value);
            public record InnerB(int Value);
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DisabledAggressiveInliningShouldNotGenerateMethodImpleAttribute()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper(EnableAggressiveInlining = false)]
            public partial class MyMapper
            {
                public partial B Map(A a);
            }

            public record A(InnerA Value);
            public record InnerA(int Value);
            public record B(InnerB Value);
            public record InnerB(int Value);
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AlreadyHavingMethodImplShouldNotGenerateMethodImpleAttributeForOnlyThatMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;
            using System.Runtime.CompilerServices;

            [Mapper(EnableAggressiveInlining = true)]
            public partial class MyMapper
            {
                [MethodImpl]
                public partial B Map(A a);
            }

            public record A(InnerA Value);
            public record InnerA(int Value);
            public record B(InnerB Value);
            public record InnerB(int Value);
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task EnabledAggressiveInliningShouldGenerateMethodImpleAttributeForUnsafeAccessors()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;
            using System.Runtime.CompilerServices;

            [Mapper(IncludedMembers = MemberVisibility.All, IncludedConstructors = MemberVisibility.All)]
            public partial class MyMapper
            {
                public partial B Map(A a);
            }

            class A
            {
                private A() { }
                private int value;
                private int _value { get; set; }
                public int Value { private get; set; }

            }
            class B
            {
                private B() { }
                private int value;
                private int _value { get; set; }
                public int Value { get; private set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DisabledAggressiveInliningShouldNotGenerateMethodImpleAttributeForUnsafeAccessors()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;
            using System.Runtime.CompilerServices;

            [Mapper(EnableAggressiveInlining = false, IncludedMembers = MemberVisibility.All, IncludedConstructors = MemberVisibility.All)]
            public partial class MyMapper
            {
                public partial B Map(A a);
            }

            class A
            {
                private A() { }
                private int value;
                private int _value { get; set; }
                public int Value { private get; set; }

            }
            class B
            {
                private B() { }
                private int value;
                private int _value { get; set; }
                public int Value { get; private set; }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }
}
