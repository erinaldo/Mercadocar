
----------------- JOB -----------------

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
SET NOCOUNT ON;

DECLARE 
     @Peca_ID                     INT, 
     @Enum_Tipo_Troca             INT, 
     @Enum_Tipo_Estorno           INT, 
     @Enum_Status_Liberado        INT, 
     @Enum_Motivo_Troca_Garantia  INT, 
	 @Enum_Objeto_Tipo_Peca       INT,
	 @Dias_Analise_Trocas         INT;

SET @Peca_ID                      = 56138 --50695 --27049--30377 --110646;
SET @Enum_Tipo_Troca              = 550;
SET @Enum_Tipo_Estorno            = 648;
SET @Enum_Status_Liberado         = 1398;
SET @Enum_Motivo_Troca_Garantia   = 1898;
SET @Enum_Objeto_Tipo_Peca        = 507;
SET @Dias_Analise_Trocas          = 90

/*
DECLARE @Temp TABLE
(
	Lojas_ID		INT,
	Peca_ID         INT, 
	Peca_Qtd_Vendas INT,
	Peca_Qtd_Trocas	INT
	
)
*/

----------------------------
-- TOTAL DE VENDAS DO ITEM
----------------------------

--INSERT INTO @Temp (Lojas_ID, Peca_ID, Peca_Qtd_Vendas)
SELECT DISTINCT
      CT.Lojas_ID,
      --CT.Loja_Origem_ID,
	  --Enum_Tipo_ID,
       Objeto_ID AS Peca_ID,
       --dbo.fun_Retorna_Codigo_Mercadocar_Peca(IT.Objeto_ID) AS Cod_MercadoCar,
	   Peca_DSTecnica, 
       Count(IT.Objeto_ID) AS Peca_Qtd_Vendas
FROM Romaneio_Venda_IT AS IT
     INNER JOIN Romaneio_Venda_CT AS CT
     ON CT.Romaneio_Venda_CT_ID = IT.Romaneio_Venda_CT_ID
	 INNER JOIN vw_Peca_Ativa_e_Visualiza AS p
	 ON p.Peca_ID = @Peca_ID
WHERE 
     Enum_Status_ID = @Enum_Status_Liberado
	 AND Enum_Objeto_Tipo_ID = @Enum_Objeto_Tipo_Peca
	 AND Enum_Tipo_ID NOT IN (@Enum_Tipo_Troca, @Enum_Tipo_Estorno)
     --AND Enum_Motivo_Troca_ID NOT IN (@Enum_Motivo_Troca_Garantia)
     AND Objeto_ID = @Peca_ID
	 AND Romaneio_Venda_CT_Data_Geracao > dateadd(Day, - @Dias_Analise_Trocas, getdate())
GROUP BY 
CT.Lojas_ID, 
--Loja_Origem_ID,
--Enum_Tipo_ID, 
Peca_DSTecnica,
Objeto_ID;


----------------------------
-- TOTAL DE TROCAS DO ITEM
----------------------------

--INSERT INTO @Temp (Lojas_ID, Peca_ID, Peca_Qtd_Trocas)
SELECT DISTINCT
       CT.Lojas_ID,
	   --CT.Loja_Origem_ID,
       --Enum_Tipo_ID,
       Objeto_ID AS Peca_ID,
       --dbo.fun_Retorna_Codigo_Mercadocar_Peca(IT.Objeto_ID) AS Cod_MercadoCar,
	   p.Peca_DSTecnica, 
       Count(IT.Objeto_ID) AS Peca_Qtd_Trocas
FROM Romaneio_Venda_IT AS IT
     INNER JOIN Romaneio_Venda_CT AS CT
     ON CT.Romaneio_Venda_CT_ID = IT.Romaneio_Venda_CT_ID
	 INNER JOIN vw_Peca_Ativa_e_Visualiza AS p
	 ON p.Peca_ID = @Peca_ID
WHERE 
     Enum_Status_ID = @Enum_Status_Liberado
	 AND Enum_Objeto_Tipo_ID = @Enum_Objeto_Tipo_Peca
     AND Enum_Tipo_ID IN(@Enum_Tipo_Troca, @Enum_Tipo_Estorno)
     AND Objeto_ID = @Peca_ID
	 AND Romaneio_Venda_CT_Data_Geracao > dateadd(Day, - @Dias_Analise_Trocas, getdate())
GROUP BY 
CT.Lojas_ID, 
--CT.Loja_Origem_ID,
--CT.Enum_Tipo_ID, 
Peca_DSTecnica,
Objeto_ID;


--SELECT * FROM @Temp