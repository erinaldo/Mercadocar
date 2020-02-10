SET ANSI_NULLS ON 
GO 
SET QUOTED_IDENTIFIER ON 
GO
-------------------------------------------------------------------------------  
-- <summary>  
--      Procedure de Preenchimento de consulta de Romaneios para Pesquisa de
--      Romaneios       
-- </summary>  
-- <history>  
--    [msisiliani]    - 27/10/2011  Created
--    [rribani]   - 26/12/2011    Alterado
--      Alterado para exibir o código dos itens.
--    [gfpinheiro]  - 04/11/2016    Alterado
--      Converter para bigint na consulta por cupom ou nota fiscal.
--    [wpinheiro] - 02/11/2018    Alterado
--      removido convert bigint no romaneio documento
--    [mmukuno]   06/12/2018  Modified
--      Alterado para as tabelas novas de romaneio
--    [mmukuno]   08/11/2019  Modified
--      Novo parametro por peça
-- </history>
-----------------------------------------------------------------------------------  
CREATE PROCEDURE [dbo].[p_Venda_Tecnica_Consultar_Romaneio_Pesquisa_Demanda_202755]
(  
      @Lojas_ID               INT,
      @Data_Inicial_Pesquisa_Cliente      DATETIME,
      @Data_Final_Pesquisa_Cliente      DATETIME,
      @Enum_Tipo_Documento_Venda        INT,
      @Numero_Documento           VARCHAR(18),
      @Cliente_ID               uniqueidentifier,
      @Peca_ID                INT = NULL
)  
AS  
 
--/*********************************************************************/
--DECLARE
--      @Lojas_ID               INT,
--      @Data_Inicial_Pesquisa_Cliente      DATETIME,
--      @Data_Final_Pesquisa_Cliente      DATETIME,
--      @Enum_Tipo_Documento_Venda        INT,
--      @Numero_Documento           VARCHAR(18),
--      @Cliente_ID               uniqueidentifier 
 
--SET     @Lojas_ID               = 1
--SET     @Data_Inicial_Pesquisa_Cliente      = '2018-10-26 15:00:00'
--SET     @Data_Final_Pesquisa_Cliente      = '2018-12-26 16:00:00'
--SET     @Enum_Tipo_Documento_Venda        = NULL --936 Cupom Fiscal, 937 Nota Fiscal, 935 Grupo, 934 Romaneio
--SET     @Numero_Documento           = NULL
--SET     @Cliente_ID               = '8E0B938F-8DB5-4365-8DEB-67EF2017E8BA'
--/*********************************************************************/
 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SET NOCOUNT ON
 
DECLARE   @Enum_Tipo_Documento_Venda_Romaneio_ID      INT,
      @Enum_Tipo_Documento_Venda_Grupo_ID       INT,
      @Enum_Tipo_Documento_Venda_Cupom_Fiscal_ID    INT,
      @Enum_Tipo_Documento_Venda_Nota_Fiscal_ID   INT,
      @Enum_Tipo_Romaneio_Venda_Tecnica       INT,
      @Enum_Tipo_Romaneio_Auto_Servico        INT,
      @Enum_Tipo_Romaneio_Especial          INT,
      @Enum_Tipo_Objeto_Peca              INT,
      @Enum_Tipo_Objeto_Servico           INT,
      @Enum_Tipo_Objeto_Encomenda           INT,
      @Enum_Status_Romaneio_Venda_Liberado      INT 
      
