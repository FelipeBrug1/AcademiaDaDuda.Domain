// Alisson Cordova De Assis

namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Cpf
    {
        public string Valor { get; }

        public Cpf(string valor)
        {
            Valor = valor;
        }
    }
}
