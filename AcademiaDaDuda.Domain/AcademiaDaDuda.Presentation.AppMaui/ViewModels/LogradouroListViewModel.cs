// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    public ObservableCollection<LogradouroDto> Logradouros { get; } = new();
    public ObservableCollection<string> TiposDeBusca { get; } = new() { "Todos", "Cidade", "ID", "CEP" };

    private string _textoBusca = string.Empty;
    private string _tipoDeBusca = "Todos";

    public string TextoBusca
    {
        get => _textoBusca;
        set => SetProperty(ref _textoBusca, value);
    }

    public string TipoDeBusca
    {
        get => _tipoDeBusca;
        set => SetProperty(ref _tipoDeBusca, value);
    }

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Logradouros";
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            ReplaceItems(await _logradouroService.ObterTodosAsync(cancellation.Token));
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O carregamento demorou mais que o esperado. Tente novamente.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Não foi possível carregar os logradouros. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchLogradourosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var term = TextoBusca.Trim();
            IEnumerable<LogradouroDto> results;

            switch (TipoDeBusca)
            {
                case "ID":
                    if (!int.TryParse(term, out var id) || id <= 0)
                    {
                        await Shell.Current.DisplayAlertAsync("Busca", "Informe um ID numérico maior que zero.", "OK");
                        return;
                    }
                    var byId = await _logradouroService.ObterPorIdAsync(id, cancellation.Token);
                    results = byId is null ? [] : [byId];
                    break;

                case "CEP":
                    var cep = DigitsOnly(term);
                    if (cep.Length != 8)
                    {
                        await Shell.Current.DisplayAlertAsync("Busca", "Informe os 8 dígitos do CEP.", "OK");
                        return;
                    }
                    var byCep = await _logradouroService.ObterPorCepAsync(cep, cancellation.Token);
                    results = byCep is null ? [] : [byCep];
                    break;

                case "Cidade":
                    results = string.IsNullOrWhiteSpace(term)
                        ? await _logradouroService.ObterTodosAsync(cancellation.Token)
                        : await _logradouroService.ObterPorCidadeAsync(term, cancellation.Token);
                    break;

                default:
                    var all = await _logradouroService.ObterTodosAsync(cancellation.Token);
                    results = string.IsNullOrWhiteSpace(term)
                        ? all
                        : all.Where(item =>
                            Contains(item.Nome, term) ||
                            Contains(item.Bairro, term) ||
                            Contains(item.Cidade, term) ||
                            Contains(item.Estado, term) ||
                            Contains(item.Pais, term) ||
                            item.Cep.Contains(DigitsOnly(term), StringComparison.OrdinalIgnoreCase));
                    break;
            }

            ReplaceItems(results);
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A busca demorou mais que o esperado. Tente novamente.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro na busca", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadLogradourosAsync();
    }

    [RelayCommand]
    private Task AddLogradouroAsync() =>
        Shell.Current.GoToAsync($"{nameof(LogradouroPage)}?Id=0");

    [RelayCommand]
    private Task EditLogradouroAsync(LogradouroDto? logradouro) =>
        logradouro is null
            ? Task.CompletedTask
            : Shell.Current.GoToAsync($"{nameof(LogradouroPage)}?Id={logradouro.Id}");

    [RelayCommand]
    private async Task DeleteLogradouroAsync(LogradouroDto? logradouro)
    {
        if (logradouro is null || IsBusy)
            return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Excluir logradouro",
            $"Deseja excluir “{logradouro.Nome}”, CEP {logradouro.Cep}?",
            "Excluir",
            "Cancelar");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            if (await _logradouroService.RemoverAsync(logradouro.Id, cancellation.Token))
            {
                Logradouros.Remove(logradouro);
                await Shell.Current.DisplayAlertAsync("Concluído", "Logradouro excluído.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Não encontrado", "Esse logradouro já foi removido.", "OK");
                await LoadLogradourosAsync();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A exclusão demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            if (HasForeignKeyError(exception))
                await Shell.Current.DisplayAlertAsync("Exclusão não permitida", "Este logradouro está vinculado a um aluno ou colaborador e não pode ser removido.", "OK");
            else
                await Shell.Current.DisplayAlertAsync("Erro ao excluir", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReplaceItems(IEnumerable<LogradouroDto> items)
    {
        Logradouros.Clear();
        foreach (var item in items)
            Logradouros.Add(item);
    }

    private static bool Contains(string value, string term) =>
        value.Contains(term, StringComparison.CurrentCultureIgnoreCase);

    private static string DigitsOnly(string value) =>
        new string(value.Where(char.IsDigit).ToArray());

    private static bool HasForeignKeyError(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

