using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;
    private List<MatriculaDto> _todasMatriculas = [];
    private string _searchText = string.Empty;

    public ObservableCollection<MatriculaDto> Matriculas { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
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
    private Task SearchMatriculasAsync()
    {
        ApplyFilter();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        IsRefreshing = true;
        await LoadMatriculasAsync();
    }

    private void ApplyFilter()
    {
        var term = SearchText.Trim();
        var results = string.IsNullOrWhiteSpace(term)
            ? _todasMatriculas
            : _todasMatriculas.Where(matricula =>
                Contains(matricula.AlunoMatricula.Nome, term) ||
                Contains(matricula.Plano.ToString(), term) ||
                Contains(matricula.Objetivo, term));

        Matriculas.Clear();
        foreach (var matricula in results.OrderByDescending(item => item.DataInicio))
            Matriculas.Add(matricula);
    }

    private static bool Contains(string? value, string term) =>
        value?.Contains(term, StringComparison.CurrentCultureIgnoreCase) == true;
}
