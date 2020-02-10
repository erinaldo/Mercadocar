-------------------------------------------------------------------------------
--  fmoraes 22/01/2020
--  FT 213917 - Registrar as Exclusões do Lote de Abastecimento 
-------------------------------------------------------------------------------


-- ============================================================================
-- UTILITARIOS
-- ============================================================================

USE MCAR_Desenvolvimento
GO

/*
SELECT * FROM Usuario WHERE Usuario_Login = 'fmoraes'
SELECT * FROM Usuario WHERE Usuario_ID = 7533

-------------------------------------------------------------------------------
-- PESQUISA

-- O campo do TYPE armazena o tipo do objeto a ser localizado, onde :
-- U => Tabela Usuário
-- S => Tabela de sistema
-- P => Procedure
-- V => View
-- F => Function
 
SELECT A.NAME, A.TYPE, B.TEXT
  FROM SYSOBJECTS  A (nolock)
  JOIN SYSCOMMENTS B (nolock)   
    ON A.ID = B.ID
WHERE 
  B.TEXT LIKE '%Loja%'  --- Informação a ser procurada no corpo da procedure, funcao ou view
  AND A.TYPE = 'U' --- Tipo de objeto a ser localizado no caso procedure
 ORDER BY A.NAME
 
*/
-------------------------------------------------------------------------------

-- Nome do formulário
-- SELECT * FROM Perm_Formulario WHERE Perm_Frm_NM LIKE '%Abastecimento Interno%'

-------------------------------------------------------------------------------

-- ============================================================================
-- ENUMERADOS
-- ============================================================================

-- SELECT * FROM Enumerado WHERE Enum_Nome ='Status_Lote_Abastecimento_Interno'

-- ============================================================================
-- TABELAS
-- ============================================================================

-- ALTER TABLE Abastecimento_Interno_Reserva ADD Abastecimento_Interno_Reserva_Excluido BIT;
-- sp_RENAME 'Abastecimento_Interno_Reserva.Abastecimento_Interno_Reserva_Pendente_Contagem' , 'Abastecimento_Interno_Reserva_Excluido', 'COLUMN'

-- ============================================================================
-- PROCEDURES
-- ============================================================================

-- p_EL_Consultar_Abastecimento_Interno_Reservas_Acompanhamento_FT_213917
-- p_EL_Consultar_Abastecimento_Interno_Lote_FT_213917
-- p_EL_Consultar_Lote_Abastecimento_Interno_Disponivel_Coletor_FT_213917
-- p_EL_Consultar_Inventario_Item_Pendente_Contagem_Grid
-- p_EL_Inventario_Consultar_Itens_Divergentes_Propriedades

-- EXEC p_Jobs_Calcular_Abastecimento_Interno_Reservas

-- ============================================================================
-- CONSULTAS
-- ============================================================================

-- SELECT TOP 5 * FROM [Mcar_Desenvolvimento].[dbo].[Abastecimento_Interno_Lote] ORDER BY Abastecimento_Interno_Lote_ID DESC
-- SELECT * FROM [Mcar_Desenvolvimento].[dbo].[Abastecimento_Interno_Lote] WHERE Enum_Status_ID = 1364 ORDER BY Abastecimento_Interno_Lote_ID DESC
-- SELECT * FROM [Mcar_Desenvolvimento].[dbo].[Abastecimento_Interno_Lote] WHERE Abastecimento_Interno_Lote_ID = 48292

-- SELECT TOP 5 * FROM [Mcar_Desenvolvimento].[dbo].[Abastecimento_Interno_Reserva] ORDER BY Abastecimento_Interno_Reserva DESC
-- SELECT * FROM [Mcar_Chamados].[dbo].[Abastecimento_Interno_Reserva] WHERE Abastecimento_Interno_lote_ID = 136252
-- SELECT * FROM [Mcar_Desenvolvimento].[dbo].[Abastecimento_Interno_Reserva] WHERE Abastecimento_Interno_lote_ID = 48292

