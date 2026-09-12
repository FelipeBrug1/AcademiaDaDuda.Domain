// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Infrastructure.Data;
using AcademiaDaDuda.Infrastructure.Repositories;
using AcademiaDaDuda.Infrastructure.Tests;

using Xunit;

namespace AcademiaDaDuda.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    private readonly MatriculaRepository _repository;
    private readonly AlunoRepository _alunoRepository;
    private readonly LogradouroRepository _logradouroRepository;

    public MatriculaInfrastructureTests()
    {
        _repository = new MatriculaRepository(ConnectionString, DatabaseType);
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
            "Aluno Matricula Teste",
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

    private async Task<Matricula> CriarEInserirMatriculaAsync(MatriculaPlano plano = MatriculaPlano.Mensal, DateOnly? dataInicio = null)
    {
        var aluno = await CriarEInserirAlunoAsync();
        var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);

        var matriculaResult = Matricula.Criar(
            0,
            aluno,
            plano,
            inicio,
            "Treino de Força",
            MatriculaRestricoes.None,
            null,
            ""
        );

        if (matriculaResult.IsFailure)
            throw new Exception($"Falha ao criar Matricula: {string.Join(", ", matriculaResult.Notifications.Select(n => n.Mensagem))}");

        return await _repository.Adicionar(matriculaResult.Value!);
    }

    [Fact(DisplayName = "Matricula: Adicionar e ObterPorId com Sucesso")]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync();

        var obtida = await _repository.ObterPorId(matricula.Id);

        Assert.NotNull(obtida);
        Assert.Equal(matricula.Id, obtida.Id);
        Assert.Equal(matricula.AlunoId, obtida.AlunoId);
        Assert.Equal(matricula.Plano, obtida.Plano);
        Assert.Equal(matricula.Objetivo, obtida.Objetivo);
    }

    [Fact(DisplayName = "Matricula: ObterPorId retorna nulo quando inexistente")]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtida = await _repository.ObterPorId(999999);
        Assert.Null(obtida);
    }

    [Fact(DisplayName = "Matricula: ObterTodos com Sucesso")]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        await CriarEInserirMatriculaAsync();

        var todas = await _repository.ObterTodos();

        Assert.NotNull(todas);
        Assert.NotEmpty(todas);
    }

    [Fact(DisplayName = "Matricula: Atualizar com Sucesso")]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync();
        var aluno = await _alunoRepository.ObterPorId(matricula.AlunoId);

        var atualizada = Matricula.Criar(
            matricula.Id,
            aluno!,
            MatriculaPlano.Semestral,
            matricula.DataInicio,
            "Hipertrofia e Resistência",
            MatriculaRestricoes.None,
            null,
            ""
        ).Value!;

        var resultado = await _repository.Atualizar(atualizada);

        Assert.NotNull(resultado);
        Assert.Equal("Hipertrofia e Resistência", resultado.Objetivo);
        Assert.Equal(MatriculaPlano.Semestral, resultado.Plano);

        var noBanco = await _repository.ObterPorId(matricula.Id);
        Assert.NotNull(noBanco);
        Assert.Equal("Hipertrofia e Resistência", noBanco.Objetivo);
        Assert.Equal(MatriculaPlano.Semestral, noBanco.Plano);
    }

    [Fact(DisplayName = "Matricula: Remover com Sucesso")]
    public async Task Matricula_Remover_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync();

        var removida = await _repository.Remover(matricula.Id);

        Assert.True(removida);
        var noBanco = await _repository.ObterPorId(matricula.Id);
        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Matricula: ObterPorAluno com Sucesso")]
    public async Task Matricula_ObterPorAluno_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync();

        var lista = await _repository.ObterPorAluno(matricula.AlunoId);

        Assert.NotNull(lista);
        Assert.Contains(lista, m => m.Id == matricula.Id);
    }

    [Fact(DisplayName = "Matricula: ObterMatriculaAtivaPorAluno com Sucesso")]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync();

        var ativa = await _repository.ObterMatriculaAtivaPorAluno(matricula.AlunoId);

        Assert.NotNull(ativa);
        Assert.Equal(matricula.Id, ativa.Id);
    }

    [Fact(DisplayName = "Matricula: PossuiMatriculaAtiva validação correta")]
    public async Task Matricula_PossuiMatriculaAtiva_ValidaCorretamente()
    {
        var matricula = await CriarEInserirMatriculaAsync();

        var possui = await _repository.PossuiMatriculaAtiva(matricula.AlunoId);
        Assert.True(possui);

        var naoPossui = await _repository.PossuiMatriculaAtiva(999999);
        Assert.False(naoPossui);
    }

    [Fact(DisplayName = "Matricula: ObterAtivas com Sucesso")]
    public async Task Matricula_ObterAtivas_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync();

        var ativas = await _repository.ObterAtivas(matricula.AlunoId);

        Assert.NotNull(ativas);
        Assert.Contains(ativas, m => m.Id == matricula.Id);
    }

    [Fact(DisplayName = "Matricula: ObterPorPlano com Sucesso")]
    public async Task Matricula_ObterPorPlano_Sucesso()
    {
        var matricula = await CriarEInserirMatriculaAsync(MatriculaPlano.Anual);

        var lista = await _repository.ObterPorPlano(MatriculaPlano.Anual);

        Assert.NotNull(lista);
        Assert.Contains(lista, m => m.Id == matricula.Id);
    }
}