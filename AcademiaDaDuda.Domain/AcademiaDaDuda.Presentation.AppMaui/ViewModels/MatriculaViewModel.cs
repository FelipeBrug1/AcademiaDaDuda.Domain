using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Converters;
using AcademiaDaDuda.Presentation.AppMaui.Services;
using AcademiaDaDuda.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LogradouroDto = AcademiaDaDuda.Application.DTOs.LogradouroDto;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    private const int TamanhoMaximoLaudoBytes = 15 * 1024 * 1024;

    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;
    private bool _initialized;
    private bool _suspendSelectionSync;
    private int _matriculaId;
    private bool _isEditMode;
    private MatriculaDto _matricula = NovaMatricula();

    public IReadOnlyList<AppMatriculaPlano> PlanoTipos { get; } = Enum.GetValues<AppMatriculaPlano>();
    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = [];

    public MatriculaDto Matricula
    {
        get => _matricula;
        set
        {
            if (!SetProperty(ref _matricula, value))
                return;

            SyncSelectionFromFlags();
            NotificarPropriedadesVisuais();
        }
    }

    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(SaveButtonText));
                OnPropertyChanged(nameof(StudentCpfIsReadOnly));
            }
        }
    }

    public string SaveButtonText => IsEditMode ? "Salvar alterações" : "Criar matrícula";
    public bool StudentCpfIsReadOnly => IsEditMode;
    public bool HasAlunoVinculado => Matricula.AlunoMatricula?.Id > 0;
    public bool HasNoAlunoVinculado => !HasAlunoVinculado;
    public ImageSource FotoAlunoSource => FotoToImageSourceConverter.CreateImageSource(Matricula.AlunoMatricula?.Foto);
    public bool IsAlunoEntre12E16 => Matricula.AlunoMatricula is { DataNascimento: var nascimento } && ExigeLaudoPorIdade(nascimento);
    public bool HasRestricoes => Matricula.RestricoesMedicas != AppMatriculaRestricoes.None;
    public bool ExigeLaudo => IsAlunoEntre12E16 || HasRestricoes;
    public bool HasLaudo => Matricula.LaudoMedico?.Conteudo is { Length: > 0 };
    public bool LaudoPendente => ExigeLaudo && !HasLaudo;
    public bool LaudoOpcional => !ExigeLaudo && !HasLaudo;

    public MatriculaViewModel(
        IMatriculaService matriculaService,
        IAlunoService alunoService,
        DatabaseSettingsStartup databaseSettingsStartup)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        _databaseSettingsStartup = databaseSettingsStartup;
        Title = "Nova matrícula";
        BuildRestricoesMulti();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar matrícula";
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova matrícula";
            Matricula = NovaMatricula();
            SyncSelectionFromFlags();
        }

        RecalcularDataFim();
        NotificarPropriedadesVisuais();
    }

    public void RecalcularDataFim()
    {
        var inicio = Matricula.DataInicio;
        if (inicio == default)
            return;

        Matricula.DataFim = Matricula.Plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };

        OnPropertyChanged(nameof(Matricula));
    }

    [RelayCommand]
    private Task CancelAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0 || IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var matricula = await _matriculaService.ObterPorIdAsync(MatriculaId, cancellation.Token);
            if (matricula is null)
            {
                await Shell.Current.DisplayAlertAsync("Matrícula não encontrada", "O registro solicitado não existe mais.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            matricula.LaudoMedicoRemovido = false;
            Matricula = matricula;
            Title = $"Matrícula {Matricula.Id}";
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "Não foi possível carregar a matrícula a tempo.", "OK");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao carregar", $"Não foi possível carregar a matrícula. {exception.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchByCpfAsync()
    {
        var cpf = DigitsOnly(Matricula.AlunoMatricula?.Cpf);
        if (cpf.Length != 11)
        {
            await Shell.Current.DisplayAlertAsync("CPF inválido", "Informe os 11 dígitos do CPF do aluno.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var aluno = await _alunoService.ObterPorCpfAsync(cpf, cancellation.Token);
            if (aluno is null)
            {
                await Shell.Current.DisplayAlertAsync("Aluno não encontrado", "Nenhum aluno foi localizado com esse CPF.", "OK");
                return;
            }

            Matricula.AlunoMatricula = aluno;
            OnPropertyChanged(nameof(Matricula));
            NotificarPropriedadesVisuais();
            await Shell.Current.DisplayAlertAsync("Aluno localizado", "Os dados e a foto do aluno foram carregados.", "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "A consulta do aluno demorou mais que o esperado.", "OK");
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
    private async Task SelecionarFotoAsync()
    {
        try
        {
            var origem = await Shell.Current.DisplayActionSheetAsync("Laudo médico", "Cancelar", null, "Galeria", "Câmera");
            FileResult? arquivo = origem switch
            {
                "Galeria" => await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o laudo médico",
                    FileTypes = FilePickerFileType.Images
                }),
                "Câmera" => await PhotoCaptureService.CapturePhotoAsync(),
                _ => null
            };

            if (arquivo is null)
                return;

            await using var source = await arquivo.OpenReadAsync();
            using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer);
            if (buffer.Length == 0 || buffer.Length > TamanhoMaximoLaudoBytes)
            {
                await Shell.Current.DisplayAlertAsync("Arquivo inválido", "Selecione uma imagem válida de até 15 MB.", "OK");
                return;
            }

            Matricula.LaudoMedico = new ArquivoDto { Conteudo = buffer.ToArray() };
            Matricula.LaudoMedicoRemovido = false;
            OnPropertyChanged(nameof(Matricula));
            NotificarPropriedadesVisuais();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Erro ao selecionar laudo", exception.Message, "OK");
        }
    }

    [RelayCommand]
    private void RemoverLaudo()
    {
        Matricula.LaudoMedico = null;
        Matricula.LaudoMedicoRemovido = IsEditMode;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    [RelayCommand]
    private async Task SaveMatriculaAsync()
    {
        if (IsBusy)
            return;

        RecomputeRestricoesFlags();
        RecalcularDataFim();
        if (!await ValidateMatriculaAsync())
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            AlunoDto? aluno = Matricula.AlunoMatricula.Id > 0
                ? await _alunoService.ObterPorIdAsync(Matricula.AlunoMatricula.Id, cancellation.Token)
                : await _alunoService.ObterPorCpfAsync(DigitsOnly(Matricula.AlunoMatricula.Cpf), cancellation.Token);

            if (aluno is null)
            {
                await Shell.Current.DisplayAlertAsync("Aluno não encontrado", "Busque um aluno cadastrado antes de salvar a matrícula.", "OK");
                return;
            }

            Matricula.AlunoMatricula = aluno;
            OnPropertyChanged(nameof(Matricula));
            NotificarPropriedadesVisuais();
            if (!await ValidateMatriculaAsync())
                return;

            if (!IsEditMode && await _matriculaService.PossuiMatriculaAtivaAsync(aluno.Id, cancellation.Token))
            {
                await Shell.Current.DisplayAlertAsync("Matrícula ativa", "Este aluno já possui uma matrícula ativa. Uma nova matrícula só poderá ser criada depois do encerramento da atual.", "OK");
                return;
            }

            Matricula.DataFim = CalcularDataFim(Matricula.Plano, Matricula.DataInicio);
            Matricula.ObservacoesRestricoes = Matricula.ObservacoesRestricoes?.Trim() ?? string.Empty;
            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula, cancellation.Token);
                await Shell.Current.DisplayAlertAsync("Concluído", "Matrícula atualizada com sucesso.", "OK");
            }
            else
            {
                await _matriculaService.AdicionarAsync(Matricula, cancellation.Token);
                await Shell.Current.DisplayAlertAsync("Concluído", "Matrícula criada com sucesso.", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "Não foi possível salvar a matrícula a tempo.", "OK");
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

    private async Task<bool> ValidateMatriculaAsync()
    {
        var errors = new List<string>();
        if (Matricula.AlunoMatricula is null || Matricula.AlunoMatricula.Id <= 0)
            errors.Add("Vincule um aluno cadastrado pelo CPF.");
        if (!Enum.IsDefined(Matricula.Plano))
            errors.Add("Selecione um plano válido.");
        if (Matricula.DataInicio == default)
            errors.Add("Informe a data de início.");
        if (string.IsNullOrWhiteSpace(Matricula.Objetivo))
            errors.Add("Informe o objetivo da matrícula.");
        if (ExigeLaudo && !HasLaudo)
            errors.Add(IsAlunoEntre12E16
                ? "Alunos de 12 a 16 anos precisam anexar um laudo médico."
                : "Alunos com restrições de saúde precisam anexar um parecer médico.");

        if (errors.Count == 0)
            return true;

        await Shell.Current.DisplayAlertAsync("Revise os dados", string.Join(Environment.NewLine, errors.Select(error => $"• {error}")), "OK");
        return false;
    }

    private void BuildRestricoesMulti()
    {
        foreach (var value in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (value == AppMatriculaRestricoes.None)
                continue;

            var item = new RestricaoSelectable
            {
                Value = value,
                IsSelected = Matricula.RestricoesMedicas.HasFlag(value)
            };
            item.PropertyChanged += RestricaoItem_PropertyChanged;
            RestricoesMulti.Add(item);
        }
    }

    private void RestricaoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RestricaoSelectable.IsSelected) && !_suspendSelectionSync)
            RecomputeRestricoesFlags();
    }

    private void RecomputeRestricoesFlags()
    {
        var flags = AppMatriculaRestricoes.None;
        foreach (var item in RestricoesMulti)
        {
            if (item.IsSelected)
                flags |= item.Value;
        }

        Matricula.RestricoesMedicas = flags;
        OnPropertyChanged(nameof(Matricula));
        NotificarPropriedadesVisuais();
    }

    private void SyncSelectionFromFlags()
    {
        _suspendSelectionSync = true;
        try
        {
            foreach (var item in RestricoesMulti)
                item.IsSelected = Matricula.RestricoesMedicas.HasFlag(item.Value);
        }
        finally
        {
            _suspendSelectionSync = false;
        }
    }

    private void NotificarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(HasAlunoVinculado));
        OnPropertyChanged(nameof(HasNoAlunoVinculado));
        OnPropertyChanged(nameof(FotoAlunoSource));
        OnPropertyChanged(nameof(IsAlunoEntre12E16));
        OnPropertyChanged(nameof(HasRestricoes));
        OnPropertyChanged(nameof(ExigeLaudo));
        OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(LaudoPendente));
        OnPropertyChanged(nameof(LaudoOpcional));
    }

    private static bool ExigeLaudoPorIdade(DateOnly nascimento)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return nascimento > hoje.AddYears(-17) && nascimento <= hoje.AddYears(-12);
    }

    private static DateOnly CalcularDataFim(AppMatriculaPlano plano, DateOnly inicio) => plano switch
    {
        AppMatriculaPlano.Mensal => inicio.AddMonths(1),
        AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
        AppMatriculaPlano.Semestral => inicio.AddMonths(6),
        AppMatriculaPlano.Anual => inicio.AddYears(1),
        _ => inicio
    };

    private static string DigitsOnly(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    private static MatriculaDto NovaMatricula()
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return new MatriculaDto
        {
            AlunoMatricula = new AlunoDto
            {
                Nome = string.Empty,
                Cpf = string.Empty,
                DataNascimento = hoje.AddYears(-18),
                Telefone = string.Empty,
                Endereco = new LogradouroDto
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = "Brasil"
                },
                Numero = string.Empty
            },
            Plano = AppMatriculaPlano.Anual,
            DataInicio = hoje,
            DataFim = hoje.AddYears(1),
            Objetivo = string.Empty,
            RestricoesMedicas = AppMatriculaRestricoes.None,
            ObservacoesRestricoes = string.Empty
        };
    }
}
