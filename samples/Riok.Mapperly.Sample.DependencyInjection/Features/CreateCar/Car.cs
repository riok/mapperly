namespace Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar;

public class Car
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public CarColor Color { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public IEnumerable<Tire> Tires { get; } = [];
}

public enum CarColor
{
    Black = default,
    Blue = 2,
    White = 3,
}

public class Manufacturer(int id, string name)
{
    public int Id { get; } = id;

    public string Name { get; } = name;
}

public class Tire
{
    public string Description { get; set; } = string.Empty;
}
