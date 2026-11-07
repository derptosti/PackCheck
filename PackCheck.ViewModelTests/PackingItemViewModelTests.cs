using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Services;
using PackCheck.ViewModels;
using Xunit;

namespace PackCheck.ViewModelTests;

public class PackingItemViewModelTests
{
    [Fact]
    public async Task FailedInitialReadBlocksExistingItemSaveAndDeleteEvenIfNameIsEntered()
    {
        var items = new FakeItems { ReadError = new InvalidOperationException("Connection unavailable.") };
        var dialogs = new FakeDialogs();
        var viewModel = CreateViewModel(items, dialogs);
        viewModel.Initialize(new ItemNavigationParameter(7, 42));
        await viewModel.AppearingCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.True(viewModel.NeedsItemLoad);
        Assert.False(viewModel.CanSave);
        viewModel.Name = "Renamed torch";
        await viewModel.SaveCommand.ExecuteAsync(null);
        await viewModel.DeleteCommand.ExecuteAsync(null);

        Assert.Empty(items.Updates);
        Assert.Equal(0, items.CreateCount);
        Assert.Equal(0, items.DeleteCount);
        Assert.Equal(0, dialogs.ConfirmationCount);
        Assert.Contains("Load the existing item", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task RetryLoadsOriginalFieldsBeforeAnExistingItemCanBeSaved()
    {
        var items = new FakeItems { ReadError = new InvalidOperationException("Connection unavailable.") };
        var viewModel = CreateViewModel(items);
        viewModel.Initialize(new ItemNavigationParameter(7, 42));
        await viewModel.AppearingCommand.ExecuteAsync(null);
        viewModel.Name = "This value must not overwrite unloaded fields";
        items.ReadError = null;
        await viewModel.RetryLoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.CanSave);
        Assert.False(viewModel.NeedsItemLoad);
        Assert.False(viewModel.HasError);
        Assert.Equal("Torch", viewModel.Name);
        Assert.Equal("3", viewModel.RequiredQuantityText);
        Assert.Equal("2", viewModel.PackedQuantityText);
        Assert.Equal("torch", viewModel.SelectedCategory?.Key);
        Assert.False(viewModel.IsEssential);

        viewModel.Name = "Spare torches";
        await viewModel.SaveCommand.ExecuteAsync(null);
        var write = Assert.Single(items.Updates);
        Assert.Equal("Spare torches", write.Name);
        Assert.Equal(3, write.RequiredQuantity);
        Assert.Equal(2, write.PackedQuantity);
        Assert.Equal("torch", write.CategoryKey);
        Assert.False(write.IsEssential);
    }

    [Fact]
    public async Task NewItemCanBeCreatedWithoutLoadingAnExistingEntry()
    {
        var items = new FakeItems { ReadError = new InvalidOperationException("Reads should not run for a new item.") };
        var viewModel = CreateViewModel(items);
        viewModel.Initialize(new ItemNavigationParameter(7));
        await viewModel.AppearingCommand.ExecuteAsync(null);

        Assert.True(viewModel.CanSave);
        Assert.False(viewModel.NeedsItemLoad);
        Assert.False(viewModel.CanDelete);
        viewModel.Name = "Sleeping bag";
        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.Equal(0, items.ReadCount);
        Assert.Equal(1, items.CreateCount);
        Assert.Empty(items.Updates);
        Assert.False(viewModel.HasError);
    }

    private static PackingItemViewModel CreateViewModel(FakeItems items, FakeDialogs? dialogs = null) =>
        new(items, new FakeNavigation(), dialogs ?? new FakeDialogs(), new WeakReferenceMessenger());

    private sealed class FakeDialogs : IDialogService
    {
        public int ConfirmationCount { get; private set; }
        public Task<bool> ConfirmAsync(string title, string message)
        {
            ConfirmationCount++;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeItems : IPackingItemDataService
    {
        public Exception? ReadError { get; set; }
        public int ReadCount { get; private set; }
        public int CreateCount { get; private set; }
        public int DeleteCount { get; private set; }
        public List<SavePackingItemRequest> Updates { get; } = [];
        private static PackingItemDto OriginalItem() => new()
        {
            Id = 42, TripId = 7, Name = "Torch", RequiredQuantity = 3,
            PackedQuantity = 2, CategoryKey = "torch", IsEssential = false
        };

        public Task<PackingItemDto> GetAsync(int tripId, int itemId)
        {
            ReadCount++;
            return ReadError is null ? Task.FromResult(OriginalItem()) : Task.FromException<PackingItemDto>(ReadError);
        }
        public Task<List<PackingItemDto>> GetAllAsync(int tripId) => throw new NotSupportedException();
        public Task<PackingItemDto> CreateAsync(int tripId, SavePackingItemRequest request)
        {
            CreateCount++;
            return Task.FromResult(OriginalItem());
        }
        public Task<PackingItemDto> UpdateAsync(int tripId, int itemId, SavePackingItemRequest request)
        {
            Updates.Add(request);
            return Task.FromResult(OriginalItem());
        }
        public Task DeleteAsync(int tripId, int itemId)
        {
            DeleteCount++;
            return Task.CompletedTask;
        }
    }
}
