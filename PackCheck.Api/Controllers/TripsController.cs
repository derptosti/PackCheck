using Microsoft.AspNetCore.Mvc;
using PackCheck.Api.Services;
using PackCheck.Contracts;

namespace PackCheck.Api.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController(ITripService service) : ControllerBase
{
    [HttpGet]
    public Task<List<TripDto>> GetAll(CancellationToken cancellationToken) => service.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public Task<TripDto> Get(int id, CancellationToken cancellationToken) => service.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<TripDto>> Create(CreateTripRequest request, CancellationToken cancellationToken)
    {
        var trip = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = trip.Id }, trip);
    }

    [HttpPut("{id:int}")]
    public Task<TripDto> Update(int id, UpdateTripRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
