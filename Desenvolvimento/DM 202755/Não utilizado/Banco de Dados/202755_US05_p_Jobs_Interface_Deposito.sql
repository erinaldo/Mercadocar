SET ANSI_NULLS ON; 
GO

SET QUOTED_IDENTIFIER ON; 
GO
-------------------------------------------------------------------------------
-- <summary>
--    Job que verifica a tabela Solicitacao_Pagamento por novos registros  
--    do tipo Romaneio de crédito e adiciona na tabela-ponte do Microsiga 
--    Interface_Deposito
-- </summary>
-- <history>
--    [fmoraes] - Criado      - 16/01/2020
--    Demanda Política de Créditos (202755 US05)
-- </history>
-------------------------------------------------------------------------------

ALTER PROCEDURE dbo.p_Jobs_Interface_Deposito
AS

DECLARE 
    @LOOP                    INT = 0,
    @Enum_Romaneio_Credito   INT,
    @Lojas_ID                INT,
    @Data_Vencimento         DATETIME,
    @Data_Processamento      DATETIME,
    @Processado              CHAR(1),
    @Pago                    BIT;

SET @Enum_Romaneio_Credito   = 2791;
SET @Data_Vencimento         = CAST(GETDATE() AS DATE);
SET @Data_Processamento      = NULL;
SET @Processado              = NULL;
SET @Pago                    = 0;

DECLARE @Lojas TABLE
(
Lojas_ID INT
);

DECLARE @Deposito TABLE
(
Solicitacao_Pagamento_ID   INT,
E2_CODCLIENTE              INT,
E2_NOMFAV                  VARCHAR(100),
E2_CPFCNPJ                 VARCHAR(14),
E2_BANCO                   INT,
E2_AGENCIA                 VARCHAR(10),
E2_CONTA                   VARCHAR(10),
E2_TPCONTA                 CHAR(1),
E2_VALOR                   NUMERIC(15, 2),
E2_VENCTO                  DATETIME,
E2_PROCESSA                CHAR(1),
E2_DTPROCESS               DATETIME, 
E2_PAGO                    BIT
); 

SET NOCOUNT ON;

-------------------------------------------------------------------------------
-- SOLICITAÇÃO DE DEPÓSITO DO TIPO ROMANEIO DE CRÉDITO
-------------------------------------------------------------------------------

INSERT @Lojas
SELECT DISTINCT Solicitacao_Pagamento.Lojas_ID
FROM Solicitacao_Pagamento
    JOIN Romaneio_Venda_CT
    ON Romaneio_Venda_CT.Lojas_ID = Solicitacao_Pagamento.Lojas_ID
        AND Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID = Solicitacao_Pagamento.Objeto_Origem_ID
WHERE NOT EXISTS
(
    SELECT 1
    FROM Interface_Deposito
    WHERE Interface_Deposito.Solicitacao_Pagamento_ID = Solicitacao_Pagamento.Solicitacao_Pagamento_ID
  AND Solicitacao_Pagamento.Enum_Origem_ID = @Enum_Romaneio_Credito
)
ORDER BY Solicitacao_Pagamento.Lojas_ID;

