using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar.Mapperly;

[Mapper]
public partial class CreateCarMapperly : ICreateCarMapperly
{
    public partial Car CommandToCar(CreateCarCommand command);
    public CreateCarAnswer CarToAnswer(Car car)
    => new global::Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar.CreateCarAnswer
    (
        car.Id,
        car.Id != Guid.Empty? Status.Success : Status.Fail
    );
}
