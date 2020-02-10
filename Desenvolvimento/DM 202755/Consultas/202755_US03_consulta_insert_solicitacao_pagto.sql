USE MCAR_Desenvolvimento
GO

/*
Informações avançadas:             
     Tipo de exceção: Exception    

Detalhes da exceção:               
     ex.message:                   Implicit conversion from data type nvarchar to varbinary(max) is not allowed. Use the CONVERT function to run this query.
     ex.Source:                    .Net SqlClient Data Provider
     ex.TargetSite:                OnError

     ex.StackTrace:                   em System.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   em System.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   em System.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   em System.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
   em System.Data.SqlClient.SqlDataReader.TryConsumeMetaData()
   em System.Data.SqlClient.SqlDataReader.get_MetaData()
   em System.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString, Boolean isInternal, Boolean forDescribeParameterEncryption, Boolean shouldCacheForAlwaysEncrypted)
   em System.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean async, Int32 timeout, Task& task, Boolean asyncWrite, Boolean inRetry, SqlDataReader ds, Boolean describeParameterEncryptionRequest)
   em System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry)
   em System.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)
   em System.Data.SqlClient.SqlCommand.ExecuteScalar()
   em Dapper.SqlMapper.ExecuteScalarImpl[T](IDbConnection cnn, CommandDefinition& command)
   em Dapper.SqlMapper.ExecuteScalar(IDbConnection cnn, String sql, Object param, IDbTransaction transaction, Nullable`1 commandTimeout, Nullable`1 commandType)
   em MercadoCar.SQLinq.Dapper.IDbConnectionExtensions.ExecuteScalarSQLinq(IDbConnection dbconnection, ISQLinq sqlinq, IDbTransaction transaction, Nullable`1 commandTimeout, Nullable`1 commandType) na E:\M\Default\Outros Projetos\SQLinq\DapperExtensions\SQLinq.Dapper\DapperExtensions\IDbConnectionExtensions.cs:linha 50
   em Mercadocar.InfraEstrutura.BancoDados.SqlHelper.ExecuteScalar(Object objetoAcessoDados, ISQLinq sqlinq) na D:\SC\Mercadocar v1\SIM\Front-SIM\MC_Infra_Estrutura_Net\BancoDados_SqlHelper.cs:linha 667
   em Mercadocar.ObjetosNegocio.Data.Solicitacao_PagamentoDATA.Incluir(Solicitacao_PagamentoDO dtoSolicitacaoPagamento, Erro objErro, TransactionManager& objTransaction) na D:\SC\Mercadocar v1\SIM\Front-SIM\MC_Data\Financeiro\Solicitacao_PagamentoDATA.cs:linha 47
   em Mercadocar.RegrasNegocio.Solicitacao_PagamentoBUS.Incluir(Solicitacao_PagamentoDO dtoIncluir, TransactionManager& objTransaction) na D:\SC\Mercadocar v1\SIM\Front-SIM\MC_Business\Financeiro\Solicitacao_PagamentoBUS.cs:linha 68
   em Mercadocar.RegrasNegocio.Solicitacao_PagamentoBUS.Incluir(Solicitacao_PagamentoDO dtoIncluir) na D:\SC\Mercadocar v1\SIM\Front-SIM\MC_Business\Financeiro\Solicitacao_PagamentoBUS.cs:linha 49
   em Mercadocar.Formularios.frmRomaneio_Grid.Efetivar_Solcitacao_Deposito(UsuarioDO dtoUsuario, DataRowView objRow) na D:\SC\Mercadocar v1\SIM\Front-SIM\MC_Formularios\Venda Tecnica\Pesquisa\frmRomaneio_Grid.cs:linha 3613
   em Mercadocar.Formularios.frmRomaneio_Grid.Clicar_Menu_Aprovar(Object sender, EventArgs e) na D:\SC\Mercadocar v1\SIM\Front-SIM\MC_Formularios\Venda Tecnica\Pesquisa\frmRomaneio_Grid.cs:linha 388

Informações sobre o sistema:       
     Versão Sistema SIM:           3.408
     Ultima atualização em:        14/01/2020 09:27:32
     Módulo Ativo:                 Venda Técnica


*/

DECLARE 
@sqlinq_1 INT = 1,
@sqlinq_2 INT = 987665132,
@sqlinq_3 INT = 0,
@sqlinq_4 INT = 2791,
@sqlinq_5 INT = 1598,
@sqlinq_6 INT = 7533,
@sqlinq_7 INT = 0,
@sqlinq_8 INT = 2,
@sqlinq_9 VARCHAR = '295',
@sqlinq_10 VARCHAR = '22991',
@sqlinq_11 DECIMAL = 0.30,
@sqlinq_12 VARCHAR = NULL,
@sqlinq_13 INT = 0,
@sqlinq_14 VARCHAR = NULL,
@sqlinq_15 DATE = NULL,
@sqlinq_16 DATE = '2020-01-13 17:25:51', --'13/01/2020 17:25:51',
@sqlinq_17 DATE = NULL,
@sqlinq_18 VARBINARY = NULL,
@sqlinq_19 VARCHAR = NULL

INSERT Solicitacao_Pagamento (
Lojas_ID, 
Objeto_Origem_ID, 
Enum_Tipo_Pagamento_ID, 
Enum_Origem_ID, 
Enum_Status_ID, 
Usuario_Criacao_ID, 
Usuario_Ultima_Alteracao_ID, 
Banco_ID, 
Solicitacao_Pagamento_Banco_Agencia, 
Solicitacao_Pagamento_Banco_Conta, 
Solicitacao_Pagamento_Valor, 
Solicitacao_Pagamento_Credito_Online, 
Enum_Motivo_Recusa_Pagamento_ID, 
Solicitacao_Pagamento_Comprovante_Estorno, 
Solicitacao_Pagamento_Data_Pagamento, 
Solicitacao_Pagamento_Data_Criacao, 
Solicitacao_Pagamento_Data_Ultima_Alteracao, 
Solicitacao_Pagamento_Comprovante_Pgto, 
Solicitacao_Pagamento_Obs) 
VALUES (@sqlinq_1, @sqlinq_2, @sqlinq_3, @sqlinq_4, @sqlinq_5, @sqlinq_6, @sqlinq_7, @sqlinq_8, @sqlinq_9, @sqlinq_10, @sqlinq_11, @sqlinq_12, @sqlinq_13, @sqlinq_14, @sqlinq_15, @sqlinq_16, @sqlinq_17, @sqlinq_18, @sqlinq_19); SELECT SCOPE_IDENTITY();

SELECT * FROM Solicitacao_Pagamento

