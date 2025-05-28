namespace Riok.Mapperly.Tests.Mapping;

public class IncludeMappingConfigurationTest
{
    [Fact]
    public Task UsesMapPropertyFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B MapOther(A a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapperIgnoreSourceFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreSource(nameof(A.Ignored))]
            partial B MapOther(A a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);
            """,
            "class A { public string Property { get; set; } public string Ignored { get; set; } }",
            "class B { public string Property { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapperIgnoreTargetFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreTarget(nameof(A.Ignored))]
            partial B MapOther(A a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);
            """,
            "class A { public string Property { get; set; } }",
            "class B { public string Property { get; set; } public string Ignored { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapValueFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue(nameof(A.Value), "C1")]
            partial B MapOther(A a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);
            """,
            "class A { }",
            "class B { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapValueWithUseFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue(nameof(A.Value), Use = nameof(GetValue))]
            partial B MapOther(A a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);

            string GetValue() => "C1";
            """,
            "class A { }",
            "class B { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapPropertyFromSourceFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(A.Value))]
            private partial B MapOther(A source);

            [IncludeMappingConfiguration(nameof(MapOther))]
            public partial B Map(A source);
            """,
            "class A { }",
            "class B { public A Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapNestedPropertiesFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapNestedProperties(nameof(A.Value))]
            private partial B MapOther(A source);

            [IncludeMappingConfiguration(nameof(MapOther))]
            public partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public int Id { get; set; } }",
            "class C { public int Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapperIgnoreObsoleteMembersFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreObsoleteMembers(IgnoreObsoleteMembersStrategy.Both)]
            private partial B MapOther(A source);

            [IncludeMappingConfiguration(nameof(MapOther))]
            public partial B Map(A source);
            """,
            "class A { public int Id { get; set; } [Obsolete]public int Value { get; set; } }",
            "class B { public int Id { get; set; } [Obsolete]public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AppliesRecursively()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B MapBase(A a);

            [IncludeMappingConfiguration(nameof(MapBase))]
            partial B MapFirst(A a);

            [IncludeMappingConfiguration(nameof(MapFirst))]
            partial B MapSecond(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AppliesOnDifferentMethodDeclaration()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [IncludeMappingConfiguration(nameof(MapOther))]
            public partial B Map(A source);

            [MapProperty(nameof(A.Source), nameof(B.Target))]
            public partial void MapOther(A source, B target);
            """,
            "class A { public string Source { get; set; } }",
            "class B { public string Target { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IncludesConfigurationFromBaseClassMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName1), nameof(B.DestinationName1))]
            public static partial B Map(A a);

            [IncludeMappingConfiguration(nameof(Map))]
            [MapProperty(nameof(A.SourceName2), nameof(B.DestinationName2))]
            public static partial BDerived MapDerived(ADerived a);
            """,
            "class A { public string SourceName1 { get; set; } }",
            "class ADerived : A { public string SourceName2 { get; set; } }",
            "class B { public string DestinationName1 { get; set; } }",
            "class BDerived : B { public string DestinationName2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ResolvesMatchingCandidateIfOnlyOneMatches()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(B.DestinationName), nameof(A.SourceName))]
            partial A MapOther(B b);

            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B MapOther(A a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldIncludeUseReferencedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue), nameof(B.StringValue), Use = nameof(ModifyString))]
            public partial B MapOther(A source);

            [IncludeMappingConfiguration(nameof(MapOther))]
            public partial B Map(A source);

            private string ModifyString(string source) => source + "-modified";
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldIncludeConfigurationOnUseReferencedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial B Map(A source);

            [IncludeMappingConfiguration(nameof(MapSubPropertyOther))]
            public partial BY MapSubProperty(AX source);

            [MapProperty(nameof(AX.Source), nameof(BY.Target))]
            public partial void MapSubPropertyOther(AX source, BY target);
            """,
            "class A { public AX Item { get; set; } }",
            "class AX { public string Source { get; set; } }",
            "class B { public BY Item { get; set; } }",
            "class BY { public string Target { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IncludesStringFormatAndFormatProviderFromMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider]
            private readonly IFormatProvider _enCulture = CultureInfo.GetCultureInfo("en-US");

            [MapProperty(nameof(A.Price), nameof(B.Price), StringFormat = "C", FormatProvider = nameof(_enCulture))]
            public partial B MapOther(A source);

            [IncludeMappingConfiguration(nameof(MapOther))]
            public partial B Map(A source);
            """,
            "class A { public int Price { get; set; } }",
            "class B { public string Price { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsCircularReferences()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            [IncludeMappingConfiguration(nameof(MapOther))]
            partial B Map(A a);

            [IncludeMappingConfiguration(nameof(Map))]
            partial B MapOther(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsDuplicatedName()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName1), nameof(B.DestinationName1))]
            public static partial B MapOther(A a);

            [MapProperty(nameof(ADerived.SourceName1), nameof(BDerived.DestinationName1))]
            [MapProperty(nameof(ADerived.SourceName2), nameof(BDerived.DestinationName2))]
            public static partial BDerived MapOther(ADerived a);

            [IncludeMappingConfiguration(nameof(MapOther))]
            public static partial BDerived Map(ADerived a);
            """,
            "class A { public string SourceName1 { get; set; } }",
            "class ADerived : A { public string SourceName2 { get; set; } }",
            "class B { public string DestinationName1 { get; set; } }",
            "class BDerived : B { public string DestinationName2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsSourceTypeIsInvalidInInclude()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(C.SourceName), nameof(B.DestinationName))]
            partial B MapUnrelated(C a);

            [IncludeMappingConfiguration(nameof(MapUnrelated))]
            partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            "class C { public string SourceName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsTargetTypeIsInvalidInInclude()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(C.DestinationName))]
            partial C MapUnrelated(A a);

            [IncludeMappingConfiguration(nameof(MapUnrelated))]
            partial B MapOther(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            "class C { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsOnMultipleCandidatesButNoneMatches()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(C.SourceName), nameof(B.DestinationName))]
            partial B MapUnrelated(C a);

            [MapProperty(nameof(D.SourceName), nameof(B.DestinationName))]
            partial B MapUnrelated(D a);

            [IncludeMappingConfiguration(nameof(MapUnrelated))]
            partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            "class C { public string SourceName { get; set; } }",
            "class D { public string SourceName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
