SET ANSI_NULLS ON 
GO 
SET QUOTED_IDENTIFIER ON 
GO
 
-- =============================================
-- <summary>
--		Calcula percentual de troca sobre venda das peças
-- </summary>
-- <history>
-- 		[guisuzuki]	- 09/10/2019	Created
-- =============================================
--CREATE PROCEDURE p_Jobs_Venda_Tecnica_Atualizar_Indice_Troca_Pecas
--AS
BEGIN
 
-------------- p_Jobs_Venda_Tecnica_Atualizar_Indice_Troca_Pecas --------------
 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SET NOCOUNT ON;
 
DECLARE 
     @Dias_Analise_Trocas              INT,
     @Enum_Tipo_Troca                  INT, 
--     @Enum_Tipo_Estorno                INT, 
     @Enum_Motivo_Troca_Venda_Errada   INT, 
     @Enum_Motivo_Troca_Duvida_Produto INT, 
     @Enum_Objeto_Tipo_Peca            INT, 
	 @Enum_Tipo_Uso_Foto_Peca_ID       INT,
	 @Peca_ID                          INT;
 
SELECT TOP 1 
	@Dias_Analise_Trocas = Parametros_Sistema_Valor
FROM Parametros_Sistema 
WHERE 
	Parametros_Sistema.Parametros_Sistema_Tipo = 'DIAS_ANALISE_TROCAS_PECAS' 
	AND Parametros_Sistema.Lojas_ID = 1;
 
SET @Enum_Tipo_Troca                   = 550;
--SET @Enum_Tipo_Estorno                 = 648;
SET @Enum_Motivo_Troca_Venda_Errada    = 574;
SET @Enum_Motivo_Troca_Duvida_Produto  = 1896;
SET @Enum_Objeto_Tipo_Peca             = 507;
SET @Enum_Tipo_Uso_Foto_Peca_ID        = 1518;
SET @Peca_ID                           = 91083; --79472 --44495 --91083
 
 
     ------------------------------ Sumariza Venda ----------------------------
WITH Venda
     AS (SELECT Romaneio_Venda_It.Objeto_Id  AS Objeto_Id, 
                COUNT(Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id) AS Qtde_Venda
         FROM Romaneio_Venda_Ct
              JOIN Romaneio_Venda_It
              ON Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id = Romaneio_Venda_It.Romaneio_Venda_Ct_Id
                 AND Romaneio_Venda_Ct.Lojas_Id = Romaneio_Venda_It.Lojas_Id
			  JOIN Peca
			  ON Romaneio_Venda_It.Objeto_ID = Peca.Peca_ID
         WHERE Peca.Peca_IsAtivo = 1
		       AND Enum_Tipo_Id NOT IN (@Enum_Tipo_Troca)
               AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao >= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
			   AND  Peca.Peca_ID = @Peca_ID 
         GROUP BY Romaneio_Venda_It.Objeto_Id),
 
     ------------------------------ Sumariza Troca ----------------------------
     Troca
     AS (SELECT Romaneio_Venda_It.Objeto_Id AS Objeto_Id, 
                COUNT(Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id) AS Qtde_Troca
         FROM Romaneio_Venda_Ct
              JOIN Romaneio_Venda_It
              ON Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id = Romaneio_Venda_It.Romaneio_Venda_Ct_Id
                 AND Romaneio_Venda_Ct.Lojas_Id = Romaneio_Venda_It.Lojas_Id
			  JOIN Peca
			  ON Romaneio_Venda_It.Objeto_ID = Peca.Peca_ID
         WHERE Peca.Peca_IsAtivo = 1
		      AND Enum_Tipo_Id IN (@Enum_Tipo_Troca)
              AND Romaneio_Venda_CT.Enum_Motivo_Troca_ID IN (@Enum_Motivo_Troca_Venda_Errada, @Enum_Motivo_Troca_Duvida_Produto)
         AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao >= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
         AND NOT EXISTS
         (
             SELECT 1
             FROM Solicitacao_Garantia_Ct
             WHERE Solicitacao_Garantia_Ct.Romaneio_Credito_Ct_Id = Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id
                   AND Loja_Credito_Ct_Id = Romaneio_Venda_Ct.Lojas_Id
         )
		 AND  Peca.Peca_ID = @Peca_ID 
         GROUP BY Romaneio_Venda_It.Objeto_Id)
 
     --------------------------- Sumariza Porcentagem -------------------------
     SELECT 
	 CONVERT(NUMERIC(18, 6), CONVERT(DECIMAL(18, 6), Troca.Qtde_Troca)) AS Troca,
	 CONVERT(NUMERIC(18, 6), CONVERT(DECIMAL(18, 6), Venda.Qtde_Venda)) AS Venda, 
	 CONVERT(NUMERIC(18, 6), CONVERT(DECIMAL(18, 6), Troca.Qtde_Troca) / CONVERT(DECIMAL(18, 6), Venda.Qtde_Venda)) * 100 AS Porcentagem
     FROM Troca, Venda
 
END