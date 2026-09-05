//  Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Domain.Entities
{
    public class Aluno : Pessoa
    {
        public Aluno(
            int id,
            string nome,
            Cpf cpf,
            DateOnly dataNascimento,
            Telefone telefone,
            Email email,
            Senha senha,
            Endereco endereco,
            Arquivo? foto = null)
            : base(id, nome, cpf, dataNascimento, telefone, email, senha, endereco, foto)
        {
        }
    }
}
