using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Infrastructure.Data;

namespace AcademiaDaDuda.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType databaseType) =>
        databaseType switch
        {
            AppDatabaseType.Sqlite => DatabaseType.Sqlite,
            AppDatabaseType.MySql => DatabaseType.MySql,
            AppDatabaseType.SqlServer => DatabaseType.SqlServer,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Tipo de banco não suportado pela infraestrutura.")
        };

    public static AppDatabaseType ToApplication(this DatabaseType databaseType) =>
        databaseType switch
        {
            DatabaseType.Sqlite => AppDatabaseType.Sqlite,
            DatabaseType.MySql => AppDatabaseType.MySql,
            DatabaseType.SqlServer => AppDatabaseType.SqlServer,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Tipo de banco não suportado pela aplicação.")
        };
}
