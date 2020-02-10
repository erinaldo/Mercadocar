-------------------------------------------------------------------------------
--  fmoraes 12/11/2019
--  DM 202755
--  Política de créditos
-------------------------------------------------------------------------------

--USE MCAR_Desenvolvimento
USE MCAR_Chamados
GO

-- ============================================================================
-- RESUMO DAS HISTÓRIAS
-- ============================================================================


-- US02 =======================================================================

-- PROCEDURES:
-- sp p_Venda_Tecnica_Consultar_Romaneio_Pesquisa_demanda_202755_US02
-- sp P_Venda_Tecnica_Consultar_Romaneio_Liberado_demanda_202755_US02

-- ENUMERADOS:
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Enum_Tipo_Troca'

-- TABELA:
-- ALTER TABLE Romaneio_Venda_CT ADD Enum_Tipo_Troca_ID INT;

-- CONSULTA:
 SELECT 
Romaneio_Pre_Venda_CT_ID,
Enum_Status_ID,
Romaneio_Venda_CT_Cliente_Nome,
Romaneio_Venda_CT_Cliente_CNPJCPF 
FROM Romaneio_Venda_CT 
WHERE Enum_Tipo_Troca_ID > 0
ORDER BY Romaneio_Venda_CT_Data_Geracao DESC

-- US03 =======================================================================

-- PROCEDURES:
-- SP p_Jobs_Pendencias_Validacao
 EXEC p_Jobs_Pendencias_Validacao

-- ENUMERADOS:
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Processo_Validacao'
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Status_Validacao'

-- TABELA:
-- Pendencia_Validacao

-- CONSULTA:
 SELECT * FROM Pendencia_Validacao

-- Script Permissão US03

/*
INSERT INTO Parametros_Sistema 
(
Lojas_ID, 
Parametros_Sistema_Tipo, 
Parametros_Sistema_Valor, 
Parametros_Sistema_Observacao
)
VALUES (1,'CREDITO_PAGO_ACIMA_DE', '500', '')
*/

-- US04 =======================================================================

-- PROCEDURES:
-- SP p_Venda_Tecnica_Consultar_Romaneio_Venda_Grid_DM_202755_US04

-- ENUMERADOS:
-- SELECT * FROM Enumerado WHERE Enum_Extenso = 'Romaneio de crédito' AND Enum_Nome = 'Solicitacao_Pagamento_Objeto_Origem'

-- TABELA:
-- ALTER TABLE Solicitacao_Pagamento ADD Lojas_ID INT NOT NULL DEFAULT 17 -- E-commerce

-- CONSULTA:
 SELECT * FROM Solicitacao_Pagamento

-- Script Permissão US04

-- US05 =======================================================================

-- PROCEDURES:
-- SP p_Jobs_Interface_Deposito
-- EXEC p_Jobs_Interface_Deposito

-- TABELA:
-- Interface_Deposito

-- CONSULTA:
-- SELECT * FROM Interface_Deposito


-- ============================================================================
-- UTILITARIOS
-- ============================================================================

/*
SELECT * FROM Usuario WHERE Usuario_Login = 'fmoraes'
SELECT * FROM Usuario WHERE Usuario_ID = 7533

-------------------------------------------------------------------------------
-- PESQUISA

-- O campo do TYPE armazena o tipo do objeto a ser localizado, onde :
-- U => Tabela Usuário
-- S => Tabela de sistema
-- P => Procedure
-- V => View
-- F => Function
 
SELECT A.NAME, A.TYPE, B.TEXT
  FROM SYSOBJECTS  A (nolock)
  JOIN SYSCOMMENTS B (nolock) 
    ON A.ID = B.ID
WHERE B.TEXT LIKE '%feriado%'  --- Informação a ser procurada no corpo da procedure, funcao ou view
  AND A.TYPE = 'P' --- Tipo de objeto a ser localizado no caso procedure
 ORDER BY A.NAME
 
*/

-------------------------------------------------------------------------------

-- SELECT TOP 1 @Usuario_Sistema = Usuario.Usuario_ID
-- FROM Usuario
-- WHERE Usuario.Usuario_Login LIKE 'Sistema';


