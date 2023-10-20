namespace Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar.Mapperly;

public interface ICreateCarMapperly
{
    public Car CommandToCar(CreateCarCommand command);
    public CreateCarAnswer CarToAnswer(Car car);
}
