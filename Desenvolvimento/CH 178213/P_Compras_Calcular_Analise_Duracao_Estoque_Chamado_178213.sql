-------------------------------------------------------------------------------
-- <summary>
--   Procedure de calculo da  Análise de duração de estoque NOVO
-- </summary>
-- <history>
--  alfelix  - 28/12/2011 Created
--  wpinheiro  - 25/07/2012 Modified
--   Alterado calculo da Venda periodo
--  wpinheiro  - 30/07/2012 Modified
--   Corrigido calculo Qtde Multipla
--  wpinheiro  - 31/07/2012 Modified
--   Alterado calculo Disponivel transferencias para considerar Minimo
--  wpinheiro  - 22/08/2012 Modified
--   Considerar curva NULL
--  wpinheiro  - 21/01/2013 Modified
--   Considerar apenas peças principais no grupo de alternativo
--  wpinheiro  - 31/01/2013 Modified
--   Correção calculo alternativo
--  wpinheiro  - 28/03/2013 Modified
--   Tratamento para duraçoes do tipo excedente
--  wpinheiro  - 03/04/2013 Modified
--   SUbtrair quantidade transito pres nao conferidos NF
--  wpinheiro  - 12/05/2014 Modified
--   Considerar transito entrada no calculo de dias de estoque
--  wpinheiro  - 01/07/2014 Modified
--   Caso o produto esteja amarrado a agenda, ignorar as peças do
--  mesmo na puxada por fabricante
--  moshiro  - 08/04/2015 Modified
--   Retirada do tratamento fixo para Embalagem Unitária, buscar
--  o embalagem de compra com menor quantidade
--  rnoliveira - 02/02/2015 Modified
--   Atualizar o campo Analise_Cotacao_Peca_Origem_Pendencia_Compra
--   quando a origem for pendencia de compra.
--  bmune  - 21/07/2015 Modified
--   Adicionado a coluna de Peca_Preco_Valor_Fabrica
--  bmune  - 04/09/2015 Modified
--   Feito o tratamento para jogar a quantidade máxima que o produto suporta nas lojas
--  bmune  - 10/09/2015 Modified
--   Tratamento para calcular as informações de compra e excedente de peças do grupo
--   alternativo para jogar todas as informações na peça principal
--  bmune  - 14/09/2015 Modified
--   Tratamento para não calcular a quantidade a comprar qndo não tem venda
--  bmune  - 16/09/2015 Modified
--   Tratamento para zerar as quantidades a serem compradas dos itens de fabricante
--   alternativo, mantendo somente a quantidade no grupo
--  wpinheiro - 22/09/2015 Modified
--   Alterado para nao considerar estoque de segurança, ou seja, sempre comprar o que tive
--  abaixo do maximo
--  wpinheiro - 30/09/2015 Modified
--   Comentado subselect que subtraia transito 747
--  gfpinheiro - 12/07/2016 Modified
--   Alterado para considerar o estoque de peças alternativas do CD
--  mraugusto - 07/06/2017 Modified
--   Se a peça está marcada no cadastro que envia 100% para as lojas devemos ignorar o CD,
--   caso contrário devemos considerar o CD para o cálculo de compra.
--  rnoliveira - 31/07/2017 Modified
--   Acrescentado agrupamento para a coluna #Lojas.Lojas_Tipo 
--  bmune  - 22/09/2017 Modified
--   Tratamento de Peca_Comprar para as peças de fabricante alternativo   
--  wpinheiro - 16/01/2018 Modified
--   COmentando update no Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia para alternativos
--  wpinheiro - 23/02/2018 Modified
--   descomentado update no Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia para alternativos
--  wpinheiro - 08/10/2018 Modified
--   Não considerar mais sto andre como loja nova
--  fmoraes - 10/05/2019 Modified
--   Problema com retorno cálculo (10,2)
-- </history>
-------------------------------------------------------------------------------
ALTER PROCEDURE [dbo].[P_Compras_Calcular_Analise_Duracao_Estoque_Chamado_178213]
  (
  @Id_Analise_Cotacao INT
  )
AS
------------------------------------------------------------------------------
--DECLARE @ID_Analise_Cotacao INT
--SET @ID_Analise_Cotacao = 793208
-------------------------------------------------------------------------------

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SET NOCOUNT ON

DECLARE
  @Enum_Tipo_Movimentacao_Venda            INT = 93,
  @Enum_Tipo_Movimentacao_Entrada_Garantia INT = 78,
  @Enum_Tipo_Duracao                       INT,
  @Enum_Tipo_Duracao_Excedente             INT = 1046,
  @Id_Fabricante                           INT,
  @Id_Produto                              INT,
  @Quantidade_Dias_Consumo                 INT,
  @Quantidade_Dias_Pedido                  INT,
  @Data date,
  @Analise_Cotacao_Dias_Excedentes         INT,
  @Curvas                                  VARCHAR(10),
  @Aux_Curvas                              INT = 1,
  @Acao_Item_Alternativo                   INT = 696,
  @Vezes_Considerar                        INT,
  @Loja_Garantia_Id                        INT = 13,
  @Loja_Nova                               BIT = 0


DECLARE @Lojas_Filtro TABLE (Lojas_Id INT)
DECLARE @Pecas_Fabricante_Alternativo TABLE (Peca_Id INT, Fabricante_Alternativo_Ct_Id INT)
DECLARE @Curvas_Filtro TABLE ( Curva CHAR(1))

CREATE TABLE #Lojas ( Lojas_Id INT, Lojas_Tipo VARCHAR(50))

--==Lojas selecionadas na duraçao de estoque==--
INSERT INTO @Lojas_Filtro (Lojas_Id)

  SELECT Lojas_Id
  FROM Analise_Cotacao_Loja_Parametros
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao

--==Consulta lojas a serem usadas no decorrer do calculo==--
INSERT INTO #Lojas
  SELECT DISTINCT
    Lojas_Id,
    Lojas_Tipo
  FROM Lojas
  WHERE Lojas_Tipo IN ('Loja', 'CD')

  SELECT
    @Vezes_Considerar = Analise_Cotacao_Loja_Parametros_Vezes_Considerar
  FROM Analise_Cotacao_Loja_Parametros
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao

  SELECT
    @Quantidade_Dias_Consumo             = ISNULL(Analise_Cotacao_Dias_Consumo, 0),
    @Quantidade_Dias_Pedido              = ISNULL(Analise_Cotacao_Dias_Pedido, 0),
    @Id_Fabricante                       = ISNULL(Fabricante_Id, 0),
    @Id_Produto                          = ISNULL(Produto_Id, 0),
    @Data                                = DATEADD(DAY, -1, Analise_Cotacao_Data_Cadastro_Analise),
    @Analise_Cotacao_Dias_Excedentes     = ISNULL(Analise_Cotacao_Dias_Excedentes, 0),
    @Curvas                              = Analise_Cotacao_Curva_Analise,
    @Enum_Tipo_Duracao = Enum_Tipo_Id
  FROM Analise_Cotacao
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao

--==Pega curvas selecionadas na tela==--
  WHILE @Aux_Curvas <= LEN(@Curvas)
  BEGIN
      INSERT INTO @Curvas_Filtro (Curva)
      VALUES (SUBSTRING(@Curvas, @Aux_Curvas, 1))
      SET @Aux_Curvas = @Aux_Curvas + 2
  END

--==Apaga dados para recalcular==--
  DELETE FROM Analise_Cotacao_Loja
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
  DELETE FROM Analise_Cotacao_Peca
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao

