-------------------------------------------------------------------------------
--  USUÁRIO: fmoraes (Desenvolvimento: 7493, Chamados: 13145, Produção: 13145)
--  DATA: 26/11/2019
--  DEMANDA 202755 - Política de Créditos - US03
-------------------------------------------------------------------------------

DECLARE

 -- Geral
     @Grupo_ID                          INT,                    -- Loop do Grupo de Trabalho
     @Perm_Acao_ID                      INT,                    -- Loop da Ação
     @Perm_Modulo_ID                    INT,                    -- Loop do Módulo
     @Perm_Grupo_Filho_ID               INT,                    -- Loop do Grupo de Usuários
     @Usuario_Ultima_Alteracao_ID       INT          = 13145,   -- Código do Usuário
     @LOOP                              INT          = 0, 
     @LOOP_GRUPOS                       INT          = 0,

-- Cadastro de Formulário 
     @Perm_Frm_ID                       INT, 
     @Perm_Frm_NM                       VARCHAR(100) = 'Pendências de Validação',       --Nome do formulário
     @Perm_Frm_NmProjeto                VARCHAR(100) = 'frmPendencia_Validacao_Grid',   -- Classe do formulário
     @Perm_Frm_DS                       VARCHAR(100) = 'Pesquisa as pendências dos créditos gerados na tabela Pendencia_Validacao.', --Descrição
     @Perm_Frm_Assembly                 VARCHAR(100) = 'MC_Formularios.dll', 
     @Perm_Frm_Namespace                VARCHAR(100) = 'Mercadocar.Formularios', 
     @Perm_Frm_URL                      VARCHAR(100) = '', 
     @Perm_Frm_IsWeb                    INT          = 0, 
     @Servidor_ID                       INT          = 0, 
     @Perm_Frm_IsMobile                 INT          = 0, 
     @Enum_Tipo_Abertura_Formulario_ID  INT          = 2075, 
     @Perm_Frm_IsURL_Absoluto           INT          = 0, 
     @Perm_Frm_Autenticacao_Web_Mcar    BIT          = 0, 
     @Perm_Frm_Logar_Classe_Acessada    BIT          = 1, 

-- Cadastro de Grupo de Trabalho
     @Perm_Grupo_ID                      INT,

-- Cadastro de Menu
     @Perm_Menu_Pai_ID                   INT          = 0, 
     @Perm_Menu_NM                       VARCHAR(100) = 'Pendências de Validação', 
     @Perm_Menu_Ordenacao                INT          = 1000; 

-- Temp Grupo
 DECLARE
     @Perm_Grupo                        TABLE
     (
     Grupo_ID                           INT,
     Perm_Grupo_NM                      VARCHAR(150),
     Perm_Grupo_DS                      VARCHAR(250),
     Enum_Tipo_Grupo_ID                 INT, 
     Perm_Grupo_IsAtivo                 BIT,
     Usuario_Ultima_Alteracao_ID        INT,
     Perm_Acao_ID                       INT
     );

-- Temp Ação
 --DECLARE
 --    @Perm_Acao TABLE(Perm_Acao_ID INT);

-- Temp Módulos
 DECLARE
     @Perm_Modulo TABLE(Perm_Modulo_ID INT);

-- Temp Grupo de Usuários
 DECLARE
     @Perm_Grupo_Usuarios TABLE(Perm_Grupo_ID INT);

------------------------ Incluir Grupo de Trabalho ----------------------------

