//  Felipe Antonio Brüggemann       
using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.Services;
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Domain.Entities;

public class Colaborador : Pessoa
{
    public DateOnly DataAdmissao { get; private set; }
    public ColaboradorTipo Tipo { get; private set; }
    public ColaboradorVinculo Vinculo { get; private set; }

    private Colaborador(
        int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone,
        Email email, Endereco endereco, Senha senha, Arquivo? foto,
        DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
        DataAdmissao = dataAdmissao;
        Tipo = tipo;
        Vinculo = vinculo;
    }

    public static Result<Colaborador> Criar(
        int id, string nome, string cpf, DateOnly dataNascimento, string telefone,
        string email, Logradouro logradouro, string numero, string complemento,
        string senha, byte[]? foto, DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var notifications = new List<Notification>();

        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        var hoje = DateOnly.FromDateTime(DateTime.Today);

        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > hoje.AddYears(-18))
            notifications.Add(new Notification("DataNascimento", "COLABORADOR_MAIORIDADE_OBRIGATORIA"));

        if (dataAdmissao == default)
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_OBRIGATORIO"));
        else if (dataAdmissao > hoje)
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_MAIOR_ATUAL"));

        if (!Enum.IsDefined(tipo))
            notifications.Add(new Notification("Tipo", "TIPO_COLABORADOR_INVALIDO"));

        if (!Enum.IsDefined(vinculo))
            notifications.Add(new Notification("Vinculo", "VINCULO_COLABORADOR_INVALIDO"));

        if (Enum.IsDefined(tipo) && Enum.IsDefined(vinculo) && tipo == ColaboradorTipo.Administrador && vinculo != ColaboradorVinculo.CLT)
            notifications.Add(new Notification("Vinculo", "ADMINISTRADOR_CLT_INVALIDO"));

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
            return Result<Colaborador>.Failure(notifications);

        var colaborador = new Colaborador(
            id, nome, cpfResult.Value!, dataNascimento, telefoneResult.Value!,
            emailResult.Value!, enderecoResult.Value!, senhaResult.Value!, fotoObj,
            dataAdmissao, tipo, vinculo);

        return Result<Colaborador>.Success(colaborador);
    }
}
