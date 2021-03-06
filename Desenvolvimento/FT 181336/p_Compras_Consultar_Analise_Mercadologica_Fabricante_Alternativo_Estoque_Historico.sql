-------------------------------------------------------------------------------
-- <summary>  
--	Retorna o historico de movimentaÃ§ao do grupo do fabricante alternativo	
-- </summary>  
-- <history>  
--		[mmukuno]		- 06/02/2017	Modified
--		[rnoliveira]	- 24/07/2017	Modified
--			Foi alterado para agrupar por fabricante_alternativo_ct_id
--			Foi removido o agrupamento por Movimentacao_Peca_CT_ID
--		[bmune]			- 25/06/2018	Modified
--			Modificado a consulta do estoque inicial para buscar a soma de todas as peças
--		[efsousa]			- 25/09/2018	Modified
--			Retirado HINT de Recompile
--		[fmoraes]		- 24/07/2019	Modified
--			Criada Tabela @tempTransferencias para retorno do campo Transferencia_CT_ID para permitir a consulta do 
--			romaneio de transferências da Entreda de Garantia
-- </history>  
-------------------------------------------------------------------------------

ALTER PROCEDURE [dbo].[p_Compras_Consultar_Analise_Mercadologica_Fabricante_Alternativo_Estoque_Historico_FT_181336]
(
     @Peca_ID     INT, 
     @Loja_ID     INT, 
     @DataInicial DATETIME
)
--WITH RECOMPILE
AS

-------------------------------------------------------------------------------
--DECLARE 
--     @Peca_ID      INT, 
--     @Loja_ID      INT, 
--     @DataInicial  DATETIME;

