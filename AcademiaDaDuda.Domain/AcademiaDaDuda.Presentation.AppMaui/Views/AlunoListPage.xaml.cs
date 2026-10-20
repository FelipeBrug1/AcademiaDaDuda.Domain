using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class AlunoListPage : ContentPage
{
    public AlunoListPage(AlunoListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AlunoListViewModel viewModel)
            await viewModel.LoadAlunosCommand.ExecuteAsync(null);
    }

    private async void OnEditAlunoClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: AlunoDto aluno } && BindingContext is AlunoListViewModel viewModel)
            await viewModel.EditAlunoCommand.ExecuteAsync(aluno);
    }

    private async void OnDeleteAlunoClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: AlunoDto aluno } && BindingContext is AlunoListViewModel viewModel)
            await viewModel.DeleteAlunoCommand.ExecuteAsync(aluno);
    }
}
