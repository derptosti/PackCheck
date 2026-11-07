namespace PackCheck.Api.Entities;

public class PackingItem
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? CategoryKey { get; set; }
    public int RequiredQuantity { get; set; } = 1;
    public int PackedQuantity { get; set; }
    public bool IsEssential { get; set; } = true;
}
