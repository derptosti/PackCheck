namespace PackCheck.Contracts;

public record ItemCategory(string Key, string Name)
{
    public override string ToString() => Name;
}

public static class ItemCategories
{
    public static IReadOnlyList<ItemCategory> All { get; } =
    [
        new("backpack", "Backpack"),
        new("shoe", "Shoes"),
        new("water-bottle", "Water bottle"),
        new("sunglasses", "Sunglasses"),
        new("torch", "Torch"),
        new("toothbrush", "Toothbrush")
    ];

    public static bool IsSupported(string? key) => All.Any(category => category.Key == key);
}

public class PackingTemplateDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TemplateItemDto> Items { get; set; } = [];
    public override string ToString() => Name;
}

public record TemplateItemDto(string Name, string? CategoryKey, int RequiredQuantity = 1, bool IsEssential = true);

public static class PackingTemplates
{
    public static IReadOnlyList<PackingTemplateDto> All { get; } =
    [
        new() { Key = "camping", Name = "Camping", Description = "A few nights outdoors.", Items =
        [new("Backpack", "backpack"), new("Shoes", "shoe", 2), new("Water bottle", "water-bottle"),
         new("Torch", "torch"), new("Toothbrush", "toothbrush"), new("Sleeping bag", null),
         new("Tent", null), new("Sunglasses", "sunglasses", 1, false)] },
        new() { Key = "beach", Name = "Beach holiday", Description = "Sun, sea and the essentials.", Items =
        [new("Backpack", "backpack"), new("Sunglasses", "sunglasses"), new("Water bottle", "water-bottle"),
         new("Shoes", "shoe", 2), new("Toothbrush", "toothbrush"), new("Sunscreen", null),
         new("Beach towel", null), new("Swimwear", null)] },
        new() { Key = "weekend", Name = "Weekend away", Description = "Travel light for a short break.", Items =
        [new("Backpack", "backpack"), new("Shoes", "shoe", 2), new("Toothbrush", "toothbrush"),
         new("Water bottle", "water-bottle"), new("Phone charger", null), new("Change of clothes", null),
         new("Sunglasses", "sunglasses", 1, false)] }
    ];
}
