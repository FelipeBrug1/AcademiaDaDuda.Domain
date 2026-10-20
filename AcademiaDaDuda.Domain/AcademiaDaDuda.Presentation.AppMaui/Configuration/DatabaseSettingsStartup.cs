using AcademiaDaDuda.Application.DependencyInjection;
using AcademiaDaDuda.Application.Enums;
using Microsoft.Maui.Storage;

namespace AcademiaDaDuda.Presentation.AppMaui.Configuration;

public sealed class DatabaseSettingsStartup(RepositoryConfig repositoryConfig)
{
    public const string DatabaseTypeKey = "settings.database.type";
    public const string SqlitePathKey = "settings.database.sqlite_path";
    public const string ServerKey = "settings.database.server";
    public const string PortKey = "settings.database.port";
    public const string NameKey = "settings.database.name";
    public const string UsernameKey = "settings.database.username";
    public const string PasswordKey = "settings.database.password";
    public const string TrustCertificateKey = "settings.database.trust_certificate";
    public const string ThemeKey = "settings.theme";

    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized)
                return;

            var defaultPath = Path.Combine(FileSystem.AppDataDirectory, "academia-da-duda.db3");
            var sqlitePath = Preferences.Get(SqlitePathKey, defaultPath);
            var databaseTypeValue = Preferences.Get(DatabaseTypeKey, nameof(AppDatabaseType.Sqlite));

            if (!Enum.TryParse<AppDatabaseType>(databaseTypeValue, ignoreCase: true, out var databaseType))
                databaseType = AppDatabaseType.Sqlite;

            if (databaseType == AppDatabaseType.Sqlite)
            {
                repositoryConfig.DatabaseType = databaseType;
                repositoryConfig.ConnectionString = DatabaseConnectionStrings.Build(
                    databaseType, sqlitePath, string.Empty, 0, string.Empty, string.Empty, string.Empty, false);
            }
            else
            {
                var server = Preferences.Get(ServerKey, string.Empty);
                var port = Preferences.Get(PortKey, databaseType == AppDatabaseType.MySql ? 3306 : 1433);
                var database = Preferences.Get(NameKey, string.Empty);
                var username = Preferences.Get(UsernameKey, string.Empty);
                string password;
                try
                {
                    password = await SecureStorage.Default.GetAsync(PasswordKey) ?? string.Empty;
                }
                catch (Exception)
                {
                    password = string.Empty;
                }
                var trustCertificate = Preferences.Get(TrustCertificateKey, true);

                if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(database))
                {
                    repositoryConfig.DatabaseType = databaseType;
                    repositoryConfig.ConnectionString = DatabaseConnectionStrings.Build(
                        databaseType, sqlitePath, server, port, database, username, password, trustCertificate);
                }
            }

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }
}
