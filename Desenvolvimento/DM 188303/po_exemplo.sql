/*
SELECT * FROM Enumerado WHERE Enum_Nome = 'MotivoTroca'

Enum_ID     Enum_Sigla      Enum_Extenso                                       Enum_Nome                                          Enum_IsAtivo
----------- --------------- -------------------------------------------------- -------------------------------------------------- ------------
573         NULL            Compra Errada                                      MotivoTroca                                        1
574         NULL            Venda Errada                                       MotivoTroca                                        1
830         NULL            Mata - Mata                                        MotivoTroca                                        1
843                         Crédito de Bateria                                 MotivoTroca                                        1
1896        NULL            Dúvidas sobre o modelo                             MotivoTroca                                        1
1897        NULL            Adaptação de Peças                                 MotivoTroca                                        1
1898        NULL            Garantia                                           MotivoTroca                                        1
1899        NULL            Troca de Marca                                     MotivoTroca                                        1
1900        NULL            Peça quebrada                                      MotivoTroca                                        1
1901        NULL            Etiqueta da embalagem errada                       MotivoTroca                                        1
1902        NULL            Embalagem da peça errada                           MotivoTroca                                        1
1903        NULL            Cobrou o lado oposto                               MotivoTroca                                        1
*/

----------------- JOB -----------------

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SET NOCOUNT ON;

DECLARE 
     @Peca_ID                    INT, 
     @Enum_Tipo_Troca            INT, 
     @Enum_Tipo_Estorno          INT, 
     @Enum_Status_Liberado       INT, 
     @Enum_Motivo_Troca_Garantia INT, 
     @Enum_Objeto_Tipo_Peca      INT, 
     @Dias_Analise_Trocas        INT;

SET @Peca_ID                     = 56138; --50695 --27049--30377 --110646;
SET @Enum_Tipo_Troca             = 550;
SET @Enum_Tipo_Estorno           = 648;
SET @Enum_Status_Liberado        = 1398;
SET @Enum_Motivo_Troca_Garantia  = 1898;
SET @Enum_Objeto_Tipo_Peca       = 507;
SET @Dias_Analise_Trocas         = 90;


------------------------------------
-- TOTAL DE TROCAS E VENDAS DO ITEM
------------------------------------

WITH Venda
     AS (SELECT Romaneio_Venda_Ct.Lojas_Id, 
                Romaneio_Venda_It.Objeto_Id, 
                COUNT(Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id) AS Qtde_Venda
         FROM Romaneio_Venda_Ct
              JOIN Romaneio_Venda_It
              ON Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id = Romaneio_Venda_It.Romaneio_Venda_Ct_Id
                 AND Romaneio_Venda_Ct.Lojas_Id = Romaneio_Venda_It.Lojas_Id
         WHERE Enum_Tipo_Id NOT IN(@Enum_Tipo_Troca, @Enum_Tipo_Estorno)
              AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao <= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
              AND Romaneio_Venda_Ct.Lojas_Id = 1 --TESTE EM PRODUCAO
         GROUP BY Romaneio_Venda_Ct.Lojas_Id, 
                  Romaneio_Venda_It.Objeto_Id
		--ORDER BY 
		--	Romaneio_Venda_IT.Objeto_ID
),
     Troca
     AS (

     -- Troca
     SELECT Romaneio_Venda_Ct.Lojas_Id, 
            Romaneio_Venda_It.Objeto_Id, 
            COUNT(Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id) AS Qtde_Troca
     FROM Romaneio_Venda_Ct
          JOIN Romaneio_Venda_It
          ON Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id = Romaneio_Venda_It.Romaneio_Venda_Ct_Id
             AND Romaneio_Venda_Ct.Lojas_Id = Romaneio_Venda_It.Lojas_Id
     WHERE Enum_Tipo_Id IN(@Enum_Tipo_Troca, @Enum_Tipo_Estorno)
          AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao <= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
          --AND Romaneio_Venda_CT.Enum_Motivo_Troca_ID <> @Enum_Motivo_Troca_Garantia
          AND Romaneio_Venda_Ct.Lojas_Id = 1 --TESTE EM PRODUCAO
          AND NOT EXISTS
     (
         SELECT 1
         FROM Solicitacao_Garantia_Ct
         WHERE Solicitacao_Garantia_Ct.Romaneio_Credito_Ct_Id = Romaneio_Venda_Ct.Romaneio_Venda_Ct_Id
               AND Loja_Credito_Ct_Id = Romaneio_Venda_Ct.Lojas_Id
     )
     GROUP BY Romaneio_Venda_Ct.Lojas_Id, 
              Romaneio_Venda_It.Objeto_Id
     --ORDER BY
     --	Romaneio_Venda_IT.Objeto_ID
     )
     SELECT Venda.Lojas_Id, 
            Venda.Objeto_Id, 
            Venda.Qtde_Venda, 
            Troca.Qtde_Troca, 
            CONVERT(NUMERIC(18, 6), CONVERT(DECIMAL(18, 6), Troca.Qtde_Troca) / CONVERT(DECIMAL(18, 6), Venda.Qtde_Venda)) * 100 AS Percentual
     FROM Venda
          JOIN Troca
          ON Venda.Lojas_Id = Troca.Lojas_Id
             AND Venda.Objeto_Id = Troca.Objeto_Id;