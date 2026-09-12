// Felipe Antonio Brüggemann
using AcademiaDaDuda.Domain.Entities;
using AcademiaDaDuda.Domain.ValueObjects;


namespace AcademiaDaDuda.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    // ==========================================
    // CEP (10 Testes)
    // ==========================================
    [Theory(DisplayName = "Cep: dígitos inválidos -> CEP_DIGITOS")]
    [InlineData("123")]
    [InlineData("12-345")]
    [InlineData("12345-67")]
    [InlineData("123456789")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos(string input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData("12345-678")]
    [InlineData("12345678")]
    [InlineData(" 88500-000 ")]
    public void Deve_Criar_Cep_Quando_Valido(string input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(input.Replace("-", "").Trim(), result.Value!.Valor);
    }

    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio(string? input)
    {
        var result = Cep.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    // ==========================================
    // ENDEREÇO (6 Testes)
    // ==========================================
    [Theory(DisplayName = "Endereco: criação válida com número e complemento")]
    [InlineData("10", "Bloco A")]
    [InlineData("1", "")]
    [InlineData("100", "Apto 202")]
    public void Deve_Criar_Endereco_Quando_Valido(string numero, string complemento)
    {
        var logradouro = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
        var result = Endereco.Criar(logradouro, numero, complemento);
        Assert.True(result.IsSuccess);
        Assert.Equal(logradouro.Id, result.Value!.LogradouroId);
        Assert.Equal(numero, result.Value.Numero);
        Assert.Equal(complemento, result.Value.Complemento);
    }

    [Theory(DisplayName = "Endereco: valida obrigatoriedade do logradouro e número")]
    [InlineData(null, "1", "LOGRADOURO_OBRIGATORIO")]
    [InlineData("valid", "", "NUMERO_OBRIGATORIO")]
    [InlineData("valid", "   ", "NUMERO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_EnderecoInvalido(string? logradouroCase, string numero, string expected)
    {
        Logradouro? logradouro = null;
        if (logradouroCase == "valid")
            logradouro = Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco.Criar(logradouro!, numero, "");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == expected);
    }

    // ==========================================
    // CPF (14 Testes)
    // ==========================================
    [Theory(DisplayName = "Cpf: nulo/vazio/espaços -> CPF_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Single(result.Notifications);
        Assert.Equal("CPF_OBRIGATORIO", result.Notifications.First().Mensagem);
    }

    [Theory(DisplayName = "Cpf: formatos válidos com 11 dígitos")]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    public void Deve_Criar_Cpf_Quando_ValorValido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(input.Replace(".", "").Replace("-", "").Trim(), result.Value!.Valor);
    }

    [Theory(DisplayName = "Cpf: quantidade incorreta de dígitos -> CPF_DIGITOS")]
    [InlineData("123.456.789-0")]
    [InlineData("111.111.111-1")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Criacao_Quando_CpfInvalido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }

    [Theory(DisplayName = "Cpf: sem dígitos -> CPF_DIGITOS")]
    [InlineData(" dfgdf ")]
    [InlineData("abc")]
    [InlineData("!@#$%")]
    public void Deve_Falhar_Criacao_Quando_CpfSemDigitos(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }

    // ==========================================
    // TELEFONE (10 Testes)
    // ==========================================
    [Theory(DisplayName = "Telefone: dígitos inválidos -> TELEFONE_DIGITOS")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    [InlineData("119123456")]
    [InlineData("119123456789")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Theory(DisplayName = "Telefone: formatos válidos (com e sem formatação)")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    [InlineData("49999998888")]
    public void Deve_Criar_Telefone_Quando_Valido(string input)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(input.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", ""), result.Value!.Valor);
    }

    [Theory(DisplayName = "Telefone: obrigatório -> TELEFONE_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    // ==========================================
    // SENHA (8 Testes)
    // ==========================================
    [Theory(DisplayName = "Senha: valida requisito de uppercase e tamanho mínimo")]
    [InlineData("abcdef", false)]
    [InlineData("Abcdef", true)]
    [InlineData("123456", false)]
    [InlineData("Senha123", true)]
    [InlineData("abc", false)]
    public void Deve_Validar_RequisitoUppercase_Senha(string senha, bool isSuccess)
    {
        var result = Senha.Criar(senha);
        Assert.Equal(isSuccess, result.IsSuccess);
    }

    [Theory(DisplayName = "Senha: obrigatório -> SENHA_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia(string? input)
    {
        var result = Senha.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIO");
    }

    // ==========================================
    // ARQUIVO (3 Testes)
    // ==========================================
    [Fact(DisplayName = "Arquivo: criação válida")]
    public void Deve_Criar_Arquivo_Quando_Valido()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var result = Arquivo.Criar(bytes);
        Assert.True(result.IsSuccess);
        Assert.Equal(bytes, result.Value!.Conteudo);
    }

    [Fact(DisplayName = "Arquivo: falha quando conteúdo é nulo")]
    public void Deve_Falhar_Criacao_Quando_ArquivoNulo()
    {
        var result = Arquivo.Criar(null!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo: tamanho excede o limite")]
    public void Deve_Falhar_Criacao_Quando_ArquivoTamanhoInvalido()
    {
        var bytesGrandes = new byte[16 * 1024 * 1024]; // 16MB
        var result = Arquivo.Criar(bytesGrandes);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }

    // ==========================================
    // EMAIL (10 Testes)
    // ==========================================
    [Theory(DisplayName = "Email: remove espaços e aceita formato válido")]
    [InlineData(" user@example.com ", "user@example.com")]
    [InlineData("TESTE@DOMINIO.COM.BR", "TESTE@DOMINIO.COM.BR")]
    [InlineData("alisson@dominio.com", "alisson@dominio.com")]
    public void Deve_Criar_Email_E_Remove_Espacos_Quando_InputTemEspacos(string input, string expected)
    {
        var result = Email.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected.Trim(), result.Value!.Valor, ignoreCase: true);
    }

    [Theory(DisplayName = "Email: formato inválido -> EMAIL_FORMATO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sem_arroba.com")]
    [InlineData("usuario@")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@dominio")]
    [InlineData("usuario@dominio.")]
    public void Deve_Falhar_Criacao_Quando_FormatoEmailInvalido(string? input)
    {
        var result = Email.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }
}