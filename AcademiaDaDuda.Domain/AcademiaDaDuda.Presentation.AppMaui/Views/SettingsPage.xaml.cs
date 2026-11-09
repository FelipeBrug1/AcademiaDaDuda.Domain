using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class SettingsPage : ContentPage
{
    private bool _isLoadingSettings = true;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SettingsViewModel viewModel)
        {
            _isLoadingSettings = true;
            await viewModel.LoadSettingsCommand.ExecuteAsync(null);
            _isLoadingSettings = false;
        }
    }

    private void OnThemeSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isLoadingSettings || BindingContext is not SettingsViewModel viewModel || ThemePicker.SelectedIndex < 0)
            return;

        viewModel.ApplySelectedTheme();
    }
}
