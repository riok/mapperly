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
        bool asCompilationReference,
        bool noExpressionInlining = false
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
            $$"""
            using System;
            using System.Linq;
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.TestDependency.Mapper;

            [Mapper]
            [UseStaticMapper(typeof(DateTimeMapper){{(noExpressionInlining ? ", NoExpressionInlining = true" : "")}})]
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UseStaticGenericMapperStaticMethodFromAnotherAssemblyWithNoExpressionInlining(bool asCompilationReference)
    {
        var result = ExecuteStaticGenericMapperStaticMethodFromAnotherAssemblyCompilation(
            asCompilationReference,
            noExpressionInlining: true
        );

        result
            .HaveAssertedAllDiagnostics()
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
    public void ProjectionWithAssemblyLevelUseStaticMapperNoExpressionInliningShouldInvoke()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            [assembly: UseStaticMapper(typeof(OtherMapper), NoExpressionInlining = true)]

            class A { public AExternal Value { get; set; } }
            class B { public string Value { get; set; } }
            class AExternal { public string Name { get; set; } }

            static class OtherMapper
            {
                public static string ToName(AExternal s) => s.Name;
            }

            // non-public included members remap the assembly-level static mappers to a new compilation
            [Mapper(IncludedMembers = MemberVisibility.All)]
            public static partial class Mapper
            {
                public static partial IQueryable<B> Map(IQueryable<A> q);
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Value = global::OtherMapper.ToName(x.Value),
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public void AssemblyLevelUseStaticGenericMapperStaticMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            [assembly: UseStaticMapper<OtherMapper>]

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

    [Fact]
    public void AssemblyLevelUseStaticMapperStaticMethod()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            [assembly: UseStaticMapper(typeof(OtherMapper))]

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

    [Fact]
    public void CombineAssemblyLevelUseStaticMappers()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            [assembly: UseStaticMapper(typeof(OtherMapper))]
            [assembly: UseStaticMapper<AnotherMapper>]

            record A(int Value1, long Value2);
            record B(string Value1, string Value2);

            class OtherMapper { public static string IntToString(int source) => source.ToString(); }
            class AnotherMapper { public static string LongToString(long source) => source.ToString(); }

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
                var target = new global::B(
                    global::OtherMapper.IntToString(source.Value1),
                    global::AnotherMapper.LongToString(source.Value2)
                );
                return target;
                """
            );
    }

    [Fact]
    public void SelfReferencingUseStaticMapperStaticMethodNotCauseDiagnostic()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            [assembly: UseStaticMapper(typeof(Mapper))]

            record A(AExternal Value);
            record B(BExternal Value);
            record AExternal();
            record BExternal();

            [Mapper]
            public partial class Mapper
            {
                partial B Map(A source);

                public static BExternal ToBExternal(AExternal source) => new BExternal();
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(ToBExternal(source.Value));
                return target;
                """
            );
    }

    [Fact]
    public Task ProjectionWithUseStaticMapperShouldInlineGenerator()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            public class Maker
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class Car
            {
                public int Id { get; set; }
                public string Name { get; set; } = null!;
                public Maker Make { get; set; } = null!;
            }

            public record MakerDto
            {
                public int Id { get; init; }
                public string MakerName { get; init; } = null!;
            }

            public record CarDto
            {
                public int Id { get; init; }
                public string CarName { get; init; } = null!;
                public MakerDto Maker { get; init; } = null!;
            }

            [Mapper]
            public static partial class OtherMapper
            {
                public static partial IQueryable<MakerDto> ProjectToMakerDto(this IQueryable<Maker> query);

                [MapperRequiredMapping(RequiredMappingStrategy.Target)]
                [MapProperty(nameof(Maker.Name), nameof(MakerDto.MakerName))]
                public static partial MakerDto ToMakerDto(this Maker maker);
            }

            [Mapper]
            [UseStaticMapper(typeof(OtherMapper))]
            public static partial class Mapper
            {
                public static partial IQueryable<CarDto> ProjectToCarDto(this IQueryable<Car> query);

                [MapperRequiredMapping(RequiredMappingStrategy.Target)]
                [MapProperty(nameof(Car.Name), nameof(CarDto.CarName))]
                [MapProperty(nameof(Car.Make), nameof(CarDto.Maker))]
                public static partial CarDto MapToCarDto(this Car car);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ProjectionWithUseStaticMapperShouldReportDiagnosticWhenInliningFails()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            public class Maker
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class Car
            {
                public int Id { get; set; }
                public string Name { get; set; } = null!;
                public Maker Make { get; set; } = null!;
            }

            public record MakerDto
            {
                public int Id { get; init; }
                public string MakerName { get; init; } = null!;
            }

            public record CarDto
            {
                public int Id { get; init; }
                public string CarName { get; init; } = null!;
                public MakerDto Maker { get; init; } = null!;
            }

            [Mapper]
            public static partial class OtherMapper
            {
                public static MakerDto ToMakerDto(Maker maker)
                {
                    var id = maker.Id;
                    return new MakerDto { Id = id, MakerName = maker.Name };
                }
            }

            [Mapper]
            [UseStaticMapper(typeof(OtherMapper))]
            public static partial class Mapper
            {
                public static partial IQueryable<CarDto> ProjectToCarDto(this IQueryable<Car> query);

                [MapperRequiredMapping(RequiredMappingStrategy.Target)]
                [MapProperty(nameof(Car.Name), nameof(CarDto.CarName))]
                [MapProperty(nameof(Car.Make), nameof(CarDto.Maker))]
                public static partial CarDto MapToCarDto(this Car car);
            }
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingCannotInline);
    }

    [Theory]
    [InlineData("[UseStaticMapper(typeof(EnumMapper), NoExpressionInlining = true)]")]
    [InlineData("[UseStaticMapper<EnumMapper>(NoExpressionInlining = true)]")]
    public void ProjectionWithUseStaticMapperNoExpressionInliningShouldInvoke(string useStaticMapperAttribute)
    {
        var source = TestSourceBuilder.CSharp(
            $$"""
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            class A { public C Status { get; set; } }
            class B { public D Status { get; set; } }
            enum C { Value1, Value2 }
            enum D { Value1, Value2 }

            [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
            public static partial class EnumMapper
            {
                public static partial D MapToStatus(C s);
            }

            [Mapper]
            {{useStaticMapperAttribute}}
            public static partial class Mapper
            {
                public static partial IQueryable<B> Map(IQueryable<A> q);
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Status = global::EnumMapper.MapToStatus(x.Status),
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public void ProjectionWithUseStaticMapperShouldIgnoreNoExpressionInliningOfReferencedMapper()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            class A { public AExternal Value { get; set; } }
            class B { public string Value { get; set; } }
            class AExternal { public string Name { get; set; } }

            [Mapper(NoExpressionInlining = true)]
            public static class OtherMapper
            {
                [MapperNoExpressionInlining]
                public static string ToName(AExternal s) => s.Name;
            }

            [Mapper]
            [UseStaticMapper(typeof(OtherMapper))]
            public static partial class Mapper
            {
                public static partial IQueryable<B> Map(IQueryable<A> q);
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Value = x.Value.Name,
                            }
                        );
                #nullable enable
                """
            );
    }
}
