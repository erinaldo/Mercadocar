-------------------------------------------------------------------------------
--  USUÁRIO: fmoraes (Desenvolvimento: 7493, Chamados: , Produção: 13145)
--  DATA: / /
--  Serviço - Titulo
-------------------------------------------------------------------------------

DECLARE

 -- Geral
     @Usuario_Ultima_Alteracao_ID        INT          = 0, -- Código do Usuário (NECESSÁRIO INFORMAR)
     @Perm_Acao_ID                       INT, -- Loop da Ação
     @Perm_Grupo_Filho_ID                INT, -- Loop do Grupo de Usuários
     @LOOP                               INT          = 0, 

-- Cadastro de Formulário 
     @Perm_Frm_ID                        INT          = 0, -- Código do Formulário (NECESSÁRIO INFORMAR)

 -- Cadastro de Grupo de Trabalho
     @Perm_Grupo_ID                      INT, 
     @Perm_Grupo_NM                      VARCHAR(100) = '',    -- Nome do Grupo (NECESSÁRIO INFORMAR)
     @Perm_Grupo_DS                      VARCHAR(100) = '',    -- Descrição do Grupo (NECESSÁRIO INFORMAR)
     @Enum_Tipo_Grupo_ID                 INT          = 784;  -- Tipo do Grupo (784 - Permissão / 785 - Usuário)

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
NULL -- Inserir os códigos (NECESSÁRIO INFORMAR)
  );

------------------------ Incluir Grupos de Usuários ---------------------------

INSERT INTO @Perm_Grupo_Usuarios
  SELECT Perm_Grupo_ID FROM Perm_Grupo WHERE Perm_Grupo_ID IN
  (
  NULL -- Inserir os códigos (NECESSÁRIO INFORMAR)
  );


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
  (   SELECT Perm_Grupo_ID
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