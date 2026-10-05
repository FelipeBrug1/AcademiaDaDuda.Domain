// Felipe Antonio Brüggemann
using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    public ObservableCollection<string> FilterTypes { get; } = ["Cidade", "Id", "Cep"];
    public ObservableCollection<LogradouroDto> Logradouros { get; set; } = [];

    private string _searchText = string.Empty;
    public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

    private string _selectedFilterType = "Cidade";
    public string SelectedFilterType { get => _selectedFilterType; set => SetProperty(ref _selectedFilterType, value); }

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Logradouros";
    }

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        await Shell.Current.GoToAsync("logradouro");
    }

    [RelayCommand]
    private async Task EditLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro == null) return;
        await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadLogradourosAsync();
    }

    [RelayCommand]
    private async Task SearchLogradourosAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() => Logradouros.Clear());

            IEnumerable<LogradouroDto> resultados = [];
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _logradouroService.ObterTodosAsync(cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Cidade")
            {
                resultados = await _logradouroService.ObterPorCidadeAsync(SearchText.Trim(), cts.Token) ?? [];
            }
            else if (SelectedFilterType == "Id")
            {
                if (int.TryParse(SearchText.Trim(), out int id) && id > 0)
                {
                    var item = await _logradouroService.ObterPorIdAsync(id, cts.Token);
                    if (item != null) resultados = [item];
                }
            }
            else if (SelectedFilterType == "Cep")
            {
                var cepLimpo = new string([.. SearchText.Where(char.IsDigit)]);
                if (cepLimpo.Length == 8)
                {
                    var item = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                    if (item != null) resultados = [item];
                }
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados) Logradouros.Add(item);
                OnPropertyChanged(nameof(Logradouros));
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() => Logradouros.Clear());
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var lista = await _logradouroService.ObterTodosAsync(cts.Token);
            if (lista != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in lista) Logradouros.Add(item);
                    OnPropertyChanged(nameof(Logradouros));
                });
            }
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro == null) return;
        bool confirm = await Shell.Current.DisplayAlertAsync("Confirmar Exclusão", $"Deseja realmente excluir o logradouro {logradouro.Nome}?", "Sim", "Não");
        if (!confirm) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _logradouroService.RemoverAsync(logradouro.Id, cts.Token);
            if (success)
            {
                Logradouros.Remove(logradouro);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro excluído!", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}