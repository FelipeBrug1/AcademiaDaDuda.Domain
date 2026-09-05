//  Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Domain.Entities
{
    public class Matricula : Entity
    {
        public Aluno Aluno { get; protected set; }
        public MatriculaPlano Plano { get; protected set; }
        public DateOnly DataInicio { get; protected set; }
        public DateOnly DataFim { get; protected set; }
        public string Objetivo { get; protected set; }
        public MatriculaRestricoes Restricoes { get; protected set; }
        public string? ObservacoesRestricoes { get; protected set; }
        public Arquivo? LaudoMedico { get; protected set; }

        public Matricula(
            int id,
            Aluno aluno,
            MatriculaPlano plano,
            DateOnly dataInicio,
            DateOnly dataFim,
            string objetivo,
            MatriculaRestricoes restricoes = MatriculaRestricoes.None,
            string? observacoesRestricoes = null,
            Arquivo? laudoMedico = null) : base(id)
        {
            Aluno = aluno;
            Plano = plano;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Objetivo = objetivo;
            Restricoes = restricoes;
            ObservacoesRestricoes = observacoesRestricoes;
            LaudoMedico = laudoMedico;
        }
    }
}
