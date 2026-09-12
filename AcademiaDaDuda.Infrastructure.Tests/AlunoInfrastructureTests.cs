//Felipe AntonioBruggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Data;
using AcademiaDaDuda.Infrastructure.Repositories;
using AcademiaDaDuda.Infrastructure.Tests;

using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly AlunoRepository _alunoRepo;
    private readonly LogradouroRepository _logradouroRepo;

    public AlunoInfrastructureTests()
    {
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    internal static async Task<Aluno> CriarEInserirAlunoAsync(AlunoRepository alunoRepo, LogradouroRepository logradouroRepo, DatabaseType dbType)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo, dbType.ToString());
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value!;

        var alunoResult = Aluno.Criar(
            id: 0,
            nome: "Aluno Alisson " + Guid.NewGuid().ToString("N")[..5],
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(2000, 10, 10),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "100",
            complemento: "Assis",
            senha: $"SenhaValida123{dbType}",
            foto: foto
        );

        if (alunoResult.IsFailure)
            throw new Exception($"Falha ao criar Aluno: {string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");

        return await alunoRepo.Adicionar(alunoResult.Value!);
    }

    [Fact(DisplayName = "Aluno: Adicionar e ObterPorId com Sucesso")]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);

        var obtido = await _alunoRepo.ObterPorId(aluno.Id);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(aluno.Nome, obtido.Nome);
        Assert.Equal("Assis", obtido.Endereco.Complemento);
        Assert.Equal($"SenhaValida123{DatabaseType}", obtido.Senha.Valor);
    }

    [Fact(DisplayName = "Aluno: ObterPorId retorna nulo quando inexistente")]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _alunoRepo.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Aluno: ObterTodos com Sucesso")]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var todos = await _alunoRepo.ObterTodos();
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "Aluno: Atualizar com Sucesso")]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var novoNome = "Aluno Alisson Editado " + Guid.NewGuid().ToString("N")[..5];
        var logradouro = await _logradouroRepo.ObterPorId(aluno.Endereco.LogradouroId);

        var atualizado = Aluno.Criar(
            aluno.Id,
            novoNome,
            aluno.Cpf.Valor,
            aluno.DataNascimento,
            aluno.Telefone.Valor,
            aluno.Email.Valor,
            logradouro!,
            "200",
            "Assis",
            $"SenhaValida123{DatabaseType}",
            aluno.Foto
        ).Value!;

        var resultado = await _alunoRepo.Atualizar(atualizado);

        Assert.NotNull(resultado);
        Assert.Equal(novoNome, resultado.Nome);

        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(novoNome, noBanco.Nome);
    }

    [Fact(DisplayName = "Aluno: Remover com Sucesso")]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var removido = await _alunoRepo.Remover(aluno.Id);
        Assert.True(removido);

        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Aluno: ObterPorCpf com Sucesso")]
    public async Task Aluno_ObterPorCpf_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var obtido = await _alunoRepo.ObterPorCpf(aluno.Cpf);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
    }

    [Fact(DisplayName = "Aluno: ObterPorEmail com Sucesso")]
    public async Task Aluno_ObterPorEmail_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var obtido = await _alunoRepo.ObterPorEmail(aluno.Email);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
    }

    [Fact(DisplayName = "Aluno: CpfJaExiste validação correta")]
    public async Task Aluno_CpfJaExiste_ValidaCorretamente()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        Assert.True(await _alunoRepo.CpfJaExiste(aluno.Cpf));
        Assert.False(await _alunoRepo.CpfJaExiste(aluno.Cpf, aluno.Id));
    }

    [Fact(DisplayName = "Aluno: EmailJaExiste validação correta")]
    public async Task Aluno_EmailJaExiste_ValidaCorretamente()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        Assert.True(await _alunoRepo.EmailJaExiste(aluno.Email));
        Assert.False(await _alunoRepo.EmailJaExiste(aluno.Email, aluno.Id));
    }

    [Fact(DisplayName = "Aluno: ObterPorNome com Sucesso")]
    public async Task Aluno_ObterPorNome_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var resultados = await _alunoRepo.ObterPorNome(aluno.Nome);
        Assert.NotNull(resultados);
        Assert.NotEmpty(resultados);
    }

    [Fact(DisplayName = "Aluno: TrocarSenha com Sucesso")]
    public async Task Aluno_TrocarSenha_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo, DatabaseType);
        var novaSenha = Senha.Criar($"NovaSenha{DatabaseType}123").Value!;

        var alterou = await _alunoRepo.TrocarSenha(aluno.Id, novaSenha);
        Assert.True(alterou);

        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(noBanco);
        Assert.Equal($"NovaSenha{DatabaseType}123", noBanco.Senha.Valor);
    }
}