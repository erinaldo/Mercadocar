-------------------------------------------------------------------------------
--  fmoraes 16/09/2019
--  FT 181334 - Restrição de acesso na conferência CD
-------------------------------------------------------------------------------

DECLARE -- Grupo de Trabalho

-- Foi criado um novo grupo pelo Netto em Produção: Acesso de abrir tela de itens do lote de conferência

     @Usuario_Ultima_Alteracao_ID INT          = 13145, -- fmoraes (Produção)
     @Perm_Modulo_ID              INT          = 6, -- Módulo Entreposto
     @Perm_Frm_ID                 INT          = 696, -- Pecas Pendentes de Conferência
     @Perm_Acao_ID                INT          = 266, -- Visualizar_Qtde
     @Perm_Grupo_NM               VARCHAR(100) = 'Permissão de visualizar Qtde Pendente na tela Itens pendentes Separação/Guarda', 
     @Perm_Grupo_DS               VARCHAR(100) = 'Permissão de visualizar Qtde Pendente na tela Itens pendentes Separação/Guarda.', 
     @Enum_Tipo_Grupo_ID          INT          = 784;   -- Grupo de Permissão 

DECLARE -- Grupo de Usuários
     @Perm_Grupo_Filho_ID INT = 1881; -- Líderes CD
----------------- Incluir Relacionamento Formulario x Ação ----------------------
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

----------------- Incluir Grupo de Permissão ----------------------
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
 @Perm_Grupo_Filho_ID, 
 @Usuario_Ultima_Alteracao_ID
);

----------------- Incluir Relacionamento Grupo x Formulario x Ação ----------------------
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