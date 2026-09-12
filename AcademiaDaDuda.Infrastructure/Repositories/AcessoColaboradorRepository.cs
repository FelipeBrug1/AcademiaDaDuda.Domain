// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Repositories;
using AcademiaDaDuda.Infrastructure.Data;
using AcademiaDaDuda.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDaDuda.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    private const int PessoaTipoColaborador = 1; // 0 = Aluno, 1 = Colaborador

    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery =>
        @"SELECT id_acesso, pessoa_tipo, pessoa_id, data_hora 
          FROM tb_acesso 
          WHERE pessoa_tipo = 1";

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de colaborador por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY data_hora DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string sql = @"INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) 
                           VALUES (@PessoaTipo, @PessoaId, @DataHora)";
            string query = FormatInsertQuery(sql);

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoColaborador, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO", "Falha ao obter ID inserido para o acesso.", cancellationToken);
            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO", $"Erro ao adicionar acesso de colaborador: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = @"UPDATE tb_acesso 
                             SET pessoa_id = @PessoaId, data_hora = @DataHora 
                             WHERE id_acesso = @Id AND pessoa_tipo = 1";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de colaborador encontrado com ID {entity.Id} para atualização.");

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO", $"Erro ao atualizar acesso ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO", $"Erro ao remover acesso ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery}";
            if (colaboradorId.HasValue) query += " AND pessoa_id = @ColaboradorId";
            if (inicio.HasValue) query += " AND data_hora >= @Inicio";
            if (fim.HasValue) query += " AND data_hora <= @Fim";
            query += " ORDER BY data_hora DESC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToString("yyyy-MM-dd") + " 00:00:00", DbType.String);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.ToString("yyyy-MM-dd") + " 23:59:59", DbType.String);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_PERIODO", $"Erro ao obter acessos do colaborador por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND pessoa_id = @ColaboradorId ORDER BY data_hora DESC LIMIT 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataFormatada = data.ToString("yyyy-MM-dd");
            string query = $@"SELECT data_hora 
                              FROM tb_acesso 
                              WHERE pessoa_tipo = 1 AND pessoa_id = @ColaboradorId 
                              AND data_hora >= @Inicio AND data_hora <= @Fim 
                              ORDER BY data_hora ASC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Inicio", dataFormatada + " 00:00:00", DbType.String);
            command.AddParameter("@Fim", dataFormatada + " 23:59:59", DbType.String);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var registros = new List<DateTime>();
            while (await reader.ReadAsync(cancellationToken))
            {
                registros.Add(reader.GetDateTimeValue("data_hora"));
            }

            TimeSpan totalTrabalhado = TimeSpan.Zero;
            for (int i = 0; i < registros.Count - 1; i += 2)
            {
                totalTrabalhado += (registros[i + 1] - registros[i]);
            }

            return totalTrabalhado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_HORAS_TRABALHADAS", $"Erro ao calcular horas trabalhadas no dia: {ex.Message}", ex);
        }
    }

    public static AcessoColaborador Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso");
            int colaboradorId = reader.GetInt32Value("pessoa_id");
            var dataHora = reader.GetDateTimeValue("data_hora");

            var ctor = typeof(AcessoColaborador).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
            if (ctor != null)
            {
                return (AcessoColaborador)ctor.Invoke(new object?[] { id, colaboradorId, dataHora })!;
            }

            throw new InfrastructureException("ERRO_CONSTRUTOR_ACESSO_COLABORADOR", "Construtor de AcessoColaborador não encontrado.");
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_COLABORADOR", $"Erro ao mapear acesso de colaborador: {ex.Message}", ex);
        }
    }
}