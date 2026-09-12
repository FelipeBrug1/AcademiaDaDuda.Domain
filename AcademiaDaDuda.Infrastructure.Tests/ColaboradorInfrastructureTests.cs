// Felipe Antonio Brüggemann

using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Data;
using AcademiaDaDuda.Infrastructure.Exceptions;
using AcademiaDaDuda.Infrastructure.Repositories;
using AcademiaDaDuda.Infrastructure.Tests;
using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    private readonly ColaboradorRepository _colaboradorRepo;
    private readonly LogradouroRepository _logradouroRepo;

    public ColaboradorInfrastructureTests()
    {
        _colaboradorRepo = new ColaboradorRepository(ConnectionString, DatabaseType);
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    internal static async Task<Colaborador> CriarEInserirColaboradorAsync(ColaboradorRepository colaboradorRepo, LogradouroRepository logradouroRepo, DatabaseType dbType)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo, dbType.ToString());
        var foto = Arquivo.Criar(new byte[] { 5, 6, 7, 8 }).Value!;

        var colaboradorResult = Colaborador.Criar(
            id: 0,
            nome: "Colaborador Alisson " + Guid.NewGuid().ToString("N")[..5],
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(1995, 5, 15),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "200",
            complemento: "Assis",
            senha: $"SenhaValida123{dbType}",
            foto: foto,
            dataAdmissao: new DateOnly(2023, 1, 1),
            tipo: ColaboradorTipo.Instrutor,
            vinculo: ColaboradorVinculo.CLT
        );

        if (colaboradorResult.IsFailure)
            throw new Exception($"Falha ao criar Colaborador: {string.Join(", ", colaboradorResult.Notifications.Select(n => n.Mensagem))}");

        return await colaboradorRepo.Adicionar(colaboradorResult.Value!);
    }

    [Fact(DisplayName = "Colaborador: Adicionar e ObterPorId com Sucesso")]
    public async Task Colaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);

        var obtido = await _colaboradorRepo.ObterPorId(colaborador.Id);

        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);
        Assert.Equal(colaborador.Nome, obtido.Nome);
        Assert.Equal("Assis", obtido.Endereco.Complemento);
        Assert.Equal($"SenhaValida123{DatabaseType}", obtido.Senha.Valor);
    }

    [Fact(DisplayName = "Colaborador: ObterPorId retorna nulo quando inexistente")]
    public async Task Colaborador_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _colaboradorRepo.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Colaborador: ObterTodos com Sucesso")]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var todos = await _colaboradorRepo.ObterTodos();
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "Colaborador: Atualizar com Sucesso")]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var novoNome = "Colaborador Alisson Editado " + Guid.NewGuid().ToString("N")[..5];
        var logradouro = await _logradouroRepo.ObterPorId(colaborador.Endereco.LogradouroId);

        var atualizado = Colaborador.Criar(
            colaborador.Id,
            novoNome,
            colaborador.Cpf.Valor,
            colaborador.DataNascimento,
            colaborador.Telefone.Valor,
            colaborador.Email.Valor,
            logradouro!,
            "300",
            "Assis",
            $"SenhaValida123{DatabaseType}",
            colaborador.Foto,
            colaborador.DataAdmissao,
            ColaboradorTipo.Administrador,
            ColaboradorVinculo.CLT
        ).Value!;

        var resultado = await _colaboradorRepo.Atualizar(atualizado);

        Assert.NotNull(resultado);
        Assert.Equal(novoNome, resultado.Nome);
        Assert.Equal(ColaboradorTipo.Administrador, resultado.Tipo);

        var noBanco = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(novoNome, noBanco.Nome);
    }

    [Fact(DisplayName = "Colaborador: Atualizar lança exceção quando inexistente")]
    public async Task Colaborador_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo, DatabaseType.ToString());
        var foto = Arquivo.Criar(new byte[] { 1, 2 }).Value!;
        var inexistente = Colaborador.Criar(
            999999, "Inexistente", GerarCpf(), new DateOnly(1990, 1, 1),
            GerarTelefone(), GerarEmail(), logradouro, "1", "Assis",
            $"SenhaValida123{DatabaseType}", foto, new DateOnly(2020, 1, 1),
            ColaboradorTipo.Atendente, ColaboradorVinculo.CLT
        ).Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _colaboradorRepo.Atualizar(inexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact(DisplayName = "Colaborador: Remover com Sucesso")]
    public async Task Colaborador_Remover_Sucesso()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var removido = await _colaboradorRepo.Remover(colaborador.Id);
        Assert.True(removido);

        var noBanco = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Colaborador: Remover retorna false quando inexistente")]
    public async Task Colaborador_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _colaboradorRepo.Remover(999999);
        Assert.False(removido);
    }

    [Fact(DisplayName = "Colaborador: ObterPorCpf com Sucesso e Nulo")]
    public async Task Colaborador_ObterPorCpf_SucessoENulo()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var obtido = await _colaboradorRepo.ObterPorCpf(colaborador.Cpf);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);

        var cpfInexistente = Cpf.Criar(GerarCpf()).Value!;
        var naoObtido = await _colaboradorRepo.ObterPorCpf(cpfInexistente);
        Assert.Null(naoObtido);
    }

    [Fact(DisplayName = "Colaborador: ObterPorEmail com Sucesso e Nulo")]
    public async Task Colaborador_ObterPorEmail_SucessoENulo()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var obtido = await _colaboradorRepo.ObterPorEmail(colaborador.Email);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);

        var emailInexistente = Email.Criar(GerarEmail()).Value!;
        var naoObtido = await _colaboradorRepo.ObterPorEmail(emailInexistente);
        Assert.Null(naoObtido);
    }

    [Fact(DisplayName = "Colaborador: CpfJaExiste validação correta")]
    public async Task Colaborador_CpfJaExiste_ValidacaoCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        Assert.True(await _colaboradorRepo.CpfJaExiste(colaborador.Cpf));
        Assert.False(await _colaboradorRepo.CpfJaExiste(colaborador.Cpf, colaborador.Id));

        var cpfInedito = Cpf.Criar(GerarCpf()).Value!;
        Assert.False(await _colaboradorRepo.CpfJaExiste(cpfInedito));
    }

    [Fact(DisplayName = "Colaborador: EmailJaExiste validação correta")]
    public async Task Colaborador_EmailJaExiste_ValidacaoCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        Assert.True(await _colaboradorRepo.EmailJaExiste(colaborador.Email));
        Assert.False(await _colaboradorRepo.EmailJaExiste(colaborador.Email, colaborador.Id));

        var emailInedito = Email.Criar(GerarEmail()).Value!;
        Assert.False(await _colaboradorRepo.EmailJaExiste(emailInedito));
    }

    [Fact(DisplayName = "Colaborador: ObterPorTipo filtragem correta")]
    public async Task Colaborador_ObterPorTipo_FiltragemCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var resultados = await _colaboradorRepo.ObterPorTipo(colaborador.Tipo);
        Assert.NotNull(resultados);
        Assert.Contains(resultados, c => c.Id == colaborador.Id);
    }

    [Fact(DisplayName = "Colaborador: ObterPorVinculo filtragem correta")]
    public async Task Colaborador_ObterPorVinculo_FiltragemCorreta()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var resultados = await _colaboradorRepo.ObterPorVinculo(colaborador.Vinculo);
        Assert.NotNull(resultados);
        Assert.Contains(resultados, c => c.Id == colaborador.Id);
    }

    [Fact(DisplayName = "Colaborador: TrocarSenha com Sucesso e Falha")]
    public async Task Colaborador_TrocarSenha_SucessoEFalha()
    {
        var colaborador = await CriarEInserirColaboradorAsync(_colaboradorRepo, _logradouroRepo, DatabaseType);
        var novaSenha = Senha.Criar($"NovaSenha{DatabaseType}123").Value!;

        var alterou = await _colaboradorRepo.TrocarSenha(colaborador.Id, novaSenha);
        Assert.True(alterou);

        var atualizado = await _colaboradorRepo.ObterPorId(colaborador.Id);
        Assert.NotNull(atualizado);
        Assert.Equal($"NovaSenha{DatabaseType}123", atualizado.Senha.Valor);

        var alterouInexistente = await _colaboradorRepo.TrocarSenha(999999, novaSenha);
        Assert.False(alterouInexistente);
    }
}