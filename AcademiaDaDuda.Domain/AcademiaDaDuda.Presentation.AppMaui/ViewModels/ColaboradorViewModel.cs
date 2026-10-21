using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Converters;
using AcademiaDaDuda.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;
using LogradouroDto = AcademiaDaDuda.Application.DTOs.LogradouroDto;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(ColaboradorId), "Id")]
public partial class ColaboradorViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;
    private readonly ILogradouroService _logradouroService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    private ColaboradorDto _colaborador = NewColaborador();
    private int _colaboradorId;
    private bool _isEditMode;
    private bool _initialized;
    private string _confirmarSenha = string.Empty;

    public IEnumerable<AppColaboradorTipo> ColaboradorTipos { get; } = Enum.GetValues<AppColaboradorTipo>();
    public IEnumerable<AppColaboradorVinculo> ColaboradorVinculos { get; } = Enum.GetValues<AppColaboradorVinculo>();

    public ColaboradorDto Colaborador
    {
        get => _colaborador;
        set
        {
            if (SetProperty(ref _colaborador, value))
            {
                NotifyAddressState();
                OnPropertyChanged(nameof(FotoSource));
            }
        }
    }

    public ImageSource FotoSource => FotoToImageSourceConverter.CreateImageSource(Colaborador.Foto);

    public int ColaboradorId
    {
        get => _colaboradorId;
        set => SetProperty(ref _colaboradorId, value);
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

    public string SaveButtonText => IsEditMode ? "Salvar alterações" : "Cadastrar colaborador";

    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public bool HasEnderecoVinculado => Colaborador.Endereco?.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public ColaboradorViewModel(
        IColaboradorService colaboradorService,
        ILogradouroService logradouroService,
        DatabaseSettingsStartup databaseSettingsStartup)
    {
        _colaboradorService = colaboradorService;
        _logradouroService = logradouroService;
        _databaseSettingsStartup = databaseSettingsStartup;
        Title = "Novo colaborador";
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        ConfirmarSenha = string.Empty;
        if (ColaboradorId > 0)
        {
            IsEditMode = true;
            Title = "Editar colaborador";
            await LoadColaboradorAsync();
            return;
        }

        IsEditMode = false;
        Title = "Novo colaborador";
        Colaborador = NewColaborador();
        NotifyAddressState();
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadColaboradorAsync()
    {
        if (ColaboradorId <= 0 || IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var existing = await _colaboradorService.ObterPorIdAsync(ColaboradorId, cancellation.Token);
            if (existing is null)
            {
                await Shell.Current.DisplayAlertAsync("Não encontrado", "O colaborador solicitado não existe mais.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            existing.Endereco ??= NewEndereco();
            existing.Senha = string.Empty;
            Colaborador = existing;
            ConfirmarSenha = string.Empty;
            NotifyAddressState();
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "Não foi possível carregar os dados do colaborador a tempo.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao carregar", $"Não foi possível carregar o colaborador. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        var cpf = PessoaFormValidation.DigitsOnly(Colaborador.Cpf);
        if (cpf.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync("CPF inválido", "Informe os 11 dígitos do CPF para consultar.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var found = await _colaboradorService.ObterPorCpfAsync(cpf, cancellation.Token);
            if (found is null)
            {
                await Shell.Current.DisplayAlertAsync("Colaborador não encontrado", "Nenhum cadastro foi localizado com esse CPF.", "OK");
                return;
            }

            var fullRecord = await _colaboradorService.ObterPorIdAsync(found.Id, cancellation.Token);
            if (fullRecord is null)
            {
                await Shell.Current.DisplayAlertAsync("Colaborador não encontrado", "Não foi possível carregar os dados completos desse cadastro.", "OK");
                return;
            }

            fullRecord.Endereco ??= NewEndereco();
            fullRecord.Senha = string.Empty;
            ColaboradorId = fullRecord.Id;
            IsEditMode = true;
            Title = "Editar colaborador";
            Colaborador = fullRecord;
            ConfirmarSenha = string.Empty;
            NotifyAddressState();
            await Shell.Current.DisplayAlertAsync("Cadastro localizado", "Os dados foram carregados para edição.", "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A busca por CPF demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro na busca", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCepAsync()
    {
        var cep = PessoaFormValidation.DigitsOnly(Colaborador.Endereco?.Cep);
        if (cep.Length != 8)
        {
            await Shell.Current.DisplayAlertAsync("CEP inválido", "Informe os 8 dígitos do CEP para consultar.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var address = await _logradouroService.ObterPorCepAsync(cep, cancellation.Token);
            if (address is null)
            {
                await Shell.Current.DisplayAlertAsync("Logradouro não encontrado", "Cadastre esse CEP na opção Logradouros antes de salvar o colaborador.", "OK");
                return;
            }

            Colaborador.Endereco = address;
            OnPropertyChanged(nameof(Colaborador));
            NotifyAddressState();
            await Shell.Current.DisplayAlertAsync("CEP encontrado", "O endereço cadastrado foi vinculado ao colaborador.", "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A consulta do CEP demorou mais que o esperado.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro na consulta", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectPhotoAsync()
    {
        try
        {
            var choice = await Shell.Current.DisplayActionSheetAsync("Foto do colaborador", "Cancelar", null, "Galeria", "Câmera");
            FileResult? photo = choice switch
            {
                "Galeria" => await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione uma foto",
                    FileTypes = FilePickerFileType.Images
                }),
                "Câmera" => await PhotoCaptureService.CapturePhotoAsync(),
                _ => null
            };

            if (photo is null)
                return;

            await using var source = await photo.OpenReadAsync();
            using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer);
            Colaborador.Foto = new ArquivoDto { Conteudo = buffer.ToArray() };
            OnPropertyChanged(nameof(FotoSource));
            OnPropertyChanged(nameof(Colaborador));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao selecionar foto", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SaveColaboradorAsync()
    {
        if (IsBusy)
            return;

        var errors = PessoaFormValidation.Validate(Colaborador, IsEditMode, ConfirmarSenha);
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (Colaborador.DataAdmissao == default || Colaborador.DataAdmissao > today)
            errors.Add("Data de admissão deve ser preenchida e não pode ser futura.");
        if (Colaborador.Tipo == AppColaboradorTipo.Administrador && Colaborador.Vinculo != AppColaboradorVinculo.CLT)
            errors.Add("Colaborador com tipo Administrador deve ter vínculo CLT.");

        if (errors.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync("Revise os dados", string.Join(Environment.NewLine, errors.Select(error => $"• {error}")), "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var cep = PessoaFormValidation.DigitsOnly(Colaborador.Endereco?.Cep);
            var address = await _logradouroService.ObterPorCepAsync(cep, cancellation.Token);
            if (address is null)
            {
                await Shell.Current.DisplayAlertAsync("Endereço obrigatório", "O CEP informado não está cadastrado em Logradouros. Cadastre-o antes de salvar o colaborador.", "OK");
                return;
            }

            Colaborador.Endereco = address;
            Colaborador.Id = ColaboradorId;
            Colaborador.Nome = Colaborador.Nome.Trim();
            Colaborador.Cpf = PessoaFormValidation.DigitsOnly(Colaborador.Cpf);
            Colaborador.Telefone = PessoaFormValidation.DigitsOnly(Colaborador.Telefone);
            Colaborador.Email = Colaborador.Email?.Trim() ?? string.Empty;
            Colaborador.Numero = Colaborador.Numero.Trim();
            Colaborador.Complemento = string.IsNullOrWhiteSpace(Colaborador.Complemento) ? null : Colaborador.Complemento.Trim();
            Colaborador.Senha = string.IsNullOrWhiteSpace(Colaborador.Senha) ? null : Colaborador.Senha.Trim();

            if (IsEditMode)
            {
                await _colaboradorService.AtualizarAsync(Colaborador, cancellation.Token);
                await Shell.Current.DisplayAlertAsync("Concluído", "Colaborador atualizado com sucesso.", "OK");
            }
            else
            {
                await _colaboradorService.AdicionarAsync(Colaborador, cancellation.Token);
                await Shell.Current.DisplayAlertAsync("Concluído", "Colaborador cadastrado com sucesso.", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "Não foi possível salvar o colaborador a tempo.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao salvar", exception.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyAddressState()
    {
        OnPropertyChanged(nameof(HasEnderecoVinculado));
        OnPropertyChanged(nameof(HasNoEnderecoVinculado));
    }

    private static ColaboradorDto NewColaborador() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Endereco = NewEndereco(),
        Numero = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty,
        DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
        Tipo = AppColaboradorTipo.Atendente,
        Vinculo = AppColaboradorVinculo.CLT
    };

    private static LogradouroDto NewEndereco() => new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = "Brasil"
    };
}