SET     @Enum_Tipo_Romaneio_Venda_Tecnica       = 549
SET     @Enum_Tipo_Romaneio_Auto_Servico        = 551
SET     @Enum_Tipo_Romaneio_Especial          = 572
SET     @Enum_Tipo_Documento_Venda_Romaneio_ID      = 934
SET     @Enum_Tipo_Documento_Venda_Grupo_ID       = 935
SET     @Enum_Tipo_Documento_Venda_Cupom_Fiscal_ID    = 936
SET     @Enum_Tipo_Documento_Venda_Nota_Fiscal_ID   = 937
SET     @Enum_Tipo_Objeto_Peca              = 507
SET     @Enum_Tipo_Objeto_Servico           = 508
SET     @Enum_Tipo_Objeto_Encomenda           = 509
SET     @Enum_Status_Romaneio_Venda_Liberado      = 1398
 
 
/****************Pesquisa por Romaneio**********************/
IF  @Enum_Tipo_Documento_Venda = @Enum_Tipo_Documento_Venda_Romaneio_ID 
 
    SELECT DISTINCT
      Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID        AS Romaneio_Ct_ID,
      Romaneio_Venda_Grupo.Romaneio_Grupo_ID          AS Romaneio_Grupo_ID,     
      CASE 
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca THEN Fabricante_CD + '.' + Produto_CD + '.' + Peca_CD                  
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico THEN Servico_CD
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Encomenda THEN CONVERT(VARCHAR(10), Objeto_ID)
      ELSE
        CAST('' AS VARCHAR(1))
      END                           AS Codigo,
      ISNULL(servico.Servico_Descricao,vw_Peca.Produto_DsResum  + ' - ' + vw_Peca.Peca_DSTecnica) AS Peca_DS,
 
      Tipo_Romaneio.Enum_Extenso                AS Tipo_Romaneio, 
      Romaneio_Venda_CT.Lojas_ID                AS Lojas_ID,
      Lojas.Lojas_NM                      AS Lojas_NM,
      dbo.fun_Retorna_Cliente_Nome(Romaneio_Venda_CT.Cliente_ID)  AS Cliente_Nome,
      Romaneio_Venda_CT.Romaneio_Venda_CT_Data_Liberacao    AS Romaneio_Documento_Data_Emissao,
      Vendedor.Nome_Completo                  AS Vendedor_Nome,
      
      ISNULL(Romaneio_Venda_IT.Romaneio_Venda_IT_Preco_Pago,0)  AS Valor_Romaneio,
      Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_Valor_Pago  AS Valor_Venda
    FROM    
      Romaneio_Venda_Grupo
    INNER JOIN Romaneio_Venda_CT ON
      Romaneio_Venda_Grupo.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
    AND
      Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_ID = Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID
    INNER JOIN Romaneio_Venda_IT ON
      Romaneio_Venda_IT.Romaneio_Venda_CT_ID = Romaneio_Venda_CT.Romaneio_Venda_CT_ID
    AND
      Romaneio_Venda_IT.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
    LEFT OUTER JOIN vw_Peca ON
      vw_Peca.Peca_ID = Romaneio_Venda_IT.Objeto_ID
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca
    LEFT OUTER JOIN Servico ON 
      Romaneio_Venda_IT.Objeto_ID = Servico.Servico_ID 
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico
    LEFT JOIN vw_Usuario Vendedor ON
      Vendedor.Usuario_ID = Romaneio_Venda_CT.Usuario_Vendedor_ID
    INNER JOIN  Enumerado Tipo_Romaneio ON 
      Tipo_Romaneio.Enum_ID = Romaneio_Venda_CT.Enum_Tipo_ID
    INNER JOIN Lojas ON
      Romaneio_Venda_CT.Lojas_ID = Lojas.Lojas_Id
    WHERE   
      (Romaneio_Venda_CT.Lojas_ID = @Lojas_ID OR @Lojas_ID IS NULL)
    AND
      Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID = @Numero_Documento
    AND 
      Romaneio_Venda_CT.Enum_Tipo_ID IN (@Enum_Tipo_Romaneio_Venda_Tecnica, @Enum_Tipo_Romaneio_Auto_Servico, @Enum_Tipo_Romaneio_Especial)
          
 
