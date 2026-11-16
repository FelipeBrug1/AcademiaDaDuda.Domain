using AcademiaDaDuda.Application.Enums;

namespace AcademiaDaDuda.Application.DTOs;

public sealed record AcessoEventoDto(
    int Id,
    int PessoaId,
    string Nome,
    string Cpf,
    TipoPessoaAcesso TipoPessoa,
    DateTime DataHora,
    bool Entrada);
