// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AcademiaDaDuda.Presentation.AppMaui.Localization;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    public ObservableCollection<LogradouroDto> Logradouros { get; } = new();
    public ObservableCollection<LocalizedOption> TiposDeBusca { get; } =
        [new("Todos"), new("Cidade"), new("ID"), new("CEP")];

    private string _textoBusca = string.Empty;
    private LocalizedOption _tipoDeBusca = null!;

    public string TextoBusca
    {
        get => _textoBusca;
        set => SetProperty(ref _textoBusca, value);
    }

    public LocalizedOption TipoDeBusca
    {
        get => _tipoDeBusca;
        set => SetProperty(ref _tipoDeBusca, value);
    }

    public LogradouroListViewModel(ILogradouroService logradouroService, DatabaseSettingsStartup databaseSettingsStartup)
    {
        _logradouroService = logradouroService;
        _databaseSettingsStartup = databaseSettingsStartup;
        TipoDeBusca = TiposDeBusca[0];
        SetTitle("Logradouros");
    }

    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            ReplaceItems(await _logradouroService.ObterTodosAsync(cancellation.Token));
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "O carregamento demorou mais que o esperado. Tente novamente.", "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro", global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizationManager.Instance.Format("Não foi possível carregar os logradouros. {0}", exception.Message), "OK");
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
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var term = TextoBusca.Trim();
            IEnumerable<LogradouroDto> results;

            switch (TipoDeBusca.Key)
            {
                case "ID":
                    if (!int.TryParse(term, out var id) || id <= 0)
                    {
                        await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Busca", "Informe um ID numérico maior que zero.", "OK");
                        return;
                    }
                    var byId = await _logradouroService.ObterPorIdAsync(id, cancellation.Token);
                    results = byId is null ? [] : [byId];
                    break;

                case "CEP":
                    var cep = DigitsOnly(term);
                    if (cep.Length != 8)
                    {
                        await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Busca", "Informe os 8 dígitos do CEP.", "OK");
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
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "A busca demorou mais que o esperado. Tente novamente.", "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro na busca", exception.Message, "OK");
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

        var confirmed = await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync(
            "Excluir logradouro",
            global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizationManager.Instance.Format("Deseja excluir “{0}”, CEP {1}?", logradouro.Nome, logradouro.Cep),
            "Excluir",
            "Cancelar");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            if (await _logradouroService.RemoverAsync(logradouro.Id, cancellation.Token))
            {
                Logradouros.Remove(logradouro);
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Concluído", "Logradouro excluído.", "OK");
            }
            else
            {
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Não encontrado", "Esse logradouro já foi removido.", "OK");
                await LoadLogradourosAsync();
            }
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "A exclusão demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            if (HasForeignKeyError(exception))
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Exclusão não permitida", "Este logradouro está vinculado a um aluno ou colaborador e não pode ser removido.", "OK");
            else
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro ao excluir", exception.Message, "OK");
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

