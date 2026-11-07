using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using PackCheck.ViewModels;
using Xunit;

namespace PackCheck.ViewModelTests;

public class TripsViewModelTests
{
    [Fact]
    public async Task TripCreatedAndPackingUpdatedMessagesInvalidateListForNextAppearance()
    {
        var service = new FakeTrips();
        var messenger = new WeakReferenceMessenger();
        var viewModel = new TripsViewModel(service, new FakeNavigation(), messenger);
        await viewModel.AppearingCommand.ExecuteAsync(null);
        await viewModel.AppearingCommand.ExecuteAsync(null);
        Assert.Equal(1, service.CallCount);

        messenger.Send(new TripCreatedMessage(2));
        await viewModel.AppearingCommand.ExecuteAsync(null);
        Assert.Equal(2, service.CallCount);

        messenger.Send(new PackingItemUpdatedMessage(2));
        await viewModel.AppearingCommand.ExecuteAsync(null);
        Assert.Equal(3, service.CallCount);
    }

    [Fact]
    public async Task ApiAddressChangeClearsOldTripsAndReloadsOnNextAppearance()
    {
        var service = new FakeTrips();
        var messenger = new WeakReferenceMessenger();
        var viewModel = new TripsViewModel(service, new FakeNavigation(), messenger);
        await viewModel.AppearingCommand.ExecuteAsync(null);
        Assert.Single(viewModel.Trips);

        messenger.Send(new ApiAddressChangedMessage("https://new-api.example/"));
        Assert.Empty(viewModel.Trips);
        service.NextTrips = [new() { Id = 99, Name = "New server trip" }];
        await viewModel.AppearingCommand.ExecuteAsync(null);
        Assert.Equal(99, Assert.Single(viewModel.Trips).Id);
    }

    [Fact]
    public async Task ReturningWhileOldServerLoadIsPendingDiscardsOldDataAndCompletesQueuedReload()
    {
        var pending = new TaskCompletionSource<List<TripDto>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new FakeTrips { PendingFirstResponse = pending.Task };
        var messenger = new WeakReferenceMessenger();
        var viewModel = new TripsViewModel(service, new FakeNavigation(), messenger);
        var firstAppearance = viewModel.AppearingCommand.ExecuteAsync(null);
        Assert.True(viewModel.IsBusy);

        messenger.Send(new ApiAddressChangedMessage("https://new-api.example/"));
        service.NextTrips = [new() { Id = 99, Name = "New server trip" }];
        // The page returns while its original appearance command is running.
        Assert.True(viewModel.AppearingCommand.CanExecute(null));
        await viewModel.AppearingCommand.ExecuteAsync(null);
        pending.SetResult([new() { Id = 1, Name = "Old server trip" }]);
        await firstAppearance;

        Assert.Equal(2, service.CallCount);
        Assert.Equal(99, Assert.Single(viewModel.Trips).Id);
        Assert.False(viewModel.IsBusy);
        Assert.False(viewModel.HasError);
    }

    private sealed class FakeTrips : ITripDataService
    {
        public int CallCount { get; private set; }
        public Task<List<TripDto>>? PendingFirstResponse { get; set; }
        public List<TripDto> NextTrips { get; set; } = [new() { Id = 1, Name = "Camping trip" }];
        public Task<List<TripDto>> GetAllAsync()
        {
            CallCount++;
            return CallCount == 1 && PendingFirstResponse is not null ? PendingFirstResponse : Task.FromResult(NextTrips);
        }
        public Task<TripDto> GetAsync(int id) => throw new NotSupportedException();
        public Task<TripDto> CreateAsync(CreateTripRequest request) => throw new NotSupportedException();
        public Task DeleteAsync(int id) => throw new NotSupportedException();
    }
}