--SET @Peca_ID	   = 83628
--SET @Loja_ID	   = 13
--SET @DataInicial   = '2014-01-01'
-------------------------------------------------------------------------------

     SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
     SET NOCOUNT ON;

     DECLARE 
          @SaidaTransferencia                             INT, 
          @EntradaTransferencia                           INT, 
          @SaidaEC                                        INT, 
          @EntradaEC                                      INT, 
          @Entrada_Garantia                               INT, 
          @Entrada_Cross                                  INT, 
          @Saida_Cross                                    INT, 
          @Enum_Tipo_Inventario_Rotativo                  INT, 
          @Enum_Tipo_Movimentacao_Venda                   INT, 
          @Enum_Tipo_Movimentacao_Troca                   INT, 
          @Enum_Tipo_Movimentacao_Venda_Oferta            INT, 
          @Enum_Status_Fabricante_Alternativo_Liberado_ID INT, 
          @Enum_Acao_Item_Alternativo_Excluir_ID          INT = 697;

     SET @EntradaEC                                       = 75;
	 SET @Entrada_Cross                                   = 76;
     SET @EntradaTransferencia                            = 77;
     SET @Entrada_Garantia                                = 78;
     SET @Enum_Tipo_Movimentacao_Troca                    = 83;
     SET @SaidaEC                                         = 85;
     SET @Saida_Cross                                     = 86;
     SET @SaidaTransferencia                              = 87;
     SET @Enum_Tipo_Movimentacao_Venda                    = 93;
	 SET @Enum_Status_Fabricante_Alternativo_Liberado_ID  = 1043;
     SET @Enum_Tipo_Movimentacao_Venda_Oferta             = 1311;
	 SET @Enum_Tipo_Inventario_Rotativo                   = 1648;


     DECLARE 
          @Fabricante_Alternativo_CT_ID INT;
     SELECT @Fabricante_Alternativo_CT_ID = Fabricante_Alternativo_CT.Fabricante_Alternativo_CT_ID
     FROM Fabricante_Alternativo_CT
          JOIN Fabricante_Alternativo_IT
          ON Fabricante_Alternativo_CT.Fabricante_Alternativo_CT_ID = Fabricante_Alternativo_IT.Fabricante_Alternativo_CT_ID
             AND Fabricante_Alternativo_CT.Fabricante_Alternativo_CT_Ativo = 1
             AND Fabricante_Alternativo_IT.Fabricante_Alternativo_IT_Ativo = 1
     WHERE Fabricante_Alternativo_CT.Enum_Status_ID = @Enum_Status_Fabricante_Alternativo_Liberado_ID
           AND Peca_ID = @Peca_ID;


     CREATE TABLE #tmp_peca_grupo_alternativo
     (
          Peca_ID INT
     );
     INSERT INTO #tmp_peca_grupo_alternativo(Peca_ID)
     SELECT Peca_ID
     FROM Fabricante_Alternativo_IT
     WHERE Fabricante_Alternativo_CT_ID = @Fabricante_Alternativo_CT_ID
           AND Fabricante_Alternativo_IT.Fabricante_Alternativo_IT_Ativo = 1;

     -------------------------------------------------------------------------------
     /*** Pegar a data do ultimo fechamento de estoque. ***/
     -------------------------------------------------------------------------------

     DECLARE 
          @DataUltimoFechamento AS DATETIME;
     SELECT @DataUltimoFechamento = ISNULL(MAX(Estoque_Fechamento_Data), DATEADD(MONTH, -1, @DataInicial))
     FROM vw_Estoque_Fechamento
          INNER JOIN #tmp_peca_grupo_alternativo
          ON vw_Estoque_Fechamento.Peca_ID IN(SELECT Peca_ID
                                              FROM #tmp_peca_grupo_alternativo)
     WHERE Estoque_Fechamento_Data < @DataInicial
           AND Loja_ID = @Loja_ID;

     DECLARE 
          @tempMovimentacao TABLE
     (
          ID                           INT IDENTITY(1, 1), 
          Fabricante_Alternativo_CT_ID INT, 
          Loja_ID                      INT, 
          Movimentacao_Peca_CT_ID      INT, 
          Data_Movimento               DATETIME, 
          EstoqueInicial               INT, 
          Quantidade_Entrada           INT, 
          Quantidade_Saida             INT, 
          EstoqueFinal                 INT, 
          Inventario_Rotativo          BIT
     );

	 DECLARE 
          @tempTransferencias TABLE 
     (
          Movimentacao_Peca_CT_ID                    INT, 
          Enum_Tipo_Movimentacao_ID                  INT, 
          Enum_Extenso                               VARCHAR(50), 
          OrigemDestino                              VARCHAR(50), 
          Objeto_Origem_ID                           INT, 
          Objeto_Origem_Visivel_ID                   VARCHAR(50), 
          Movimentacao_Peca_IT_Data                  DATETIME, 
          Movimentacao_Peca_IT_Quantidade            INT, 
          Movimentacao_Peca_IT_Valor_Peca_Preco      DECIMAL, 
          Movimentacao_Peca_IT_Valor_Custo_Medio     DECIMAL, 
          Movimentacao_Peca_IT_Valor_Custo_Unitario  DECIMAL, 
          Movimentacao_Peca_IT_Valor_Custo_Reposicao DECIMAL, 
          Movimentacao_Peca_IT_Valor_Desconto        DECIMAL
     );

     IF OBJECT_ID('TEMPDB..#Temp') IS NOT NULL
       BEGIN
         DROP TABLE #Temp;
     END;

     CREATE TABLE #Temp
     (
          Fabricante_Alternativo_CT_ID INT, 
          Peca_ID                      INT, 
          Loja_ID                      INT, 
          Movimentacao_Peca_CT_ID      INT, 
          Movimentacao_Peca_CT_Data    DATETIME, 
          Estoque_Inicial              INT, 
          Quantidade_Entrada           INT, 
          Quantidade_Saida             INT, 
          Estoque_Final                INT
     );

     INSERT INTO #Temp
     SELECT @Fabricante_Alternativo_CT_ID AS Fabricante_Alternativo_CT_ID, 
            vw_Movimentacao_Peca.Peca_ID AS Peca_ID, 
            Loja_ID AS Loja_ID, 
            Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
            Movimentacao_Peca_CT_Data AS Movimentacao_Peca_CT_Data, 
            CAST(0 AS INT) AS Estoque_Inicial, 
            (CASE WHEN Enum_Tipo_Movimentacao_ID IN(SELECT Enum_ID
                                                    FROM Enumerado
                                                    WHERE Enum_Nome = 'TipoMovimentacao'
                                                          AND Enum_Sigla = 'Entrada')
               THEN Movimentacao_Peca_IT_Quantidade
               ELSE 0
             END) AS Quantidade_Entrada, 
            (CASE WHEN Enum_Tipo_Movimentacao_ID IN(SELECT Enum_ID
                                                    FROM Enumerado
                                                    WHERE Enum_Nome = 'TipoMovimentacao'
                                                          AND Enum_Sigla = 'Saida')
               THEN Movimentacao_Peca_IT_Quantidade
               ELSE 0
             END) AS Quantidade_Saida, 
            CAST(0 AS INT) AS Estoque_Final
     FROM vw_Movimentacao_Peca
          INNER JOIN #tmp_peca_grupo_alternativo
          ON vw_Movimentacao_Peca.Peca_ID = #tmp_peca_grupo_alternativo.Peca_ID
     WHERE Loja_ID = @Loja_ID
           AND Movimentacao_Peca_CT_Data > @DataUltimoFechamento
     ORDER BY Movimentacao_Peca_CT_Data;

     INSERT INTO #Temp
     SELECT @Fabricante_Alternativo_CT_ID, 
            0, 
            @Loja_ID, 
            0, 
            @DataUltimoFechamento, 
            SUM(vw_Estoque_Fechamento.Estoque_Fechamento_Quantidade), 
            0, 
            0, 
            0
     FROM #tmp_peca_grupo_alternativo
          INNER JOIN vw_Estoque_Fechamento
          ON vw_Estoque_Fechamento.Peca_ID = #tmp_peca_grupo_alternativo.Peca_ID
     WHERE NOT EXISTS
     (
         SELECT 1
         FROM #Temp
         WHERE #Temp.Peca_ID = #tmp_peca_grupo_alternativo.Peca_ID
               AND #temp.Movimentacao_Peca_CT_Data = @DataUltimoFechamento
     )
           AND vw_Estoque_Fechamento.Loja_ID = @Loja_ID
           AND vw_Estoque_Fechamento.Estoque_Fechamento_Data = @DataUltimoFechamento;

     INSERT INTO @TempMovimentacao
     SELECT Fabricante_Alternativo_CT_ID AS Fabricante_Alternativo_CT_ID, 
            Loja_ID AS Loja_ID, 
            0 AS Movimentacao_Peca_CT_ID, 
            Movimentacao_Peca_CT_Data AS Movimentacao_Peca_CT_Data, 
            Estoque_Inicial AS Estoque_Inicial, 
            SUM(Quantidade_Entrada) AS Quantidade_Entrada, 
            SUM(Quantidade_Saida) AS Quantidade_Saida, 
            Estoque_Final AS Estoque_Final, 
            0 AS Inventario_Rotativo
     FROM #Temp
     GROUP BY Fabricante_Alternativo_CT_ID, 
              Loja_ID, 
              Movimentacao_Peca_CT_Data, 
              Estoque_Inicial, 
              Estoque_Final
     ORDER BY Movimentacao_Peca_CT_Data;

     DECLARE 
          @COD INT;
     DECLARE 
          @MAX INT;

     SET @COD = 1;
     SELECT @MAX = COUNT(*)
     FROM @TempMovimentacao;

     UPDATE @TempMovimentacao
       SET 
           EstoqueInicial = ISNULL(
     (
         SELECT SUM(ISNULL(Estoque_Fechamento.Estoque_Fechamento_Quantidade, 0))
         FROM vw_Estoque_Fechamento AS Estoque_Fechamento
         WHERE Estoque_Fechamento.Peca_ID IN
         (
             SELECT Peca_ID
             FROM #Temp
         )
               AND Estoque_Fechamento.Loja_ID = TempMovimentacao.Loja_ID
               AND Estoque_Fechamento.Estoque_Fechamento_Data = @DataUltimoFechamento
     ), 0), 
           EstoqueFinal = ISNULL(
     (
         SELECT SUM(ISNULL(Estoque_Fechamento.Estoque_Fechamento_Quantidade, 0))
         FROM vw_Estoque_Fechamento AS Estoque_Fechamento
         WHERE Estoque_Fechamento.Peca_ID IN
         (
             SELECT Peca_ID
             FROM #Temp
         )
               AND Estoque_Fechamento.Loja_ID = TempMovimentacao.Loja_ID
               AND Estoque_Fechamento.Estoque_Fechamento_Data = @DataUltimoFechamento
     ), 0) + Quantidade_Entrada - Quantidade_Saida
     FROM @TempMovimentacao TempMovimentacao
     WHERE TempMovimentacao.ID = 1;

     UPDATE MovDia
       SET 
           Inventario_Rotativo = 1
     FROM @TempMovimentacao MovDia
          JOIN Inventario_CT
          ON MovDia.Loja_ID = Inventario_CT.Lojas_ID
          JOIN Inventario_IT
          ON Inventario_CT.Inventario_CT_ID = Inventario_IT.Inventario_CT_ID
     WHERE Inventario_CT.Enum_Tipo_ID = @Enum_Tipo_Inventario_Rotativo
           AND Inventario_IT.Peca_ID IN
     (
         SELECT Peca_ID
         FROM #tmp_peca_grupo_alternativo
     )
           AND MovDia.Data_Movimento BETWEEN CONVERT(DATE, Inventario_CT.Inventario_CT_Data_Criacao) AND CONVERT(DATE, Inventario_CT.Inventario_CT_Data_Finalizacao);

     WHILE @COD <= @MAX
       BEGIN
         UPDATE MovDia
           SET 
               EstoqueInicial = MovDiaAnterior.EstoqueFinal, 
               EstoqueFinal = MovDiaAnterior.EstoqueFinal + MovDia.Quantidade_Entrada - MovDia.Quantidade_Saida
         FROM @TempMovimentacao MovDia
              INNER JOIN @TempMovimentacao MovDiaAnterior
              ON MovDiaAnterior.ID = @COD - 1
         WHERE MovDia.ID = @COD;

         SET @COD = @COD + 1;
       END;

     SELECT TempMovimentacao.Loja_ID AS Loja_ID, 
            CONVERT(DATE, TempMovimentacao.Data_Movimento) AS Data_Movimento, 
            ISNULL(TempMovimentacao.EstoqueInicial, 0) AS EstoqueInicial, 
            SUM(ISNULL(TempMovimentacao.Quantidade_Entrada, 0)) AS Quantidade_Entrada, 
            SUM(ISNULL(TempMovimentacao.Quantidade_Saida, 0)) AS Quantidade_Saida, 
            ISNULL(TempMovimentacao.EstoqueFinal, 0) AS EstoqueFinal, 
            TempMovimentacao.Inventario_Rotativo AS Inventario_Rotativo
     FROM @TempMovimentacao AS TempMovimentacao
     WHERE TempMovimentacao.Data_Movimento >= @DataInicial
     GROUP BY TempMovimentacao.Loja_ID, 
              CONVERT(DATE, TempMovimentacao.Data_Movimento), 
              TempMovimentacao.Inventario_Rotativo, 
              TempMovimentacao.EstoqueInicial, 
              TempMovimentacao.EstoqueFinal
     HAVING ISNULL(SUM(ISNULL(TempMovimentacao.Quantidade_Entrada, 0)), 0) <> 0
            OR ISNULL(SUM(ISNULL(TempMovimentacao.Quantidade_Saida, 0)), 0) <> 0
     ORDER BY CONVERT(DATE, TempMovimentacao.Data_Movimento) DESC;

	 INSERT INTO @tempTransferencias
     SELECT Movimentacao_Peca_CT_ID AS Movimentacao_Peca_CT_ID, 
            Enum_Tipo_Movimentacao_ID AS Enum_Tipo_Movimentacao_ID, 
            Enum_Extenso AS Enum_Extenso,
            CASE WHEN Enum_Tipo_Movimentacao_ID = @EntradaTransferencia
                      OR Enum_Tipo_Movimentacao_ID = @EntradaEC
              THEN LojaOrigem.Lojas_NM WHEN Enum_Tipo_Movimentacao_ID = @SaidaTransferencia
                                            OR Enum_Tipo_Movimentacao_ID = @SaidaEC
              THEN LojaDestino.Lojas_NM WHEN Enum_Tipo_Movimentacao_ID = @Entrada_Cross
              THEN LojaOrigem.Lojas_NM WHEN Enum_Tipo_Movimentacao_ID = @Saida_Cross
              THEN LojaDestino.Lojas_NM
              ELSE ''
            END AS OrigemDestino,
            CASE --Trocas, exibir o numero do romaneio
                   WHEN(Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Troca)
              THEN CASE WHEN
     (
         SELECT ISNULL(Transferencia_CT.Objeto_Origem_ID, 0)
         FROM Transferencia_CT AS CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID = 798
               AND CT.Loja_Destino_ID = @Loja_ID
     ) <> 0
                     THEN
     (
         SELECT Objeto_Origem_ID
         FROM Transferencia_CT AS CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID = 798
               AND CT.Loja_Destino_ID = @Loja_ID
     )
                     ELSE(Movimentacao_Peca_IT.Objeto_Origem_ID)
                   END
              ELSE(Movimentacao_Peca_IT.Objeto_Origem_ID)
            END AS Objeto_Origem_ID,
            CASE --Trocas, exibir o numero do romaneio0
                   WHEN(Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Troca)
              THEN CASE WHEN
     (
         SELECT ISNULL(Transferencia_CT.Objeto_Origem_ID, 0)
         FROM Transferencia_CT AS CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID = 798
               AND CT.Loja_Destino_ID = @Loja_ID
     ) <> 0
                     THEN CONVERT(VARCHAR,
     (
         SELECT Objeto_Origem_ID
         FROM Transferencia_CT AS CT
         WHERE CT.Objeto_Origem_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
               AND CT.Enum_Tipo_Transferencia_ID = 798
               AND CT.Loja_Destino_ID = @Loja_ID
     ))
                     ELSE CONVERT(VARCHAR, Movimentacao_Peca_IT.Objeto_Origem_ID)
                   END WHEN(Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Venda
                            OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Enum_Tipo_Movimentacao_Venda_Oferta)
              THEN 'Clique p/ detalhes'
              ELSE CONVERT(VARCHAR, Movimentacao_Peca_IT.Objeto_Origem_ID)
            END AS Objeto_Origem_Visivel_ID, 
            CONVERT(DATE, Movimentacao_Peca_IT_Data) AS Movimentacao_Peca_IT_Data, 
            Movimentacao_Peca_IT_Quantidade AS Movimentacao_Peca_IT_Quantidade, 
            Movimentacao_Peca_IT_Valor_Peca_Preco AS Movimentacao_Peca_IT_Valor_Peca_Preco, 
            Movimentacao_Peca_IT_Valor_Custo_Medio AS Movimentacao_Peca_IT_Valor_Custo_Medio, 
            Movimentacao_Peca_IT_Valor_Custo_Unitario AS Movimentacao_Peca_IT_Valor_Custo_Unitario, 
            Movimentacao_Peca_IT_Valor_Custo_Reposicao AS Movimentacao_Peca_IT_Valor_Custo_Reposicao, 
            Movimentacao_Peca_IT_Valor_Desconto AS Movimentacao_Peca_IT_Valor_Desconto
     FROM vw_Movimentacao_Peca AS Movimentacao_Peca_IT
          INNER JOIN Enumerado
          ON Enum_Tipo_Movimentacao_ID = Enum_ID
          LEFT OUTER JOIN Transferencia_CT
          ON Transferencia_CT.Transferencia_CT_ID = Movimentacao_Peca_IT.Objeto_Origem_ID
             AND (Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @SaidaTransferencia
                  OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @EntradaTransferencia
                  OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @SaidaEC
                  OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @EntradaEC
                  OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Entrada_Cross
                  OR Movimentacao_Peca_IT.Enum_Tipo_Movimentacao_ID = @Saida_Cross)
          LEFT OUTER JOIN Lojas AS LojaOrigem
          ON Transferencia_CT.Loja_Origem_ID = LojaOrigem.Lojas_ID
          LEFT OUTER JOIN Lojas AS LojaDestino
          ON Transferencia_CT.Loja_Destino_ID = LojaDestino.Lojas_ID
     WHERE Movimentacao_Peca_IT.Movimentacao_Peca_CT_ID IN
     (
         SELECT Movimentacao_Peca_CT_ID
         FROM #Temp
     )
     ORDER BY Movimentacao_Peca_IT_Data DESC;

	 SELECT * FROM @tempTransferencias;

     SELECT Transferencia_CT_ID, 
            Movimentacao_Peca_CT_ID, 
            Enum_Tipo_Movimentacao_ID, 
            Enum_Extenso, 
            TF.Objeto_Origem_ID, 
            Movimentacao_Peca_IT_Data
     FROM @tempTransferencias AS TF
          INNER JOIN Transferencia_CT AS CT
          ON TF.Objeto_Origem_ID = CT.Objeto_Origem_ID
     WHERE Enum_Tipo_Movimentacao_ID = 78;

-------------------------------------------------------------------------------
--	DELETA AS TABELAS TEMPORARIAS
-------------------------------------------------------------------------------

     IF OBJECT_ID('tempdb..#Temp') IS NOT NULL
       BEGIN
         DROP TABLE #Temp;
     END;

     IF OBJECT_ID('tempdb..#tmp_peca_grupo_alternativo') IS NOT NULL
       BEGIN
         DROP TABLE #tmp_peca_grupo_alternativo;
     END;

     IF OBJECT_ID('tempdb..#tmp_calendario') IS NOT NULL
       BEGIN
         DROP TABLE #tmp_calendario;
     END;

SET NOCOUNT OFF;