using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorListViewModel viewModel)
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
    }

    private async void OnEditColaboradorClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: ColaboradorDto colaborador } && BindingContext is ColaboradorListViewModel viewModel)
            await viewModel.EditColaboradorCommand.ExecuteAsync(colaborador);
    }

    private async void OnDeleteColaboradorClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: ColaboradorDto colaborador } && BindingContext is ColaboradorListViewModel viewModel)
            await viewModel.DeleteColaboradorCommand.ExecuteAsync(colaborador);
    }
}
