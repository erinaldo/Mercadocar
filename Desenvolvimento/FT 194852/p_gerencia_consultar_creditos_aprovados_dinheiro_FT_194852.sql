-------------------------------------------------------------------------------
-- <summary>
--		Relatório de Romaneio de Crédito Aprovados em dinheiro
-- </summary>
-- <history>
-- 		[mmukuno] -	12/01/2017 - Created
--      [efsousa] - 02/08/2019 - Modify
--			Alterado execução dos filtros onde antes fazia o scan e depois validava o nulo, alterei para validar o nulo e depois valido os filtros.
--      [fmoraes] - 05/09/2019 - Modify
--			Adiconadas colunas Data_Venda e Prazo_Pagamento
-- </history>
-------------------------------------------------------------------------------
--ALTER Procedure [dbo].p_gerencia_consultar_creditos_aprovados_dinheiro_FT_194852
--(
--	@Tipo_Exibicao			           INT,
--	@Lojas_ID				           INT,
--	@Romaneio_ID			           INT,
--	@Usuario_Aprovacao_ID	           INT,
--	@Valor_Romaneio_Inicial	           DECIMAL(18,2),
--	@Valor_Romaneio_Final	           DECIMAL(18,2),
--	@Valor_Romaneio_Indevido_Inicial   DECIMAL(18,2),
--	@Valor_Romaneio_Indevido_Final	   DECIMAL(18,2),
--	@Data_Geracao_Inicial	           DATETIME,
--	@Data_Geracao_Final		           DATETIME,
--	@Data_Liberacao_Inicial	           DATETIME,
--	@Data_Liberacao_Final	           DATETIME,
--	@Data_Aprovacao_Inicial	           DATETIME,
--	@Data_Aprovacao_Final	           DATETIME
--)
--AS
---------------------------------------------------------------------------------

DECLARE 
     @Tipo_Exibicao                   INT, 
     @Lojas_ID                        INT, 
     @Romaneio_ID                     INT, 
     @Usuario_Aprovacao_ID            INT, 
     @Valor_Romaneio_Inicial          DECIMAL(18, 2), 
     @Valor_Romaneio_Final            DECIMAL(18, 2), 
     @Valor_Romaneio_Indevido_Inicial DECIMAL(18, 2), 
     @Valor_Romaneio_Indevido_Final   DECIMAL(18, 2), 
     @Data_Geracao_Inicial            DATETIME, 
     @Data_Geracao_Final              DATETIME, 
     @Data_Liberacao_Inicial          DATETIME, 
     @Data_Liberacao_Final            DATETIME, 
     @Data_Aprovacao_Inicial          DATETIME, 
     @Data_Aprovacao_Final            DATETIME;

-------------------------------------------------------
-- 0 Loja
-- 1 Romaneio
-- 2 Usuario de Aprovação
-------------------------------------------------------

SET @Tipo_Exibicao                     = 1;
SET @Lojas_ID                          = 1;
SET @Romaneio_ID                       = NULL;
SET @Usuario_Aprovacao_ID              = NULL;
SET @Valor_Romaneio_Inicial            = NULL;
SET @Valor_Romaneio_Final              = NULL;
SET @Valor_Romaneio_Indevido_Inicial   = NULL;
SET @Valor_Romaneio_Indevido_Final     = NULL;
SET @Data_Geracao_Inicial              = '2019-01-01';
SET @Data_Geracao_Final                = '2019-09-02';
SET @Data_Liberacao_Inicial            = NULL;
SET @Data_Liberacao_Final              = NULL;
SET @Data_Aprovacao_Inicial            = NULL;
SET @Data_Aprovacao_Final              = NULL;
-------------------------------------------------------------------------------

IF OBJECT_ID('tempdb..#tmp_vendas_credito') IS NOT NULL
  BEGIN
    DROP TABLE #tmp_vendas_credito;
END;

IF OBJECT_ID('tempdb..#tmp_Romaneio') IS NOT NULL
  BEGIN
    DROP TABLE #tmp_Romaneio;
END;

IF OBJECT_ID('tempdb..#tmp_Credito_Aprovado') IS NOT NULL
  BEGIN
    DROP TABLE #tmp_Credito_Aprovado;
END;

SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

IF(@Valor_Romaneio_Inicial IS NULL
   AND @Valor_Romaneio_Final IS NOT NULL)
  SET @Valor_Romaneio_Inicial = 0;

IF(@Valor_Romaneio_Indevido_Inicial IS NULL
   AND @Valor_Romaneio_Indevido_Final IS NOT NULL)
  SET @Valor_Romaneio_Indevido_Inicial = 0;

