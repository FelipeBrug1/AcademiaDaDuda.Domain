using AcademiaDaDuda.Domain.Common;

namespace AcademiaDaDuda.Domain.Entities;

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; private set; }
    public DateTime DataHoraChegada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    private AcessoAluno(int id, Aluno aluno, DateTime dataHoraChegada, DateTime? dataHoraSaida = null) : base(id)
    {
        Aluno = aluno;
        DataHoraChegada = dataHoraChegada;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoAluno> CriarEntrada(int id, Aluno aluno)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);

        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno!, DateTime.Now));
    }

    public Result<AcessoAluno> RegistrarSaida()
    {
        if (DataHoraSaida.HasValue)
            return Result<AcessoAluno>.Failure("AcessoAluno", "SAIDA_JA_REGISTRADA");

        DataHoraSaida = DateTime.Now;
        return Result<AcessoAluno>.Success(this);
    }
}