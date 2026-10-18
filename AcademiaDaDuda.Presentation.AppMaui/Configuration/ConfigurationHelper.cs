// Felipe Antonio Brüggemann
using AcademiaDaDuda.Application.DependencyInjection;
using AcademiaDaDuda.Application.Mappings;
using AcademiaDaDuda.Infrastructure;
using AcademiaDaDuda.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    private const string NomeArquivoBanco = "db_academia_tio_alisson.db";

    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) = ObterConfiguracaoAtual();

        var repoConfig = new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        };

        services.AddSingleton(repoConfig);

        // Assina mensagens para recarga dinâmica de banco sem reiniciar o app
        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(repoConfig, (r, m) =>
        {
            var (novaConnStr, novoDbType) = ObterConfiguracaoAtual();
            r.ConnectionString = novaConnStr;
            r.DatabaseType = novoDbType.ToInfrastructure();
        });

        services.AddApplicationServices();
    }

    public static string ObterCaminhoPadraoSqlite() =>
        Path.Combine(FileSystem.AppDataDirectory, NomeArquivoBanco);

    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        var caminhoPadrao = ObterCaminhoPadraoSqlite();
        var dbPath = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", caminhoPadrao));
        if (string.IsNullOrWhiteSpace(dbPath))
            dbPath = caminhoPadrao;

        var complemento = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
        var connectionString = $"Data Source={dbPath};{complemento}";

        return (connectionString, AppDatabaseType.Sqlite);
    }
}
