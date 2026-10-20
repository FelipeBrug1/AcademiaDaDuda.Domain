// Felipe Antonio Brüggemann

using AcademiaDaDuda.Application.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace AcademiaDaDuda.Presentation.AppMaui.Configuration;

public static class DatabaseConnectionStrings
{
    public static string Build(
        AppDatabaseType databaseType,
        string sqlitePath,
        string server,
        int port,
        string database,
        string username,
        string password,
        bool trustServerCertificate)
    {
        return databaseType switch
        {
            AppDatabaseType.Sqlite => new SqliteConnectionStringBuilder
            {
                DataSource = sqlitePath,
                ForeignKeys = true
            }.ToString(),
            AppDatabaseType.MySql => new MySqlConnectionStringBuilder
            {
                Server = server,
                Port = checked((uint)port),
                Database = database,
                UserID = username,
                Password = password,
                AllowBatch = true,
                SslMode = MySqlSslMode.Preferred
            }.ToString(),
            AppDatabaseType.SqlServer => new SqlConnectionStringBuilder
            {
                DataSource = $"{server},{port}",
                InitialCatalog = database,
                UserID = username,
                Password = password,
                Encrypt = true,
                TrustServerCertificate = trustServerCertificate,
                ConnectTimeout = 15
            }.ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Tipo de banco inválido.")
        };
    }
}
