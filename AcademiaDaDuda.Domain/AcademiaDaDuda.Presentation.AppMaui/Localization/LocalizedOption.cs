using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui.Localization;

public sealed class LocalizedOption : INotifyPropertyChanged, IRecipient<CulturePreferencesUpdatedMessage>
{
    public string Key { get; }
    public string DisplayName => LocalizationManager.Instance.Translate(Key);

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizedOption(string key)
    {
        Key = key;
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(CulturePreferencesUpdatedMessage message) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
}

public sealed record LanguageOption(string CultureName, string DisplayName);
