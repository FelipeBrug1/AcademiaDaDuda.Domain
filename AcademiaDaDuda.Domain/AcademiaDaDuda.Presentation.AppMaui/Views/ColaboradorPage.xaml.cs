using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class ColaboradorPage : ContentPage
{
    public ColaboradorPage(ColaboradorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorViewModel viewModel)
            await viewModel.InitializeAsync();
    }

    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        if (SenhaEntry is null || ConfirmarSenhaEntry is null)
            return;

        SenhaEntry.IsPassword = !e.Value;
        ConfirmarSenhaEntry.IsPassword = !e.Value;
    }
}
