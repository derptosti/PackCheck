using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackCheck.Api.Data;
using PackCheck.Api.Entities;
using PackCheck.Contracts;
using Xunit;

namespace PackCheck.Tests;

public sealed class PackCheckApiTests
{
    [Fact]
    public async Task CreateTripPersistsItsTemplateAndPackingItems()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();

        var trip = await CreateTripAsync(client);
        var saved = await client.GetFromJsonAsync<TripDto>($"/api/trips/{trip.Id}");
        Assert.NotNull(saved);
        Assert.Equal("Ardennes camping", saved.Name);
        Assert.Equal(PackingTemplates.All.Single(template => template.Key == "camping").Items.Count, saved.Items.Count);
        Assert.All(saved.Items, item =>
        {
            Assert.Equal(saved.Id, item.TripId);
            Assert.Equal(0, item.PackedQuantity);
        });
        Assert.Contains(saved.Items, item => item.Name == "Sleeping bag" && item.CategoryKey == null && item.IsEssential);
        Assert.Equal(0, saved.Progress);

        var list = await client.GetFromJsonAsync<List<TripDto>>("/api/trips");
        Assert.Contains(list!, candidate => candidate.Id == saved.Id && candidate.Items.Count == saved.Items.Count);
    }

    [Fact]
    public async Task ManualItemCanBeAddedPackedEditedAndDeleted()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        var trip = await CreateTripAsync(client);
        var route = $"/api/trips/{trip.Id}/items";

        using var createdResponse = await client.PostAsJsonAsync(route, new SavePackingItemRequest
        {
            Name = "  Sleeping mat  ", RequiredQuantity = 2, IsEssential = true
        });
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var item = await createdResponse.Content.ReadFromJsonAsync<PackingItemDto>();
        Assert.NotNull(item);
        Assert.Equal("Sleeping mat", item.Name);
        Assert.Null(item.CategoryKey);

        using var updatedResponse = await client.PutAsJsonAsync($"{route}/{item.Id}", new SavePackingItemRequest
        {
            Name = "Sleeping mat", RequiredQuantity = 2, PackedQuantity = 1, IsEssential = false
        });
        Assert.True(updatedResponse.IsSuccessStatusCode, await updatedResponse.Content.ReadAsStringAsync());
        var saved = await client.GetFromJsonAsync<PackingItemDto>($"{route}/{item.Id}");
        Assert.NotNull(saved);
        Assert.Equal(1, saved.PackedQuantity);
        Assert.False(saved.IsEssential);
        var summary = await client.GetFromJsonAsync<TripDto>($"/api/trips/{trip.Id}");
        Assert.Equal(1, summary!.PackedQuantity);
        Assert.True(summary.Progress > 0);

        using var deleted = await client.DeleteAsync($"{route}/{item.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var missing = await client.GetAsync($"{route}/{item.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(1, -1)]
    public async Task InvalidQuantitiesAreRejectedWithoutChangingTheItem(int required, int packed)
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        var trip = await CreateTripAsync(client);
        var item = trip.Items.First();
        var route = $"/api/trips/{trip.Id}/items/{item.Id}";

        using var response = await client.PutAsJsonAsync(route, new SavePackingItemRequest
        {
            Name = item.Name, CategoryKey = item.CategoryKey, RequiredQuantity = required, PackedQuantity = packed
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var unchanged = await client.GetFromJsonAsync<PackingItemDto>(route);
        Assert.Equal(item.RequiredQuantity, unchanged!.RequiredQuantity);
        Assert.Equal(item.PackedQuantity, unchanged.PackedQuantity);
    }

    [Fact]
    public async Task CrossTripItemRoutesCannotReadChangeOrDeleteAnotherTripsItem()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        var firstTrip = await CreateTripAsync(client);
        var secondTrip = await CreateTripAsync(client, "Another trip");
        var item = firstTrip.Items.First();
        var wrongRoute = $"/api/trips/{secondTrip.Id}/items/{item.Id}";

        using var get = await client.GetAsync(wrongRoute);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        using var put = await client.PutAsJsonAsync(wrongRoute, new SavePackingItemRequest
        {
            Name = "Wrong trip update", RequiredQuantity = 1
        });
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        using var delete = await client.DeleteAsync(wrongRoute);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        var original = await client.GetFromJsonAsync<PackingItemDto>($"/api/trips/{firstTrip.Id}/items/{item.Id}");
        Assert.Equal(item.Name, original!.Name);
    }

    [Fact]
    public async Task DeletingTripRemovesItsRelatedItemsAndDatabaseRejectsOrphans()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        var trip = await CreateTripAsync(client);

        using var response = await client.DeleteAsync($"/api/trips/{trip.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PackCheckDbContext>();
        Assert.False(await db.Set<PackingItem>().AnyAsync(item => item.TripId == trip.Id));

        db.Set<PackingItem>().Add(new PackingItem
        {
            TripId = trip.Id, Name = "Orphan", RequiredQuantity = 1, PackedQuantity = 0, IsEssential = true
        });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task InvalidTemplateAndUnsupportedCategoryDoNotCreateRecords()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        using var invalidTrip = await client.PostAsJsonAsync("/api/trips", new CreateTripRequest
        {
            Name = "Trip", DepartureDate = DateTime.Today, TemplateKey = "not-a-template"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidTrip.StatusCode);
        var trips = await client.GetFromJsonAsync<List<TripDto>>("/api/trips");
        Assert.Empty(trips!);

        var trip = await CreateTripAsync(client);
        using var invalidItem = await client.PostAsJsonAsync($"/api/trips/{trip.Id}/items", new SavePackingItemRequest
        {
            Name = "Unsupported", CategoryKey = "automatic-suitcase", RequiredQuantity = 1
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidItem.StatusCode);
        var saved = await client.GetFromJsonAsync<TripDto>($"/api/trips/{trip.Id}");
        Assert.Equal(trip.Items.Count, saved!.Items.Count);
    }

    [Fact]
    public async Task ValidPhotoWithoutAzureConfigurationReportsUnavailableAndLeavesPackingUnchanged()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        var trip = await CreateTripAsync(client);
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Wl6d1sAAAAASUVORK5CYII=");
        using var body = Upload(png, "item.png", "image/png");

        using var response = await client.PostAsync("/api/recognition", body);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var unchanged = await client.GetFromJsonAsync<TripDto>($"/api/trips/{trip.Id}");
        Assert.Equal(0, unchanged!.PackedQuantity);
    }

    [Fact]
    public async Task InvalidImageBytesAreRejectedBeforeCallingAzure()
    {
        using var factory = new PackCheckApiFactory();
        using var client = factory.CreateClient();
        using var body = Upload("This is not an image"u8.ToArray(), "fake.png", "image/png");

        using var response = await client.PostAsync("/api/recognition", body);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    private static async Task<TripDto> CreateTripAsync(HttpClient client, string name = "Ardennes camping")
    {
        using var response = await client.PostAsJsonAsync("/api/trips", new CreateTripRequest
        {
            Name = name, DepartureDate = new DateTime(2026, 11, 8), TemplateKey = "camping"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var trip = await response.Content.ReadFromJsonAsync<TripDto>();
        Assert.NotNull(trip);
        Assert.True(trip.Id > 0);
        return trip;
    }

    private static MultipartFormDataContent Upload(byte[] bytes, string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var image = new ByteArrayContent(bytes);
        image.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(image, "image", fileName);
        return form;
    }
}
