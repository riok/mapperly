using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, UseReferenceHandling = true)]
public static partial class CarMapper
{
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);
    // public static partial C MapCarToDto(int value);

    // public static partial B Map(A src, int value);
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

// public record C
// {
//     public int Value { get; set; }
// }