SELECT Romaneio_Venda_CT.Romaneio_Venda_CT_ID, 
       Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID, 
       Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID, 
       Cliente_ID, 
       Enum_Tipo_ID, 
       Usuario_Aprovacao_Credito_ID, 
       Romaneio_Venda_CT.Lojas_ID, 
       Romaneio_Venda_CT_Valor_Pago, 
       Romaneio_Venda_CT_Data_Geracao, 
       Romaneio_Venda_CT_Data_Liberacao, 
       Romaneio_Venda_CT_Data_Aprovacao_Credito, 
       Romaneio_Venda_CT_Motivo_Aprovacao_Credito
INTO #tmp_vendas_credito
FROM Romaneio_Venda_CT
WHERE(@Romaneio_ID IS NULL
      OR Romaneio_Venda_ct.Romaneio_Pre_Venda_CT_ID = @Romaneio_ID)
     AND (@Lojas_ID IS NULL
          OR Romaneio_Venda_ct.Lojas_ID = @Lojas_ID)
     AND (@Usuario_Aprovacao_ID IS NULL
          OR Romaneio_Venda_ct.Usuario_Aprovacao_Credito_ID = @Usuario_Aprovacao_ID)
     AND (@Data_Aprovacao_Inicial IS NULL
          OR (Romaneio_Venda_CT_Data_Aprovacao_Credito BETWEEN @Data_Aprovacao_Inicial AND DATEADD(DAY, 1, @Data_Aprovacao_Final)))
     AND (@Data_Geracao_Inicial IS NULL
          OR (Romaneio_Venda_CT_Data_Geracao BETWEEN @Data_Geracao_Inicial AND DATEADD(DAY, 1, @Data_Geracao_Final)))
     AND (@Data_Liberacao_Inicial IS NULL
          OR (Romaneio_Venda_CT_Data_Liberacao BETWEEN @Data_Liberacao_Inicial AND DATEADD(DAY, 1, @Data_Liberacao_Final)))
     AND (@Valor_Romaneio_Inicial IS NULL
          OR (ABS(Romaneio_Venda_CT_Valor_Pago) BETWEEN @Valor_Romaneio_Inicial AND @Valor_Romaneio_Final))
     AND Romaneio_Venda_ct.Usuario_Aprovacao_Credito_ID IS NOT NULL
ORDER BY Romaneio_Venda_CT_Data_Aprovacao_Credito DESC;

