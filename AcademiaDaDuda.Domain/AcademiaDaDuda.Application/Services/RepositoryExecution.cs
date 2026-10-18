namespace AcademiaDaDuda.Application.Services;

internal static class RepositoryExecution
{
    public static async Task<TResult> ExecuteAsync<TRepository, TResult>(
        Func<TRepository> factory,
        Func<TRepository, Task<TResult>> operation)
    {
        var repository = factory();
        try
        {
            return await operation(repository);
        }
        finally
        {
            if (repository is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (repository is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