INSERT INTO @Perm_Grupo
    (
     Grupo_ID, -- ID de controle no LOOP_GRUPOS
     Perm_Grupo_NM,
     Perm_Grupo_DS,
     Enum_Tipo_Grupo_ID, 
     Perm_Grupo_IsAtivo, 
     Usuario_Ultima_Alteracao_ID,
     Perm_Acao_ID
     )
     VALUES
     (1,
     'Acesso para acessar no módulo Gerência o menu da tela Pendências de Validação',
     'Acesso para acessar no módulo Gerência o menu da tela Pendências de Validação.',
     784,
     1, 
     @Usuario_Ultima_Alteracao_ID,
     20 -- Habilitar_Opcao_No_Menu_Do_Modulo
     ),

     (2,
     'Acesso para consultar na tela Pendências de Validação',
     'Acesso para consultar na tela Pendências de Validação.',
     784,
     1, 
     @Usuario_Ultima_Alteracao_ID,
     8 -- Consultar
     ),

     (3,
     'Acesso para alterar a Loja ativa na tela Pendências de Validação',
     'Acesso para alterar a Loja ativa na tela Pendências de Validação.',
     784,
     1, 
     @Usuario_Ultima_Alteracao_ID, 
     24 -- Alterar_Loja_Ativa
     ),

     (4,
     'Acesso para habilitar a opção todas as Lojas na tela Pendências de Validação',
     'Acesso para habilitar a opção todas as Lojas na tela Pendências de Validação.',
     784,
     1, 
     @Usuario_Ultima_Alteracao_ID,
     127 -- Habilitar_Opcao_Todos_No_Combo
     ),

     (5,
     'Acesso para selecionar no menu suspenso na tela Pendências de Validação',
     'Acesso para selecionar no menu suspenso na tela Pendências de Validação.',
     784,
     1, 
     @Usuario_Ultima_Alteracao_ID,
     4 --Selecionar
     );

----------------------------- Incluir Módulos ---------------------------------

INSERT INTO @Perm_Modulo
  SELECT Perm_Modulo_ID FROM Perm_Modulo WHERE Perm_Modulo_ID IN
  (
  11 -- Gerência
  );

------------------------ Incluir Grupos de Usuários ---------------------------

INSERT INTO @Perm_Grupo_Usuarios
  SELECT Perm_Grupo_ID FROM Perm_Grupo WHERE Perm_Grupo_ID IN
  (
  1392, -- Diretores
  1347, -- Gerentes de Loja
   918, -- Gerentes de Mecânica
   930, -- Gerentes de Acessórios
  1348  -- Gerentes Gerais
  );

------------------------- Cadastro de Formulario ------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM Perm_Formulario
    WHERE Perm_Frm_NmProjeto = @Perm_Frm_NmProjeto
) AND @Perm_Frm_NmProjeto IS NOT NULL AND DATALENGTH(@Perm_Frm_NmProjeto) > 0
BEGIN
    INSERT INTO Perm_Formulario 
    (    Perm_Frm_NM, 
         Perm_Frm_NmProjeto, 
         Perm_Frm_DS, 
         Perm_Frm_Assembly, 
         Perm_Frm_Namespace, 
         Perm_Frm_URL, 
         Perm_Frm_IsWeb, 
         Servidor_ID, 
         Perm_Frm_IsMobile, 
         Enum_Tipo_Abertura_Formulario_ID, 
         Usuario_Ultima_Alteracao_ID, 
         Perm_Frm_IsURL_Absoluto, 
         Perm_Frm_Autenticacao_Web_Mcar, 
         Perm_Frm_Logar_Classe_Acessada 
    ) 
    VALUES 
    (    @Perm_Frm_NM, 
         @Perm_Frm_NmProjeto, 
         @Perm_Frm_DS, 
         @Perm_Frm_Assembly, 
         @Perm_Frm_Namespace, 
         @Perm_Frm_URL, 
         @Perm_Frm_IsWeb, 
         @Servidor_ID, 
         @Perm_Frm_IsMobile, 
         @Enum_Tipo_Abertura_Formulario_ID, 
         @Usuario_Ultima_Alteracao_ID, 
         @Perm_Frm_IsURL_Absoluto, 
         @Perm_Frm_Autenticacao_Web_Mcar, 
         @Perm_Frm_Logar_Classe_Acessada
    )
    SELECT @Perm_Frm_ID = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SET @Perm_Frm_ID = 
    (    SELECT Perm_Frm_ID
         FROM Perm_Formulario
         WHERE Perm_Frm_NmProjeto = @Perm_Frm_NmProjeto
     );
END;

----------------------- Cadastro Grupo de Trabalho ----------------------------

