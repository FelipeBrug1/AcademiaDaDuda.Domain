// Felipe Antonio Brüggemann    
using AcademiaDaDuda.Application.DependencyInjection;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Application.Mappings;
using AcademiaDaDuda.Infrastructure;
using AcademiaDaDuda.Presentation.AppMaui.Configuration;
using AcademiaDaDuda.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDaDuda.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
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

    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        var databaseTypeStr = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        if (Enum.TryParse<AppDatabaseType>(databaseTypeStr, out var databaseType))
        {
        }
        else
        {
            databaseType = AppDatabaseType.Sqlite;
        }

        string connectionString;

        if (databaseType == AppDatabaseType.Sqlite)
        {
            var defaultDbPath = DeviceInfo.Platform == DevicePlatform.WinUI
                ? @"C:\DEV\AcademiaTioAlisson\db_academia_tio_alisson.db"
                : Path.Combine(FileSystem.AppDataDirectory, "db_academia_tio_alisson.db");

            var dbPath = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultDbPath));
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = defaultDbPath;
            }

            var complemento = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
            connectionString = $"Data Source={dbPath};{complemento}";
        }
        else
        {
            var prefix = databaseType == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
            var defaultServer = databaseType == AppDatabaseType.SqlServer ? "172.24.32.1" : "10.30.21.16";
            var defaultUser = databaseType == AppDatabaseType.SqlServer ? "sa" : "root";
            var defaultComplemento = databaseType == AppDatabaseType.SqlServer
                ? "TrustServerCertificate=True; Encrypt=True; Connect Timeout=5; Connection Timeout=5;"
                : "Connection Timeout=5; Default Command Timeout=30;";

            var dbServer = Preferences.Get($"{prefix}_Servidor", Preferences.Get("Servidor", defaultServer));
            var dbDatabase = Preferences.Get($"{prefix}_Banco", Preferences.Get("Banco", "db_academia_tio_alisson"));
            var dbUser = Preferences.Get($"{prefix}_Usuario", Preferences.Get("Usuario", defaultUser));
            var dbPassword = Preferences.Get($"{prefix}_Senha", Preferences.Get("Senha", "abcBolinhas12345"));
            var dbComplemento = Preferences.Get($"{prefix}_Complemento", Preferences.Get("Complemento", defaultComplemento));

            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        return (connectionString, databaseType);
    }
}