namespace Riok.Mapperly.Tests.Mapping;

public class IncludeMappingConfigurationTest
{
    [Fact]
    public Task UsesMapPropertyFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
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
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
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
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
            """,
            "class A { public string Property { get; set; } }",
            "class B { public string Property { get; set; } public string Ignored { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UsesMapPropertyFromSourceFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(A.Value))]
            private partial B OtherMapper(A source);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            public partial B Mapper(A source);
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
            private partial B OtherMapper(A source);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            public partial B Mapper(A source);
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
            private partial B OtherMapper(A source);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            public partial B Mapper(A source);
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
            partial B BaseMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(BaseMappingMethod))]
            partial B FirstMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(FirstMappingMethod))]
            partial B SecondMappingMethod(A a);
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
            [IncludeMappingConfiguration(nameof(ReferencedMapper))]
            public partial B Mapper(A source);

            [MapProperty(nameof(A.Source), nameof(B.Target))]
            public partial void ReferencedMapper(A source, B target);
            """,
            "class A { public string Source { get; set; } }",
            "class B { public string Target { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AppliesOnDifferentMethodDeclaration2()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Source), nameof(B.Target))]
            public partial B ReferencedMapper(A source);

            [IncludeMappingConfiguration(nameof(ReferencedMapper))]
            public partial void Mapper(A source, B target);
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
            partial A OtherMapper(B b);

            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B OtherMapper(A a);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            partial B Mapper(A a);
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
            public partial B OtherMapper(A source);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            public partial B Mapper(A source);

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
            public partial B MainMapper(A source);

            [IncludeMappingConfiguration(nameof(ReferencedMapper))]
            public partial BY UsedMapper(AX source);

            [MapProperty(nameof(AX.Source), nameof(BY.Target))]
            public partial void ReferencedMapper(AX source, BY target);
            """,
            "class A { public AX Item { get; set; } }",
            "class AX { public string Source { get; set; } }",
            "class B { public BY Item { get; set; } }",
            "class BY { public string Target { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReportsCircularReferences()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            [IncludeMappingConfiguration(nameof(MapAnother))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
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
            public static partial B OtherMapper(A a);

            [MapProperty(nameof(ADerived.SourceName1), nameof(BDerived.DestinationName1))]
            [MapProperty(nameof(ADerived.SourceName2), nameof(BDerived.DestinationName2))]
            public static partial BDerived OtherMapper(ADerived a);

            [IncludeMappingConfiguration(nameof(OtherMapper))]
            public static partial BDerived Mapper(ADerived a);
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
            partial B UnrelatedMapper(C a);

            [IncludeMappingConfiguration(nameof(UnrelatedMapper))]
            partial B MapAnother(A a);
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
            partial C UnrelatedMapper(A a);

            [IncludeMappingConfiguration(nameof(UnrelatedMapper))]
            partial B MapAnother(A a);
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
            partial B UnrelatedMapper(C a);

            [MapProperty(nameof(D.SourceName), nameof(B.DestinationName))]
            partial B UnrelatedMapper(D a);

            [IncludeMappingConfiguration(nameof(UnrelatedMapper))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            "class C { public string SourceName { get; set; } }",
            "class D { public string SourceName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
