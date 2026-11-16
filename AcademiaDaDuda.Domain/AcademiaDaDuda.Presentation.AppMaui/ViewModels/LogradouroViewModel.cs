// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AcademiaDaDuda.Presentation.AppMaui.Localization;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;
    private LogradouroDto _logradouro = NewLogradouro();
    private int _logradouroId;
    private bool _isEditMode;

    public LogradouroDto Logradouro
    {
        get => _logradouro;
        set => SetProperty(ref _logradouro, value);
    }

    public int LogradouroId
    {
        get => _logradouroId;
        set => SetProperty(ref _logradouroId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (SetProperty(ref _isEditMode, value))
                OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public string SaveButtonText => LocalizationManager.Instance.Translate(IsEditMode ? "Salvar alterações" : "Cadastrar logradouro");

    public LogradouroViewModel(ILogradouroService logradouroService, DatabaseSettingsStartup databaseSettingsStartup)
    {
        _logradouroService = logradouroService;
        _databaseSettingsStartup = databaseSettingsStartup;
        SetTitle("Novo logradouro");
    }

    public async Task InitializeAsync()
    {
        if (LogradouroId <= 0)
        {
            IsEditMode = false;
            SetTitle("Novo logradouro");
            Logradouro = NewLogradouro();
            return;
        }

        IsEditMode = true;
        SetTitle("Editar logradouro");
        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var existing = await _logradouroService.ObterPorIdAsync(LogradouroId, cancellation.Token);
            if (existing is null)
            {
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Não encontrado", "O logradouro solicitado não existe mais.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }
            Logradouro = existing;
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "Não foi possível carregar os dados do logradouro a tempo.", "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro", global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizationManager.Instance.Format("Não foi possível carregar o logradouro. {0}", exception.Message), "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        var cep = DigitsOnly(Logradouro.Cep);
        if (cep.Length != 8)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("CEP inválido", "Informe os 8 dígitos do CEP para consultar.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var existing = await _logradouroService.ObterPorCepAsync(cep, cancellation.Token);
            if (existing is null)
            {
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("CEP disponível", "Nenhum cadastro foi encontrado com esse CEP. Você pode continuar o cadastro.", "OK");
                return;
            }

            Logradouro = existing;
            LogradouroId = existing.Id;
            IsEditMode = true;
            SetTitle("Editar logradouro");
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("CEP já cadastrado", "Os dados foram carregados para edição.", "OK");
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "A consulta do CEP demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro na consulta", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLogradouroAsync()
    {
        if (IsBusy)
            return;

        var errors = Validate(Logradouro);
        if (errors.Count > 0)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Revise os dados", string.Join(Environment.NewLine, errors), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            Normalize(Logradouro);
            Logradouro.Id = LogradouroId;
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            if (IsEditMode)
            {
                await _logradouroService.AtualizarAsync(Logradouro, cancellation.Token);
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Concluído", "Logradouro atualizado.", "OK");
            }
            else
            {
                await _logradouroService.AdicionarAsync(Logradouro, cancellation.Token);
                await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Concluído", "Logradouro cadastrado.", "OK");
            }
            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Tempo esgotado", "Não foi possível salvar o logradouro a tempo.", "OK");
        }
        catch (InvalidOperationException exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Regra de negócio", exception.Message, "OK");
        }
        catch (ArgumentException exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Dados inválidos", exception.Message, "OK");
        }
        catch (Exception exception)
        {
            await global::AcademiaDaDuda.Presentation.AppMaui.Localization.LocalizedAlerts.DisplayAlertAsync("Erro ao salvar", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");

    private static List<string> Validate(LogradouroDto logradouro)
    {
        var errors = new List<string>();
        if (DigitsOnly(logradouro.Cep).Length != 8)
            errors.Add("• CEP: informe exatamente 8 dígitos.");
        if (string.IsNullOrWhiteSpace(logradouro.Nome))
            errors.Add("• Logradouro: campo obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Bairro))
            errors.Add("• Bairro: campo obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Cidade))
            errors.Add("• Cidade: campo obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Estado))
            errors.Add("• Estado: informe a sigla de 2 letras.");
        else
        {
            var state = logradouro.Estado.Trim();
            if (state.Length != 2 || !state.All(char.IsLetter))
                errors.Add("• Estado: informe uma sigla válida com 2 letras.");
        }
        if (string.IsNullOrWhiteSpace(logradouro.Pais))
            errors.Add("• País: campo obrigatório.");
        return errors;
    }

    private static void Normalize(LogradouroDto logradouro)
    {
        logradouro.Cep = DigitsOnly(logradouro.Cep);
        logradouro.Nome = logradouro.Nome.Trim();
        logradouro.Bairro = logradouro.Bairro.Trim();
        logradouro.Cidade = logradouro.Cidade.Trim();
        logradouro.Estado = logradouro.Estado.Trim().ToUpperInvariant();
        logradouro.Pais = string.IsNullOrWhiteSpace(logradouro.Pais) ? "Brasil" : logradouro.Pais.Trim();
    }

    private static string DigitsOnly(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    private static LogradouroDto NewLogradouro() => new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };
}
