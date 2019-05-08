DECLARE @LOJA_ID    AS int,
        @DT_CRIACAO AS date,
        @ID_Usuario AS int

SET @LOJA_ID = 3
SET @DT_CRIACAO = '2019-04-04' --2019-04-03
SET @ID_Usuario = 11007

--SELECT * FROM Endereco_Separador_CT WHERE Usuario_Separacao_ID = 11007

SELECT
  Usuario_ID,
  dbo.fun_Retorna_Dois_Nomes(Usuario_Nome_Completo) AS Nome,
  Romaneio_Pre_Venda_CT_ID,

  (SELECT
    COUNT(Separacao_Venda_IT.Separacao_Venda_IT_ID)
  FROM Separacao_Venda_IT
  WHERE Separacao_Venda_IT.Separacao_Venda_CT_ID = Separacao_Venda_CT.Separacao_Venda_CT_ID
    AND Separacao_Venda_IT.Lojas_ID = Separacao_Venda_CT.Lojas_ID)
  AS Itens,

  Separacao_Venda_CT_Data_Criacao AS Criacao,
  Separacao_Venda_CT_Data_Inicio_Separacao AS Inicio,
  Separacao_Venda_CT_Data_Termino_Separacao AS Termino,

  -- O Problema está no calculo do tempo médio de separação
  -- que usa o Início e Término para este cálculo.
  -- O término está igual para algumas separações.
  CONVERT(varchar(8), DATEADD(SECOND,
  (AVG(DATEDIFF(SECOND,
  Separacao_Venda_CT.Separacao_Venda_CT_Data_Inicio_Separacao,
  Separacao_Venda_CT.Separacao_Venda_CT_Data_Termino_Separacao))), 0), 14) AS Tempo_Separacao

  INTO  
   #Temp_Result

FROM Separacao_Venda_CT,
     Endereco_Separador_CT
     INNER JOIN Usuario
       ON Usuario.Usuario_ID = Endereco_Separador_CT.Usuario_Separacao_ID

WHERE CONVERT(date, Separacao_Venda_CT_Data_Criacao) = CONVERT(date, @DT_CRIACAO)
AND Separacao_Venda_CT.Lojas_ID = @LOJA_ID
AND Usuario_Separacao_ID = @ID_Usuario

GROUP BY Separacao_Venda_CT.Lojas_ID,
         Usuario_ID,
         Usuario_Nome_Completo,
         Separacao_Venda_CT_Data_Criacao,
         Separacao_Venda_CT_Data_Inicio_Separacao,
         Separacao_Venda_CT_Data_Termino_Separacao,
         Romaneio_Pre_Venda_CT_ID,
         Separacao_Venda_CT_ID

SELECT 
         Usuario_ID,
         Nome,
         Romaneio_Pre_Venda_CT_ID,
         Criacao,
         Inicio,
         Termino,
         Tempo_Separacao
FROM #Temp_Result

--SELECT COUNT(Romaneio_Pre_Venda_CT_ID) AS Total_romaneios, CONCAT(SUM(DATEDIFF(HOUR, '0:00:00',Tempo_Separacao)), ' horas') AS Total_tempo FROM #Temp_Result

--SELECT Inicio, COUNT(Inicio) AS Ocorrências FROM #Temp_Result GROUP BY Inicio HAVING COUNT(*) > 1

SELECT Termino, COUNT(Termino) AS Ocorrencias FROM #Temp_Result GROUP BY Termino HAVING COUNT(*) > 1

SELECT CONCAT(AVG(DATEDIFF(MINUTE, '0:00:00',Tempo_Separacao)), ' minutos') AS Media_Dia FROM #Temp_Result

SELECT CONCAT(SUM(DATEDIFF(HOUR,  '0:00:00',Tempo_Separacao)), ' horas') AS Soma_Dia FROM #Temp_Result

DROP TABLE #Temp_Result