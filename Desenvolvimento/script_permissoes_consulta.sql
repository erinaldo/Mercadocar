/*
----------------------------- Tipos de Permissões -----------------------------
--784 -- grupo de permissão
--785 -- grupo de usuário

--SELECT * FROM Enumerado WHERE Enum_ID IN (784, 785)

------------------------------ Grupo de Trabalho ------------------------------

SELECT * FROM Perm_Grupo 
WHERE Perm_Grupo_IsAtivo = 1 
AND Perm_Grupo_NM NOT LIKE '%old - %'
AND Enum_Tipo_Grupo_ID IN (784, 785)
AND Perm_Grupo_ID = 2404 
--AND Perm_Grupo_NM LIKE '%Diretor%'

*/


DECLARE 
     @Perm_Modulo_ID      INT          = NULL, -- Código do Módulo
     @Perm_Modulo_Nm      VARCHAR(100) = '%%', -- Nome do Módulo
     @Perm_Acao_ID        INT          = NULL, -- Código da Ação
     @Perm_Acao_NM        VARCHAR(100) = '%%', -- Nome da Ação
     @Perm_Frm_Nm         VARCHAR(100) = '%%', -- Nome do Formulário
     @Perm_Frm_NmProjeto  VARCHAR(100) = '%%', -- Nome da classe do Formulário
     @Perm_Grupo_ID       INT          = NULL, -- Código do Grupo
     @Perm_Grupo_NM       VARCHAR(100) = '%%', -- Nome do Grupo Pai
     @Perm_Grupo_Filho_NM VARCHAR(100) = '%%', -- Nome do Grupo Filho
     @Perm_Grupo_Filho_DS VARCHAR(100) = '%%'; -- Descrição do Grupo Filho


------------------- Grupo de Usuários no Grupo de Trabalho --------------------
SELECT Perm_Grupo_REL_Grupo_ID, 
       Perm_Grupo_Filho_ID, 
       Perm_Grupo_NM AS Perm_Grupo_Filho_NM, 
       Perm_Grupo_DS AS Perm_Grupo_Filho_DS
FROM Perm_Grupo_REL_Grupo AS rgrp
     JOIN Perm_Grupo AS pgr
     ON Perm_Grupo_Filho_ID = pgr.Perm_Grupo_ID
     JOIN Enumerado AS enum
     ON pgr.Enum_Tipo_Grupo_ID = enum.Enum_ID
WHERE Perm_Grupo_IsAtivo = 1
      AND pgr.Perm_Grupo_NM NOT LIKE '%old - %'
      AND Perm_Grupo_Pai_ID IN
(
    SELECT Perm_Grupo_ID
    FROM Perm_Grupo
    WHERE Perm_Grupo_ID IN
    (
        SELECT CASE WHEN @Perm_Grupo_ID IS NOT NULL
                         AND ISNUMERIC(@Perm_Grupo_ID) > 0
                 THEN @Perm_Grupo_ID
                 ELSE CASE WHEN LEN(@Perm_Grupo_NM) > 0
                        THEN
        (
            SELECT Perm_Grupo_ID
            FROM Perm_Grupo
            WHERE Perm_Grupo_NM LIKE @Perm_Grupo_NM
        )
                      END
               END
    )
)
      AND Perm_Grupo_NM LIKE @Perm_Grupo_Filho_NM
      AND Perm_Grupo_DS LIKE @Perm_Grupo_Filho_DS;

------------------- Relacionamento Ação x Formulario x Grupo ------------------

SELECT DISTINCT 
       rfrm.Perm_Frm_ID, 
       pfrm.Perm_Frm_NmProjeto, 
       pfrm.Perm_Frm_NM, 
       rfrm.Perm_Acao_ID, 
       pac.Perm_Acao_NM, 
       pmod.Perm_Modulo_ID, 
       pmod.Perm_Modulo_NM, 
       pgr.Enum_Tipo_Grupo_ID, 
       enum.Enum_Extenso, 
       pgrp.Perm_Grupo_ID, 
       pgr.Perm_Grupo_NM
FROM Perm_Formulario_REL_Acao AS rfrm
     JOIN Perm_Formulario AS pfrm
     ON rfrm.Perm_Frm_ID = pfrm.Perm_Frm_ID
     JOIN Perm_Acao AS pac
     ON rfrm.Perm_Acao_ID = pac.Perm_Acao_ID
     JOIN Perm_Modulo_REL_Formulario AS rmod
     ON rfrm.Perm_Frm_ID = rmod.Perm_Frm_ID
     JOIN Perm_Modulo AS pmod
     ON rmod.Perm_Modulo_ID = pmod.Perm_Modulo_ID
     JOIN Perm_GrupoPermissao AS pgrp
     ON rfrm.Perm_Acao_ID = pgrp.Perm_Acao_ID
     JOIN Perm_Grupo AS pgr
     ON pgrp.Perm_Grupo_ID = pgr.Perm_Grupo_ID
     JOIN Enumerado AS enum
     ON pgr.Enum_Tipo_Grupo_ID = enum.Enum_ID
WHERE Perm_Grupo_IsAtivo = 1
      AND pgr.Perm_Grupo_NM NOT LIKE '%old - %'
      AND pmod.Perm_Modulo_ID LIKE CASE WHEN ISNUMERIC(@Perm_Modulo_ID) > 0
                                     THEN @Perm_Modulo_ID
                                     ELSE pmod.Perm_Modulo_ID
                                   END
      AND Perm_Modulo_Nm LIKE @Perm_Modulo_Nm
      AND rfrm.Perm_Acao_ID LIKE CASE WHEN ISNUMERIC(@Perm_Acao_ID) > 0
                                   THEN @Perm_Acao_ID
                                   ELSE rfrm.Perm_Acao_ID
                                 END
      AND pac.Perm_Acao_NM LIKE @Perm_Acao_NM
      AND Perm_Frm_Nm LIKE @Perm_Frm_Nm
      AND Perm_Frm_NmProjeto LIKE @Perm_Frm_NmProjeto
      AND pgrp.Perm_Grupo_ID IN
(
    SELECT Perm_Grupo_ID
    FROM Perm_Grupo
    WHERE Perm_Grupo_ID IN
    (
        SELECT CASE WHEN @Perm_Grupo_ID IS NOT NULL
                         AND ISNUMERIC(@Perm_Grupo_ID) > 0
                 THEN @Perm_Grupo_ID
                 ELSE CASE WHEN LEN(@Perm_Grupo_NM) > 0
                        THEN
        (
            SELECT Perm_Grupo_ID
            FROM Perm_Grupo
            WHERE Perm_Grupo_NM LIKE @Perm_Grupo_NM
        )
                      END
               END
    )
)
      AND Perm_Grupo_NM LIKE @Perm_Grupo_NM;