--==Consulta grupos dos alternativos==--
--==Todas as peças dos grupos de alternativos o qual a peça o fabricante imformado seja o fabricante da peça principal do grupo==--
INSERT INTO @Pecas_Fabricante_Alternativo (
  Peca_Id,
  Fabricante_Alternativo_Ct_Id)

  SELECT DISTINCT
    Fabricante_Alternativo_It.Peca_Id,
    Fabricante_Alternativo_It.Fabricante_Alternativo_Ct_Id
  FROM Fabricante_Alternativo_It
  WHERE Fabricante_Alternativo_It_Ativo = 1
  AND Fabricante_Alternativo_Ct_Id IN (
      SELECT DISTINCT
        Fabricante_Alternativo_It.Fabricante_Alternativo_Ct_Id
      FROM Peca
      INNER JOIN Fabricante_Alternativo_It
        ON Fabricante_Alternativo_It.Peca_Id = Peca.Peca_Id
        AND Fabricante_Alternativo_It.Enum_Acao_Id <> @Acao_Item_Alternativo
        AND Fabricante_Alternativo_It.Fabricante_Alternativo_It_Ativo = 1
        AND Fabricante_Alternativo_It_Peca_Principal = 1
      WHERE (Peca.Fabricante_Id = @Id_Fabricante
      OR @Id_Fabricante = 0)
      AND (Peca.Produto_Id = @Id_Produto
      OR @Id_Produto = 0)
      AND (Peca.Peca_Isativo = 1)
      AND (ISNULL(Peca_Curva_Mista, 'E') IN (
        SELECT Curva FROM @Curvas_Filtro))
  )

--==Deleta as peças que estejam configuradas para serem puxadas por produto, desde que a duração seja diferente de Consulta==--
  IF (ISNULL(@Id_Fabricante, 0) <> 0
      AND ISNULL(@Id_Produto, 0) = 0
      AND @Enum_Tipo_Duracao <> 1045)
  BEGIN
    DELETE FROM @Pecas_Fabricante_Alternativo
    WHERE Peca_Id IN (
      SELECT DISTINCT Peca_Id
      FROM Agenda_Compras
      JOIN Peca
        ON Peca.Produto_Id = Agenda_Compras.Produto_Id
      WHERE Agenda_Compras.Produto_Id <> 0
      AND ISNULL(Agenda_Compras.Fabricante_Id, 0) = 0
      AND Enum_Status_Id <> 977--Concluido
    )
  END


