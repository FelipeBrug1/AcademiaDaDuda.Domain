// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.Repositories;
using AcademiaDaDuda.Domain.ValueObjects;
using AcademiaDaDuda.Infrastructure.Data;
using AcademiaDaDuda.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDaDuda.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository, IMatriculaRepository
{
    public MatriculaRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery =>
        @"SELECT id_matricula, aluno_id, plano, data_inicio, data_fim, objetivo, 
                 restricao_medica, obs_restricao, laudo_medico
          FROM tb_matricula";

    public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE id_matricula = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter matrícula por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY data_inicio DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todas as matrículas: {ex.Message}", ex);
        }
    }

    public async Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string sql = @"INSERT INTO tb_matricula (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico)
                           VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObsRestricao, @LaudoMedico)";
            string query = FormatInsertQuery(sql);

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio.ToString("yyyy-MM-dd"), DbType.String);
            command.AddParameter("@DataFim", entity.DataFim.ToString("yyyy-MM-dd"), DbType.String);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value, DbType.String);
            command.AddParameter("@LaudoMedico", (object?)entity.LaudoMedico?.Conteudo ?? DBNull.Value, DbType.Binary);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_MATRICULA", "Falha ao obter ID inserido para a matrícula.", cancellationToken);
            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_MATRICULA", $"Erro ao adicionar matrícula: {ex.Message}", ex);
        }
    }

    public async Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = @"UPDATE tb_matricula
                             SET aluno_id = @AlunoId, plano = @Plano, data_inicio = @DataInicio, data_fim = @DataFim,
                                 objetivo = @Objetivo, restricao_medica = @RestricaoMedica, obs_restricao = @ObsRestricao,
                                 laudo_medico = @LaudoMedico
                             WHERE id_matricula = @Id";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio.ToString("yyyy-MM-dd"), DbType.String);
            command.AddParameter("@DataFim", entity.DataFim.ToString("yyyy-MM-dd"), DbType.String);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value, DbType.String);
            command.AddParameter("@LaudoMedico", (object?)entity.LaudoMedico?.Conteudo ?? DBNull.Value, DbType.Binary);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhuma matrícula encontrada com ID {entity.Id} para atualização.");

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_MATRICULA", $"Erro ao atualizar matrícula ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_matricula WHERE id_matricula = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_MATRICULA", $"Erro ao remover matrícula ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE aluno_id = @AlunoId ORDER BY data_inicio DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ALUNO", $"Erro ao obter matrículas do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string currentDate = GetCurrentDateFunction();
            string query = $"{BaseSelectQuery} WHERE aluno_id = @AlunoId AND data_fim >= {currentDate} AND data_inicio <= {currentDate} ORDER BY data_fim DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULA_ATIVA", $"Erro ao obter matrícula ativa do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string currentDate = GetCurrentDateFunction();
            string query = $"SELECT COUNT(1) FROM tb_matricula WHERE aluno_id = @AlunoId AND data_fim >= {currentDate} AND data_inicio <= {currentDate}";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_MATRICULA_ATIVA", $"Erro ao verificar matrícula ativa do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            string currentDate = GetCurrentDateFunction();
            string query = alunoId > 0
                ? $"{BaseSelectQuery} WHERE data_fim >= {currentDate} AND data_inicio <= {currentDate} AND aluno_id = @AlunoId ORDER BY data_fim DESC"
                : $"{BaseSelectQuery} WHERE data_fim >= {currentDate} AND data_inicio <= {currentDate} ORDER BY data_fim DESC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId > 0) command.AddParameter("@AlunoId", alunoId, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ATIVAS", $"Erro ao obter matrículas ativas: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            string currentDate = GetCurrentDateFunction();
            string dateLimit = GetDateAddDaysExpression(currentDate, "@Dias");
            string query = $"{BaseSelectQuery} WHERE data_fim >= {currentDate} AND data_fim <= {dateLimit} ORDER BY data_fim ASC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Dias", dias, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_VENCENDO", $"Erro ao obter matrículas vencendo em {dias} dias: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE plano = @Plano ORDER BY data_inicio DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Plano", (int)plano, DbType.Int32);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_PLANO", $"Erro ao obter matrículas pelo plano {plano}: {ex.Message}", ex);
        }
    }

    public static Matricula Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_matricula");
            int alunoId = reader.GetInt32Value("aluno_id");
            var plano = (MatriculaPlano)reader.GetInt32Value("plano");
            var dataInicio = reader.GetDateOnlyValue("data_inicio");
            var dataFim = reader.GetDateOnlyValue("data_fim");
            string objetivo = reader.GetStringValue("objetivo");
            var restricoes = (MatriculaRestricoes)reader.GetInt32Value("restricao_medica");
            string obsRestricao = reader.GetNullableString("obs_restricao");
            byte[]? laudoBytes = reader.GetNullableBytes("laudo_medico");
            Arquivo? laudoMedico = laudoBytes != null ? Arquivo.Criar(laudoBytes).Value : null;

            var ctor = typeof(Matricula).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
            if (ctor != null)
            {
                return (Matricula)ctor.Invoke(new object?[]
                {
                    id, alunoId, plano, dataInicio, dataFim, objetivo, restricoes, laudoMedico, obsRestricao
                })!;
            }

            throw new InfrastructureException("ERRO_CONSTRUTOR_MATRICULA", "Construtor de Matrícula não encontrado.");
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_MATRICULA", $"Erro ao mapear dados da matrícula: {ex.Message}", ex);
        }
    }
}