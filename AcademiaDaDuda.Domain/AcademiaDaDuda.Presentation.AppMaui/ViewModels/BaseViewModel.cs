using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private bool _isRefreshing;
    private string _title = string.Empty;

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
}
