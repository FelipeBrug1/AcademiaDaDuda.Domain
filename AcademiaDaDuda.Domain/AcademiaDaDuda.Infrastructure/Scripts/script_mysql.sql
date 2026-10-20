CREATE TABLE IF NOT EXISTS tb_logradouro (
    id_logradouro INT NOT NULL AUTO_INCREMENT,
    cep VARCHAR(9) NOT NULL,
    nome VARCHAR(150) NOT NULL,
    bairro VARCHAR(100) NOT NULL,
    cidade VARCHAR(100) NOT NULL,
    estado VARCHAR(2) NOT NULL,
    pais VARCHAR(80) NOT NULL DEFAULT 'Brasil',
    PRIMARY KEY (id_logradouro),
    UNIQUE KEY ux_tb_logradouro_cep (cep),
    KEY ix_tb_logradouro_cidade (cidade)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS tb_aluno (
    id_aluno INT NOT NULL AUTO_INCREMENT,
    cpf VARCHAR(14) NOT NULL,
    nome VARCHAR(150) NOT NULL,
    nascimento DATE NOT NULL,
    telefone VARCHAR(20) NOT NULL,
    email VARCHAR(254) NOT NULL,
    logradouro_id INT NOT NULL,
    numero VARCHAR(20) NOT NULL,
    complemento VARCHAR(150) NULL,
    senha VARCHAR(255) NOT NULL,
    foto LONGBLOB NULL,
    PRIMARY KEY (id_aluno),
    UNIQUE KEY ux_tb_aluno_cpf (cpf),
    CONSTRAINT fk_tb_aluno_logradouro FOREIGN KEY (logradouro_id)
        REFERENCES tb_logradouro (id_logradouro) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS tb_colaborador (
    id_colaborador INT NOT NULL AUTO_INCREMENT,
    cpf VARCHAR(14) NOT NULL,
    nome VARCHAR(150) NOT NULL,
    nascimento DATE NOT NULL,
    telefone VARCHAR(20) NOT NULL,
    email VARCHAR(254) NOT NULL,
    logradouro_id INT NOT NULL,
    numero VARCHAR(20) NOT NULL,
    complemento VARCHAR(150) NULL,
    senha VARCHAR(255) NOT NULL,
    foto LONGBLOB NULL,
    admissao DATE NOT NULL,
    tipo INT NOT NULL,
    vinculo INT NOT NULL,
    PRIMARY KEY (id_colaborador),
    UNIQUE KEY ux_tb_colaborador_cpf (cpf),
    CONSTRAINT fk_tb_colaborador_logradouro FOREIGN KEY (logradouro_id)
        REFERENCES tb_logradouro (id_logradouro) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS tb_matricula (
    id_matricula INT NOT NULL AUTO_INCREMENT,
    aluno_id INT NOT NULL,
    plano INT NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NOT NULL,
    objetivo VARCHAR(500) NOT NULL,
    restricao_medica TINYINT(1) NOT NULL DEFAULT 0,
    obs_restricao TEXT NULL,
    laudo_medico LONGBLOB NULL,
    PRIMARY KEY (id_matricula),
    KEY ix_tb_matricula_aluno_id (aluno_id),
    KEY ix_tb_matricula_data_fim (data_fim),
    CONSTRAINT fk_tb_matricula_aluno FOREIGN KEY (aluno_id)
        REFERENCES tb_aluno (id_aluno) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS tb_acesso (
    id_acesso INT NOT NULL AUTO_INCREMENT,
    pessoa_tipo INT NOT NULL,
    pessoa_id INT NOT NULL,
    data_hora DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_acesso),
    KEY ix_tb_acesso_pessoa (pessoa_tipo, pessoa_id),
    KEY ix_tb_acesso_data_hora (data_hora)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
