using System;
using System.Collections.Generic;
using System.Text;

namespace PackCheck.Core.Entities;

public class PackingItem
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CategoryKey { get; set; }

    public int RequiredQuantity { get; set; } = 1;

    public int PackedQuantity { get; set; }

    public Trip Trip { get; set; } = null!;
}