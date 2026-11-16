using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAcessoService _acessoService;
    private readonly DatabaseSettingsStartup _databaseSettingsStartup;
    private List<AcessoPessoaDto> _pessoasCarregadas = [];
    private IReadOnlyList<AcessoEventoDto> _eventosCarregados = [];

    [ObservableProperty]
    public partial TipoPessoaAcesso TipoPessoaSelecionado { get; set; } = TipoPessoaAcesso.Aluno;

    [ObservableProperty]
    public partial AcessoPessoaDto? PessoaSelecionada { get; set; }

    [ObservableProperty]
    public partial string TermoBusca { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool EstaNaAcademia { get; set; }

    [ObservableProperty]
    public partial bool? MatriculaAtiva { get; set; }

    [ObservableProperty]
    public partial int? DiasRestantesPlano { get; set; }

    [ObservableProperty]
    public partial string MensagemResultado { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ResultadoSucesso { get; set; }

    public ObservableCollection<AcessoPessoaDto> PessoasDisponiveis { get; } = [];
    public ObservableCollection<AcessoEventoLinha> EventosHoje { get; } = [];

    public bool SemEventosHoje => EventosHoje.Count == 0;
    public bool TemPessoaSelecionada => PessoaSelecionada is not null;
    public bool SemPessoaSelecionada => PessoaSelecionada is null;
    public bool TemMensagemResultado => !string.IsNullOrWhiteSpace(MensagemResultado);
    public bool StatusMatriculaVisivel => TipoPessoaSelecionado == TipoPessoaAcesso.Aluno && MatriculaAtiva.HasValue;
    public string PlaceholderBusca => Traduzir("Buscar por nome, CPF ou ID");
    public string PlaceholderPessoa => Traduzir(TipoPessoaSelecionado == TipoPessoaAcesso.Aluno ? "Selecione um aluno" : "Selecione um colaborador");
    public string EstadoModoAtual => Traduzir(TipoPessoaSelecionado == TipoPessoaAcesso.Aluno ? "Modo atual: aluno" : "Modo atual: colaborador");
    public Color CorModoAluno => TipoPessoaSelecionado == TipoPessoaAcesso.Aluno ? Color.FromArgb("#6C4CE5") : Color.FromArgb("#514A62");
    public Color CorModoColaborador => TipoPessoaSelecionado == TipoPessoaAcesso.Colaborador ? Color.FromArgb("#6C4CE5") : Color.FromArgb("#514A62");
    public string SituacaoAtual => PessoaSelecionada is null
        ? Traduzir("Selecione uma pessoa para consultar o acesso")
        : Traduzir(EstaNaAcademia ? "Dentro da academia" : "Fora da academia");
    public string DetalheSituacao => PessoaSelecionada is null
        ? string.Empty
        : TipoPessoaSelecionado == TipoPessoaAcesso.Aluno
            ? MatriculaAtiva == true
                ? Traduzir("Matrícula ativa") + (DiasRestantesPlano.HasValue ? $" · {Traduzir("Dias restantes do plano")}: {DiasRestantesPlano.Value}" : string.Empty)
                : Traduzir("Aluno sem matrícula ativa")
            : Traduzir("Colaborador selecionado");
    public string TextoAcaoAcesso => Traduzir(EstaNaAcademia ? "Registrar saída" : "Registrar entrada");
    public Color CorResultado => ResultadoSucesso ? Color.FromArgb("#087F5B") : Color.FromArgb("#B42318");

    public AcessoViewModel(IAcessoService acessoService, DatabaseSettingsStartup databaseSettingsStartup)
    {
        _acessoService = acessoService;
        _databaseSettingsStartup = databaseSettingsStartup;
        SetTitle("MenuAcesso");
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            var pessoaAnteriorId = PessoaSelecionada?.Id;
            _pessoasCarregadas = (await _acessoService.ObterPessoasAsync(TipoPessoaSelecionado)).ToList();
            AplicarFiltroPessoas(pessoaAnteriorId);
            await AtualizarEventosAsync();

            if (PessoaSelecionada is not null)
                await AtualizarStatusSelecionadoAsync();
        }
        catch (Exception exception)
        {
            MensagemResultado = LocalizationManager.Instance.Format("Não foi possível carregar os acessos. {0}", exception.Message);
            ResultadoSucesso = false;
        }
        finally
        {
            IsBusy = false;
            AtualizarPropriedadesVisuais();
        }
    }

    [RelayCommand]
    private async Task SelecionarAlunosAsync()
    {
        if (TipoPessoaSelecionado == TipoPessoaAcesso.Aluno)
            return;

        TipoPessoaSelecionado = TipoPessoaAcesso.Aluno;
        PessoaSelecionada = null;
        TermoBusca = string.Empty;
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task SelecionarColaboradoresAsync()
    {
        if (TipoPessoaSelecionado == TipoPessoaAcesso.Colaborador)
            return;

        TipoPessoaSelecionado = TipoPessoaAcesso.Colaborador;
        PessoaSelecionada = null;
        TermoBusca = string.Empty;
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync()
    {
        if (IsBusy || PessoaSelecionada is null)
            return;

        try
        {
            IsBusy = true;
            await _databaseSettingsStartup.EnsureInitializedAsync();
            var resultado = await _acessoService.RegistrarAcessoAsync(
                TipoPessoaSelecionado,
                PessoaSelecionada.Id,
                DateTime.Now);

            ResultadoSucesso = resultado.Registrado;
            MensagemResultado = CriarMensagemResultado(resultado);

            if (resultado.Registrado)
            {
                await AtualizarEventosAsync();
                await AtualizarStatusSelecionadoAsync();
            }
        }
        catch (Exception exception)
        {
            ResultadoSucesso = false;
            MensagemResultado = LocalizationManager.Instance.Format("Não foi possível registrar o acesso. {0}", exception.Message);
        }
        finally
        {
            IsBusy = false;
            AtualizarPropriedadesVisuais();
        }
    }

    private async Task AtualizarStatusSelecionadoAsync()
    {
        if (PessoaSelecionada is null)
        {
            EstaNaAcademia = false;
            MatriculaAtiva = null;
            DiasRestantesPlano = null;
            AtualizarPropriedadesVisuais();
            return;
        }

        var status = await _acessoService.ObterStatusAsync(
            TipoPessoaSelecionado,
            PessoaSelecionada.Id,
            DateTime.Now);

        EstaNaAcademia = status?.EstaNaAcademia ?? false;
        MatriculaAtiva = status?.MatriculaAtiva;
        DiasRestantesPlano = status?.DiasRestantesPlano;
        AtualizarPropriedadesVisuais();
    }

    private async Task AtualizarEventosAsync()
    {
        _eventosCarregados = (await _acessoService.ObterEventosDoDiaAsync(DateOnly.FromDateTime(DateTime.Today))).ToList();
        RecriarLinhasEventos();
    }

    private void AplicarFiltroPessoas(int? pessoaAnteriorId = null)
    {
        var busca = TermoBusca.Trim();
        var digitosBusca = new string(busca.Where(char.IsDigit).ToArray());
        var buscaPorId = int.TryParse(digitosBusca, out var idBusca);
        var filtradas = _pessoasCarregadas
            .Where(pessoa => string.IsNullOrEmpty(busca)
                || pessoa.Nome.Contains(busca, StringComparison.CurrentCultureIgnoreCase)
                || (buscaPorId && pessoa.Id == idBusca)
                || (!string.IsNullOrEmpty(digitosBusca) && pessoa.Cpf.Contains(digitosBusca, StringComparison.Ordinal)))
            .ToList();

        PessoasDisponiveis.Clear();
        foreach (var pessoa in filtradas)
            PessoasDisponiveis.Add(pessoa);

        if (pessoaAnteriorId.HasValue)
            PessoaSelecionada = filtradas.FirstOrDefault(pessoa => pessoa.Id == pessoaAnteriorId.Value);
        else if (PessoaSelecionada is not null && filtradas.All(pessoa => pessoa.Id != PessoaSelecionada.Id))
            PessoaSelecionada = null;

        OnPropertyChanged(nameof(PlaceholderPessoa));
    }

    private string CriarMensagemResultado(ResultadoRegistroAcessoDto resultado)
    {
        var localizer = LocalizationManager.Instance;
        if (!resultado.Registrado)
        {
            var motivo = resultado.Codigo switch
            {
                "ALUNO_SEM_MATRICULA" => localizer.Translate("Aluno sem matrícula ativa. A entrada não foi autorizada."),
                "PESSOA_NAO_ENCONTRADA" => localizer.Translate("Pessoa não encontrada."),
                _ => localizer.Translate("Não foi possível registrar o acesso.")
            };
            return localizer.Format("Acesso negado: {0}", motivo);
        }

        if (resultado.Entrada)
        {
            var mensagem = localizer.Format("Entrada registrada para {0}.", resultado.Nome);
            return resultado.DiasRestantesPlano.HasValue
                ? $"{mensagem} {localizer.Format("Dias restantes do plano: {0}.", resultado.DiasRestantesPlano.Value)}"
                : mensagem;
        }

        var permanencia = resultado.Permanencia?.ToString("c", LocalizationManager.Instance.CurrentCulture) ?? "00:00:00";
        return localizer.Format("Saída registrada para {0}. Permanência: {1}.", resultado.Nome, permanencia);
    }

    private void RecriarLinhasEventos()
    {
        EventosHoje.Clear();
        foreach (var evento in _eventosCarregados)
            EventosHoje.Add(new AcessoEventoLinha(evento));

        AtualizarPropriedadesVisuais();
    }

    private void AtualizarPropriedadesVisuais()
    {
        OnPropertyChanged(nameof(SemEventosHoje));
        OnPropertyChanged(nameof(TemPessoaSelecionada));
        OnPropertyChanged(nameof(SemPessoaSelecionada));
        OnPropertyChanged(nameof(TemMensagemResultado));
        OnPropertyChanged(nameof(StatusMatriculaVisivel));
        OnPropertyChanged(nameof(PlaceholderBusca));
        OnPropertyChanged(nameof(PlaceholderPessoa));
        OnPropertyChanged(nameof(EstadoModoAtual));
        OnPropertyChanged(nameof(CorModoAluno));
        OnPropertyChanged(nameof(CorModoColaborador));
        OnPropertyChanged(nameof(SituacaoAtual));
        OnPropertyChanged(nameof(DetalheSituacao));
        OnPropertyChanged(nameof(TextoAcaoAcesso));
        OnPropertyChanged(nameof(CorResultado));
    }

    partial void OnTipoPessoaSelecionadoChanged(TipoPessoaAcesso value) => AtualizarPropriedadesVisuais();

    partial void OnPessoaSelecionadaChanged(AcessoPessoaDto? value)
    {
        _ = AtualizarStatusSelecionadoComTratamentoAsync();
        AtualizarPropriedadesVisuais();
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltroPessoas();

    partial void OnEstaNaAcademiaChanged(bool value) => AtualizarPropriedadesVisuais();

    partial void OnMatriculaAtivaChanged(bool? value) => AtualizarPropriedadesVisuais();

    partial void OnDiasRestantesPlanoChanged(int? value) => AtualizarPropriedadesVisuais();

    partial void OnMensagemResultadoChanged(string value) => AtualizarPropriedadesVisuais();

    partial void OnResultadoSucessoChanged(bool value) => OnPropertyChanged(nameof(CorResultado));

    protected override void OnCultureChanged()
    {
        base.OnCultureChanged();
        AtualizarPropriedadesVisuais();
        RecriarLinhasEventos();
    }

    private async Task AtualizarStatusSelecionadoComTratamentoAsync()
    {
        try
        {
            await _databaseSettingsStartup.EnsureInitializedAsync();
            await AtualizarStatusSelecionadoAsync();
        }
        catch (Exception exception)
        {
            ResultadoSucesso = false;
            MensagemResultado = LocalizationManager.Instance.Format("Não foi possível consultar o acesso. {0}", exception.Message);
        }
    }

    private static string Traduzir(string chave) => LocalizationManager.Instance.Translate(chave);
}
