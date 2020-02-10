-------------------------------------------------------------------------------
--  fmoraes 11/11/2019
--  CH 203974
--  Tela de Acompanhamento Produto de Alto Risco
-------------------------------------------------------------------------------
DECLARE @Objeto_Origem INT = 10016737;

/*
SELECT COUNT(*) FROM Romaneio_Cancelado_Ct (NOLOCK) WHERE Romaneio_Pre_Venda_Ct_ID = 10016737
SELECT TOP(5) * FROM Usuario WHERE Usuario_Login = 'amoura' AND Usuario_IsAtivo = 1 --6675
SELECT * FROM Peca WHERE Peca_DSTecnica LIKE '%Traseiro Quantum 85/90%'
*/

--EXEC p_Jobs_Gerar_Transferencia;

--SELECT * FROM Transferencia_CT WHERE Objeto_Origem_ID = @Objeto_Origem
--SELECT * FROM Romaneio_Cancelado_Ct (NOLOCK) WHERE Romaneio_Pre_Venda_Ct_ID = @Objeto_Origem
SELECT * FROM Produto_Alto_Risco_CT_Conferencia WHERE Objeto_Origem_ID = @Objeto_Origem
--SELECT * FROM Produto_Alto_Risco_CT_Conferencia WHERE Produto_Alto_Risco_Conferencia_ID = 3920

SELECT * FROM Romaneio_Venda_CT WHERE Romaneio_Pre_Venda_CT_ID = @Objeto_Origem --atualizado para a tabela nova
SELECT COUNT(*) FROM Romaneio_Venda_CT (NOLOCK) WHERE Romaneio_Pre_Venda_CT_ID = 10016737


--- Atualizar Estoque de Peça

SELECT * FROM SYS.TABLES WHERE name LIKE '%estoque%'

SELECT * FROM Peca WHERE Peca_CDFabricante = 'L27073'

--UPDATE Estoque SET Estoque_Qtde = 20 WHERE Loja_ID = 1 AND Peca_ID = 30469
--UPDATE Estoque_Reserva SET Estoque_Reserva_Qtde = 0 WHERE Lojas_ID = 1 AND Objeto_ID = 30469

SELECT * FROM Estoque WHERE Loja_ID = 1 AND Peca_ID = 30469
SELECT * FROM Estoque_Reserva WHERE Lojas_ID = 1 AND Objeto_ID = 30469