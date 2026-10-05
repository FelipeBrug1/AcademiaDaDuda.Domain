// Felipe Antonio Brüggemann
using AcademiaDaDuda.Infrastructure.Data;

namespace AcademiaDaDuda.Presentation.AppMaui.Configuration
{
    public enum AppDatabaseType
    {
        Sqlite,
        SqlServer,
        MySql
    }

    public static class AppDatabaseTypeExtensions
    {
        public static DatabaseType ToInfrastructure(this AppDatabaseType appType)
        {
            return appType switch
            {
                AppDatabaseType.Sqlite => DatabaseType.Sqlite,
                // Map other types when supported by the infrastructure layer
                _ => throw new NotSupportedException($"Tipo de banco não suportado pela infraestrutura: {appType}")
            };
        }
    }
}
