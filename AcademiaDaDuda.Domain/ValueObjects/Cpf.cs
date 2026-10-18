//Felipe Antonio Brüggemann 
using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Services;

namespace AcademiaDaDuda.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }
    private Cpf(string valor)
    {
        Valor = valor;
    }
    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");
        var textoLimpo = NormalizadoService.LimparEDigitos(valor);
        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");
        if (!Validar(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");
        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }
    private static bool Validar(string cpf)
    {
        if (cpf.Length != 11 || cpf.All(digito => digito == cpf[0]))
            return false;

        var primeiroDigito = CalcularDigitoVerificador(cpf[..9]);
        var segundoDigito = CalcularDigitoVerificador(cpf[..10]);

        return cpf[9] - '0' == primeiroDigito
            && cpf[10] - '0' == segundoDigito;
    }

    private static int CalcularDigitoVerificador(string digitos)
    {
        var soma = 0;
        for (var i = 0; i < digitos.Length; i++)
            soma += (digitos[i] - '0') * (digitos.Length + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Valor;
}