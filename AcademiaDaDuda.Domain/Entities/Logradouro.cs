//  Felipe Antonio Brüggemann
namespace AcademiaDaDuda.Domain.Entities
{
    public class Logradouro : Entity
    {
        public Cep Cep { get; protected set; }
        public string Pais { get; protected set; }
        public string Estado { get; protected set; }
        public string Cidade { get; protected set; }
        public string Bairro { get; protected set; }
        public string NomeLogradouro { get; protected set; }

        public Logradouro(int id, Cep cep, string pais, string estado, string cidade, string bairro, string nomeLogradouro)
            : base(id)
        {
            Cep = cep;
            Pais = pais;
            Estado = estado;
            Cidade = cidade;
            Bairro = bairro;
            NomeLogradouro = nomeLogradouro;
        }
    }
}
