---- FT 194852

-- Tabelas da PROC
SELECT TOP (5) * FROM Romaneio_CT ORDER BY Romaneio_Pre_Venda_Ct_ID DESC
SELECT TOP (5) * FROM Romaneio_Venda_CT ORDER BY Romaneio_Venda_CT_ID DESC
SELECT TOP (5) * FROM Romaneio_Venda_Pagamento ORDER BY Romaneio_Venda_Grupo_ID DESC
SELECT TOP (5) * FROM Romaneio_Credito_Aprovacao ORDER BY Romaneio_Pre_Venda_CT_ID DESC
SELECT TOP (5) * FROM Romaneio_Venda_Origem ORDER BY Romaneio_Pre_Venda_CT_ID DESC


SELECT Romaneio_Venda_CT_ID, Lojas_ID, Romaneio_Venda_Grupo_ID, Objeto_Origem_ID, Loja_Origem_ID
--ISNULL(Romaneio_Venda_CT_ID, 0) AS  Romaneio_Venda_CT_ID
FROM Romaneio_Venda_CT WITH (NOLOCK)
WHERE Romaneio_Pre_Venda_CT_ID = 987662870  AND Lojas_ID = 1

-- Data de Venda Origem
SELECT * FROM Romaneio_Credito_Aprovacao WHERE Romaneio_Pre_Venda_CT_ID = 987662870
SELECT * FROM Romaneio_Venda_CT WHERE Romaneio_Pre_Venda_CT_ID = 987662870
SELECT * FROM Romaneio_Venda_Origem WHERE Romaneio_Pre_Venda_CT_ID = 987662870

-- Prazo de Pagamento
SELECT * FROM Condicao_Pagamento WHERE Condicao_Pagamento_ID = 238
SELECT * FROM Forma_Pagamento WHERE Forma_Pagamento_ID = 42