-- Peça excluída Lote 48277
-- 0639-0083-0433
-- 0613-0812-0063

-- SELECT TOP 5 * FROM Estoque_Divergencia ORDER BY Estoque_Divergencia_ID DESC


/*
-- Finalizar as reservas de lotes que estão Liberados
SELECT *
FROM 
	Abastecimento_Interno_Reserva
JOIN Abastecimento_Interno_Lote lote on 
	lote.Abastecimento_Interno_Lote_ID = Abastecimento_Interno_Reserva.Abastecimento_Interno_Lote_ID
WHERE 
	Abastecimento_Interno_Reserva_Separado = 0
	AND lote.Enum_Status_ID = 1365
    AND ISNULL(Abastecimento_Interno_Reserva.Abastecimento_Interno_Reserva_Pendente_Contagem, 0) = 0
*/

/*
-- Carrega reservas pendentes (Itens sem Lote e não Fabricante Alternativo)
SELECT *
FROM Abastecimento_Interno_Reserva
     LEFT JOIN Fabricante_Alternativo_IT
     ON Fabricante_Alternativo_IT.Peca_ID = Abastecimento_Interno_Reserva.Peca_ID
        AND Fabricante_Alternativo_IT.Fabricante_Alternativo_IT_Ativo = 1
        AND Fabricante_Alternativo_IT.Enum_Acao_ID != 696
WHERE ISNULL(Abastecimento_Interno_Lote_ID, 0) = 0 
        AND Fabricante_Alternativo_IT.Fabricante_Alternativo_CT_ID IS NULL
        OR ISNULL(Abastecimento_Interno_Reserva.Abastecimento_Interno_Reserva_Pendente_Contagem, 0) = 1
*/

/*
-- Popular variavel com a quantidade de peças que ja foram separadas e ja estão em algum lote
SELECT 
    Peca.Peca_DSTecnica,
    Abastecimento_Interno_Reserva.Peca_ID,
	ISNULL(SUM(ISNULL(Abastecimento_Interno_Reserva_Qtde_Reserva,0)), 0) AS Qtde_Reserva_Pendente_Liberacao
FROM
	Fabricante_Alternativo_IT 
    JOIN Abastecimento_Interno_Reserva 
    ON Abastecimento_Interno_Reserva.Peca_ID = Fabricante_Alternativo_IT.Peca_ID
    AND Abastecimento_Interno_Reserva.Lojas_ID = 1
    AND ISNULL(Abastecimento_Interno_Lote_ID, 0) != 0
    AND Abastecimento_Interno_Reserva_Separado = 0
    JOIN Peca 
    ON Peca.Peca_ID = Abastecimento_Interno_Reserva.Peca_ID
WHERE 
	Fabricante_Alternativo_IT_Ativo = 1
    AND Enum_Acao_ID != 696
    AND Fabricante_Alternativo_CT_ID IN
    (
    SELECT Fabricante_Alternativo_CT_ID FROM Fabricante_Alternativo_CT
    )
GROUP BY
    Peca.Peca_DSTecnica,
    Abastecimento_Interno_Reserva.Peca_ID
    */

-- SELECT * FROM Mcar_Desenvolvimento.dbo.Abastecimento_Interno_Reserva WHERE Abastecimento_Interno_Reserva_Excluido = 1
SELECT * FROM Mcar_Desenvolvimento.dbo.Abastecimento_Interno_Lote WHERE Abastecimento_Interno_Lote_ID = 48302
SELECT * FROM Mcar_Desenvolvimento.dbo.Abastecimento_Interno_Reserva WHERE Abastecimento_Interno_lote_ID = 48302
--SELECT TOP 5 * FROM Mcar_Desenvolvimento.dbo.Estoque_Divergencia ORDER BY Estoque_Divergencia_ID DESC

-- 0372-0083-0188
-- Lote 48302 - Peça 0840-0546-0178 - ñ excluido
-- Lote 48302 - Peça 0266-0746-0200 - exclído


--sp p_Cad_Consultar_Objeto_Destino_Grid