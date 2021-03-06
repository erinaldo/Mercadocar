SET ANSI_NULLS ON; 
GO

SET QUOTED_IDENTIFIER ON; 
GO
-------------------------------------------------------------------------------
-- <summary>
--    Cria as pendências de validações
-- </summary>
-- <history>
--    [fmoraes] - Criado - 26/11/2019
-- </history>
-------------------------------------------------------------------------------

CREATE PROCEDURE dbo.p_Jobs_Pendencias_Validacao
AS
  BEGIN

    DECLARE 
         @Cargo_GerenteLoja                             INT, 
         @Enum_ProcessoValidacao_CreditoDePeçaSemOrigem INT, 
         @Enum_ProcessoValidacao_DevolucaoEmDinheiro    INT, 
         @Enum_StatusValidacao_Pendente                 INT, 
         @Enum_TipoTroca_FiltroPorPeca                  INT, 
         @ParametroSistema_CreditoPagoAcimaDe           NUMERIC(10, 4), 
         @Usuario_Sistema                               INT;

    SELECT TOP 1 @Cargo_GerenteLoja = Cargo.Cargo_ID
    FROM Cargo
    WHERE Cargo.Cargo_Descricao LIKE 'Gerente de Loja';

    SELECT TOP 1 @Usuario_Sistema = Usuario.Usuario_ID
    FROM Usuario
    WHERE Usuario.Usuario_Login LIKE 'Sistema';

    SELECT TOP 1 @ParametroSistema_CreditoPagoAcimaDe = CONVERT(NUMERIC(10, 4), Parametros_Sistema.Parametros_Sistema_Valor)
    FROM Parametros_Sistema
    WHERE Parametros_Sistema.Parametros_Sistema_Tipo LIKE 'CREDITO_PAGO_ACIMA_DE'
          AND Parametros_Sistema.Lojas_ID = 1;

    SET @Enum_ProcessoValidacao_CreditoDePeçaSemOrigem = 2787;
    SET @Enum_ProcessoValidacao_DevolucaoEmDinheiro    = 2788;
    SET @Enum_StatusValidacao_Pendente                 = 2789;
    SET @Enum_TipoTroca_FiltroPorPeca                  = 2786;

    --SELECT @GerenteLoja
    --SELECT @Usuario_Sistema
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

/*Todos os créditos gerados a partir da funcionalidade da US01, 
  com validação do gerente da loja (Cargo_id = 39). (crédito de peça sem origem)*/

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
           GETDATE(), 
           Romaneio_Venda_CT.Romaneio_Venda_CT_Valor_Pago
    FROM Romaneio_Venda_CT
    --INNER JOIN 
    --  Romaneio_Venda_IT ON 
    --  Romaneio_Venda_IT.Romaneio_Venda_CT_ID = Romaneio_Venda_CT.Romaneio_Venda_CT_ID
    --  AND Romaneio_Venda_IT.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
    WHERE Romaneio_Venda_CT.Enum_Tipo_Troca_ID = @Enum_TipoTroca_FiltroPorPeca
          AND NOT EXISTS
    (
        SELECT 1
        FROM Pendencia_Validacao
        WHERE Pendencia_Validacao.Objeto_Origem_ID = Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID
              AND Pendencia_Validacao.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
              AND Pendencia_Validacao.Enum_Processo_ID IN(@Enum_ProcessoValidacao_CreditoDePeçaSemOrigem, @Enum_ProcessoValidacao_DevolucaoEmDinheiro)
    ); 
    --AND Romaneio_Venda_CT.Romaneio_Venda_CT_ID NOT IN (SELECT Objeto_Origem_ID FROM Pendencia_Validacao)

/*
  Créditos pagos em dinheiro acima de 500 reais (parâmetro de sistema: CREDITO_PAGO_ACIMA_DE), 
  com validação do gerente da loja (Cargo_id = 39) (Devolução em dinheiro)  
  */

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
           @Enum_ProcessoValidacao_DevolucaoEmDinheiro, 
           @Enum_StatusValidacao_Pendente, 
           Romaneio_Venda_CT.Usuario_Aprovacao_Credito_ID, 
           GETDATE(), 
           Romaneio_Venda_CT.Romaneio_Venda_CT_Valor_Pago
    FROM Romaneio_Venda_CT
    WHERE Enum_Tipo_ID IN(550, 797)
    AND Romaneio_Venda_CT_Valor_Pago < -1 * @ParametroSistema_CreditoPagoAcimaDe
    AND Usuario_Aprovacao_Credito_ID IS NOT NULL
    AND NOT EXISTS
    (
        SELECT 1
        FROM Pendencia_Validacao
        WHERE Pendencia_Validacao.Objeto_Origem_ID = Romaneio_Venda_CT.Romaneio_Pre_Venda_CT_ID
              AND Pendencia_Validacao.Lojas_ID = Romaneio_Venda_CT.Lojas_ID
              AND Pendencia_Validacao.Enum_Processo_ID IN(@Enum_ProcessoValidacao_CreditoDePeçaSemOrigem, @Enum_ProcessoValidacao_DevolucaoEmDinheiro)
    ); 
    --AND Romaneio_Venda_CT.Romaneio_Venda_CT_ID NOT IN (SELECT Objeto_Origem_ID FROM Pendencia_Validacao)
  END;