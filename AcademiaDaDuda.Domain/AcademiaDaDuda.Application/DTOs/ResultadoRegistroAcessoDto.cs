namespace AcademiaDaDuda.Application.DTOs;

public sealed record ResultadoRegistroAcessoDto(
    bool Registrado,
    bool Entrada,
    string Codigo,
    string Nome,
    DateTime DataHora,
    int? DiasRestantesPlano = null,
    TimeSpan? Permanencia = null);
