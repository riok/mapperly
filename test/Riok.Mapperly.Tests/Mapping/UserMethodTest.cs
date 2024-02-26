using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class UserMethodTest
{
    [Fact]
    public Task WithNamespaceShouldWork()
    {
        var source = TestSourceBuilder.Mapping("int", "string", TestSourceBuilderOptions.Default with { Namespace = "MyCompany.MyMapper" });
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task InstanceMapperShouldSupportUserDefinedStaticMethods()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class MyMapper
            {
                public static int MapInt(int s) => s;

                public partial B Map(A s);
            }

            public record A(int Value);
            public record B(int Value);
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task InstanceMapperShouldUseStaticExistingTargetMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial B Map(A s);"
            static void MapList(List<int> src, List<string> dst) { }
            """,
            "class A { public List<int> Value { get; set; } }",
            "public class B { public List<string> Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithExistingInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A source, B target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("target.StringValue = source.StringValue;");
    }

    [Fact]
    public void WithExistingInstanceNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A? source, B? target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null || target == null)
                    return;
                target.StringValue = source.StringValue;
                """
            );
    }

    [Fact]
    public Task WithExistingInstanceDisabledNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map(A source, B target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public void WithExistingCollectionInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IEnumerable<A> source, ICollection<B> target)",
            "record A(string Value);",
            "record B(string Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                foreach (var item in source)
                {
                    target.Add(MapToB(item));
                }
                """
            );
    }

    [Fact]
    public void WithExistingDictionaryInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(IReadOnlyDictionary<string, A> source, IDictionary<string, B> target)",
            "record A(string Value);",
            "record B(string Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                foreach (var item in source)
                {
                    target[item.Key] = MapToB(item.Value);
                }
                """
            );
    }

    [Fact]
    public void WithMultipleUserDefinedMethodShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBody(
            """
            partial int ToInt(string source);
            partial int ToInt2(string source);
            """
        );

        TestHelper.GenerateMapper(source).Should().AllMethodsHaveBody("return int.Parse(source);");
    }

    [Fact]
    public void WithSameNamesShouldGenerateUniqueMethodNames()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B MapToB(A source);",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public B? Value { get; set; } }",
            "class B { public B? Value { get; set; } }"
        );

        TestHelper.GenerateMapper(source).Should().HaveOnlyMethods("MapToB", "MapToB1");
    }

    [Fact]
    public void InvalidSignatureReturnTypeAdditionalParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("partial string ToString(T source, string format);");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "ToString has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureAdditionalParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A a, B b, string format);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "Map has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureAdditionalParameterWithReferenceHandlingShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A a, B b, [ReferenceHandler] IReferenceHandler refHandler, string format);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureAsyncShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial async Task<B> Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void WithInvalidGenericSignatureShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("partial TTarget Map<TSource, TTarget, TOther>(TSource source);");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "Map has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task WithClassBaseTypeShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class BaseMapper : BaseMapper3
            {
                public string MyMapping(int value)
                    => $"my-to-string-{{value}}";

                protected partial short MyIntToShortMapping(int value);
            }

            public interface BaseMapper2 : BaseMapper3
            {
                long MyMapping2(int value)
                    => (long)value;
            }

            public interface BaseMapper3
            {
                decimal MyMapping3(int value)
                    => (decimal)value;
            }

            [Mapper]
            public partial class MyMapper : BaseMapper, BaseMapper2
            {
                public partial B Map(A source);
            }

            class A { public int Value { get; set; } public int Value2 { get; set; } public int Value3 { get; set; } public int Value4 { get; set; } }
            class B { public string Value { get; set; } public long Value2 { get; set; } public decimal Value3 { get; set; } public short Value4 { get; set; } }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExistingTargetsWithClassBaseTypeShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class BaseMapper : BaseMapper3
            {
                public void MyMapping(List<int> src, List<string> dst) { }

                protected partial void MyIntToShortMapping(List<int> src, List<short> dst);
            }

            public interface BaseMapper2 : BaseMapper3
            {
                void MyMapping2(int src, long dst);
            }

            public interface BaseMapper3
            {
                void MyMapping3(C src, D dst) { }
            }

            [Mapper]
            public partial class MyMapper : BaseMapper, BaseMapper2
            {
                public partial B Map(A source);
            }

            class A { public List<int> Value { get; set; } public int Value2 { get; set; } public C Value3 { get; set; } public List<int> Value4 { get; set; } }
            class B { public List<string> Value { get; } public long Value2 { get; } public D Value3 { get; } public List<short> Value4 { get; } },
            class C { public int Value { get; set; } },
            class D { public string Value { get; set; } },
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithMapperClassModifiersShouldCopyModifiersToMapper()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            internal sealed abstract partial class BaseMapper
            {
                public partial B AToB(A source);

                protected partial short IntToShort(int value);

                protected abstract string IntToString(int value);
            }

            class A { public int Value { get; set; } public int Value2 { get; set; } }
            class B { public string Value { get; set; } public short Value2 { get; set; } }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithMethodClassModifiersShouldCopyModifiersToMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class BaseMapper
            {
                public virtual partial B AToB(A source);

                public partial short IntToShort(int value);
            }

            [Mapper]
            public partial class BaseMapper2 : BaseMapper
            {
                public override partial B AToB(A source);
            }

            [Mapper]
            public partial class BaseMapper3 : BaseMapper
            {
                public sealed override partial B AToB(A source);

                public new partial short IntToShort(int value);
            }

            class A { public int Value { get; set; } public int Value2 { get; set; } }
            class B { public string Value { get; set; } public short Value2 { get; set; } }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithNullableGenericAndNullableDisabledTargetShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class Mapper
            {
                public partial B Map(A source);

                private ICollection<string?> MapValue(IEnumerable<C> source)
                    => source.Select(x => x.Name).ToList();
            }

            #nullable disable
            public class B { public ICollection<string> Value { get; set; } }
            #nullable restore

            public class A { public IEnumerable<C> Value { get; } }
            public record C(string Name);
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = MapValue(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void DisabledAutoUserMappingsShouldIgnoreUserImplemented()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static int NotAMappingMethod(int s) => s;
            public partial B Map(A s);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(s.Value);
                return target;
                """
            );
    }

    [Fact]
    public void DisabledAutoUserMappingWithExplicitIncludedMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static int NotAMappingMethod(int s) => s;
            [UserMapping(Default = true)]
            public static int AMappingMethod(int s) => s;
            public partial B Map(A s);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(AMappingMethod(s.Value));
                return target;
                """
            );
    }

    [Fact]
    public void DisabledAutoUserMappingWithMultipleExplicitIncludedMethodsOneDefault()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping]
            public static int NotAMappingMethod(int s) => s;
            [UserMapping(Default = true)]
            public static int AMappingMethod(int s) => s;
            public partial B Map(A s);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(AMappingMethod(s.Value));
                return target;
                """
            );
    }

    [Fact]
    public void DisabledAutoUserMappingWithExplicitIncludedButIgnoredMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static int NotAMappingMethod(int s) => s;
            [UserMapping(Ignore = true)]
            public static int NotAMappingMethod2(int s) => s;
            public partial B Map(A s);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(s.Value);
                return target;
                """
            );
    }

    [Fact]
    public void EnabledAutoUserMappingWithIgnoredMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Ignore = true)]
            public static int NotAMappingMethod(int s) => s;
            public partial B Map(A s);
            """,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(s.Value);
                return target;
                """
            );
    }

    [Fact]
    public Task UserMappingAttributeOnNonMappingMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping]
            public static void NotAMappingMethod2(int s) {}
            public partial B Map(A s);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DisabledAutoUserMappingsMultipleDefaultUserMappingsShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Default = true)]
            public static int IntMapping(int x) => x + 10;

            [UserMapping(Default = true)]
            public static int IntMapping2(int x) => x + 20;

            public partial B Map(A s);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MultipleDefaultUserMappingsShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Default = true)]
            public static int IntMapping(int x) => x + 10;

            [UserMapping(Default = true)]
            public static int IntMapping2(int x) => x + 20;

            public partial B Map(A s);
            """,
            "public record A(int Value);",
            "public record B(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ExplicitDefaultFalseUserMappingsShouldBeIgnored()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Default = false)]
            public static int IntMapping(int x) => x + 10;

            [UserMapping(Default = false)]
            public static int IntMapping2(int x) => x + 20;

            public partial B Map(A s);
            """,
            "public record A(int Value);",
            "public record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(s.Value);
                return target;
                """
            );
    }

    [Fact]
    public void UserImplementedWithDefaultFalseUserMappingsShouldUseFirstNonFalseDefault()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Default = false)]
            public static int IntMapping(int x) => x + 10;

            [UserMapping(Default = false)]
            public static int IntMapping2(int x) => x + 20;

            [UserMapping]
            public static int IntMapping3(int x) => x + 30;

            [UserMapping]
            public static int IntMapping4(int x) => x + 40;

            public partial B Map(A s);
            """,
            "public record A(int Value);",
            "public record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MultipleUserMappingsWithoutDefault,
                "Multiple user mappings discovered for the mapping from int to int without specifying an explicit default"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B(IntMapping3(s.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UserImplementedWithDefaultTrueAndFalseUserMappingsShouldUseDefaultTrue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Default = false)]
            public static int IntMapping(int x) => x;

            [UserMapping]
            public static int IntMapping2(int x) => x + 20;

            [UserMapping(Default = true)]
            public static int IntMapping3(int x) => x + 30;

            public partial B Map(A s);
            """,
            "public record A(int Value);",
            "public record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(IntMapping3(s.Value));
                return target;
                """
            );
    }

    [Fact]
    public void UnrelatedUserImplementedShouldNotReportDiagnostic()
    {
        // duplicated user mapping for int => int but
        // but not used (also no MapPropertyAttribute.Use reference)
        // therefore no RMG060 should be reported
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public int IntMapping(int x) => x;
            public int IntMapping2(int x) => x + 20;
            public partial B Map(A s);
            """,
            "public record A(string Value);",
            "public record B(string Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(s.Value);
                return target;
                """
            );
    }
}
