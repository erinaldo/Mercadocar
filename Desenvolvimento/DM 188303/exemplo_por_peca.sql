--------------------------------------------------------------------------
-- fmoraes 26/09/2019

-- DM 188303
-- Alerta ao vendedor sobre alto indíce de troca

--------------------------------------------------------------------------


----------------- PARAMETROS -----------------

--SELECT * FROM Parametros_Sistema WHERE Parametros_Sistema_Tipo LIKE '%dias%'

--SELECT * FROM Processo WHERE Processo_Nome LIKE '%VENDA_TECNICA%' ORDER BY Processo_ID

--SELECT * FROM Processo_Parametros WHERE Processo_Parametros_Descricao LIKE '%perc%' ORDER BY Processo_ID

--SELECT TOP(5) * FROM Processo_Parametros_Valores


----------------- ENUMERADOS -----------------

--1898 = Garantia
--507  = Peca
--508  = Servico
--550  = Troca
--1398 = Liberado

--SELECT * FROM Enumerado WHERE enum_nome = 'tipo_Dado'
--SELECT * FROM Enumerado WHERE Enum_Nome = 'MotivoTroca' 
--SELECT * FROM Enumerado WHERE Enum_Extenso = 'Troca'
--SELECT * FROM Enumerado WHERE Enum_ID = 508

/*
SELECT * FROM Enumerado WHERE enum_id IN (
549,
551,
572,
797)
*/


----------------- OBJETOS -----------------

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

----------------- VENTEC -----------------

--SELECT TOP(5) * FROM Romaneio_Venda_CT WHERE Romaneio_Venda_CT_ID = 90064226 ORDER BY Romaneio_Venda_CT_ID DESC

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

--SELECT * FROM Romaneio_Venda_IT WHERE Romaneio_Venda_CT_ID = 90063613 ORDER BY Romaneio_Venda_IT_ID DESC
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


----------------- GARANTIA -----------------

--sp p_Jobs_DW_Peca_Resumo


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

SET @Peca_ID                     = 56138; --56138 --50695 --27049--30377 --110646;
SET @Enum_Tipo_Troca             = 550;
SET @Enum_Tipo_Estorno           = 648;
SET @Enum_Status_Liberado        = 1398;
SET @Enum_Motivo_Troca_Garantia  = 1898;
SET @Enum_Objeto_Tipo_Peca       = 507;
SET @Dias_Analise_Trocas         = 90;


----------------------------
-- TOTAL DE TROCAS E VENDAS DO ITEM
----------------------------

SELECT DISTINCT 
       CT.Lojas_ID, 
       IT.Objeto_ID AS Peca_ID, 
       p.Peca_DSTecnica, 
	   ISNULL(TROCA.Peca_Qtd_Trocas, 0) AS Peca_Qtd_Trocas,
       COUNT(IT.Objeto_ID) AS Peca_Qtd_Vendas
	   
FROM Romaneio_Venda_IT AS IT
     INNER JOIN Romaneio_Venda_CT AS CT
     ON CT.Romaneio_Venda_CT_ID = IT.Romaneio_Venda_CT_ID
	 AND CT.Lojas_Id = IT.Lojas_Id
     INNER JOIN vw_Peca_Ativa_e_Visualiza AS p
     ON p.Peca_ID = @Peca_ID
     LEFT JOIN
(
    SELECT DISTINCT 
           CT.Lojas_ID, 
           IT.Objeto_ID, 
           p.Peca_DSTecnica, 
           COUNT(IT.Objeto_ID) AS Peca_Qtd_Trocas
    FROM Romaneio_Venda_IT AS IT
         INNER JOIN Romaneio_Venda_CT AS CT
         ON CT.Romaneio_Venda_CT_ID = IT.Romaneio_Venda_CT_ID
		 AND CT.Lojas_Id = IT.Lojas_Id
         INNER JOIN vw_Peca_Ativa_e_Visualiza AS p
         ON p.Peca_ID = @Peca_ID
    WHERE Enum_Status_ID = @Enum_Status_Liberado
          AND Enum_Objeto_Tipo_ID = @Enum_Objeto_Tipo_Peca
          AND Enum_Tipo_ID IN(@Enum_Tipo_Troca, @Enum_Tipo_Estorno)
         AND Objeto_ID = @Peca_ID
         AND Romaneio_Venda_CT_Data_Geracao > DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
    GROUP BY CT.Lojas_ID, 
             IT.Objeto_ID, 
             p.Peca_DSTecnica
) AS TROCA
     ON CT.Lojas_ID = TROCA.Lojas_ID
        AND IT.Objeto_ID = TROCA.Objeto_ID
        AND p.Peca_DSTecnica = TROCA.Peca_DSTecnica
WHERE Enum_Status_ID = @Enum_Status_Liberado
      AND Enum_Objeto_Tipo_ID = @Enum_Objeto_Tipo_Peca
      AND Enum_Tipo_ID NOT IN(@Enum_Tipo_Troca, @Enum_Tipo_Estorno)
--AND Enum_Motivo_Troca_ID NOT IN (@Enum_Motivo_Troca_Garantia)
AND IT.Objeto_ID = @Peca_ID
AND Romaneio_Venda_CT_Data_Geracao > DATEADD(Day, -@Dias_Analise_Trocas, GETDATE())
GROUP BY CT.Lojas_ID, 
         P.Peca_DSTecnica, 
         IT.Objeto_ID, 
         TROCA.Peca_Qtd_Trocas
ORDER BY CT.Lojas_ID;