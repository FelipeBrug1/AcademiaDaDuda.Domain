# Academia da Duda

Aplicativo multiplataforma em .NET MAUI para gestão de academia, com identidade visual própria em tons de roxo e a marca da academia com o nome do autor.

## Funcionalidades entregues

- Cadastro, listagem, busca, edição e exclusão de alunos.
- Cadastro, listagem, busca, edição e exclusão de colaboradores.
- Cadastro, listagem, busca, edição e exclusão de matrículas, com seleção de aluno, plano, vigência calculada, restrições múltiplas e laudo médico.
- Controle de entrada e saída de alunos e colaboradores, com data e hora registradas e histórico do dia.
- Validação de matrícula ativa na entrada do aluno; o painel informa os dias restantes do plano e o tempo de permanência na saída.
- Consulta da situação atual da pessoa na academia e pesquisa por nome ou CPF.
- Busca parcial por CPF com debounce, além de busca por nome, e-mail e identificador nas listas; a busca exata por CPF nos formulários carrega um registro para edição.
- Seleção de foto pela galeria e captura pela câmera, com a foto salva junto ao cadastro.
- Validação imediata de e-mail, opção de exibir senhas e seleção de foto ao tocar na imagem do colaborador.
- Aplicação imediata do tema ao escolhê-lo nas configurações.
- Associação de aluno e colaborador a um logradouro já cadastrado, com validação dos dados pessoais e profissionais.
- Confirmação antes de excluir e mensagem amigável para registros que ainda possuem vínculos.
- Dashboard com a marca da academia e o nome completo do autor, resumo de cadastros e atalho para o controle de acesso.

## Como abrir e compilar

Abra `AcademiaDaDuda.Domain.slnx` no Visual Studio. Para evitar o erro Android APT2265, mantenha a cópia do projeto em um caminho sem acentos ou outros caracteres não ASCII. A cópia usada nesta entrega está em `C:\workspace\AcademiaDaDuda.Domain`.

Selecione o projeto `AcademiaDaDuda.Presentation.AppMaui`, o destino Android e um emulador ou aparelho conectado. A compilação do app exige o SDK .NET MAUI para Android; a implantação exige também o Android SDK e o ADB configurados no Visual Studio.

Os dados SQLite são gravados no diretório local de dados do aplicativo no dispositivo.

## Demonstração para gravação

A sequência para demonstrar o acesso de aluno sem matrícula, aluno matriculado e colaborador está em `docs/Roteiro_Gravacao.txt`. Use dados fictícios e CPFs válidos ainda não cadastrados.

## Repositório

https://github.com/FelipeBrug1/AcademiaDaDuda.Domain
