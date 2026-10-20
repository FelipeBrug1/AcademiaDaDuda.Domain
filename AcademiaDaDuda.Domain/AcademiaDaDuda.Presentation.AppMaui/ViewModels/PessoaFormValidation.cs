using AcademiaDaDuda.Application.DTOs;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

internal static class PessoaFormValidation
{
    public static List<string> Validate(PessoaDto pessoa, bool isEditMode, string? confirmarSenha)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(pessoa.Nome))
            errors.Add("Nome é obrigatório.");

        var cpf = DigitsOnly(pessoa.Cpf);
        if (cpf.Length != 11)
            errors.Add("CPF deve conter os 11 dígitos numéricos.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var minimumBirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-12));
        if (pessoa.DataNascimento == default || pessoa.DataNascimento > minimumBirthDate || pessoa.DataNascimento > today)
            errors.Add("O cadastro exige idade mínima de 12 anos.");

        var telefone = DigitsOnly(pessoa.Telefone);
        if (telefone.Length is not (10 or 11))
            errors.Add("Telefone deve conter DDD e 10 ou 11 dígitos.");

        if (!ValidarEmail(pessoa.Email))
            errors.Add("Informe um e-mail válido.");

        var senha = pessoa.Senha?.Trim() ?? string.Empty;
        if (!isEditMode && string.IsNullOrWhiteSpace(senha))
            errors.Add("Senha é obrigatória no cadastro.");
        if (!string.IsNullOrWhiteSpace(senha))
        {
            if (senha.Length < 6 || !senha.Any(char.IsUpper))
                errors.Add("A senha deve ter no mínimo 6 caracteres e uma letra maiúscula.");
            if (!string.Equals(senha, confirmarSenha?.Trim(), StringComparison.Ordinal))
                errors.Add("A confirmação da senha não corresponde.");
        }

        var cep = DigitsOnly(pessoa.Endereco?.Cep);
        if (cep.Length != 8)
            errors.Add("Informe um CEP com 8 dígitos para localizar um logradouro cadastrado.");
        if (string.IsNullOrWhiteSpace(pessoa.Numero))
            errors.Add("Número do endereço é obrigatório.");

        return errors;
    }

    public static string DigitsOnly(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    private static bool ValidarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var parts = email.Trim().Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return false;

        var labels = parts[1].Split('.');
        return labels.Length >= 2 && labels.All(label => !string.IsNullOrWhiteSpace(label));
    }
}
