using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Services;
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Infrastructure.Repositories;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class AcessoServiceTests : TestBase
{
    [Fact(DisplayName = "Acesso: aluno sem matrícula ativa tem a entrada negada")]
    public async Task AlunoSemMatriculaAtiva_NaoRegistraAcesso()
    {
        var aluno = await CriarEInserirAlunoAsync("Aluno sem matrícula");
        var service = CriarService();

        var resultado = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Aluno,
            aluno.Id,
            DateTime.Today.AddHours(10),
            TestContext.Current.CancellationToken);

        Assert.False(resultado.Registrado);
        Assert.True(resultado.Entrada);
        Assert.Equal("ALUNO_SEM_MATRICULA", resultado.Codigo);
        Assert.Empty(await new AcessoAlunoRepository(ConnectionString, DatabaseType)
            .ObterAcessosPorAlunoPeriodo(aluno.Id, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today), TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Acesso: aluno com matrícula ativa registra entrada e saída")]
    public async Task AlunoComMatriculaAtiva_RegistraEntradaESaida()
    {
        var aluno = await CriarEInserirAlunoAsync("Aluno com matrícula");
        var matriculaRepo = new MatriculaRepository(ConnectionString, DatabaseType);
        var matricula = Matricula.Criar(
            0,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Condicionamento físico",
            MatriculaRestricoes.None,
            null).Value!;
        await matriculaRepo.Adicionar(matricula, TestContext.Current.CancellationToken);

        var service = CriarService();
        var entrada = DateTime.Today.AddHours(10);
        var saida = entrada.AddHours(2);

        var resultadoEntrada = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Aluno, aluno.Id, entrada, TestContext.Current.CancellationToken);
        var resultadoSaida = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Aluno, aluno.Id, saida, TestContext.Current.CancellationToken);

        Assert.True(resultadoEntrada.Registrado);
        Assert.True(resultadoEntrada.Entrada);
        Assert.Equal("ENTRADA_REGISTRADA", resultadoEntrada.Codigo);
        Assert.NotNull(resultadoEntrada.DiasRestantesPlano);
        Assert.True(resultadoSaida.Registrado);
        Assert.False(resultadoSaida.Entrada);
        Assert.Equal("SAIDA_REGISTRADA", resultadoSaida.Codigo);
        Assert.Equal(TimeSpan.FromHours(2), resultadoSaida.Permanencia);

        var eventos = (await service.ObterEventosDoDiaAsync(
                DateOnly.FromDateTime(DateTime.Today), TestContext.Current.CancellationToken))
            .Where(evento => evento.PessoaId == aluno.Id)
            .OrderBy(evento => evento.DataHora)
            .ToList();
        Assert.Collection(eventos,
            evento => Assert.True(evento.Entrada),
            evento => Assert.False(evento.Entrada));
    }

    [Fact(DisplayName = "Acesso: colaborador registra entrada e saída sem matrícula")]
    public async Task Colaborador_RegistraEntradaESaida()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        var service = CriarService();
        var entrada = DateTime.Today.AddHours(9);

        var resultadoEntrada = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Colaborador, colaborador.Id, entrada, TestContext.Current.CancellationToken);
        var resultadoSaida = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Colaborador, colaborador.Id, entrada.AddHours(1), TestContext.Current.CancellationToken);

        Assert.True(resultadoEntrada.Registrado);
        Assert.True(resultadoEntrada.Entrada);
        Assert.True(resultadoSaida.Registrado);
        Assert.False(resultadoSaida.Entrada);
        Assert.Equal(TimeSpan.FromHours(1), resultadoSaida.Permanencia);
    }

    [Fact(DisplayName = "Acesso: saída após a meia-noite mantém o estado e a direção do evento")]
    public async Task SaidaAposMeiaNoite_MantemStatusEDirecaoDoHistorico()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        var service = CriarService();
        var entrada = DateTime.Today.AddDays(-1).AddHours(23).AddMinutes(59);
        var saida = DateTime.Today.AddMinutes(10);

        var resultadoEntrada = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Colaborador, colaborador.Id, entrada, TestContext.Current.CancellationToken);
        var statusAntesDaSaida = await service.ObterStatusAsync(
            TipoPessoaAcesso.Colaborador, colaborador.Id, saida, TestContext.Current.CancellationToken);
        var resultadoSaida = await service.RegistrarAcessoAsync(
            TipoPessoaAcesso.Colaborador, colaborador.Id, saida, TestContext.Current.CancellationToken);
        var eventosHoje = (await service.ObterEventosDoDiaAsync(
                DateOnly.FromDateTime(saida), TestContext.Current.CancellationToken))
            .Where(evento => evento.PessoaId == colaborador.Id)
            .ToList();

        Assert.True(resultadoEntrada.Registrado);
        Assert.True(resultadoEntrada.Entrada);
        Assert.True(statusAntesDaSaida?.EstaNaAcademia);
        Assert.True(resultadoSaida.Registrado);
        Assert.False(resultadoSaida.Entrada);
        Assert.Equal(TimeSpan.FromMinutes(11), resultadoSaida.Permanencia);
        var eventoDeHoje = Assert.Single(eventosHoje);
        Assert.False(eventoDeHoje.Entrada);
    }

    [Fact(DisplayName = "Acesso: matrícula ativa permite registrar antes das seis horas")]
    public async Task AlunoComMatriculaAtiva_RegistraAcessoAntesDasSeis()
    {
        var aluno = await CriarEInserirAlunoAsync("Aluno em horário livre");
        var matricula = Matricula.Criar(
            0,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Condicionamento físico",
            MatriculaRestricoes.None,
            null).Value!;
        await new MatriculaRepository(ConnectionString, DatabaseType)
            .Adicionar(matricula, TestContext.Current.CancellationToken);

        var resultado = await CriarService().RegistrarAcessoAsync(
            TipoPessoaAcesso.Aluno,
            aluno.Id,
            DateTime.Today.AddHours(5).AddMinutes(59),
            TestContext.Current.CancellationToken);

        Assert.True(resultado.Registrado);
        Assert.True(resultado.Entrada);
        Assert.Equal("ENTRADA_REGISTRADA", resultado.Codigo);
        Assert.Single(await new AcessoAlunoRepository(ConnectionString, DatabaseType)
            .ObterAcessosPorAlunoPeriodo(aluno.Id, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today), TestContext.Current.CancellationToken));
    }

    private async Task<Aluno> CriarEInserirAlunoAsync(string nome)
    {
        var logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var aluno = Aluno.Criar(
            0,
            nome,
            GerarCpf(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            GerarTelefone(),
            GerarEmail(),
            logradouro,
            "100",
            "",
            "SenhaForte123",
            null).Value!;

        return await new AlunoRepository(ConnectionString, DatabaseType)
            .Adicionar(aluno, TestContext.Current.CancellationToken);
    }

    private async Task<Colaborador> CriarEInserirColaboradorAsync()
    {
        var logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var colaborador = Colaborador.Criar(
            0,
            "Colaborador de teste",
            GerarCpf(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            GerarTelefone(),
            GerarEmail(),
            logradouro,
            "100",
            "",
            "SenhaForte123",
            null,
            DateOnly.FromDateTime(DateTime.Today),
            ColaboradorTipo.Instrutor,
            ColaboradorVinculo.CLT).Value!;

        return await new ColaboradorRepository(ConnectionString, DatabaseType)
            .Adicionar(colaborador, TestContext.Current.CancellationToken);
    }

    private AcessoService CriarService() => new(
        () => new AlunoRepository(ConnectionString, DatabaseType),
        () => new ColaboradorRepository(ConnectionString, DatabaseType),
        () => new MatriculaRepository(ConnectionString, DatabaseType),
        () => new AcessoAlunoRepository(ConnectionString, DatabaseType),
        () => new AcessoColaboradorRepository(ConnectionString, DatabaseType));
}
