//  Felipe Antonio Brüggemann

using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Services;


namespace AcademiaDaDuda.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string? valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIA");

        if (valor!.Length < 6)
            return Result<Senha>.Failure("Senha", "SENHA_TAMANHO_MINIMO");

        return Result<Senha>.Success(new Senha(valor));
    }

    public override string ToString() => Valor;
}