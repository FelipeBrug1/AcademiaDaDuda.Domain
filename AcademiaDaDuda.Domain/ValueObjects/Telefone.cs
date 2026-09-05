//  Felipe Antonio Brüggemann

namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Telefone
    {
        public string Numero { get; }

        public Telefone(string numero)
        {
            Numero = numero;
        }
    }
}
