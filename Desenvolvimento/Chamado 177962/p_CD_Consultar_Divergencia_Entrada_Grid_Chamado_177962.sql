-------------------------------------------------------------------------------
-- <summary>
--   Procedure de Preenchimento do Grid de Divergencias de Entrada.
-- </summary>
-- <history>
--  [econforti]  - 17/10/2012 Created
--  [wpinheiro]  - 13/06/2013 Modified
--   Adicionado coluna data
--  [wpinheiro]  - 15/04/2014 Modified
--   Adicionado coluna estoque e endereço
--  [bmune]   - 02/02/2015 Modified
--   Adicionado um novo filtro pelo tipo de pre recebimento
--  [moshiro]  - 11/03/2015 Modified
--   Alteração das colunas buscadas
--  [moshiro]  - 20/03/2015 Modified
--   Alterado para Left Join na Recebimento_CT e Recebimento_IT, pois
--  apenas retornava um tipo de separação (conferência e guarda não
--  retornava).
--  [moshiro]  - 06/04/2015 Modified
--   Tratamento do novo recurso de Pareceres
--  [mmukuno]  - 07/09/2015 Modified
--   Desconsiderar o grupo de volume com status em pendente de separação
--  [rnoliveira] - 09/09/2015 Modified
--   Separar a consulta qdo for feito uma consulta por CD e outra por Loja
--   quando for por CD é criado uma temporaria sem o numero do volume
--   e ao consultar os itens da temporaria é feito uma subquery para rotornar
--   o numero do volume caso exista.
--  [mmukuno]  - 21/09/2015 Modified
--   Coluna Destino
--  [bmune]   - 23/08/2016 Modified
--   Retirado a coluna comprador_ID, pois não esta sendo usado na tela e está
--   causando um erro ao trazer mais de um comprador daquele fabricante  
--  [bmune]   - 30/05/2017 Modified
--   Alterado o JOIN do fabricante comprador para ser LEFT JOIN
--  [efsousa]   - 27/09/2018 Modified
--   Retirado HINT de Recompile
--  [bmune]   - 14/03/2019 Modified
--   Modificado a consulta para trazer os dados do pre_recebimento
--  [bmune]   - 19/03/2019 Modified
--   Modificado o where para não consultar nas tabelas novas a informação de tipo
--  [ladnascimento]   -22/03/2019 Modified
--   Inclusão de clausula where complexa para filtro @Tipo_Pre_Recebimento_ID
--  [bmune]   - 08/05/2019 Modified
--   Modificado 
-- </history>
-------------------------------------------------------------------------------  
CREATE PROCEDURE [dbo].[p_CD_Consultar_Divergencia_Entrada_Grid_Chamado_177962] (
  @Lojas_Origem_ID           INT,
  @Status                    udtEnumerado READONLY,
  @Enum_Tipo_ID              INT,
  @Date_Inicial              DATETIME,
  @Date_Final                DATETIME,
  @Fabricante_ID             INT,
  @Produto_ID                INT,
  @Peca_ID                   INT,
  @Tipo_Pre_Recebimento_ID   INT,
  @Usuario_Comprador_ID      INT,
  @Status_Parecer            udtEnumerado READONLY,
  @Date_Inicial_Parecer      DATETIME,
  @Date_Final_Parecer        DATETIME)
AS
-------------------------------------------------------------------------------
--DECLARE
-- @Lojas_Origem_ID            INT,
-- @Enum_Tipo_ID               INT,
-- @Date_Inicial               DATETIME,
-- @Date_Final                 DATETIME,
-- @Fabricante_ID              INT,
-- @Produto_ID                 INT,
-- @Peca_ID                    INT,
-- @Tipo_Pre_Recebimento_ID    INT,
-- @Status                     udtEnumerado,
-- @Usuario_Comprador_ID       INT,
-- @Status_Parecer             udtEnumerado,
-- @Date_Inicial_Parecer       DATETIME,
-- @Date_Final_Parecer         DATETIME

