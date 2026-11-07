using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PackCheck.Messages;

public sealed class ApiAddressChangedMessage(string address) : ValueChangedMessage<string>(address);
