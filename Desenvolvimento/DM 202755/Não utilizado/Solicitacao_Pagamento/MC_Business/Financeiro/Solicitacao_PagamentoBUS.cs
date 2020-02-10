
namespace Mercadocar.RegrasNegocio
{
    public class Solicitacao_PagamentoBUS
    {

        #region "   CRUDE              "

        public bool Alterar(DataRow dtrAlterar)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidorCentral);
                objTransaction.BeginTransaction();

                this.Alterar(dtrAlterar, ref objTransaction);

                if (objTransaction != null)
                {
                    objTransaction.Commit();
                }

                return true;
            }
            catch (Exception)
            {
                if (objTransaction != null)
                    objTransaction.RollBack();
                throw;
            }
        }

        public bool Alterar(DataRow dtrAlterar, ref TransactionManager objTransaction)
        {
            try
            {
                Solicitacao_PagamentoDATA datSolicitacaoPagamento = new Solicitacao_PagamentoDATA();
                Erro objErro = new Erro();

                DBUtil objUtil = new DBUtil();

                dtrAlterar["Usuario_Ultima_Alteracao_ID"] = Root.Funcionalidades.UsuarioDO_Ativo.ID;
                dtrAlterar["Solicitacao_Pagamento_Usuario_Ultima_Alteracao"] = Root.Funcionalidades.UsuarioDO_Ativo.Nome_Completo;
                dtrAlterar["Solicitacao_Pagamento_Data_Ultima_Alteracao"] = objUtil.Obter_Data_do_Servidor(true, ref objTransaction);

                if (dtrAlterar["Enum_Origem_ID"].DefaultInteger() == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                {
                    if (Validar_Solicitacao_Pagamento_Finalizada(dtrAlterar["Enum_Status_ID"].DefaultInteger()) == false)
                    {
                        dtrAlterar["Solicitacao_Pagamento_Data_Pagamento"] = Utilitario.Obter_DateTime_Valor_Minimo_DB();
                    }
                    else
                    {
                        Solicitacao_PagamentoBUS.Enviar_Email_Cliente(dtrAlterar);
                    }

                    if (dtrAlterar["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Efetuado.DefaultInteger())
                    {
                        SAC_CTBUS busSACCT = new SAC_CTBUS();

                        // Caso nao tenha sido gerado o pagamento, eu o faco
                        // Conferenncia com divergencia gera pagamento posteriores por exemplo, pois nao tem laudo
                        if (dtrAlterar["Romaneio_CT_ID"].DefaultInteger() == 0)
                        {
                            // Removido, o pagamento sera feito na aprovacao do laudo
                            DataTable dttRomaneioCredito = busSACCT.Gerar_Romaneio_Credito_Liberado(dtrAlterar["Objeto_Origem_ID"].DefaultInteger(), ref objTransaction);
                            dtrAlterar["Solicitacao_Pagamento_Data_Pagamento"] = objUtil.Obter_Data_do_Servidor(true, ref objTransaction);
                            dtrAlterar["Solicitacao_Pagamento_Comprovante_Estorno"] = dttRomaneioCredito.Rows[0]["Romaneio_CT_ID"].DefaultString();
                        }

                        busSACCT.Efetuar_Pagamento_Bancario(dtrAlterar, ref objTransaction);
                    }

                    if (dtrAlterar["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Negado.DefaultInteger())
                    {
                        SAC_CTBUS busSACCT = new SAC_CTBUS();
                        busSACCT.Tratar_DataSet(dtrAlterar["Objeto_Origem_ID"].DefaultInteger(), ref objTransaction);
                    }
                }

                if (dtrAlterar["Enum_Origem_ID"].DefaultInteger() == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                {
                    if (Validar_Solicitacao_Pagamento_Finalizada(dtrAlterar["Enum_Status_ID"].DefaultInteger()))
                    {
                        dtrAlterar["Solicitacao_Pagamento_Data_Pagamento"] = objUtil.Obter_Data_do_Servidor(true, ref objTransaction);

                        Solicitacao_DepositoBUS solicitacaoDepositoBUS = new Solicitacao_DepositoBUS();

                        DataRow dtrDeposito = solicitacaoDepositoBUS.Consulta_Interface_Deposito(dtrAlterar["Solicitacao_Pagamento_ID"].DefaultInteger(), ref objTransaction);

                        if (dtrAlterar["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Efetuado.ToInteger())
                        {
                            dtrDeposito["Pago"] = true;
                        }

                        Solicitacao_DepositoDO dtoDeposito = solicitacaoDepositoBUS.Preencher_Deposito(dtrDeposito);

                        solicitacaoDepositoBUS.Alterar(dtoDeposito, objErro, ref objTransaction);
                    }
                }

                datSolicitacaoPagamento.Alterar(dtrAlterar, objErro, ref objTransaction);

                if (objErro.TemErro())
                {
                    throw new McException(objErro);
                }
                else if (dtrAlterar["Enum_Origem_ID"].DefaultInteger() == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                {
                    Solicitacao_PagamentoBUS.Enviar_Email_Cliente(dtrAlterar);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   CONSULTAS          "

        public List<Solicitacao_Pagamento_Forma_PagtoDO> Obter_DataObject_Forma_Pagamento()
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidorCentral);

                return this.Obter_DataObject_Forma_Pagamento(ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.CloseConnection();
                }
            }
        }

        private List<Solicitacao_Pagamento_Forma_PagtoDO> Obter_DataObject_Forma_Pagamento(ref TransactionManager objTransaction)
        {
            try
            {
                return new Solicitacao_PagamentoDATA().Consultar_Solicitacao_Pagamento_Forma_Pagto(ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #endregion

    }

}