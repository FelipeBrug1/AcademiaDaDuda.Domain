using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;
    private List<MatriculaDto> _todasMatriculas = [];
    private string _searchText = string.Empty;
    private string _selectedFilterType = "Aluno";

    public ObservableCollection<MatriculaDto> Matriculas { get; } = [];
    public ObservableCollection<string> FilterTypes { get; } = ["Aluno", "Plano", "Objetivo", "ID", "ID do aluno", "Ativas", "Vencendo em dias"];

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

    public MatriculaListViewModel(
        IMatriculaService matriculaService,
        DatabaseSettingsStartup databaseSettingsStartup)
    {
        _matriculaService = matriculaService;
        _databaseSettingsStartup = databaseSettingsStartup;
        Title = "Matrículas";
    }

    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            _todasMatriculas = (await _matriculaService.ObterTodasAsync(cancellation.Token)).ToList();
            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O carregamento das matrículas demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao carregar", $"Não foi possível carregar as matrículas. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        var term = SearchText.Trim();
        if (RequiresNumber(SelectedFilterType) && !string.IsNullOrWhiteSpace(term) && !int.TryParse(term, out _))
        {
            await Shell.Current.DisplayAlertAsync("Busca de matrícula", "Informe um número inteiro válido para esse filtro.", "OK");
            return;
        }

        if ((SelectedFilterType is "ID" or "ID do aluno" or "Ativas") &&
            !string.IsNullOrWhiteSpace(term) && int.TryParse(term, out var positiveValue) && positiveValue <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Busca de matrícula", "Informe um identificador maior que zero.", "OK");
            return;
        }

        if (SelectedFilterType == "Vencendo em dias" && string.IsNullOrWhiteSpace(term))
        {
            await Shell.Current.DisplayAlertAsync("Busca por vencimento", "Informe em quantos dias a matrícula deve vencer.", "OK");
            return;
        }

        if (SelectedFilterType == "Vencendo em dias" && int.TryParse(term, out var days) && days < 0)
        {
            await Shell.Current.DisplayAlertAsync("Busca por vencimento", "Informe zero ou um número positivo de dias.", "OK");
            return;
        }

        ApplyFilter();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        IsRefreshing = true;
        await LoadMatriculasAsync();
    }

    [RelayCommand]
    private Task AddMatriculaAsync() => Shell.Current.GoToAsync(nameof(MatriculaPage));

    [RelayCommand]
    private Task EditMatriculaAsync(MatriculaDto? matricula) =>
        matricula is null ? Task.CompletedTask : Shell.Current.GoToAsync($"{nameof(MatriculaPage)}?Id={matricula.Id}");

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto? matricula)
    {
        if (matricula is null || IsBusy)
            return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Excluir matrícula",
            $"Deseja realmente excluir a matrícula de {matricula.AlunoMatricula.Nome}?",
            "Excluir",
            "Cancelar");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            if (await _matriculaService.RemoverAsync(matricula.Id, cancellation.Token))
            {
                _todasMatriculas.RemoveAll(item => item.Id == matricula.Id);
                ApplyFilter();
                await Shell.Current.DisplayAlertAsync("Concluído", "Matrícula excluída.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Não encontrada", "Essa matrícula já foi removida.", "OK");
                await LoadMatriculasAsync();
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A exclusão demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao excluir", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var term = SearchText.Trim();
        IEnumerable<MatriculaDto> results = _todasMatriculas;
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        switch (SelectedFilterType)
        {
            case "Plano":
                if (!string.IsNullOrWhiteSpace(term))
                    results = results.Where(matricula => Contains(matricula.Plano.GetDisplayName(), term));
                break;
            case "Objetivo":
                if (!string.IsNullOrWhiteSpace(term))
                    results = results.Where(matricula => Contains(matricula.Objetivo, term));
                break;
            case "ID":
                if (int.TryParse(term, out var matriculaId))
                    results = results.Where(matricula => matricula.Id == matriculaId);
                break;
            case "ID do aluno":
                if (int.TryParse(term, out var alunoId))
                    results = results.Where(matricula => matricula.AlunoMatricula.Id == alunoId);
                break;
            case "Ativas":
                results = results.Where(matricula => matricula.DataInicio <= hoje && matricula.DataFim >= hoje);
                if (int.TryParse(term, out var activeStudentId) && activeStudentId > 0)
                    results = results.Where(matricula => matricula.AlunoMatricula.Id == activeStudentId);
                break;
            case "Vencendo em dias":
                if (int.TryParse(term, out var days) && days >= 0)
                {
                    var limit = hoje.AddDays(days);
                    results = results.Where(matricula =>
                        matricula.DataInicio <= hoje && matricula.DataFim >= hoje && matricula.DataFim <= limit);
                }
                else
                {
                    results = [];
                }
                break;
            default:
                if (!string.IsNullOrWhiteSpace(term))
                    results = results.Where(matricula => Contains(matricula.AlunoMatricula.Nome, term));
                break;
        }

        Matriculas.Clear();
        foreach (var matricula in results.OrderByDescending(item => item.DataInicio))
            Matriculas.Add(matricula);
    }

    private static bool RequiresNumber(string filterType) => filterType is "ID" or "ID do aluno" or "Ativas" or "Vencendo em dias";

    private static bool Contains(string? value, string term) =>
        value?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true;
}
