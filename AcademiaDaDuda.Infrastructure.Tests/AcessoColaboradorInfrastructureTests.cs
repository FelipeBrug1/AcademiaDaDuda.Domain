// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class AcessoColaboradorInfrastructureTests : TestBase
{
    private readonly AcessoColaboradorRepository _repository;
    private readonly ColaboradorRepository _colaboradorRepository;
    private readonly LogradouroRepository _logradouroRepository;

    public AcessoColaboradorInfrastructureTests()
    {
        _repository = new AcessoColaboradorRepository(ConnectionString, DatabaseType);
        _colaboradorRepository = new ColaboradorRepository(ConnectionString, DatabaseType);
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    private async Task<Colaborador> CriarEInserirColaboradorAsync()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepository, DatabaseType.ToString());
        var cpf = GerarCpf();
        var email = GerarEmail();
        var telefone = GerarTelefone();

        var result = Colaborador.Criar(
            0,
            "Colaborador Acesso Teste",
            cpf,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            telefone,
            email,
            logradouro,
            "100",
            "Sala B",
            "SenhaForte123",
            null,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Instrutor,
            ColaboradorVinculo.CLT
        );

        return await _colaboradorRepository.Adicionar(result.Value!);
    }

    private async Task<AcessoColaborador> CriarEInserirAcessoAsync(Colaborador? colaborador = null, DateTime? dataHora = null)
    {
        var targetColab = colaborador ?? await CriarEInserirColaboradorAsync();
        var horario = dataHora ?? DateTime.Today.AddHours(8); // Dentro do intervalo comercial permitido (06:00 às 22:00)

        var result = AcessoColaborador.Criar(0, targetColab, horario);
        if (result.IsFailure)
            throw new Exception($"Falha ao criar AcessoColaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return await _repository.Adicionar(result.Value!);
    }

    [Fact(DisplayName = "AcessoColaborador: Adicionar e ObterPorId com Sucesso")]
    public async Task AcessoColaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var acesso = await CriarEInserirAcessoAsync();

        var obtido = await _repository.ObterPorId(acesso.Id);

        Assert.NotNull(obtido);
        Assert.Equal(acesso.Id, obtido.Id);
        Assert.Equal(acesso.ColaboradorId, obtido.ColaboradorId);
    }

    [Fact(DisplayName = "AcessoColaborador: ObterPorId retorna nulo quando inexistente")]
    public async Task AcessoColaborador_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "AcessoColaborador: ObterTodos com Sucesso")]
    public async Task AcessoColaborador_ObterTodos_Sucesso()
    {
        await CriarEInserirAcessoAsync();

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "AcessoColaborador: Remover com Sucesso")]
    public async Task AcessoColaborador_Remover_Sucesso()
    {
        var acesso = await CriarEInserirAcessoAsync();

        var removido = await _repository.Remover(acesso.Id);

        Assert.True(removido);
        var noBanco = await _repository.ObterPorId(acesso.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "AcessoColaborador: ObterAcessosPorColaboradorPeriodo com Sucesso")]
    public async Task AcessoColaborador_ObterAcessosPorColaboradorPeriodo_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        var acesso = await CriarEInserirAcessoAsync(colaborador, DateTime.Today.AddHours(9));

        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var fim = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var lista = await _repository.ObterAcessosPorColaboradorPeriodo(colaborador.Id, inicio, fim);

        Assert.NotNull(lista);
        Assert.Contains(lista, a => a.Id == acesso.Id);
    }

    [Fact(DisplayName = "AcessoColaborador: ObterUltimoAcesso com Sucesso")]
    public async Task AcessoColaborador_ObterUltimoAcesso_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        await CriarEInserirAcessoAsync(colaborador, DateTime.Today.AddHours(8));
        var ultimoAcesso = await CriarEInserirAcessoAsync(colaborador, DateTime.Today.AddHours(17));

        var obtido = await _repository.ObterUltimoAcesso(colaborador.Id);

        Assert.NotNull(obtido);
        Assert.Equal(ultimoAcesso.Id, obtido.Id);
    }

    [Fact(DisplayName = "AcessoColaborador: ObterHorasTrabalhadasNoDia com Sucesso")]
    public async Task AcessoColaborador_ObterHorasTrabalhadasNoDia_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        // Entrada 08:00 e Saída 12:00 = 4 horas
        await CriarEInserirAcessoAsync(colaborador, DateTime.Today.AddHours(8));
        await CriarEInserirAcessoAsync(colaborador, DateTime.Today.AddHours(12));

        var horas = await _repository.ObterHorasTrabalhadasNoDia(colaborador.Id, hoje);

        Assert.Equal(TimeSpan.FromHours(4), horas);
    }
}