SELECT DISTINCT 
       #tmp_vendas_credito.Romaneio_Pre_Venda_CT_ID             AS Romaneio_Pre_Venda_CT_ID, 
       #tmp_vendas_credito.Romaneio_Venda_Grupo_ID              AS Romaneio_Venda_Grupo_ID, 
       #tmp_vendas_credito.Lojas_ID                             AS Lojas_ID,
       CASE WHEN vw_Condicao_Pagamento.Forma_Pagamento_ID IN(1, 4)
         THEN SUM(Romaneio_Venda_Pagamento_Valor)
         ELSE 0
       END                                                      AS Valor_Disponivel, 
       ABS(#tmp_vendas_credito.Romaneio_Venda_CT_Valor_Pago)    AS Valor_Devolvido, 
       SUM(Romaneio_Venda_Pagamento_Valor)                      AS Valor_Pago_Dinheiro, 
       vw_Condicao_Pagamento.Forma_Pagamento_ID                 AS Forma_Pagamento_ID, 
       #tmp_vendas_credito.Cliente_ID, 
       #tmp_vendas_credito.Enum_Tipo_ID, 
       #tmp_vendas_credito.Usuario_Aprovacao_Credito_ID, 
       #tmp_vendas_credito.Romaneio_Venda_CT_Valor_Pago, 
       #tmp_vendas_credito.Romaneio_Venda_CT_Data_Geracao, 
       #tmp_vendas_credito.Romaneio_Venda_CT_Data_Liberacao, 
       #tmp_vendas_credito.Romaneio_Venda_CT_Data_Aprovacao_Credito, 
       #tmp_vendas_credito.Romaneio_Venda_CT_Motivo_Aprovacao_Credito, 
       MIN(Venda_Origem.Romaneio_Venda_CT_Data_Liberacao)       AS Data_Venda_Origem, 
       MAX(Prazo_Pagamento_DS)                                  AS Prazo_Pagamento_DS
INTO #tmp_Romaneio
FROM #tmp_vendas_credito
     CROSS APPLY fun_Retorna_Romaneio_Venda_Origem(#tmp_vendas_credito.Romaneio_Venda_CT_ID, #tmp_vendas_credito.Lojas_ID) AS func
     INNER JOIN Romaneio_Venda_CT AS Venda_Origem
                 ON Venda_Origem.Romaneio_Venda_CT_ID = func.Romaneio_CT_ID_Pai
                    AND Venda_Origem.Lojas_ID = func.Lojas_ID_Pai
     INNER JOIN Romaneio_Venda_Pagamento
                 ON Romaneio_Venda_Pagamento.Romaneio_Venda_Grupo_ID = Venda_Origem.Romaneio_Venda_Grupo_ID
                    AND Romaneio_Venda_Pagamento.Lojas_ID = Venda_Origem.Lojas_ID
     LEFT JOIN vw_Condicao_Pagamento
                 ON Romaneio_Venda_Pagamento.Condicao_Pagamento_ID = vw_Condicao_Pagamento.Condicao_Pagamento_ID
GROUP BY vw_Condicao_Pagamento.Forma_Pagamento_ID, 
         #tmp_vendas_credito.Romaneio_Venda_CT_Valor_Pago, 
         #tmp_vendas_credito.Romaneio_Pre_Venda_CT_ID, 
         #tmp_vendas_credito.Romaneio_Venda_Grupo_ID, 
         #tmp_vendas_credito.Cliente_ID, 
         #tmp_vendas_credito.Enum_Tipo_ID, 
         #tmp_vendas_credito.Usuario_Aprovacao_Credito_ID, 
         #tmp_vendas_credito.Romaneio_Venda_CT_Valor_Pago, 
         #tmp_vendas_credito.Romaneio_Venda_CT_Data_Geracao, 
         #tmp_vendas_credito.Romaneio_Venda_CT_Data_Liberacao, 
         #tmp_vendas_credito.Romaneio_Venda_CT_Data_Aprovacao_Credito, 
         #tmp_vendas_credito.Romaneio_Venda_CT_Motivo_Aprovacao_Credito, 
         #tmp_vendas_credito.Lojas_ID;

--SELECT * FROM #tmp_Romaneio ORDER BY 1

SELECT DISTINCT 
       Lojas.Lojas_ID                            AS Lojas_ID, 
       Lojas.Lojas_NM                            AS Lojas, 
       Enumerado.Enum_Extenso                    AS Tipo, 
       #tmp_Romaneio.Romaneio_Pre_Venda_CT_ID    AS Romaneio_Pre_Venda_CT_ID, 
       #tmp_Romaneio.Romaneio_Venda_Grupo_ID     AS Romaneio_Venda_Grupo_ID, 
       Cliente.Cliente_Nome                      AS Cliente, 
       Romaneio_Venda_CT_Data_Liberacao          AS Data_Liberacao, 
       Romaneio_Venda_CT_Data_Geracao            AS Data_Geracao, 
       ISNULL(gerente.Usuario_Nome_Completo, '') AS Usuario_Geracao, 
       Romaneio_Venda_CT_Valor_Pago              AS Valor, 
       SUM(Valor_Disponivel) - MAX(Valor_Devolvido) AS Valor_Indevido, 
       Romaneio_Venda_CT_Data_Aprovacao_Credito  AS Data_Aprovacao, 
       Gerente.Usuario_Nome_Completo             AS Usuario_Aprovacao, 
       LTRIM(RTRIM(REPLACE(Romaneio_Venda_CT_Motivo_Aprovacao_Credito, CHAR(13) + CHAR(10), ''))) AS Motivo, 
       MIN(Data_Venda_Origem)                    AS Data_Venda_Origem, 
       MAX(Prazo_Pagamento_DS)                   AS Prazo_Pagamento_DS
INTO #tmp_Credito_Aprovado
FROM #tmp_Romaneio
     JOIN Enumerado(NOLOCK)
     ON Enumerado.Enum_ID = #tmp_Romaneio.Enum_Tipo_ID
     INNER JOIN Lojas
     ON #tmp_Romaneio.Lojas_ID = Lojas.Lojas_ID
     LEFT JOIN Usuario AS gerente(NOLOCK)
     ON #tmp_Romaneio.Usuario_Aprovacao_Credito_ID = gerente.Usuario_ID
     JOIN Cliente(NOLOCK)
     ON Cliente.Cliente_ID = #tmp_Romaneio.Cliente_ID
WHERE(@Romaneio_ID IS NULL
      OR Romaneio_Pre_Venda_CT_ID = @Romaneio_ID)
     AND (@Data_Geracao_Inicial IS NULL
          OR (Romaneio_Venda_CT_Data_Geracao BETWEEN @Data_Geracao_Inicial AND DATEADD(DAY, 1, @Data_Geracao_Final)))
     AND (@Data_Liberacao_Inicial IS NULL
          OR (Romaneio_Venda_CT_Data_Liberacao BETWEEN @Data_Liberacao_Inicial AND DATEADD(DAY, 1, @Data_Liberacao_Final)))
GROUP BY Lojas.Lojas_ID, 
         Lojas.Lojas_NM, 
         Enumerado.Enum_Extenso, 
         #tmp_Romaneio.Romaneio_Pre_Venda_CT_ID, 
         #tmp_Romaneio.Romaneio_Venda_Grupo_ID, 
         Cliente.Cliente_Nome, 
         Romaneio_Venda_CT_Data_Liberacao, 
         Romaneio_Venda_CT_Data_Geracao, 
         Romaneio_Venda_CT_Data_Aprovacao_Credito, 
         ISNULL(gerente.Usuario_Nome_Completo, ''), 
         Romaneio_Venda_CT_Valor_Pago, 
         gerente.Usuario_Nome_Completo, 
         LTRIM(RTRIM(REPLACE(Romaneio_Venda_CT_Motivo_Aprovacao_Credito, CHAR(13) + CHAR(10), '')))
HAVING MAX(Valor_Devolvido) > SUM(Valor_Disponivel);

-- Exibição por loja
IF(@Tipo_Exibicao = 0)
  BEGIN
    SELECT Lojas                                 AS Lojas_NM, 
           SUM(Valor)                            AS Valor, 
           SUM(Valor_Indevido)                   AS Valor_Indevido, 
           COUNT(#tmp_Credito_Aprovado.Romaneio_Pre_Venda_CT_ID) AS Qtde_Romaneio
    FROM #tmp_Credito_Aprovado
    GROUP BY Lojas
    HAVING(@Valor_Romaneio_Indevido_Inicial IS NULL
           OR (ABS(SUM(Valor_Indevido)) BETWEEN @Valor_Romaneio_Indevido_Inicial AND @Valor_Romaneio_Indevido_Final))
          AND (@Valor_Romaneio_Inicial IS NULL
               OR (ABS(SUM(Valor)) BETWEEN @Valor_Romaneio_Inicial AND @Valor_Romaneio_Final));
END;

-- Exibição dos Romaneio
IF(@Tipo_Exibicao = 1)
  BEGIN
    SELECT Lojas_ID                 AS Lojas_ID, 
           Lojas                    AS Lojas_NM, 
           Tipo                     AS Tipo, 
           Romaneio_Pre_Venda_CT_ID AS Romaneio_Pre_Venda_CT_ID, 
           Romaneio_Venda_Grupo_ID  AS Romaneio_Venda_Grupo_ID, 
           Cliente                  AS Cliente, 
           Data_Liberacao           AS Data_Liberacao, 
           Data_Geracao             AS Data_Geracao, 
           Usuario_Geracao          AS Usuario_Geracao, 
           Valor                    AS Valor, 
           Valor_Indevido           AS Valor_Indevido, 
           Data_Aprovacao           AS Data_Aprovacao, 
           Usuario_Aprovacao        AS Usuario_Aprovacao, 
           Motivo                   AS Motivo, 
           Prazo_Pagamento_DS       AS Prazo_Pagamento_DS, 
           Prazo_Pagamento_DS       AS Prazo_Pagamento_DS
    FROM #tmp_Credito_Aprovado
    WHERE(@Valor_Romaneio_Indevido_Inicial IS NULL
          OR (ABS(Valor_Indevido) BETWEEN @Valor_Romaneio_Indevido_Inicial AND @Valor_Romaneio_Indevido_Final))
         AND (@Valor_Romaneio_Inicial IS NULL
              OR (ABS(Valor) BETWEEN @Valor_Romaneio_Inicial AND @Valor_Romaneio_Final));
END;

-- Exibição por Usuario Aprovação
IF(@Tipo_Exibicao = 2)
  BEGIN
    SELECT Usuario_Aprovacao        AS Usuario_Aprovacao, 
           SUM(Valor)               AS Valor, 
           SUM(Valor_Indevido)      AS Valor_Indevido, 
           COUNT(#tmp_Credito_Aprovado.Romaneio_Pre_Venda_CT_ID) AS Qtde_Romaneio
    FROM #tmp_Credito_Aprovado
    GROUP BY Usuario_Aprovacao
    HAVING(@Valor_Romaneio_Indevido_Inicial IS NULL
           OR (ABS(SUM(Valor_Indevido)) BETWEEN @Valor_Romaneio_Indevido_Inicial AND @Valor_Romaneio_Indevido_Final))
          AND (@Valor_Romaneio_Inicial IS NULL
               OR (ABS(SUM(Valor)) BETWEEN @Valor_Romaneio_Inicial AND @Valor_Romaneio_Final));
END;

IF OBJECT_ID('tempdb..#tmp_vendas_credito') IS NOT NULL
  BEGIN
    DROP TABLE #tmp_vendas_credito;
END;

IF OBJECT_ID('tempdb..#tmp_Romaneio') IS NOT NULL
  BEGIN
    DROP TABLE #tmp_Romaneio;
END;

IF OBJECT_ID('tempdb..#tmp_Credito_Aprovado') IS NOT NULL
  BEGIN
    DROP TABLE #tmp_Credito_Aprovado;
END;