-- ============================================================================
-- US02 - CRIAR NOVO TIPO DE TROCA
-- ============================================================================
/*
ALTER TABLE Romaneio_Venda_CT ADD Enum_Tipo_Troca_ID INT;

INSERT INTO Enumerado (Enum_Sigla, Enum_Extenso, Enum_Nome, Enum_IsAtivo)
VALUES ('','Filtro por peça', 'Enum_Tipo_Troca', 1)
*/
-------------------------------------------------------------------------------

-- ENUMERADOS:

-- SELECT * FROM Enumerado WHERE Enum_Nome = 'TipoRomaneio'
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Status_Romaneio_Venda'

-- CONSULTAS:

-- SELECT * FROM MCAR_Chamados.dbo.Romaneio_Venda_CT WHERE Lojas_ID = 1 AND Romaneio_Pre_Venda_CT_ID IN(16366071)
-- SELECT * FROM MCAR_Chamados.dbo.Romaneio_Venda_Grupo WHERE Romaneio_Venda_Grupo_ID = 7495411

-- 1688-0083-0642 (sem estoque)
-- 0948-0530-0090
-- 0955-0160-0362
-- 0079-0015-0141
-- 0079-0015-0142
-- 0200-0015-0667
-- 0227-0266-0215
-- 1569-0235-0021
-- 0624-0530-0032
-- 0543-0090-0120
-- 0925-0162-0042

-------------------------------------------------------------------------------

/*
SELECT TOP(10) 
Romaneio_Venda_CT_ID,
Romaneio_Pre_Venda_CT_ID,
Romaneio_Venda_CT_Cliente_Nome,
Romaneio_Venda_CT_Cliente_CNPJCPF
FROM Romaneio_Venda_CT 
WHERE Lojas_ID = 1 
AND Enum_Tipo_ID NOT IN(550, 551)
AND Enum_Status_ID = 1398
AND Romaneio_Venda_CT_Data_Geracao < '2020-01-15' 
ORDER BY Romaneio_Venda_CT_ID DESC
*/
-------------------------------------------------------------------------------
/*
SELECT 
Romaneio_Venda_CT_ID,
Lojas_ID,
Enum_Tipo_ID,
Enum_Status_ID,
Enum_Motivo_Troca_ID,
Usuario_Gerente_ID,
Objeto_Origem_ID,
Loja_Origem_ID,
Romaneio_Venda_CT_Cliente_Nome,
Romaneio_Pre_Venda_CT_ID,
Usuario_Aprovacao_Credito_ID,
Enum_Tipo_Troca_ID
FROM Romaneio_Venda_CT WHERE Enum_Tipo_Troca_ID > 0
*/

-------------------------------------------------------------------------------
-- QUALIDADE
-------------------------------------------------------------------------------

-- BUG 73031 - Teste em Chamados - Teste DM 202755 US02

-- Chamados
-- 0553-0015-0860 - Peça ID: 53158
-- 0553-0015-1154 - Peça ID: 158674

-- Dinheiro
-- SELECT * FROM Condicao_Pagamento WHERE Condicao_Pagamento_ID = 115
-- SELECT * FROM Forma_Pagamento WHERE Forma_Pagamento_ID = 1

-- SELECT * FROM Solicitacao_Garantia_CT WHERE Solicitacao_Garantia_CT_ID IN (111041)
-- SELECT * FROM Romaneio_Venda_Origem WHERE Lojas_ID = 1 AND Romaneio_Pre_Venda_CT_ID IN (16366050)

-- Tabelas Antigas
-- SELECT * FROM Romaneio_Pre_Venda_CT WHERE Lojas_ID = 1 AND Romaneio_Pre_Venda_CT_ID IN (16366050)

-- Tabelas Novas
-- SELECT * FROM Romaneio_Venda_CT WHERE Lojas_ID = 1 AND Romaneio_Pre_Venda_CT_ID IN (16366050)


-- ============================================================================
-- US03 - TELA PARA CONTROLE DE VALIDAÇÕES
-- ============================================================================

