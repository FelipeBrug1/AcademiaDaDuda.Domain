//  Felipe Antonio Brüggemann


namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Cep
    {
        public string Valor { get; }

        public Cep(string valor)
        {
            Valor = valor;
        }
    }
}
