// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnEditLogradouroClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not LogradouroListViewModel viewModel ||
            sender is not Button { BindingContext: LogradouroDto logradouro })
            return;

        await viewModel.EditLogradouroCommand.ExecuteAsync(logradouro);
    }

    private async void OnDeleteLogradouroClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not LogradouroListViewModel viewModel ||
            sender is not Button { BindingContext: LogradouroDto logradouro })
            return;

        await viewModel.DeleteLogradouroCommand.ExecuteAsync(logradouro);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LogradouroListViewModel viewModel)
            await viewModel.LoadLogradourosCommand.ExecuteAsync(null);
    }
}
