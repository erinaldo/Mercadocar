-------------------------------------------------------------------------------
-- <summary>  
--	Retorna o historico de movimentaçao de uma peça por loja	
-- </summary>  
-- <history>  
--		[wpinheiro]		- 22/11/2011	Modified
--			Alterado para exibir numero da troca e nao da transferencia
--		[kcosta]		- 03/01/2012	Modified
--			Alterado CASE de exibição no origem Troca, para considerar Objeto_Origem_ID
--		[raraki]		- 06/12/2012	Modified
--			Adicionado CASE para exibição de Origem/Destino em caso de Transf. Cross
--		[svcosta]		- 06/06/2013	Modified
--			Alteração no filtro da Data do Ultimo Fechamento. Adicionado os filtos Lojas e Peca_ID.
--		[frribeiro]		- 13/11/2013	Modified
--			Alteração da tabela Estoque_Fechamento pela vw_Estoque_Fechamento
--		[frribeiro]		- 20/08/2014	Modified
--			Removido IF e UNION MCAR_DW
--		[wpinheiro]		- 22/08/2014	Modified
--			ALterado para vw_movimentacao_peca
--		[wpinheiro]		- 08/12/2014	Modified
--			Alterado @DataUltimoFechamento = ISNULL(MAX(Estoque_Fechamento_Data), DATEADD(MONTH, -1, @DataInicial))
--		[bmune]			- 12/05/2015	Modified
--			Adicionado um check indicando os dias que tiveram inventário rotativo
--		[msisiliani]	- 12/05/2015	Modified
--			Inclusa a coluna [Objeto_Origem_Visivel_ID] para não ser apresentada o ID do obejto origem quando for 
--		uma venda ou venda oferta
--		[apacheco]		- 26/06/2017	Modified
--			Remoção do update para tratamento do inventario 
-- </history>  
---------------------------------------------------------------------------

--CREATE PROCEDURE [dbo].[p_Compras_Consultar_Analise_Mercadologica_Estoque_Historico]
--(@Peca_ID     INT, 
-- @Loja_ID     INT, 
-- @DataInicial DATETIME
--)
--AS

---------------------------------------------------------------------------

	DECLARE
	@Peca_ID								INT,
	@Loja_ID								INT,
	@DataInicial							DATETIME;

	SET @Peca_ID							= 193095 
	SET @Loja_ID							= 1
	SET @DataInicial						= '2019-04-24'

---------------------------------------------------------------------------

     SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
     SET NOCOUNT ON;
     
	 DECLARE 
	 @SaidaTransferencia                         INT, 
	 @EntradaTransferencia                       INT, 
	 @SaidaEC                                    INT, 
	 @EntradaEC                                  INT, 
	 @Entrada_Garantia                           INT, 
	 @Entrada_Cross                              INT, 
	 @Saida_Cross                                INT, 
	 @Enum_Tipo_Inventario_Rotativo              INT, 
	 @Enum_Tipo_Movimentacao_Venda               INT, 
	 @Enum_Tipo_Movimentacao_Troca               INT, 
	 @Enum_Tipo_Movimentacao_Venda_Oferta        INT;

     SET @SaidaTransferencia                     = 87;
     SET @EntradaTransferencia                   = 77;
     SET @SaidaEC                                = 85;
     SET @EntradaEC                              = 75;
     SET @Entrada_Garantia                       = 78;
     SET @Entrada_Cross                          = 76;
     SET @Saida_Cross                            = 86;
     SET @Enum_Tipo_Inventario_Rotativo          = 1648;
     SET @Enum_Tipo_Movimentacao_Troca           = 83;
     SET @Enum_Tipo_Movimentacao_Venda           = 93;
     SET @Enum_Tipo_Movimentacao_Venda_Oferta    = 1311;

---------------------------------------------------------------------------
     /*** Pegar a data do último fechamento de estoque. ***/
