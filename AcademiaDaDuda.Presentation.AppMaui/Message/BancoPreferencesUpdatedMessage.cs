// Felipe Antonio Brüggemann
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDaDuda.Presentation.AppMaui.Message;

public sealed class BancoPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value)
{
}