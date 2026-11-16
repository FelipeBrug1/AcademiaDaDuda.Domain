using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AcademiaDaDuda.Presentation.AppMaui.Localization;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    public ObservableCollection<AlunoDto> Alunos { get; } = [];
    public ObservableCollection<LocalizedOption> FilterTypes { get; } =
        [new("Nome"), new("CPF"), new("ID"), new("E-mail")];

    private string _searchText = string.Empty;
    private LocalizedOption _selectedFilterType = null!;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public LocalizedOption SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public AlunoListViewModel(IAlunoService alunoService, DatabaseSettingsStartup databaseSettingsStartup)
    {
        _alunoService = alunoService;
        _databaseSettingsStartup = databaseSettingsStartup;
        SelectedFilterType = FilterTypes[0];
        SetTitle("Alunos");
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            ReplaceItems(await _alunoService.ObterTodosAsync(cancellation.Token));
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "O carregamento dos alunos demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro ao carregar", global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizationManager.Instance.Format("Não foi possível carregar os alunos. {0}", exception.Message), "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchAlunosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var all = await _alunoService.ObterTodosAsync(cancellation.Token);
            var term = SearchText.Trim();
            IEnumerable<AlunoDto> results = all;

            if (!string.IsNullOrWhiteSpace(term))
            {
                switch (SelectedFilterType.Key)
                {
                    case "CPF":
                        var cpf = DigitsOnly(term);
                        if (cpf.Length != 11)
                        {
                            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Busca por CPF", "Informe os 11 dígitos do CPF.", "OK");
                            return;
                        }
                        results = all.Where(aluno => DigitsOnly(aluno.Cpf) == cpf);
                        break;

                    case "ID":
                        if (!int.TryParse(term, out var id) || id <= 0)
                        {
                            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Busca por ID", "Informe um número inteiro maior que zero.", "OK");
                            return;
                        }
                        results = all.Where(aluno => aluno.Id == id);
                        break;

                    case "E-mail":
                        results = all.Where(aluno => Contains(aluno.Email, term));
                        break;

                    default:
                        results = all.Where(aluno => Contains(aluno.Nome, term));
                        break;
                }
            }

            ReplaceItems(results.OrderBy(aluno => aluno.Nome, StringComparer.CurrentCultureIgnoreCase));
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "A busca demorou mais que o esperado.", "OK");
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
        if (IsBusy)
            return;

        IsRefreshing = true;
        await LoadAlunosAsync();
    }

    [RelayCommand]
    private Task AddAlunoAsync() => Shell.Current.GoToAsync(nameof(AlunoPage));

    [RelayCommand]
    private Task EditAlunoAsync(AlunoDto? aluno) =>
        aluno is null ? Task.CompletedTask : Shell.Current.GoToAsync($"{nameof(AlunoPage)}?Id={aluno.Id}");

    [RelayCommand]
    private async Task DeleteAlunoAsync(AlunoDto? aluno)
    {
        if (aluno is null || IsBusy)
            return;

        var confirmed = await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync(
            "Excluir aluno",
            global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizationManager.Instance.Format("Deseja realmente excluir {0}?", aluno.Nome),
            "Excluir",
            "Cancelar");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            if (await _alunoService.RemoverAsync(aluno.Id, cancellation.Token))
            {
                Alunos.Remove(aluno);
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Concluído", "Aluno excluído.", "OK");
            }
            else
            {
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Não encontrado", "Esse aluno já foi removido.", "OK");
                await LoadAlunosAsync();
            }
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "A exclusão demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            if (HasForeignKeyError(exception))
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Exclusão não permitida", "Este aluno possui matrículas ou acessos vinculados e não pode ser excluído.", "OK");
            else
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro ao excluir", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReplaceItems(IEnumerable<AlunoDto> items)
    {
        Alunos.Clear();
        foreach (var item in items)
            Alunos.Add(item);
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
