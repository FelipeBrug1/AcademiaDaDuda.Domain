// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class AcessoAlunoInfrastructureTests : TestBase
{
    private readonly AcessoAlunoRepository _repository;
    private readonly AlunoRepository _alunoRepository;
    private readonly LogradouroRepository _logradouroRepository;

    public AcessoAlunoInfrastructureTests()
    {
        _repository = new AcessoAlunoRepository(ConnectionString, DatabaseType);
        _alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    private async Task<Aluno> CriarEInserirAlunoAsync()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepository, DatabaseType.ToString());
        var cpf = GerarCpf();
        var email = GerarEmail();
        var telefone = GerarTelefone();

        var alunoResult = Aluno.Criar(
            0,
            "Aluno Acesso Teste",
            cpf,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            telefone,
            email,
            logradouro,
            "100",
            "Apto 1",
            "SenhaForte123",
            null
        );

        return await _alunoRepository.Adicionar(alunoResult.Value!);
    }

    private async Task<AcessoAluno> CriarEInserirAcessoAsync(Aluno? aluno = null, DateTime? dataHora = null)
    {
        var targetAluno = aluno ?? await CriarEInserirAlunoAsync();
        var horario = dataHora ?? DateTime.Today.AddHours(10); // Horário dentro da janela permitida (06:00 às 22:00)

        var acessoResult = AcessoAluno.Criar(0, targetAluno, horario);
        if (acessoResult.IsFailure)
            throw new Exception($"Falha ao criar AcessoAluno: {string.Join(", ", acessoResult.Notifications.Select(n => n.Mensagem))}");

        return await _repository.Adicionar(acessoResult.Value!);
    }

    [Fact(DisplayName = "AcessoAluno: Adicionar e ObterPorId com Sucesso")]
    public async Task AcessoAluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var acesso = await CriarEInserirAcessoAsync();

        var obtido = await _repository.ObterPorId(acesso.Id);

        Assert.NotNull(obtido);
        Assert.Equal(acesso.Id, obtido.Id);
        Assert.Equal(acesso.AlunoId, obtido.AlunoId);
    }

    [Fact(DisplayName = "AcessoAluno: ObterPorId retorna nulo quando inexistente")]
    public async Task AcessoAluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact(DisplayName = "AcessoAluno: ObterTodos com Sucesso")]
    public async Task AcessoAluno_ObterTodos_Sucesso()
    {
        await CriarEInserirAcessoAsync();

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact(DisplayName = "AcessoAluno: Remover com Sucesso")]
    public async Task AcessoAluno_Remover_Sucesso()
    {
        var acesso = await CriarEInserirAcessoAsync();

        var removido = await _repository.Remover(acesso.Id);

        Assert.True(removido);
        var noBanco = await _repository.ObterPorId(acesso.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "AcessoAluno: ObterAcessosPorAlunoPeriodo com Sucesso")]
    public async Task AcessoAluno_ObterAcessosPorAlunoPeriodo_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();
        var acesso = await CriarEInserirAcessoAsync(aluno, DateTime.Today.AddHours(8));

        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var fim = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var lista = await _repository.ObterAcessosPorAlunoPeriodo(aluno.Id, inicio, fim);

        Assert.NotNull(lista);
        Assert.Contains(lista, a => a.Id == acesso.Id);
    }

    [Fact(DisplayName = "AcessoAluno: ObterUltimoAcesso com Sucesso")]
    public async Task AcessoAluno_ObterUltimoAcesso_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync();
        await CriarEInserirAcessoAsync(aluno, DateTime.Today.AddHours(8));
        var ultimoAcesso = await CriarEInserirAcessoAsync(aluno, DateTime.Today.AddHours(14));

        var obtido = await _repository.ObterUltimoAcesso(aluno.Id);

        Assert.NotNull(obtido);
        Assert.Equal(ultimoAcesso.Id, obtido.Id);
    }

    [Fact(DisplayName = "AcessoAluno: EstaNaAcademia validação correta")]
    public async Task AcessoAluno_EstaNaAcademia_ValidaCorretamente()
    {
        var aluno = await CriarEInserirAlunoAsync();

        // 1 registro hoje = número ímpar (está dentro)
        await CriarEInserirAcessoAsync(aluno, DateTime.Today.AddHours(10));
        var estaNaAcademia = await _repository.EstaNaAcademia(aluno.Id);
        Assert.True(estaNaAcademia);

        // 2 registros hoje = número par (saiu)
        await CriarEInserirAcessoAsync(aluno, DateTime.Today.AddHours(11));
        var saiuDaAcademia = await _repository.EstaNaAcademia(aluno.Id);
        Assert.False(saiuDaAcademia);
    }

    [Fact(DisplayName = "AcessoAluno: ObterAlunosSemAcessoNosUltimosDias com Sucesso")]
    public async Task AcessoAluno_ObterAlunosSemAcessoNosUltimosDias_Sucesso()
    {
        var alunoSemAcesso = await CriarEInserirAlunoAsync();

        var alunos = await _repository.ObterAlunosSemAcessoNosUltimosDias(7);

        Assert.NotNull(alunos);
        Assert.Contains(alunos, a => a.Id == alunoSemAcesso.Id);
    }
}