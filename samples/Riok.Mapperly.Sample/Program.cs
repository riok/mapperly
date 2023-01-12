using System.Text.Json;
using Riok.Mapperly.Sample;

var car = new Car
{
    Name = "my car",
    NumberOfSeats = 5,
    Color = CarColor.Blue,
    Manufacturer = new Manufacturer(1, "best manufacturer"),
    Tires =
    {
        new Tire { Description = "front left tire" },
        new Tire { Description = "front right tire" },
        new Tire { Description = "back left tire" },
        new Tire { Description = "back right tire" },
    },
};

var carDto = CarMapper.MapCarToDto(car);

Console.WriteLine("Mapped car to car DTO:");
Console.WriteLine(JsonSerializer.Serialize(carDto, new JsonSerializerOptions { WriteIndented = true }));
