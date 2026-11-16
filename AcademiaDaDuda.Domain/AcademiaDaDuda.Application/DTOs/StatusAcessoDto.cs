namespace AcademiaDaDuda.Application.DTOs;

public sealed record StatusAcessoDto(
    bool EstaNaAcademia,
    bool? MatriculaAtiva,
    DateTime? UltimaEntrada,
    int? DiasRestantesPlano);
