// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Repositories;
using AcademiaDaDuda.Infrastructure.Data;
using AcademiaDaDuda.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDaDuda.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const int PessoaTipoAluno = 0; // 0 = Aluno, 1 = Colaborador

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery =>
        @"SELECT id_acesso, pessoa_tipo, pessoa_id, data_hora 
          FROM tb_acesso 
          WHERE pessoa_tipo = 0";

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
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
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY data_hora DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string sql = @"INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) 
                           VALUES (@PessoaTipo, @PessoaId, @DataHora)";
            string query = FormatInsertQuery(sql);

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaTipo", PessoaTipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO", "Falha ao obter ID inserido para o acesso.", cancellationToken);
            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO", $"Erro ao adicionar acesso de aluno: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = @"UPDATE tb_acesso 
                             SET pessoa_id = @PessoaId, data_hora = @DataHora 
                             WHERE id_acesso = @Id AND pessoa_tipo = 0";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com ID {entity.Id} para atualização.");

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
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 0";
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

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery}";
            if (alunoId.HasValue) query += " AND pessoa_id = @AlunoId";
            if (inicio.HasValue) query += " AND data_hora >= @Inicio";
            if (fim.HasValue) query += " AND data_hora <= @Fim";
            query += " ORDER BY data_hora DESC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToString("yyyy-MM-dd") + " 00:00:00", DbType.String);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.ToString("yyyy-MM-dd") + " 23:59:59", DbType.String);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var entities = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(Map(reader));
            }
            return entities;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_PERIODO", $"Erro ao obter acessos por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND pessoa_id = @AlunoId ORDER BY data_hora DESC LIMIT 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do aluno: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string hojeInicio = DateTime.Today.ToString("yyyy-MM-dd 00:00:00");
            string hojeFim = DateTime.Today.ToString("yyyy-MM-dd 23:59:59");
            string query = @"SELECT COUNT(1) FROM tb_acesso 
                             WHERE pessoa_tipo = 0 AND pessoa_id = @AlunoId 
                             AND data_hora >= @HojeInicio AND data_hora <= @HojeFim";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            command.AddParameter("@HojeInicio", hojeInicio, DbType.String);
            command.AddParameter("@HojeFim", hojeFim, DbType.String);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            return count % 2 != 0; // Quantidade ímpar no dia = dentro da academia
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_PRESENCA", $"Erro ao verificar presença do aluno: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            string hourExpr = GetDateHourExpression("data_hora");
            string monthExpr = GetDateMonthExpression("data_hora");
            string query = $@"SELECT {hourExpr} AS hora, COUNT(1) AS total 
                              FROM tb_acesso 
                              WHERE pessoa_tipo = 0 AND {monthExpr} = @Mes 
                              GROUP BY hora 
                              ORDER BY total DESC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var resultado = new Dictionary<TimeOnly, int>();
            while (await reader.ReadAsync(cancellationToken))
            {
                int hora = reader.GetInt32Value("hora");
                int total = reader.GetInt32Value("total");
                resultado[new TimeOnly(hora, 0)] = total;
            }
            return resultado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_HORARIO_PROCURADO", $"Erro ao obter horários procurados no mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            var resultado = new Dictionary<int, TimeSpan>
            {
                [mes] = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(15))
            };
            return await Task.FromResult(resultado);
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_PERMANENCIA_MEDIA", $"Erro ao calcular permanência média: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            string dataLimite = DateTime.Today.AddDays(-dias).ToString("yyyy-MM-dd 00:00:00");
            string query = $@"SELECT a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email, 
                                     a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
                                     l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
                              FROM tb_aluno a
                              INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
                              WHERE a.id_aluno NOT IN (
                                  SELECT DISTINCT pessoa_id 
                                  FROM tb_acesso 
                                  WHERE pessoa_tipo = 0 AND data_hora >= @DataLimite
                              )";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@DataLimite", dataLimite, DbType.String);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var alunos = new List<Aluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                int id = reader.GetInt32Value("id_aluno");
                var logradouro = LogradouroRepository.Map(reader, "logradouro_nome");
                var fotoBytes = reader.GetNullableBytes("foto");
                var foto = fotoBytes != null ? Domain.ValueObjects.Arquivo.Criar(fotoBytes).Value : null;

                var aluno = Aluno.Criar(
                    id,
                    reader.GetStringValue("nome"),
                    reader.GetStringValue("cpf"),
                    reader.GetDateOnlyValue("nascimento"),
                    reader.GetStringValue("telefone"),
                    reader.GetStringValue("email"),
                    logradouro,
                    reader.GetStringValue("numero"),
                    reader.GetNullableString("complemento"),
                    reader.GetStringValue("senha"),
                    foto
                ).Value!;

                alunos.Add(aluno);
            }
            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ALUNOS_SEM_ACESSO", $"Erro ao obter alunos sem acesso recente: {ex.Message}", ex);
        }
    }

    public static AcessoAluno Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso");
            int alunoId = reader.GetInt32Value("pessoa_id");
            var dataHora = reader.GetDateTimeValue("data_hora");

            var ctor = typeof(AcessoAluno).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
            if (ctor != null)
            {
                return (AcessoAluno)ctor.Invoke(new object?[] { id, alunoId, dataHora })!;
            }

            throw new InfrastructureException("ERRO_CONSTRUTOR_ACESSO_ALUNO", "Construtor de AcessoAluno não encontrado.");
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", $"Erro ao mapear acesso de aluno: {ex.Message}", ex);
        }
    }
}