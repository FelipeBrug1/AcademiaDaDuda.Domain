// Felipe Antonio Brüggemann 
using AcademiaDaDuda.Infrastructure.Data;

namespace AcademiaDaDuda.Application.DependencyInjection;

public class RepositoryConfig
{
    public required string ConnectionString { get; set; }
    public required DatabaseType DatabaseType { get; set; }
}