IF (SELECT COUNT(Lojas_ID) FROM @Lojas) > 0
    INSERT INTO @Deposito
    SELECT DISTINCT
          Solicitacao_Pagamento.Solicitacao_Pagamento_ID             AS Solicitacao_Pagamento_ID,
          CAST(Cliente.Cliente_CD AS VARCHAR(10))                    AS Cliente_CD,
          CAST(Cliente.Cliente_Nome AS VARCHAR(200))                 AS Cliente_Nome,
          Cliente.Cliente_CPF_CNPJ                                   AS Cliente_CPFCNPJ,
          Banco.Banco_CD                                             AS Banco_CD,
          Solicitacao_Pagamento.Solicitacao_Pagamento_Banco_Agencia  AS Solicitacao_Pagamento_Banco_Agencia,
          Solicitacao_Pagamento.Solicitacao_Pagamento_Banco_Conta    AS Solicitacao_Pagamento_Banco_Conta,
          Conta_Tipo.Enum_Sigla                                      AS Banco_Conta_Tipo_Sigla,
          Solicitacao_Pagamento.Solicitacao_Pagamento_Valor          AS Solicitacao_Pagamento_Valor,
          @Data_Vencimento                                           AS Data_Vencimento,
          @Processado                                                AS Processado,
          @Data_Processamento                                        AS Data_Processamento,
          @Pago                                                      AS Pago
    FROM Solicitacao_Pagamento
        JOIN Lojas
        ON Solicitacao_Pagamento.Lojas_ID = Lojas.Lojas_Id
        JOIN Romaneio_Venda_CT
        ON Romaneio_Venda_CT.Lojas_ID = Solicitacao_Pagamento.Lojas_ID
            AND Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID = Solicitacao_Pagamento.Objeto_Origem_ID
        JOIN Cliente
        ON Romaneio_Venda_CT.Cliente_ID = Cliente.Cliente_ID
        JOIN Banco
        ON Solicitacao_Pagamento.Banco_ID = Banco.Banco_ID
        JOIN Conta
        ON Cliente.Pessoa_ID = Conta.Pessoa_ID
            AND Conta.Conta_Especial = 1
        JOIN Enumerado AS Conta_Tipo
        ON Conta.Enum_Tipo_Conta_ID = Conta_Tipo.Enum_ID
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM Interface_Deposito
        WHERE Interface_Deposito.Solicitacao_Pagamento_ID = Solicitacao_Pagamento.Solicitacao_Pagamento_ID
      AND Solicitacao_Pagamento.Enum_Origem_ID = @Enum_Romaneio_Credito
    )
    ORDER BY Solicitacao_Pagamento.Solicitacao_Pagamento_ID;

-------------------------------------------------------------------------------
-- VERIFICA FINAL DE SEMANA E FERIADOS
-------------------------------------------------------------------------------

IF (SELECT COUNT(Lojas_ID) FROM @Lojas) > 0
    WHILE(1 = 1)
    BEGIN
      SELECT @LOOP = MIN(Lojas_ID)
      FROM @Lojas WHERE Lojas_ID > @LOOP
      IF @LOOP IS NULL BREAK
      SET @Lojas_ID = @LOOP

          IF (SELECT DISTINCT
		        Feriado_IT.Feriado_IT_Data
	        FROM
		        Feriado_CT 
		        JOIN Feriado_IT ON
			        Feriado_CT.Feriado_CT_ID = Feriado_IT.Feriado_CT_ID
		        JOIN Feriado_Cidade ON
			        Feriado_CT.Feriado_CT_ID = Feriado_Cidade.Feriado_CT_ID
		        JOIN Endereco ON
			        Feriado_Cidade.Cidade_ID = Endereco.Cidade_ID
		        JOIN PessoaJuridica ON
			        Endereco.Pessoa_ID = PessoaJuridica.PessoaJuridica_ID
		        JOIN Lojas ON
			        PessoaJuridica.PessoaJuridica_ID = Lojas.PessoaJuridica_ID
	        WHERE
		        Lojas.Lojas_Id = @Lojas_ID
	        AND
		        Feriado_IT.Feriado_IT_Data = @Data_Vencimento
	        AND
		        Feriado_CT.Feriado_CT_IsAtivo = 1
        ) IS NOT NULL
        BEGIN 
        UPDATE @Deposito SET E2_VENCTO = 
            CASE DATEPART(WEEKDAY,@Data_Vencimento)
            WHEN 1 THEN @Data_Vencimento +2
            WHEN 6 THEN @Data_Vencimento +4
            WHEN 7 THEN @Data_Vencimento +3
            ELSE @Data_Vencimento +2
        END;
        END
        ELSE
        BEGIN
            UPDATE @Deposito SET E2_VENCTO = 
                CASE DATEPART(WEEKDAY,@Data_Vencimento)
                WHEN 1 THEN @Data_Vencimento +2
                WHEN 6 THEN @Data_Vencimento +3
                WHEN 7 THEN @Data_Vencimento +3
                ELSE @Data_Vencimento +1
            END
        END
    END;

-------------------------------------------------------------------------------
-- ENVIO DA SOLICITAÇÃO DE DEPÓSITO AO MICROSIGA
-------------------------------------------------------------------------------

INSERT INTO Interface_Deposito
SELECT * FROM @deposito

SET NOCOUNT OFF;