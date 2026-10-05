// Felipe Antonio Brüggemann
using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(LogradouroId), "Id")]
public partial class LogradouroViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    private LogradouroDto _logradouro = new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };

    public LogradouroDto Logradouro { get => _logradouro; set => SetProperty(ref _logradouro, value); }

    private int _logradouroId;
    public int LogradouroId { get => _logradouroId; set => SetProperty(ref _logradouroId, value); }

    private bool _isEditMode;
    public bool IsEditMode { get => _isEditMode; set => SetProperty(ref _isEditMode, value); }

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Detalhes do Logradouro";
    }

    public async Task InitializeAsync()
    {
        if (LogradouroId > 0)
        {
            IsEditMode = true;
            Title = "Editar Logradouro";
            await LoadLogradouroAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Logradouro";
            Logradouro = new LogradouroDto
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = "Brasil"
            };
        }
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadLogradouroAsync()
    {
        if (LogradouroId <= 0) return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var item = await _logradouroService.ObterPorIdAsync(LogradouroId, cts.Token);
            if (item != null) Logradouro = item;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        var apenasDigitos = new string([.. (Logradouro.Cep ?? "").Where(char.IsDigit)]);
        if (apenasDigitos.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("Validação", "O CEP deve ter 8 dígitos.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var item = await _logradouroService.ObterPorCepAsync(apenasDigitos, cts.Token);
            if (item != null)
            {
                Logradouro = item;
                LogradouroId = item.Id;
                IsEditMode = true;
                Title = "Editar Logradouro";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveLogradouroAsync()
    {
        if (IsBusy) return;
        if (!await ValidateAsync(Logradouro)) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            Logradouro.Cep = new string([.. Logradouro.Cep.Where(char.IsDigit)]);
            Logradouro.Estado = Logradouro.Estado.Trim().ToUpperInvariant();
            Logradouro.Nome = Logradouro.Nome.Trim();
            Logradouro.Bairro = Logradouro.Bairro.Trim();
            Logradouro.Cidade = Logradouro.Cidade.Trim();
            Logradouro.Pais = string.IsNullOrWhiteSpace(Logradouro.Pais) ? "Brasil" : Logradouro.Pais.Trim();

            if (IsEditMode)
                await _logradouroService.AtualizarAsync(Logradouro, cts.Token);
            else
                await _logradouroService.AdicionarAsync(Logradouro, cts.Token);

            await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro salvo!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<bool> ValidateAsync(LogradouroDto logradouro)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(logradouro.Cep)) errors.Add("• CEP é obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Nome)) errors.Add("• Nome/Rua é obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Bairro)) errors.Add("• Bairro é obrigatório.");
        if (string.IsNullOrWhiteSpace(logradouro.Cidade)) errors.Add("• Cidade é obrigatória.");
        if (string.IsNullOrWhiteSpace(logradouro.Estado) || logradouro.Estado.Trim().Length != 2) errors.Add("• UF deve ter 2 letras.");

        if (errors.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Validação", string.Join("\n", errors), "OK");
            return false;
        }
        return true;
    }
}