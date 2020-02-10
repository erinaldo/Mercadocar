USE MCAR_Desenvolvimento
GO

SELECT
	Solicitacao_Pagamento.Solicitacao_Pagamento_ID                                     AS Solicitacao_Pagamento_ID,
	Solicitacao_Pagamento.Lojas_ID                                                     AS Lojas_ID,
	Lojas.Lojas_NM                                                                     AS Lojas_NM,
	Solicitacao_Pagamento.Objeto_Origem_ID                                             AS Objeto_Origem_ID,
	Romaneio_Venda_CT.Romaneio_Venda_CT_ID                                             AS Romaneio_CT_ID,
	Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID                                          AS Romaneio_Grupo_ID,
	Solicitacao_Pagamento.Enum_Tipo_Pagamento_ID                                       AS Enum_Tipo_Pagamento_ID,
	Solicitacao_Pagamento.Enum_Origem_ID                                               AS Enum_Origem_ID,
	Pagamento_Origem.Enum_Extenso                                                      AS Origem_Descricao,
	Solicitacao_Pagamento.Enum_Status_ID                                               AS Enum_Status_ID,
	Pagamento_Status.Enum_Extenso                                                      AS Status_Descricao,
	Solicitacao_Pagamento.Enum_Motivo_Recusa_Pagamento_ID                              AS Enum_Motivo_Recusa_Pagamento_ID,
	Solicitacao_Pagamento.Banco_ID                                                     AS Banco_ID,
	Banco.Banco_CD                                                                     AS Banco_CD,
	Banco.Banco_Nome                                                                   AS Banco_Nome,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Banco_Agencia                          AS Solicitacao_Pagamento_Banco_Agencia,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Banco_Conta                            AS Solicitacao_Pagamento_Banco_Conta,
	Conta.Enum_Tipo_Conta_ID                                                           AS Banco_Conta_Tipo_ID,
	Conta_Tipo.Enum_Sigla                                                              AS Banco_Conta_Tipo_Sigla,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Valor                                  AS Solicitacao_Pagamento_Valor,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Credito_Online                         AS Solicitacao_Pagamento_Credito_Online,
	Solicitacao_Pagamento.Usuario_Criacao_ID                                           AS Usuario_Criacao_ID,
	Usuario_Criacao.Usuario_Nome_Completo                                              AS Solicitacao_Pagamento_Usuario_Criacao,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Criacao                           AS Solicitacao_Pagamento_Data_Criacao,
	Solicitacao_Pagamento.Usuario_Ultima_Alteracao_ID                                  AS Usuario_Ultima_Alteracao_ID,
	Usuario_Alteracao.Usuario_Nome_Completo                                            AS Solicitacao_Pagamento_Usuario_Ultima_Alteracao,
	Solicitacao_Pagamento_Data_Ultima_Alteracao										   AS Solicitacao_Pagamento_Data_Ultima_Alteracao,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Comprovante_Pgto                       AS Solicitacao_Pagamento_Comprovante_Pgto,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Comprovante_Estorno					   AS Solicitacao_Pagamento_Comprovante_Estorno,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Data_Pagamento                         AS Solicitacao_Pagamento_Data_Pagamento,
	Solicitacao_Pagamento.Solicitacao_Pagamento_Obs                                    AS Solicitacao_Pagamento_Obs,
	CAST(Cliente.Cliente_CD AS VARCHAR(10))                                            AS Cliente_CD,
	CAST(Cliente.Cliente_Nome AS VARCHAR(200))                                         AS Cliente_Nome,
	Cliente.Cliente_CPF_CNPJ                                                           AS Cliente_CPFCNPJ,
	ContatoVirtual.ContatoVirtual_Email                                                AS Cliente_Email,
	CONCAT(Contato.Contato_DDD, Contato.Contato_Telefone)                              AS Cliente_Telefone,
	Romaneio_Venda_CT.Enum_Tipo_ID                                                     AS Cliente_Forma_Pagamento_Tipo_ID,
	Pagamento_Tipo.Enum_Extenso                                                        AS Cliente_Forma_Pagamento,
	Romaneio_Venda_CT.Romaneio_Venda_CT_Data_Liberacao                                 AS Cliente_Forma_Pagamento_Solicitacao_Data
FROM Solicitacao_Pagamento
	Join Lojas ON Solicitacao_Pagamento.Lojas_ID = Lojas.Lojas_Id
	Join Romaneio_Venda_CT ON Romaneio_Venda_CT.Lojas_ID = Solicitacao_Pagamento.Lojas_ID 
	AND Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID = Solicitacao_Pagamento.Objeto_Origem_ID
	--Join Romaneio_Venda_Pagamento ON Romaneio_Venda_CT.Romaneio_Venda_Grupo_ID = Romaneio_Venda_Pagamento.Romaneio_Venda_Grupo_ID
	--Join vw_Condicao_Pagamento ON Romaneio_Venda_Pagamento.Condicao_Pagamento_ID = vw_Condicao_Pagamento.Condicao_Pagamento_ID
	Join Cliente ON Romaneio_Venda_CT.Cliente_ID = Cliente.Cliente_ID
	Join Usuario AS Usuario_Criacao ON Solicitacao_Pagamento.Usuario_Criacao_ID = Usuario_Criacao.Usuario_ID
	LEFT Join Usuario AS Usuario_Alteracao ON Solicitacao_Pagamento.Usuario_Ultima_Alteracao_ID = Usuario_Alteracao.Usuario_ID
	Join Contato ON Cliente.Pessoa_ID = Contato.Pessoa_ID
	Join ContatoVirtual ON Cliente.Pessoa_ID = ContatoVirtual.Pessoa_ID
	Join Banco ON Solicitacao_Pagamento.Banco_ID = Banco.Banco_ID
	Join Conta ON Cliente.Pessoa_ID = Conta.Pessoa_ID AND Conta.Conta_Especial = 1
	Join Enumerado AS Pagamento_Origem ON Solicitacao_Pagamento.Enum_Origem_ID = Pagamento_Origem.Enum_ID
	Join Enumerado AS Pagamento_Status ON Solicitacao_Pagamento.Enum_Status_ID = Pagamento_Status.Enum_ID
	Join Enumerado AS Pagamento_Tipo ON Romaneio_Venda_CT.Enum_Tipo_ID = Pagamento_Tipo.Enum_ID
	Join Enumerado AS Conta_Tipo ON Conta.Enum_Tipo_Conta_ID = Conta_Tipo.Enum_ID
Where Solicitacao_Pagamento.Solicitacao_Pagamento_ID = 6
	AND
	Contato.Contato_ID = (
		SELECT TOP 1 Contato_ID 
		FROM   Contato
		WHERE  Contato.Pessoa_ID = Cliente.Pessoa_ID
	  )
	AND
	ContatoVirtual.ContatoVirtual_ID = (
		SELECT TOP 1 ContatoVirtual_ID 
		FROM   ContatoVirtual
		WHERE  ContatoVirtual.Pessoa_ID = Cliente.Pessoa_ID
	  )

-------------------------------------------------------------------------------
/*
SELECT * FROM Romaneio_Venda_CT 
WHERE 
Romaneio_Pre_Venda_CT_ID IN(
987665197,
987665132,
987665194,
987665134
)
*/