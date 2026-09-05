//  Felipe Antonio Brüggemann


namespace AcademiaDaDuda.Domain.ValueObjects
{
    public record Arquivo
    {
        public string NomeArquivo { get; }
        public byte[] Conteudo { get; }

        public Arquivo(string nomeArquivo, byte[] conteudo)
        {
            NomeArquivo = nomeArquivo;
            Conteudo = conteudo;
        }
    }
}
