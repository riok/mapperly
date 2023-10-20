namespace Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar;

public sealed record CreateCarAnswer(Guid Id, Status Status);

public enum Status
{
    Success = default,
    Fail = 2,
}
