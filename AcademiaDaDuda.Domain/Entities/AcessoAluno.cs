//  Felipe Antonio Brüggemann

namespace AcademiaDaDuda.Domain.Entities
{
    public class AcessoAluno : Entity
    {
        public Aluno Aluno { get; protected set; }
        public DateTime DataHoraChegada { get; protected set; }
        public DateTime? DataHoraSaida { get; protected set; }

        public AcessoAluno(int id, Aluno aluno, DateTime dataHoraChegada, DateTime? dataHoraSaida = null) : base(id)
        {
            Aluno = aluno;
            DataHoraChegada = dataHoraChegada;
            DataHoraSaida = dataHoraSaida;
        }
    }
}
