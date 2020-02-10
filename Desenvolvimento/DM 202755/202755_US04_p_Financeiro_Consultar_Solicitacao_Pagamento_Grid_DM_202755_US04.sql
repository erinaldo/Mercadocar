SET ANSI_NULLS ON 
GO 
SET QUOTED_IDENTIFIER ON 
GO
-------------------------------------------------------------------------------
-- <summary>
--      Procedure de Preenchimento do Formulário de pesquisa do Solicitacao_Pagamento
-- </summary>
-- <history>
--    [cbarbosa]  - 08/01/2013  Created
--    [tnovelli]  - 04/02/2014  Alterado
--      Tratamento na cláusula where, inserção de parênteses.
--    [fmoraes]   23/12/2019  Modified 
--      Alterada para funcionar pelo Objeto_Origem_ID relacionado à tabela Romaneio_Venda_CT (Demanda 202755 - US04)
-- </history>
-----------------------------------------------------------------------------------
--ALTER PROCEDURE [dbo].[p_Financeiro_Consultar_Solicitacao_Pagamento_Grid_DM_202755_US04]
--@Solicitacao_Pagamento_ID               INT,
--@SAC_ID                                 INT,
--@Objeto_Origem_ID                       INT,
--@CPF_CNPJ                               VARCHAR(14),
--@Data_Inicial                           DATETIME,
--@Data_Final                             DATETIME,
--@Solicitacao_Pagamento_Status           udtEnumerado READONLY,
--@Solicitacao_Pagamento_Objeto_Origem    udtEnumerado READONLY
--AS
-----------------------------------------------------------------------------------

DECLARE
@Solicitacao_Pagamento_ID              INT,
@SAC_ID                                INT,
@Objeto_Origem_ID                      INT,
@Pedido_Web_ID                         INT,
@CPF_CNPJ                              VARCHAR(14),
@Data_Inicial                          DATETIME,
@Data_Final                            DATETIME,
@Solicitacao_Pagamento_Status          udtEnumerado,
@Enum_Origem_ID                        INT;

SET @Solicitacao_Pagamento_ID         = NULL
SET @SAC_ID                           = NULL
SET @Objeto_Origem_ID                 = 987665197
SET @Pedido_Web_ID                    = NULL
SET @CPF_CNPJ                         = NULL
SET @Data_Inicial                     = NULL
SET @Data_Final                       = NULL
SET @Enum_Origem_ID                   = 2791
 
/*
1598 - Encaminhado ao financeiro
1599 - Efetuado
1600 - Negado
1616 - Em Analise
*/

INSERT INTO @Solicitacao_Pagamento_Status VALUES (1598);

/*
1617 - SAC
2791 - Romaneio de crédito
*/

-------------------------------------------------------------------------------
DECLARE 
     @Enum_Origem_SAC             INT, 
     @Enum_Origem_RomaneioCredito INT;

SET @Enum_Origem_SAC             = 1617;
SET @Enum_Origem_RomaneioCredito = 2786;

-------------------------------------------------------------------------------

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SET NOCOUNT ON

IF @Enum_Origem_ID = @Enum_Origem_RomaneioCredito
  BEGIN
    SELECT Solicitacao_Pagamento.Solicitacao_Pagamento_ID AS Solicitacao_Pagamento_ID, 
           Solicitacao_Pagamento.Lojas_ID AS Lojas_ID,
           Lojas.Lojas_NM AS Loja,
           Solicitacao_Pagamento.Objeto_Origem_ID AS Objeto_Origem_ID, 
           Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID AS Objeto_Origem_Grupo_ID,
           CAST(Cliente.Cliente_ID AS VARCHAR(40)) AS Cliente_ID,
           CAST(ISNULL(Cliente.Cliente_Nome, '') AS VARCHAR(200)) AS Cliente_Nome,
           Cliente.Cliente_CPF_CNPJ AS Cliente_CPFCNPJ, 
           Solicitacao_Pagamento_Status.Enum_ID AS Enum_Status_ID,
           Solicitacao_Pagamento_Status.Enum_Extenso AS Solicitacao_Pagamento_Status, 
           Solicitacao_Pagamento.Usuario_Criacao_ID AS Usuario_Criacao_ID,
           Usuario.Usuario_Nome_Completo AS Solicitacao_Pagamento_Usuario_Criacao, 
           Solicitacao_Pagamento.Enum_Origem_ID AS Enum_Origem_ID,
           Solicitacao_Pagamento_Origem.Enum_Extenso AS Solicitacao_Pagamento_Origem, 
           Solicitacao_Pagamento.Banco_ID AS Banco_ID,
           Banco.Banco_Nome AS Banco_Nome, 
           Solicitacao_Pagamento.Solicitacao_Pagamento_Valor AS Solicitacao_Pagamento_Valor, 
           Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Criacao AS Solicitacao_Pagamento_Data_Criacao, 
           Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Ultima_Alteracao AS Solicitacao_Pagamento_Data_Ultima_Alteracao
    FROM Solicitacao_Pagamento
         INNER JOIN Lojas
     ON Solicitacao_Pagamento.Lojas_ID = Lojas.Lojas_Id
         INNER JOIN Romaneio_Venda_CT
         ON Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID = Solicitacao_Pagamento.Objeto_Origem_ID
     AND Romaneio_Venda_CT.Lojas_ID = Solicitacao_Pagamento.Lojas_ID
         INNER JOIN Cliente
         ON Romaneio_Venda_CT.Cliente_ID = Cliente.Cliente_ID
         INNER JOIN Usuario
         ON Solicitacao_Pagamento.Usuario_Criacao_ID = Usuario.Usuario_ID
         INNER JOIN Enumerado AS Solicitacao_Pagamento_Origem
         ON Solicitacao_Pagamento.Enum_Origem_ID = Solicitacao_Pagamento_Origem.Enum_ID
         INNER JOIN Banco
         ON Solicitacao_Pagamento.Banco_ID = Banco.Banco_ID
         INNER JOIN @Solicitacao_Pagamento_Status AS Solicitacao_Pagamento_Status_Filtro
         ON Solicitacao_Pagamento.Enum_Status_ID = Solicitacao_Pagamento_Status_Filtro.Enum_ID
         INNER JOIN Enumerado AS Solicitacao_Pagamento_Status
         ON Solicitacao_Pagamento.Enum_Status_ID = Solicitacao_Pagamento_Status.Enum_ID
    WHERE(Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID = @Objeto_Origem_ID)
         AND (Solicitacao_Pagamento.Solicitacao_Pagamento_ID = @Solicitacao_Pagamento_ID OR @Solicitacao_Pagamento_ID IS NULL)
         AND (Cliente.Cliente_CPF_CNPJ = @CPF_CNPJ OR @CPF_CNPJ IS NULL)
         AND (Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Criacao BETWEEN @Data_Inicial AND @Data_Final OR @Data_Inicial IS NULL)
         AND Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID IS NOT NULL
    ORDER BY Solicitacao_Pagamento.Solicitacao_Pagamento_ID;