--------------------------------------------------------------------------------------------------
------------------------------Seleciona Informações das Lojas-------------------------------------
---------------------Apenas peças que não estão em grupos de alternativo--------------------------
--------------------------------------------------------------------------------------------------

  SELECT DISTINCT
  @Id_Analise_Cotacao                  AS Analise_Cotacao_Id,
  Vw.Peca_Id                           AS Peca_Id,
  #Lojas.Lojas_Id                      AS Lojas_Id,
  CEILING(ISNULL(Estoque_Calculo_Venda_Media, 0)) * @Vezes_Considerar AS Analise_Cotacao_Loja_Qtde_Venda_Media,
  Estoque_Calculo_Venda_Media AS Analise_Cotacao_Loja_Qtde_Venda_Media_Original,
  Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(
  ISNULL(E.Estoque_Qtde, 0)) - ISNULL((
    SELECT SUM(Estoque_Divergencia_Qtde)
    FROM Estoque_Divergencia AS Ed
    WHERE Usuario_Tratamento_Id IS NULL
    AND Ed.Objeto_Id = Vw.Peca_Id
    AND Ed.Lojas_Id = #Lojas.Lojas_Id), 0) AS Analise_Cotacao_Loja_Qtde_Estoque, ((
      SELECT ISNULL((SUM(
        ISNULL(Estoque_Transito_Qtde_Transito, 0) +
        ISNULL(Estoque_Transito_Qtde_Processamento, 0))), 0)
      FROM Estoque_Transito
      WHERE Estoque_Transito.Peca_Id = Vw.Peca_Id
      AND Estoque_Transito.Loja_Destino_Id = #Lojas.Lojas_Id
      AND (Estoque_Transito_Qtde_Transito > 0
      OR Estoque_Transito_Qtde_Processamento > 0)
      AND (Estoque_Transito.Loja_Origem_Id NOT IN (@Loja_Garantia_Id))
      AND (Estoque_Transito.Loja_Destino_Id NOT IN (@Loja_Garantia_Id))
      AND
      --(Enum_Tipo_Id NOT IN(441,439,440,416,1458,1465)) --Não considerar Separação, pois ainda não abateu do estoque --voltar Santo Andre
      (Enum_Tipo_Id NOT IN (439, 440, 416, 1458, 1465)) --Não considerar Separação, pois ainda não abateu do estoque
  )) AS Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada,

  ((SELECT ISNULL((SUM(ISNULL(
    Estoque_Transito_Qtde_Transito, 0) + ISNULL(
    Estoque_Transito_Qtde_Processamento, 0))), 0)
  FROM Estoque_Transito
  WHERE Estoque_Transito.Peca_Id = Vw.Peca_Id
  AND Estoque_Transito.Loja_Origem_Id = #Lojas.Lojas_Id
  AND Estoque_Transito.Loja_Origem_Id <> Estoque_Transito.Loja_Destino_Id
  AND (Estoque_Transito_Qtde_Transito > 0
  OR Estoque_Transito_Qtde_Processamento > 0)
  AND (Estoque_Transito.Loja_Origem_Id NOT IN (@Loja_Garantia_Id))
  AND (Estoque_Transito.Loja_Destino_Id NOT IN (@Loja_Garantia_Id))
  AND
  --(Enum_Tipo_Id NOT IN(441,439,440,416,1458,1465)) --Não considerar Separação, pois ainda não abateu do estoque --voltar Santo Andre
  (Enum_Tipo_Id NOT IN (439, 440, 416, 1458, 1465)) --Não considerar Separação, pois ainda não abateu do estoque
  )) AS Analise_Cotacao_Loja_Qtde_Estoque_Transito_Saida,
  Estoque_Calculo_Venda_Media_Parametrizado * @Vezes_Considerar AS Analise_Cotacao_Loja_Qtde_Venda_Periodo,
  0 AS Analise_Cotacao_Loja_Qtde_Calculada,
  ISNULL(Estoque_Calculo_Frequencia_Media, 0) AS Analise_Cotacao_Loja_Qtde_Frequencia_Media,
  --ISNULL(Estoque_Calculo_Maximo_Qtde, 0) AS Analise_Cotacao_Loja_Qtde_Estoque_Maximo,
  CASE WHEN ((ISNULL(Peca_Estoque_Totalmente_Lojas, 0) = 1)
  AND Lojas_Tipo = ('CD')) THEN 0
  ELSE ISNULL(Estoque_Calculo_Maximo_Qtde, 0)
  END AS Analise_Cotacao_Loja_Qtde_Estoque_Maximo,

  --CASE WHEN ISNULL(Peca_Estoque_Totalmente_Lojas, 0) = 1
  --THEN ISNULL((
  --  SELECT Estoque_Calculo.Estoque_Calculo_Maximo_Qtde
  --  FROM Estoque_Calculo
  --  WHERE Estoque_Calculo.Peca_ID = vw.Peca_ID
  --  AND Estoque_Calculo.Loja_ID = #Lojas.Lojas_ID
  --  AND Estoque_Calculo.Loja_ID IN (
  --    SELECT Lojas_ID
  --    FROM #Lojas
  --    WHERE Lojas_Tipo IN ('Loja'))
  --), 0)
  --ELSE ISNULL((
  --  SELECT Estoque_Calculo.Estoque_Calculo_Maximo_Qtde
  --  FROM Estoque_Calculo
  --  WHERE Estoque_Calculo.Peca_ID = vw.Peca_ID
  --  AND Estoque_Calculo.Loja_ID = #Lojas.Lojas_ID
  --  AND Estoque_Calculo.Loja_ID IN (
  --    SELECT  Lojas_ID
  --    FROM #Lojas
  --    WHERE Lojas_Tipo IN ('Loja', 'CD'))
  --), 0)
  --END AS Analise_Cotacao_Loja_Qtde_Estoque_Maximo,

  ISNULL(Estoque_Calculo_Seguranca_Qtde, 0) AS Analise_Cotacao_Loja_Qtde_Estoque_Seguranca,
  Dbo.Fun_Retorna_Zero_Para_Valores_Negativos ((
  ISNULL(E.Estoque_Qtde, 0) - 
  Dbo.Fun_Retorna_Maior_Valor(Estoque_Calculo_Maximo_Qtde, Estoque_Calculo_Minimo_Qtde) - ISNULL((
    SELECT SUM(Estoque_Divergencia_Qtde)
    FROM Estoque_Divergencia AS Ed
    WHERE Usuario_Tratamento_Id IS NULL
    AND Ed.Objeto_Id = Vw.Peca_Id
    AND Ed.Lojas_Id = #Lojas.Lojas_Id), 0))
  ) AS Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia,
  NULL AS Fabricante_Alternativo_Ct_Id
  INTO #Analise_Cotacao_Loja
  FROM Peca AS Vw
  FULL OUTER JOIN #Lojas
    ON #Lojas.Lojas_Id = #Lojas.Lojas_Id
  LEFT OUTER JOIN Estoque AS E
    ON (E.Peca_Id = Vw.Peca_Id)
    AND (E.Loja_Id = #Lojas.Lojas_Id)
  LEFT OUTER JOIN Estoque_Calculo
    ON (Estoque_Calculo.Peca_Id = Vw.Peca_Id)
    AND (Estoque_Calculo.Loja_Id = #Lojas.Lojas_Id)
    AND (Estoque_Calculo.Loja_Id IN (
      SELECT Lojas_Id
      FROM @Lojas_Filtro)
    OR Lojas_Tipo = ('CD'))
  LEFT JOIN Fabricante_Alternativo_It
    ON Fabricante_Alternativo_It.Peca_Id = Vw.Peca_Id
    AND Fabricante_Alternativo_It.Enum_Acao_Id <> @Acao_Item_Alternativo
    AND Fabricante_Alternativo_It.Fabricante_Alternativo_It_Ativo = 1
    --AND Fabricante_Alternativo_IT_Peca_Principal = 1
  LEFT JOIN Fabricante_Alternativo_Ct
    ON Fabricante_Alternativo_It.Fabricante_Alternativo_Ct_Id = Fabricante_Alternativo_Ct.Fabricante_Alternativo_Ct_Id
    AND Fabricante_Alternativo_Ct.Fabricante_Alternativo_Ct_Ativo = 1
  WHERE (Vw.Fabricante_Id = @Id_Fabricante
  OR @Id_Fabricante = 0)
  AND (Vw.Produto_Id = @Id_Produto
  OR @Id_Produto = 0)
  AND (Fabricante_Alternativo_It.Fabricante_Alternativo_Ct_Id IS NULL)
  AND (ISNULL(Vw.Peca_Curva_Mista, 'E') IN (
    SELECT Curva
    FROM @Curvas_Filtro))
  AND (Vw.Peca_Isativo = 1)
  GROUP BY
    Vw.Peca_Id,
    #Lojas.Lojas_Id,
    #Lojas.Lojas_Tipo,
    E.Estoque_Qtde,
    Estoque_Calculo_Venda_Media,
    Estoque_Calculo_Minimo_Qtde,
    Estoque_Calculo_Maximo_Qtde,
    Estoque_Calculo_Seguranca_Qtde,
    Estoque_Calculo_Frequencia_Media,
    Fabricante_Alternativo_It.Fabricante_Alternativo_Ct_Id,
    Estoque_Calculo_Venda_Media_Parametrizado,
    Vw.Peca_Estoque_Totalmente_Lojas

--==Deleta as peças que estejam configuradas para serem puxadas por produto, desde que a duração seja diferente de Consulta==--
  IF (ISNULL(@Id_Fabricante, 0) <> 0
    AND ISNULL(@Id_Produto, 0) = 0
    AND @Enum_Tipo_Duracao <> 1045)
  BEGIN
    DELETE FROM #Analise_Cotacao_Loja
    WHERE Peca_Id IN (
      SELECT DISTINCT Peca_Id
      FROM Agenda_Compras
      JOIN Peca
        ON Peca.Produto_Id = Agenda_Compras.Produto_Id
      WHERE Agenda_Compras.Produto_Id <> 0
      AND ISNULL(Agenda_Compras.Fabricante_Id, 0) = 0
      AND Enum_Status_Id <> 977) --Concluido
  END


--------------------------------------------------------------------------------------------------
------------------Seleciona Informações das Lojas dos Fabricantes Alternativos--------------------
--------------------------------------------------------------------------------------------------

  SELECT DISTINCT
    @Id_Analise_Cotacao        AS Analise_Cotacao_Id,
    Vw.Peca_Id                 AS Peca_Id,
    #Lojas.Lojas_Id            AS Lojas_Id,
    CEILING(ISNULL(Estoque_Calculo_Venda_Media, 0)) * @Vezes_Considerar AS Analise_Cotacao_Loja_Qtde_Venda_Media,
    Estoque_Calculo_Venda_Media AS Analise_Cotacao_Loja_Qtde_Venda_Media_Original,
    Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(ISNULL(E.Estoque_Qtde, 0)) - ISNULL((
      SELECT SUM(Estoque_Divergencia_Qtde)
      FROM Estoque_Divergencia AS Ed
      WHERE Usuario_Tratamento_Id IS NULL
      AND Ed.Objeto_Id = Vw.Peca_Id
      AND Ed.Lojas_Id = #Lojas.Lojas_Id), 0) AS Analise_Cotacao_Loja_Qtde_Estoque,
      ((SELECT ISNULL((SUM( ISNULL(
        Estoque_Transito.Estoque_Transito_Qtde_Transito, 0) + ISNULL(
        Estoque_Transito.Estoque_Transito_Qtde_Processamento, 0))), 0)
      FROM Estoque_Transito
      WHERE Estoque_Transito.Peca_Id = Vw.Peca_Id
      AND Estoque_Transito.Loja_Destino_Id = #Lojas.Lojas_Id
      AND (Estoque_Transito_Qtde_Transito > 0
      OR Estoque_Transito_Qtde_Processamento > 0)
      AND (Estoque_Transito.Loja_Origem_Id NOT IN (@Loja_Garantia_Id))
      AND (Estoque_Transito.Loja_Destino_Id NOT IN (@Loja_Garantia_Id))
      AND
      --(Enum_Tipo_Id NOT IN(441,439,440,416,1458,1465)) --Não considerar Separação, pois ainda não abateu do estoque --voltar sto andre
      (Enum_Tipo_Id NOT IN (439, 440, 416, 1458, 1465)) --Não considerar Separação, pois ainda não abateu do estoque
    )) AS Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada,

    ((SELECT ISNULL((SUM( ISNULL(
      Estoque_Transito.Estoque_Transito_Qtde_Transito, 0) + ISNULL(
      Estoque_Transito.Estoque_Transito_Qtde_Processamento, 0))), 0)
    FROM Estoque_Transito
    WHERE Estoque_Transito.Peca_Id = Vw.Peca_Id
    AND Estoque_Transito.Loja_Origem_Id = #Lojas.Lojas_Id
    AND Estoque_Transito.Loja_Origem_Id <> Estoque_Transito.Loja_Destino_Id
    AND (Estoque_Transito_Qtde_Transito > 0
    OR Estoque_Transito_Qtde_Processamento > 0)
    AND (Estoque_Transito.Loja_Origem_Id NOT IN (@Loja_Garantia_Id))
    AND (Estoque_Transito.Loja_Destino_Id NOT IN (@Loja_Garantia_Id))
    AND
    --(Enum_Tipo_Id NOT IN(441,439,440,416,1458,1465)) --Não considerar Separação, pois ainda não abateu do estoque --voltar Santo Andre
    (Enum_Tipo_Id NOT IN (439, 440, 416, 1458, 1465)) --Não considerar Separação, pois ainda não abateu do estoque
    )) AS Analise_Cotacao_Loja_Qtde_Estoque_Transito_Saida,

    Estoque_Calculo_Venda_Media_Parametrizado * @Vezes_Considerar AS Analise_Cotacao_Loja_Qtde_Venda_Periodo,
    0 AS Analise_Cotacao_Loja_Qtde_Calculada,
    ISNULL(Estoque_Calculo.Estoque_Calculo_Frequencia_Media, 0) AS Analise_Cotacao_Loja_Qtde_Frequencia_Media,

    CASE WHEN ((ISNULL(Peca_Estoque_Totalmente_Lojas, 0) = 1)
      AND Lojas_Tipo = ('CD')) THEN 0
    ELSE ISNULL(Estoque_Calculo_Maximo_Qtde, 0)
    END AS Analise_Cotacao_Loja_Qtde_Estoque_Maximo,
    ISNULL(Estoque_Calculo.Estoque_Calculo_Seguranca_Qtde, 0) AS Analise_Cotacao_Loja_Qtde_Estoque_Seguranca,
    Dbo.Fun_Retorna_Zero_Para_Valores_Negativos ((
    ISNULL(E.Estoque_Qtde, 0) - Dbo.Fun_Retorna_Maior_Valor(Estoque_Calculo_Maximo_Qtde, Estoque_Calculo_Minimo_Qtde) - ISNULL((
      SELECT SUM(Estoque_Divergencia_Qtde)
      FROM Estoque_Divergencia AS Ed
      WHERE Usuario_Tratamento_Id IS NULL
      AND Ed.Objeto_Id = Vw.Peca_Id
      AND Ed.Lojas_Id = #Lojas.Lojas_Id), 0))) AS Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia,
    Pecas_Fabricante_Alternativo.Fabricante_Alternativo_Ct_Id AS Fabricante_Alternativo_Ct_Id,
    (SELECT TOP 1 Peca.Peca_Comprar
    FROM Fabricante_Alternativo_IT
    JOIN Peca
      ON Peca.Peca_ID = Fabricante_Alternativo_IT.Peca_ID
    WHERE Fabricante_Alternativo_IT.Fabricante_Alternativo_CT_ID = Pecas_Fabricante_Alternativo.Fabricante_Alternativo_Ct_Id
    AND Fabricante_Alternativo_IT.Fabricante_Alternativo_IT_Ativo = 1
    AND Fabricante_Alternativo_IT.Fabricante_Alternativo_IT_Peca_Principal = 1) AS Peca_Comprar
    INTO #Analise_Cotacao_Loja_Fabricante_Alternativo

  FROM Peca AS Vw
  FULL OUTER JOIN #Lojas
    ON #Lojas.Lojas_Id = #Lojas.Lojas_Id
  LEFT OUTER JOIN Estoque AS E
    ON (E.Loja_Id = #Lojas.Lojas_Id)
    AND (E.Peca_Id = Vw.Peca_Id)
  LEFT OUTER JOIN Estoque_Calculo
    ON (Estoque_Calculo.Peca_Id = Vw.Peca_Id)
    AND (Estoque_Calculo.Loja_Id = #Lojas.Lojas_Id)
    AND (Estoque_Calculo.Loja_Id IN (
      SELECT Lojas_Id FROM @Lojas_Filtro)
      OR Lojas_Tipo = ('CD'))
  JOIN @Pecas_Fabricante_Alternativo AS Pecas_Fabricante_Alternativo
    ON Pecas_Fabricante_Alternativo.Peca_Id = Vw.Peca_Id
  GROUP BY
    Vw.Peca_Id,
    #Lojas.Lojas_Id,
    #Lojas.Lojas_Tipo,
    E.Estoque_Qtde,
    Estoque_Calculo_Venda_Media,
    Estoque_Calculo_Maximo_Qtde,
    Estoque_Calculo_Minimo_Qtde,
    Estoque_Calculo_Seguranca_Qtde,
    Estoque_Calculo_Frequencia_Media,
    Pecas_Fabricante_Alternativo.Fabricante_Alternativo_Ct_Id,
    Estoque_Calculo_Venda_Media_Parametrizado,
    Vw.Peca_Estoque_Totalmente_Lojas


--------------------------------------------------------------------------------------------------
-----------------------------------Insere Informações das Lojas-----------------------------------
--------------------------------------------------------------------------------------------------

INSERT INTO Analise_Cotacao_Loja (
Analise_Cotacao_Id,
Peca_Id,
Lojas_Id,
Analise_Cotacao_Loja_Qtde_Venda_Media,
Analise_Cotacao_Loja_Qtde_Venda_Media_Original,
Analise_Cotacao_Loja_Qtde_Estoque,
Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada,
Analise_Cotacao_Loja_Qtde_Estoque_Transito_Saida,
Analise_Cotacao_Loja_Qtde_Venda_Periodo,
Analise_Cotacao_Loja_Qtde_Calculada,
Analise_Cotacao_Loja_Qtde_Frequencia_Media,
Analise_Cotacao_Loja_Qtde_Estoque_Maximo,
Analise_Cotacao_Loja_Qtde_Estoque_Seguranca,
Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia,
Fabricante_Alternativo_Ct_Id)

  SELECT
    Analise_Cotacao_Id,
    Peca_Id,
    Lojas_Id,
  CASE WHEN Lojas_ID IN (
    SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD') THEN 0
  ELSE Analise_Cotacao_Loja_Qtde_Venda_Media
  END AS Analise_Cotacao_Loja_Qtde_Venda_Media,
  Analise_Cotacao_Loja_Qtde_Venda_Media_Original,
  Analise_Cotacao_Loja_Qtde_Estoque,
  Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada,
  Analise_Cotacao_Loja_Qtde_Estoque_Transito_Saida,
  Analise_Cotacao_Loja_Qtde_Venda_Periodo,
  Analise_Cotacao_Loja_Qtde_Calculada,
  Analise_Cotacao_Loja_Qtde_Frequencia_Media,
  Analise_Cotacao_Loja_Qtde_Estoque_Maximo,
  Analise_Cotacao_Loja_Qtde_Estoque_Seguranca,
  Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia,
  Fabricante_Alternativo_Ct_Id
  FROM #Analise_Cotacao_Loja
  WHERE Peca_Id NOT IN (
    SELECT Peca_Id
    FROM #Analise_Cotacao_Loja_Fabricante_Alternativo)

  UNION

  SELECT
    Analise_Cotacao_Id,
    Peca_Id,
    Lojas_Id,
  CASE WHEN Lojas_ID IN (
    SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD') THEN 0
  ELSE Analise_Cotacao_Loja_Qtde_Venda_Media
  END AS Analise_Cotacao_Loja_Qtde_Venda_Media,
  Analise_Cotacao_Loja_Qtde_Venda_Media_Original,
  Analise_Cotacao_Loja_Qtde_Estoque,
  Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada,
  Analise_Cotacao_Loja_Qtde_Estoque_Transito_Saida,
  Analise_Cotacao_Loja_Qtde_Venda_Periodo,
  Analise_Cotacao_Loja_Qtde_Calculada,
  Analise_Cotacao_Loja_Qtde_Frequencia_Media,
  Analise_Cotacao_Loja_Qtde_Estoque_Maximo,
  Analise_Cotacao_Loja_Qtde_Estoque_Seguranca,
  Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia,
  Fabricante_Alternativo_Ct_Id
  FROM #Analise_Cotacao_Loja_Fabricante_Alternativo


--------------------------------------------------------------------------------------------------
-------------Carrega as informações já somadas das quantidades das peças alternativas-------------
--------------------------------------------------------------------------------------------------

  SELECT
    Fabricante_Alternativo_Ct_Id AS Fabricante_Alternativo_Ct_Id,
    Lojas_Id AS Lojas_Id,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Estoque, 0))                     AS Qtde_Estoque_Total,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada, 0))    AS Qtde_Estoque_Transito,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Estoque_Minimo, 0))              AS Qtde_Estoque_Minimo,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Estoque_Seguranca, 0))           AS Qtde_Estoque_Seguranca,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Estoque_Maximo, 0))              AS Qtde_Estoque_Maximo,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Venda_Media, 0))                 AS Qtde_Venda_Media,
    SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Venda_Periodo, 0))               AS Qtde_Venda_Periodo,
    0                                                                     AS Qtde_Estoque_Cd,
    0                                                                     AS Peca_Principal_Id
  INTO #Dados_Calculados_Peca_Fabricante_Alternativo
  FROM Analise_Cotacao_Loja
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
  AND Fabricante_Alternativo_Ct_Id IS NOT NULL
  GROUP BY Fabricante_Alternativo_Ct_Id, Lojas_Id

UPDATE #Dados_Calculados_Peca_Fabricante_Alternativo
SET

  -- Qtde_Estoque_Cd = ISNULL((
    -- SELECT SUM(ISNULL(
      -- Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque,0) + ISNULL(
      -- Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada,0))
    -- FROM Analise_Cotacao_Loja
    -- WHERE Analise_Cotacao_Loja.Analise_Cotacao_Id = @Id_Analise_Cotacao
    -- AND #Dados_Calculados_Peca_Fabricante_Alternativo.Fabricante_Alternativo_Ct_Id = Analise_Cotacao_Loja.Fabricante_Alternativo_Ct_Id
    -- AND Analise_Cotacao_Loja.Lojas_Id IN (
      -- SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD')
  -- ),0),

  Peca_Principal_Id = (
  SELECT TOP 1 Peca_Id
  FROM Fabricante_Alternativo_It
  WHERE #Dados_Calculados_Peca_Fabricante_Alternativo.Fabricante_Alternativo_Ct_Id = Fabricante_Alternativo_It.Fabricante_Alternativo_Ct_Id
  AND Fabricante_Alternativo_It.Fabricante_Alternativo_It_Ativo = 1
  AND Fabricante_Alternativo_It.Fabricante_Alternativo_It_Peca_Principal = 1)

  -- Qtde_Estoque_Minimo = (
    -- SELECT SUM(ISNULL(Estoque_Calculo.Estoque_Calculo_Minimo_Qtde,0))
    -- FROM Estoque_Calculo
    -- JOIN Analise_Cotacao_Loja
      -- ON Analise_Cotacao_Loja.Lojas_Id = Estoque_Calculo.Loja_Id
      -- AND Analise_Cotacao_Loja.Peca_Id = Estoque_Calculo.Peca_Id
    -- WHERE Analise_Cotacao_Loja.Analise_Cotacao_Id = @Id_Analise_Cotacao
    -- AND Analise_Cotacao_Loja.Fabricante_Alternativo_Ct_Id = #Dados_Calculados_Peca_Fabricante_Alternativo.Fabricante_Alternativo_Ct_Id
    -- AND Analise_Cotacao_Loja.Lojas_Id = #Dados_Calculados_Peca_Fabricante_Alternativo.Lojas_Id
  -- )


--------------------------------------------------------------------------------------------------
------------------Atualiza a Quantidade Calculada, exceto das peças alternativas------------------
--------------------------------------------------------------------------------------------------

UPDATE Analise_Cotacao_Loja

SET Analise_Cotacao_Loja_Qtde_Calculada =
  CASE WHEN Analise_Cotacao_Loja.Fabricante_Alternativo_Ct_Id IS NULL
    THEN Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(
    Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Maximo - (
    Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque + 
    Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))
  ELSE
    CASE WHEN
      (Temp.Peca_Principal_Id = Analise_Cotacao_Loja.Peca_Id
      AND (Temp.Qtde_Venda_Media > 0
      OR Temp.Qtde_Venda_Periodo > 0))
    THEN Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(
      Temp.Qtde_Estoque_Maximo - (
      Temp.Qtde_Estoque_Total + Temp.Qtde_Estoque_Transito))
    ELSE 0
    END
  END,
  Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia =
  CASE WHEN Analise_Cotacao_Loja.Fabricante_Alternativo_Ct_Id IS NOT NULL THEN
    CASE WHEN Analise_Cotacao_Loja.Peca_Id = Temp.Peca_Principal_Id THEN
      Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(Temp.Qtde_Estoque_Total - Temp.Qtde_Estoque_Maximo)
    ELSE 0
    END
  ELSE Analise_Cotacao_Loja_Qtde_Disponivel_Transferencia
  END
FROM Analise_Cotacao_Loja
LEFT JOIN #Dados_Calculados_Peca_Fabricante_Alternativo Temp
  ON Analise_Cotacao_Loja.Fabricante_Alternativo_Ct_Id = Temp.Fabricante_Alternativo_Ct_Id
  AND Analise_Cotacao_Loja.Lojas_Id = Temp.Lojas_Id
WHERE Analise_Cotacao_Loja.Analise_Cotacao_Id = @Id_Analise_Cotacao
AND Analise_Cotacao_Loja.Lojas_ID NOT IN (
  SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD')


--------------------------------------------------------------------------------------------------
--------------------------------Seleciona as informações das peças--------------------------------
--------------------------------------------------------------------------------------------------

SELECT DISTINCT
  @Id_Analise_Cotacao                                 AS Analise_Cotacao_Id,
  Analise_Cotacao_Loja.Peca_Id                        AS Peca_Id,
  ISNULL(Peca.Peca_Curva_Mista, 'E')                  AS Analise_Cotacao_Peca_Curva,
  ISNULL((
    SELECT TOP 1 Peca_Embalagem_Id
    FROM Peca_Embalagem                               AS Pe
    WHERE Pe.Peca_Id = Analise_Cotacao_Loja.Peca_Id
    AND Pe.Peca_Embalagem_Ativo = 1
    AND Pe.Peca_Embalagem_Compra = 1
    ORDER BY Pe.Peca_Embalagem_Quantidade), 0)        AS Peca_Embalagem_Compra_Id,

  CASE WHEN (Peca.Peca_Comprar = 0) THEN 0
  ELSE Dbo.Fun_Retorna_Zero_Para_Valores_Negativos
    ( --Peças sem alternativos
     SUM(Analise_Cotacao_Loja_Qtde_Calculada) - ISNULL((
      SELECT SUM(
      Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque) +
      Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada
      ))
      FROM #Analise_Cotacao_Loja
      WHERE #Analise_Cotacao_Loja.Lojas_Id IN (
        SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD')
      AND @Loja_Nova = 0
      AND #Analise_Cotacao_Loja.Peca_Id = Analise_Cotacao_Loja.Peca_Id
      AND #Analise_Cotacao_Loja.Analise_Cotacao_Id = @Id_Analise_Cotacao), 0))
  END AS Analise_Cotacao_Peca_Qtde_Calculada,
  ISNULL(Peca_Preco_Custo_Reposicao, 0)          AS Analise_Cotacao_Peca_Custo_Reposicao,
  ISNULL(Estoque_Custo_Medio, 0)                 AS Analise_Cotacao_Peca_Custo_Medio,
  ISNULL(Estoque_Custo_Ultimo_Custo, 0)          AS Analise_Cotacao_Peca_Ultimo_Custo,
  ISNULL(Estoque_Custo_Reposicao_Efetivo, 0)     AS Analise_Cotacao_Peca_Custo_Reposicao_Efetivo,
  ISNULL(Estoque_Custo_Medio_Efetivo, 0)         AS Analise_Cotacao_Peca_Custo_Medio_Efetivo,
  ISNULL(Estoque_Custo_Ultimo_Custo_Efetivo, 0)  AS Analise_Cotacao_Peca_Ultimo_Custo_Efetivo,

  CASE WHEN ISNULL(Fabricante_Alternativo_Ct_Id, 0) = 0 THEN FLOOR((
    CASE WHEN SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Venda_Media, 0)) = 0 OR
      (SELECT SUM(
        Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque)) + SUM(
        Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))
      FROM #Analise_Cotacao_Loja
      WHERE #Analise_Cotacao_Loja.Peca_Id = Analise_Cotacao_Loja.Peca_Id) <= 0 THEN 0
    ELSE (SELECT SUM(
      Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque)) + SUM(
      Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))
      FROM #Analise_Cotacao_Loja
      WHERE #Analise_Cotacao_Loja.Peca_Id = Analise_Cotacao_Loja.Peca_Id) / (SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Venda_Media, 0)) / 30)
    END))
  ELSE FLOOR((
    CASE WHEN SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Venda_Media, 0)) = 0 OR
      (SELECT SUM(
        Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja_Fabricante_Alternativo.Analise_Cotacao_Loja_Qtde_Estoque)) + SUM(
        Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja_Fabricante_Alternativo.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))
      FROM #Analise_Cotacao_Loja_Fabricante_Alternativo
      WHERE #Analise_Cotacao_Loja_Fabricante_Alternativo.Peca_Id = Analise_Cotacao_Loja.Peca_Id) <= 0 THEN 0
    ELSE (SELECT SUM(
        Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja_Fabricante_Alternativo.Analise_Cotacao_Loja_Qtde_Estoque)) + SUM(
        Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(#Analise_Cotacao_Loja_Fabricante_Alternativo.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))
      FROM #Analise_Cotacao_Loja_Fabricante_Alternativo
      WHERE #Analise_Cotacao_Loja_Fabricante_Alternativo.Peca_Id = Analise_Cotacao_Loja.Peca_Id) / (SUM(ISNULL(Analise_Cotacao_Loja_Qtde_Venda_Media, 0)) / 30)
    END))
  END AS Analise_Cotacao_Peca_Dias_Estoque_Duracao,

  Fabricante_Alternativo_Ct_Id AS Fabricante_Alternativo_Ct_Id,
  Dbo.Fun_Retorna_Estoque_Garantia(Analise_Cotacao_Loja.Peca_Id) AS Analise_Cotacao_Peca_Garantia_Qtde,
  ISNULL((SELECT SUM(Solicitacao_Garantia_It_Quantidade)
    FROM Solicitacao_Garantia_Ct
    INNER JOIN Solicitacao_Garantia_It
      ON Solicitacao_Garantia_It.Solicitacao_Garantia_Ct_Id = Solicitacao_Garantia_Ct.Solicitacao_Garantia_Ct_Id
      AND Solicitacao_Garantia_It.Lojas_Id = Solicitacao_Garantia_Ct.Lojas_Id
    WHERE Solicitacao_Garantia_Ct.Solicitacao_Garantia_Ct_Data_Criacao BETWEEN DATEADD(DAY, -@Quantidade_Dias_Consumo, GETDATE()) AND GETDATE()
    AND Solicitacao_Garantia_It.Objeto_Origem_Id = Analise_Cotacao_Loja.Peca_Id), 0) AS Analise_Cotacao_Peca_Garantia_Perc,
    CASE WHEN (ISNULL((
      (CASE WHEN (SUM(
        Analise_Cotacao_Loja_Qtde_Venda_Media) / 30) <= 0 OR SUM(
        Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) <= 0 THEN 0
      ELSE ISNULL((SUM(
        Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) / (SUM(
        Analise_Cotacao_Loja_Qtde_Venda_Media) / 30)), 0)
      END) - @Analise_Cotacao_Dias_Excedentes) * ISNULL((SUM(
        Analise_Cotacao_Loja_Qtde_Venda_Media) / 30), 0), 0) > 0) THEN ISNULL((
      (CASE WHEN (SUM(
        Analise_Cotacao_Loja_Qtde_Venda_Media) / 30) <= 0 OR SUM(
        Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) <= 0 THEN 0
      ELSE ISNULL((SUM(
        Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) / (SUM(
        Analise_Cotacao_Loja_Qtde_Venda_Media) / 30)), 0)
      END) - @Analise_Cotacao_Dias_Excedentes) * ISNULL((SUM(
      Analise_Cotacao_Loja_Qtde_Venda_Media) / 30), 0), 0)
    ELSE 0
    END AS Analise_Cotacao_Peca_Excedente_Qtde,

  CASE WHEN (ISNULL((
    (CASE WHEN (SUM(
      Analise_Cotacao_Loja_Qtde_Venda_Media) / 30) <= 0 OR SUM(
      Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) <= 0 THEN 0
    ELSE ISNULL((SUM(
      Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) / (SUM(
      Analise_Cotacao_Loja_Qtde_Venda_Media) / 30)), 0)
    END) - @Analise_Cotacao_Dias_Excedentes) * ISNULL((SUM(
      Analise_Cotacao_Loja_Qtde_Venda_Media) / 30), 0), 0) > 0) THEN ISNULL((
    (CASE WHEN (SUM(Analise_Cotacao_Loja_Qtde_Venda_Media) / 30) <= 0 OR SUM(
      Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) <= 0 THEN 0
    ELSE ISNULL((SUM(
      Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada) / (SUM(
      Analise_Cotacao_Loja_Qtde_Venda_Media) / 30)), 0)
    END) - @Analise_Cotacao_Dias_Excedentes) * ISNULL((SUM(
    Analise_Cotacao_Loja_Qtde_Venda_Media) / 30), 0) * ISNULL(
    Peca_Preco_Custo_Reposicao, 0), 0)
  ELSE 0
  END AS Analise_Cotacao_Peca_Excedente_Valor,
  
  SUM(Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque) +
  Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(Analise_Cotacao_Loja.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada)) AS Estoque_Total,
  SUM(Analise_Cotacao_Loja_Qtde_Venda_Media) AS Qtde_Venda_Media,
  CONVERT(DECIMAL(12, 2), ISNULL(SUM(Analise_Cotacao_Loja_Qtde_Venda_Periodo), 0)) AS Qtde_Venda_Periodo,
  COALESCE(Peca_Preco.Peca_Preco_Valor_Fabrica, Peca_Preco_Custo_Reposicao, 0) AS Analise_Cotacao_Peca_Preco_Fabrica INTO #Analise_Cotacao_Peca