/*

-- ENUMERADO Processo_Validacao

INSERT INTO Enumerado (Enum_Sigla, Enum_Extenso, Enum_Nome, Enum_IsAtivo)
  VALUES ('','Crédito de peça sem origem', 'Processo_Validacao', 1)
INSERT INTO Enumerado (Enum_Sigla, Enum_Extenso, Enum_Nome, Enum_IsAtivo)
  VALUES ('','Devolução em dinheiro', 'Processo_Validacao', 1)


-- ENUMERADO Status_Validacao

INSERT INTO Enumerado (Enum_Sigla, Enum_Extenso, Enum_Nome, Enum_IsAtivo)
  VALUES ('','Pendente', 'Status_Validacao', 1)
INSERT INTO Enumerado (Enum_Sigla, Enum_Extenso, Enum_Nome, Enum_IsAtivo)
  VALUES ('','Validado', 'Status_Validacao', 1)

-------------------------------------------------------------------------------

DROP TABLE Pendencia_Validacao

CREATE TABLE Pendencia_Validacao
(
     Pendencia_Validacao_ID             INT IDENTITY(1, 1) NOT NULL, 
     Lojas_ID                           INT, 
     Objeto_Origem_ID                   BIGINT, 
     Cargo_ID                           INT, 
     Enum_Processo_ID                   INT, 
     Enum_Status_ID                     INT, 
     Usuario_Geracao_ID                 INT, 
     Usuario_Validacao_ID               INT, 
     Pendencia_Validacao_Data_Geracao   DATETIME, 
     Pendencia_Validacao_Data_Validacao DATETIME, 
     Pendencia_Validacao_Valor          DECIMAL(18, 2), 
     Pendencia_Validacao_Observacao     VARCHAR(1000)

     CONSTRAINT [PK_Pendencia_Validacao] PRIMARY KEY CLUSTERED ([Pendencia_Validacao_ID] ASC)
     WITH(
   PAD_INDEX = OFF, 
   STATISTICS_NORECOMPUTE = OFF, 
   IGNORE_DUP_KEY = OFF, 
   ALLOW_ROW_LOCKS = ON, 
   ALLOW_PAGE_LOCKS = ON
   ) ON [PRIMARY]
) ON [PRIMARY];

-------------------------------------------------------------------------------

-- DROP TABLE Pendencia_Validacao_Configuracao

CREATE TABLE Pendencia_Validacao_Configuracao
(
     Pendencia_Validacao_Configuracao_ID  INT IDENTITY(1, 1) NOT NULL, 
   Enum_Processo_ID                     INT, 
     Acao_ID                              INT, 
     Grupo_Usuario_ID                     INT, 
     IsAtivo                              INT

     CONSTRAINT [PK_Pendencia_Validacao_Configuracao] PRIMARY KEY CLUSTERED ([Pendencia_Validacao_Configuracao_ID] ASC)
     WITH(
   PAD_INDEX = OFF, 
   STATISTICS_NORECOMPUTE = OFF, 
   IGNORE_DUP_KEY = OFF, 
   ALLOW_ROW_LOCKS = ON, 
   ALLOW_PAGE_LOCKS = ON
   ) ON [PRIMARY]
) ON [PRIMARY];

*/
-------------------------------------------------------------------------------

-- sp p_Jobs_Pendencias_Validacao_Demanda_202755 (Suzuki)

-- SELECT * FROM Romaneio_Venda_CT WHERE Enum_Tipo_Troca_ID > 0
-- SELECT * FROM Usuario WHERE Usuario.Usuario_Login LIKE 'Sistema';
-- SELECT * FROM Usuario WHERE Usuario_Nome_Completo LIKE 'Rodrigo de Souza Campos'
-- SELECT * FROM Enumerado WHERE Enum_ID = 448
-- SELECT * FROM Cargo WHERE Cargo.Cargo_Descricao LIKE 'Gerente de Loja';

