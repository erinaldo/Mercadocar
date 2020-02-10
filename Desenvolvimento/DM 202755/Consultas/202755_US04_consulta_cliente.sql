SELECT
CAST(ISNULL(Cliente.Cliente_Nome, '') AS VARCHAR(200)) AS Cliente_Nome,
CAST(Cliente.Cliente_ID AS VARCHAR(40)) AS Cliente_ID,
Cliente.Cliente_CPF_CNPJ AS Cliente_CPF_CNPJ,
Contato_DDD,
Contato_Telefone
FROM Cliente
Join Contato 
ON Cliente.Pessoa_ID = Contato.Pessoa_ID
WHERE
Cliente_ID = '807b29d3-2719-4b9a-ad0d-b91e83619e8f'

SELECT
CAST(ISNULL(Cliente.Cliente_Nome, '') AS VARCHAR(200)) AS Cliente_Nome,
CAST(Cliente.Cliente_ID AS VARCHAR(40)) AS Cliente_ID,
ContatoVirtual_Email
FROM Cliente
Join ContatoVirtual
ON Cliente.Pessoa_ID = ContatoVirtual.Pessoa_ID
WHERE
Cliente_ID = '807b29d3-2719-4b9a-ad0d-b91e83619e8f'

SELECT
CAST(ISNULL(Cliente.Cliente_Nome, '') AS VARCHAR(200)) AS Cliente_Nome,
CAST(Cliente.Cliente_ID AS VARCHAR(40)) AS Cliente_ID,
Conta.Banco_ID AS Banco_ID,
Banco_Nome,
Agencia_CD,
Conta_CD,
Conta_Digito
FROM Cliente
Join Conta
ON Cliente.Pessoa_ID = Conta.Pessoa_ID
Join Agencia
ON Conta.Agencia_ID = Agencia.Agencia_ID
Join Banco
ON Conta.Banco_ID = Banco.Banco_ID
WHERE
Cliente_ID = '807b29d3-2719-4b9a-ad0d-b91e83619e8f'
AND Conta_IsAtivo = 1
AND Banco_IsAtivo = 1
