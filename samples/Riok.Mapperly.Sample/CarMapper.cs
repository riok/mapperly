using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Existing Mapper
// Link all try find
// Pass context
// Update TryFind to handle parameters

// Context should pass down all available params
// FindOrBuildMapping should update current context with used params
// When creating Builder ctx.CurrentParams as arg

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class CarMapper
{
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);

    // [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    // public static partial CarDto MapCarToDto(Car car, int v);

    // public static partial C MapCarToDto(int value);

    // public static partial B Map(A src, int value);
    public static partial C Map(A src, int value, int v1);

    public static partial DogDto Map(Dog src, int value, int v1);

    public static partial DataADto Map(DataA src);

    public static partial DataBDto Map(DataB src);
}

public class A
{
    public string StringValue { get; set; }
}

public class B
{
    public string StringValue { get; set; }
    public string Value { get; init; }
}

public record C
{
    public int Value { get; init; }
    public int V1 { get; init; }
}

public class Dog
{
    public DogOwner Owner { get; set; }
    public int Value { get; set; }
}

public class DogOwner
{
    public string Name { get; set; }
}

public class DogOwnerDto
{
    public string Name { get; set; }
}

public class DogDto
{
    public DogOwnerDto Owner { get; set; }
    public int Value { get; set; }
}

public record DataA(DataC Nest);

public record DataADto(DataD Nest);

public record DataB(DataC Nest);

public record DataBDto(DataD Nest);

public record DataC(int Value)
{
    public int Value { get; init; } = Value;
}

public record DataD
{
    public int Value { get; init; }
}
