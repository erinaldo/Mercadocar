-------------------------------------------------------------------------------
--  fmoraes 16/09/2019
--  FT 181334 - Restri��o de acesso na confer�ncia CD
-------------------------------------------------------------------------------

DECLARE -- Grupo de Trabalho

-- Foi criado um novo grupo pelo Netto em Produ��o: Acesso de abrir tela de itens do lote de confer�ncia

     @Usuario_Ultima_Alteracao_ID INT          = 13145, -- fmoraes (Produ��o)
     @Perm_Modulo_ID              INT          = 6, -- M�dulo Entreposto
     @Perm_Frm_ID                 INT          = 696, -- Pecas Pendentes de Confer�ncia
     @Perm_Acao_ID                INT          = 266, -- Visualizar_Qtde
     @Perm_Grupo_NM               VARCHAR(100) = 'Permiss�o de visualizar Qtde Pendente na tela Itens pendentes Separa��o/Guarda', 
     @Perm_Grupo_DS               VARCHAR(100) = 'Permiss�o de visualizar Qtde Pendente na tela Itens pendentes Separa��o/Guarda.', 
     @Enum_Tipo_Grupo_ID          INT          = 784;   -- Grupo de Permiss�o 

DECLARE -- Grupo de Usu�rios
     @Perm_Grupo_Filho_ID INT = 1881; -- L�deres CD
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
 @Perm_Grupo_Filho_ID, 
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