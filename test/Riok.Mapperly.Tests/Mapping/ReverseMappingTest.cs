namespace Riok.Mapperly.Tests.Mapping;

public class ReverseMappingTest
{
    [Fact]
    public Task BasicReverseMappingShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(Product.Price), nameof(ProductDTO.PriceInEuro))]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial Product ToProduct(this ProductDTO source);
            """,
            "class Product { public decimal Price { get; set; } }",
            "class ProductDTO { public decimal PriceInEuro { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseMappingWithMultiplePropertiesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(Product.Name), nameof(ProductDTO.ProductName))]
            [MapProperty(nameof(Product.Price), nameof(ProductDTO.PriceInEuro))]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial Product ToProduct(this ProductDTO source);
            """,
            "class Product { public string Name { get; set; } public decimal Price { get; set; } }",
            "class ProductDTO { public string ProductName { get; set; } public decimal PriceInEuro { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseMappingWithIgnoredPropertiesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(Product.Price), nameof(ProductDTO.PriceInEuro))]
            [MapperIgnoreSource(nameof(Product.Ignored))]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial Product ToProduct(this ProductDTO source);
            """,
            "class Product { public decimal Price { get; set; } public string Ignored { get; set; } }",
            "class ProductDTO { public decimal PriceInEuro { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact(Skip = "How to map a reverse value to an empty class?")]
    public Task ReverseMappingWithMapValueShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue(nameof(ProductDTO.CalculatedValue), "Fixed")]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial Product ToProduct(this ProductDTO source);
            """,
            "class Product { }",
            "class ProductDTO { public string CalculatedValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseMappingWithUseConverterShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(Product.Price), nameof(ProductDTO.PriceInEuro), Use = nameof(ConvertPrice))]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial Product ToProduct(this ProductDTO source);

            [UserMapping(Default = false)]
            private static decimal ConvertPrice(decimal price) => price * 1.1m;
            """,
            "class Product { public decimal Price { get; set; } }",
            "class ProductDTO { public decimal PriceInEuro { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseMappingWithNestedPropertiesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(Product.Discounts.EndUsers), nameof(ProductDTO.Discount))]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial Product ToProduct(this ProductDTO source);
            """,
            "class Product { public Discounts Discounts { get; set; } }",
            "class Discounts { public decimal EndUsers { get; set; } }",
            "class ProductDTO { public decimal Discount { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact(Skip = "Didn't add a check")]
    public Task ReverseMappingShouldReportCircularReference()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(Product.Price), nameof(ProductDTO.PriceInEuro))]
            [IncludeMappingConfiguration(nameof(ToProduct), Reverse = true)]
            public static partial ProductDTO ToDTO(this Product source);

            [IncludeMappingConfiguration(nameof(ToDTO), Reverse = true)]
            public static partial void ToProduct(this ProductDTO source, Product target);
            """,
            "class Product { public decimal Price { get; set; } }",
            "class ProductDTO { public decimal PriceInEuro { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact(
        Skip = "It should work similarly to mapping nested properties. However, the MapNestedProperties class only works with the source."
    )]
    // ProductDTO.Id = Product.Info.Id
    // Reverse
    // Product.Info.Id = ProductDTO.Id
    public Task ReverseMappingWithMapNestedPropertiesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapNestedProperties(nameof(Product.Info))]
            public static partial ProductDTO ToProduct(this Product source);

            [IncludeMappingConfiguration(nameof(ToProduct), Reverse = true)]
            public static partial Product ToDTO(this ProductDTO source);
            """,
            "class Product { public ProductInfo Info { get; set; } }",
            "class ProductInfo { public int Id { get; set; } }",
            "class ProductDTO { public int Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseMappingWithMapperIgnoreObsoleteMembersShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperIgnoreObsoleteMembers(IgnoreObsoleteMembersStrategy.Both)]
            public static partial Product ToProduct(this ProductDTO source);

            [IncludeMappingConfiguration(nameof(ToProduct), Reverse = true)]
            public static partial ProductDTO ToDTO(this Product source);
            """,
            "class Product { public int Id { get; set; } [Obsolete]public int Value { get; set; } }",
            "class ProductDTO { public int Id { get; set; } [Obsolete]public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    // Reverse Target to Source to calm down report
    public Task ReverseMappingWithMapperRequiredMappingShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapperRequiredMapping(RequiredMappingStrategy.Target)]
            public static partial Product ToProduct(this ProductDTO source);

            [IncludeMappingConfiguration(nameof(ToProduct), Reverse = true)]
            public static partial ProductDTO ToDTO(this Product source);
            """,
            "class Product { public int Value { get; set; } }",
            "class ProductDTO { public int Value { get; set; } public int UnmappedTarget { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseMappingWithMapDerivedTypeShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<ASubType1, BSubType1>]
            public partial B Map(A src);

            [IncludeMappingConfiguration(nameof(Map), Reverse = true)]
            public partial A Map2(this B src);
            """,
            "abstract class A { public string BaseValue { get; set; } }",
            "abstract class B { public string BaseValue { get; set; } }",
            "class ASubType1 : A { public string Value1 { get; set; } }",
            "class BSubType1 : B { public string Value1 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReverseEnumShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapEnumValue(C.Value2, D.Value2)]
            partial D Map(C src);

            [IncludeMappingConfiguration(nameof(Map), Reverse = true)]
            public partial C Map2(D src);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "enum C { Value1 = 10, Value2 = 100 }",
            "enum D { Value1 = 10, Value2 = 200 }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
