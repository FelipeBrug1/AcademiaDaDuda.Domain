using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

public sealed class CulturePreferencesUpdatedMessage : ValueChangedMessage<string>
{
    public CulturePreferencesUpdatedMessage(string value) : base(value) { }
}
