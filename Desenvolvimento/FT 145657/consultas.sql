--------------------------------------------------------------------------

-- FT 145657
-- Gravar log ao tentar liberar comanda com romaneio cancelado

--------------------------------------------------------------------------

-- Para selecionar Comandas Internas
/*
SELECT * FROM Comanda_Interna WHERE Lojas_ID = 1 AND Comanda_Interna_ID = 100011
SELECT * FROM Comanda_Externa WHERE Lojas_ID = 1 AND Comanda_Externa_ID = 200001
SELECT * FROM Enumerado WHERE Enum_ID = 918

SELECT 
Comanda_Externa_ID, 
Lojas_ID, 
Romaneio_CT_ID, 
Comanda_Externa_Sequencia 
FROM Comanda_Externa_Romaneio
WHERE Lojas_ID = 1 AND Comanda_Externa_ID = 200001 AND Comanda_Externa_Sequencia = 4;

SELECT TOP(5) * FROM Romaneio_Cancelado_Ct 
--WHERE Romaneio_Pre_Venda_Ct_ID = 987664899 
ORDER BY Romaneio_Cancelado_Ct_ID DESC
*/

--SELECT * FROM vw_Peca_Ativa_e_Visualiza WHERE Produto_DS = 'Aplique Cromado' AND Peca_ID = 149235

------ CRIAR TABELA DE LOG --------

/*
CREATE TABLE dbo.Caixa_Log
(
     Caixa_Log_ID                   INT IDENTITY(1, 1) PRIMARY KEY NOT NULL, 
	 Romaneio_Pre_Venda_Ct_ID       INT NOT NULL, 
     Lojas_ID                       INT NOT NULL, 
     Caixa_Log_Data                 DATETIME NOT NULL,
	 Caixa_Log_Nome_Computador VARCHAR(30) NOT NULL
);
*/

--DROP TABLE dbo.Caixa_Log

--SELECT * FROM Caixa_Log


----  ALTERACAO PARA O ESCOPO  -----

--SELECT * FROM Enumerado WHERE Enum_Nome = 'Historico_Operacao'

/*
SELECT 
Lojas_ID, 
Enum_Processo_ID, 
Enum_Operacao_ID,
Enum_Tipo_Objeto_Origem_ID,
Objeto_Origem_ID,
Usuario_ID, 
Historico_Processo_Data, 
Historico_Processo_Estacao_Nome 
FROM Historico_Processo 
WHERE 
Lojas_ID = 1 
AND Objeto_Origem_ID = 10013568
*/

/*
DELETE FROM Historico_Processo WHERE Lojas_ID = 1 
AND Objeto_Origem_ID = 10013569
AND Historico_Processo_Data >= '2019-10-11'
*/

SELECT * FROM Historico_Processo WHERE Enum_Operacao_ID = 2432

SELECT TOP (5) * FROM Historico_Processo WHERE Enum_Operacao_ID = 2280 ORDER BY Historico_Processo_Data DESC

select * from Historico_Processo where Historico_Processo_Data >= '2019-10-14'

