using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Presentation.AppMaui.ViewModels;

namespace AcademiaDaDuda.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    private CancellationTokenSource? _searchCts;
    private bool _isPageReady;

    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
            _isPageReady = true;
            if (!string.IsNullOrWhiteSpace(viewModel.SearchText))
                await viewModel.SearchColaboradoresCommand.ExecuteAsync(null);
        }
    }

    protected override void OnDisappearing()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = null;
        _isPageReady = false;
        base.OnDisappearing();
    }

    private async void OnSearchDebounceTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isPageReady)
            return;

        _searchCts?.Cancel();
        _searchCts?.Dispose();
        var search = new CancellationTokenSource();
        _searchCts = search;

        try
        {
            await Task.Delay(300, search.Token);
            if (!search.IsCancellationRequested && BindingContext is ColaboradorListViewModel viewModel)
                await viewModel.SearchColaboradoresCommand.ExecuteAsync(null);
        }
        catch (TaskCanceledException)
        {
            // Uma nova tecla reiniciou o intervalo do debounce.
        }
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
