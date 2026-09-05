//  Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Common;

namespace AcademiaDaDuda.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHoraChegada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    private AcessoColaborador(int id, Colaborador colaborador, DateTime dataHoraChegada, DateTime? dataHoraSaida = null) : base(id)
    {
        Colaborador = colaborador;
        DataHoraChegada = dataHoraChegada;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoColaborador> CriarEntrada(int id, Colaborador colaborador)
    {
        var notifications = new List<Notification>();

        if (colaborador == null)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_OBRIGATORIO"));

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador!, DateTime.Now));
    }

    public Result<AcessoColaborador> RegistrarSaida()
    {
        if (DataHoraSaida.HasValue)
            return Result<AcessoColaborador>.Failure("AcessoColaborador", "SAIDA_JA_REGISTRADA");

        DataHoraSaida = DateTime.Now;
        return Result<AcessoColaborador>.Success(this);
    }
}