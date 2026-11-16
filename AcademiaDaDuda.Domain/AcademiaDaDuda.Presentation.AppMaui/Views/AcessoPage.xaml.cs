using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class AcessoPage : ContentPage
{
    public AcessoPage(AcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AcessoViewModel viewModel)
            await viewModel.CarregarCommand.ExecuteAsync(null);
    }
}
