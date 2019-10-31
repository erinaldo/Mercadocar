-------------------------------------------------------------------------------
--  fmoraes 16/09/2019
--  FT 191453 - Possibilitar Estornar Servi�os Finalizados na Instala��o
-------------------------------------------------------------------------------

DECLARE -- Grupo de Trabalho
     @Usuario_Ultima_Alteracao_ID INT          = 13145, -- fmoraes (Produ��o)
     @Perm_Modulo_ID              INT          = 17, -- M�dulo Instala��o
     @Perm_Frm_ID                 INT          = 677, -- Propriedades do Cadastro de Instala��o
     @Perm_Acao_ID                INT          = 113, -- Estornar
     @Perm_Grupo_NM               VARCHAR(100) = 'Permiss�o para estornar servi�os finalizados', 
     @Perm_Grupo_DS               VARCHAR(100) = 'Permiss�o para estornar servi�os finalizados.', 
     @Enum_Tipo_Grupo_ID          INT          = 784;   -- Grupo de Permiss�o 

DECLARE -- Grupo de Usu�rios
     @Diretores               INT = 1392, 
     @Gerentes_Loja           INT = 1347, 
     @Gerentes_Gerais         INT = 1348, 
     @Encarregados_Instala��o INT = 904;

----------------- Incluir Relacionamento Formulario x A��o ----------------------
IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Formulario_REL_Acao
    WHERE Perm_Frm_ID = @Perm_Frm_ID
          AND Perm_Acao_ID = @Perm_Acao_ID
)
  BEGIN
    INSERT INTO Perm_Formulario_REL_Acao
    (Perm_Frm_ID, 
     Perm_Acao_ID
    )
    VALUES
    (@Perm_Frm_ID, 
     @Perm_Acao_ID
    );
END;

----------------- Incluir Relacionamento Formulario x Modulo ----------------------

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Modulo_REL_Formulario
    WHERE Perm_Frm_ID = @Perm_Frm_ID
          AND Perm_Modulo_ID = @Perm_Modulo_ID
)
  BEGIN
    INSERT INTO Perm_Modulo_REL_Formulario
    (Perm_Frm_ID, 
     Perm_Modulo_ID
    )
    VALUES
    (@Perm_Frm_ID, 
     @Perm_Modulo_ID
    );
END;

----------------- Incluir Grupo de Permiss�o ----------------------
DECLARE 
     @Perm_Grupo_ID INT;

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

----------------- Incluir Relacionamento Grupo x Grupo ----------------------
INSERT INTO Perm_Grupo_REL_Grupo
(Perm_Grupo_Pai_ID, 
 Perm_Grupo_Filho_ID, 
 Usuario_Ultima_Alteracao_ID
)
VALUES
(@Perm_Grupo_ID, 
 @Diretores, 
 @Usuario_Ultima_Alteracao_ID
);
----------------- Incluir Relacionamento Grupo x Grupo ----------------------
INSERT INTO Perm_Grupo_REL_Grupo
(Perm_Grupo_Pai_ID, 
 Perm_Grupo_Filho_ID, 
 Usuario_Ultima_Alteracao_ID
)
VALUES
(@Perm_Grupo_ID, 
 @Gerentes_Loja, 
 @Usuario_Ultima_Alteracao_ID
);
----------------- Incluir Relacionamento Grupo x Grupo ----------------------
INSERT INTO Perm_Grupo_REL_Grupo
(Perm_Grupo_Pai_ID, 
 Perm_Grupo_Filho_ID, 
 Usuario_Ultima_Alteracao_ID
)
VALUES
(@Perm_Grupo_ID, 
 @Gerentes_Gerais, 
 @Usuario_Ultima_Alteracao_ID
);
----------------- Incluir Relacionamento Grupo x Grupo ----------------------
INSERT INTO Perm_Grupo_REL_Grupo
(Perm_Grupo_Pai_ID, 
 Perm_Grupo_Filho_ID, 
 Usuario_Ultima_Alteracao_ID
)
VALUES
(@Perm_Grupo_ID, 
 @Encarregados_Instala��o, 
 @Usuario_Ultima_Alteracao_ID
);

----------------- Incluir Relacionamento Grupo x Formulario x A��o ----------------------
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