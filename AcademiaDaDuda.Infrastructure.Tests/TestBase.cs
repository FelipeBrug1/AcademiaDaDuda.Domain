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
    protected static string GerarCpf()
    {
        var numeroBase = (DateTime.UtcNow.Ticks % 1_000_000_000L + Interlocked.Increment(ref _counter)) % 1_000_000_000L;
        var primeirosDigitos = numeroBase.ToString("D9");
        if (primeirosDigitos.All(digito => digito == primeirosDigitos[0]))
            primeirosDigitos = "529982247";

        var primeiroDigito = CalcularDigitoVerificador(primeirosDigitos);
        var segundoDigito = CalcularDigitoVerificador(primeirosDigitos + primeiroDigito);
        return primeirosDigitos + primeiroDigito + segundoDigito;
    }

    private static int CalcularDigitoVerificador(string digitos)
    {
        var soma = 0;
        for (var i = 0; i < digitos.Length; i++)
            soma += (digitos[i] - '0') * (digitos.Length + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
    protected static string GerarEmail() => $"user_{Guid.NewGuid():N}"[..18] + "@test.com";
    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    #endregion
}