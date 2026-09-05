//  Felipe Antonio Brüggemann

namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Senha
    {
        public string Valor { get; }

        public Senha(string valor)
        {
            Valor = valor;
        }
    }
}