FROM Analise_Cotacao_Loja
INNER JOIN Peca
  ON (Peca.Peca_Id = Analise_Cotacao_Loja.Peca_Id)
LEFT JOIN Estoque_Custo
  ON (Estoque_Custo.Peca_Id = Analise_Cotacao_Loja.Peca_Id)
LEFT JOIN Peca_Preco
  ON (Peca_Preco.Peca_Id = Analise_Cotacao_Loja.Peca_Id)
  AND (Peca_Preco.Loja_Id = 1)
WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
GROUP BY
  Analise_Cotacao_Loja.Peca_Id,
  Peca.Peca_Curva_Mista,
  Peca.Peca_Comprar,
  Peca_Preco_Custo_Reposicao,
  Estoque_Custo_Medio,
  Estoque_Custo_Ultimo_Custo,
  Estoque_Custo_Reposicao_Efetivo,
  Estoque_Custo_Medio_Efetivo,
  Estoque_Custo_Ultimo_Custo_Efetivo,
  Fabricante_Alternativo_Ct_Id,
  Peca_Preco.Peca_Preco_Valor_Fabrica


--------------------------------------------------------------------------------------------------
--------------Seleciona as informações das peças do Grupo de Fabricante Alternativo---------------
--------------------------------------------------------------------------------------------------

