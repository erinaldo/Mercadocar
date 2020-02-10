-------------------------------------------------------------------------------
--  USUÁRIO: fmoraes (Desenvolvimento: 7533, Chamados: 13145, Produção: 13145)
--  DATA: 28/11/2019
--  DEMANDA 202755 - Política de Créditos - US04
-------------------------------------------------------------------------------

DECLARE

 -- Geral
     @Usuario_Ultima_Alteracao_ID        INT          = 13145,-- Código do Usuário
     @Perm_Acao_ID                       INT,                 -- Loop da Ação
     @Perm_Grupo_Filho_ID                INT,                 -- Loop do Grupo de Usuários
     @LOOP                               INT          = 0, 

-- Cadastro de Formulário 
     @Perm_Frm_ID                        INT          = 851, -- Produção e Chamados = 851 (Desenvolvimeno = 807) 
     @Perm_Frm_NmProjeto                 VARCHAR(100) = 'frmRomaneio_Grid', -- Listagem de Romaneio de Venda

     @Perm_Frm_ID_SolicitacaoPagto       INT          = 835, -- Produção e Chamados = 835 (Desenvolvimeno = 1198)
     @Perm_Frm_DS_SolicitacaoPagto       VARCHAR(100) = 'Solicitação de Pagamento para o Financeiro'; --Descrição

 -- Cadastro de Grupo de Trabalho
 DECLARE
     @Perm_Grupo_ID                      INT, 
     @Perm_Grupo_NM                      VARCHAR(100) = 'Acesso para liberar depósito em conta na Listagem de Romaneios de Venda', -- Nome do Grupo
     @Perm_Grupo_DS                      VARCHAR(100) = 'Acesso para liberar depósito em conta na Listagem de Romaneios de Venda.',-- Descrição da Ação
     @Enum_Tipo_Grupo_ID                 INT          = 784, -- Tipo Permissão

-- Cadastro de Módulo
    @Perm_Modulo_ID                     INT          = 18,  -- Financeiro

-- Cadastro de Menu
     @Perm_Menu_Pai_ID                   INT          = 0, 
     @Perm_Menu_Ordenacao                INT          = NULL,
     @Perm_Menu_NM                       VARCHAR(100) = 'Solicitação de Pagamento'; 

 -- Temp Ação
 DECLARE
     @Perm_Acao TABLE(Perm_Acao_ID INT);

 -- Temp Grupo de Usuários
 DECLARE
     @Perm_Grupo_Usuarios TABLE(Perm_Grupo_ID INT);



------------------------------ Incluir Ação -----------------------------------

INSERT INTO @Perm_Acao
  SELECT Perm_Acao_ID FROM Perm_Acao WHERE Perm_Acao_ID IN
  (
591 -- Permite liberar venda no caixa com a forma de pagamento depósito em conta.
  );

------------------------ Incluir Grupos de Usuários ---------------------------

INSERT INTO @Perm_Grupo_Usuarios
  SELECT Perm_Grupo_ID FROM Perm_Grupo WHERE Perm_Grupo_ID IN
  (
  1392, -- Diretores
  1347, -- Gerentes de Loja
   918, -- Gerentes de Mecânica
   930, -- Gerentes de Acessórios
  1348, -- Gerentes Gerais
   783, -- Encarregados(as) de Caixa
   784  -- Fiscais de Caixa
  );

------------------------- Cadastro de Formulario ------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Formulario
    WHERE Perm_Frm_ID = @Perm_Frm_ID_SolicitacaoPagto
    AND Perm_Frm_DS = @Perm_Frm_DS_SolicitacaoPagto
)
BEGIN
UPDATE Perm_Formulario SET Perm_Frm_DS = @Perm_Frm_DS_SolicitacaoPagto WHERE Perm_Frm_ID = @Perm_Frm_ID_SolicitacaoPagto
END;

---------------------------- Cadastro de Menu ---------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Menu
    WHERE Perm_Frm_ID = @Perm_Frm_ID_SolicitacaoPagto
)
BEGIN
    INSERT INTO Perm_Menu 
     (   Perm_Menu_Pai_ID,   
         Perm_Modulo_ID,     
         Perm_Frm_ID,        
         Perm_Menu_NM,       
         Perm_Menu_Ordenacao,     
         Usuario_Ultima_Alteracao_ID
     )   
    Values 
     (   @Perm_Menu_Pai_ID, 
         @Perm_Modulo_ID,   
         @Perm_Frm_ID_SolicitacaoPagto,   
         @Perm_Menu_NM,   
         @Perm_Menu_Ordenacao,   
         @Usuario_Ultima_Alteracao_ID
      );