/*
SELECT Funcionario.Funcionario_ID    AS Funcionario_ID,
PessoaFisica.PessoaFisica_NM                           
+ ' ' +                                                
PessoaFisica.PessoaFisica_Sobrenome    AS Nome_Completo,
Departamento_Cargo.Cargo_ID,
Cargo.Cargo_Descricao
FROM Departamento_Cargo
INNER JOIN Departamento ON
Departamento.Departamento_ID = Departamento_Cargo.Departamento_ID
INNER JOIN Cargo ON
Cargo.Cargo_ID = Departamento_Cargo.Cargo_ID                     
INNER JOIN Funcionario ON
Funcionario.Departamento_Cargo_ID = Departamento_Cargo.Departamento_Cargo_ID
INNER JOIN PessoaFisica ON
Funcionario.PessoaFisica_ID = PessoaFisica.PessoaFisica_ID
WHERE PessoaFisica.PessoaFisica_NM = 'Rodrigo' AND PessoaFisica.PessoaFisica_Sobrenome ='de Souza Campos'
*/

-------------------------------------------------------------------------------
-- QUALIDADE
-------------------------------------------------------------------------------

-- SELECT TOP 5 * FROM Romaneio_Venda_CT ORDER BY Romaneio_Venda_CT_ID DESC

-- BUG 73258
-- SELECT * FROM Romaneio_Venda_CT WHERE Lojas_ID = 1 AND Romaneio_Pre_Venda_CT_ID = 16366077

/*
SELECT 
    Cargo_ID, 
    Cargo_Descricao_Abreviado,
    Romaneio_Venda_CT.Usuario_Gerente_ID,
    Usuario.Usuario_Nome_Completo,
    Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID
FROM Romaneio_Venda_CT 
JOIN Usuario
    ON Usuario.Usuario_ID = Romaneio_Venda_CT.Usuario_Gerente_ID
JOIN Funcionario 
    ON Funcionario.PessoaFisica_ID = Usuario.Pessoa_ID
JOIN Cargo 
    ON Cargo.Cargo_ID = Funcionario.Departamento_Cargo_ID 
WHERE 
    Usuario_ID = 1372 -- Douglas
*/

-- ============================================================================
-- US04 - CONTROLE DE DEPÓSITOS EM CONTA
-- ============================================================================


----------------------- LISTAGEM DE ROMANEIOS DE VENDA ------------------------

/*

SELECT TOP 50
Romaneio_Venda_CT_Cliente_Nome, 
Romaneio_Pre_Venda_CT_ID,
Enum_Tipo_ID, 
Romaneio_Venda_Grupo_ID,
Enum_Tipo_Origem_ID,
Objeto_Origem_ID,
Romaneio_Venda_CT_Cliente_CNPJCPF, 
Romaneio_Venda_CT_Data_Geracao,
Romaneio_Venda_CT_Data_Liberacao,
Romaneio_Venda_CT_Data_Aprovacao_Credito,
Enum_Tipo_Troca_ID
FROM Romaneio_Venda_CT
INNER JOIN Condicao_Pagamento
ON Condicao_Pagamento.Condicao_Pagamento_ID = Romaneio_Venda_CT.Condicao_Pagamento_ID
INNER JOIN Forma_Pagamento
ON Forma_Pagamento.Forma_Pagamento_ID = Condicao_Pagamento.Forma_Pagamento_ID
WHERE 
Lojas_ID = 1
AND Enum_Status_ID = 1398 AND Enum_Tipo_ID IN(551, 549) --1397 AND Enum_Tipo_ID IN(550, 797) 
AND Forma_Pagamento.Forma_Pagamento_ID = 1 -- Dinheiro
AND Cliente_ID NOT IN ('F03889DF-B23C-422F-BD26-1485B9A26F69')
--AND Romaneio_Venda_Grupo_ID <> NULL
AND Romaneio_Venda_CT_Data_Liberacao < '2020-01-10'
ORDER BY Romaneio_Venda_CT_Data_Geracao DESC

SELECT Romaneio_Pre_Venda_CT_ID, Enum_Tipo_ID, Enum_Tipo_Troca_ID FROM Romaneio_Venda_CT WHERE Romaneio_Pre_Venda_CT_ID = 987665197 AND Lojas_ID = 1

SELECT TOP(10) * FROM Romaneio_Credito_Aprovacao ORDER BY Romaneio_Credito_Aprovacao_Data DESC -- créditos

-- Linha 980 da Romaneio_VendaBUS.cs
*/

