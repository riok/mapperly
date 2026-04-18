using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

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
    public Task InstanceMapperShouldUseStaticExistingTargetMethodWithRefKeyword()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(A.Value), Use = nameof(MapList))]
            public static partial void Update([MappingTarget] this A dest, A src);"
            static void MapList([MappingTarget] this ref int[] dest, int[] src) { }
            """,
            "public class A { public int[] Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task InstanceMapperShouldUseStaticExistingTargetMethodWithRefKeywordAndSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(A.Value), Use = nameof(MapList))]
            [MapDerivedType<B, B>]
            [MapDerivedType<C, C>]
            public static partial void Update([MappingTarget] this A dest, A src);"
            static void MapList([MappingTarget] this ref int[] dest, int[] src) { }
            """,
            "public abstract class A { public int[] Value { get; set; } }",
            "public class B : A { public int BValue { get; set; } }",
            "public class C : A { public int CValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ShouldAutoUseVoidMethodWithRefTargetParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial void Update([MappingTarget] B target, A source);
            private static void MapOptional(Optional<string> src, ref string target)
            {
                if (src.HasValue)
                    target = src.Value;
            }
            """,
            "public class A { public Optional<string> Name { get; set; } }",
            "public class B { public string Name { get; set; } }",
            """
            public struct Optional<T>
            {
                public Optional(T value) { Value = value; HasValue = true; }
                public bool HasValue { get; }
                public T Value { get; }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "Update",
                """
                var targetRef = target.Name;
                MapOptional(source.Name, ref targetRef);
                target.Name = targetRef;
                """
            );
    }

    [Fact]
    public void ShouldPreferNewInstanceMappingOverRefMappingForPropertyMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial void Update([MappingTarget] B target, A source);
            private static string FromOptional(Optional<string> src) => src.HasValue ? src.Value : "";
            private static void MapOptional(Optional<string> src, ref string target)
            {
                if (src.HasValue)
                    target = src.Value;
            }
            """,
            "public class A { public Optional<string> Name { get; set; } }",
            "public class B { public string Name { get; set; } }",
            """
            public struct Optional<T>
            {
                public Optional(T value) { Value = value; HasValue = true; }
                public bool HasValue { get; }
                public T Value { get; }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "Update",
                """
                target.Name = FromOptional(source.Name);
                """
            );
    }

    [Fact]
    public void ShouldPreferExplicitDefaultRefMappingOverNewInstanceMappingForPropertyMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial void Update([MappingTarget] B target, A source);
            private static string FromOptional(Optional<string> src) => src.HasValue ? src.Value : "";
            [UserMapping(Default = true)]
            private static void MapOptional(Optional<string> src, ref string target)
            {
                if (src.HasValue)
                    target = src.Value;
            }
            """,
            "public class A { public Optional<string> Name { get; set; } }",
            "public class B { public string Name { get; set; } }",
            """
            public struct Optional<T>
            {
                public Optional(T value) { Value = value; HasValue = true; }
                public bool HasValue { get; }
                public T Value { get; }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMethodBody(
                "Update",
                """
                var targetRef = target.Name;
                MapOptional(source.Name, ref targetRef);
                target.Name = targetRef;
                """
            );
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
    public void WithMultipleUserDefinedMethodsShouldNotDiagnostic()
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

                protected partial double MyIntToDoubleMapping(int value);
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
            class B { public string Value { get; set; } public long Value2 { get; set; } public decimal Value3 { get; set; } public double Value4 { get; set; } }
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

                protected partial void MyIntToDoubleMapping(List<int> src, List<double> dst);
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
            class B { public List<string> Value { get; } public long Value2 { get; } public D Value3 { get; } public List<double> Value4 { get; } },
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

                protected partial double IntToDouble(int value);

                protected abstract string IntToString(int value);
            }

            class A { public int Value { get; set; } public int Value2 { get; set; } }
            class B { public string Value { get; set; } public double Value2 { get; set; } }
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

                [UserMapping(Default = true)]
                public partial double IntToDouble(int value);
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

                public new partial double IntToDouble(int value);
            }

            class A { public int Value { get; set; } public int Value2 { get; set; } }
            class B { public string Value { get; set; } public double Value2 { get; set; } }
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

            public static int IntMapping3(int x) => x + 30;

            public static int IntMapping4(int x) => x + 40;

            public partial B Map(A s);
            """,
            "public record A(int Value);",
            "public record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
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
    public void DisabledAutoUserMappingDiscoveryShouldUseFirstNonFalseDefault()
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
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "public record A(int Value);",
            "public record B(int Value);"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
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

    [Fact]
    public Task ShouldDiscoverBaseClassPartialMappingMethodsWithDisabledAutoUserMappings()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithBaseClass("BaseMapper") with
            {
                AutoUserMappings = false,
            },
            """
            [Mapper]
            public class BaseMapper
            {
                public partial D BaseMapping(C source);
            }
            """,
            "record A(C NestedValue);",
            "record B(D NestedValue);",
            "record C(int Value);",
            "record D(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldNotDiscoverInterfaceMappingMethodsWithDisabledAutoUserMappings()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithBaseClass("IBaseMapper") with
            {
                AutoUserMappings = false,
            },
            """
            public interface IBaseMapper
            {
                public static D BaseMapping(C source)
                    => new D(source.Value + 10);
            }
            """,
            "record A(C NestedValue);",
            "record B(D NestedValue);",
            "record C(int Value);",
            "record D(int Value);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void MappingMethodWithSingleTargetParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes("public partial B Map([MappingTarget] A source);", "record A;", "record b;");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "Map has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void WithGenericSourceTypeConstraintAndUserImplemented()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial To Map(From source);

            public OtherValue PickMe<TSource>(TSource source)
                where TSource : IValue => new OtherValue { Value = source.Value };
            """,
            "public interface IValue { public string Value { get; } }",
            "public class OtherValue { public string Value { get; set; } }",
            "public class A : IValue { public string Value { get; set; } }",
            "public class B : IValue { public string Value { get; set; } }",
            "public class From { public A AV { get; set; } public B BV { get; set; } }",
            "public class To { public OtherValue AV { get; set; } public OtherValue BV { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::To();
                target.AV = PickMe<global::A>(source.AV);
                target.BV = PickMe<global::B>(source.BV);
                return target;
                """
            );
    }

    [Fact]
    public void WithGenericSourceAndTargetTypeAndUserImplemented()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial DocumentDto Map(Document source);

            private UserDto MapUser(User source) => new UserDto { Id = source.Id };

            private Optional<TTarget> MapOptional<TSource, TTarget>(Optional<TSource> source)
                where TSource : notnull
                where TTarget : notnull
                => new Optional<TTarget> { Value = MapUser(source.Value) };
            """,
            "public class Optional<T> where T : notnull { public T? Value { get; set; } }",
            "public class User { public string Id { get; set; } }",
            "public class UserDto { public string Id { get; set; } }",
            "public class Document { public Optional<User> ModifiedBy { get; set; } }",
            "public class DocumentDto { public Optional<UserDto> ModifiedBy { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::DocumentDto();
                target.ModifiedBy = MapOptional<global::User, global::UserDto>(source.ModifiedBy);
                return target;
                """
            );
    }

    [Fact]
    public Task WithGenericUserImplementedOptionalSample()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial DocumentDto MapDocument(Document source);

            private Optional<TTarget> MapOptional<TSource, TTarget>(Optional<TSource> source)
                where TSource : notnull
                where TTarget : notnull
                => source.HasValue
                    ? Optional.Of(Map(source.Value))
                    : Optional.Empty();

            private partial TTarget Map<TSource, TTarget>(TSource source)
                where TSource : notnull
                where TTarget : notnull;

            private partial UserDto MapUser(User source);
            """,
            "public record Document(string Title, User CreatedBy, Optional<User> ModifiedBy);",
            "public record DocumentDto(string Title, UserDto CreatedBy, Optional<UserDto> ModifiedBy);",
            "public record User(string Name);",
            "public record UserDto(string Name);",
            "public readonly record struct EmptyOptional;",
            "public static class Optional { public static Optional<T> Of<T>(T value) where T : notnull => new(value); public static EmptyOptional Empty() => default; }",
            "public class Optional<T> where T : notnull { public Optional() {} public Optional(T value) { HasValue = true; Value = value; } public bool HasValue { get; } public T Value { get; } public static implicit operator Optional<T>(EmptyOptional _) => new(); }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithGenericSourceTypeConstraintsAndUserImplementedGenericAndNonGeneric()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial To Map(From source);

            public OtherValue PickMeForAAndB<TSource>(TSource source)
                where TSource : IValue => new OtherValue { Value = source.Value };

            public OtherValue PickMeForIValue(IValue source) => new OtherValue { Value = source.Value };
            """,
            "public interface IValue { public string Value { get; } }",
            "public class OtherValue { public string Value { get; set; } }",
            "public class A : IValue { public string Value { get; set; } }",
            "public class B : IValue { public string Value { get; set; } }",
            "public class From { public A AV { get; set; } public B BV { get; set; } public IValue IV { get; set; } }",
            "public class To { public OtherValue AV { get; set; } public OtherValue BV { get; set; } public OtherValue IV { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::To();
                target.AV = PickMeForAAndB<global::A>(source.AV);
                target.BV = PickMeForAAndB<global::B>(source.BV);
                target.IV = PickMeForIValue(source.IV);
                return target;
                """
            );
    }

    [Fact]
    public void WithGenericSourceAndTargetTypeParametersAndUserImplemented()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial ADto Map(A source);

            private TTarget MapGeneric<TSource, TTarget>(TSource source)
                where TSource : IMyInterface
                where TTarget : new(), IMyOtherInterface
                => new TTarget { MyValue = source.MyValue };
            """,
            "public interface IMyInterface { public string MyValue { get; } }",
            "public interface IMyOtherInterface { public string MyValue { get; set; } }",
            "public class B : IMyInterface { public string MyValue { get; set; } }",
            "public class BDto : IMyOtherInterface { public string MyValue { get; set; } }",
            "public class A { public B Nested { get; set; } }",
            "public class ADto { public BDto Nested { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::ADto();
                target.Nested = MapGeneric<global::B, global::BDto>(source.Nested);
                return target;
                """
            );
    }

    [Fact]
    public Task WithGenericUserImplementedAndReferenceHandling()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial B Map(A a);

            private Optional<TTarget> MapOptional<TSource, TTarget>(Optional<TSource> source)
                where TSource : notnull
                where TTarget : notnull
                => new Optional<TTarget>();
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public Optional<C> Value { get; set; } }",
            "class B { public B Parent { get; set; } public Optional<D> Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }",
            "class Optional<T> where T : notnull { public T Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithGenericUserImplementedWithReferenceHandlerParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial B Map(A a);

            private Optional<TTarget> MapOptional<TSource, TTarget>(Optional<TSource> source, [ReferenceHandler] IReferenceHandler refHandler)
                where TSource : notnull
                where TTarget : notnull
                => new Optional<TTarget>();
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public Optional<C> Value { get; set; } }",
            "class B { public B Parent { get; set; } public Optional<D> Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }",
            "class Optional<T> where T : notnull { public T Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithGenericUserImplementedAndAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial ADto Map(A source, int ctx);

            private TTarget MapGeneric<TSource, TTarget>(TSource source)
                where TSource : IEntity
                where TTarget : new(), IDto
                => new TTarget { Id = source.Id };
            """,
            "public interface IEntity { public int Id { get; } }",
            "public interface IDto { public int Id { get; set; } }",
            "public class B : IEntity { public int Id { get; set; } }",
            "public class BDto : IDto { public int Id { get; set; } }",
            "public class A { public B Nested { get; set; } }",
            "public class ADto { public BDto Nested { get; set; } public int Ctx { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::ADto();
                target.Nested = MapGeneric<global::B, global::BDto>(source.Nested);
                target.Ctx = ctx;
                return target;
                """
            );
    }

    [Fact]
    public Task WithGenericUserImplementedWithReferenceHandlerParameterAndRecursiveType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial B Map(A a);

            private Wrapper<TTarget> MapWrapper<TSource, TTarget>(Wrapper<TSource> source, [ReferenceHandler] IReferenceHandler refHandler)
                where TSource : notnull
                where TTarget : notnull
                => new Wrapper<TTarget>();
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public Wrapper<C> Value { get; set; } }",
            "class B { public B Parent { get; set; } public Wrapper<D> Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }",
            "class Wrapper<T> where T : notnull { public T Inner { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithGenericUserImplementedExistingTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial ADto Map(A source);

            private void MapGeneric<TSource, TTarget>([MappingTarget] TTarget target, TSource source)
                where TSource : IEntity
                where TTarget : IDto
            {
                target.Id = source.Id;
            }
            """,
            "public interface IEntity { public int Id { get; } public string Name { get; } }",
            "public interface IDto { public int Id { get; set; } public string Name { get; set; } }",
            "public class B : IEntity { public int Id { get; set; } public string Name { get; set; } }",
            "public class BDto : IDto { public int Id { get; set; } public string Name { get; set; } }",
            "public class A { public B Nested { get; set; } }",
            "public class ADto { public BDto Nested { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::ADto();
                target.Nested = MapToBDto(source.Nested);
                return target;
                """
            );
    }

    [Fact]
    public Task WithGenericUserImplementedExistingTargetAndReferenceHandling()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial B Map(A a);

            private void MapGeneric<TSource, TTarget>([MappingTarget] TTarget target, TSource source, [ReferenceHandler] IReferenceHandler refHandler)
                where TSource : IEntity
                where TTarget : IDto
            {
                target.Id = source.Id;
            }
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "interface IEntity { public int Id { get; } }",
            "interface IDto { public int Id { get; set; } }",
            "class C : IEntity { public int Id { get; set; } public C Parent { get; set; } }",
            "class D : IDto { public int Id { get; set; } public D Parent { get; set; } }",
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithGenericUserImplementedMultipleTypeParametersPartialMatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial OuterDto Map(Outer source);

            private Pair<TTarget1, TTarget2> MapPair<TSource1, TSource2, TTarget1, TTarget2>(Pair<TSource1, TSource2> source)
                where TSource1 : notnull
                where TSource2 : notnull
                where TTarget1 : notnull
                where TTarget2 : notnull
                => new Pair<TTarget1, TTarget2>();
            """,
            "class Pair<T1, T2> where T1 : notnull where T2 : notnull { public T1 Left { get; set; } public T2 Right { get; set; } }",
            "class X { public string Value { get; set; } }",
            "class XDto { public string Value { get; set; } }",
            "class Y { public int Value { get; set; } }",
            "class YDto { public int Value { get; set; } }",
            "class Outer { public Pair<X, Y> Items { get; set; } }",
            "class OuterDto { public Pair<XDto, YDto> Items { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::OuterDto();
                target.Items = MapPair<global::X, global::Y, global::XDto, global::YDto>(source.Items);
                return target;
                """
            );
    }

    [Fact]
    public void WithGenericUserImplementedOnStaticMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial ADto Map(A source);

            private static TTarget MapGeneric<TSource, TTarget>(TSource source)
                where TSource : IEntity
                where TTarget : new(), IDto
                => new TTarget { Id = source.Id };
            """,
            TestSourceBuilderOptions.AsStatic,
            "public interface IEntity { public int Id { get; } }",
            "public interface IDto { public int Id { get; set; } }",
            "public class B : IEntity { public int Id { get; set; } }",
            "public class BDto : IDto { public int Id { get; set; } }",
            "public class A { public B Nested { get; set; } }",
            "public class ADto { public BDto Nested { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::ADto();
                target.Nested = MapGeneric<global::B, global::BDto>(source.Nested);
                return target;
                """
            );
    }

    [Fact]
    public void WithGenericUserImplementedConstraintMismatchShouldNotMatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial ADto Map(A source);

            private TTarget MapGeneric<TSource, TTarget>(TSource source)
                where TSource : ISpecialEntity
                where TTarget : new(), IDto
                => new TTarget { Id = source.Id };
            """,
            "public interface ISpecialEntity { public int Id { get; } }",
            "public interface IDto { public int Id { get; set; } }",
            "public class B { public int Id { get; set; } }",
            "public class BDto : IDto { public int Id { get; set; } }",
            "public class A { public B Nested { get; set; } }",
            "public class ADto { public BDto Nested { get; set; } }"
        );

        // B does not implement ISpecialEntity, so the generic method should NOT match.
        // Mapperly should fall back to generating a regular mapping.
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::ADto();
                target.Nested = MapToBDto(source.Nested);
                return target;
                """
            );
    }
}