---------------------------------------------------------------------------

     DECLARE @DataUltimoFechamento AS DATETIME;
     SELECT @DataUltimoFechamento = ISNULL(MAX(Estoque_Fechamento_Data), DATEADD(MONTH, -1, @DataInicial))
     FROM --Estoque_Fechamento
     vw_Estoque_Fechamento
     WHERE Estoque_Fechamento_Data < @DataInicial
           AND Loja_ID = @Loja_ID
           AND Peca_ID = @Peca_ID;

     --select @DataUltimoFechamento	

     DECLARE @tempMovimentacao TABLE
     (ID                      INT IDENTITY(1, 1), 
      Peca_ID                 INT, 
      Loja_ID                 INT, 
      Movimentacao_Peca_CT_ID INT, 
      Data_Movimento          DATETIME, 
      EstoqueInicial          INT, 
      Quantidade_Entrada      INT, 
      Quantidade_Saida        INT, 
      EstoqueFinal            INT, 
      Inventario_Rotativo     BIT
     );
     IF OBJECT_ID('TEMPDB..#Temp') IS NOT NULL
         BEGIN
             DROP TABLE #Temp;
     END;
     CREATE TABLE #Temp
     (Peca_ID                   INT, 
      Loja_ID                   INT, 
      Movimentacao_Peca_CT_ID   INT, 
      Movimentacao_Peca_CT_Data DATETIME, 
      Estoque_Inicial           INT, 
      Quantidade_Entrada        INT, 
      Quantidade_Saida          INT, 
      Estoque_Final             INT, 
      Inventario_Rotativo       INT
     );
     INSERT INTO #Temp
            SELECT Peca_ID AS Peca_ID, 
                   Loja_ID AS Loja_ID, 
                   Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
                   Movimentacao_Peca_CT_Data AS Movimentacao_Peca_CT_Data, 
                   CAST(0 AS INT) AS Estoque_Inicial, 
                   (CASE
                        WHEN Enum_Tipo_Movimentacao_ID IN(SELECT Enum_ID
                                                          FROM Enumerado
                                                          WHERE Enum_Nome = 'TipoMovimentacao'
                                                                AND Enum_Sigla = 'Entrada')
                        THEN Movimentacao_Peca_IT_Quantidade
                        ELSE 0
                    END) AS Quantidade_Entrada, 
                   (CASE
                        WHEN Enum_Tipo_Movimentacao_ID IN(SELECT Enum_ID
                                                          FROM Enumerado
                                                          WHERE Enum_Nome = 'TipoMovimentacao'
                                                                AND Enum_Sigla = 'Saida')
                        THEN Movimentacao_Peca_IT_Quantidade
                        ELSE 0
                    END) AS Quantidade_Saida, 
                   CAST(0 AS INT) AS Estoque_Final, 
                   (CASE
                        WHEN Enum_Tipo_Movimentacao_ID IN(SELECT Enum_ID
                                                          FROM Enumerado
                                                          WHERE Enum_Extenso IN('Inventario - Negativo', 'Inventario - Positivo'))
                        THEN 1
                        ELSE 0
                    END) AS Inventario_Rotativo
            FROM vw_Movimentacao_Peca
            WHERE Peca_ID = @Peca_ID
                  AND Loja_ID = @Loja_ID
                  AND Movimentacao_Peca_CT_Data > @DataUltimoFechamento
            ORDER BY Movimentacao_Peca_CT_Data;
     INSERT INTO @TempMovimentacao
            SELECT Peca_ID AS Peca_ID, 
                   Loja_ID AS Loja_ID, 
                   Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
                   Movimentacao_Peca_CT_Data AS Movimentacao_Peca_CT_Data, 
                   Estoque_Inicial AS Estoque_Inicial, 
                   SUM(Quantidade_Entrada), 
                   SUM(Quantidade_Saida), 
                   Estoque_Final AS Estoque_Final, 
                   MAX(Inventario_Rotativo) AS Inventario_Rotativo
            FROM #Temp
            GROUP BY Peca_ID, 
                     Loja_ID, 
                     Movimentacao_Peca_CT_ID, 
                     Movimentacao_Peca_CT_Data, 
                     Estoque_Inicial, 
                     Estoque_Final
            ORDER BY Movimentacao_Peca_CT_Data;

     --select * from @TempMovimentacao
     DECLARE @COD INT;
     DECLARE @MAX INT;
     SET @COD = 1;
     SELECT @MAX = COUNT(*)
     FROM @TempMovimentacao;
     UPDATE @TempMovimentacao
       SET 
           EstoqueInicial = ISNULL(Estoque_Fechamento_Quantidade, 0), 
           EstoqueFinal = ISNULL(Estoque_Fechamento_Quantidade, 0) + Quantidade_Entrada - Quantidade_Saida
     FROM @TempMovimentacao TempMovimentacao
          LEFT JOIN vw_Estoque_Fechamento Estoque_Fechamento ON Estoque_Fechamento.Peca_ID = TempMovimentacao.Peca_ID
                                                                AND Estoque_Fechamento.Loja_ID = TempMovimentacao.Loja_ID
                                                                AND Estoque_Fechamento.Estoque_Fechamento_Data = @DataUltimoFechamento
     WHERE TempMovimentacao.ID = 1;

     --UPDATE
     --	MovDia
     --SET
     --	Inventario_Rotativo = 1
     --FROM
     --	@TempMovimentacao MovDia
     --	JOIN Inventario_CT ON
     --		MovDia.Loja_ID = Inventario_CT.Lojas_ID
     --	AND
     --		MovDia.Data_Movimento BETWEEN CONVERT(DATE, Inventario_CT.Inventario_CT_Data_Inicio) AND CONVERT(DATE, Inventario_CT.Inventario_CT_Data_Finalizacao)
     --	JOIN Inventario_IT ON
     --		Inventario_CT.Inventario_CT_ID = Inventario_IT.Inventario_CT_ID
     --	AND
     --		MovDia.Peca_ID = Inventario_IT.Peca_ID
     --WHERE	
     --	Inventario_CT.Enum_Tipo_ID = @Enum_Tipo_Inventario_Rotativo
     --	AND
     --	Inventario_IT.Inventario_IT_Qtde_Estoque_Ajustada <> 0

     WHILE @COD <= @MAX
         BEGIN
             UPDATE MovDia
               SET 
                   EstoqueInicial = MovDiaAnterior.EstoqueFinal, 
                   EstoqueFinal = MovDiaAnterior.EstoqueFinal + MovDia.Quantidade_Entrada - MovDia.Quantidade_Saida
             FROM @TempMovimentacao MovDia
                  INNER JOIN @TempMovimentacao MovDiaAnterior ON MovDiaAnterior.ID = @COD - 1
             WHERE MovDia.ID = @COD;
             SET @COD = @COD + 1;
         END;
     IF DATEADD(MM, 6, @DataInicial) >= GETDATE()
         BEGIN
             SELECT TempMovimentacao.Peca_ID AS Peca_ID, 
                    TempMovimentacao.Loja_ID AS Loja_ID, 
                    TempMovimentacao.Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
                    TempMovimentacao.Data_Movimento AS Data_Movimento, 
                    TempMovimentacao.EstoqueInicial AS EstoqueInicial, 
                    TempMovimentacao.Quantidade_Entrada AS Quantidade_Entrada, 
                    TempMovimentacao.Quantidade_Saida AS Quantidade_Saida, 
                    TempMovimentacao.EstoqueFinal AS EstoqueFinal, 
                    ISNULL(SUM(ISNULL(Abastecimento_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Abastecimento_Entrada, 
                    ISNULL(SUM(ISNULL(Distribuicao_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Distribuicao_Entrada, 
                    ISNULL(SUM(ISNULL(Transferencia_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Transferencia_Entrada, 
                    ISNULL(SUM(ISNULL(Garantia_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Garantia_Entrada, 
                    ISNULL(SUM(ISNULL(Devolucao_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Devolucao_Entrada, 
                    ISNULL(SUM(ISNULL(Ajuste_Estoque_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Ajuste_Estoque_Entrada, 
                    ISNULL(SUM(ISNULL(Retorno_Garantia_Fornecedor_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Retorno_Garantia_Fornecedor_Entrada, 
                    ISNULL(SUM(ISNULL(Compra_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Compra_Entrada, 
                    ISNULL(SUM(ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Troca_Entrada, 
                    ISNULL(SUM(ISNULL(Encomenda_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Encomenda_Entrada, 
                    ISNULL(SUM(ISNULL(Abastecimento_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Abastecimento_Saida, 
                    ISNULL(SUM(ISNULL(Distribuicao_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Distribuicao_Saida, 
                    ISNULL(SUM(ISNULL(Transferencia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Transferencia_Saida, 
                    ISNULL(SUM(ISNULL(Garantia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Garantia_Saida, 
                    ISNULL(SUM(ISNULL(Devolucao_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Devolucao_Saida, 
                    ISNULL(SUM(ISNULL(Ajuste_Estoque_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Ajuste_Estoque_Saida, 
                    ISNULL(SUM(ISNULL(Envio_Garantia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Envio_Garantia_Saida, 
                    ISNULL(SUM(ISNULL(Prejuizo_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Prejuizo_Saida, 
                    ISNULL(SUM(ISNULL(Venda_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Venda_Saida, 
                    ISNULL(SUM(ISNULL(Governo_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Governo_Saida, 
                    ISNULL(SUM(ISNULL(Encomenda_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Encomenda_Saida, 
                    SUM((ISNULL(Venda_Saida.Movimentacao_Peca_IT_Quantidade, 0) * ISNULL(Venda_Saida.Movimentacao_Peca_IT_Valor_Peca_Preco, 0)) - (ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Quantidade, 0) * ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Valor_Peca_Preco, 0))) AS Valor_Movimentado, 
                    TempMovimentacao.Inventario_Rotativo AS Inventario_Rotativo
             FROM Movimentacao_Peca_IT
                  INNER JOIN Movimentacao_Peca_CT ON Movimentacao_Peca_CT.Movimentacao_Peca_CT_ID = Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID
                  INNER JOIN @TempMovimentacao TempMovimentacao ON TempMovimentacao.Data_Movimento = Movimentacao_Peca_CT.Movimentacao_Peca_CT_Data
                  -- Abastecimento_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Abastecimento_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Abastecimento_Entrada.Movimentacao_Peca_IT_ID
                                                                                AND Abastecimento_Entrada.Enum_Tipo_Movimentacao_ID = 75
                  -- Distribuicao_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Distribuicao_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Distribuicao_Entrada.Movimentacao_Peca_IT_ID
                                                                               AND Distribuicao_Entrada.Enum_Tipo_Movimentacao_ID = 76
                  -- Transferencia_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Transferencia_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Transferencia_Entrada.Movimentacao_Peca_IT_ID
                                                                                AND Transferencia_Entrada.Enum_Tipo_Movimentacao_ID = 77
                  -- Garantia_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Garantia_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Garantia_Entrada.Movimentacao_Peca_IT_ID
                                                                           AND Garantia_Entrada.Enum_Tipo_Movimentacao_ID = 78
                  -- Devolucao_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Devolucao_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Devolucao_Entrada.Movimentacao_Peca_IT_ID
                                                                            AND Devolucao_Entrada.Enum_Tipo_Movimentacao_ID = 79
                  -- Ajuste_Estoque_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Ajuste_Estoque_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Ajuste_Estoque_Entrada.Movimentacao_Peca_IT_ID
                                                                                 AND Ajuste_Estoque_Entrada.Enum_Tipo_Movimentacao_ID = 80
                  -- Retorno_Garantia_Fornecedor_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Retorno_Garantia_Fornecedor_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Retorno_Garantia_Fornecedor_Entrada.Movimentacao_Peca_IT_ID
                                                                                              AND Retorno_Garantia_Fornecedor_Entrada.Enum_Tipo_Movimentacao_ID = 81
                  -- Compra_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Compra_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Compra_Entrada.Movimentacao_Peca_IT_ID
                                                                         AND Compra_Entrada.Enum_Tipo_Movimentacao_ID = 82
                  -- Troca_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Troca_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Troca_Entrada.Movimentacao_Peca_IT_ID
                                                                        AND Troca_Entrada.Enum_Tipo_Movimentacao_ID = 83
                  -- Encomenda_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Encomenda_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Encomenda_Entrada.Movimentacao_Peca_IT_ID
                                                                            AND Encomenda_Entrada.Enum_Tipo_Movimentacao_ID = 84
                  -- Abastecimento_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Abastecimento_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Abastecimento_Saida.Movimentacao_Peca_IT_ID
                                                                              AND Abastecimento_Saida.Enum_Tipo_Movimentacao_ID = 85
                  -- Distribuicao_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Distribuicao_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Distribuicao_Saida.Movimentacao_Peca_IT_ID
                                                                             AND Distribuicao_Saida.Enum_Tipo_Movimentacao_ID = 86
                  -- Transferencia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Transferencia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Transferencia_Saida.Movimentacao_Peca_IT_ID
                                                                              AND Transferencia_Saida.Enum_Tipo_Movimentacao_ID = 87
                  -- Garantia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Garantia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Garantia_Saida.Movimentacao_Peca_IT_ID
                                                                         AND Garantia_Saida.Enum_Tipo_Movimentacao_ID = 88
                  -- Devolucao_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Devolucao_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Devolucao_Saida.Movimentacao_Peca_IT_ID
                                                                          AND Devolucao_Saida.Enum_Tipo_Movimentacao_ID = 89
                  -- Ajuste_Estoque_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Ajuste_Estoque_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Ajuste_Estoque_Saida.Movimentacao_Peca_IT_ID
                                                                               AND Ajuste_Estoque_Saida.Enum_Tipo_Movimentacao_ID = 90
                  -- Envio_Garantia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Envio_Garantia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Envio_Garantia_Saida.Movimentacao_Peca_IT_ID
                                                                               AND Envio_Garantia_Saida.Enum_Tipo_Movimentacao_ID = 91
                  -- Prejuizo_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Prejuizo_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Prejuizo_Saida.Movimentacao_Peca_IT_ID
                                                                         AND Prejuizo_Saida.Enum_Tipo_Movimentacao_ID = 92
                  -- Venda_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Venda_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Venda_Saida.Movimentacao_Peca_IT_ID
                                                                      AND Venda_Saida.Enum_Tipo_Movimentacao_ID = 93
                  -- Governo_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Governo_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Governo_Saida.Movimentacao_Peca_IT_ID
                                                                        AND Governo_Saida.Enum_Tipo_Movimentacao_ID = 94
                  -- Encomenda_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Encomenda_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Encomenda_Saida.Movimentacao_Peca_IT_ID
                                                                          AND Encomenda_Saida.Enum_Tipo_Movimentacao_ID = 95
             WHERE Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID IN
             (
                 SELECT Movimentacao_Peca_CT_ID
                 FROM @TempMovimentacao
             )
                   AND Movimentacao_Peca_CT.Movimentacao_Peca_CT_Data >= @DataInicial
             GROUP BY TempMovimentacao.Peca_ID, 
                      TempMovimentacao.Loja_ID, 
                      TempMovimentacao.Movimentacao_Peca_CT_ID, 
                      TempMovimentacao.Data_Movimento, 
                      TempMovimentacao.EstoqueInicial, 
                      TempMovimentacao.Quantidade_Entrada, 
                      TempMovimentacao.Quantidade_Saida, 
                      TempMovimentacao.EstoqueFinal, 
                      TempMovimentacao.Inventario_Rotativo
             ORDER BY TempMovimentacao.Data_Movimento DESC;
     END;
         ELSE
         BEGIN
             SELECT TempMovimentacao.Peca_ID AS Peca_ID, 
                    TempMovimentacao.Loja_ID AS Loja_ID, 
                    TempMovimentacao.Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
                    TempMovimentacao.Data_Movimento AS Data_Movimento, 
                    TempMovimentacao.EstoqueInicial AS EstoqueInicial, 
                    TempMovimentacao.Quantidade_Entrada AS Quantidade_Entrada, 
                    TempMovimentacao.Quantidade_Saida AS Quantidade_Saida, 
                    TempMovimentacao.EstoqueFinal AS EstoqueFinal, 
                    ISNULL(SUM(ISNULL(Abastecimento_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Abastecimento_Entrada, 
                    ISNULL(SUM(ISNULL(Distribuicao_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Distribuicao_Entrada, 
                    ISNULL(SUM(ISNULL(Transferencia_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Transferencia_Entrada, 
                    ISNULL(SUM(ISNULL(Garantia_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Garantia_Entrada, 
                    ISNULL(SUM(ISNULL(Devolucao_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Devolucao_Entrada, 
                    ISNULL(SUM(ISNULL(Ajuste_Estoque_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Ajuste_Estoque_Entrada, 
                    ISNULL(SUM(ISNULL(Retorno_Garantia_Fornecedor_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Retorno_Garantia_Fornecedor_Entrada, 
                    ISNULL(SUM(ISNULL(Compra_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Compra_Entrada, 
                    ISNULL(SUM(ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Troca_Entrada, 
                    ISNULL(SUM(ISNULL(Encomenda_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Encomenda_Entrada, 
                    ISNULL(SUM(ISNULL(Abastecimento_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Abastecimento_Saida, 
                    ISNULL(SUM(ISNULL(Distribuicao_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Distribuicao_Saida, 
                    ISNULL(SUM(ISNULL(Transferencia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Transferencia_Saida, 
                    ISNULL(SUM(ISNULL(Garantia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Garantia_Saida, 
                    ISNULL(SUM(ISNULL(Devolucao_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Devolucao_Saida, 
                    ISNULL(SUM(ISNULL(Ajuste_Estoque_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Ajuste_Estoque_Saida, 
                    ISNULL(SUM(ISNULL(Envio_Garantia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Envio_Garantia_Saida, 
                    ISNULL(SUM(ISNULL(Prejuizo_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Prejuizo_Saida, 
                    ISNULL(SUM(ISNULL(Venda_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Venda_Saida, 
                    ISNULL(SUM(ISNULL(Governo_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Governo_Saida, 
                    ISNULL(SUM(ISNULL(Encomenda_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Encomenda_Saida, 
                    SUM((ISNULL(Venda_Saida.Movimentacao_Peca_IT_Quantidade, 0) * ISNULL(Venda_Saida.Movimentacao_Peca_IT_Valor_Peca_Preco, 0)) - (ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Quantidade, 0) * ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Valor_Peca_Preco, 0))) AS Valor_Movimentado, 
                    TempMovimentacao.Inventario_Rotativo AS Inventario_Rotativo
             FROM Movimentacao_Peca_IT
                  INNER JOIN Movimentacao_Peca_CT ON Movimentacao_Peca_CT.Movimentacao_Peca_CT_ID = Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID
                  INNER JOIN @TempMovimentacao TempMovimentacao ON TempMovimentacao.Data_Movimento = Movimentacao_Peca_CT.Movimentacao_Peca_CT_Data
                  -- Abastecimento_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Abastecimento_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Abastecimento_Entrada.Movimentacao_Peca_IT_ID
                                                                                AND Abastecimento_Entrada.Enum_Tipo_Movimentacao_ID = 75
                  -- Distribuicao_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Distribuicao_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Distribuicao_Entrada.Movimentacao_Peca_IT_ID
                                                                               AND Distribuicao_Entrada.Enum_Tipo_Movimentacao_ID = 76
                  -- Transferencia_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Transferencia_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Transferencia_Entrada.Movimentacao_Peca_IT_ID
                                                                                AND Transferencia_Entrada.Enum_Tipo_Movimentacao_ID = 77
                  -- Garantia_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Garantia_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Garantia_Entrada.Movimentacao_Peca_IT_ID
                                                                           AND Garantia_Entrada.Enum_Tipo_Movimentacao_ID = 78
                  -- Devolucao_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Devolucao_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Devolucao_Entrada.Movimentacao_Peca_IT_ID
                                                                            AND Devolucao_Entrada.Enum_Tipo_Movimentacao_ID = 79
                  -- Ajuste_Estoque_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Ajuste_Estoque_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Ajuste_Estoque_Entrada.Movimentacao_Peca_IT_ID
                                                                                 AND Ajuste_Estoque_Entrada.Enum_Tipo_Movimentacao_ID = 80
                  -- Retorno_Garantia_Fornecedor_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Retorno_Garantia_Fornecedor_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Retorno_Garantia_Fornecedor_Entrada.Movimentacao_Peca_IT_ID
                                                                                              AND Retorno_Garantia_Fornecedor_Entrada.Enum_Tipo_Movimentacao_ID = 81
                  -- Compra_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Compra_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Compra_Entrada.Movimentacao_Peca_IT_ID
                                                                         AND Compra_Entrada.Enum_Tipo_Movimentacao_ID = 82
                  -- Troca_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Troca_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Troca_Entrada.Movimentacao_Peca_IT_ID
                                                                        AND Troca_Entrada.Enum_Tipo_Movimentacao_ID = 83
                  -- Encomenda_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Encomenda_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Encomenda_Entrada.Movimentacao_Peca_IT_ID
                                                                            AND Encomenda_Entrada.Enum_Tipo_Movimentacao_ID = 84
                  -- Abastecimento_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Abastecimento_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Abastecimento_Saida.Movimentacao_Peca_IT_ID
                                                                              AND Abastecimento_Saida.Enum_Tipo_Movimentacao_ID = 85
                  -- Distribuicao_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Distribuicao_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Distribuicao_Saida.Movimentacao_Peca_IT_ID
                                                                             AND Distribuicao_Saida.Enum_Tipo_Movimentacao_ID = 86
                  -- Transferencia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Transferencia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Transferencia_Saida.Movimentacao_Peca_IT_ID
                                                                              AND Transferencia_Saida.Enum_Tipo_Movimentacao_ID = 87
                  -- Garantia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Garantia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Garantia_Saida.Movimentacao_Peca_IT_ID
                                                                         AND Garantia_Saida.Enum_Tipo_Movimentacao_ID = 88
                  -- Devolucao_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Devolucao_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Devolucao_Saida.Movimentacao_Peca_IT_ID
                                                                          AND Devolucao_Saida.Enum_Tipo_Movimentacao_ID = 89
                  -- Ajuste_Estoque_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Ajuste_Estoque_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Ajuste_Estoque_Saida.Movimentacao_Peca_IT_ID
                                                                               AND Ajuste_Estoque_Saida.Enum_Tipo_Movimentacao_ID = 90
                  -- Envio_Garantia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Envio_Garantia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Envio_Garantia_Saida.Movimentacao_Peca_IT_ID
                                                                               AND Envio_Garantia_Saida.Enum_Tipo_Movimentacao_ID = 91
                  -- Prejuizo_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Prejuizo_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Prejuizo_Saida.Movimentacao_Peca_IT_ID
                                                                         AND Prejuizo_Saida.Enum_Tipo_Movimentacao_ID = 92
                  -- Venda_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Venda_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Venda_Saida.Movimentacao_Peca_IT_ID
                                                                      AND Venda_Saida.Enum_Tipo_Movimentacao_ID = 93
                  -- Governo_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Governo_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Governo_Saida.Movimentacao_Peca_IT_ID
                                                                        AND Governo_Saida.Enum_Tipo_Movimentacao_ID = 94
                  -- Encomenda_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Encomenda_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Encomenda_Saida.Movimentacao_Peca_IT_ID
                                                                          AND Encomenda_Saida.Enum_Tipo_Movimentacao_ID = 95
             WHERE Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID IN
             (
                 SELECT Movimentacao_Peca_CT_ID
                 FROM @TempMovimentacao
             )
                   AND Movimentacao_Peca_CT.Movimentacao_Peca_CT_Data >= @DataInicial
             GROUP BY TempMovimentacao.Peca_ID, 
                      TempMovimentacao.Loja_ID, 
                      TempMovimentacao.Movimentacao_Peca_CT_ID, 
                      TempMovimentacao.Data_Movimento, 
                      TempMovimentacao.EstoqueInicial, 
                      TempMovimentacao.Quantidade_Entrada, 
                      TempMovimentacao.Quantidade_Saida, 
                      TempMovimentacao.EstoqueFinal, 
                      TempMovimentacao.Inventario_Rotativo
             UNION ALL
             SELECT TempMovimentacao.Peca_ID AS Peca_ID, 
                    TempMovimentacao.Loja_ID AS Loja_ID, 
                    TempMovimentacao.Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
                    TempMovimentacao.Data_Movimento AS Data_Movimento, 
                    TempMovimentacao.EstoqueInicial AS EstoqueInicial, 
                    TempMovimentacao.Quantidade_Entrada AS Quantidade_Entrada, 
                    TempMovimentacao.Quantidade_Saida AS Quantidade_Saida, 
                    TempMovimentacao.EstoqueFinal AS EstoqueFinal, 
                    ISNULL(SUM(ISNULL(Abastecimento_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Abastecimento_Entrada, 
                    ISNULL(SUM(ISNULL(Distribuicao_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Distribuicao_Entrada, 
                    ISNULL(SUM(ISNULL(Transferencia_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Transferencia_Entrada, 
                    ISNULL(SUM(ISNULL(Garantia_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Garantia_Entrada, 
                    ISNULL(SUM(ISNULL(Devolucao_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Devolucao_Entrada, 
                    ISNULL(SUM(ISNULL(Ajuste_Estoque_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Ajuste_Estoque_Entrada, 
                    ISNULL(SUM(ISNULL(Retorno_Garantia_Fornecedor_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Retorno_Garantia_Fornecedor_Entrada, 
                    ISNULL(SUM(ISNULL(Compra_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Compra_Entrada, 
                    ISNULL(SUM(ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Troca_Entrada, 
                    ISNULL(SUM(ISNULL(Encomenda_Entrada.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Encomenda_Entrada, 
                    ISNULL(SUM(ISNULL(Abastecimento_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Abastecimento_Saida, 
                    ISNULL(SUM(ISNULL(Distribuicao_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Distribuicao_Saida, 
                    ISNULL(SUM(ISNULL(Transferencia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Transferencia_Saida, 
                    ISNULL(SUM(ISNULL(Garantia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Garantia_Saida, 
                    ISNULL(SUM(ISNULL(Devolucao_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Devolucao_Saida, 
                    ISNULL(SUM(ISNULL(Ajuste_Estoque_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Ajuste_Estoque_Saida, 
                    ISNULL(SUM(ISNULL(Envio_Garantia_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Envio_Garantia_Saida, 
                    ISNULL(SUM(ISNULL(Prejuizo_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Prejuizo_Saida, 
                    ISNULL(SUM(ISNULL(Venda_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Venda_Saida, 
                    ISNULL(SUM(ISNULL(Governo_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Governo_Saida, 
                    ISNULL(SUM(ISNULL(Encomenda_Saida.Movimentacao_Peca_IT_Quantidade, 0)), 0) AS Qtde_Encomenda_Saida, 
                    SUM((ISNULL(Venda_Saida.Movimentacao_Peca_IT_Quantidade, 0) * ISNULL(Venda_Saida.Movimentacao_Peca_IT_Valor_Peca_Preco, 0)) - (ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Quantidade, 0) * ISNULL(Troca_Entrada.Movimentacao_Peca_IT_Valor_Peca_Preco, 0))) AS Valor_Movimentado, 
                    TempMovimentacao.Inventario_Rotativo AS Inventario_Rotativo
             FROM MCAR_DW.dbo.Movimentacao_Peca_IT Movimentacao_Peca_IT
                  INNER JOIN MCAR_DW.dbo.Movimentacao_Peca_CT Movimentacao_Peca_CT ON Movimentacao_Peca_CT.Movimentacao_Peca_CT_ID = Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID
                  INNER JOIN @TempMovimentacao TempMovimentacao ON TempMovimentacao.Data_Movimento = Movimentacao_Peca_CT.Movimentacao_Peca_CT_Data
                  -- Abastecimento_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Abastecimento_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Abastecimento_Entrada.Movimentacao_Peca_IT_ID
                                                                                AND Abastecimento_Entrada.Enum_Tipo_Movimentacao_ID = 75
                  -- Distribuicao_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Distribuicao_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Distribuicao_Entrada.Movimentacao_Peca_IT_ID
                                                                               AND Distribuicao_Entrada.Enum_Tipo_Movimentacao_ID = 76
                  -- Transferencia_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Transferencia_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Transferencia_Entrada.Movimentacao_Peca_IT_ID
                                                                                AND Transferencia_Entrada.Enum_Tipo_Movimentacao_ID = 77
                  -- Garantia_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Garantia_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Garantia_Entrada.Movimentacao_Peca_IT_ID
                                                                           AND Garantia_Entrada.Enum_Tipo_Movimentacao_ID = 78
                  -- Devolucao_Entrada 
                  LEFT OUTER JOIN Movimentacao_Peca_IT Devolucao_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Devolucao_Entrada.Movimentacao_Peca_IT_ID
                                                                            AND Devolucao_Entrada.Enum_Tipo_Movimentacao_ID = 79
                  -- Ajuste_Estoque_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Ajuste_Estoque_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Ajuste_Estoque_Entrada.Movimentacao_Peca_IT_ID
                                                                                 AND Ajuste_Estoque_Entrada.Enum_Tipo_Movimentacao_ID = 80
                  -- Retorno_Garantia_Fornecedor_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Retorno_Garantia_Fornecedor_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Retorno_Garantia_Fornecedor_Entrada.Movimentacao_Peca_IT_ID
                                                                                              AND Retorno_Garantia_Fornecedor_Entrada.Enum_Tipo_Movimentacao_ID = 81
                  -- Compra_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Compra_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Compra_Entrada.Movimentacao_Peca_IT_ID
                                                                         AND Compra_Entrada.Enum_Tipo_Movimentacao_ID = 82
                  -- Troca_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Troca_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Troca_Entrada.Movimentacao_Peca_IT_ID
                                                                        AND Troca_Entrada.Enum_Tipo_Movimentacao_ID = 83
                  -- Encomenda_Entrada
                  LEFT OUTER JOIN Movimentacao_Peca_IT Encomenda_Entrada ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Encomenda_Entrada.Movimentacao_Peca_IT_ID
                                                                            AND Encomenda_Entrada.Enum_Tipo_Movimentacao_ID = 84
                  -- Abastecimento_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Abastecimento_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Abastecimento_Saida.Movimentacao_Peca_IT_ID
                                                                              AND Abastecimento_Saida.Enum_Tipo_Movimentacao_ID = 85
                  -- Distribuicao_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Distribuicao_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Distribuicao_Saida.Movimentacao_Peca_IT_ID
                                                                             AND Distribuicao_Saida.Enum_Tipo_Movimentacao_ID = 86
                  -- Transferencia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Transferencia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Transferencia_Saida.Movimentacao_Peca_IT_ID
                                                                              AND Transferencia_Saida.Enum_Tipo_Movimentacao_ID = 87
                  -- Garantia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Garantia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Garantia_Saida.Movimentacao_Peca_IT_ID
                                                                         AND Garantia_Saida.Enum_Tipo_Movimentacao_ID = 88
                  -- Devolucao_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Devolucao_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Devolucao_Saida.Movimentacao_Peca_IT_ID
                                                                          AND Devolucao_Saida.Enum_Tipo_Movimentacao_ID = 89
                  -- Ajuste_Estoque_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Ajuste_Estoque_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Ajuste_Estoque_Saida.Movimentacao_Peca_IT_ID
                                                                               AND Ajuste_Estoque_Saida.Enum_Tipo_Movimentacao_ID = 90
                  -- Envio_Garantia_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Envio_Garantia_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Envio_Garantia_Saida.Movimentacao_Peca_IT_ID
                                                                               AND Envio_Garantia_Saida.Enum_Tipo_Movimentacao_ID = 91
                  -- Prejuizo_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Prejuizo_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Prejuizo_Saida.Movimentacao_Peca_IT_ID
                                                                         AND Prejuizo_Saida.Enum_Tipo_Movimentacao_ID = 92
                  -- Venda_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Venda_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Venda_Saida.Movimentacao_Peca_IT_ID
                                                                      AND Venda_Saida.Enum_Tipo_Movimentacao_ID = 93
                  -- Governo_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Governo_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Governo_Saida.Movimentacao_Peca_IT_ID
                                                                        AND Governo_Saida.Enum_Tipo_Movimentacao_ID = 94
                  -- Encomenda_Saida
                  LEFT OUTER JOIN Movimentacao_Peca_IT Encomenda_Saida ON Movimentacao_Peca_IT.Movimentacao_Peca_IT_ID = Encomenda_Saida.Movimentacao_Peca_IT_ID
                                                                          AND Encomenda_Saida.Enum_Tipo_Movimentacao_ID = 95
             WHERE Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID IN
             (
                 SELECT Movimentacao_Peca_CT_ID
                 FROM @TempMovimentacao
             )
                   AND Movimentacao_Peca_CT.Movimentacao_Peca_CT_Data >= @DataInicial
             GROUP BY TempMovimentacao.Peca_ID, 
                      TempMovimentacao.Loja_ID, 
                      TempMovimentacao.Movimentacao_Peca_CT_ID, 
                      TempMovimentacao.Data_Movimento, 
                      TempMovimentacao.EstoqueInicial, 
                      TempMovimentacao.Quantidade_Entrada, 
                      TempMovimentacao.Quantidade_Saida, 
                      TempMovimentacao.EstoqueFinal, 
                      TempMovimentacao.Inventario_Rotativo
             ORDER BY TempMovimentacao.Data_Movimento DESC;
     END;
     SELECT Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
            Enum_Tipo_Movimentacao_ID AS Enum_Tipo_Movimentacao_ID, 
            Enum_Extenso AS Enum_Extenso,
            CASE
                WHEN Enum_Tipo_Movimentacao_ID = @EntradaTransferencia
                     OR Enum_Tipo_Movimentacao_ID = @EntradaEC
                THEN LojaOrigem.Lojas_NM
                WHEN Enum_Tipo_Movimentacao_ID = @SaidaTransferencia
                     OR Enum_Tipo_Movimentacao_ID = @SaidaEC
                THEN LojaDestino.Lojas_NM
                WHEN Enum_Tipo_Movimentacao_ID = @Entrada_Cross
                THEN LojaOrigem.Lojas_NM
                WHEN Enum_Tipo_Movimentacao_ID = @Saida_Cross
                THEN LojaDestino.Lojas_NM
                ELSE ''
            END AS OrigemDestino,
            CASE --Trocas, exibir o numero do romaneio
                WHEN(Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Troca)
                THEN CASE
                         WHEN
     (
         SELECT ISNULL(Transferencia_CT.Objeto_Origem_ID, 0)
         FROM Transferencia_CT CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID IN (424, 798)
               AND CT.Loja_Destino_ID = @Loja_ID
     ) <> 0
                         THEN
     (
         SELECT Objeto_Origem_ID
         FROM Transferencia_CT CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID IN (424, 798)
               AND CT.Loja_Destino_ID = @Loja_ID
     )
                         ELSE(Movimentacao_Peca_IT.Objeto_Origem_ID)
                     END
                ELSE(Movimentacao_Peca_IT.Objeto_Origem_ID)
            END AS Objeto_Origem_ID,
            CASE --Trocas, exibir o numero do romaneio0
                WHEN(Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Troca)
                THEN CASE
                         WHEN
     (
         SELECT ISNULL(Transferencia_CT.Objeto_Origem_ID, 0)
         FROM Transferencia_CT CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID IN (424, 798)
               AND CT.Loja_Destino_ID = @Loja_ID
     ) <> 0
                         THEN CONVERT(VARCHAR,
     (
         SELECT Objeto_Origem_ID
         FROM Transferencia_CT CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID IN (424, 798)
               AND CT.Loja_Destino_ID = @Loja_ID
     ))
                         ELSE CONVERT(VARCHAR, Movimentacao_Peca_IT.Objeto_Origem_ID)
                     END
                WHEN(Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Venda
                     OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Venda_Oferta)
                THEN 'Clique p/ detalhes'
                ELSE CONVERT(VARCHAR, Movimentacao_Peca_IT.Objeto_Origem_ID)
            END AS Objeto_Origem_Visivel_ID, 
            Movimentacao_Peca_IT_Data AS Movimentacao_Peca_IT_Data, 
            Movimentacao_Peca_IT_Quantidade AS Movimentacao_Peca_IT_Quantidade, 
            Movimentacao_Peca_IT_Valor_Peca_Preco AS Movimentacao_Peca_IT_Valor_Peca_Preco, 
            Movimentacao_Peca_IT_Valor_Custo_Medio AS Movimentacao_Peca_IT_Valor_Custo_Medio, 
            Movimentacao_Peca_IT_Valor_Custo_Unitario AS Movimentacao_Peca_IT_Valor_Custo_Unitario, 
            Movimentacao_Peca_IT_Valor_Custo_Reposicao AS Movimentacao_Peca_IT_Valor_Custo_Reposicao, 
            Movimentacao_Peca_IT_Valor_Desconto AS Movimentacao_Peca_IT_Valor_Desconto
     FROM vw_Movimentacao_Peca Movimentacao_Peca_IT
          INNER JOIN Enumerado ON Enum_Tipo_Movimentacao_ID = Enum_ID
          LEFT OUTER JOIN Transferencia_CT ON Transferencia_CT.Transferencia_CT_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
                                              AND (Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @SaidaTransferencia
                                                   OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @EntradaTransferencia
                                                   OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @SaidaEC
                                                   OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @EntradaEC
                                                   OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Entrada_Cross
                                                   OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Saida_Cross)
          --OR
          --	Transferencia_CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
          LEFT OUTER JOIN Lojas LojaOrigem ON Transferencia_CT.Loja_Origem_ID = LojaOrigem.Lojas_ID
          LEFT OUTER JOIN Lojas LojaDestino ON Transferencia_CT.Loja_Destino_ID = LojaDestino.Lojas_ID
     WHERE Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID IN
     (
         SELECT Movimentacao_Peca_CT_ID
         FROM @TempMovimentacao
     )
     ORDER BY Movimentacao_Peca_IT_Data DESC;
     DROP TABLE #Temp;
     SET NOCOUNT OFF;