// Felipe Antonio Brüggemann    
using AcademiaDaDuda.Infrastructure.Data;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDaDuda.Infrastructure.Tests;

public abstract class TestBase
{
        protected const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;

    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        ConnectionString = DatabaseType switch
        {
            
            DatabaseType.Sqlite => $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_academia_da_duda.db")};Cache=Shared;",
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };
    }

    #region Geradores de dados aleatórios
    private static int _counter = 10000;
    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + Interlocked.Increment(ref _counter)).ToString("D8")[..8];
    protected static string GerarCpf() => (10000000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    protected static string GerarEmail() => $"user_{Guid.NewGuid():N}"[..18] + "@test.com";
    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    #endregion
}