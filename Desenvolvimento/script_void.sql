
SET ANSI_NULLS ON 
GO 
SET QUOTED_IDENTIFIER ON 
GO
-------------------------------------------------------------------------------
-- <summary>
--    Cria as pendências de validações
-- </summary>
-- <history>
--    [guisuzuki] - Criado      - 31/10/2019
--    Demanda Política de Créditos (202755 US03)
--    [fmoraes]   - Modificado  - 26/11/2019
--    Continuação da Demanda Política de Créditos (202755 US03)
-- </history>
-------------------------------------------------------------------------------
 
--CREATE PROCEDURE dbo.p_Jobs_Pendencias_Validacao
--AS
 
DECLARE 
        @Usuario_Sistema                               INT,
		@Enum_Tipo_Troca                               INT,
        @Enum_Tipo_Resta                               INT,
		@Enum_TipoTroca_FiltroPorPeca                  INT, 
        @Enum_ProcessoValidacao_CreditoDePeçaSemOrigem INT, 
        @Enum_ProcessoValidacao_DevolucaoEmDinheiro    INT, 
        @Enum_StatusValidacao_Pendente                 INT, 
        @Cargo_GerenteLoja                             INT,
        @ParametroSistema_CreditoPagoAcimaDe           NUMERIC(10, 4);
 
SET @Enum_Tipo_Troca                                   = 550;
SET @Enum_Tipo_Resta                                   = 797;
SET @Enum_TipoTroca_FiltroPorPeca                      = 2786;
SET @Enum_ProcessoValidacao_CreditoDePeçaSemOrigem     = 2787;
SET @Enum_ProcessoValidacao_DevolucaoEmDinheiro        = 2788;
SET @Enum_StatusValidacao_Pendente                     = 2789;
 
SELECT TOP 1 @Cargo_GerenteLoja = Cargo.Cargo_ID
FROM Cargo
WHERE Cargo.Cargo_Descricao LIKE 'Gerente de Loja';
 
SELECT TOP 1 @ParametroSistema_CreditoPagoAcimaDe = CONVERT(NUMERIC(10, 4), Parametros_Sistema.Parametros_Sistema_Valor)
FROM Parametros_Sistema
WHERE Parametros_Sistema.Parametros_Sistema_Tipo LIKE 'CREDITO_PAGO_ACIMA_DE'
        AND Parametros_Sistema.Lojas_ID = 1;

SET NOCOUNT ON;
 
-------------------------------------------------------------------------------
-- Crédito de peça sem origem
-------------------------------------------------------------------------------
 /*
INSERT INTO Pendencia_Validacao
(Lojas_ID, 
    Objeto_Origem_ID, 
    Cargo_ID, 
    Enum_Processo_ID, 
    Enum_Status_ID, 
    Usuario_Geracao_ID, 
    Pendencia_Validacao_Data_Geracao, 
    Pendencia_Validacao_Valor
)
SELECT Romaneio_Venda_CT.Lojas_ID, 
        Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID, 
        @Cargo_GerenteLoja, 
        @Enum_ProcessoValidacao_CreditoDePeçaSemOrigem, 
        @Enum_StatusValidacao_Pendente, 
        Romaneio_Venda_CT.Usuario_Gerente_ID, 
        Romaneio_Venda_CT.Romaneio_Venda_CT_Data_Geracao, 
        Romaneio_Venda_CT.Romaneio_Venda_CT_Valor_Pago
FROM Romaneio_Venda_CT
WHERE Romaneio_Venda_CT.Enum_Tipo_Troca_ID = @Enum_TipoTroca_FiltroPorPeca
        AND NOT EXISTS
(
    SELECT 1
    FROM Pendencia_Validacao
    WHERE Pendencia_Validacao.Objeto_Origem_ID = Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID
            AND Pendencia_Validacao.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
            AND Pendencia_Validacao.Enum_Processo_ID IN(@Enum_ProcessoValidacao_CreditoDePeçaSemOrigem, @Enum_ProcessoValidacao_DevolucaoEmDinheiro)
); 
 
-------------------------------------------------------------------------------
-- Créditos pagos em dinheiro acima de 500 reais
-------------------------------------------------------------------------------
 
INSERT INTO Pendencia_Validacao
(Lojas_ID, 
    Objeto_Origem_ID, 
    Cargo_ID, 
    Enum_Processo_ID, 
    Enum_Status_ID, 
    Usuario_Geracao_ID, 
    Pendencia_Validacao_Data_Geracao, 
    Pendencia_Validacao_Valor
)*/
SELECT Romaneio_Venda_CT.Lojas_ID, 
        Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID, 
        @Cargo_GerenteLoja, 
        @Enum_ProcessoValidacao_DevolucaoEmDinheiro, 
        @Enum_StatusValidacao_Pendente, 
        Romaneio_Venda_CT.Usuario_Aprovacao_Credito_ID, 
        Romaneio_Venda_CT.Romaneio_Venda_CT_Data_Aprovacao_Credito, 
        Romaneio_Venda_CT.Romaneio_Venda_CT_Valor_Pago
FROM Romaneio_Venda_CT
WHERE Enum_Tipo_ID IN(@Enum_Tipo_Troca, @Enum_Tipo_Resta)
AND Romaneio_Venda_CT_Valor_Pago < @ParametroSistema_CreditoPagoAcimaDe *-1
AND Usuario_Aprovacao_Credito_ID IS NOT NULL
AND NOT EXISTS
(
    SELECT 1
    FROM Pendencia_Validacao
    WHERE Pendencia_Validacao.Objeto_Origem_ID = Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID
            AND Pendencia_Validacao.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
            AND Pendencia_Validacao.Enum_Processo_ID IN(@Enum_ProcessoValidacao_CreditoDePeçaSemOrigem, @Enum_ProcessoValidacao_DevolucaoEmDinheiro)
); 

SET NOCOUNT OFF
