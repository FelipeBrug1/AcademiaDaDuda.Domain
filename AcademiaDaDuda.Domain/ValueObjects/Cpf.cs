// Alisson Cordova De Assis

using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Services;
    
namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Cpf
    {
        public string Valor { get; }

        public Cpf(string valor)
        {
            Valor = valor;
        }
        public static Result<Cpf> Criar(string? valor)
        {
            if (NormalizadoService.TextoVazioOuNulo(valor))
                return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

            var textoLimpo = NormalizadoService.LimparEDigitos(valor);
            if (textoLimpo.Length != 11 || !ValidarCpfMatematica(textoLimpo))
                return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

            return Result<Cpf>.Success(new Cpf(textoLimpo));
        }

        private static bool ValidarCpfMatematica(string cpf)
        {
            if (cpf.Distinct().Count() == 1) return false;

            int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            string tempCpf = cpf[..9];
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += (tempCpf[i] - '0') * multiplicador1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += (tempCpf[i] - '0') * multiplicador2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cpf.EndsWith($"{digito1}{digito2}");
        }

        public override string ToString() => Valor;
    }
}