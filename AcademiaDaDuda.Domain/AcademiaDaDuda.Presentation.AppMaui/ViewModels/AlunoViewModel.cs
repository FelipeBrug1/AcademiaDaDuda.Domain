using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Converters;
using AcademiaDaDuda.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;

    private AlunoDto _aluno = NewAluno();
    private int _alunoId;
    private bool _isEditMode;
    private bool _initialized;
    private string _confirmarSenha = string.Empty;

    public AlunoDto Aluno
    {
        get => _aluno;
        set
        {
            if (SetProperty(ref _aluno, value))
            {
                NotifyAddressState();
                OnPropertyChanged(nameof(FotoSource));
            }
        }
    }

    public ImageSource FotoSource => FotoToImageSourceConverter.CreateImageSource(Aluno.Foto);

    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
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

    public string SaveButtonText => IsEditMode ? "Salvar alterações" : "Cadastrar aluno";

    public string ConfirmarSenha
    {
        get => _confirmarSenha;
        set => SetProperty(ref _confirmarSenha, value);
    }

    public bool HasEnderecoVinculado => Aluno.Endereco?.Id > 0;
    public bool HasNoEnderecoVinculado => !HasEnderecoVinculado;

    public AlunoViewModel(
        IAlunoService alunoService,
        ILogradouroService logradouroService,
        DatabaseSettingsStartup databaseSettingsStartup)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;
        _databaseSettingsStartup = databaseSettingsStartup;
        Title = "Novo aluno";
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        ConfirmarSenha = string.Empty;
        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar aluno";
            await LoadAlunoAsync();
            return;
        }

        IsEditMode = false;
        Title = "Novo aluno";
        Aluno = NewAluno();
        NotifyAddressState();
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadAlunoAsync()
    {
        if (AlunoId <= 0 || IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var existing = await _alunoService.ObterPorIdAsync(AlunoId, cancellation.Token);
            if (existing is null)
            {
                await Shell.Current.DisplayAlertAsync("Não encontrado", "O aluno solicitado não existe mais.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            existing.Endereco ??= NewEndereco();
            existing.Senha = string.Empty;
            Aluno = existing;
            ConfirmarSenha = string.Empty;
            NotifyAddressState();
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "Não foi possível carregar os dados do aluno a tempo.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao carregar", $"Não foi possível carregar o aluno. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        var cpf = PessoaFormValidation.DigitsOnly(Aluno.Cpf);
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
            var found = await _alunoService.ObterPorCpfAsync(cpf, cancellation.Token);
            if (found is null)
            {
                await Shell.Current.DisplayAlertAsync("Aluno não encontrado", "Nenhum cadastro foi localizado com esse CPF.", "OK");
                return;
            }

            var fullRecord = await _alunoService.ObterPorIdAsync(found.Id, cancellation.Token);
            if (fullRecord is null)
            {
                await Shell.Current.DisplayAlertAsync("Aluno não encontrado", "Não foi possível carregar os dados completos desse cadastro.", "OK");
                return;
            }

            fullRecord.Endereco ??= NewEndereco();
            fullRecord.Senha = string.Empty;
            AlunoId = fullRecord.Id;
            IsEditMode = true;
            Title = "Editar aluno";
            Aluno = fullRecord;
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
        var cep = PessoaFormValidation.DigitsOnly(Aluno.Endereco?.Cep);
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
                await Shell.Current.DisplayAlertAsync("Logradouro não encontrado", "Cadastre esse CEP na opção Logradouros antes de salvar o aluno.", "OK");
                return;
            }

            Aluno.Endereco = address;
            OnPropertyChanged(nameof(Aluno));
            NotifyAddressState();
            await Shell.Current.DisplayAlertAsync("CEP encontrado", "O endereço cadastrado foi vinculado ao aluno.", "OK");
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
            var choice = await Shell.Current.DisplayActionSheetAsync("Foto do aluno", "Cancelar", null, "Galeria", "Câmera");
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
            Aluno.Foto = new ArquivoDto { Conteudo = buffer.ToArray() };
            OnPropertyChanged(nameof(FotoSource));
            OnPropertyChanged(nameof(Aluno));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao selecionar foto", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SaveAlunoAsync()
    {
        if (IsBusy)
            return;

        var errors = PessoaFormValidation.Validate(Aluno, IsEditMode, ConfirmarSenha);
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

            var cep = PessoaFormValidation.DigitsOnly(Aluno.Endereco?.Cep);
            var address = await _logradouroService.ObterPorCepAsync(cep, cancellation.Token);
            if (address is null)
            {
                await Shell.Current.DisplayAlertAsync("Endereço obrigatório", "O CEP informado não está cadastrado em Logradouros. Cadastre-o antes de salvar o aluno.", "OK");
                return;
            }

            Aluno.Endereco = address;
            Aluno.Id = AlunoId;
            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = PessoaFormValidation.DigitsOnly(Aluno.Cpf);
            Aluno.Telefone = PessoaFormValidation.DigitsOnly(Aluno.Telefone);
            Aluno.Email = Aluno.Email?.Trim() ?? string.Empty;
            Aluno.Numero = Aluno.Numero.Trim();
            Aluno.Complemento = string.IsNullOrWhiteSpace(Aluno.Complemento) ? null : Aluno.Complemento.Trim();
            Aluno.Senha = string.IsNullOrWhiteSpace(Aluno.Senha) ? null : Aluno.Senha.Trim();

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno, cancellation.Token);
                await Shell.Current.DisplayAlertAsync("Concluído", "Aluno atualizado com sucesso.", "OK");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno, cancellation.Token);
                await Shell.Current.DisplayAlertAsync("Concluído", "Aluno cadastrado com sucesso.", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "Não foi possível salvar o aluno a tempo.", "OK");
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

    private static AlunoDto NewAluno() => new()
    {
        Nome = string.Empty,
        Cpf = string.Empty,
        DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
        Telefone = string.Empty,
        Email = string.Empty,
        Endereco = NewEndereco(),
        Numero = string.Empty,
        Complemento = string.Empty,
        Senha = string.Empty
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
