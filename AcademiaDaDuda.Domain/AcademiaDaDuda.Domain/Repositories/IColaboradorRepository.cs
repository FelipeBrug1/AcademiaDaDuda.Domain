// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Domain.Repositories;

public interface IColaboradorRepository : IRepository<Colaborador>
{
    Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default);
    Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default);
}