--SET @Lojas_Origem_ID         = 15
--SET @Enum_Tipo_ID            = 2530
--SET @Date_Inicial            = NULL--'1900-01-01'
--SET @Date_Final              = NULL--'1900-01-01'
--SET @Fabricante_ID           = NULL
--SET @Produto_ID              = NULL
--SET @Peca_ID                 = NULL --62659 --44274
--SET @Tipo_Pre_Recebimento_ID = 168--NULL
--SET @Usuario_Comprador_ID    = NULL
--SET @Date_Inicial_Parecer    = NULL
--SET @Date_Final_Parecer      = NULL
--INSERT INTO @Status (Enum_ID) Values (1319)
--INSERT INTO @Status (Enum_ID) Values (1320)
--INSERT INTO @Status (Enum_ID) Values (1321)
--INSERT INTO @Status_Parecer (Enum_ID) Values (1893)
--INSERT INTO @Status_Parecer (Enum_ID) Values (1894)
--INSERT INTO @Status_Parecer (Enum_ID) Values (1895)
  -------------------------------------------------------------------------------

  SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
  SET NOCOUNT ON

  DECLARE @Pre_Recebimento_Tipo_Objeto_ID                                 INT = 507,  -- Peca
          @Preco_Loja_ID                                                  INT = 1,    -- Tucuruvi
          @Tipo_Divergencia_Entrada_Separacao                             INT = 1316, -- Divergência Separação
          @Tipo_Divergencia_Entrada_Conferencia                           INT = 1317, -- Divergência Conferência
          @Tipo_Divergencia_Entrada_Guarda                                INT = 1318, -- Divergência Guarda
          @Status_Pendente                                                INT = 1319, -- Pendente
          @Status_Parcial                                                 INT = 1320, -- Parcial
          @Status_Finalizado                                              INT = 1321, -- Finalizado
          @Status_Parecer_Pendente                                        INT = 1893, -- Parecer Pendente
          @Status_Parecer_Em_Andamento                                    INT = 1894, -- Parecer Em Andamento
          @Status_Parecer_Finalizado                                      INT = 1895, -- Parecer Finalizado
          @Tipo_Parecer_Posicionamento_Fornecedor                         INT = 982,  -- Tipo Posicionamento ao Fornecedor
          @Tipo_Parecer_Interno                                           INT = 983,  -- Tipo Interno
          @Enum_Status_Pre_Recebimento_Volume_Em_Separacao                INT = 736,  -- Pendente Separação
          @Enum_Tipo_Divergencia_Entrada_Conferencia_Abastecimento_ID     INT = 2529,
          @Enum_Tipo_Divergencia_Entrada_Conferencia_Cross_ID             INT = 2530,
          @Enum_Tipo_Divergencia_Entrada_Separacao_Abastecimento_ID       INT = 2531,
          @Enum_Tipo_Divergencia_Entrada_Separacao_Cross_ID               INT = 2532

  -----------SE A LOJA FOR CENTRO DE DITRIBUIÇÃO CONSULTA DIVERGENCIAS DE CONFERENCIA OU SEPARAÇÃO-------------

  IF (SELECT COUNT(1)
    FROM Lojas
    WHERE Lojas_Tipo = 'CD'
    AND Lojas_Id = @Lojas_Origem_ID)
    > 0
  BEGIN
    SELECT
    DISTINCT
      Fabricante_Comprador.Fabricante_ID              AS Fabricante_ID,
      EnumTipo.Enum_Extenso                           AS Tipo,
      EnumStatus.Enum_Extenso                         AS Status,
      vw_Peca.Peca_ID                                 AS Peca_ID,
      vw_Peca.Fabricante_CD                           AS Fabricante_CD,
      vw_Peca.Produto_CD                              AS Produto_CD,
      vw_Peca.Peca_CD                                 AS Peca_CD,
      Produto_DsResum + ' ' + Peca_DSTecnica          AS Peca_DSTecnica,
      Pre_Recebimento_Divergencia_Qtde                AS Pre_Recebimento_Divergencia_Qtde,
      Pre_Recebimento_Divergencia_Qtde * ISNULL(Peca_Preco.Peca_Preco_Custo_Reposicao, 0) 
      AS Custo_Total_Qtde,
      ABS(Pre_Recebimento_Divergencia_Qtde) - (
      SELECT ISNULL(SUM(ABS(Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_Solucao_Qtde)), 0)
      FROM Pre_Recebimento_Divergencia_Solucao
      WHERE Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID)
      AS Qtde_Pendente,
      ABS(ABS(Pre_Recebimento_Divergencia_Qtde) - (
      SELECT ISNULL(SUM(ABS(Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_Solucao_Qtde)), 0)
      FROM Pre_Recebimento_Divergencia_Solucao
      WHERE Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID)
      ) * ISNULL(Peca_Preco.Peca_Preco_Custo_Reposicao, 0) AS Custo_Total_Pendente,
      Pre_Recebimento_Divergencia_Data_Geracao        AS Pre_Recebimento_Divergencia_Data_Geracao,
      Usuario.Usuario_Nome_Completo                   AS Usuario_Geracao,
      ISNULL(Pre_Recebimento_Divergencia.Lojas_Origem_ID, 0) AS Lojas_Origem_ID,
      CASE
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Separacao
        THEN 'Grupo: ' + CONVERT(VARCHAR, Pre_Recebimento_Divergencia.Objeto_Origem_ID)
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Conferencia
        AND Pre_Recebimento_Divergencia_Qtde < 0 
        THEN 'Grupo: ' + CONVERT(VARCHAR, CONVERT(VARCHAR, Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID) + '.') --+ CONVERT(VARCHAR, Pre_Recebimento_Volume_CT_Numero))
        WHEN (Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Conferencia
        AND ISNULL(Pre_Recebimento_Divergencia.Objeto_Origem_ID, 0) <> 0)
        AND Pre_Recebimento_Divergencia_Qtde > 0
        THEN 'Romaneio: ' + CONVERT(VARCHAR, Pre_Recebimento_Divergencia.Objeto_Origem_ID)
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Guarda
        THEN 'Grupo: ' + CONVERT(VARCHAR, CONVERT(VARCHAR, Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID) + '.') -- + CONVERT(VARCHAR, Pre_Recebimento_Volume_CT_Numero))
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Enum_Tipo_Divergencia_Entrada_Conferencia_Abastecimento_ID
        THEN 'Abastecimento: ' + CAST(Pre_Recebimento_Divergencia.Objeto_Origem_ID AS VARCHAR)
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Enum_Tipo_Divergencia_Entrada_Separacao_Abastecimento_ID
        THEN (SELECT TOP 1 'Abastecimento: ' + CAST(Conferencia_CT.Objeto_Origem_ID AS VARCHAR)
          FROM Conferencia_Separacao
          INNER JOIN Conferencia_CT
            ON Conferencia_CT.Conferencia_CT_ID = Conferencia_Separacao.Conferencia_CT_ID
          WHERE Conferencia_Separacao_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID)
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Enum_Tipo_Divergencia_Entrada_Separacao_Cross_ID
        THEN (SELECT TOP 1 'Grupo.Vol.: ' + CAST(Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID AS VARCHAR) + '.' +
            CAST(Pre_Recebimento_Volume_CT.Pre_Recebimento_Volume_CT_Numero AS VARCHAR)
          FROM Conferencia_Separacao
          INNER JOIN Conferencia_CT
            ON Conferencia_CT.Conferencia_CT_ID = Conferencia_Separacao.Conferencia_CT_ID
          INNER JOIN Pre_Recebimento_Volume_CT
            ON Pre_Recebimento_Volume_CT.Pre_Recebimento_Volume_CT_ID = Conferencia_CT.Objeto_Origem_ID
          WHERE Conferencia_Separacao_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID)

        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Enum_Tipo_Divergencia_Entrada_Conferencia_Cross_ID THEN (SELECT TOP 1
            'Grupo.Vol.: ' +
            CAST(Pre_Recebimento_Grupo_ID AS VARCHAR) + '.' +
            CAST(Pre_Recebimento_Volume_CT_Numero AS VARCHAR)
          FROM Pre_Recebimento_Volume_CT
          WHERE Pre_Recebimento_Volume_CT_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID)

        ELSE ''
      END AS Origem,
      ISNULL(Pre_Recebimento_Divergencia.Lojas_Destino_ID, 0) AS Lojas_Destino_ID,
      LojaDestino.Lojas_NM AS Loja_Destino,
      CASE
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Conferencia AND
          Pre_Recebimento_Divergencia_Qtde < 0 AND
          Pre_Recebimento_Divergencia.Enum_Status_ID = @Status_Finalizado AND
          ISNULL(Pre_Recebimento_Divergencia_Solucao.Objeto_Destino_ID, 0) <> 0 THEN 'Grupo: ' + CONVERT(VARCHAR, CONVERT(VARCHAR, ISNULL(volume_destino.Pre_Recebimento_Grupo_ID, '')) + '.') + CONVERT(VARCHAR, ISNULL(volume_destino.Pre_Recebimento_Volume_CT_Numero, ''))
        ELSE ''
      END AS Destino,

      CASE
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID IN (@Enum_Tipo_Divergencia_Entrada_Separacao_Cross_ID, @Enum_Tipo_Divergencia_Entrada_Conferencia_Cross_ID) THEN Enum_Tipo_Pre_Conferencia.Enum_Extenso
        ELSE EnumTipoPre.Enum_Extenso
      END AS Enum_Extenso_Tipo_Pre_Recebimento,
      CASE
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID IN (@Enum_Tipo_Divergencia_Entrada_Separacao_Cross_ID, @Enum_Tipo_Divergencia_Entrada_Conferencia_Cross_ID) THEN Fornecedor_Conferencia.Fornecedor_Nome
        ELSE Fornecedor.Fornecedor_Nome
      END AS Fornecedor_Nome,
      ISNULL(Usuario_Autorizacao_ID, 0) AS Usuario_Autorizacao_ID,
      Usuario_Autorizacao.Usuario_Nome_Completo AS Usuario_Autorizacao_Nome,
      Pre_Recebimento_Divergencia_Data_Autorizacao AS Pre_Recebimento_Divergencia_Data_Autorizacao,
      ISNULL(Pre_Recebimento_Divergencia.Objeto_Origem_ID, 0) AS Objeto_Origem_ID,
      Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID AS Pre_Recebimento_Divergencia_ID,
      CASE
        WHEN vw_Peca.Fabricante_ID IS NOT NULL THEN dbo.fun_Concatena_Comprador_Por_Fabricante(vw_Peca.Fabricante_ID)
        ELSE ''
      END AS Compradores,
      (SELECT
        MIN(Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_Parecer_Data_Proxima_Cobranca)
      FROM Pre_Recebimento_Divergencia_Parecer
      WHERE Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID
      AND Pre_Recebimento_Divergencia_Parecer.Enum_Tipo_ID = @Tipo_Parecer_Posicionamento_Fornecedor
      AND Pre_Recebimento_Divergencia_Parecer.Enum_Status_ID IN (@Status_Parecer_Pendente, @Status_Parecer_Em_Andamento)
      AND Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_Parecer_Data_Proxima_Cobranca IS NOT NULL)
      AS Proxima_Cobranca INTO #Tmp_Divergencia
    FROM Pre_Recebimento_Divergencia
    JOIN vw_Peca
      ON vw_Peca.Peca_ID = Pre_Recebimento_Divergencia.Peca_ID
    LEFT JOIN Peca_Preco
      ON Peca_Preco.Peca_ID = vw_Peca.Peca_ID
      AND Peca_Preco.Loja_ID = @Preco_Loja_ID
    JOIN Enumerado EnumTipo
      ON EnumTipo.Enum_ID = Pre_Recebimento_Divergencia.Enum_Tipo_ID
    JOIN Enumerado EnumStatus
      ON EnumStatus.Enum_ID = Pre_Recebimento_Divergencia.Enum_Status_ID
    LEFT JOIN Pre_Recebimento_CT
      ON Pre_Recebimento_CT.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID
    LEFT JOIN Fornecedor
      ON Pre_Recebimento_CT.Forn_ID = Fornecedor.Forn_ID
    LEFT JOIN Enumerado EnumTipoPre
      ON EnumTipoPre.Enum_ID = Pre_Recebimento_CT.Enum_Tipo_ID
    LEFT JOIN Usuario Usuario_Autorizacao
      ON Pre_Recebimento_Divergencia.Usuario_Autorizacao_ID = Usuario_Autorizacao.Usuario_ID
    LEFT JOIN Pre_Recebimento_Volume_CT
      ON Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID
      AND Pre_Recebimento_Volume_CT.Enum_Status_Volume_ID != @Enum_Status_Pre_Recebimento_Volume_Em_Separacao

    LEFT JOIN Pre_Recebimento_Divergencia_Solucao
      ON Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID
      AND Pre_Recebimento_Divergencia_Solucao.Lojas_ID = Pre_Recebimento_Divergencia.Lojas_Origem_ID
    LEFT JOIN Pre_Recebimento_Volume_CT volume_destino
      ON volume_destino.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID
      AND volume_destino.Lojas_ID = Pre_Recebimento_Divergencia.Lojas_Origem_ID
      AND volume_destino.Pre_Recebimento_Volume_CT_ID = Pre_Recebimento_Divergencia_Solucao.Objeto_Destino_ID

    JOIN Lojas LojaDestino
      ON LojaDestino.Lojas_Id = Pre_Recebimento_Divergencia.Lojas_Destino_ID
    JOIN Usuario
      ON Usuario.Usuario_ID = Pre_Recebimento_Divergencia.Usuario_Geracao_ID
    LEFT JOIN Fabricante_Comprador
      ON Fabricante_Comprador.Fabricante_ID = vw_Peca.Fabricante_ID
      AND Fabricante_Comprador.Fabricante_Comprador_IsAtivo = 1
    LEFT JOIN Usuario Comprador
      ON Comprador.Usuario_ID = Fabricante_Comprador.Usuario_Comprador_ID
    LEFT JOIN Pre_Recebimento_Volume_CT Volume_Conferencia
      ON Pre_Recebimento_Divergencia.Objeto_Origem_ID = Volume_Conferencia.Pre_Recebimento_Volume_CT_ID
    LEFT JOIN Pre_Recebimento_CT Pre_Recebimento_Conferencia
      ON Volume_Conferencia.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Conferencia.Pre_Recebimento_Grupo_ID
    LEFT JOIN Fornecedor Fornecedor_Conferencia
      ON Pre_Recebimento_Conferencia.Forn_ID = Fornecedor_Conferencia.Forn_ID
    LEFT JOIN Enumerado Enum_Tipo_Pre_Conferencia
      ON Pre_Recebimento_Conferencia.Enum_Tipo_ID = Enum_Tipo_Pre_Conferencia.Enum_ID
    WHERE Pre_Recebimento_Divergencia.Lojas_Origem_ID = @Lojas_Origem_ID
    AND (Pre_Recebimento_Divergencia.Enum_Status_ID IN (SELECT DISTINCT
      Enum_ID
    FROM @Status)
    )
    AND (Pre_Recebimento_Divergencia.Enum_Tipo_ID = ISNULL(@Enum_Tipo_ID, Pre_Recebimento_Divergencia.Enum_Tipo_ID))
    AND (Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_Data_Geracao BETWEEN @Date_Inicial AND @Date_Final
    OR @Date_Inicial IS NULL)
    AND (vw_Peca.Fabricante_ID = ISNULL(@Fabricante_ID, vw_Peca.Fabricante_ID))
    AND (vw_Peca.Produto_ID = ISNULL(@Produto_ID, vw_Peca.Produto_ID))
    AND (vw_Peca.Peca_ID = ISNULL(@Peca_ID, vw_Peca.Peca_ID))
    --AND  
    -- (Pre_Recebimento_CT.Enum_Tipo_ID = @Tipo_Pre_Recebimento_ID OR @Tipo_Pre_Recebimento_ID IS NULL)
    AND (
    @Tipo_Pre_Recebimento_ID IS NULL
    OR (Pre_Recebimento_Divergencia.Enum_Tipo_ID IN (@Enum_Tipo_Divergencia_Entrada_Separacao_Cross_ID, @Enum_Tipo_Divergencia_Entrada_Conferencia_Cross_ID)
    AND Pre_Recebimento_Conferencia.Enum_Tipo_ID = @Tipo_Pre_Recebimento_ID)
    OR (Pre_Recebimento_Divergencia.Enum_Tipo_ID NOT IN (@Enum_Tipo_Divergencia_Entrada_Separacao_Cross_ID, @Enum_Tipo_Divergencia_Entrada_Conferencia_Cross_ID)
    AND Pre_Recebimento_CT.Enum_Tipo_ID = @Tipo_Pre_Recebimento_ID)
    )
    AND (Comprador.Usuario_ID = ISNULL(@Usuario_Comprador_ID, Comprador.Usuario_ID))
    AND (EXISTS (SELECT
    DISTINCT
      1
    FROM Pre_Recebimento_Divergencia_Parecer
    WHERE Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID
    AND Pre_Recebimento_Divergencia_Parecer.Enum_Status_ID IN (SELECT
      Enum_ID
    FROM @Status_Parecer)
    AND (Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_Parecer_Data_Proxima_Cobranca BETWEEN @Date_Inicial_Parecer AND @Date_Final_Parecer)
    OR @Date_Inicial_Parecer IS NULL)
    OR (SELECT
      COUNT(*)
    FROM @Status_Parecer)
    = 0
    )

    SELECT
      Fabricante_ID AS Fabricante_ID,
      Tipo AS Tipo,
      Status AS Status,
      Peca_ID AS Peca_ID,
      Fabricante_CD AS Fabricante_CD,
      Produto_CD AS Produto_CD,
      Peca_CD AS Peca_CD,
      Peca_DSTecnica AS Peca_DSTecnica,
      Pre_Recebimento_Divergencia_Qtde AS Pre_Recebimento_Divergencia_Qtde,
      Custo_Total_Qtde AS Custo_Total_Qtde,
      Qtde_Pendente AS Qtde_Pendente,
      Custo_Total_Pendente AS Custo_Total_Pendente,
      Pre_Recebimento_Divergencia_Data_Geracao AS Pre_Recebimento_Divergencia_Data_Geracao,
      Usuario_Geracao AS Usuario_Geracao,
      Lojas_Origem_ID AS Lojas_Origem_ID,
      Origem AS Origem,
      Lojas_Destino_ID AS Lojas_Destino_ID,
      Loja_Destino AS Loja_Destino,
      Destino AS Destino,
      Enum_Extenso_Tipo_Pre_Recebimento AS Enum_Extenso_Tipo_Pre_Recebimento,
      Fornecedor_Nome AS Fornecedor_Nome,
      Usuario_Autorizacao_ID AS Usuario_Autorizacao_ID,
      Usuario_Autorizacao_Nome AS Usuario_Autorizacao_Nome,
      Pre_Recebimento_Divergencia_Data_Autorizacao AS Pre_Recebimento_Divergencia_Data_Autorizacao,
      Objeto_Origem_ID AS Objeto_Origem_ID,
      Pre_Recebimento_Divergencia_ID AS Pre_Recebimento_Divergencia_ID,
      Compradores AS Compradores,
      Proxima_Cobranca AS Proxima_Cobranca
    FROM #Tmp_Divergencia
  END
  ELSE
  BEGIN
    SELECT
    DISTINCT
      Fabricante_Comprador.Fabricante_ID,
      EnumTipo.Enum_Extenso AS Tipo,
      EnumStatus.Enum_Extenso AS Status,
      vw_Peca.Peca_ID AS Peca_ID,
      vw_Peca.Fabricante_CD AS Fabricante_CD,
      vw_Peca.Produto_CD AS Produto_CD,
      vw_Peca.Peca_CD AS Peca_CD,
      Produto_DsResum + ' ' + Peca_DSTecnica AS Peca_DSTecnica,
      Pre_Recebimento_Divergencia_Qtde AS Pre_Recebimento_Divergencia_Qtde,
      Pre_Recebimento_Divergencia_Qtde * ISNULL(Peca_Preco.Peca_Preco_Custo_Reposicao, 0) AS Custo_Total_Qtde,
      ABS(Pre_Recebimento_Divergencia_Qtde) - (SELECT
        ISNULL(SUM(ABS(Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_Solucao_Qtde)), 0)
      FROM Pre_Recebimento_Divergencia_Solucao
      WHERE Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID)
      AS Qtde_Pendente,
      ABS(ABS(Pre_Recebimento_Divergencia_Qtde) - (SELECT
        ISNULL(SUM(ABS(Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_Solucao_Qtde)), 0)
      FROM Pre_Recebimento_Divergencia_Solucao
      WHERE Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID)
      ) * ISNULL(Peca_Preco.Peca_Preco_Custo_Reposicao, 0) AS Custo_Total_Pendente,
      Pre_Recebimento_Divergencia_Data_Geracao AS Pre_Recebimento_Divergencia_Data_Geracao,
      Usuario.Usuario_Nome_Completo AS Usuario_Geracao,
      ISNULL(Pre_Recebimento_Divergencia.Lojas_Origem_ID, 0) AS Lojas_Origem_ID,
      CASE
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Separacao THEN 'Grupo: ' + CONVERT(VARCHAR, Pre_Recebimento_Divergencia.Objeto_Origem_ID)
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Conferencia AND
          Pre_Recebimento_Divergencia_Qtde < 0 THEN 'Grupo: ' + CONVERT(VARCHAR, CONVERT(VARCHAR, Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID) + '.' + CONVERT(VARCHAR, Pre_Recebimento_Volume_CT.Pre_Recebimento_Volume_CT_Numero))
        WHEN (Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Conferencia AND
          ISNULL(Pre_Recebimento_Divergencia.Objeto_Origem_ID, 0) <> 0) AND
          Pre_Recebimento_Divergencia_Qtde > 0 THEN 'Romaneio: ' + CONVERT(VARCHAR, Pre_Recebimento_Divergencia.Objeto_Origem_ID)
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Guarda THEN 'Grupo: ' + CONVERT(VARCHAR, CONVERT(VARCHAR, Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID) + '.' + CONVERT(VARCHAR, Pre_Recebimento_Volume_CT.Pre_Recebimento_Volume_CT_Numero))
        ELSE ''
      END AS Origem,
      ISNULL(Pre_Recebimento_Divergencia.Lojas_Destino_ID, 0) AS Lojas_Destino_ID,
      LojaDestino.Lojas_NM AS Loja_Destino,
      CASE
        WHEN Pre_Recebimento_Divergencia.Enum_Tipo_ID = @Tipo_Divergencia_Entrada_Conferencia AND
          Pre_Recebimento_Divergencia_Qtde < 0 AND
          Pre_Recebimento_Divergencia.Enum_Status_ID = @Status_Finalizado AND
          ISNULL(Pre_Recebimento_Divergencia_Solucao.Objeto_Destino_ID, 0) <> 0 THEN 'Grupo: ' + CONVERT(VARCHAR, CONVERT(VARCHAR, ISNULL(volume_destino.Pre_Recebimento_Grupo_ID, '')) + '.') + CONVERT(VARCHAR, ISNULL(volume_destino.Pre_Recebimento_Volume_CT_Numero, ''))
        ELSE ''
      END AS Destino,

      EnumTipoPre.Enum_Extenso AS Enum_Extenso_Tipo_Pre_Recebimento,
      Fornecedor.Fornecedor_Nome AS Fornecedor_Nome,
      ISNULL(Usuario_Autorizacao_ID, 0) AS Usuario_Autorizacao_ID,
      Usuario_Autorizacao.Usuario_Nome_Completo AS Usuario_Autorizacao_Nome,
      Pre_Recebimento_Divergencia_Data_Autorizacao AS Pre_Recebimento_Divergencia_Data_Autorizacao,
      ISNULL(Pre_Recebimento_Divergencia.Objeto_Origem_ID, 0) AS Objeto_Origem_ID,
      Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID AS Pre_Recebimento_Divergencia_ID,
      CASE
        WHEN vw_Peca.Fabricante_ID IS NOT NULL THEN dbo.fun_Concatena_Comprador_Por_Fabricante(vw_Peca.Fabricante_ID)
        ELSE ''
      END AS Compradores,
      (SELECT
        MIN(Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_Parecer_Data_Proxima_Cobranca)
      FROM Pre_Recebimento_Divergencia_Parecer
      WHERE Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID
      AND Pre_Recebimento_Divergencia_Parecer.Enum_Tipo_ID = @Tipo_Parecer_Posicionamento_Fornecedor
      AND Pre_Recebimento_Divergencia_Parecer.Enum_Status_ID IN (@Status_Parecer_Pendente, @Status_Parecer_Em_Andamento)
      AND Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_Parecer_Data_Proxima_Cobranca IS NOT NULL)
      Proxima_Cobranca
    FROM Pre_Recebimento_Divergencia
    JOIN vw_Peca
      ON vw_Peca.Peca_ID = Pre_Recebimento_Divergencia.Peca_ID
    LEFT JOIN Peca_Preco
      ON Peca_Preco.Peca_ID = vw_Peca.Peca_ID
      AND Peca_Preco.Loja_ID = @Preco_Loja_ID
    JOIN Enumerado EnumTipo
      ON EnumTipo.Enum_ID = Pre_Recebimento_Divergencia.Enum_Tipo_ID
    JOIN Enumerado EnumStatus
      ON EnumStatus.Enum_ID = Pre_Recebimento_Divergencia.Enum_Status_ID
    LEFT JOIN Pre_Recebimento_CT
      ON Pre_Recebimento_CT.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID
    LEFT JOIN Fornecedor
      ON Pre_Recebimento_CT.Forn_ID = Fornecedor.Forn_ID
    LEFT JOIN Enumerado EnumTipoPre
      ON EnumTipoPre.Enum_ID = Pre_Recebimento_CT.Enum_Tipo_ID
    LEFT JOIN Usuario Usuario_Autorizacao
      ON Pre_Recebimento_Divergencia.Usuario_Autorizacao_ID = Usuario_Autorizacao.Usuario_ID
    LEFT JOIN Pre_Recebimento_Volume_CT
      ON Pre_Recebimento_Volume_CT.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID
      AND Pre_Recebimento_Volume_CT.Enum_Status_Volume_ID != @Enum_Status_Pre_Recebimento_Volume_Em_Separacao

    LEFT JOIN Pre_Recebimento_Divergencia_Solucao
      ON Pre_Recebimento_Divergencia_Solucao.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID
      AND Pre_Recebimento_Divergencia_Solucao.Lojas_ID = Pre_Recebimento_Divergencia.Lojas_Origem_ID
    LEFT JOIN Pre_Recebimento_Volume_CT volume_destino
      ON volume_destino.Pre_Recebimento_Grupo_ID = Pre_Recebimento_Divergencia.Objeto_Origem_ID
      AND volume_destino.Lojas_ID = Pre_Recebimento_Divergencia.Lojas_Origem_ID
      AND volume_destino.Pre_Recebimento_Volume_CT_ID = Pre_Recebimento_Divergencia_Solucao.Objeto_Destino_ID

    JOIN Lojas LojaDestino
      ON LojaDestino.Lojas_Id = Pre_Recebimento_Divergencia.Lojas_Destino_ID
    JOIN Usuario
      ON Usuario.Usuario_ID = Pre_Recebimento_Divergencia.Usuario_Geracao_ID
    LEFT JOIN Fabricante_Comprador
      ON Fabricante_Comprador.Fabricante_ID = vw_Peca.Fabricante_ID
      AND Fabricante_Comprador.Fabricante_Comprador_IsAtivo = 1
    LEFT JOIN Usuario Comprador
      ON Comprador.Usuario_ID = Fabricante_Comprador.Usuario_Comprador_ID
    LEFT JOIN Pre_Recebimento_IT
      ON Pre_Recebimento_IT.Pre_Recebimento_CT_ID = Pre_Recebimento_CT.Pre_Recebimento_CT_ID
      AND Pre_Recebimento_IT.Enum_Tipo_Objeto_ID = @Pre_Recebimento_Tipo_Objeto_ID
      AND Pre_Recebimento_IT.Objeto_Origem_ID = Pre_Recebimento_Divergencia.Peca_ID

    WHERE Pre_Recebimento_Divergencia.Lojas_Origem_ID = @Lojas_Origem_ID
    AND (Pre_Recebimento_Divergencia.Enum_Status_ID IN (SELECT DISTINCT
      Enum_ID
    FROM @Status)
    )
    AND (Pre_Recebimento_Divergencia.Enum_Tipo_ID = ISNULL(@Enum_Tipo_ID, Pre_Recebimento_Divergencia.Enum_Tipo_ID))
    AND (Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_Data_Geracao BETWEEN @Date_Inicial AND @Date_Final
    OR @Date_Inicial IS NULL)
    AND (vw_Peca.Fabricante_ID = ISNULL(@Fabricante_ID, vw_Peca.Fabricante_ID))
    AND (vw_Peca.Produto_ID = ISNULL(@Produto_ID, vw_Peca.Produto_ID))
    AND (vw_Peca.Peca_ID = ISNULL(@Peca_ID, vw_Peca.Peca_ID))
    AND (Pre_Recebimento_CT.Enum_Tipo_ID = @Tipo_Pre_Recebimento_ID
    OR @Tipo_Pre_Recebimento_ID IS NULL)
    AND (Comprador.Usuario_ID = ISNULL(@Usuario_Comprador_ID, Comprador.Usuario_ID))
    AND (EXISTS (SELECT
    DISTINCT
      1
    FROM Pre_Recebimento_Divergencia_Parecer
    WHERE Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_ID = Pre_Recebimento_Divergencia.Pre_Recebimento_Divergencia_ID
    AND Pre_Recebimento_Divergencia_Parecer.Enum_Status_ID IN (SELECT
      Enum_ID
    FROM @Status_Parecer)
    AND (Pre_Recebimento_Divergencia_Parecer.Pre_Recebimento_Divergencia_Parecer_Data_Proxima_Cobranca BETWEEN @Date_Inicial_Parecer AND @Date_Final_Parecer)
    OR @Date_Inicial_Parecer IS NULL)
    OR (SELECT
      COUNT(*)
    FROM @Status_Parecer)
    = 0
    )
    ORDER BY vw_Peca.Fabricante_CD,
    vw_Peca.Produto_CD,
    vw_Peca.Peca_CD
  END

  IF OBJECT_ID('tempdb..#Tmp_Divergencia') IS NOT NULL
  BEGIN
    DROP TABLE #Tmp_Divergencia
  END

  SET NOCOUNT OFF