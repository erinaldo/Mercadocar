
SET ANSI_NULLS ON 
GO 
SET QUOTED_IDENTIFIER ON 
GO
 
--/****** Object:  StoredProcedure [dbo].[p_Venda_Tecnica_Consultar_Romaneio_Venda_Grid]    Script Date: 27/08/2019 16:09:52 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
-------------------------------------------------------------------------------
-- <summary>
--		Consulta os romaneios de acordo com os filtros de pesquisa, para o 
--	preenchimento da tela.
-- </summary>
-- <history>
--     [msisiliani]		12/11/2015	Created
--     [msisiliani]		10/02/2016	Modified
--			Ajuste para a procedure consumir os dados da 'nova' estrutura de romaneios
--     [msisiliani]		19/02/2016	Modified
--			Inclusão da opção de visão da litagem por itens/ romaneios.
--     [rnoliveira]		13/09/2016	Modified
--			Retirado o parametro de CPFCNPJ pois quando é 
--			pesquisa por cpf é carregado a variavel de clienteid e existem 
--			situações onde existe o clienteid e não tem o cpfcnpj, 
--			alterado o @Cliente_ID para VARCHAR(50)
--			Na tabela temporaria carregar todos os campos para 
--			não precisar ir denovo na tabela de sistema para exibir os registros
--		[msisiliani]	13/09/2016	Modified 
--			Inclusão de condição no where relacionado ao filtro @Cliente_Tipo_Grupo_CT_ID
--		[bmune]			30/09/2016	Modified 
--			Retirado o JOIN com a tabela cliente
--		[rnoliveira]	30/09/2016	Modified 
--			Alterado a busca por numero de nota fiscal
--		[msisiliani]	30/09/2016	Modified 
--			Ordenação DESC pela coluna Romaneio_Venda_CT.Romaneio_Venda_CT_Data_Geracao
--		[rnoliveira]	03/10/2016	Modified 
--			Acrescentado a coluna de Transferencia_CT_ID
--			Alterar a ordem das colunas
--		[msisiliani]	04/10/2016	Modified 
--			Alteração do filtro de comanda
--		[mmukuno]		05/10/2016	Modified 
--			Alteração do filtro de data
--		[rnoliveira]	07/10/2016	Modified 
--			Acrescentado um grupo de filtros com cliente
--			e periodo		
--		[rnoliveira]	25/10/2016	Modified 
--			Retirado a tabela Interface_Fiscal_CT
--		[rnoliveira]	31/10/2016	Modified 
--			Alterado join pra trazer a encomenda
--		[rnoliveira]	03/11/2016	Modified 
--			Tratar parametros de cliente fora da consulta principal. 
--			Retira a condição OR das consultas de status e tipo
--		[marcardoso]	11/06/2018	Modified 
--			Retirando os parãmetros abaixo da procedure pornão usar mais na grid de consulta, também retirando o ELSEIF referente a esses parâmetros não utilzados mais
--                 Parâmetros retirados: @Cliente_ID , @Usar_Filtro_Cliente , @Enum_Tempo_Em_Dias_Para_Pesquisa_ID	, @Data_Personalizada_Inicial	, @Data_Personalizada_Final			
--		[vrici]			07/08/2018	Modified 
--			Correção para que os créditos não sejam exibidos caso não informe o CPF.
--		[marcardoso]	09/08/2018	Modified 
--			Incluindo a data de criação como parâmetro 
--          concatenando no campo observação as informações "motivo de cancelamento" concatenado o motivo pré-definido com as observações do motivo de cancelamento.
--          criando duas novas colunas Data de Cancelamento e Tempo de Cancelamento (Data de Geração até Cancelamento) 
--		[vrici]			02/10/2018	Modified 
--			Incluindo regra para créditos: Créditos deverão ser exibidos mesmo sem selecionar "mostrar crédito" se houver a inclusão do número do romaneio ou
--			CPF do cliente, em qualquer um dos status. Créditos liberados poderão ser consultados sem CPF e NÚMERO no caso de selecionada a opção "Mostrar Crédito"
--		[vrici]			04/10/2018	Modified 
--			Ignorar datas ao consultar créditos cancelados
--		[fmoraes]		01/07/2019	Modified 
--			Modificação realizadas pelo vrici que não haviam sido implementadas, alterando a verificação do Usuario_Gerente_Id e DELETE [Tmp] por DELETE [#Romaneio_Venda_Ct]
--          e na sua cláusula WHERE na verificação do Cliente_Tipo_Grupo_It.
--		[vrici]			27/08/2019	Modified 
--			Exibir créditos inutilizados mesmo sem número do romaneio e/ou CPF Cliente
--		[fmoraes]		12/12/2019	Modified 
--			Adcionado retorno [Romaneio_Venda_Ct].[Cliente_Id]
-- </history>
-------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[p_Venda_Tecnica_Consultar_Romaneio_Venda_Grid_DM_202755_US04](
	@Enum_Filtro_Tipo_Exibicao_Id INT,
	@Lojas_Id INT,
	@Enum_Tipo_Documento_Venda_Id INT,
	@Usuario_Vendedor_Id INT,
	@Usuario_Gerente_Id INT,
	@Cliente_Tipo_Grupo_Ct_Id INT,
	@Fabricante_Id INT,
	@Produto_Id INT,
	@Peca_Id INT,
	@Servico_Id INT,
	@Enum_Tipos_Romaneio [UDTGENERICO] READONLY,
	@Enum_Status_Romaneio_Venda [UDTGENERICO] READONLY,
	@Cliente_Id VARCHAR(50),
	@Cliente_Cpf_Cnpj VARCHAR(50),
	@Numero_Documento VARCHAR(50),
	@Data_Geracao_Inicial DATE,
	@Data_Geracao_Final DATE,
	@Data_Liberacao_Inicial DATE,
	@Data_Liberacao_Final DATE,
	@Data_Cancelamento_Inicial DATE,
	@Data_Cancelamento_Final DATE,
	@Exibir_Creditos BIT,
	@Enum_Tipo_Comanda_Id INT,
	@Numero_Comanda INT)
AS
 
--------------------------------------------------------------------------------------
     --DECLARE 
     --	@Enum_Filtro_Tipo_Exibicao_ID					INT = 0,
     --	@Lojas_ID										INT = 1,
     --	@Enum_Tipo_Documento_Venda_ID					INT = NULL,
     --	@Usuario_Vendedor_ID							INT = NULL,
     --	@Usuario_Gerente_ID								INT = NULL,
     --	@Cliente_Tipo_Grupo_CT_ID						INT = NULL,
     --	@Fabricante_ID									INT = NULL,
     --	@Produto_ID										INT = NULL,
     --	@Peca_ID										INT = NULL,  
     --	@Servico_ID										INT = NULL,
     --	@Numero_Documento								VARCHAR(50) = NULL,
     --	@Data_Geracao_Inicial							DATE = '2019-01-30 00:00:00',
     --	@Data_Geracao_Final								DATE = '2019-08-30 00:00:00',
     --	@Data_Liberacao_Inicial							DATE = NULL,
     --	@Data_Liberacao_Final							DATE = NULL,
     --	@Data_Cancelamento_Inicial						DATE = NULL,
     --	@Data_Cancelamento_Final						DATE = NULL,
     --	@Exibir_Creditos								BIT = 1,
     --	@Enum_Tipo_Comanda_ID							INT = NULL,
     --	@Numero_Comanda									INT = NULL, 
     --	@Usar_Filtro_Cliente							BIT = 0,
     --	@Cliente_ID										VARCHAR(50) = NULL --'6E340EC1-8297-4BB5-A006-2FA49F54B5F8'
 
     --DECLARE @Enum_Tipos_Romaneio						TABLE (Valor INT NULL)
     --DECLARE @Enum_Status_Romaneio_Venda				TABLE (Valor INT NULL)
 
     --INSERT INTO @Enum_Tipos_Romaneio (VALOR) VALUES 
     --(549),
     --(550),
     --(551),
     --(572),
     --(648),
     --(797)
 
     --INSERT INTO @Enum_Status_Romaneio_Venda (VALOR) VALUES 
     --(1397),
     --(1398),
     --(1399),
     --(1400),
     --(1401),
     --(1952)
     -------------------------------------------------------------------------------------
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SET NOCOUNT ON;
 
	DECLARE 
		@Enum_Filtro_Tipo_Exibicao_Romaneio_Id INT = 0,
		@Enum_Filtro_Tipo_Exibicao_Itens_Id INT = 1,
		@Enum_Objeto_Tipo_Peca_Id INT = 507,
		@Enum_Objeto_Tipo_Servico_Id INT = 508,
		@Enum_Objeto_Tipo_Encomenda_Id INT = 509,
		@Enum_Tipo_Romaneio_Troca_Id INT = 550,
		@Enum_Tipo_Romaneio_Licitacao_Id INT = 564,
		@Enum_Tipodocumento_Nota_Fiscal INT = 568,
		@Enum_Tipodocumento_Cupom_Fiscal INT = 569,
		@Enum_Tipo_Romaneio_Estorno_Id INT = 648, --crédito de estorno de cartão
		@Enum_Tipo_Romaneio_Resta_Id INT = 797, --crédito restante originado de outro crédito
		@Enum_Tipo_Documento_Venda_Romaneio_Id INT = 934,
		@Enum_Tipo_Documento_Venda_Grupo_Venda_Id INT = 935,
		@Enum_Tipo_Documento_Venda_Cupom_Fiscal_Id INT = 936,
		@Enum_Tipo_Documento_Venda_Nota_Fiscal_Id INT = 937,
		@Enum_Tipo_Transferencia_Transf_Ventec_Id INT = 1155,
		@Enum_Status_Romaneio_Venda_Pendente_Id INT = 1397,
		@Enum_Status_Romaneio_Venda_Liberado_Id INT = 1398,
		@Enum_Status_Romaneio_Venda_Cancelado_Id INT = 1399,
		@Enum_Status_Romaneio_Venda_Reativado_Id INT = 1400,
		@Enum_Status_Romaneio_Venda_Usado_Outra_Loja_Id INT = 1401,
		@Enum_Status_Romaneio_Venda_Grupo_Cancelado_Id INT = 1406,
		@Enum_Tipo_Origem_Interface_Fiscal_Grupo_Romaneio_Id INT = 1459,
		@Enum_Status_Romaneio_Venda_Inutilizado_Id INT = 1952,
		@Enum_Tipo_Comanda_Comanda_Interna INT = 2023,
		@Enum_Tipo_Comanda_Comanda_Externa INT = 2024,
		@Enum_Tempo_Em_Dias_Para_Pesquisa_Sete_Id INT = 2133,
		@Enum_Tempo_Em_Dias_Para_Pesquisa_Quinze_Id INT = 2134,
		@Enum_Tempo_Em_Dias_Para_Pesquisa_Trinta_Id INT = 2135,
		@Enum_Tempo_Em_Dias_Para_Pesquisa_Quarenta_Cinco INT = 2136,
		@Enum_Tempo_Em_Dias_Para_Pesquisa_Sessenta_Id INT = 2137,
		@Enum_Tempo_Em_Dias_Para_Pesquisa_Personalizado_Id INT = 2138;
 
     ----- Correção para que os créditos não sejam exibidos caso não informe o CPF.
     --	SET @Exibir_Creditos =
     --	(
     --		SELECT 
     --			CASE
     --				WHEN @Cliente_Id IS NULL THEN 0
     --				ELSE @Exibir_Creditos
     --			END
     --)
     ------------------------TABELA TEMPORARIA DE ROMANEIOS--------------------------------------------
 
	CREATE TABLE [#Romaneio_Venda_Ct](
		[Romaneio_Venda_Ct_Id] [BIGINT],
		[Lojas_Id] [INT] NOT NULL,
		[Cliente_Id] [UNIQUEIDENTIFIER] NOT NULL,
		[Enum_Tipo_Id] [INT] NOT NULL,
		[Enum_Status_Id] [INT] NOT NULL,
		[Condicao_Pagamento_Id] [INT] NOT NULL,
		[Usuario_Vendedor_Id] [INT] NOT NULL,
		[Usuario_Gerente_Id] [INT] NULL,
		[Usuario_Aprovacao_Cancelamento_Id] [INT] NULL,
		[Romaneio_Venda_Grupo_Id] [BIGINT] NULL,
		[Romaneio_Venda_Ct_Data_Geracao] [DATETIME] NOT NULL,
		[Romaneio_Venda_Ct_Valor_Pago] [DECIMAL](18,2) NOT NULL,
		[Romaneio_Venda_Ct_Valor_Lista] [DECIMAL](18,2) NULL,
		[Romaneio_Pre_Venda_Ct_Id] [INT] NULL,
		[Transferencias_Ct_Id] [VARCHAR](120) NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial] [VARCHAR](500) NULL,
		[Romaneio_Venda_Ct_Motivo_Troca] [VARCHAR](500) NULL,
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito] [VARCHAR](500) NULL,
		[Romaneio_Venda_Ct_Motivo_Cancelamento] [VARCHAR](500) NULL,
		[Romaneio_Venda_Ct_Data_Cancelamento] [DATETIME] NULL);
	CREATE TABLE [#Romaneios_Venda_Ct_Com_Comandas](
		[Romaneio_Venda_Ct_Id] [BIGINT] NOT NULL,
		[Lojas_Id] [INT] NOT NULL,
		[Comanda_Externa_Id] [BIGINT],
		[Comanda_Interna_Id] [BIGINT]);
 
     ------------------------TABELA TEMPORARIA DE ROMANEIOS--------------------------------------------
 
	IF(ISNULL(@Numero_Documento,'') <> '')
	BEGIN
 
             /*Pesquisa pelo número do documento (romaneio/ comandas/ grupo/ cupom/ NF)*/
 
	IF(@Enum_Tipo_Documento_Venda_Id = @Enum_Tipo_Documento_Venda_Romaneio_Id)
	BEGIN
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[Romaneio_Venda_Ct]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND ([Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id] = @Numero_Documento);
	END;
		ELSE
	BEGIN
	IF(@Enum_Tipo_Documento_Venda_Id = @Enum_Tipo_Documento_Venda_Grupo_Venda_Id)
	BEGIN
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[Romaneio_Venda_Ct]
	INNER JOIN [Romaneio_Venda_Grupo]
		ON
		   [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id]
		   AND [Romaneio_Venda_Grupo].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND ([Romaneio_Venda_Grupo].[Romaneio_Grupo_Id] = @Numero_Documento);
	END;
		ELSE
	BEGIN
	IF(@Enum_Tipo_Documento_Venda_Id = @Enum_Tipo_Documento_Venda_Nota_Fiscal_Id)
	BEGIN
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[Romaneio_Venda_Ct]
	INNER JOIN [Romaneio_Venda_Grupo]
		ON
		   [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id]
		   AND [Romaneio_Venda_Grupo].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Numero_Documento] = @Numero_Documento
		 AND [Romaneio_Venda_Grupo].[Enum_Documento_Tipo_Id] = @Enum_Tipodocumento_Nota_Fiscal;
	END;
		ELSE
	BEGIN
	IF(@Enum_Tipo_Documento_Venda_Id = @Enum_Tipo_Documento_Venda_Cupom_Fiscal_Id)
	BEGIN
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[Romaneio_Venda_Ct]
	INNER JOIN [Romaneio_Venda_Grupo]
		ON
		   [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id]
		   AND [Romaneio_Venda_Grupo].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND [Romaneio_Venda_Grupo].[Enum_Documento_Tipo_Id] = @Enum_Tipodocumento_Cupom_Fiscal
		 AND [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Numero_Documento] = @Numero_Documento;
	END;
	END;
	END;
	END;
	END;
		ELSE
	BEGIN
	IF(@Enum_Tipo_Comanda_Id IS NOT NULL)
	BEGIN
	IF(@Enum_Tipo_Comanda_Id = @Enum_Tipo_Comanda_Comanda_Externa)
	BEGIN
	INSERT INTO [#Romaneios_Venda_Ct_Com_Comandas]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Comanda_Externa_Romaneio].[Comanda_Externa_Id],
		NULL
	FROM 
		[Romaneio_Venda_Ct]
	INNER JOIN [Comanda_Externa_Romaneio]
		ON
		   [Comanda_Externa_Romaneio].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Comanda_Externa_Romaneio].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND [Comanda_Externa_Romaneio].[Comanda_Externa_Id] = @Numero_Comanda;
	END;
		ELSE
	BEGIN
	IF(@Enum_Tipo_Comanda_Id = @Enum_Tipo_Comanda_Comanda_Interna)
	BEGIN
	INSERT INTO [#Romaneios_Venda_Ct_Com_Comandas]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		NULL,
		[Comanda_Interna_Romaneio].[Comanda_Interna_Id]
	FROM 
		[Romaneio_Venda_Ct]
	INNER JOIN [Comanda_Interna_Romaneio]
		ON
		   [Comanda_Interna_Romaneio].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Comanda_Interna_Romaneio].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND [Comanda_Interna_Romaneio].[Comanda_Interna_Id] = @Numero_Comanda;
	END;
	END;
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[#Romaneios_Venda_Ct_Com_Comandas] AS [Romaneios_Venda_Ct_Com_Comandas]
	INNER JOIN [Romaneio_Venda_Ct]
		ON
		   [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id] = [Romaneios_Venda_Ct_Com_Comandas].[Romaneio_Venda_Ct_Id]
		   AND [Romaneio_Venda_Ct].[Lojas_Id] = [Romaneios_Venda_Ct_Com_Comandas].[Lojas_Id]
	LEFT JOIN [Romaneio_Venda_It]
		ON
		   [Romaneio_Venda_It].[Romaneio_Venda_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id]
		   AND [Romaneio_Venda_It].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Encomenda_Venda_It]
		ON
		   [Encomenda_Venda_It].[Encomenda_Venda_It_Id] = [Romaneio_Venda_It].[Objeto_Id]
		   AND [Encomenda_Venda_It].[Lojas_Id] = [Romaneio_Venda_It].[Lojas_Id]
	LEFT JOIN [Peca]
		ON
		   [Romaneio_Venda_It].[Objeto_Id] = [Peca].[Peca_Id]
		   AND [Romaneio_Venda_It].[Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Peca_Id
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND ([Romaneio_Venda_Ct].[Enum_Tipo_Id] IN
	(
		SELECT 
			[Valor]
		FROM 
			@Enum_Tipos_Romaneio
	))
		 AND ([Romaneio_Venda_Ct].[Enum_Status_Id] IN
	(
		SELECT 
			[Valor]
		FROM 
			@Enum_Status_Romaneio_Venda
	))
		 AND (
			 (
			 [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] BETWEEN @Data_Geracao_Inicial AND DATEADD(DAY,1,@Data_Geracao_Final)
			 OR @Data_Geracao_Inicial IS NULL)
			 OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
		 OR (
			@Cliente_Id IS NOT NULL
			AND @Exibir_Creditos = 1
			AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		(
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Liberacao] BETWEEN @Data_Liberacao_Inicial AND DATEADD(DAY,1,@Data_Liberacao_Final)
		OR @Data_Liberacao_Inicial IS NULL)
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
	OR (
	   @Cliente_Id IS NOT NULL
	   AND @Exibir_Creditos = 1
	   AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		(
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Cancelamento] BETWEEN @Data_Cancelamento_Inicial AND DATEADD(DAY,1,@Data_Cancelamento_Final)
		OR @Data_Cancelamento_Inicial IS NULL)
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
	OR (
	   @Cliente_Id IS NOT NULL
	   AND @Exibir_Creditos = 1
	   AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id] = @Usuario_Vendedor_Id
		OR @Usuario_Vendedor_Id IS NULL)
	AND (
		[Romaneio_Venda_Ct].[Enum_Status_Id] = 1399
		AND [Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id] = @Usuario_Gerente_Id
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] <> 1399
		AND [Romaneio_Venda_Ct].[Usuario_Gerente_Id] = @Usuario_Gerente_Id
		OR @Usuario_Gerente_Id IS NULL)
	AND (
		(
		[Romaneio_Venda_It].[Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Servico_Id
		AND [Romaneio_Venda_It].[Objeto_Id] = @Servico_Id)
		OR @Servico_Id IS NULL)
	AND (
		(
		(
		[Peca].[Fabricante_Id] = @Fabricante_Id
		OR @Fabricante_Id IS NULL)
		AND (
			[Peca].[Produto_Id] = @Produto_Id
			OR @Produto_Id IS NULL)
		AND (
			[Peca].[Peca_Id] = @Peca_Id
			OR @Peca_Id IS NULL))
		OR (
		   (
		   [Encomenda_Venda_It].[Fabricante_Id] = @Fabricante_Id
		   OR @Fabricante_Id IS NULL)
		   AND (
			   [Encomenda_Venda_It].[Produto_Id] = @Produto_Id
			   OR @Produto_Id IS NULL)
		   AND (
			   [Encomenda_Venda_It].[Peca_Id] = @Peca_Id
			   OR @Peca_Id IS NULL)))
	AND (
		(([Romaneio_Venda_Ct].[Enum_Tipo_Id] NOT IN(@Enum_Tipo_Romaneio_Troca_Id,@Enum_Tipo_Romaneio_Estorno_Id,@Enum_Tipo_Romaneio_Resta_Id,@Enum_Tipo_Romaneio_Licitacao_Id)))
	OR (
	   @Exibir_Creditos = 1
	   AND (
		   @Cliente_Id IS NOT NULL
		   OR Enum_Status_Id IN   (@Enum_Status_Romaneio_Venda_Liberado_Id, @Enum_Status_Romaneio_Venda_Inutilizado_Id ) )));
	END;
		ELSE
	BEGIN
	IF(
	  @Fabricante_Id IS NULL
	  AND @Produto_Id IS NULL
	  AND @Peca_Id IS NULL
	  AND @Servico_Id IS NULL)
	BEGIN
 
                             /*Pesquisa no filtro geral sem considerar a peça/ serviço*/
 
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[Romaneio_Venda_Ct]
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND ([Romaneio_Venda_Ct].[Enum_Tipo_Id] IN
	(
		SELECT 
			[Valor]
		FROM 
			@Enum_Tipos_Romaneio
	))
		 AND ([Romaneio_Venda_Ct].[Enum_Status_Id] IN
	(
		SELECT 
			[Valor]
		FROM 
			@Enum_Status_Romaneio_Venda
	))
		 AND (
			 (
			 [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] BETWEEN @Data_Geracao_Inicial AND DATEADD(DAY,1,@Data_Geracao_Final)
			 OR @Data_Geracao_Inicial IS NULL)
			 OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
		 OR (
			@Cliente_Id IS NOT NULL
			AND @Exibir_Creditos = 1
			AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		(
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Liberacao] BETWEEN @Data_Liberacao_Inicial AND DATEADD(DAY,1,@Data_Liberacao_Final)
		OR @Data_Liberacao_Inicial IS NULL)
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
	OR (
	   @Cliente_Id IS NOT NULL
	   AND @Exibir_Creditos = 1
	   AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		(
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Cancelamento] BETWEEN @Data_Cancelamento_Inicial AND DATEADD(DAY,1,@Data_Cancelamento_Final)
		OR @Data_Cancelamento_Inicial IS NULL)
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
	OR (
	   @Cliente_Id IS NOT NULL
	   AND @Exibir_Creditos = 1
	   AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id] = @Usuario_Vendedor_Id
		OR @Usuario_Vendedor_Id IS NULL)
	AND (
		[Romaneio_Venda_Ct].[Enum_Status_Id] = 1399
		AND [Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id] = @Usuario_Gerente_Id
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] <> 1399
		AND [Romaneio_Venda_Ct].[Usuario_Gerente_Id] = @Usuario_Gerente_Id
		OR @Usuario_Gerente_Id IS NULL)
	AND (
		(([Romaneio_Venda_Ct].[Enum_Tipo_Id] NOT IN(@Enum_Tipo_Romaneio_Troca_Id,@Enum_Tipo_Romaneio_Estorno_Id,@Enum_Tipo_Romaneio_Resta_Id,@Enum_Tipo_Romaneio_Licitacao_Id)))
	OR (
	   @Exibir_Creditos = 1
	   AND (
		   @Cliente_Id IS NOT NULL
		   OR Enum_Status_Id IN   (@Enum_Status_Romaneio_Venda_Liberado_Id, @Enum_Status_Romaneio_Venda_Inutilizado_Id))));
	END;
		ELSE
	BEGIN
 
                             /*Pesquisa por todos os filtros*/
 
	INSERT INTO [#Romaneio_Venda_Ct]
	SELECT 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Ct].[Lojas_Id],
		[Romaneio_Venda_Ct].[Cliente_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id],
		[Romaneio_Venda_Ct].[Enum_Status_Id],
		[Romaneio_Venda_Ct].[Condicao_Pagamento_Id],
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id],
		[Romaneio_Venda_Ct].[Usuario_Gerente_Id],
		[Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],
		NULL,
		[Romaneio_Venda_Ct_Motivo_Venda_Especial],
		[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],
		[Romaneio_Venda_Ct_Motivo_Cancelamento],
		[Romaneio_Venda_Ct_Data_Cancelamento]
	FROM 
		[Romaneio_Venda_Ct]
	LEFT JOIN [Romaneio_Venda_It]
		ON
		   [Romaneio_Venda_It].[Romaneio_Venda_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id]
		   AND [Romaneio_Venda_It].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Encomenda_Venda_It]
		ON
		   [Encomenda_Venda_It].[Encomenda_Venda_It_Id] = [Romaneio_Venda_It].[Objeto_Id]
		   AND [Encomenda_Venda_It].[Lojas_Id] = [Romaneio_Venda_It].[Lojas_Id]
	LEFT JOIN [Peca]
		ON
		   [Romaneio_Venda_It].[Objeto_Id] = [Peca].[Peca_Id]
		   AND [Romaneio_Venda_It].[Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Peca_Id
	WHERE
		 (
		 [Romaneio_Venda_Ct].[Lojas_Id] = @Lojas_Id
		 OR @Lojas_Id IS NULL)
		 AND ([Romaneio_Venda_Ct].[Enum_Tipo_Id] IN
	(
		SELECT 
			[Valor]
		FROM 
			@Enum_Tipos_Romaneio
	))
		 AND ([Romaneio_Venda_Ct].[Enum_Status_Id] IN
	(
		SELECT 
			[Valor]
		FROM 
			@Enum_Status_Romaneio_Venda
	))
		 AND (
			 (
			 [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] BETWEEN @Data_Geracao_Inicial AND DATEADD(DAY,1,@Data_Geracao_Final)
			 OR @Data_Geracao_Inicial IS NULL)
			 OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
		 OR (
			@Cliente_Id IS NOT NULL
			AND @Exibir_Creditos = 1
			AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		(
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Liberacao] BETWEEN @Data_Liberacao_Inicial AND DATEADD(DAY,1,@Data_Liberacao_Final)
		OR @Data_Liberacao_Inicial IS NULL)
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
	OR (
	   @Cliente_Id IS NOT NULL
	   AND @Exibir_Creditos = 1
	   AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		(
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Cancelamento] BETWEEN @Data_Cancelamento_Inicial AND DATEADD(DAY,1,@Data_Cancelamento_Final)
		OR @Data_Cancelamento_Inicial IS NULL)
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] IN(@Enum_Status_Romaneio_Venda_Pendente_Id,@Enum_Status_Romaneio_Venda_Reativado_Id)
	OR (
	   @Cliente_Id IS NOT NULL
	   AND @Exibir_Creditos = 1
	   AND [Romaneio_Venda_Ct].Enum_Status_Id = 1399))
	AND (
		[Romaneio_Venda_Ct].[Usuario_Vendedor_Id] = @Usuario_Vendedor_Id
		OR @Usuario_Vendedor_Id IS NULL)
	AND (
		[Romaneio_Venda_Ct].[Enum_Status_Id] = 1399
		AND [Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id] = @Usuario_Gerente_Id
		OR [Romaneio_Venda_Ct].[Enum_Status_Id] <> 1399
		AND [Romaneio_Venda_Ct].[Usuario_Gerente_Id] = @Usuario_Gerente_Id
		OR @Usuario_Gerente_Id IS NULL)
	AND (
		(
		[Romaneio_Venda_It].[Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Servico_Id
		AND [Romaneio_Venda_It].[Objeto_Id] = @Servico_Id)
		OR @Servico_Id IS NULL)
	AND (
		(
		(
		[Peca].[Fabricante_Id] = @Fabricante_Id
		OR @Fabricante_Id IS NULL)
		AND (
			[Peca].[Produto_Id] = @Produto_Id
			OR @Produto_Id IS NULL)
		AND (
			[Peca].[Peca_Id] = @Peca_Id
			OR @Peca_Id IS NULL))
		OR (
		   (
		   [Encomenda_Venda_It].[Fabricante_Id] = @Fabricante_Id
		   OR @Fabricante_Id IS NULL)
		   AND (
			   [Encomenda_Venda_It].[Produto_Id] = @Produto_Id
			   OR @Produto_Id IS NULL)
		   AND (
			   [Encomenda_Venda_It].[Peca_Id] = @Peca_Id
			   OR @Peca_Id IS NULL)))
	AND (
		(([Romaneio_Venda_Ct].[Enum_Tipo_Id] NOT IN(@Enum_Tipo_Romaneio_Troca_Id,@Enum_Tipo_Romaneio_Estorno_Id,@Enum_Tipo_Romaneio_Resta_Id,@Enum_Tipo_Romaneio_Licitacao_Id)))
	OR (
	   @Exibir_Creditos = 1
	   AND (
		   @Cliente_Id IS NOT NULL
		   OR Enum_Status_Id IN   (@Enum_Status_Romaneio_Venda_Liberado_Id, @Enum_Status_Romaneio_Venda_Inutilizado_Id))));
	END;
	END;
	END;
 
     -------------------------Verificar variavel de @Cliente_ID se esta preenchida para aplicar a validação-------------------------------------  
	IF(@Cliente_Id IS NOT NULL)
	BEGIN
	DELETE [#Romaneio_Venda_Ct]
	WHERE 
		[Cliente_Id] <> @Cliente_Id;
	END;   
     -------------------------Verificar variavel de @Cliente_ID se esta preenchida para aplicar a validação-------------------------------------  
     -------------------------Verificar variavel de @Cliente_Tipo_Grupo_CT_ID se esta preenchida para aplicar a validação-------------------------------------
	IF(@Cliente_Tipo_Grupo_Ct_Id IS NOT NULL)
	BEGIN
	DELETE [#Romaneio_Venda_Ct]
	WHERE 
		[#Romaneio_Venda_Ct].Cliente_Id NOT IN(SELECT 
												   [Tmp].Cliente_Id
											   FROM 
												   [#Romaneio_Venda_Ct] AS [Tmp]
											   INNER JOIN [Cliente_Detalhes]
												   ON [Cliente_Detalhes].[Cliente_Id] = [Tmp].[Cliente_Id]
											   LEFT JOIN [Cliente_Tipo_Grupo_It]
												   ON [Cliente_Tipo_Grupo_It].[Cliente_Tipo_Id] = [Cliente_Detalhes].[Cliente_Tipo_Id]
											   WHERE ISNULL([Cliente_Tipo_Grupo_It].[Cliente_Tipo_Grupo_Ct_Id],0) = @Cliente_Tipo_Grupo_Ct_Id);
	END;
     -------------------------Verificar variavel de @Cliente_Tipo_Grupo_CT_ID se esta preenchida para aplicar a validação-------------------------------------
     -------------------------Atualizar o campo Transferencias_CT_ID para Romaneios que tenham vinculo com Transferencia--------------------
	UPDATE [#Romaneio_Venda_Ct]
		   SET [Transferencias_Ct_Id] = [Dbo].[Fun_Venda_Tecnica_Concatena_Romaneios_Transferencia]([Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id],[Romaneio_Venda_Ct].[Lojas_Id])
	FROM [#Romaneio_Venda_Ct] [Romaneio_Venda_Ct]
	INNER JOIN [Transferencia_Ct]
		ON
		   [Transferencia_Ct].[Objeto_Origem_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Transferencia_Ct].[Loja_Destino_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
		   AND [Transferencia_Ct].[Enum_Tipo_Transferencia_Id] = @Enum_Tipo_Transferencia_Transf_Ventec_Id;
     -------------------------Atualizar o campo Transferencias_CT_ID para Romaneios que tenham vinculo com Transferencia--------------------
 
	IF(@Enum_Filtro_Tipo_Exibicao_Id = @Enum_Filtro_Tipo_Exibicao_Romaneio_Id)
	BEGIN
 
             /*#ROMANEIO*/
 
	SELECT DISTINCT 
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id] AS [Romaneio_Pre_Venda_Ct_Id],
		[Romaneio_Venda_Grupo].[Romaneio_Grupo_Id] AS [Romaneio_Grupo_Id],
		[Enumerado_Tipo].[Enum_Extenso] AS [Enumerado_Tipo_Extenso],
		[Enumerado_Status].[Enum_Extenso] AS [Enumerado_Status_Extenso],
		[Romaneio_Venda_Ct].[Cliente_ID] AS [Cliente_Id],
		[Dbo].[Fun_Retorna_Cliente_Nome]([Romaneio_Venda_Ct].[Cliente_Id]) AS [Nome_Cliente],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista] AS [Romaneio_Venda_Ct_Valor_Lista],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago] AS [Romaneio_Venda_Ct_Valor_Pago],
		CASE
			WHEN [Romaneio_Venda_Ct].[Enum_Tipo_Id] NOT IN(@Enum_Tipo_Romaneio_Troca_Id,@Enum_Tipo_Romaneio_Estorno_Id,@Enum_Tipo_Romaneio_Resta_Id) THEN 
			[Dbo].[Fun_Calcula_Percentual_Desconto]([Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Pago],
			[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Valor_Lista])
			ELSE 0
															  END AS [% Desc],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] AS [Romaneio_Venda_Ct_Data_Geracao],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Cancelamento] AS [Romaneio_Venda_Ct_Data_Cancelamento],
		[Dbo].[Fun_Retorna_Hora_Minuto](DATEDIFF(minute,[Romaneio_Venda_Ct_Data_Geracao],[Romaneio_Venda_Ct_Data_Cancelamento])) AS [Tempo_De_Cancelamento],
		[Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Data_Liberacao] AS [Data_Liberacao],
		[Usuario_Vendedor].[Usuario_Nome_Completo] AS [Usuario_Vendedor_Nome_Completo],
		[Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Numero_Documento] AS [Romaneio_Venda_Grupo_Numero_Documento],
		[Lojas].[Lojas_Nm] AS [Loja_Nome],
		[Usuario_Gerente].[Usuario_Nome_Completo] AS [Nome_Gerente],
		[Comanda_Interna_Romaneio].[Comanda_Interna_Id] AS [Comanda_Interna_Id],
		[Comanda_Externa_Romaneio].[Comanda_Externa_Id] AS [Comanda_Externa_Id],
		CASE
			WHEN ISNULL([Pacote_Venda_Ct].[Enum_Status_Id],0) <> 472 THEN CAST(0 AS BIT)
			ELSE CAST(1 AS BIT)
														   END AS [Romaneio_Conferido],
		CASE
			WHEN [Romaneio_Venda_Ct].[Enum_Status_Id] <> @Enum_Status_Romaneio_Venda_Cancelado_Id THEN '' COLLATE Sql_Latin1_General_Cp1_Ci_As
			ELSE [Enumerado_Cancelamento].[Enum_Extenso]+' - '
		END+COALESCE([Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Motivo_Venda_Especial],[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Motivo_Troca],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Motivo_Aprovacao_Credito],[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Motivo_Cancelamento]) AS [Motivo],
		[Usuario_Aprovacao_Cancelamento].[Usuario_Nome_Completo] AS [Usuario_Aprovacao_Cancelamento_Nome_Completo],
		[Romaneio_Venda_Ct].[Transferencias_Ct_Id] AS [Transferencia_Ct_Id],
		[Lojas].[Lojas_Id] AS [Lojas_Id],
		[Enumerado_Tipo].[Enum_Id] AS [Enumerado_Tipo_Id],
		[Enumerado_Status].[Enum_Id] AS [Enumerado_Status_Id],
		RTRIM(ISNULL([Forma_Pagamento].[Forma_Pagamento_Ds],'')) AS [Forma_Pagamento_Ds],
		[Usuario_Aprovacao_Cancelamento].[Usuario_Id] AS [Usuario_Aprovacao_Cancelamento_Id],
		1 AS [Aprova_Credito],
		[Encomenda_Venda_Ct].[Encomenda_Venda_Ct_Id] AS [Encomenda_Venda_Ct_Id],
		0 AS [Reativado],
		0 AS [Romaneio_Cancelado_Ct_Inutilizado],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id] AS [Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] AS [Romaneio_Venda_Grupo_Id]
	FROM 
		[#Romaneio_Venda_Ct] AS [Romaneio_Venda_Ct]
	LEFT JOIN [Romaneio_Venda_It]
		ON
		   [Romaneio_Venda_It].[Romaneio_Venda_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id]
		   AND [Romaneio_Venda_It].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	INNER JOIN [Enumerado] AS [Enumerado_Tipo]
		ON [Enumerado_Tipo].[Enum_Id] = [Romaneio_Venda_Ct].[Enum_Tipo_Id]
	INNER JOIN [Enumerado] AS [Enumerado_Status]
		ON [Enumerado_Status].[Enum_Id] = [Romaneio_Venda_Ct].[Enum_Status_Id]
	INNER JOIN [Lojas]
		ON [Lojas].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Romaneio_Venda_Grupo]
		ON
		   [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id]
		   AND [Romaneio_Venda_Grupo].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	INNER JOIN [Usuario] AS [Usuario_Vendedor]
		ON [Usuario_Vendedor].[Usuario_Id] = [Romaneio_Venda_Ct].[Usuario_Vendedor_Id]
	LEFT JOIN [Usuario] AS [Usuario_Gerente]
		ON [Usuario_Gerente].[Usuario_Id] = [Romaneio_Venda_Ct].[Usuario_Gerente_Id]
	LEFT JOIN [Usuario] AS [Usuario_Aprovacao_Cancelamento]
		ON [Usuario_Aprovacao_Cancelamento].[Usuario_Id] = [Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id]
	LEFT JOIN [Condicao_Pagamento]
		ON
		   [Romaneio_Venda_Ct].[Condicao_Pagamento_Id] = [Condicao_Pagamento].[Condicao_Pagamento_Id]
		   AND [Romaneio_Venda_Ct].[Lojas_Id] = [Condicao_Pagamento].[Loja_Id]
	LEFT JOIN [Forma_Pagamento]
		ON [Condicao_Pagamento].[Forma_Pagamento_Id] = [Forma_Pagamento].[Forma_Pagamento_Id]
	LEFT JOIN [Comanda_Interna_Romaneio]
		ON
		   [Comanda_Interna_Romaneio].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Comanda_Interna_Romaneio].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Comanda_Externa_Romaneio]
		ON
		   [Comanda_Externa_Romaneio].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Comanda_Externa_Romaneio].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Pacote_Venda_Ct]
		ON
		   [Pacote_Venda_Ct].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Pacote_Venda_Ct].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Encomenda_Venda_Ct]
		ON
		   [Encomenda_Venda_Ct].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Encomenda_Venda_Ct].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Romaneio_Cancelado_Ct]
		ON
		   [Romaneio_Cancelado_Ct].[Romaneio_Pre_Venda_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Romaneio_Cancelado_Ct].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Enumerado] AS [Enumerado_Cancelamento]
		ON [Enumerado_Cancelamento].[Enum_Id] = [Romaneio_Cancelado_Ct].[Enum_Motivo_Cancelamento_Romaneio_Id]
	ORDER BY 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] DESC;
	END;
		ELSE
	BEGIN
 
             /*#ITENS*/
 
	SELECT DISTINCT 
		[Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id] AS [Romaneio_Pre_Venda_Ct_Id],
		[Romaneio_Venda_Grupo].[Romaneio_Grupo_Id] AS [Romaneio_Grupo_Id],
		CASE
			WHEN [Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Peca_Id THEN [Vw_Peca].[Fabricante_Cd]+'.'+[Vw_Peca].[Produto_Cd]+'.'+[Vw_Peca].[Peca_Cd]
			WHEN [Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Servico_Id THEN [Servico].[Servico_Cd]
			WHEN [Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Encomenda_Id THEN CASE
																				 WHEN [Encomenda_Venda_It].[Encomenda_Venda_It_Id] IS NOT NULL THEN CONVERT(VARCHAR(10),[Encomenda_Venda_It].[Encomenda_Venda_It_Id])
																				 ELSE CONVERT(VARCHAR(10),[Objeto_Id])
																			 END
			ELSE CAST('' AS VARCHAR(1))
													  END AS [Codigo],
		[Enumerado_Tipo].[Enum_Extenso] AS [Enumerado_Tipo_Extenso],
		[Enumerado_Status].[Enum_Extenso] AS [Enumerado_Status_Extenso],
		[Romaneio_Venda_Ct].[Cliente_ID] AS [Cliente_Id],
		[Dbo].[Fun_Retorna_Cliente_Nome]([Romaneio_Venda_Ct].[Cliente_Id]) AS [Nome_Cliente],
		[Romaneio_Venda_It].[Romaneio_Venda_It_Preco_Lista] AS [Romaneio_Venda_It_Preco_Lista],
		[Romaneio_Venda_It].[Romaneio_Venda_It_Preco_Pago] AS [Romaneio_Venda_It_Preco_Pago],
		CASE
			WHEN [Romaneio_Venda_Ct].[Enum_Tipo_Id] NOT IN(@Enum_Tipo_Romaneio_Troca_Id,@Enum_Tipo_Romaneio_Estorno_Id,@Enum_Tipo_Romaneio_Resta_Id) THEN 
			[Dbo].[Fun_Calcula_Percentual_Desconto]([Romaneio_Venda_It].[Romaneio_Venda_It_Preco_Pago],
			[Romaneio_Venda_It].[Romaneio_Venda_It_Preco_Lista])
			ELSE 0
															  END AS [% Desc],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] AS [Data_Criacao],
		[Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Data_Liberacao] AS [Data_Liberacao],
		[Usuario_Vendedor].[Usuario_Nome_Completo] AS [Nome_Vendedor],
		[Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Numero_Documento] AS [Documento],
		[Lojas].[Lojas_Nm] AS [Loja_Nome],
		[Usuario_Gerente].[Usuario_Nome_Completo] AS [Nome_Gerente],
		[Comanda_Interna_Romaneio].[Comanda_Interna_Id] AS [Comanda_Interna_Id],
		[Comanda_Externa_Romaneio].[Comanda_Externa_Id] AS [Comanda_Externa_Id],
		[Romaneio_Venda_Ct].[Transferencias_Ct_Id] AS [Transferencia_Ct_Id],
		[Romaneio_Venda_Ct].[Enum_Tipo_Id] AS [Enum_Romaneio_Tipo_Id],
		[Usuario_Aprovacao_Cancelamento].[Usuario_Nome_Completo] AS [Usuario_Aprovacao_Cancelamento_Id],
		[Encomenda_Venda_Ct].[Encomenda_Venda_Ct_Id] AS [Encomenda_Venda_Ct_Id],
		[Enumerado_Tipo].[Enum_Id] AS [Enumerado_Tipo_Id],
		[Enumerado_Status].[Enum_Id] AS [Enumerado_Status_Id],
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id] AS [Romaneio_Venda_Ct_Id],
		[Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] AS [Romaneio_Venda_Grupo_Id],
		[Romaneio_Venda_Ct].[Lojas_Id] AS [Lojas_Id]
	FROM 
		[#Romaneio_Venda_Ct] AS [Romaneio_Venda_Ct]
	INNER JOIN [Enumerado] AS [Enumerado_Tipo]
		ON [Enumerado_Tipo].[Enum_Id] = [Romaneio_Venda_Ct].[Enum_Tipo_Id]
	INNER JOIN [Enumerado] AS [Enumerado_Status]
		ON [Enumerado_Status].[Enum_Id] = [Romaneio_Venda_Ct].[Enum_Status_Id]
	INNER JOIN [Lojas]
		ON [Lojas].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Romaneio_Venda_It]
		ON
		   [Romaneio_Venda_It].[Romaneio_Venda_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Id]
		   AND [Romaneio_Venda_It].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Romaneio_Venda_Grupo]
		ON
		   [Romaneio_Venda_Grupo].[Romaneio_Venda_Grupo_Id] = [Romaneio_Venda_Ct].[Romaneio_Venda_Grupo_Id]
		   AND [Romaneio_Venda_Grupo].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	INNER JOIN [Usuario] AS [Usuario_Vendedor]
		ON [Usuario_Vendedor].[Usuario_Id] = [Romaneio_Venda_Ct].[Usuario_Vendedor_Id]
	LEFT JOIN [Usuario] AS [Usuario_Gerente]
		ON [Usuario_Gerente].[Usuario_Id] = [Romaneio_Venda_Ct].[Usuario_Gerente_Id]
	LEFT JOIN [Usuario] AS [Usuario_Aprovacao_Cancelamento]
		ON [Usuario_Aprovacao_Cancelamento].[Usuario_Id] = [Romaneio_Venda_Ct].[Usuario_Aprovacao_Cancelamento_Id]
	LEFT JOIN [Encomenda_Venda_It]
		ON
		   [Encomenda_Venda_It].[Encomenda_Venda_It_Id] = [Romaneio_Venda_It].[Objeto_Id]
		   AND [Encomenda_Venda_It].[Lojas_Id] = [Romaneio_Venda_It].[Lojas_Id]
	LEFT JOIN [Vw_Peca]
		ON
		   [Vw_Peca].[Peca_Id] = [Romaneio_Venda_It].[Objeto_Id]
		   AND [Romaneio_Venda_It].[Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Peca_Id
	LEFT JOIN [Servico]
		ON
		   [Servico].[Servico_Id] = [Romaneio_Venda_It].[Objeto_Id]
		   AND [Romaneio_Venda_It].[Enum_Objeto_Tipo_Id] = @Enum_Objeto_Tipo_Servico_Id
	LEFT JOIN [Encomenda_Venda_Ct]
		ON
		   [Encomenda_Venda_Ct].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Encomenda_Venda_Ct].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Comanda_Interna_Romaneio]
		ON
		   [Comanda_Interna_Romaneio].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Comanda_Interna_Romaneio].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	LEFT JOIN [Comanda_Externa_Romaneio]
		ON
		   [Comanda_Externa_Romaneio].[Romaneio_Ct_Id] = [Romaneio_Venda_Ct].[Romaneio_Pre_Venda_Ct_Id]
		   AND [Comanda_Externa_Romaneio].[Lojas_Id] = [Romaneio_Venda_Ct].[Lojas_Id]
	ORDER BY 
		[Romaneio_Venda_Ct].[Romaneio_Venda_Ct_Data_Geracao] DESC;
	END;
	DROP TABLE [#Romaneio_Venda_Ct];
	DROP TABLE [#Romaneios_Venda_Ct_Com_Comandas];
	SET NOCOUNT OFF;
	
