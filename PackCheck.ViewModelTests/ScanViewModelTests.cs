using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using PackCheck.ViewModels;
using Xunit;

namespace PackCheck.ViewModelTests;

public class ScanViewModelTests
{
    [Fact]
    public async Task ChoosingAndClassifyingPhotoDoesNotWriteToChecklist()
    {
        var fixture = new ScanFixture();
        await fixture.Scan.PickCommand.ExecuteAsync(null);

        Assert.True(fixture.Scan.HasPhoto);
        Assert.True(fixture.Scan.HasResult);
        Assert.Equal("torch", fixture.Scan.SelectedCategory?.Key);
        Assert.Equal(42, fixture.Scan.SelectedItem?.Id);
        Assert.True(fixture.Scan.ConfirmCommand.CanExecute(null));
        Assert.Equal(1, fixture.Recognition.CallCount);
        Assert.Empty(fixture.Items.Writes);
        Assert.Empty(fixture.Recipient.TripIds);
    }

    [Fact]
    public async Task ExplicitConfirmationPacksExactlyOneAndSendsRefreshMessage()
    {
        var fixture = new ScanFixture();
        await fixture.Scan.PickCommand.ExecuteAsync(null);
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);

        var write = Assert.Single(fixture.Items.Writes);
        Assert.Equal(7, write.TripId);
        Assert.Equal(42, write.ItemId);
        Assert.Equal(1, write.Request.PackedQuantity);
        Assert.Equal(3, write.Request.RequiredQuantity);
        Assert.Equal("torch", write.Request.CategoryKey);
        Assert.Equal("Torch", write.Request.Name);
        Assert.True(write.Request.IsEssential);
        Assert.Equal([7], fixture.Recipient.TripIds);
        Assert.False(fixture.Scan.ConfirmCommand.CanExecute(null));
        Assert.Contains("Packed one Torch", fixture.Scan.ConfirmationMessage);
    }

    [Fact]
    public async Task SamePhotoCannotBeConfirmedTwiceEvenWhenMoreQuantityIsNeeded()
    {
        var fixture = new ScanFixture();
        await fixture.Scan.PickCommand.ExecuteAsync(null);
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);
        // Invoke the command directly as well as checking CanExecute. The guard
        // must protect the operation even if a caller bypasses the UI state.
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);

        Assert.Single(fixture.Items.Writes);
        Assert.Equal(1, fixture.Items.StoredItem.PackedQuantity);
        Assert.Single(fixture.Recipient.TripIds);
    }

    [Fact]
    public async Task NewPhotoAllowsNextExplicitConfirmation()
    {
        var fixture = new ScanFixture();
        await fixture.Scan.PickCommand.ExecuteAsync(null);
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);
        await fixture.Scan.PickCommand.ExecuteAsync(null);
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);

        Assert.Equal(2, fixture.Items.Writes.Count);
        Assert.Equal(2, fixture.Items.StoredItem.PackedQuantity);
        Assert.Equal(2, fixture.Recipient.TripIds.Count);
    }

    [Theory]
    [InlineData("torch", 0.40, "Low confidence")]
    [InlineData("Negative", 0.98, "did not recognize")]
    public async Task UncertainOrNegativePredictionRequiresManualCategoryCorrection(string key, double probability, string warning)
    {
        var fixture = new ScanFixture();
        fixture.Recognition.Result = new RecognitionResultDto
        {
            ConfidenceThreshold = 0.65,
            Predictions =
            [
                new() { CategoryKey = key, DisplayName = key, Probability = probability },
                new() { CategoryKey = "torch", DisplayName = "Torch", Probability = 0.20 }
            ]
        };
        await fixture.Scan.PickCommand.ExecuteAsync(null);

        Assert.Null(fixture.Scan.SelectedCategory);
        Assert.False(fixture.Scan.ConfirmCommand.CanExecute(null));
        Assert.Contains(warning, fixture.Scan.ConfidenceMessage);
        Assert.Empty(fixture.Items.Writes);

        fixture.Scan.SelectedCategory = ItemCategories.All.Single(category => category.Key == "torch");
        Assert.True(fixture.Scan.ConfirmCommand.CanExecute(null));
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);
        Assert.Single(fixture.Items.Writes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FreshCompletedOrRecategorizedServerItemCannotBeWritten(bool completed)
    {
        var fixture = new ScanFixture();
        await fixture.Scan.PickCommand.ExecuteAsync(null);
        // The DTO visible to the page is an earlier snapshot. Another caller
        // changes the server entry before the user confirms it.
        if (completed) fixture.Items.StoredItem.PackedQuantity = 3;
        else fixture.Items.StoredItem.CategoryKey = "backpack";
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);

        Assert.Empty(fixture.Items.Writes);
        Assert.Empty(fixture.Recipient.TripIds);
        Assert.Contains("entry changed", fixture.Scan.ErrorMessage);
        Assert.False(fixture.Scan.ConfirmCommand.CanExecute(null));
    }

    [Fact]
    public async Task AlreadyCompletedMatchingItemIsVisibleButCannotBeConfirmed()
    {
        var fixture = new ScanFixture();
        fixture.Items.StoredItem.PackedQuantity = 3;
        await fixture.Scan.PickCommand.ExecuteAsync(null);

        var completed = Assert.Single(fixture.Scan.MatchingItems);
        Assert.True(completed.IsPacked);
        fixture.Scan.SelectedItem = completed;
        Assert.False(fixture.Scan.ConfirmCommand.CanExecute(null));
        await fixture.Scan.ConfirmCommand.ExecuteAsync(null);
        Assert.Empty(fixture.Items.Writes);
    }

    [Fact]
    public async Task RecognitionFailureAppearsAsErrorWithoutWriting()
    {
        var fixture = new ScanFixture();
        fixture.Recognition.Error = new InvalidOperationException("Photo recognition is unavailable.");
        await fixture.Scan.PickCommand.ExecuteAsync(null);

        Assert.True(fixture.Scan.HasError);
        Assert.Equal("Photo recognition is unavailable.", fixture.Scan.ErrorMessage);
        Assert.False(fixture.Scan.IsBusy);
        Assert.False(fixture.Scan.HasResult);
        Assert.False(fixture.Scan.ConfirmCommand.CanExecute(null));
        Assert.Empty(fixture.Items.Writes);
    }

    [Fact]
    public async Task CancellingPhotoPickerDoesNotClassifyOrWrite()
    {
        var fixture = new ScanFixture();
        fixture.Photos.Photo = null;
        await fixture.Scan.PickCommand.ExecuteAsync(null);

        Assert.Equal(0, fixture.Recognition.CallCount);
        Assert.False(fixture.Scan.HasPhoto);
        Assert.False(fixture.Scan.HasResult);
        Assert.False(fixture.Scan.HasError);
        Assert.Empty(fixture.Items.Writes);
    }

    private sealed class ScanFixture
    {
        public FakePackingItems Items { get; } = new();
        public FakeRecognition Recognition { get; } = new();
        public FakePhotos Photos { get; } = new();
        public PackingUpdateRecipient Recipient { get; } = new();
        public ScanViewModel Scan { get; }

        public ScanFixture()
        {
            var messenger = new WeakReferenceMessenger();
            messenger.Register<PackingItemUpdatedMessage>(Recipient);
            Scan = new ScanViewModel(Items, Recognition, Photos, new FakeNavigation(), messenger);
            Scan.Initialize(7);
        }
    }

    public sealed class PackingUpdateRecipient : IRecipient<PackingItemUpdatedMessage>
    {
        public List<int> TripIds { get; } = [];
        public void Receive(PackingItemUpdatedMessage message) => TripIds.Add(message.Value);
    }

    private sealed class FakeRecognition : IRecognitionService
    {
        public int CallCount { get; private set; }
        public Exception? Error { get; set; }
        public RecognitionResultDto Result { get; set; } = new()
        {
            ConfidenceThreshold = 0.65,
            Predictions = [new() { CategoryKey = "torch", DisplayName = "Torch", Probability = 0.95 }]
        };

        public Task<RecognitionResultDto> ClassifyImageAsync(PhotoUpload photo)
        {
            CallCount++;
            return Error is null ? Task.FromResult(Result) : Task.FromException<RecognitionResultDto>(Error);
        }
    }

    private sealed class FakePhotos : IPhotoService
    {
        public PhotoUpload? Photo { get; set; } = new([1, 2, 3], "torch.jpg", "image/jpeg");
        public Task<PhotoUpload?> PickAsync() => Task.FromResult(Photo);
        public Task<PhotoUpload?> CaptureAsync() => Task.FromResult(Photo);
    }

    private sealed class FakePackingItems : IPackingItemDataService
    {
        public PackingItemDto StoredItem { get; } = new()
        {
            Id = 42, TripId = 7, Name = "Torch", CategoryKey = "torch", RequiredQuantity = 3, IsEssential = true
        };
        public List<(int TripId, int ItemId, SavePackingItemRequest Request)> Writes { get; } = [];

        public Task<List<PackingItemDto>> GetAllAsync(int tripId) => Task.FromResult(new List<PackingItemDto> { Snapshot() });
        public Task<PackingItemDto> GetAsync(int tripId, int itemId) => Task.FromResult(Snapshot());
        public Task<PackingItemDto> UpdateAsync(int tripId, int itemId, SavePackingItemRequest request)
        {
            Writes.Add((tripId, itemId, request));
            StoredItem.PackedQuantity = request.PackedQuantity;
            return Task.FromResult(Snapshot());
        }
        public Task<PackingItemDto> CreateAsync(int tripId, SavePackingItemRequest request) => throw new NotSupportedException();
        public Task DeleteAsync(int tripId, int itemId) => throw new NotSupportedException();

        private PackingItemDto Snapshot() => new()
        {
            Id = StoredItem.Id, TripId = StoredItem.TripId, Name = StoredItem.Name,
            CategoryKey = StoredItem.CategoryKey, RequiredQuantity = StoredItem.RequiredQuantity,
            PackedQuantity = StoredItem.PackedQuantity, IsEssential = StoredItem.IsEssential
        };
    }
}
