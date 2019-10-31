-------------------------------------------------------------------------------
-- fmoraes 26/09/2019
-- DM 188303
-- Alerta ao vendedor sobre alto indíce de troca
-------------------------------------------------------------------------------


---------------------------------- PARAMETROS ---------------------------------

--SELECT * FROM Parametros_Sistema WHERE Parametros_Sistema_Tipo LIKE '%dias%'
--SELECT * FROM Processo WHERE Processo_Nome LIKE '%VENDA_TECNICA%' ORDER BY Processo_ID
--SELECT * FROM Processo_Parametros WHERE Processo_Parametros_Descricao LIKE '%perc%' ORDER BY Processo_ID
--SELECT TOP(5) * FROM Processo_Parametros_Valores


---------------------------------- ENUMERADOS ---------------------------------

--507  = Peca
--508  = Servico
--550  = Troca
--575  = Outros (Inativo)
--648  = Estorno
--1397 = Pendente
--1398 = Liberado
--1400 = Reativado
--1518 = Peça
--1897 = Adaptação de Peças
--1898 = Garantia

--SELECT * FROM Enumerado WHERE enum_nome = 'tipo_Dado'
--SELECT * FROM Enumerado WHERE Enum_Nome = 'MotivoTroca' 
--SELECT * FROM Enumerado WHERE Enum_Extenso = 'Troca'
--SELECT * FROM Enumerado WHERE Enum_ID = 1898

/*
SELECT * FROM Enumerado WHERE enum_id IN (
549,
551,
572,
797)
*/


----------------------------------- OBJETOS -----------------------------------

--SELECT * FROM sysobjects WHERE name LIKE '%garantia%' AND xtype = 'U'

/*
SELECT DISTINCT o.* 
FROM sysobjects o 
JOIN syscomments s 
ON o.id = s.id 
WHERE s.text like '%garantia%'
ORDER BY o.name 
*/
/*
SELECT * FROM Log_Alteracao_Objeto WHERE Objeto_Nome IN
(
'p_Sac_Consultar_Solicitacao_Garantia_Filtro_Documento',
'p_Sac_Consultar_Pagamento_Romaneio_Venda_Origem',
'p_Venda_Tecnica_Consultar_Romaneio_Venda_Grid'
)
AND Data_Alteracao > '2019-04-20'
*/


----------------------------------- VENTEC ------------------------------------

--SELECT TOP(5) * FROM Romaneio_Venda_CT WHERE Romaneio_Venda_CT_ID = 90064226 ORDER BY Romaneio_Venda_CT_ID DESC
--SELECT TOP(5) * FROM Romaneio_Venda_CT WHERE Enum_Motivo_Troca_ID = 1898 ORDER BY Romaneio_Venda_CT_ID DESC
--SELECT * FROM Romaneio_Venda_CT WHERE Enum_Tipo_ID = 648 AND Enum_Tipo_Origem_ID = 550 ORDER BY Romaneio_Venda_CT_ID DESC

/*
SELECT TOP(50) 
Romaneio_Venda_CT_ID, 
Enum_Tipo_ID, 
Enum_Status_ID, 
Enum_Motivo_Troca_ID, 
Enum_Tipo_Origem_ID, 
Romaneio_Venda_Grupo_ID, 
Objeto_Origem_ID, 
Loja_Origem_ID
FROM Romaneio_Venda_CT 
WHERE Lojas_ID = 1 AND Enum_Motivo_Troca_ID != 1898 
ORDER BY Romaneio_Venda_CT_ID DESC
*/

--SELECT * FROM Romaneio_Venda_IT WHERE Romaneio_Venda_CT_ID = 586792 ORDER BY Romaneio_Venda_IT_ID DESC
--SELECT * FROM Romaneio_Venda_IT WHERE Lojas_ID = 1 AND Objeto_ID = 27049 ORDER BY Romaneio_Venda_IT_ID DESC

/*
SELECT TOP (10) Romaneio_Venda_CT_ID, Objeto_ID, p.Peca_DSTecnica, COUNT(Objeto_ID) AS Qtde 
FROM Romaneio_Venda_IT 
INNER JOIN vw_Peca_Ativa_e_Visualiza AS p 
ON p.Peca_ID = Objeto_ID
WHERE Lojas_ID = 1
AND Enum_Objeto_Tipo_ID = 507
GROUP BY Romaneio_Venda_CT_ID, p.Peca_DSTecnica, Objeto_ID
HAVING COUNT(Objeto_ID) > 20
ORDER BY Romaneio_Venda_CT_ID DESC
*/

--SELECT TOP(5) * FROM Solicitacao_Garantia_CT ORDER BY Solicitacao_Garantia_CT_ID DESC
--SELECT * FROM Peca WHERE Peca_ID = 30384

/*
SELECT * FROM Peca WHERE Peca_ID = 7627
SELECT * FROM Peca_Foto WHERE Origem_ID = 7627

SELECT COUNT(*) FROM vw_Peca_Ativas
*/


-------------- p_Jobs_Venda_Tecnica_Atualizar_Indice_Troca_Pecas --------------

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
               AND Romaneio_Venda_Ct.Romaneio_Venda_Ct_Data_Geracao <= DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
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
     ORDER BY Peca_ID;


----------------- p_Jobs_Venda_Tecnica_Atualizar_Peca_Tem_Foto ----------------

DECLARE
     @Enum_Tipo_Uso_Foto_Peca_ID       INT;

SET @Enum_Tipo_Uso_Foto_Peca_ID        = 1518;


SELECT DISTINCT 
      Peca.Peca_ID AS Peca_ID, 
      ISNULL(Peca_Foto.Peca_Foto_IsAtivo, 0) AS Peca_Tem_Foto
FROM Peca_Foto
     JOIN Peca
     ON Peca_Foto.Origem_ID = Peca.Peca_ID
WHERE Peca_Foto.Enum_Tipo_Uso_ID = @Enum_Tipo_Uso_Foto_Peca_ID
      --AND ISNULL(Peca_Foto.Peca_Foto_IsAtivo, 0) = 1
ORDER BY Peca_ID;