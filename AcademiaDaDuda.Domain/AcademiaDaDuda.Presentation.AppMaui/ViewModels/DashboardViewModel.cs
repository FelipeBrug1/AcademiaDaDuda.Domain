// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcademiaDaDuda.Presentation.AppMaui.Localization;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    private int _totalLogradouros;
    private int _totalAlunos;
    private int _totalColaboradores;
    private int _totalMatriculas;
    private int _totalCadastros;
    private DateTime? _lastUpdatedAt;

    public int TotalLogradouros { get => _totalLogradouros; private set => SetProperty(ref _totalLogradouros, value); }
    public int TotalAlunos { get => _totalAlunos; private set => SetProperty(ref _totalAlunos, value); }
    public int TotalColaboradores { get => _totalColaboradores; private set => SetProperty(ref _totalColaboradores, value); }
    public int TotalMatriculas { get => _totalMatriculas; private set => SetProperty(ref _totalMatriculas, value); }
    public int TotalCadastros { get => _totalCadastros; private set => SetProperty(ref _totalCadastros, value); }
    public string UltimaAtualizacao => _lastUpdatedAt is { } updatedAt
        ? LocalizationManager.Instance.Format("Atualizado em {0:g}", updatedAt)
        : LocalizationManager.Instance.Translate("Ainda não atualizado");

    public DashboardViewModel(
        ILogradouroService logradouroService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IMatriculaService matriculaService,
        DatabaseSettingsStartup databaseSettingsStartup)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        _databaseSettingsStartup = databaseSettingsStartup;
        SetTitle("Dashboard");
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            var logradouros = await _logradouroService.ObterTodosAsync(cancellation.Token);
            var alunos = await _alunoService.ObterTodosAsync(cancellation.Token);
            var colaboradores = await _colaboradorService.ObterTodosAsync(cancellation.Token);
            var matriculas = await _matriculaService.ObterTodasAsync(cancellation.Token);

            TotalLogradouros = logradouros.Count();
            TotalAlunos = alunos.Count();
            TotalColaboradores = colaboradores.Count();
            TotalMatriculas = matriculas.Count();
            TotalCadastros = TotalLogradouros + TotalAlunos + TotalColaboradores + TotalMatriculas;
            _lastUpdatedAt = DateTime.Now;
            OnPropertyChanged(nameof(UltimaAtualizacao));
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "Não foi possível atualizar o resumo dentro do tempo esperado.", "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro ao carregar", global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizationManager.Instance.Format("Não foi possível carregar o dashboard. {0}", exception.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task OpenLogradourosAsync() => Shell.Current.GoToAsync("//logradouros");

    [RelayCommand]
    private Task OpenAlunosAsync() => Shell.Current.GoToAsync("//alunos");

    [RelayCommand]
    private Task OpenColaboradoresAsync() => Shell.Current.GoToAsync("//colaboradores");

    [RelayCommand]
    private Task OpenMatriculasAsync() => Shell.Current.GoToAsync("//matriculas");

    [RelayCommand]
    private Task OpenAcessoAsync() => Shell.Current.GoToAsync("//acesso");
}
