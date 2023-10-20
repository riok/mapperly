using MediatR;
using Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar.Mapperly;

namespace Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar;

public sealed record CreateCarCommand(string Name, Manufacturer Manufacturer) : IRequest<CreateCarAnswer>;

public class CreateCarCommandHandler(ICreateCarMapperly mapperly) : IRequestHandler<CreateCarCommand, CreateCarAnswer>
{
    public async Task<CreateCarAnswer> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        var car = mapperly.CommandToCar(request);
        car.Id = Guid.NewGuid();
        return await Task.FromResult(mapperly.CarToAnswer(car));
    }
}