SELECT
  @Id_Analise_Cotacao        AS Analise_Cotacao_Id,
  Lojas_Id                   AS Lojas_Id,
  ''                         AS Analise_Cotacao_Peca_Curva,
  0                          AS Peca_Embalagem_Compra_Id,
  Qtde_Calculada =
  CASE
    WHEN Peca_Comprar = 0 THEN 0
    ELSE Dbo.Fun_Retorna_Zero_Para_Valores_Negativos((SUM(
      Analise_Cotacao_Loja_Qtde_Estoque_Maximo - (
      Analise_Cotacao_Loja_Qtde_Estoque + Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))))
  END,
  CONVERT(DECIMAL(10, 2), 0) AS Analise_Cotacao_Peca_Custo_Reposicao,
  CONVERT(DECIMAL(10, 2), 0) AS Analise_Cotacao_Peca_Custo_Medio,
  CONVERT(DECIMAL(10, 2), 0) AS Analise_Cotacao_Peca_Ultimo_Custo,
  CONVERT(DECIMAL(10, 2), 0) AS Analise_Cotacao_Peca_Custo_Reposicao_Efetivo,
  CONVERT(DECIMAL(10, 2), 0) AS Analise_Cotacao_Peca_Custo_Medio_Efetivo,
  CONVERT(DECIMAL(10, 2), 0) AS Analise_Cotacao_Peca_Ultimo_Custo_Efetivo,
  0                          AS Analise_Cotacao_Peca_Dias_Estoque_Duracao,
  Fabricante_Alternativo_Ct_Id AS Fabricante_Alternativo_Ct_Id,
  0                          AS Analise_Cotacao_Peca_Garantia_Qtde,
  0                          AS Analise_Cotacao_Peca_Garantia_Perc,
  0                          AS Analise_Cotacao_Peca_Excedente_Qtde,
  0                          AS Analise_Cotacao_Peca_Excedente_Valor,
  0                          AS Analise_Cotacao_Peca_Preco_Fabrica
  INTO #Analise_Cotacao_Loja_Fabricante_Alternativo_Agrupado
