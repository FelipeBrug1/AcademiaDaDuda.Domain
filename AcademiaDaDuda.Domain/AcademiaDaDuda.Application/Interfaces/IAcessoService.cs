using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;

namespace AcademiaDaDuda.Application.Interfaces;

public interface IAcessoService
{
    Task<IEnumerable<AcessoPessoaDto>> ObterPessoasAsync(TipoPessoaAcesso tipoPessoa, CancellationToken cancellationToken = default);
    Task<StatusAcessoDto?> ObterStatusAsync(TipoPessoaAcesso tipoPessoa, int pessoaId, DateTime dataHora, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoEventoDto>> ObterEventosDoDiaAsync(DateOnly data, CancellationToken cancellationToken = default);
    Task<ResultadoRegistroAcessoDto> RegistrarAcessoAsync(TipoPessoaAcesso tipoPessoa, int pessoaId, DateTime dataHora, CancellationToken cancellationToken = default);
}
