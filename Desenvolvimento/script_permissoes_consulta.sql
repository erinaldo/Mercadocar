/*

INSERT INTO Perm_Grupo_REL_Grupo 
     (   Perm_Grupo_Pai_ID, 
         Perm_Grupo_Filho_ID, 
         Usuario_Ultima_Alteracao_ID ) 
VALUES 
     (   2860, 
         1182, 
         13145)

--=============================================================================
--                              DELETAR GRUPOS
--=============================================================================


-- Formulario x Ação --

-- SELECT * FROM Perm_Grupo WHERE Perm_Grupo_NM LIKE '%Pendências de Validação%' ORDER BY Perm_Grupo_ID DESC
DELETE FROM Perm_Grupo WHERE Perm_Grupo_ID IN
(
2860
)

-- Formulario x Modulo --

-- SELECT * FROM Perm_Formulario_REL_Acao WHERE Perm_Frm_ID = 1084 ORDER BY Perm_Frm_REL_Acao_ID DESC
DELETE FROM Perm_Formulario_REL_Acao WHERE Perm_Frm_ID = 1084 AND Perm_Frm_REL_Acao_ID IN
(
21408
)

-- Grupo x Grupo --

-- SELECT * FROM Perm_Grupo_REL_Grupo WHERE Usuario_Ultima_Alteracao_ID = 13145 ORDER BY Perm_Grupo_REL_Grupo_ID DESC
DELETE FROM Perm_Grupo_REL_Grupo WHERE Perm_Grupo_REL_Grupo_ID IN
(
123092
)

-- Grupo x Formulario x Ação --

-- SELECT TOP 10 * FROM Perm_GrupoPermissao WHERE Perm_Frm_ID = 1084 AND Usuario_Ultima_Alteracao_ID = 13145 ORDER BY Perm_GrpPerm_ID DESC
DELETE FROM Perm_GrupoPermissao WHERE Perm_GrpPerm_ID IN
(
225079
)


--=============================================================================
--                                 CONSULTAS
--=============================================================================


----------------------------- TIPOS DE PERMISSOES -----------------------------
--784 -- grupo de permissão
--785 -- grupo de usuário

--SELECT * FROM Enumerado WHERE Enum_ID IN (784, 785)

----------------------------------- MODULO ------------------------------------

SELECT * FROM Perm_Modulo WHERE Perm_Modulo_NM LIKE '%Backoffice%'
SELECT * FROM Perm_Modulo WHERE Perm_Modulo_NmProjeto LIKE '%Backoffice%'
SELECT * FROM Perm_Modulo WHERE Perm_Modulo_DS LIKE '%Backoffice%'

---------------------------------- MENU ---------------------------------------

SELECT * FROM Perm_Menu WHERE Perm_Menu_ID = 257
SELECT * FROM Perm_Menu WHERE Perm_Menu_NM LIKE '%Solicitação de Pagamento%'
SELECT * FROM Perm_Menu WHERE Perm_Frm_ID = 851

--------------------------------- FORMULARIO ----------------------------------

-- Código:
SELECT * FROM Perm_Formulario WHERE Perm_Frm_ID = 1084

-- Nome do formulário
SELECT * FROM Perm_Formulario WHERE Perm_Frm_NM LIKE '%Listagem de Tipo de Objeto Destino%'

-- Classe do formulário
SELECT * FROM Perm_Formulario WHERE Perm_Frm_NmProjeto LIKE '%frmRomaneio_Grid%'

------------------------------------ ACAO -------------------------------------

SELECT * FROM Perm_Acao WHERE Perm_Acao_ID = 4
SELECT * FROM Perm_Acao WHERE Perm_Acao_NM LIKE '%abrir%'

------------------------------------ GRUPO ------------------------------------

SELECT * FROM Perm_Grupo WHERE Perm_Grupo_ID = 2867--1182
SELECT * FROM Perm_Grupo WHERE Perm_Grupo_NM LIKE '%Pendências de Validação%'

-------------------------------- GRUPO X GRUPO --------------------------------

SELECT * FROM Perm_Grupo_REL_Grupo WHERE Perm_Grupo_Pai_ID = 2728

----------------------------------- Usuário -----------------------------------

SELECT * FROM Usuario WHERE Usuario_ID = 7533

SELECT *
FROM Perm_Grupo_Historico_Usuarios 
WHERE Usuario_Manipulado_ID = 7533

SELECT * FROM Perm_Gerenciamento_Grupo WHERE Usuario_Gerenciamento_ID IS NOT NULL

-- Lista em que GRUPOS os usuário está

SELECT PU.Usuario_ID, PU.Usuario_Login, PU.Usuario_IsAdministrador,
PUG.Perm_Usu_REL_Grp_ID, 
PUG.Usuario_ID, 
PUG.Perm_Grupo_ID, 
PG.Perm_Grupo_NM,
ISNULL(PUG.Lojas_ID,0) As Lojas_ID
FROM Perm_Usuario_REL_Grupo PUG 
INNER JOIN Usuario PU ON PU.Usuario_ID = PUG.Usuario_ID 
INNER JOIN Perm_Grupo PG ON PG.Perm_Grupo_ID = PUG.Perm_Grupo_ID
WHERE PUG.Usuario_ID  = 7533

------------------------- Grupo de Usuário / Trabalho -------------------------

SELECT * FROM Perm_Grupo 
WHERE Perm_Grupo_IsAtivo = 1 
AND Perm_Grupo_NM NOT LIKE '%old - %'
AND Enum_Tipo_Grupo_ID IN (784, 785)
--AND Perm_Grupo_ID = 2404 
AND Perm_Grupo_NM LIKE '%Fiscais de Caixa%'

---------------- Relacionamento Grupo x Formulario x Ação ---------------------

SELECT * FROM Perm_GrupoPermissao WHERE Perm_Frm_ID = 1201

*/


DECLARE 
     @Perm_Modulo_ID      INT          = NULL, -- Código do Módulo
     @Perm_Modulo_Nm      VARCHAR(100) = '%Gerencia%', -- Nome do Módulo
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