WHILE(1 = 1)
  BEGIN
    SELECT @LOOP_GRUPOS = MIN(Grupo_ID)
    FROM @Perm_Grupo
    WHERE Grupo_ID > @LOOP_GRUPOS;
    IF @LOOP_GRUPOS IS NULL
      BREAK;
    SET @Grupo_ID = @LOOP_GRUPOS;
    IF NOT EXISTS
    (
        SELECT 1
        FROM Perm_Grupo
        WHERE Perm_Grupo_IsAtivo = 1
              AND Perm_Grupo_NM = (SELECT Perm_Grupo_NM FROM @Perm_Grupo WHERE Grupo_ID =  @Grupo_ID)
              AND Perm_Grupo_DS = (SELECT Perm_Grupo_DS FROM @Perm_Grupo WHERE Grupo_ID =  @Grupo_ID)
    )
    BEGIN
        INSERT INTO Perm_Grupo 
            (
            Perm_Grupo_NM, 
            Perm_Grupo_DS, 
            Enum_Tipo_Grupo_ID, 
            Usuario_Ultima_Alteracao_ID, 
            Perm_Grupo_IsAtivo
            )
        SELECT 
            Perm_Grupo_NM, 
            Perm_Grupo_DS, 
            Enum_Tipo_Grupo_ID, 
            Perm_Grupo_IsAtivo, 
            Usuario_Ultima_Alteracao_ID  
        FROM 
            @Perm_Grupo 
        WHERE 
            Grupo_ID =  @Grupo_ID

        SELECT @Perm_Grupo_ID = SCOPE_IDENTITY();

        SET @Perm_Acao_ID = (
                            SELECT
                                Perm_Acao_ID
                            FROM 
                                @Perm_Grupo 
                            WHERE 
                                Grupo_ID =  @Grupo_ID
                             );
    END;
    ELSE
    BEGIN
    IF (SELECT COUNT(Grupo_ID) FROM @Perm_Grupo) > 0
        SET @Perm_Grupo_ID = 
        ( SELECT Perm_Grupo_ID
          FROM Perm_Grupo
          WHERE Perm_Grupo_IsAtivo = 1
          AND Perm_Grupo_NM = (SELECT Perm_Grupo_NM FROM @Perm_Grupo WHERE Grupo_ID =  @Grupo_ID)
        );
        SET @Perm_Acao_ID = (
                            SELECT
                                Perm_Acao_ID
                            FROM 
                                @Perm_Grupo 
                            WHERE 
                                Grupo_ID =  @Grupo_ID
                             );
    END;

---------------- Incluir Relacionamento Formulario x Ação ---------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Perm_Formulario_REL_Acao
        WHERE Perm_Frm_ID = @Perm_Frm_ID
              AND Perm_Acao_ID = @Perm_Acao_ID
    )
      INSERT INTO Perm_Formulario_REL_Acao
      (   Perm_Frm_ID, 
          Perm_Acao_ID
      )
      VALUES
      (   @Perm_Frm_ID, 
          @Perm_Acao_ID
      );

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
      END;

SET @LOOP_GRUPOS = 0;

---------------------------- Cadastro de Menu ---------------------------------

    WHILE(1 = 1)
      BEGIN
        SELECT @LOOP = MIN(Perm_Modulo_ID)
        FROM @Perm_Modulo
        WHERE Perm_Modulo_ID > @LOOP;
        IF @LOOP IS NULL
          BREAK;
        SET @Perm_Modulo_ID = @LOOP;
        IF NOT EXISTS
        (
          SELECT 1
          FROM Perm_Menu
          WHERE Perm_Menu_NM = @Perm_Menu_NM
        ) AND @Perm_Menu_NM IS NOT NULL AND DATALENGTH(@Perm_Menu_NM) > 0
        BEGIN
        INSERT INTO Perm_Menu 
         ( Perm_Menu_Pai_ID,   
           Perm_Modulo_ID,     
           Perm_Frm_ID,        
           Perm_Menu_NM,       
           Perm_Menu_Ordenacao,     
           Usuario_Ultima_Alteracao_ID
         )   
        Values 
         ( @Perm_Menu_Pai_ID, 
           @Perm_Modulo_ID,   
           @Perm_Frm_ID,   
           @Perm_Menu_NM,   
           @Perm_Menu_Ordenacao,   
           @Usuario_Ultima_Alteracao_ID
          );
        END;
    END;
    SET @LOOP = 0;
  
--------------- Incluir Relacionamento Formulario x Modulo --------------------

    WHILE(1 = 1)
      BEGIN
        SELECT @LOOP = MIN(Perm_Modulo_ID)
        FROM @Perm_Modulo
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
          (   Perm_Frm_ID, 
              Perm_Modulo_ID
          )
          VALUES
          (   @Perm_Frm_ID, 
              @Perm_Modulo_ID
          );
      END;
    SET @LOOP = 0;

GO