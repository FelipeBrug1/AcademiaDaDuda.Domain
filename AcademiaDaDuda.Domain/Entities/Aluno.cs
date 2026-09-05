//  Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Services;
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Domain.Entities;

public class Aluno : Pessoa
{
    private Aluno(
        int id, string nome, Cpf cpf, DateOnly dataNascimento,
        Telefone telefone, Email email, Endereco endereco,
        Senha senha, Arquivo? foto)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
    }

    public static Result<Aluno> Criar(
        int id, string nome, string cpf, DateOnly dataNascimento,
        string telefone, string email, Logradouro logradouro, string numero, string complemento,
        string senha, byte[]? foto = null)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        if (dataNascimento == default)
        {
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIA"));
        }
        else
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var idade = hoje.Year - dataNascimento.Year;
            if (dataNascimento > hoje.AddYears(-idade)) idade--;

            if (idade < 12)
                notifications.Add(new Notification("DataNascimento", "ALUNO_IDADE_MINIMA"));
        }

        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure) notifications.AddRange(senhaResult.Notifications);

        var enderecoResult = Endereco.Criar(logradouro, numero, complemento);
        if (enderecoResult.IsFailure) notifications.AddRange(enderecoResult.Notifications);

        Arquivo? fotoObj = null;
        if (foto != null && foto.Length > 0)
        {
            var fotoResult = Arquivo.Criar(foto);
            if (fotoResult.IsFailure) notifications.AddRange(fotoResult.Notifications);
            else fotoObj = fotoResult.Value;
        }

        if (notifications.Count != 0)
            return Result<Aluno>.Failure(notifications);

        var aluno = new Aluno(
            id, nome, cpfResult.Value!, dataNascimento,
            telefoneResult.Value!, emailResult.Value!, enderecoResult.Value!,
            senhaResult.Value!, fotoObj);

        return Result<Aluno>.Success(aluno);
    }
}