-------------------------------------------------------------------------------

-- sp p_Cliente_Consultar_Cliente_Propriedades_Alteracao
-- sp p_Venda_Tecnica_Consultar_Romaneio_Venda_Grid

-- Status Romaneio Venda (estrutura antiga?):
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'StatusRomaneioVenda'
-- 535  = Gerado
-- 536  = Suspenso
-- 537  = Confirmado
-- 538  = Liberado
-- 539  = Cancelado
-- 2563 = Em Utilização

-- Status Romaneio Venda:
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Status_Romaneio_Venda'
-- 1397	= Pendente
-- 1398	= Liberado
-- 1399	= Cancelado
-- 1400	= Reativado
-- 1401	= Usado outra loja
-- 1952	= Inutilizado

-- Tipo Romaneio:
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'TipoRomaneio'
-- 182 = Transferência
-- 549 = Técnica
-- 550 = Troca
-- 551 = Auto-Serviço
-- 564 = Licitação
-- 572 = Especial
-- 648 = Estorno
-- 797 = Resta
-- 960 = Telepreço

-- CLIENTE:

-- SELECT * FROM Cliente WHERE Cliente_Nome = 'Teste Teste'
-- SELECT * FROM Cliente WHERE Cliente_ID = 'F03889DF-B23C-422F-BD26-1485B9A26F69'
-- SELECT * FROM Cliente WHERE Cliente_CD = 612000
-- SELECT * FROM Contato WHERE Pessoa_ID = 'E0B7A1FA-112F-4C7C-8DF3-4959A3C54F5E'
-- SELECT * FROM ContatoVirtual WHERE Pessoa_ID = 'E0B7A1FA-112F-4C7C-8DF3-4959A3C54F5E'
-- UPDATE ContatoVirtual SET ContatoVirtual_Email = '202755.us04@emailo.pro' WHERE Pessoa_ID = 'E0B7A1FA-112F-4C7C-8DF3-4959A3C54F5E'

-- SELECT Cliente_NOME, Cliente_CD, Cliente_ID, Pessoa_ID FROM Cliente WHERE Cliente_NOME LIKE '%Fernando Moraes Oliveira%'
-- SELECT Cliente_NOME, Cliente_CPF_CNPJ, Cliente_ID, Pessoa_ID FROM Cliente WHERE Pessoa_ID = 'EFF3795B-1E22-4279-9FFE-B01C0A3F2A78'

-- UPDATE Cliente SET Cliente_CD = 3182152 WHERE Pessoa_ID = '8DD6F3DF-7A81-4464-8785-D708E716C2A9'

-- VENDA CT:

-- SELECT * FROM Romaneio_Venda_CT WHERE Romaneio_Venda_CT_ID = 90064109
-- SELECT * FROM Romaneio_CT WHERE Romaneio_CT_ID = 90064109
-- SELECT * FROM Romaneio_CT WHERE Cliente_ID = 'F03889DF-B23C-422F-BD26-1485B9A26F69' AND Enum_Romaneio_Tipo_ID IN(550, 797) 

-- SELECT * FROM Romaneio_Venda_CT WHERE Romaneio_Venda_Grupo_ID = 90042082
-- SELECT * FROM Romaneio_CT WHERE Cliente_ID = '35E4DA83-9134-4EDE-9A33-878D0CD4C9B3'

-- PRE VENDA CT:

-- SELECT * FROM Romaneio_Venda_CT WHERE Romaneio_Pre_Venda_CT_ID = 987665197
-- SELECT * FROM Romaneio_Pre_Venda_CT WHERE Romaneio_Pre_Venda_CT_ID = 987665197
-- UPDATE Romaneio_Venda_CT SET Enum_Status_ID = 1397 WHERE Romaneio_Pre_Venda_CT_ID = 987665197
-- UPDATE Romaneio_Pre_Venda_CT SET Enum_Romaneio_Status_ID = 537 WHERE Romaneio_Pre_Venda_CT_ID = 987665197