END;
  ELSE
  IF @Enum_Origem_ID = @Enum_Origem_SAC
    BEGIN
      SELECT Solicitacao_Pagamento.Solicitacao_Pagamento_ID AS Solicitacao_Pagamento_ID, 
             SAC_CT.SAC_CT_ID AS SAC_CT_ID, 
             Cliente.Cliente_Nome AS Cliente_Nome, 
             Cliente.Cliente_CPF_CNPJ AS Cliente_CPFCNPJ, 
             Solicitacao_Pagamento_Status.Enum_Extenso AS Solicitacao_Pagamento_Status, 
             Usuario.Usuario_Nome_Completo AS Solicitacao_Pagamento_Usuario_Criacao, 
             Solicitacao_Pagamento_Origem.Enum_Extenso AS Solicitacao_Pagamento_Origem, 
             Banco.Banco_Nome AS Banco_Nome, 
             Solicitacao_Pagamento.Solicitacao_Pagamento_Valor AS Solicitacao_Pagamento_Valor, 
             Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Criacao AS Solicitacao_Pagamento_Data_Criacao, 
             Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Ultima_Alteracao AS Solicitacao_Pagamento_Data_Ultima_Alteracao
      FROM SAC_CT
           INNER JOIN Pedido_Web_CT
           ON SAC_CT.Objeto_Origem_ID = Pedido_Web_CT.PED_N_CODIGO
           INNER JOIN Cliente
           ON Pedido_Web_CT.Pessoa_ID = Cliente.Pessoa_ID
           INNER JOIN Solicitacao_Pagamento
           ON SAC_CT.SAC_CT_ID = Solicitacao_Pagamento.Objeto_Origem_ID
           INNER JOIN Usuario
           ON Solicitacao_Pagamento.Usuario_Criacao_ID = Usuario.Usuario_ID
           INNER JOIN Enumerado AS Solicitacao_Pagamento_Origem
           ON Solicitacao_Pagamento.Enum_Origem_ID = Solicitacao_Pagamento_Origem.Enum_ID
           INNER JOIN Banco
           ON Solicitacao_Pagamento.Banco_ID = Banco.Banco_ID
           INNER JOIN @Solicitacao_Pagamento_Status AS Solicitacao_Pagamento_Status_Filtro
           ON Solicitacao_Pagamento.Enum_Status_ID = Solicitacao_Pagamento_Status_Filtro.Enum_ID
           INNER JOIN Enumerado AS Solicitacao_Pagamento_Status
           ON Solicitacao_Pagamento.Enum_Status_ID = Solicitacao_Pagamento_Status.Enum_ID
      WHERE(SAC_CT.SAC_CT_ID = @SAC_ID
            OR @SAC_ID IS NULL)
           AND (Solicitacao_Pagamento.Solicitacao_Pagamento_ID = @Solicitacao_Pagamento_ID
                OR @Solicitacao_Pagamento_ID IS NULL)
           AND (Cliente.Cliente_CPF_CNPJ = @CPF_CNPJ
                OR @CPF_CNPJ IS NULL)
           AND (Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Criacao BETWEEN @Data_Inicial AND @Data_Final
                OR @Data_Inicial IS NULL)
           AND SAC_CT.SAC_CT_ID IS NOT NULL
           AND Solicitacao_Pagamento.Solicitacao_Pagamento_Valor > 0
      ORDER BY Solicitacao_Pagamento.Solicitacao_Pagamento_ID;
  END;

SET NOCOUNT OFF