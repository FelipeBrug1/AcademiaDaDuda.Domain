//  Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Common;
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.Enums;
using AcademiaDaDuda.Domain.Services;
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Domain.Entities;

public class Matricula : Entity
{
    public Aluno AlunoMatricula { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes RestricoesMedicas { get; private set; }
    public string? ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    private Matricula(
        int id, Aluno alunoMatricula, MatriculaPlano plano, DateOnly dataInicio,
        DateOnly dataFim, string objetivo, MatriculaRestricoes restricoesMedicas,
        Arquivo? laudoMedico, string? observacoesRestricoes) : base(id)
    {
        AlunoMatricula = alunoMatricula;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Objetivo = objetivo;
        RestricoesMedicas = restricoesMedicas;
        LaudoMedico = laudoMedico;
        ObservacoesRestricoes = observacoesRestricoes;
    }

    public static Result<Matricula> Criar(
        int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio,
        string objetivo, MatriculaRestricoes restricoesMedicas = MatriculaRestricoes.None,
        string? observacoesRestricoes = null, byte[]? laudoMedico = null)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

        if (!Enum.IsDefined(plano))
            notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));

        if (dataInicio == default)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIA"));

        if (NormalizadoService.TextoVazioOuNulo(objetivo))
            notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else
            objetivo = NormalizadoService.LimparEspacos(objetivo);

        DateOnly dataFim = plano switch
        {
            MatriculaPlano.Mensal => dataInicio.AddMonths(1),
            MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
            MatriculaPlano.Semestral => dataInicio.AddMonths(6),
            MatriculaPlano.Anual => dataInicio.AddYears(1),
            _ => dataInicio
        };

        Arquivo? laudoObj = null;
        if (laudoMedico != null && laudoMedico.Length > 0)
        {
            var laudoResult = Arquivo.Criar(laudoMedico);
            if (laudoResult.IsFailure) notifications.AddRange(laudoResult.Notifications);
            else laudoObj = laudoResult.Value;
        }

        if (aluno != null)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var idade = hoje.Year - aluno.DataNascimento.Year;
            if (aluno.DataNascimento > hoje.AddYears(-idade)) idade--;

            if (idade >= 12 && idade <= 16 && laudoObj == null)
                notifications.Add(new Notification("LaudoMedico", "LAUDO_MEDICO_OBRIGATORIO_MENOR"));

            if (restricoesMedicas != MatriculaRestricoes.None && laudoObj == null)
                notifications.Add(new Notification("LaudoMedico", "LAUDO_MEDICO_OBRIGATORIO_RESTRICAO"));
        }

        if (notifications.Count != 0)
            return Result<Matricula>.Failure(notifications);

        var matricula = new Matricula(
            id, aluno!, plano, dataInicio, dataFim, objetivo,
            restricoesMedicas, laudoObj, NormalizadoService.LimparEspacos(observacoesRestricoes));

        return Result<Matricula>.Success(matricula);
    }
}