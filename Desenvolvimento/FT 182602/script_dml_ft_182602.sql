---------------------- SCRIPT DML FT 182602 ----------------------

------------------------------------------------------------------
--                             ACAO                             --
------------------------------------------------------------------

INSERT INTO Perm_Acao
(Perm_Acao_NM, Perm_Acao_DS)
VALUES
('Popup_Telepreco_Email', 'Sinalizar na tela de atendimento do Telepreço, informando se é um atendente de Telepreço via e-mail.')


------------------------------------------------------------------
--                          FORMULARIO                          --
------------------------------------------------------------------



-----------------Exclui Relacionamento Formulario x Ação ----------------------
DELETE FROM Perm_Formulario_REL_Acao 
WHERE Perm_Frm_ID = 644


----------------- Incluir Relacionamento Formulario x Ação ----------------------
INSERT INTO Perm_Formulario_REL_Acao 
 (   Perm_Frm_ID,    
     Perm_Acao_ID    )
Values 
 (  644,    
     202    )

----------------- Incluir Relacionamento Formulario x Ação ----------------------
INSERT INTO Perm_Formulario_REL_Acao 
 (   Perm_Frm_ID,    
     Perm_Acao_ID    )
Values 
 (  644,    
     710    )



------------------------------------------------------------------
--                             GRUPO                            --
------------------------------------------------------------------

----------------- Excluir Relacionamento Grupo x Grupo ----------------------
DELETE FROM Perm_Grupo_REL_Grupo 
WHERE Perm_Grupo_Pai_ID = 2519
   OR Perm_Grupo_Filho_ID = 2519
----------------- Incluir Relacionamento Grupo x Grupo ----------------------
INSERT INTO Perm_Grupo_REL_Grupo 
     (   Perm_Grupo_Pai_ID, 
         Perm_Grupo_Filho_ID, 
         Usuario_Ultima_Alteracao_ID ) 
VALUES 
     (   @Perm_Grupo_ID, 
         947, 
         9165)


----------------- Excluir Relacionamento Grupo x Formulario x Ação ----------------------
DELETE FROM Perm_GrupoPermissao 
WHERE Perm_Grupo_ID = 2519
----------------- Incluir Relacionamento Grupo x Formulario x Ação ----------------------
INSERT INTO Perm_GrupoPermissao 
     (   Perm_Grupo_ID, 
         Perm_Frm_ID, 
         Perm_Acao_ID, 
         Usuario_Ultima_Alteracao_ID, 
         Perm_GrpPerm_Permissao ) 
VALUES 
     (   @Perm_Grupo_ID, 
         @Perm_Frm_ID, 
         710, 
         9165, 
         1)
GO