/****************Pesquisa por GRUPO**********************/
IF  @Enum_Tipo_Documento_Venda = @Enum_Tipo_Documento_Venda_Grupo_ID
 
    SELECT DISTINCT 
      Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID        AS Romaneio_Ct_ID,
      Romaneio_Venda_Grupo.Romaneio_Grupo_ID          AS Romaneio_Grupo_ID,     
      CASE 
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca THEN Fabricante_CD + '.' + Produto_CD + '.' + Peca_CD                  
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico THEN Servico_CD
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Encomenda THEN CONVERT(VARCHAR(10), Objeto_ID)
      ELSE
        CAST('' AS VARCHAR(1))
      END                           AS Codigo,
      ISNULL(servico.Servico_Descricao,vw_Peca.Produto_DsResum + ' - ' + vw_Peca.Peca_DSTecnica)  AS Peca_DS,
      Tipo_Romaneio.Enum_Extenso                AS Tipo_Romaneio,       
      Romaneio_Venda_CT.Lojas_ID                AS Lojas_ID,
      Lojas.Lojas_NM                      AS Lojas_NM,      
      dbo.fun_Retorna_Cliente_Nome(Romaneio_Venda_CT.Cliente_ID)  AS Cliente_Nome,
      Romaneio_Venda_Grupo_Data_Liberacao           AS Romaneio_Documento_Data_Emissao,
      Vendedor.Nome_Completo                  AS Vendedor_Nome,
      ISNULL(Romaneio_Venda_IT.Romaneio_Venda_IT_Preco_Pago,0)  AS Valor_Romaneio,
      Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_Valor_Pago  AS Valor_Venda
    FROM    
      Romaneio_Venda_Grupo
    INNER JOIN  Romaneio_Venda_CT  ON
      Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_ID
    AND 
      Romaneio_Venda_CT.Lojas_ID = Romaneio_Venda_Grupo.Lojas_ID
    INNER JOIN Romaneio_Venda_IT ON
      Romaneio_Venda_IT.Romaneio_Venda_CT_ID = Romaneio_Venda_CT.Romaneio_Venda_CT_ID
    AND
      Romaneio_Venda_IT.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
    LEFT OUTER JOIN vw_Peca ON
      vw_Peca.Peca_ID = Romaneio_Venda_IT.Objeto_ID
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca
    LEFT OUTER JOIN Servico ON 
      Romaneio_Venda_IT.Objeto_ID = Servico.Servico_ID 
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico
    LEFT JOIN vw_Usuario Vendedor ON
      Vendedor.Usuario_ID = Romaneio_Venda_CT.Usuario_Vendedor_ID 
    INNER JOIN  Enumerado Tipo_Romaneio ON
      Tipo_Romaneio.Enum_ID = Romaneio_Venda_CT.Enum_Tipo_ID
    INNER JOIN Lojas ON
      Romaneio_Venda_CT.Lojas_ID = Lojas.Lojas_Id
    WHERE   
      (Romaneio_Venda_Grupo.Lojas_ID = @Lojas_ID OR @Lojas_ID IS NULL)
    AND
      Romaneio_Venda_Grupo.Romaneio_Grupo_ID = @Numero_Documento
    AND 
      Romaneio_Venda_CT.Enum_Tipo_ID IN (@Enum_Tipo_Romaneio_Venda_Tecnica, @Enum_Tipo_Romaneio_Auto_Servico, @Enum_Tipo_Romaneio_Especial)
         
/****************Pesquisa por CUPOM ou NF**********************/           
IF  @Enum_Tipo_Documento_Venda = @Enum_Tipo_Documento_Venda_Cupom_Fiscal_ID OR
  @Enum_Tipo_Documento_Venda = @Enum_Tipo_Documento_Venda_Nota_Fiscal_ID 
 
    SELECT DISTINCT 
      Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID        AS Romaneio_Ct_ID,
      Romaneio_Venda_Grupo.Romaneio_Grupo_ID          AS Romaneio_Grupo_ID,     
      CASE 
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca THEN Fabricante_CD + '.' + Produto_CD + '.' + Peca_CD
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico THEN Servico_CD
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Encomenda THEN CONVERT(VARCHAR(10), Objeto_ID)
      ELSE
        CAST('' AS VARCHAR(1))
      END                           AS Codigo,
      
      ISNULL(servico.Servico_Descricao,vw_Peca.Produto_DsResum  + ' - ' + vw_Peca.Peca_DSTecnica) AS Peca_DS,
      
      Tipo_Romaneio.Enum_Extenso                AS Tipo_Romaneio,   
      Romaneio_Venda_CT.Lojas_ID                AS Lojas_ID,
      Lojas.Lojas_NM                      AS Lojas_NM,    
      dbo.fun_Retorna_Cliente_Nome(Romaneio_Venda_CT.Cliente_ID)  AS Cliente_Nome,
      Romaneio_Venda_Grupo_Data_Liberacao           AS Romaneio_Documento_Data_Emissao,     
      Vendedor.Nome_Completo                  AS Vendedor_Nome,
      ISNULL(Romaneio_Venda_IT.Romaneio_Venda_IT_Preco_Pago,0)  AS Valor_Romaneio,
      Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_Valor_Pago  AS Valor_Venda
    FROM    
      Romaneio_Venda_Grupo
    INNER JOIN  Romaneio_Venda_CT  ON
      Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_ID
    AND 
      Romaneio_Venda_CT.Lojas_ID = Romaneio_Venda_Grupo.Lojas_ID
    INNER JOIN Romaneio_Venda_IT ON
      Romaneio_Venda_IT.Romaneio_Venda_Ct_ID = Romaneio_Venda_CT.Romaneio_Venda_Ct_ID
    AND
      Romaneio_Venda_IT.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
    LEFT OUTER JOIN vw_Peca ON
      vw_Peca.Peca_ID = Romaneio_Venda_IT.Objeto_ID
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca
    LEFT OUTER JOIN Servico ON 
      Romaneio_Venda_IT.Objeto_ID = Servico.Servico_ID 
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico
    LEFT JOIN vw_Usuario Vendedor ON
      Vendedor.Usuario_ID = Romaneio_Venda_CT.Usuario_Vendedor_ID
    INNER JOIN  Enumerado Tipo_Romaneio ON
      Tipo_Romaneio.Enum_ID = Romaneio_Venda_CT.Enum_Tipo_ID
    INNER JOIN Lojas ON
      Romaneio_Venda_CT.Lojas_ID = Lojas.Lojas_Id
    WHERE   
      (Romaneio_Venda_CT.Lojas_ID = @Lojas_ID OR @Lojas_ID IS NULL)
    AND
      Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_Numero_Documento =  @Numero_Documento
    AND 
      Romaneio_Venda_CT.Enum_Tipo_ID IN (@Enum_Tipo_Romaneio_Venda_Tecnica, @Enum_Tipo_Romaneio_Auto_Servico, @Enum_Tipo_Romaneio_Especial)
 
 
