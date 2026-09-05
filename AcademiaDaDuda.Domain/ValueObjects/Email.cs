//  Felipe Antonio Brüggemann


namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Email
    {
        public string EnderecoEmail { get; }

        public Email(string enderecoEmail)
        {
            EnderecoEmail = enderecoEmail;
        }
    }
}
