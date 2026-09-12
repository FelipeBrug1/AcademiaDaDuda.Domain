// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly AlunoRepository _repository;
    private readonly LogradouroRepository _logradouroRepository;

    public AlunoInfrastructureTests()
    {
        _repository = new AlunoRepository(ConnectionString, DatabaseType);
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    private async Task<Aluno> CriarEInserirAlunoAsync()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepository, DatabaseType.ToString());
        var cpf = GerarCpf();
        var email = GerarEmail();
        var telefone = GerarTelefone();
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3 }).Value;

        var alunoResult = Aluno.Criar(
            0,
            "Aluno Teste",
            cpf,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            telefone,
            email,
            logradouro,
            "100",
            "Apto 1",
            "SenhaForte123",
            foto
        );

        if (alunoResult.IsFailure)
            throw new Exception($"Falha ao criar Aluno: {string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");

        return await _repository.Adicionar(alunoResult.Value!);
    }

    [Fact(DisplayName = "Aluno: Adicionar e ObterPorId com Sucesso")]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var obtido = await _repository.ObterPorId(aluno.Id);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(aluno.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(aluno.Email.Valor, obtido.Email.Valor);
    }

    [Fact(DisplayName = "Aluno: ObterPorId retorna nulo quando inexistente")]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Aluno: ObterTodos com Sucesso")]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await CriarEInserirAlunoAsync();

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "Aluno: Atualizar com Sucesso")]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();
        var novoEmail = GerarEmail();
        var novoTelefone = GerarTelefone();
        var logradouro = await _logradouroRepository.ObterPorId(aluno.Endereco.LogradouroId);

        var alunoAtualizado = Aluno.Criar(
            aluno.Id,
            "Nome Atualizado",
            aluno.Cpf.Valor,
            aluno.DataNascimento,
            novoTelefone,
            novoEmail,
            logradouro!,
            "200",
            "Casa",
            "NovaSenha123",
            aluno.Foto
        ).Value!;

        var resultado = await _repository.Atualizar(alunoAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal("Nome Atualizado", resultado.Nome);

        var noBanco = await _repository.ObterPorId(aluno.Id);
        Assert.NotNull(noBanco);
        Assert.Equal("Nome Atualizado", noBanco.Nome);
        Assert.Equal(novoEmail, noBanco.Email.Valor);
    }

    [Fact(DisplayName = "Aluno: Remover com Sucesso")]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var removido = await _repository.Remover(aluno.Id);

        Assert.True(removido);
        var noBanco = await _repository.ObterPorId(aluno.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Aluno: ObterPorCpf com Sucesso")]
    public async Task Aluno_ObterPorCpf_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var obtido = await _repository.ObterPorCpf(aluno.Cpf);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
    }

    [Fact(DisplayName = "Aluno: ObterPorEmail com Sucesso")]
    public async Task Aluno_ObterPorEmail_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var obtido = await _repository.ObterPorEmail(aluno.Email);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
    }

    [Fact(DisplayName = "Aluno: CpfJaExiste validação correta")]
    public async Task Aluno_CpfJaExiste_ValidaCorretamente()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var existe = await _repository.CpfJaExiste(aluno.Cpf);
        Assert.True(existe);

        var existeMesmoId = await _repository.CpfJaExiste(aluno.Cpf, aluno.Id);
        Assert.False(existeMesmoId);
    }

    [Fact(DisplayName = "Aluno: EmailJaExiste validação correta")]
    public async Task Aluno_EmailJaExiste_ValidaCorretamente()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var existe = await _repository.EmailJaExiste(aluno.Email);
        Assert.True(existe);

        var existeMesmoId = await _repository.EmailJaExiste(aluno.Email, aluno.Id);
        Assert.False(existeMesmoId);
    }

    [Fact(DisplayName = "Aluno: ObterPorNome com Sucesso")]
    public async Task Aluno_ObterPorNome_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();

        var resultados = await _repository.ObterPorNome(aluno.Nome);

        Assert.NotNull(resultados);
        Assert.NotEmpty(resultados);
    }

    [Fact(DisplayName = "Aluno: TrocarSenha com Sucesso")]
    public async Task Aluno_TrocarSenha_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();
        var novaSenha = Senha.Criar("SenhaTrocada123").Value!;

        var alterou = await _repository.TrocarSenha(aluno.Id, novaSenha);
        Assert.True(alterou);

        var noBanco = await _repository.ObterPorId(aluno.Id);
        Assert.NotNull(noBanco);
        Assert.Equal("SenhaTrocada123", noBanco.Senha.Valor);
    }
}