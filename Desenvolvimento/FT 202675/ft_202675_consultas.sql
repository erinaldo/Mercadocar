-- NOTA FISCAL: 91296

SELECT * FROM Enumerado WHERE Enum_ID IN(146, 156, 230, 1174, 1567, 1571, 1572, 1575, 1591)

SELECT * FROM Enumerado WHERE Enum_Nome LIKE 'Status_Recebimento' AND Enum_ID IN (189, 1636, 1760)

SELECT * FROM Condicao_Pagamento WHERE Condicao_Pagamento_ID IN(12,58)

SELECT * FROM Forma_Pagamento WHERE Forma_Pagamento_ID IN(3, 2, 3)

SELECT * FROM Forma_Pagamento WHERE Forma_Pagamento_DS LIKE '%Fatura%'

SELECT TOP(10) * FROM Interface_Entrada_CT ORDER BY Interface_Entrada_CT_Data_Processamento DESC

SELECT TOP(10) 
Recebimento_CT_ID,
NFe_Entrada_XML_ID,
Enum_Tipo_Recebimento1_ID,
Enum_Tipo_Operacao_ID,
Loja_ID,
Lojas.Lojas_NM,
Enum_Natureza_Operacao1_ID,
Condicao_Pagamento_CT_ID,
Recebimento_CT_Numero_Nota_Fiscal,
Recebimento_CT_Data_Ultima_Alteracao,
Recebimento_CT_Valor_Abatimento,
Recebimento_CT_Valor_Total
FROM Recebimento_CT 
JOIN Lojas
ON Loja_ID = lojas.Lojas_Id
WHERE
Recebimento_CT_Numero_Nota_Fiscal = '91296'
ORDER BY Recebimento_CT_ID DESC

SELECT TOP (50)
Recebimento_CT_ID,
Loja_ID,
Lojas.Lojas_NM,
Condicao_Pagamento_CT_ID,
Recebimento_CT_Numero_Nota_Fiscal, 
Recebimento_CT_Data_Emissao
FROM Recebimento_CT 
JOIN Lojas
ON Loja_ID = lojas.Lojas_Id
WHERE
Recebimento_CT_Observacao LIKE '%duplicata%'
AND Enum_Status_ID IN (189, 1636, 1760)
ORDER BY Recebimento_CT_ID DESC

SELECT TOP(10) * FROM NFE_Entrada_XML ORDER BY NFe_Entrada_XML_ID DESC -- Consulta XML
SELECT TOP(10) * FROM NFE_Entrada_XML WHERE NFe_Entrada_XML_ID = 411837
SELECT TOP(10) * FROM NFE_Entrada_XML WHERE NFe_Entrada_XML_Natureza_Operacao LIKE '%duplicata%'
SELECT * FROM NFE_Entrada_XML WHERE NFe_Entrada_XML_Numero = 91296

SELECT * FROM Pedido_Compra_CT 
WHERE Pedido_Compra_CT_ID = 265323 --219780
--Objeto_Origem_ID = 439313
ORDER BY Pedido_Compra_CT_ID DESC

/*
SELECT * INTO #TEMP FROM Pedido_Compra_CT;

ALTER TABLE #TEMP DROP COLUMN Pedido_Compra_CT_ID;

INSERT INTO [MCAR_Desenvolvimento].dbo.Pedido_Compra_CT 
SELECT * FROM #TEMP WHERE Pedido_Compra_CT_ID = 265323
DROP TABLE #TEMP

ALTER TABLE Recebimento_CT ADD Recebimento_CT_Valor_Abatimento decimal(18,2) NULL;

ALTER TABLE Recebimento_CT DROP COLUMN Recebimento_CT_Valor_Abatimento;

DELETE FROM NFE_Entrada_XML WHERE NFe_Entrada_XML_Numero = 91296
DELETE FROM Recebimento_CT WHERE Recebimento_CT_Numero_Nota_Fiscal = '91296'
DELETE FROM NFE_Entrada_XML WHERE NFe_Entrada_XML_ID IN
(119620)

sp p_CD_Consultar_Nota_Fiscal_Propriedades_FT_202675


*/
