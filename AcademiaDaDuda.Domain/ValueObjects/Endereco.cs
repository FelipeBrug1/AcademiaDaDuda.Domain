//  Felipe Antonio Brüggemann

using AcademiaDaDuda.Domain.Entities;

namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Endereco
    {
        public Logradouro Logradouro { get; }
        public string Numero { get; }
        public string Complemento { get; }

        public Endereco(Logradouro logradouro, string numero, string complemento)
        {
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
        }
    }
}
