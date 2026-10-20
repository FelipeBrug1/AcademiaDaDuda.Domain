# Academia da Duda

Aplicativo multiplataforma em .NET MAUI para gestão de academia, com identidade visual própria em tons de roxo e a marca da academia com o nome do autor.

## Funcionalidades entregues

- Cadastro, listagem, busca, edição e exclusão de alunos.
- Cadastro, listagem, busca, edição e exclusão de colaboradores.
- Busca por nome, CPF, e-mail e identificador nas listas; busca por CPF também carrega um registro para edição diretamente nos formulários.
- Seleção de foto pela galeria e captura pela câmera, com a foto salva junto ao cadastro.
- Associação de aluno e colaborador a um logradouro já cadastrado, com validação dos dados pessoais e profissionais.
- Confirmação antes de excluir e mensagem amigável para registros que ainda possuem vínculos.
- Atalhos funcionais no dashboard para Alunos, Colaboradores e Matrículas.

## Como abrir e compilar

Abra `AcademiaDaDuda.Domain.slnx` no Visual Studio. Para evitar o erro Android APT2265, mantenha a cópia do projeto em um caminho sem acentos ou outros caracteres não ASCII. A cópia usada nesta entrega está em `C:\workspace\AcademiaDaDuda.Domain`.

Selecione o projeto `AcademiaDaDuda.Presentation.AppMaui`, o destino Android e um emulador ou aparelho conectado. A compilação do app exige o SDK .NET MAUI para Android; a implantação exige também o Android SDK e o ADB configurados no Visual Studio.

Os dados SQLite são gravados no diretório local de dados do aplicativo no dispositivo.

## Demonstração para gravação

A sequência de gravação está em `docs/Roteiro_Gravacao.txt`. Use CPFs e e-mails de teste que ainda não existam no banco local.

## Repositório

https://github.com/FelipeBrug1/AcademiaDaDuda.Domain
