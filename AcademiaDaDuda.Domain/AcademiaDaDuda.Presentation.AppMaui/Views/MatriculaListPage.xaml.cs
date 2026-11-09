using AcademiaDaDuda.Presentation.AppMaui.ViewModels;
using AcademiaDaDuda.Application.DTOs;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class MatriculaListPage : ContentPage
{
    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaListViewModel viewModel)
            await viewModel.LoadMatriculasCommand.ExecuteAsync(null);
    }

    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: MatriculaDto matricula } && BindingContext is MatriculaListViewModel viewModel)
            await viewModel.EditMatriculaCommand.ExecuteAsync(matricula);
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: MatriculaDto matricula } && BindingContext is MatriculaListViewModel viewModel)
            await viewModel.DeleteMatriculaCommand.ExecuteAsync(matricula);
    }
}
