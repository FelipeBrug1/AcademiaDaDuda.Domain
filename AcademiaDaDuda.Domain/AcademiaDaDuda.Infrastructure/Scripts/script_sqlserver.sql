IF OBJECT_ID(N'dbo.tb_logradouro', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tb_logradouro (
        id_logradouro INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_tb_logradouro PRIMARY KEY,
        cep NVARCHAR(9) NOT NULL CONSTRAINT ux_tb_logradouro_cep UNIQUE,
        nome NVARCHAR(150) NOT NULL,
        bairro NVARCHAR(100) NOT NULL,
        cidade NVARCHAR(100) NOT NULL,
        estado NVARCHAR(2) NOT NULL,
        pais NVARCHAR(80) NOT NULL CONSTRAINT df_tb_logradouro_pais DEFAULT N'Brasil'
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tb_logradouro') AND name = N'ix_tb_logradouro_cidade')
    CREATE INDEX ix_tb_logradouro_cidade ON dbo.tb_logradouro (cidade);

IF OBJECT_ID(N'dbo.tb_aluno', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tb_aluno (
        id_aluno INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_tb_aluno PRIMARY KEY,
        cpf NVARCHAR(14) NOT NULL CONSTRAINT ux_tb_aluno_cpf UNIQUE,
        nome NVARCHAR(150) NOT NULL,
        nascimento DATE NOT NULL,
        telefone NVARCHAR(20) NOT NULL,
        email NVARCHAR(254) NOT NULL,
        logradouro_id INT NOT NULL,
        numero NVARCHAR(20) NOT NULL,
        complemento NVARCHAR(150) NULL,
        senha NVARCHAR(255) NOT NULL,
        foto VARBINARY(MAX) NULL,
        CONSTRAINT fk_tb_aluno_logradouro FOREIGN KEY (logradouro_id)
            REFERENCES dbo.tb_logradouro (id_logradouro) ON DELETE NO ACTION ON UPDATE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.tb_colaborador', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tb_colaborador (
        id_colaborador INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_tb_colaborador PRIMARY KEY,
        cpf NVARCHAR(14) NOT NULL CONSTRAINT ux_tb_colaborador_cpf UNIQUE,
        nome NVARCHAR(150) NOT NULL,
        nascimento DATE NOT NULL,
        telefone NVARCHAR(20) NOT NULL,
        email NVARCHAR(254) NOT NULL,
        logradouro_id INT NOT NULL,
        numero NVARCHAR(20) NOT NULL,
        complemento NVARCHAR(150) NULL,
        senha NVARCHAR(255) NOT NULL,
        foto VARBINARY(MAX) NULL,
        admissao DATE NOT NULL,
        tipo INT NOT NULL,
        vinculo INT NOT NULL,
        CONSTRAINT fk_tb_colaborador_logradouro FOREIGN KEY (logradouro_id)
            REFERENCES dbo.tb_logradouro (id_logradouro) ON DELETE NO ACTION ON UPDATE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.tb_matricula', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tb_matricula (
        id_matricula INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_tb_matricula PRIMARY KEY,
        aluno_id INT NOT NULL,
        plano INT NOT NULL,
        data_inicio DATE NOT NULL,
        data_fim DATE NOT NULL,
        objetivo NVARCHAR(500) NOT NULL,
        restricao_medica BIT NOT NULL CONSTRAINT df_tb_matricula_restricao DEFAULT 0,
        obs_restricao NVARCHAR(MAX) NULL,
        laudo_medico VARBINARY(MAX) NULL,
        CONSTRAINT fk_tb_matricula_aluno FOREIGN KEY (aluno_id)
            REFERENCES dbo.tb_aluno (id_aluno) ON DELETE CASCADE ON UPDATE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tb_matricula') AND name = N'ix_tb_matricula_aluno_id')
    CREATE INDEX ix_tb_matricula_aluno_id ON dbo.tb_matricula (aluno_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tb_matricula') AND name = N'ix_tb_matricula_data_fim')
    CREATE INDEX ix_tb_matricula_data_fim ON dbo.tb_matricula (data_fim);

IF OBJECT_ID(N'dbo.tb_acesso', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tb_acesso (
        id_acesso INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_tb_acesso PRIMARY KEY,
        pessoa_tipo INT NOT NULL,
        pessoa_id INT NOT NULL,
        data_hora DATETIME2 NOT NULL CONSTRAINT df_tb_acesso_data_hora DEFAULT SYSDATETIME()
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tb_acesso') AND name = N'ix_tb_acesso_pessoa')
    CREATE INDEX ix_tb_acesso_pessoa ON dbo.tb_acesso (pessoa_tipo, pessoa_id);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.tb_acesso') AND name = N'ix_tb_acesso_data_hora')
    CREATE INDEX ix_tb_acesso_data_hora ON dbo.tb_acesso (data_hora);
