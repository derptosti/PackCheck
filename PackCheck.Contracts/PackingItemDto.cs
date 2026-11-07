namespace PackCheck.Contracts;

public class PackingItemDto
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryKey { get; set; }
    public int RequiredQuantity { get; set; } = 1;
    public int PackedQuantity { get; set; }
    public bool IsEssential { get; set; } = true;
    public bool IsPacked => PackedQuantity >= RequiredQuantity;
    public string QuantityText => $"{PackedQuantity} / {RequiredQuantity} packed";
    public string StatusText => IsPacked ? "Packed" : IsEssential ? "Essential · still needed" : "Still needed";
}
