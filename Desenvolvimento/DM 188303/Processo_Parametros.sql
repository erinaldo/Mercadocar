DECLARE @Processo_Parametros_ID INT

INSERT INTO Processo_Parametros
(
	Processo_ID,
	Processo_Parametros_Nome,
	Processo_Parametros_Nome_Exibir,
	Processo_Parametros_Descricao,
	Processo_Parametros_Valor_Minimo,
	Processo_Parametros_Valor_Maximo,
	Processo_Parametros_Valor_Precisao,
	Processo_Parametros_Lista_Valores,
	Enum_Tipo_Dado_ID,
	Processo_Parametros_Valores_MultiSelecao,
	Processo_Parametros_IsAtivo)
SELECT
	43														Processo_ID,
	UPPER('Total_Tentativas_Digitacao_Quantidades_Separacao_Volume')		Processo_Parametros_Nome,
	'Total de Tentativas de Digitação de Quantidades'		Processo_Parametros_Nome_Exibir,
	'Total de Tentativas de Digitação de Quantidades. Na conferência cega na tela Separação do Volume para as lojas.'		Processo_Parametros_Descricao,
	0														Processo_Parametros_Valor_Minimo,
	3														Processo_Parametros_Valor_Maximo,
	0														Processo_Parametros_Valor_Precisao,
	NULL													Processo_Parametros_Lista_Valores,
	1056													Enum_Tipo_Dado_ID,
	0														Processo_Parametros_Valores_MultiSelecao,
	0														Processo_Parametros_IsAtivo


SELECT @Processo_Parametros_ID = SCOPE_IDENTITY()

INSERT INTO	Processo_Parametros_Valores
	(
	Processo_Parametros_ID,
	Lojas_ID,
	Usuario_Ultima_Alteracao_ID,
	Processo_Parametros_Valores_Valor,
	Processo_Parametros_Valores_Data_Ultima_Alteracao)
SELECT
	@Processo_Parametros_ID		Processo_Parametros_ID,
	Lojas_Id					Lojas_ID,
	0							Usuario_Ultima_Alteracao_ID,
	'3'							Processo_Parametros_Valores_Valor,
	GETDATE()					Processo_Parametros_Valores_Data_Ultima_Alteracao
FROM lojas 
where lojas_tipo = 'CD' 