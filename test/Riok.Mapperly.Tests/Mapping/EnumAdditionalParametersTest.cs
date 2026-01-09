using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Mapping;

public class EnumAdditionalParametersTest
{
    [Fact]
    public void TwoEnumSourcesByValueShouldMapToIEnumerableEnumTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial IEnumerable<Target> Combine(Source1 s1, Source2 s2);",
            "enum Source1 { B = 2 }",
            "enum Source2 { C = 3 }",
            "enum Target { A = 1, B = 2, C = 3 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                yield return s1 switch
                {
                    global::Source1.B => global::Target.B,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(s1), s1, "The value of enum Source1 is not supported"),
                };
                yield return s2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(s2), s2, "The value of enum Source2 is not supported"),
                };
                """
            );
    }

    [Fact]
    public void TwoEnumSourcesByValueShouldMapWithCascadingSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { One = 1, Two = 2, Four = 4 }",
            "enum Source2 { Two = 2, Three = 3 }",
            "enum Target { One = 1, Two = 2, Three = 3, Four = 4 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.One => global::Target.One,
                    global::Source1.Two => global::Target.Two,
                    global::Source1.Four => global::Target.Four,
                    _ => source2 switch
                {
                    global::Source2.Three => global::Target.Three,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void TwoEnumSourcesByNameShouldMapWithCascadingSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { A, B }",
            "enum Source2 { B, C }",
            "enum Target { A, B, C }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.B => global::Target.B,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void ThreeEnumSourcesShouldMapWithThreeLevelCascadingSwitch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Combine(Source1 s1, Source2 s2, Source3 s3);",
            "enum Source1 { A = 1 }",
            "enum Source2 { B = 2 }",
            "enum Source3 { C = 3 }",
            "enum Target { A = 1, B = 2, C = 3 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return s1 switch
                {
                    global::Source1.A => global::Target.A,
                    _ => s2 switch
                {
                    global::Source2.B => global::Target.B,
                    _ => s3 switch
                {
                    global::Source3.C => global::Target.C,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(s1), s1, "The value of enum Source1 is not supported"),
                },
                },
                };
                """
            );
    }

    [Fact]
    public void OverlappingValuesShouldPrioritizeFirstSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { Shared = 1 }",
            "enum Source2 { Shared = 1, Extra = 2 }",
            "enum Target { Shared = 1, Extra = 2 }"
        );

        // Source1.Shared maps to Target.Shared
        // Source2.Shared is NOT mapped because Target.Shared is already covered
        // Source2.Extra maps to Target.Extra
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.Shared => global::Target.Shared,
                    _ => source2 switch
                {
                    global::Source2.Extra => global::Target.Extra,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void AdditionalParameterCoversAllMissingShouldNotReportDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { A = 1 }",
            "enum Source2 { B = 2 }", // Contributes B mapping
            "enum Target { A = 1, B = 2 }"
        );

        // No diagnostics expected - Source2 contributes B mapping
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    _ => source2 switch
                {
                    global::Source2.B => global::Target.B,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void MixedEnumAndNonEnumParametersShouldOnlyUseEnumParams()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Map(Source1 source1, int ignored, Source2 source2);",
            "enum Source1 { A = 1 }",
            "enum Source2 { B = 2 }",
            "enum Target { A = 1, B = 2 }"
        );

        // The int parameter should be ignored for enum mapping
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    _ => source2 switch
                {
                    global::Source2.B => global::Target.B,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void SingleEnumParameterShouldFallbackToStandardMapping()
    {
        // When there's only one enum parameter, standard enum mapping should be used
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Map(Source source);",
            "enum Source { A, B, C }",
            "enum Target { A, B, C }"
        );

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::Target)source;");
    }

    [Fact]
    public void DisjointEnumsShouldMapAllValues()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValue)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { A = 1, B = 2 }",
            "enum Source2 { C = 3, D = 4 }",
            "enum Target { A = 1, B = 2, C = 3, D = 4 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.B => global::Target.B,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    global::Source2.D => global::Target.D,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void GlobalConfigByValueShouldGenerateCascading()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Combine(Source1 source1, Source2 source2);",
            new TestSourceBuilderOptions { EnumMappingStrategy = EnumMappingStrategy.ByValue },
            "enum Source1 { X = 10 }",
            "enum Source2 { Y = 20 }",
            "enum Target { X = 10, Y = 20 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.X => global::Target.X,
                    _ => source2 switch
                {
                    global::Source2.Y => global::Target.Y,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void ByValueCheckDefinedStrategyShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByValueCheckDefined)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { A = 1, B = 2 }",
            "enum Source2 { C = 3 }",
            "enum Target { A = 1, B = 2, C = 3 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.B => global::Target.B,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void ByNameWithIgnoreCaseShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)]
            partial Target Combine(Source1 source1, Source2 source2);
            """,
            "enum Source1 { apple, BANANA }",
            "enum Source2 { cherry }",
            "enum Target { Apple, Banana, Cherry }"
        );

        // Should match case-insensitively: apple -> Apple, BANANA -> Banana, cherry -> Cherry
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.apple => global::Target.Apple,
                    global::Source1.BANANA => global::Target.Banana,
                    _ => source2 switch
                {
                    global::Source2.cherry => global::Target.Cherry,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void WithFallbackValueShouldUseFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByName, FallbackValue = Target.Unknown)]
            partial Target Combine(Source1 source1, Source2 source2);
            """,
            "enum Source1 { A, B }",
            "enum Source2 { C }",
            "enum Target { A, B, C, Unknown }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.B => global::Target.B,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => global::Target.Unknown,
                },
                };
                """
            );
    }

    [Fact]
    public void WithExplicitMappingShouldUseMapEnumValue()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValue)]
            [MapEnumValue(Source1.X, Target.MappedX)]
            partial Target Combine(Source1 source1, Source2 source2);
            """,
            "enum Source1 { X = 1, Y = 2 }",
            "enum Source2 { Z = 3 }",
            "enum Target { MappedX = 10, Y = 2, Z = 3 }"
        );

        // X should be explicitly mapped to MappedX, Y by value, Z from source2 by value
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.X => global::Target.MappedX,
                    global::Source1.Y => global::Target.Y,
                    _ => source2 switch
                {
                    global::Source2.Z => global::Target.Z,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void WithIgnoredSourceValueShouldSkipMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValue)]
            [MapperIgnoreSourceValue(Source1.Ignored)]
            partial Target Combine(Source1 source1, Source2 source2);
            """,
            "enum Source1 { A = 1, Ignored = 2, C = 3 }",
            "enum Source2 { D = 4 }",
            "enum Target { A = 1, B = 2, C = 3, D = 4 }"
        );

        // Source1.Ignored should be skipped, so Target.B won't be mapped from Source1
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.C => global::Target.C,
                    _ => source2 switch
                {
                    global::Source2.D => global::Target.D,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void WithIgnoredTargetValueShouldSkipTargetMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValue)]
            [MapperIgnoreTargetValue(Target.IgnoredTarget)]
            partial Target Combine(Source1 source1, Source2 source2);
            """,
            "enum Source1 { A = 1, B = 2 }",
            "enum Source2 { C = 3 }",
            "enum Target { A = 1, IgnoredTarget = 2, C = 3 }"
        );

        // Target.IgnoredTarget (value=2) should not be mapped, so Source1.B won't have a matching target
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void WithDifferentValuesButSameNamesByNameStrategy()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapEnum(EnumMappingStrategy.ByName)] partial Target Combine(Source1 source1, Source2 source2);",
            "enum Source1 { Alpha = 100, Beta = 200 }",
            "enum Source2 { Gamma = 300 }",
            "enum Target { Alpha = 1, Beta = 2, Gamma = 3 }"
        );

        // Should match by name regardless of underlying values
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.Alpha => global::Target.Alpha,
                    global::Source1.Beta => global::Target.Beta,
                    _ => source2 switch
                {
                    global::Source2.Gamma => global::Target.Gamma,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void GlobalMapperConfigWithByNameIgnoreCase()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Combine(Source1 source1, Source2 source2);",
            new TestSourceBuilderOptions { EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true },
            "enum Source1 { ALPHA }",
            "enum Source2 { beta }",
            "enum Target { Alpha, Beta }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.ALPHA => global::Target.Alpha,
                    _ => source2 switch
                {
                    global::Source2.beta => global::Target.Beta,
                    _ => throw new global::System.ArgumentOutOfRangeException(nameof(source1), source1, "The value of enum Source1 is not supported"),
                },
                };
                """
            );
    }

    [Fact]
    public void FallbackValueWithByValueStrategyShouldUseFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValue, FallbackValue = Target.Default)]
            partial Target Map(Source1 source1, Source2 source2);
            """,
            "enum Source1 { A = 1, B = 2 }",
            "enum Source2 { C = 3 }",
            "enum Target { A = 1, B = 2, C = 3, Default = 99 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.B => global::Target.B,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => global::Target.Default,
                },
                };
                """
            );
    }

    [Fact]
    public void FallbackValueWithByValueCheckDefinedStrategyShouldUseFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValueCheckDefined, FallbackValue = Target.Undefined)]
            partial Target Map(Source1 source1, Source2 source2);
            """,
            "enum Source1 { Known = 1 }",
            "enum Source2 { AnotherKnown = 2 }",
            "enum Target { Known = 1, AnotherKnown = 2, Undefined = 0 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.Known => global::Target.Known,
                    _ => source2 switch
                {
                    global::Source2.AnotherKnown => global::Target.AnotherKnown,
                    _ => global::Target.Undefined,
                },
                };
                """
            );
    }

    [Fact]
    public void FallbackValueNotAffectedByEnumNamingStrategies()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByName, FallbackValue = Target.DefaultValue)]
            partial Target Map(Source1 source1, Source2 source2);
            """,
            "enum Source1 { value_one }",
            "enum Source2 { value_two }",
            "enum Target { value_one, value_two, DefaultValue }"
        );

        // FallbackValue should be used as-is, not affected by naming strategy
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.value_one => global::Target.value_one,
                    _ => source2 switch
                {
                    global::Source2.value_two => global::Target.value_two,
                    _ => global::Target.DefaultValue,
                },
                };
                """
            );
    }

    [Fact]
    public void FallbackValueWithExplicitMappingShouldUseFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValue, FallbackValue = Target.Default)]
            [MapEnumValue(Source1.Special, Target.MappedSpecial)]
            partial Target Map(Source1 source1, Source2 source2);
            """,
            "enum Source1 { Special = 1, Regular = 2 }",
            "enum Source2 { Extra = 3 }",
            "enum Target { MappedSpecial = 10, Regular = 2, Extra = 3, Default = 0 }"
        );

        // Should use explicit mapping for Special, regular value mapping for Regular,
        // source2 mapping for Extra, and fallback for unknown values
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.Special => global::Target.MappedSpecial,
                    global::Source1.Regular => global::Target.Regular,
                    _ => source2 switch
                {
                    global::Source2.Extra => global::Target.Extra,
                    _ => global::Target.Default,
                },
                };
                """
            );
    }

    [Fact]
    public void FallbackValueWithIgnoredValuesShouldUseFallback()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnum(EnumMappingStrategy.ByValue, FallbackValue = Target.Default)]
            [MapperIgnoreSourceValue(Source1.Ignored)]
            partial Target Map(Source1 source1, Source2 source2);
            """,
            "enum Source1 { A = 1, Ignored = 2, B = 3 }",
            "enum Source2 { C = 4 }",
            "enum Target { A = 1, B = 3, C = 4, Default = 0 }"
        );

        // Ignored value should not be mapped, fallback should be used for unknown values
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return source1 switch
                {
                    global::Source1.A => global::Target.A,
                    global::Source1.B => global::Target.B,
                    _ => source2 switch
                {
                    global::Source2.C => global::Target.C,
                    _ => global::Target.Default,
                },
                };
                """
            );
    }
}