-- VENDA IT:

-- SELECT * FROM Romaneio_Venda_IT WHERE Romaneio_Venda_CT_ID = 90042058
-- SELECT * FROM Romaneio_Venda_CT WHERE Romaneio_Venda_CT_ID = 90042058

-- GRUPO:

-- SELECT * FROM Romaneio_Venda_Grupo WHERE Romaneio_Venda_Grupo_ID = 90021245
-- SELECT * FROM Romaneio_Grupo WHERE Romaneio_Grupo_ID = 90021245
-- SELECT * FROM Enumerado WHERE Enum_ID = 1404

--PAGAMENTO CT:

-- SELECT TOP (5)* FROM Romaneio_Pagamento_Venda_Liberada
-- INNER JOIN Romaneio_Venda_CT
-- ON Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = Romaneio_Pagamento_Venda_Liberada.Romaneio_Grupo_ID
-- --WHERE Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = 90042082
-- ORDER BY Romaneio_Grupo_ID ASC

-- SELECT * FROM Condicao_Pagamento WHERE Condicao_Pagamento_ID = 385
-- SELECT * FROM Forma_Pagamento
-- SELECT * FROM vw_Condicao_Pagamento

-- BANCO:

-- SELECT * FROM Banco
-- SELECT * FROM Conta WHERE Pessoa_ID = '72C03FCE-6D76-406F-8FF2-DD3C22187832' AND Conta.Conta_Especial = 1

-- SELECT * FROM Conta WHERE Pessoa_ID = 'E0B7A1FA-112F-4C7C-8DF3-4959A3C54F5E'
-- UPDATE Conta SET Conta_Especial = 1 WHERE Pessoa_ID = 'E0B7A1FA-112F-4C7C-8DF3-4959A3C54F5E'

-- IDs Conta:
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Tipo_Conta'
-- Corrente = 2864
-- Poupanca = 2863
-- UPDATE Conta SET Enum_Tipo_Conta_ID = 2864 WHERE Pessoa_ID = '995F31FB-5668-4FA6-B8EC-FEF71B23E6C0'

/*

SELECT TOP 50
Romaneio_Venda_Pagamento_ID,
Romaneio_Venda_CT.Lojas_ID,
Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID,
Romaneio_Venda_Pagamento.Condicao_Pagamento_ID,
vw_Condicao_Pagamento.Forma_Pagamento_DS
FROM Romaneio_Venda_CT
INNER JOIN Romaneio_Venda_Pagamento 
ON Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = Romaneio_Venda_Pagamento.Romaneio_Venda_Grupo_ID
INNER JOIN vw_Condicao_Pagamento
ON Romaneio_Venda_Pagamento.Condicao_Pagamento_ID = vw_Condicao_Pagamento.Condicao_Pagamento_ID
WHERE Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = 90021245

*/

---------------------- SOLICITAÇÃO DE PAGAMENTO -------------------------------

