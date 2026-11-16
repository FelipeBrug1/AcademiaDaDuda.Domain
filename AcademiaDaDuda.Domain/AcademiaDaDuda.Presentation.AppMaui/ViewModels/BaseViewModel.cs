// Felipe Antonio Brüggemann

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using AcademiaDaDuda.Presentation.AppMaui.Localization;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class BaseViewModel : ObservableObject, IRecipient<CulturePreferencesUpdatedMessage>
{
    private bool _isBusy;
    private bool _isRefreshing;
    private string _title = string.Empty;
    private string _titleResourceKey = string.Empty;
    private object?[] _titleArguments = [];

    public BaseViewModel() => WeakReferenceMessenger.Default.Register(this);

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string DatePickerFormat => LocalizationManager.Instance.CurrentCulture.DateTimeFormat.ShortDatePattern;

    protected void SetTitle(string resourceKey, params object?[] arguments)
    {
        _titleResourceKey = resourceKey;
        _titleArguments = arguments;
        RefreshTitle();
    }

    public void Receive(CulturePreferencesUpdatedMessage message)
    {
        RefreshTitle();
        OnCultureChanged();
    }

    protected virtual void OnCultureChanged() => OnPropertyChanged(string.Empty);

    private void RefreshTitle()
    {
        if (string.IsNullOrEmpty(_titleResourceKey))
            return;

        Title = _titleArguments.Length == 0
            ? LocalizationManager.Instance.Translate(_titleResourceKey)
            : LocalizationManager.Instance.Format(_titleResourceKey, _titleArguments);
    }
}
