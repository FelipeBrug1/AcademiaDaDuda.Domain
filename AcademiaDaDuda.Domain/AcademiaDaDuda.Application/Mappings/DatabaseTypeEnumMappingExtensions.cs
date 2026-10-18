using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Infrastructure.Data;

namespace AcademiaDaDuda.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType databaseType) =>
        databaseType switch
        {
            AppDatabaseType.Sqlite => DatabaseType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Tipo de banco não suportado pela infraestrutura.")
        };

    public static AppDatabaseType ToApplication(this DatabaseType databaseType) =>
        databaseType switch
        {
            DatabaseType.Sqlite => AppDatabaseType.Sqlite,
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, "Tipo de banco não suportado pela aplicação.")
        };
}
