using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Repositories;

namespace AcademiaDaDuda.Application.Services;

public sealed class AcessoService(
    Func<IAlunoRepository> alunoRepoFactory,
    Func<IColaboradorRepository> colaboradorRepoFactory,
    Func<IMatriculaRepository> matriculaRepoFactory,
    Func<IAcessoAlunoRepository> acessoAlunoRepoFactory,
    Func<IAcessoColaboradorRepository> acessoColaboradorRepoFactory) : IAcessoService
{
    private readonly SemaphoreSlim _registroLock = new(1, 1);

    public async Task<IEnumerable<AcessoPessoaDto>> ObterPessoasAsync(
        TipoPessoaAcesso tipoPessoa,
        CancellationToken cancellationToken = default)
    {
        return tipoPessoa switch
        {
            TipoPessoaAcesso.Aluno => (await alunoRepoFactory().ObterTodos(cancellationToken))
                .OrderBy(aluno => aluno.Nome, StringComparer.CurrentCultureIgnoreCase)
                .Select(aluno => new AcessoPessoaDto(aluno.Id, aluno.Nome, aluno.Cpf.Valor))
                .ToList(),
            TipoPessoaAcesso.Colaborador => (await colaboradorRepoFactory().ObterTodos(cancellationToken))
                .OrderBy(colaborador => colaborador.Nome, StringComparer.CurrentCultureIgnoreCase)
                .Select(colaborador => new AcessoPessoaDto(colaborador.Id, colaborador.Nome, colaborador.Cpf.Valor))
                .ToList(),
            _ => []
        };
    }

    public async Task<StatusAcessoDto?> ObterStatusAsync(
        TipoPessoaAcesso tipoPessoa,
        int pessoaId,
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        if (pessoaId <= 0)
            return null;

        return tipoPessoa switch
        {
            TipoPessoaAcesso.Aluno => await ObterStatusAlunoAsync(pessoaId, dataHora, cancellationToken),
            TipoPessoaAcesso.Colaborador => await ObterStatusColaboradorAsync(pessoaId, dataHora, cancellationToken),
            _ => null
        };
    }

    public async Task<IEnumerable<AcessoEventoDto>> ObterEventosDoDiaAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        var alunos = (await alunoRepoFactory().ObterTodos(cancellationToken)).ToDictionary(aluno => aluno.Id);
        var colaboradores = (await colaboradorRepoFactory().ObterTodos(cancellationToken)).ToDictionary(colaborador => colaborador.Id);
        var eventos = new List<AcessoEventoDto>();

        var acessosAlunos = await acessoAlunoRepoFactory().ObterAcessosPorAlunoPeriodo(
            cancellationToken: cancellationToken);
        AdicionarEventos(
            acessosAlunos,
            alunos,
            TipoPessoaAcesso.Aluno,
            data,
            acesso => acesso.AlunoId,
            acesso => acesso.Id,
            acesso => acesso.DataHora,
            aluno => (aluno.Nome, aluno.Cpf.Valor),
            eventos);

        var acessosColaboradores = await acessoColaboradorRepoFactory().ObterAcessosPorColaboradorPeriodo(
            cancellationToken: cancellationToken);
        AdicionarEventos(
            acessosColaboradores,
            colaboradores,
            TipoPessoaAcesso.Colaborador,
            data,
            acesso => acesso.ColaboradorId,
            acesso => acesso.Id,
            acesso => acesso.DataHora,
            colaborador => (colaborador.Nome, colaborador.Cpf.Valor),
            eventos);

        return eventos
            .OrderByDescending(evento => evento.DataHora)
            .ThenByDescending(evento => evento.Id)
            .ToList();
    }

    public async Task<ResultadoRegistroAcessoDto> RegistrarAcessoAsync(
        TipoPessoaAcesso tipoPessoa,
        int pessoaId,
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        await _registroLock.WaitAsync(cancellationToken);
        try
        {
            return tipoPessoa switch
            {
                TipoPessoaAcesso.Aluno => await RegistrarAcessoAlunoAsync(pessoaId, dataHora, cancellationToken),
                TipoPessoaAcesso.Colaborador => await RegistrarAcessoColaboradorAsync(pessoaId, dataHora, cancellationToken),
                _ => new(false, true, "TIPO_PESSOA_INVALIDO", string.Empty, dataHora)
            };
        }
        finally
        {
            _registroLock.Release();
        }
    }

    private async Task<StatusAcessoDto?> ObterStatusAlunoAsync(int alunoId, DateTime dataHora, CancellationToken cancellationToken)
    {
        var aluno = await alunoRepoFactory().ObterPorId(alunoId, cancellationToken);
        if (aluno is null)
            return null;

        var acessos = await acessoAlunoRepoFactory().ObterAcessosPorAlunoPeriodo(
            alunoId, cancellationToken: cancellationToken);
        var cronologicos = OrdenarCronologicamente(acessos, acesso => acesso.DataHora, acesso => acesso.Id)
            .Where(acesso => acesso.DataHora <= dataHora)
            .ToList();
        var estaNaAcademia = cronologicos.Count % 2 != 0;
        var matricula = await matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        var data = DateOnly.FromDateTime(dataHora);
        int? diasRestantes = matricula is null
            ? null
            : Math.Max(0, matricula.DataFim.DayNumber - data.DayNumber);

        return new StatusAcessoDto(
            estaNaAcademia,
            matricula is not null,
            estaNaAcademia ? cronologicos[^1].DataHora : null,
            diasRestantes);
    }

    private async Task<StatusAcessoDto?> ObterStatusColaboradorAsync(int colaboradorId, DateTime dataHora, CancellationToken cancellationToken)
    {
        var colaborador = await colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken);
        if (colaborador is null)
            return null;

        var acessos = await acessoColaboradorRepoFactory().ObterAcessosPorColaboradorPeriodo(
            colaboradorId, cancellationToken: cancellationToken);
        var cronologicos = OrdenarCronologicamente(acessos, acesso => acesso.DataHora, acesso => acesso.Id)
            .Where(acesso => acesso.DataHora <= dataHora)
            .ToList();
        var estaNaAcademia = cronologicos.Count % 2 != 0;

        return new StatusAcessoDto(
            estaNaAcademia,
            null,
            estaNaAcademia ? cronologicos[^1].DataHora : null,
            null);
    }

    private async Task<ResultadoRegistroAcessoDto> RegistrarAcessoAlunoAsync(
        int alunoId,
        DateTime dataHora,
        CancellationToken cancellationToken)
    {
        var aluno = await alunoRepoFactory().ObterPorId(alunoId, cancellationToken);
        if (aluno is null)
            return new(false, true, "PESSOA_NAO_ENCONTRADA", string.Empty, dataHora);

        var status = await ObterStatusAlunoAsync(alunoId, dataHora, cancellationToken);
        var entrada = !(status?.EstaNaAcademia ?? false);
        var matricula = await matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);

        if (entrada && matricula is null)
            return new(false, true, "ALUNO_SEM_MATRICULA", aluno.Nome, dataHora);

        var novoAcesso = AcessoAluno.Criar(0, aluno, dataHora);
        if (novoAcesso.IsFailure)
            return new(false, entrada, novoAcesso.Notifications.First().Mensagem, aluno.Nome, dataHora);

        await acessoAlunoRepoFactory().Adicionar(novoAcesso.Value!, cancellationToken);
        int? diasRestantes = entrada && matricula is not null
            ? Math.Max(0, matricula.DataFim.DayNumber - DateOnly.FromDateTime(dataHora).DayNumber)
            : null;
        TimeSpan? permanencia = !entrada && status?.UltimaEntrada is { } ultimaEntrada
            ? dataHora - ultimaEntrada
            : null;

        return new(true, entrada, entrada ? "ENTRADA_REGISTRADA" : "SAIDA_REGISTRADA", aluno.Nome, dataHora, diasRestantes, permanencia);
    }

    private async Task<ResultadoRegistroAcessoDto> RegistrarAcessoColaboradorAsync(
        int colaboradorId,
        DateTime dataHora,
        CancellationToken cancellationToken)
    {
        var colaborador = await colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken);
        if (colaborador is null)
            return new(false, true, "PESSOA_NAO_ENCONTRADA", string.Empty, dataHora);

        var status = await ObterStatusColaboradorAsync(colaboradorId, dataHora, cancellationToken);
        var entrada = !(status?.EstaNaAcademia ?? false);
        var novoAcesso = AcessoColaborador.Criar(0, colaborador, dataHora);
        if (novoAcesso.IsFailure)
            return new(false, entrada, novoAcesso.Notifications.First().Mensagem, colaborador.Nome, dataHora);

        await acessoColaboradorRepoFactory().Adicionar(novoAcesso.Value!, cancellationToken);
        TimeSpan? permanencia = !entrada && status?.UltimaEntrada is { } ultimaEntrada
            ? dataHora - ultimaEntrada
            : null;

        return new(true, entrada, entrada ? "ENTRADA_REGISTRADA" : "SAIDA_REGISTRADA", colaborador.Nome, dataHora, Permanencia: permanencia);
    }

    private static void AdicionarEventos<TAcesso, TPessoa>(
        IEnumerable<TAcesso> acessos,
        IReadOnlyDictionary<int, TPessoa> pessoas,
        TipoPessoaAcesso tipoPessoa,
        DateOnly data,
        Func<TAcesso, int> idPessoa,
        Func<TAcesso, int> idAcesso,
        Func<TAcesso, DateTime> dataHora,
        Func<TPessoa, (string Nome, string Cpf)> identidade,
        ICollection<AcessoEventoDto> destino)
        where TPessoa : class
    {
        foreach (var grupo in acessos.GroupBy(idPessoa))
        {
            if (!pessoas.TryGetValue(grupo.Key, out var pessoa))
                continue;

            var (nome, cpf) = identidade(pessoa);
            var cronologicos = grupo
                .OrderBy(dataHora)
                .ThenBy(idAcesso)
                .ToList();

            for (var indice = 0; indice < cronologicos.Count; indice++)
            {
                var acesso = cronologicos[indice];
                if (DateOnly.FromDateTime(dataHora(acesso)) != data)
                    continue;

                destino.Add(new AcessoEventoDto(
                    idAcesso(acesso),
                    grupo.Key,
                    nome,
                    cpf,
                    tipoPessoa,
                    dataHora(acesso),
                    Entrada: indice % 2 == 0));
            }
        }
    }

    private static List<TAcesso> OrdenarCronologicamente<TAcesso>(
        IEnumerable<TAcesso> acessos,
        Func<TAcesso, DateTime> dataHora,
        Func<TAcesso, int> id) => acessos
        .OrderBy(dataHora)
        .ThenBy(id)
        .ToList();
}
