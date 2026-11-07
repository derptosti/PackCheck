using Microsoft.AspNetCore.Mvc;
using PackCheck.Api.Services;
using PackCheck.Contracts;

namespace PackCheck.Api.Controllers;

[ApiController]
[Route("api/trips/{tripId:int}/items")]
public class PackingItemsController(IPackingItemService service) : ControllerBase
{
    [HttpGet]
    public Task<List<PackingItemDto>> GetAll(int tripId, CancellationToken cancellationToken) =>
        service.GetAllAsync(tripId, cancellationToken);

    [HttpGet("{itemId:int}")]
    public Task<PackingItemDto> Get(int tripId, int itemId, CancellationToken cancellationToken) =>
        service.GetAsync(tripId, itemId, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<PackingItemDto>> Create(int tripId, SavePackingItemRequest request, CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(tripId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { tripId, itemId = item.Id }, item);
    }

    [HttpPut("{itemId:int}")]
    public Task<PackingItemDto> Update(int tripId, int itemId, SavePackingItemRequest request, CancellationToken cancellationToken) =>
        service.UpdateAsync(tripId, itemId, request, cancellationToken);

    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> Delete(int tripId, int itemId, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(tripId, itemId, cancellationToken);
        return NoContent();
    }
}
