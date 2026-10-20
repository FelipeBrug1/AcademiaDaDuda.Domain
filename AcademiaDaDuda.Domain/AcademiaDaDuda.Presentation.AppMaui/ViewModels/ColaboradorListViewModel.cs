using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    public ObservableCollection<ColaboradorDto> Colaboradores { get; } = [];
    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "ID", "E-mail", "Tipo", "Vínculo"];

    private string _searchText = string.Empty;
    private string _selectedFilterType = "Nome";

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(IColaboradorService colaboradorService, DatabaseSettingsStartup databaseSettingsStartup)
    {
        _colaboradorService = colaboradorService;
        _databaseSettingsStartup = databaseSettingsStartup;
        Title = "Colaboradores";
    }

    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            ReplaceItems(await _colaboradorService.ObterTodosAsync(cancellation.Token));
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O carregamento dos colaboradores demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao carregar", $"Não foi possível carregar os colaboradores. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var all = await _colaboradorService.ObterTodosAsync(cancellation.Token);
            var term = SearchText.Trim();
            IEnumerable<ColaboradorDto> results = all;

            if (!string.IsNullOrWhiteSpace(term))
            {
                switch (SelectedFilterType)
                {
                    case "CPF":
                        var cpf = DigitsOnly(term);
                        if (cpf.Length != 11)
                        {
                            await Shell.Current.DisplayAlertAsync("Busca por CPF", "Informe os 11 dígitos do CPF.", "OK");
                            return;
                        }
                        results = all.Where(colaborador => DigitsOnly(colaborador.Cpf) == cpf);
                        break;

                    case "ID":
                        if (!int.TryParse(term, out var id) || id <= 0)
                        {
                            await Shell.Current.DisplayAlertAsync("Busca por ID", "Informe um número inteiro maior que zero.", "OK");
                            return;
                        }
                        results = all.Where(colaborador => colaborador.Id == id);
                        break;

                    case "E-mail":
                        results = all.Where(colaborador => Contains(colaborador.Email, term));
                        break;

                    case "Tipo":
                        var tipos = Enum.GetValues<AppColaboradorTipo>()
                            .Where(tipo => Contains(tipo.ToString(), term) || Contains(tipo.GetDisplayName(), term))
                            .ToHashSet();
                        if (tipos.Count == 0)
                        {
                            await Shell.Current.DisplayAlertAsync("Busca por tipo", "Informe Administrador, Atendente ou Instrutor.", "OK");
                            return;
                        }
                        results = all.Where(colaborador => tipos.Contains(colaborador.Tipo));
                        break;

                    case "Vínculo":
                        var vinculos = Enum.GetValues<AppColaboradorVinculo>()
                            .Where(vinculo => Contains(vinculo.ToString(), term) || Contains(vinculo.GetDisplayName(), term))
                            .ToHashSet();
                        if (vinculos.Count == 0)
                        {
                            await Shell.Current.DisplayAlertAsync("Busca por vínculo", "Informe CLT ou Estagiário.", "OK");
                            return;
                        }
                        results = all.Where(colaborador => vinculos.Contains(colaborador.Vinculo));
                        break;

                    default:
                        results = all.Where(colaborador => Contains(colaborador.Nome, term));
                        break;
                }
            }

            ReplaceItems(results.OrderBy(colaborador => colaborador.Nome, StringComparer.CurrentCultureIgnoreCase));
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A busca demorou mais que o esperado.", "OK");
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
        if (IsBusy)
            return;

        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }

    [RelayCommand]
    private Task AddColaboradorAsync() => Shell.Current.GoToAsync(nameof(ColaboradorPage));

    [RelayCommand]
    private Task EditColaboradorAsync(ColaboradorDto? colaborador) =>
        colaborador is null ? Task.CompletedTask : Shell.Current.GoToAsync($"{nameof(ColaboradorPage)}?Id={colaborador.Id}");

    [RelayCommand]
    private async Task DeleteColaboradorAsync(ColaboradorDto? colaborador)
    {
        if (colaborador is null || IsBusy)
            return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Excluir colaborador",
            $"Deseja realmente excluir {colaborador.Nome}?",
            "Excluir",
            "Cancelar");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            if (await _colaboradorService.RemoverAsync(colaborador.Id, cancellation.Token))
            {
                Colaboradores.Remove(colaborador);
                await Shell.Current.DisplayAlertAsync("Concluído", "Colaborador excluído.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Não encontrado", "Esse colaborador já foi removido.", "OK");
                await LoadColaboradoresAsync();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A exclusão demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            if (HasForeignKeyError(exception))
                await Shell.Current.DisplayAlertAsync("Exclusão não permitida", "Este colaborador possui acessos ou registros vinculados e não pode ser excluído.", "OK");
            else
                await Shell.Current.DisplayAlertAsync("Erro ao excluir", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReplaceItems(IEnumerable<ColaboradorDto> items)
    {
        Colaboradores.Clear();
        foreach (var item in items)
            Colaboradores.Add(item);
    }

    private static bool Contains(string? value, string term) =>
        value?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true;

    private static string DigitsOnly(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

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
