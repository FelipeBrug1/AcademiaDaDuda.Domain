using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Enums;
using AcademiaDaDuda.Presentation.AppMaui.Localization;

namespace AcademiaDaDuda.Presentation.AppMaui.ViewModels;

public sealed class AcessoEventoLinha(AcessoEventoDto evento)
{
    public string Nome { get; } = evento.Nome;
    public string Cpf { get; } = evento.Cpf;
    public string Movimento { get; } = LocalizationManager.Instance.Translate(evento.Entrada ? "Entrada" : "Saída");
    public string TipoPessoa { get; } = LocalizationManager.Instance.Translate(evento.TipoPessoa == TipoPessoaAcesso.Aluno ? "Aluno" : "Colaborador");
    public string Horario { get; } = evento.DataHora.ToString("t", LocalizationManager.Instance.CurrentCulture);
    public string Descricao => $"{Movimento} · {TipoPessoa}";
}