FROM #Analise_Cotacao_Loja_Fabricante_Alternativo
WHERE Fabricante_Alternativo_Ct_Id IS NOT NULL
AND #Analise_Cotacao_Loja_Fabricante_Alternativo.Lojas_Id NOT IN (
  SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD')
GROUP BY
  Fabricante_Alternativo_Ct_Id,
  Lojas_Id,
  Peca_Comprar

SELECT
  @Id_Analise_Cotacao        AS Analise_Cotacao_Id,
  NULL                       AS Peca_Id,
  ''                         AS Analise_Cotacao_Peca_Curva,
  0                          AS Peca_Embalagem_Compra_Id,
  Analise_Cotacao_Peca_Qtde_Calculada = Dbo.Fun_Retorna_Zero_Para_Valores_Negativos
  (
  SUM(Qtde_Calculada) - ISNULL((
    SELECT SUM(
    Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(Subquery.Analise_Cotacao_Loja_Qtde_Estoque) +
    Dbo.Fun_Retorna_Zero_Para_Valores_Negativos(Subquery.Analise_Cotacao_Loja_Qtde_Estoque_Transito_Entrada))
  FROM #Analise_Cotacao_Loja_Fabricante_Alternativo AS Subquery
  WHERE Subquery.Lojas_Id IN (
  SELECT Lojas_ID FROM #Lojas WHERE Lojas_Tipo = 'CD')
  AND @Loja_Nova = 0
  AND Subquery.Fabricante_Alternativo_Ct_Id = #Analise_Cotacao_Loja_Fabricante_Alternativo_Agrupado.Fabricante_Alternativo_Ct_Id
  AND Subquery.Analise_Cotacao_Id = @Id_Analise_Cotacao), 0)),
  CONVERT(DECIMAL(10, 2), 0)                AS Analise_Cotacao_Peca_Custo_Reposicao,
  CONVERT(DECIMAL(10, 2), 0)                AS Analise_Cotacao_Peca_Custo_Medio,
  CONVERT(DECIMAL(10, 2), 0)                AS Analise_Cotacao_Peca_Ultimo_Custo,
  CONVERT(DECIMAL(10, 2), 0)                AS Analise_Cotacao_Peca_Custo_Reposicao_Efetivo,
  CONVERT(DECIMAL(10, 2), 0)                AS Analise_Cotacao_Peca_Custo_Medio_Efetivo,
  CONVERT(DECIMAL(10, 2), 0)                AS Analise_Cotacao_Peca_Ultimo_Custo_Efetivo,
  SUM(Analise_Cotacao_Peca_Dias_Estoque_Duracao) AS Analise_Cotacao_Peca_Dias_Estoque_Duracao,
  Fabricante_Alternativo_Ct_Id              AS Fabricante_Alternativo_Ct_Id,
  SUM(Analise_Cotacao_Peca_Garantia_Qtde)   AS Analise_Cotacao_Peca_Garantia_Qtde,
  SUM(Analise_Cotacao_Peca_Garantia_Perc)   AS Analise_Cotacao_Peca_Garantia_Perc,
  SUM(Analise_Cotacao_Peca_Excedente_Qtde)  AS Analise_Cotacao_Peca_Excedente_Qtde,
  SUM(Analise_Cotacao_Peca_Excedente_Valor) AS Analise_Cotacao_Peca_Excedente_Valor,
  SUM(Analise_Cotacao_Peca_Preco_Fabrica)   AS Analise_Cotacao_Peca_Preco_Fabrica
  INTO #Analise_Cotacao_Peca_Fabricante_Alternativo_Agrupado
FROM #Analise_Cotacao_Loja_Fabricante_Alternativo_Agrupado
WHERE Fabricante_Alternativo_Ct_Id IS NOT NULL
GROUP BY Fabricante_Alternativo_Ct_Id


--------------------------------------------------------------------------------------------------
----------------------------------Insere as informações das peças---------------------------------
--------------------------------------------------------------------------------------------------

INSERT INTO Analise_Cotacao_Peca (
Analise_Cotacao_Id,
Peca_Id,
Analise_Cotacao_Peca_Curva,
Peca_Embalagem_Compra_Id,
Analise_Cotacao_Peca_Qtde_Calculada,
Analise_Cotacao_Peca_Custo_Reposicao,
Analise_Cotacao_Peca_Custo_Medio,
Analise_Cotacao_Peca_Ultimo_Custo,
Analise_Cotacao_Peca_Custo_Reposicao_Efetivo,
Analise_Cotacao_Peca_Custo_Medio_Efetivo,
Analise_Cotacao_Peca_Ultimo_Custo_Efetivo,
Analise_Cotacao_Peca_Dias_Estoque_Duracao,
Fabricante_Alternativo_Ct_Id,
Analise_Cotacao_Peca_Garantia_Qtde,
Analise_Cotacao_Peca_Garantia_Perc,
Analise_Cotacao_Peca_Excedente_Qtde,
Analise_Cotacao_Peca_Excedente_Valor,
Analise_Cotacao_Peca_Preco_Fabrica)

  SELECT
    Analise_Cotacao_Id,
    Peca_Id,
    Analise_Cotacao_Peca_Curva,
    Peca_Embalagem_Compra_Id,
    Analise_Cotacao_Peca_Qtde_Calculada,
    Analise_Cotacao_Peca_Custo_Reposicao,
    Analise_Cotacao_Peca_Custo_Medio,
    Analise_Cotacao_Peca_Ultimo_Custo,
    Analise_Cotacao_Peca_Custo_Reposicao_Efetivo,
    Analise_Cotacao_Peca_Custo_Medio_Efetivo,
    Analise_Cotacao_Peca_Ultimo_Custo_Efetivo,
    Analise_Cotacao_Peca_Dias_Estoque_Duracao,
    Fabricante_Alternativo_Ct_Id,
    Analise_Cotacao_Peca_Garantia_Qtde,
    Analise_Cotacao_Peca_Garantia_Perc,
    Analise_Cotacao_Peca_Excedente_Qtde,
    Analise_Cotacao_Peca_Excedente_Valor,
    Analise_Cotacao_Peca_Preco_Fabrica
  FROM #Analise_Cotacao_Peca

  UNION

  SELECT
    Analise_Cotacao_Id,
    Peca_Id,
    Analise_Cotacao_Peca_Curva,
    Peca_Embalagem_Compra_Id,
    Analise_Cotacao_Peca_Qtde_Calculada,
    Analise_Cotacao_Peca_Custo_Reposicao,
    Analise_Cotacao_Peca_Custo_Medio,
    Analise_Cotacao_Peca_Ultimo_Custo,
    Analise_Cotacao_Peca_Custo_Reposicao_Efetivo,
    Analise_Cotacao_Peca_Custo_Medio_Efetivo,
    Analise_Cotacao_Peca_Ultimo_Custo_Efetivo,
    Analise_Cotacao_Peca_Dias_Estoque_Duracao,
    Fabricante_Alternativo_Ct_Id,
    Analise_Cotacao_Peca_Garantia_Qtde,
    Analise_Cotacao_Peca_Garantia_Perc,
    Analise_Cotacao_Peca_Excedente_Qtde,
    Analise_Cotacao_Peca_Excedente_Valor,
    Analise_Cotacao_Peca_Preco_Fabrica
  FROM #Analise_Cotacao_Peca_Fabricante_Alternativo_Agrupado


--------------------------------------------------------------------------------------------------
-------------------------Aplica a multiplicidade da embalagem de compras--------------------------
--------------------------------------------------------------------------------------------------

--==Não alternativos==--
UPDATE Analise_Cotacao_Peca
SET Analise_Cotacao_Peca_Qtde_Calculada =
    CASE
     WHEN Analise_Cotacao_Peca_Qtde_Calculada <= Peca_Qtde_Multipla_Compra THEN Peca_Qtde_Multipla_Compra
     ELSE CASE
         WHEN (
           Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra) > 0 THEN FLOOR(
           Analise_Cotacao_Peca_Qtde_Calculada + Peca_Qtde_Multipla_Compra - ((
           Analise_Cotacao_Peca_Qtde_Calculada %
           Peca_Qtde_Multipla_Compra)))
         ELSE Analise_Cotacao_Peca_Qtde_Calculada
       END
    END,
    Analise_Cotacao_Peca_Qtde_Confirmada_Analise =
    CASE
      WHEN Analise_Cotacao_Peca_Qtde_Calculada <= Peca_Qtde_Multipla_Compra THEN Peca_Qtde_Multipla_Compra
      ELSE CASE
          WHEN (
            Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra) > 0 THEN FLOOR(
            Analise_Cotacao_Peca_Qtde_Calculada + Peca_Qtde_Multipla_Compra - ((
            Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra)))
          ELSE Analise_Cotacao_Peca_Qtde_Calculada
        END
    END
FROM Analise_Cotacao_Peca
INNER JOIN Peca
  ON Analise_Cotacao_Peca.Peca_Id = Peca.Peca_Id
WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
AND Analise_Cotacao_Peca_Qtde_Calculada > 0
AND Analise_Cotacao_Peca.Peca_Id IS NOT NULL

--==ALternativos==--
UPDATE Analise_Cotacao_Peca
SET Analise_Cotacao_Peca_Qtde_Calculada =
    CASE
     WHEN Analise_Cotacao_Peca_Qtde_Calculada <= Peca_Qtde_Multipla_Compra THEN Peca_Qtde_Multipla_Compra
     ELSE CASE
         WHEN (
           Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra) > 0 THEN FLOOR(
           Analise_Cotacao_Peca_Qtde_Calculada + Peca_Qtde_Multipla_Compra - ((
           Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra)))
         ELSE Analise_Cotacao_Peca_Qtde_Calculada
       END
    END,
    Analise_Cotacao_Peca_Qtde_Confirmada_Analise =
    CASE
      WHEN Analise_Cotacao_Peca_Qtde_Calculada <= Peca_Qtde_Multipla_Compra THEN Peca_Qtde_Multipla_Compra
      ELSE CASE
          WHEN (
            Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra) > 0 THEN FLOOR(
            Analise_Cotacao_Peca_Qtde_Calculada + Peca_Qtde_Multipla_Compra - ((
            Analise_Cotacao_Peca_Qtde_Calculada % Peca_Qtde_Multipla_Compra)))
          ELSE Analise_Cotacao_Peca_Qtde_Calculada
        END
    END
FROM Analise_Cotacao_Peca
INNER JOIN Peca
  ON Peca.Peca_Id = (
  SELECT It.Peca_Id
  FROM Fabricante_Alternativo_It AS It
  WHERE It.Fabricante_Alternativo_Ct_Id = Analise_Cotacao_Peca.Fabricante_Alternativo_Ct_Id
  AND It.Fabricante_Alternativo_It_Peca_Principal = 1
  AND Fabricante_Alternativo_It_Ativo = 1)
WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
AND Analise_Cotacao_Peca_Qtde_Calculada > 0
AND Analise_Cotacao_Peca.Peca_Id IS NULL


--------------------------------------------------------------------------------------------------
-------------------------Deixa as quantidades zeradas no campo dos itens--------------------------
--------------------------------------------------------------------------------------------------

UPDATE Analise_Cotacao_Peca
SET Analise_Cotacao_Peca_Qtde_Calculada = 0,
    Analise_Cotacao_Peca_Qtde_Confirmada_Analise = 0
WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
AND Fabricante_Alternativo_Ct_Id IS NOT NULL
AND ISNULL(Peca_Id, 0) <> 0

IF (@Enum_Tipo_Duracao = @Enum_Tipo_Duracao_Excedente)
BEGIN
  UPDATE Analise_Cotacao_Peca
  SET Analise_Cotacao_Peca_Qtde_Confirmada_Analise = 0
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
  AND Analise_Cotacao_Peca_Qtde_Confirmada_Analise > 0
END

--Quando a analise de duração tiver origem da pendencia de compra os itens devem ser identificados--
DECLARE @Enum_Tipo_Analise_Duracao_Estoque_Pendencias_Id INT = 1047,
        @Enum_Status_Pendencia_Compra_Pendente_Id INT = 1139

IF (
  SELECT COUNT(Analise_Cotacao_Id) AS Qtde
  FROM Analise_Cotacao
  WHERE Analise_Cotacao_Id = @Id_Analise_Cotacao
  AND Enum_Tipo_Id = @Enum_Tipo_Analise_Duracao_Estoque_Pendencias_Id
  ) > 0
BEGIN
  UPDATE Analise_Cotacao_Peca
  SET Analise_Cotacao_Peca_Origem_Pendencia_Compra = 1
  FROM Analise_Cotacao_Peca
  INNER JOIN #Analise_Cotacao_Peca Temp_Analise_Cotacao_Peca
    ON Temp_Analise_Cotacao_Peca.Analise_Cotacao_Id = Analise_Cotacao_Peca.Analise_Cotacao_Id
    AND Temp_Analise_Cotacao_Peca.Peca_Id = Analise_Cotacao_Peca.Peca_Id
  INNER JOIN Pendencia_Compra
    ON Pendencia_Compra.Peca_Id = Temp_Analise_Cotacao_Peca.Peca_Id
    AND Pendencia_Compra.Enum_Status_Id = @Enum_Status_Pendencia_Compra_Pendente_Id
  WHERE Temp_Analise_Cotacao_Peca.Analise_Cotacao_Id = @Id_Analise_Cotacao
END


--------------------------------------------------------------------------------------------------
------------------------------------Exclui tabelas temporárias------------------------------------
--------------------------------------------------------------------------------------------------

DROP TABLE #Analise_Cotacao_Loja
DROP TABLE #Analise_Cotacao_Loja_Fabricante_Alternativo
DROP TABLE #Analise_Cotacao_Peca
DROP TABLE #Analise_Cotacao_Peca_Fabricante_Alternativo_Agrupado
DROP TABLE #Analise_Cotacao_Loja_Fabricante_Alternativo_Agrupado
DROP TABLE #Lojas
DROP TABLE #Dados_Calculados_Peca_Fabricante_Alternativo

SET NOCOUNT OFF