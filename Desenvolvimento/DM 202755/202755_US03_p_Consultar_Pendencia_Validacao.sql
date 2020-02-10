SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
 
-------------------------------------------------------------------------------
-- <summary>
--	Consulta à tabela Pendencia_Validacao.
-- </summary>
-- <history>
--	[fmoraes]	03/12/2019	Create
-- </history>
-------------------------------------------------------------------------------
--CREATE PROCEDURE [dbo].[p_Consultar_Pendencia_Validacao]
--(
--  @Lojas_ID              INT,
--  @Enum_Status_ID        INT,
--  @Enum_Processo_ID      INT,
--  @Usuario_Geracao_ID    INT,
--  @DataInicialOrigem     DATETIME,
--  @DataFinalOrigem       DATETIME,
--  @Usuario_Validacao_ID  INT,
--  @DataInicialValidacao  DATETIME,
--  @DataFinalValidacao    DATETIME
--)
--AS
------------------------------------ TESTE ------------------------------------
DECLARE
  @Lojas_ID              INT,
  @Enum_Status_ID        INT,
  @Enum_Processo_ID      INT,
  @Usuario_Geracao_ID    INT,
  @DataInicialOrigem     DATETIME,
  @DataFinalOrigem       DATETIME,
  @Usuario_Validacao_ID  INT,
  @DataInicialValidacao  DATETIME,
  @DataFinalValidacao    DATETIME

SET @Lojas_ID              = 1;
SET @Enum_Status_ID        = 2789; -- 2789: Pendente; 2790: Validado
SET @Enum_Processo_ID      = 2787; -- 2787: Crédito de peça sem origem; 2788: Devolução em dinheiro
SET @Usuario_Geracao_ID    = NULL;
SET @DataInicialOrigem     = '2019-11-26';
SET @DataFinalOrigem       = '2019-12-03';
SET @Usuario_Validacao_ID  = NULL;
SET @DataInicialValidacao  = '2019-11-26';
SET @DataFinalValidacao    = '2019-12-03';

-------------------------------------------------------------------------------
 
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SET NOCOUNT ON

SELECT * FROM Pendencia_Validacao 
WHERE Lojas_ID = @Lojas_ID
AND Enum_Status_ID = COALESCE(NULLIF(@Enum_Status_ID, ''), Enum_Status_ID)
AND Enum_Processo_ID = COALESCE(NULLIF(@Enum_Processo_ID, ''), Enum_Processo_ID)
AND Usuario_Geracao_ID = COALESCE(NULLIF(@Usuario_Geracao_ID, ''), Usuario_Geracao_ID)
AND Pendencia_Validacao_Data_Geracao BETWEEN @DataInicialOrigem AND @DataFinalOrigem
AND Usuario_Validacao_ID = COALESCE(NULLIF(@Usuario_Validacao_ID, ''), Usuario_Validacao_ID)
AND Pendencia_Validacao_Data_Validacao BETWEEN @DataInicialValidacao AND @DataFinalValidacao
ORDER BY Pendencia_Validacao_Data_Geracao ASC

------------------------- Retorna os Usuarios_Origem --------------------------

SELECT DISTINCT Usuario_Geracao_ID, Usuario.Usuario_Nome_Completo 
FROM Pendencia_Validacao 
INNER JOIN Usuario
ON Usuario.Usuario_ID = Pendencia_Validacao.Usuario_Geracao_ID
ORDER BY Usuario.Usuario_Nome_Completo ASC
 
SET NOCOUNT OFF