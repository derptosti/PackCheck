using System;
using System.Collections.Generic;
using System.Text;

namespace PackCheck.Core.Entities;

public class Trip
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime DepartureDate { get; set; }

    public string TripType { get; set; } = string.Empty;

    public ICollection<PackingItem> PackingItems { get; set; }
        = new List<PackingItem>();
}