/*

INSERT INTO Enumerado
(Enum_Sigla, 
 Enum_Extenso, 
 Enum_Nome, 
 Enum_IsAtivo
)
VALUES
('', 
 'Romaneio de crédito', 
 'Solicitacao_Pagamento_Objeto_Origem', 
 1
);

INSERT INTO Enumerado
(Enum_Sigla, 
 Enum_Extenso, 
 Enum_Nome, 
 Enum_IsAtivo
)
VALUES
('', 
 'Depósito em Conta', 
 'Forma_Solicitacao_Pagamento', 
 1
);

-------------------------------------------------------------------------------

-- ALTER TABLE Solicitacao_Pagamento ADD Lojas_ID INT NOT NULL DEFAULT 17 -- E-commerce;

-- DROP TABLE Solicitacao_Pagamento;

CREATE TABLE Solicitacao_Pagamento
(
     Solicitacao_Pagamento_ID                    INT IDENTITY(1, 1) NOT NULL, 
     Lojas_ID                                    INT NOT NULL, 
     Objeto_Origem_ID                            INT NOT NULL, 
     Enum_Origem_ID                              INT NOT NULL, 
     Enum_Status_ID                              INT NOT NULL, 
     Enum_Tipo_Pagamento_ID                      INT, 
     Banco_ID                                    INT, 
     Usuario_Criacao_ID                          INT, 
     Usuario_Ultima_Alteracao_ID                 INT, 
     Enum_Motivo_Recusa_Pagamento_ID             INT, 
     Solicitacao_Pagamento_Banco_Agencia         VARCHAR(20), 
     Solicitacao_Pagamento_Banco_Conta           VARCHAR(30), 
     Solicitacao_Pagamento_Credito_Online        VARCHAR(50), 
     Solicitacao_Pagamento_Comprovante_Estorno   VARCHAR(50), 
     Solicitacao_Pagamento_Valor                 DECIMAL(18, 2) NOT NULL, 
     Solicitacao_Pagamento_Data_Pagamento        DATETIME, 
     Solicitacao_Pagamento_Data_Criacao          DATETIME, 
     Solicitacao_Pagamento_Data_Ultima_Alteracao DATETIME, 
     Solicitacao_Pagamento_Comprovante_Pgto      VARBINARY(MAX), 
     Solicitacao_Pagamento_Obs                   VARCHAR(250)
     CONSTRAINT [PK_Solicitacao_Pagamento] PRIMARY KEY CLUSTERED ([Solicitacao_Pagamento_ID] ASC)
     WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	 CONSTRAINT FK_Solicitacao_Pagamento_Banco FOREIGN KEY (Banco_ID) REFERENCES MCAR_Desenvolvimento.dbo.Banco (Banco_ID)
)
ON [PRIMARY];

*/

-------------------------------------------------------------------------------

-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Ecommerce_Credito_Tipo'
-- SELECT * FROM Enumerado WHERE Enum_Nome = 'Solicitacao_Pagamento_Objeto_Origem' --2791

-- SP p_Financeiro_Consultar_Solicitacao_Pagamento_Grid
-- SP p_Financeiro_Consultar_Solicitacao_Pagamento_Propriedades
-- SP p_Ecommerce_Consultar_Auditoria_Solicitacao_Pagamento

-- SELECT * FROM SAC_CT 
-- SELECT * FROM Lojas WHERE Lojas_NM LIKE '%commerce%'
-- SELECT * FROM Pedido_Web_Pagamento

-- Processo de Validação:
-- SELECT * FROM Enumerado WHERE Enumerado.Enum_Nome = 'Processo_Validacao'
-- 2787 = Crédito de peça sem origem
-- 2788 = Crédito de peça sem origem

--Status:
-- SELECT * FROM Enumerado WHERE Enumerado.Enum_Nome = 'Status_Solicitacao_Pagamento'
-- 1598 = Encaminhado_ao_financeiro
-- 1599 = Efetuado
-- 1600 = Negado
-- 1616 = Em_Analise

-- Forma (Tipo) de Pagamento SAC:
-- SELECT * FROM Enumerado WHERE Enumerado.Enum_Nome = 'Forma_Solicitacao_Pagamento'
-- 1495 = DOC
-- 1496 = TED
-- 1497 = Tranferência
-- 2096 = Estorno
-- 2866 = Depósito em Conta

-- Forma de Pagamento Romaneio:
-- SELECT * FROM Forma_Pagamento WHERE Forma_Pagamento_DS LIKE '%Conta%'
-- 61 = Depósito em Conta

-- Objeto Origem:
-- SELECT * FROM Enumerado WHERE Enumerado.Enum_Nome = 'Solicitacao_Pagamento_Objeto_Origem'
-- 1617 = SAC
-- 2791 = Romaneio de crédito

-------------------------------------------------------------------------------

-- SELECT * FROM Solicitacao_Pagamento

-- UPDATE Solicitacao_Pagamento SET Enum_Status_ID = 1598 WHERE Objeto_Origem_ID IN(987665127) AND Lojas_ID = 1

