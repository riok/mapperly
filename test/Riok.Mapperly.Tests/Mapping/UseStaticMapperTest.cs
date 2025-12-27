using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

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
                private partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);
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
                private partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);
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

    private MapperGenerationResultAssertions ExecuteStaticGenericMapperStaticMethodFromAnotherAssemblyCompilation(
        bool asCompilationReference
    )
    {
        var testDependencySource = TestSourceBuilder.SyntaxTree(
            """
            using System;
            using Riok.Mapperly.Abstractions;

            namespace Riok.Mapperly.TestDependency.Mapper
            {
                [Mapper]
                public static partial class DateTimeMapper
                {
                    public static DateTimeOffset MapToDateTimeOffset(DateTime dateTime) => new(dateTime, TimeSpan.Zero);
                }
            }
            """
        );

        using var testDependencyAssembly = TestHelper.BuildAssembly(
            "Riok.Mapperly.TestDependency",
            asCompilationReference,
            testDependencySource
        );

        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Linq;
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.TestDependency.Mapper;

            [Mapper]
            [UseStaticMapper(typeof(DateTimeMapper))]
            public static partial class Mapper
            {
                public static partial IQueryable<Target> ProjectToTarget(IQueryable<Source> source);

                public static partial Target MapToTarget(Source source);

                public class Source
                {
                    public DateTime DateTime { get; set; }
                }

                public class Target
                {
                    public DateTimeOffset DateTime { get; set; }
                }
            }
            """
        );

        return TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics, additionalAssemblies: [testDependencyAssembly])
            .Should();
    }

    /// <summary>
    /// This tests a situation when your IDE runs the source generator (references are other syntax trees)
    /// </summary>
    [Fact]
    public void UseStaticGenericMapperStaticMethodFromAnotherAssemblyAsReference()
    {
        var result = ExecuteStaticGenericMapperStaticMethodFromAnotherAssemblyCompilation(asCompilationReference: true);

        result.HaveMethodBody(
            "ProjectToTarget",
            """
            #nullable disable
                    return global::System.Linq.Queryable.Select(
                        source,
                        x => new global::Mapper.Target()
                        {
                            DateTime = new global::System.DateTimeOffset(x.DateTime, global::System.TimeSpan.Zero),
                        }
                    );
            #nullable enable
            """
        );
    }

    /// <summary>
    /// This tests a situation when compiler produces final assembly (references are compiled assemblies)
    /// </summary>
    [Fact]
    public void UseStaticGenericMapperStaticMethodFromAnotherAssemblyAsCompiledAssembly()
    {
        var result = ExecuteStaticGenericMapperStaticMethodFromAnotherAssemblyCompilation(asCompilationReference: false);

        result
            .HaveDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingCannotInline)
            .HaveMethodBody(
                "ProjectToTarget",
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            source,
                            x => new global::Mapper.Target()
                            {
                                DateTime = global::Riok.Mapperly.TestDependency.Mapper.DateTimeMapper.MapToDateTimeOffset(x.DateTime),
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public void ExternalMappingDoesNotAffectOnUseStaticMapper()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            record A(int Value);
            record B(int Value);

            class OtherMapper { public static int AutoMap(int source) => source + 1; }
            class ExternalMapper { public static int ExplicitMap(int source) => source + 2; }

            [Mapper]
            [UseStaticMapper<OtherMapper>]
            public partial class Mapper
            {
                partial B Map(A source);

                [MapProperty("Value", "Value", Use = nameof(@ExternalMapper.ExplicitMap)]
                partial B MapOther(A source);
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(global::OtherMapper.AutoMap(source.Value));
                return target;
                """
            )
            .HaveMethodBody(
                "MapOther",
                """
                var target = new global::B(global::ExternalMapper.ExplicitMap(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public void AssemblyLevelUseStaticGenericMapperStaticMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            [assembly:UseStaticMapper<OtherMapper>]

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            class OtherMapper { public static BExternal ToBExternal(AExternal source) => new BExternal(); }

            [Mapper]
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
}
