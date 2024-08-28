namespace Riok.Mapperly.Sample;

public class Car
{
    public string Name { get; set; } = string.Empty;

    public int NumberOfSeats { get; set; }

    public CarColor Color { get; set; }

    public Manufacturer? Manufacturer { get; set; }

    public List<Tire> Tires { get; } = [];
}

public enum CarColor
{
    Black = 1,
    Blue = 2,
    White = 3,
}

public class Manufacturer
{
    public Manufacturer(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }

    public string Name { get; }
}

public class Tire
{
    public string Description { get; set; } = string.Empty;
}
