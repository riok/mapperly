using MediatR;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Sample.DependencyInjection.Features.CreateCar;

namespace Riok.Mapperly.Sample.DependencyInjection.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarController(ISender sender) : ControllerBase
{
    [HttpPost(template: "create-car")]
    public async Task<IActionResult> CreateCar(CreateCarCommand command, CancellationToken token)
    => Ok(await sender.Send(command, token));
}