END
ELSE
BEGIN
    IF NOT EXISTS
    (
        SELECT 1
        FROM Perm_Menu
        WHERE Perm_Menu_NM = @Perm_Menu_NM
    )
    UPDATE Perm_Menu SET Perm_Menu_NM = @Perm_Menu_NM WHERE Perm_Frm_ID = @Perm_Frm_ID_SolicitacaoPagto
END;

----------------------- Cadastro Grupo de Trabalho ----------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Grupo
    WHERE Perm_Grupo_IsAtivo = 1
          AND Perm_Grupo_NM = @Perm_Grupo_NM
          AND Perm_Grupo_DS = @Perm_Grupo_DS
) AND @Perm_Grupo_NM IS NOT NULL AND DATALENGTH(@Perm_Grupo_NM) > 0
BEGIN
    INSERT INTO Perm_Grupo
    ( Perm_Grupo_NM, 
     Perm_Grupo_DS, 
     Enum_Tipo_Grupo_ID, 
     Usuario_Ultima_Alteracao_ID, 
     Perm_Grupo_IsAtivo
    )
    VALUES
    (  @Perm_Grupo_NM, 
     @Perm_Grupo_DS, 
     @Enum_Tipo_Grupo_ID, 
     @Usuario_Ultima_Alteracao_ID, 
     1
    );
    SELECT @Perm_Grupo_ID = SCOPE_IDENTITY();
END;
ELSE
BEGIN
IF DATALENGTH(@Perm_Grupo_NM) > 0
    SET @Perm_Grupo_ID = 
    ( SELECT Perm_Grupo_ID
      FROM Perm_Grupo
      WHERE Perm_Grupo_IsAtivo = 1
      AND Perm_Grupo_NM = @Perm_Grupo_NM
    );
END;

---------------- Incluir Relacionamento Formulario x Ação ---------------------

WHILE(1 = 1)
  BEGIN
    SELECT @LOOP = MIN(Perm_Acao_ID)
    FROM @Perm_Acao
    WHERE Perm_Acao_ID > @LOOP;
    IF @LOOP IS NULL
      BREAK;
    SET @Perm_Acao_ID = @LOOP;

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Formulario_REL_Acao
    WHERE Perm_Frm_ID = @Perm_Frm_ID
          AND Perm_Acao_ID IN(@Perm_Acao_ID)
)
  INSERT INTO Perm_Formulario_REL_Acao
  (   Perm_Frm_ID, 
      Perm_Acao_ID
  )
  VALUES
  (   @Perm_Frm_ID, 
      @Perm_Acao_ID
  );
  END;
SET @LOOP = 0;

------------------ Incluir Relacionamento Grupo x Grupo -----------------------

WHILE(1 = 1)
  BEGIN
    SELECT @LOOP = MIN(Perm_Grupo_ID)
    FROM @Perm_Grupo_Usuarios
    WHERE Perm_Grupo_ID > @LOOP;
    IF @LOOP IS NULL
      BREAK;
    SET @Perm_Grupo_Filho_ID = @LOOP;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Perm_Grupo_REL_Grupo
        WHERE Perm_Grupo_Pai_ID = @Perm_Grupo_ID
              AND Perm_Grupo_Filho_ID IN(@Perm_Grupo_Filho_ID)
    ) AND @Perm_Grupo_ID IS NOT NULL AND DATALENGTH(@Perm_Grupo_ID) > 0
      INSERT INTO Perm_Grupo_REL_Grupo
      (  Perm_Grupo_Pai_ID, 
         Perm_Grupo_Filho_ID, 
         Usuario_Ultima_Alteracao_ID
      )
      VALUES
      (  @Perm_Grupo_ID, 
         @Perm_Grupo_Filho_ID, 
         @Usuario_Ultima_Alteracao_ID
      );
  END;
SET @LOOP = 0;

------------ Incluir Relacionamento Grupo x Formulario x Ação -----------------

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_GrupoPermissao
    WHERE Perm_Grupo_ID = @Perm_Grupo_ID
          AND Perm_Frm_ID = @Perm_Frm_ID
          AND Perm_Acao_ID = @Perm_Acao_ID
) AND @Perm_Grupo_ID IS NOT NULL AND DATALENGTH(@Perm_Grupo_ID) > 0
  INSERT INTO Perm_GrupoPermissao
  (  Perm_Grupo_ID, 
     Perm_Frm_ID, 
     Perm_Acao_ID, 
     Usuario_Ultima_Alteracao_ID, 
     Perm_GrpPerm_Permissao
  )
  VALUES
  (  @Perm_Grupo_ID, 
     @Perm_Frm_ID, 
     @Perm_Acao_ID, 
     @Usuario_Ultima_Alteracao_ID, 
     1
  );
GO