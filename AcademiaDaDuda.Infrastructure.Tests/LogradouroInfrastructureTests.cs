// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Exceptions;
using AcademiaDaDuda.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _repository;

    public LogradouroInfrastructureTests()
    {
        _repository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    internal static async Task<Logradouro> CriarEInserirLogradouroAsync(LogradouroRepository logradouroRepo, string cidade = "SQLite")
    {
        var cep = GerarCep();
        var logradouroResult = Logradouro.Criar(0, cep, "Felipe", "Brüggemann", cidade, "SC", "Brasil");
        if (logradouroResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Logradouro: {string.Join(", ", logradouroResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await logradouroRepo.Adicionar(logradouroResult.Value!);
    }

    [Fact(DisplayName = "Logradouro: Adicionar e ObterPorId com Sucesso")]
    public async Task Logradouro_Adicionar_E_ObterPorId_Sucesso()
    {
        var cep = GerarCep();
        var logradouro = Logradouro.Criar(0, cep, "Felipe", "Brüggemann", DatabaseType.ToString(), "SC", "Brasil").Value!;

        var inserido = await _repository.Adicionar(logradouro);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);
        Assert.Equal(cep, inserido.Cep.Valor);
        Assert.Equal("Felipe", inserido.Nome);

        var obtido = await _repository.ObterPorId(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal(cep, obtido.Cep.Valor);
    }

    [Fact(DisplayName = "Logradouro: ObterPorId retorna nulo quando inexistente")]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Logradouro: ObterTodos com Sucesso")]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        await CriarEInserirLogradouroAsync(_repository, DatabaseType.ToString());

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "Logradouro: Atualizar com Sucesso")]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository, DatabaseType.ToString());
        var novoCep = GerarCep();
        var logradouroAtualizado = Logradouro.Criar(logradouro.Id, novoCep, "Rua Nova", "Bairro Novo", "Florianópolis", "SC", "Brasil").Value!;

        var resultado = await _repository.Atualizar(logradouroAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal("Rua Nova", resultado.Nome);
        Assert.Equal("Bairro Novo", resultado.Bairro);
        Assert.Equal("Florianópolis", resultado.Cidade);

        var noBanco = await _repository.ObterPorId(logradouro.Id);
        Assert.NotNull(noBanco);
        Assert.Equal("Rua Nova", noBanco.Nome);
    }

    [Fact(DisplayName = "Logradouro: Atualizar lança exceção quando inexistente")]
    public async Task Logradouro_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var cep = GerarCep();
        var logradouroInexistente = Logradouro.Criar(999999, cep, "Rua Fake", "Bairro Fake", "Cidade Fake", "SC", "Brasil").Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(logradouroInexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact(DisplayName = "Logradouro: Remover com Sucesso")]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository, DatabaseType.ToString());

        var removido = await _repository.Remover(logradouro.Id);
        Assert.True(removido);

        var noBanco = await _repository.ObterPorId(logradouro.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Logradouro: Remover retorna false quando inexistente")]
    public async Task Logradouro_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _repository.Remover(999999);
        Assert.False(removido);
    }

    [Fact(DisplayName = "Logradouro: ObterPorCep sucesso e nulo")]
    public async Task Logradouro_ObterPorCep_SucessoENulo()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository, DatabaseType.ToString());

        var obtido = await _repository.ObterPorCep(logradouro.Cep);
        Assert.NotNull(obtido);
        Assert.Equal(logradouro.Id, obtido.Id);

        var cepInexistente = Cep.Criar("99999999").Value!;
        var naoObtido = await _repository.ObterPorCep(cepInexistente);
        Assert.Null(naoObtido);
    }

    [Fact(DisplayName = "Logradouro: CepJaExiste validação correta")]
    public async Task Logradouro_CepJaExiste_ValidaçãoCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository, DatabaseType.ToString());

        var existe = await _repository.CepJaExiste(logradouro.Cep);
        Assert.True(existe);

        var existeMesmoId = await _repository.CepJaExiste(logradouro.Cep, logradouro.Id);
        Assert.False(existeMesmoId);

        var cepInedito = Cep.Criar(GerarCep()).Value!;
        var existeInedito = await _repository.CepJaExiste(cepInedito);
        Assert.False(existeInedito);
    }

    [Fact(DisplayName = "Logradouro: ObterPorCidade filtragem correta")]
    public async Task Logradouro_ObterPorCidade_FiltragemCorreta()
    {
        var cep = GerarCep();
        var cidadeUnica = "CidadeUnica_" + Guid.NewGuid().ToString("N")[..5];
        var logradouro = Logradouro.Criar(0, cep, "Felipe", "Brüggemann", cidadeUnica, "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados = await _repository.ObterPorCidade(cidadeUnica);
        Assert.NotNull(resultados);
        Assert.Single(resultados);
        Assert.Equal(cidadeUnica, resultados.First().Cidade);

        var resultadosVazio = await _repository.ObterPorCidade("CidadeInexistente_123");
        Assert.Empty(resultadosVazio);
    }

    [Fact(DisplayName = "Logradouro: ObterPorBairro filtragem correta")]
    public async Task Logradouro_ObterPorBairro_FiltragemCorreta()
    {
        var cep = GerarCep();
        var cidade = "Cidade_" + Guid.NewGuid().ToString("N")[..5];
        var bairro = "Bairro_" + Guid.NewGuid().ToString("N")[..5];
        var logradouro = Logradouro.Criar(0, cep, "Felipe", bairro, cidade, "SC", "Brasil").Value!;

        await _repository.Adicionar(logradouro);

        var resultados = await _repository.ObterPorBairro(cidade, bairro);
        Assert.NotNull(resultados);
        Assert.Single(resultados);

        var resultadosVazio = await _repository.ObterPorBairro(cidade, "BairroInexistente");
        Assert.Empty(resultadosVazio);
    }
}