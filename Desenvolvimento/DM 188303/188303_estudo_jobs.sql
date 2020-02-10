SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SET NOCOUNT ON;

DECLARE 
     @Dias_Analise_Trocas              INT,
     @Enum_Tipo_Troca                  INT, 
     @Enum_Motivo_Troca_Venda_Errada   INT, 
     @Enum_Motivo_Troca_Duvida_Produto INT

SET @Dias_Analise_Trocas               = 90;
SET @Enum_Tipo_Troca                   = 550;
SET @Enum_Motivo_Troca_Venda_Errada    = 574;
SET @Enum_Motivo_Troca_Duvida_Produto  = 1896;


     ------------------------------ Sumariza Venda ----------------------------
--WITH Venda
     --AS (
	 SELECT Romaneio_Venda_It.Objeto_Id  AS Objeto_Id, 
                COUNT(Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id) AS Qtde_Venda
         FROM Romaneio_Venda_Ct
              JOIN Romaneio_Venda_It
              ON Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id = Romaneio_Venda_It.Romaneio_Venda_Ct_Id
                 AND Romaneio_Venda_Ct.Lojas_Id = Romaneio_Venda_It.Lojas_Id
			  JOIN Peca
			  ON Romaneio_Venda_It.Objeto_ID = Peca.Peca_ID
         WHERE Peca.Peca_IsAtivo = 1
		       AND Enum_Tipo_Id NOT IN (@Enum_Tipo_Troca)
               AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao <= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
         GROUP BY Romaneio_Venda_It.Objeto_Id
		 --),

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
		      AND Enum_Tipo_Id IN(@Enum_Tipo_Troca)
              AND Romaneio_Venda_CT.Enum_Motivo_Troca_ID IN(@Enum_Motivo_Troca_Venda_Errada, @Enum_Motivo_Troca_Duvida_Produto)
         AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao <= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
         AND NOT EXISTS
         (
             SELECT 1
             FROM Solicitacao_Garantia_Ct
             WHERE Solicitacao_Garantia_Ct.Romaneio_Credito_Ct_Id = Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id
                   AND Loja_Credito_Ct_Id = Romaneio_Venda_Ct.Lojas_Id
         )
         GROUP BY Romaneio_Venda_It.Objeto_Id)

     --------------------------- Sumariza Porcentagem -------------------------
     SELECT Venda.Objeto_Id  AS Peca_ID, 
            Venda.Qtde_Venda AS Qtde_Venda, 
            Troca.Qtde_Troca AS Qtde_Troca, 
            CONVERT(NUMERIC(18, 6), CONVERT(DECIMAL(18, 6), Troca.Qtde_Troca) / CONVERT(DECIMAL(18, 6), Venda.Qtde_Venda)) * 100 AS Peca_Indice_Trocas
     FROM Venda
          JOIN Troca
          ON Venda.Objeto_Id = Troca.Objeto_Id
	WHERE Venda.Objeto_Id = 30906
     ORDER BY Peca_ID;
