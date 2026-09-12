// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    private readonly ColaboradorRepository _repository;
    private readonly LogradouroRepository _logradouroRepository;

    public ColaboradorInfrastructureTests()
    {
        _repository = new ColaboradorRepository(ConnectionString, DatabaseType);
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    private async Task<Colaborador> CriarEInserirColaboradorAsync(
        ColaboradorTipo tipo = ColaboradorTipo.Instrutor,
        ColaboradorVinculo vinculo = ColaboradorVinculo.CLT)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepository, DatabaseType.ToString());
        var cpf = GerarCpf();
        var email = GerarEmail();
        var telefone = GerarTelefone();
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3 }).Value;

        var result = Colaborador.Criar(
            0,
            "Colaborador Teste",
            cpf,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
            telefone,
            email,
            logradouro,
            "200",
            "Sala 1",
            "SenhaForte123",
            foto,
            DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
            tipo,
            vinculo
        );

        if (result.IsFailure)
            throw new Exception($"Falha ao criar Colaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return await _repository.Adicionar(result.Value!);
    }

    [Fact(DisplayName = "Colaborador: Adicionar e ObterPorId com Sucesso")]
    public async Task Colaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();

        var obtido = await _repository.ObterPorId(colaborador.Id);

        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);
        Assert.Equal(colaborador.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(colaborador.Email.Valor, obtido.Email.Valor);
        Assert.Equal(colaborador.Tipo, obtido.Tipo);
    }

    [Fact(DisplayName = "Colaborador: ObterPorId retorna nulo quando inexistente")]
    public async Task Colaborador_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Colaborador: ObterTodos com Sucesso")]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        await CriarEInserirColaboradorAsync();

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "Colaborador: Atualizar com Sucesso")]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        var novoEmail = GerarEmail();
        var novoTelefone = GerarTelefone();
        var logradouro = await _logradouroRepository.ObterPorId(colaborador.Endereco.LogradouroId);

        var atualizado = Colaborador.Criar(
            colaborador.Id,
            "Nome Atualizado",
            colaborador.Cpf.Valor,
            colaborador.DataNascimento,
            novoTelefone,
            novoEmail,
            logradouro!,
            "300",
            "Sala 2",
            "NovaSenha123",
            colaborador.Foto,
            colaborador.DataAdmissao,
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT
        ).Value!;

        var resultado = await _repository.Atualizar(atualizado);

        Assert.NotNull(resultado);
        Assert.Equal("Nome Atualizado", resultado.Nome);
        Assert.Equal(ColaboradorTipo.Atendente, resultado.Tipo);

        var noBanco = await _repository.ObterPorId(colaborador.Id);
        Assert.NotNull(noBanco);
        Assert.Equal("Nome Atualizado", noBanco.Nome);
        Assert.Equal(ColaboradorTipo.Atendente, noBanco.Tipo);
    }

    [Fact(DisplayName = "Colaborador: Remover com Sucesso")]
    public async Task Colaborador_Remover_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();

        var removido = await _repository.Remover(colaborador.Id);

        Assert.True(removido);
        var noBanco = await _repository.ObterPorId(colaborador.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Colaborador: ObterPorCpf com Sucesso")]
    public async Task Colaborador_ObterPorCpf_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();

        var obtido = await _repository.ObterPorCpf(colaborador.Cpf);

        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);
    }

    [Fact(DisplayName = "Colaborador: ObterPorEmail com Sucesso")]
    public async Task Colaborador_ObterPorEmail_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();

        var obtido = await _repository.ObterPorEmail(colaborador.Email);

        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);
    }

    [Fact(DisplayName = "Colaborador: CpfJaExiste validação correta")]
    public async Task Colaborador_CpfJaExiste_ValidaCorretamente()
    {
        var colaborador = await CriarEInserirColaboradorAsync();

        var existe = await _repository.CpfJaExiste(colaborador.Cpf);
        Assert.True(existe);

        var existeMesmoId = await _repository.CpfJaExiste(colaborador.Cpf, colaborador.Id);
        Assert.False(existeMesmoId);
    }

    [Fact(DisplayName = "Colaborador: EmailJaExiste validação correta")]
    public async Task Colaborador_EmailJaExiste_ValidaCorretamente()
    {
        var colaborador = await CriarEInserirColaboradorAsync();

        var existe = await _repository.EmailJaExiste(colaborador.Email);
        Assert.True(existe);

        var existeMesmoId = await _repository.EmailJaExiste(colaborador.Email, colaborador.Id);
        Assert.False(existeMesmoId);
    }

    [Fact(DisplayName = "Colaborador: ObterPorTipo com Sucesso")]
    public async Task Colaborador_ObterPorTipo_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(ColaboradorTipo.Instrutor, ColaboradorVinculo.CLT);

        var lista = await _repository.ObterPorTipo(ColaboradorTipo.Instrutor);

        Assert.NotNull(lista);
        Assert.Contains(lista, c => c.Id == colaborador.Id);
    }

    [Fact(DisplayName = "Colaborador: ObterPorVinculo com Sucesso")]
    public async Task Colaborador_ObterPorVinculo_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(ColaboradorTipo.Atendente, ColaboradorVinculo.Estagio);

        var lista = await _repository.ObterPorVinculo(ColaboradorVinculo.Estagio);

        Assert.NotNull(lista);
        Assert.Contains(lista, c => c.Id == colaborador.Id);
    }

    [Fact(DisplayName = "Colaborador: TrocarSenha com Sucesso")]
    public async Task Colaborador_TrocarSenha_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync();
        var novaSenha = Senha.Criar("NovaSenhaTrocada123").Value!;

        var alterou = await _repository.TrocarSenha(colaborador.Id, novaSenha);
        Assert.True(alterou);

        var noBanco = await _repository.ObterPorId(colaborador.Id);
        Assert.NotNull(noBanco);
        Assert.Equal("NovaSenhaTrocada123", noBanco.Senha.Valor);
    }
}