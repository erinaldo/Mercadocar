------------------------------------------------------------------------------
-- Chamdo 198116 - Etiqueta
-- 18/09/2019
------------------------------------------------------------------------------


-- Traz Pecas e Curva

SELECT DISTINCT TOP (5)
CASE Conferencia_CT.Enum_Origem_ID
	WHEN 2008 THEN 'Abastecimento'
	WHEN 2009 THEN 'Romaneio'
	WHEN 2007 THEN 'Grupo.Volume'
	ELSE ''
END									AS Descricao_Origem,
vw_Peca.Peca_ID                     AS Peca_ID,
vw_Peca.Produto_DS					AS Produto_DS,
vw_Peca.Fabricante_CD				AS Fabricante_CD,
vw_Peca.Produto_CD					AS Produto_CD,
vw_Peca.Peca_CD						AS Peca_CD,
vw_Peca.Peca_CDFabricante			AS Peca_CDFabricante,
p.Estoque_Curva_Frequencia          AS Estoque_Curva_Frequencia
FROM
Conferencia_Separacao WITH (NOLOCK)
JOIN Conferencia_IT WITH (NOLOCK) ON
Conferencia_IT.Conferencia_CT_ID = Conferencia_Separacao.Conferencia_CT_ID
AND
Conferencia_IT.Peca_ID = Conferencia_Separacao.Peca_ID
JOIN Conferencia_CT WITH (NOLOCK) ON
Conferencia_CT.Conferencia_CT_ID = Conferencia_Separacao.Conferencia_CT_ID
JOIN Lojas WITH (NOLOCK) ON
Lojas.Lojas_ID = Conferencia_Separacao.Loja_Destino_ID
JOIN vw_Peca WITH (NOLOCK) ON
Conferencia_Separacao.Peca_ID = vw_Peca.Peca_ID
LEFT JOIN Peca_Endereco WITH (NOLOCK) ON
Conferencia_Separacao.Peca_ID = Peca_Endereco.Peca_ID
AND
Conferencia_Separacao.Loja_Destino_ID = Peca_Endereco.Loja_ID
AND
Peca_Endereco.Peca_Endereco_isPrincipal = 1
LEFT JOIN Pre_Recebimento_Volume_CT ON
Conferencia_CT.Objeto_Origem_ID = Pre_Recebimento_Volume_CT.Pre_Recebimento_Volume_CT_ID
LEFT JOIN Peca_Resumo_loja p ON
Peca_Endereco.Peca_ID = p.Peca_ID
WHERE
p.Estoque_Curva_Frequencia IS NOT NULL

SELECT TOP (5) 
Conferencia_Separacao_ID
FROM Conferencia_Separacao
ORDER BY Conferencia_Separacao_ID DESC;

-- Descobrir o ID para a proc sp p_CD_Consultar_Pre_Recebimento_Volume_Para_Conferencia
SELECT Pre_Recebimento_CT_ID
FROM Pre_Recebimento_Volume_Ct
WHERE Pre_Recebimento_Grupo_Id = 119469 -- numero do lote

-- Verificar a Curva de Frequencia e ID da Conferencia
SELECT TOP (5) 
Conferencia_Separacao_ID, Grupo_Separacao_ID, Peca.Peca_Curva_Frequencia, Peca_DSTecnica, Peca_CDFabricante
FROM Conferencia_Separacao
INNER JOIN Peca
ON Conferencia_Separacao.Peca_ID = Peca.Peca_ID
WHERE Loja_Destino_ID = 14 AND Peca_CDFabricante = 'J01008'
ORDER BY Conferencia_Separacao_ID DESC;

