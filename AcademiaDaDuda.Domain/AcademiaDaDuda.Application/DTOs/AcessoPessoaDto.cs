namespace AcademiaDaDuda.Application.DTOs;

public sealed record AcessoPessoaDto(int Id, string Nome, string Cpf)
{
    public string NomeCpf => $"ID: {Id} · {Nome} ({Cpf})";
}
