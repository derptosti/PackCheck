namespace PackCheck.Contracts;

public class TripDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; } = DateTime.Today;
    public string TripType { get; set; } = string.Empty;
    public List<PackingItemDto> Items { get; set; } = [];
    public int RequiredQuantity => Items.Sum(item => item.RequiredQuantity);
    public int PackedQuantity => Items.Sum(item => item.PackedQuantity);
    public double Progress => RequiredQuantity == 0 ? 0 : (double)PackedQuantity / RequiredQuantity;
    public string ProgressText => $"{PackedQuantity} / {RequiredQuantity} packed";
    public int MissingEssentials => Items.Count(item => item.IsEssential && !item.IsPacked);
    public string Subtitle => $"{DepartureDate:ddd, d MMM yyyy} · {TripType}";
}