-- UPDATE Solicitacao_Pagamento SET Enum_Status_ID = 1598 WHERE Solicitacao_Pagamento_ID = 6
-- DELETE FROM Solicitacao_Pagamento  WHERE Objeto_Origem_ID = 987665132


-- ============================================================================
-- US05 - INTEGRAÇÃO DOS DEPÓSITOS COM MICROSIGA
-- ============================================================================

/*

-- DROP TABLE Interface_Deposito

CREATE TABLE Interface_Deposito
(
     Interface_Deposito_ID      INT IDENTITY(1, 1) NOT NULL, 
	 Solicitacao_Pagamento_ID   INT NOT NULL,
     E2_CODCLIENTE              INT NOT NULL, -- Cliente_CD
     E2_NOMFAV                  VARCHAR(100) NOT NULL, --Nome do Favorecido
     E2_CPFCNPJ                 VARCHAR(14) NOT NULL, --CPF/CNPJ do favorecido
     E2_BANCO                   INT NOT NULL, --Banco a ser depositado 
     E2_AGENCIA                 VARCHAR(10) NOT NULL, --Agencia
     E2_CONTA                   VARCHAR(10) NOT NULL, --Conta
     E2_TPCONTA                 CHAR(1) NOT NULL, --Tipo de conta - [C]orrente/[P]oupança 
     E2_VALOR                   NUMERIC(15, 2) NOT NULL, --Valor
     E2_VENCTO                  DATETIME NOT NULL, --Vencimento
     E2_PROCESSA                CHAR(1), --Processado?
     E2_DTPROCESS               DATETIME, --Data Processamento 
     E2_PAGO                    BIT NOT NULL   --Pago?
	 CONSTRAINT [PK_Interface_Deposito] PRIMARY KEY CLUSTERED ([Interface_Deposito_ID] ASC)
);

*/
-------------------------------------------------------------------------------

-- Adicionadas as Siglas:
-- UPDATE Enumerado SET Enum_Sigla = 'P' WHERE Enum_ID = 2863
-- UPDATE Enumerado SET Enum_Sigla = 'C' WHERE Enum_ID = 2864

-- Cliente com status 639 estão com o campo Cliente_CD em branco (sugestão Gustavo)
-- SELECT * FROM PessoaFisica

-------------------------------------------------------------------------------

-- SOLICITACAO PAGAMENTO:

-- SELECT Romaneio_Pre_Venda_CT_ID, Enum_Status_ID FROM Romaneio_Venda_CT WHERE Enum_Tipo_Troca_ID > 0 AND Enum_Status_ID = 1398 --Liberado
-- UPDATE Romaneio_Venda_CT SET Enum_Status_ID = 1397 WHERE Romaneio_Pre_Venda_CT_ID = 987665132 --Pendente
-- DELETE FROM Solicitacao_Pagamento  WHERE Objeto_Origem_ID = 987665132

-- CALENDARIO

-- SP p_DP_Consultar_Feriados_Por_Loja
-- SELECT * FROM Feriado WHERE Feriado.Feriado_IsAtivo = 1

/*
SELECT DISTINCT
Cidade_NM,
Feriado_CT_Nome,
Feriado_CT_Nome
FROM Feriado_CT 
JOIN Feriado_IT ON
	Feriado_CT.Feriado_CT_ID = Feriado_IT.Feriado_CT_ID
JOIN Feriado_Cidade ON
	Feriado_CT.Feriado_CT_ID = Feriado_Cidade.Feriado_CT_ID
JOIN Endereco ON
	Feriado_Cidade.Cidade_ID = Endereco.Cidade_ID
JOIN Cidade ON
   Cidade.Cidade_ID = Feriado_Cidade.Cidade_ID
   WHERE Feriado_CT.Feriado_CT_IsAtivo = 1 AND Feriado_IT.Feriado_IT_Data = '2020-05-01'
*/

/*
UPDATE Interface_Deposito 
SET E2_PROCESSA = 1,
E2_DTPROCESS = '2020-01-16',
E2_PAGO = 1
WHERE Solicitacao_Pagamento_ID = 8
*/

-------------------------------------------------------------------------------


