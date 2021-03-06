-------------------------------------------------------------------------------
--  fmoraes 14/10/2019
--  FT 200223
--  Solicitar autenticação especial para fazer crédito para consumidor final
-------------------------------------------------------------------------------

DECLARE -- Grupo de Trabalho
     @LOOP                        INT          = 0, 
     @Usuario_Ultima_Alteracao_ID INT          = 13145, -- fmoraes (Produção)
     @Perm_Modulo_ID              INT          = 0,
     @Perm_Frm_ID                 INT          = 298,   -- Orcamento - Troca
     @Perm_Acao_ID                INT          = 710,   -- Liberar_Consumidor_Final
     @Perm_Acao_NM                VARCHAR(100) = 'Liberar_Consumidor_Final',
     @Perm_Grupo_NM               VARCHAR(100) = 'Acesso de Liberação para Consumidor Final', 
     @Perm_Grupo_DS               VARCHAR(100) = 'Permissão de liberar a geração de crédito para consumidor final.', 
     @Enum_Tipo_Grupo_ID          INT          = 784,   -- Tipo Grupo de Permissão 
     @Perm_Grupo_Filho_ID         INT;                  -- Grupo de Usuários

DECLARE -- Tabela Módulos
     @Perm_Modulos TABLE(Perm_Modulo_ID INT);

INSERT INTO @Perm_Modulos
SELECT Perm_Modulo_ID
FROM Perm_Modulo
WHERE Perm_Modulo_ID IN
(
13, -- Módulo S.A.C.
16  -- Módulo Venda Técnica
);

DECLARE -- Tabela Grupo de Usuários
     @Perm_Grupo_Usuarios TABLE(Perm_Grupo_ID INT);

INSERT INTO @Perm_Grupo_Usuarios
SELECT Perm_Grupo_ID
FROM Perm_Grupo
WHERE Perm_Grupo_ID IN
(
918, -- Gerentes de Mecânica
930, -- Gerentes de Acessórios
1347,-- Gerentes de Loja
1392 -- Diretores
);

------------------------------ Incluir Ação -----------------------------------
IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Acao
    WHERE Perm_Acao_ID = @Perm_Acao_ID
          AND Perm_Acao_NM = @Perm_Acao_NM
)
  INSERT INTO Perm_Acao
  (Perm_Acao_ID, 
   Perm_Acao_NM, 
   Perm_Acao_DS
  )
  VALUES
  (@Perm_Acao_ID, 
   @Perm_Acao_NM, 
   @Perm_Grupo_DS
  );

---------------- Incluir Relacionamento Formulario x Ação ---------------------
IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Formulario_REL_Acao
    WHERE Perm_Frm_ID = @Perm_Frm_ID
          AND Perm_Acao_ID = @Perm_Acao_ID
)
  INSERT INTO Perm_Formulario_REL_Acao
  (Perm_Frm_ID, 
   Perm_Acao_ID
  )
  VALUES
  (@Perm_Frm_ID, 
   @Perm_Acao_ID
  );

--------------- Incluir Relacionamento Formulario x Modulo --------------------
WHILE(1 = 1)
  BEGIN
    SELECT @LOOP = MIN(Perm_Modulo_ID)
    FROM @Perm_Modulos
    WHERE Perm_Modulo_ID > @LOOP;
    IF @LOOP IS NULL
      BREAK;
    SET @Perm_Modulo_ID = @LOOP;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Perm_Modulo_REL_Formulario
        WHERE Perm_Frm_ID = @Perm_Frm_ID
              AND Perm_Modulo_ID IN(@Perm_Modulo_ID)
    )
      INSERT INTO Perm_Modulo_REL_Formulario
      (Perm_Frm_ID, 
       Perm_Modulo_ID
      )
      VALUES
      (@Perm_Frm_ID, 
       @Perm_Modulo_ID
      );
  END;
  SET @LOOP = 0;

----------------------- Incluir Grupo de Permissão ----------------------------
DECLARE 
     @Perm_Grupo_ID INT;

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Grupo
    WHERE Perm_Grupo_IsAtivo = 1
          AND Perm_Grupo_NM = @Perm_Grupo_NM
          AND Perm_Grupo_DS = @Perm_Grupo_DS
)
  BEGIN
    INSERT INTO Perm_Grupo
    (Perm_Grupo_NM, 
     Perm_Grupo_DS, 
     Enum_Tipo_Grupo_ID, 
     Usuario_Ultima_Alteracao_ID, 
     Perm_Grupo_IsAtivo
    )
    VALUES
    (@Perm_Grupo_NM, 
     @Perm_Grupo_DS, 
     @Enum_Tipo_Grupo_ID, 
     @Usuario_Ultima_Alteracao_ID, 
     1
    );
    SELECT @Perm_Grupo_ID = SCOPE_IDENTITY();
END;
  ELSE
  BEGIN
    SELECT @Perm_Grupo_ID = Perm_Grupo_ID
    FROM Perm_Grupo
    WHERE Perm_Grupo_IsAtivo = 1
          AND Perm_Grupo_NM = @Perm_Grupo_NM
          AND Perm_Grupo_DS = @Perm_Grupo_DS;
END;
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
    )
      INSERT INTO Perm_Grupo_REL_Grupo
      (Perm_Grupo_Pai_ID, 
       Perm_Grupo_Filho_ID, 
       Usuario_Ultima_Alteracao_ID
      )
      VALUES
      (@Perm_Grupo_ID, 
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
)
  INSERT INTO Perm_GrupoPermissao
  (Perm_Grupo_ID, 
   Perm_Frm_ID, 
   Perm_Acao_ID, 
   Usuario_Ultima_Alteracao_ID, 
   Perm_GrpPerm_Permissao
  )
  VALUES
  (@Perm_Grupo_ID, 
   @Perm_Frm_ID, 
   @Perm_Acao_ID, 
   @Usuario_Ultima_Alteracao_ID, 
   1
  );
GO