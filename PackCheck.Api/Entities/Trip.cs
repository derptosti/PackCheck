namespace PackCheck.Api.Entities;

public class Trip
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public string TripType { get; set; } = string.Empty;
    public List<PackingItem> Items { get; set; } = [];
}
