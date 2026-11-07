using System.ComponentModel.DataAnnotations;

namespace PackCheck.Contracts;

public class CreateTripRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; } = DateTime.Today;
    [Required]
    public string TemplateKey { get; set; } = string.Empty;
}

public class UpdateTripRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; } = DateTime.Today;
}

public class SavePackingItemRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    public string? CategoryKey { get; set; }
    [Range(1, 999)]
    public int RequiredQuantity { get; set; } = 1;
    [Range(0, 999)]
    public int PackedQuantity { get; set; }
    public bool IsEssential { get; set; } = true;
}