/****************Pesquisa por Cliente e Data**********************/
IF @Enum_Tipo_Documento_Venda IS NULL
    
    SELECT DISTINCT 
      Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID        AS Romaneio_Ct_ID,
      Romaneio_Venda_Grupo.Romaneio_Grupo_ID          AS Romaneio_Grupo_ID,     
      CASE 
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca THEN Fabricante_CD + '.' + Produto_CD + '.' + Peca_CD
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico THEN Servico_CD
         WHEN Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Encomenda THEN CONVERT(VARCHAR(10), Objeto_ID)
      ELSE
        CAST('' AS VARCHAR(1))
      END                           AS Codigo,
      
      ISNULL(servico.Servico_Descricao,vw_Peca.Produto_DsResum  + ' - ' + vw_Peca.Peca_DSTecnica) AS Peca_DS,
      Tipo_Romaneio.Enum_Extenso                AS Tipo_Romaneio,   
      Romaneio_Venda_CT.Lojas_ID                AS Lojas_ID,
      Lojas.Lojas_NM                      AS Lojas_NM,    
      dbo.fun_Retorna_Cliente_Nome(Romaneio_Venda_CT.Cliente_ID)  AS Cliente_Nome,
      Romaneio_Venda_Grupo_Data_Liberacao           AS Romaneio_Documento_Data_Emissao,   
      Vendedor.Nome_Completo                  AS Vendedor_Nome,
      ISNULL(Romaneio_Venda_IT.Romaneio_Venda_IT_Preco_Pago,0) AS Valor_Romaneio,
      Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_Valor_Pago  AS Valor_Venda
    FROM    
      Romaneio_Venda_Grupo
    INNER JOIN  Romaneio_Venda_CT  ON
      Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = Romaneio_Venda_Grupo.Romaneio_Venda_Grupo_ID
    AND 
      Romaneio_Venda_CT.Lojas_ID = Romaneio_Venda_Grupo.Lojas_ID
    INNER JOIN Romaneio_Venda_IT ON
      Romaneio_Venda_IT.Romaneio_Venda_CT_ID = Romaneio_Venda_CT.Romaneio_Venda_CT_ID
    AND
      Romaneio_Venda_IT.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
    LEFT OUTER JOIN vw_Peca ON
      vw_Peca.Peca_ID = Romaneio_Venda_IT.Objeto_ID
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Peca
    LEFT OUTER JOIN Servico ON 
      Romaneio_Venda_IT.Objeto_ID = Servico.Servico_ID 
    AND
      Romaneio_Venda_IT.Enum_Objeto_Tipo_ID = @Enum_Tipo_Objeto_Servico
    LEFT JOIN vw_Usuario Vendedor ON
      Vendedor.Usuario_ID = Romaneio_Venda_CT.Usuario_Vendedor_ID 
    INNER JOIN  Enumerado Tipo_Romaneio ON 
      Tipo_Romaneio.Enum_ID = Romaneio_Venda_CT.Enum_Tipo_ID
    INNER JOIN Lojas ON
      Romaneio_Venda_CT.Lojas_ID = Lojas.Lojas_Id
    WHERE   
      (Romaneio_Venda_CT.Lojas_ID = @Lojas_ID OR @Lojas_ID IS NULL)
    AND
      Romaneio_Venda_CT.Cliente_ID = @Cliente_ID
    AND 
      Romaneio_Venda_CT.Enum_Tipo_ID IN (@Enum_Tipo_Romaneio_Venda_Tecnica, @Enum_Tipo_Romaneio_Auto_Servico, @Enum_Tipo_Romaneio_Especial)
    AND 
      Romaneio_Venda_CT.Romaneio_Venda_CT_Data_Liberacao BETWEEN @Data_Inicial_Pesquisa_Cliente AND DATEADD(DAY, +1, @Data_Final_Pesquisa_Cliente)
    AND 
      (Romaneio_Venda_IT.Objeto_ID = @Peca_ID OR @Peca_ID IS NULL)