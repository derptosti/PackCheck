using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PackCheck.Messages;

public sealed class TripCreatedMessage(int tripId) : ValueChangedMessage<int>(tripId);
