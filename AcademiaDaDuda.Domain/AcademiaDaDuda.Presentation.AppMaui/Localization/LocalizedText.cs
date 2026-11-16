using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

internal sealed class LocalizedText : INotifyPropertyChanged, IRecipient<CulturePreferencesUpdatedMessage>
{
    private readonly string _key;

    public string Value => LocalizationManager.Instance.Translate(_key);

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizedText(string key)
    {
        _key = key;
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(CulturePreferencesUpdatedMessage message) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
}
