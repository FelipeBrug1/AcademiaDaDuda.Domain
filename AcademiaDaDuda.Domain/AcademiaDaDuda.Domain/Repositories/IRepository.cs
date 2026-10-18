// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Entities;

namespace AcademiaDaDuda.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : Entity, IAggregateRoot
{
    Task<TEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> ObterTodos(CancellationToken cancellationToken = default);
    Task<TEntity> Adicionar(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> Atualizar(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> Remover(int id, CancellationToken cancellationToken = default);
}