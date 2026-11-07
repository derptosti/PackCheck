using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PackCheck.Messages;

public sealed class PackingItemUpdatedMessage(int tripId) : ValueChangedMessage<int>(tripId);
