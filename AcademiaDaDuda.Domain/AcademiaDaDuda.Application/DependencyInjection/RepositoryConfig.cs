using AcademiaDaDuda.Application.Enums;

namespace AcademiaDaDuda.Application.DependencyInjection;

public class RepositoryConfig
{
    public required string ConnectionString { get; set; }
    public required AppDatabaseType DatabaseType { get; set; }
}
