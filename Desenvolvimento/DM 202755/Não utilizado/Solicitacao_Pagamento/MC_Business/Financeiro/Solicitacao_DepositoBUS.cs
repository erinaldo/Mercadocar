using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.BancoDados;
using Mercadocar.InfraEstrutura.Datable;
using Mercadocar.InfraEstrutura.Erro;
using Mercadocar.ObjetosNegocio.Data;
using Mercadocar.ObjetosNegocio.DataObject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercadocar.RegrasNegocio
{
    class Solicitacao_DepositoBUS
    {
        #region "   CRUDE              "

        public void Incluir(Solicitacao_DepositoDO depositoDO, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                new Solicitacao_DepositoDATA().Incluir(depositoDO, objErro, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Alterar(Solicitacao_DepositoDO depositoDO, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                new Solicitacao_DepositoDATA().Alterar(depositoDO, objErro, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   CONSULTAS          "

        public DataRow Consulta_Interface_Deposito(int Consulta_Interface_Deposito, ref TransactionManager objTransaction)
        {
            try
            {
                DataTable dttDeposito = new Solicitacao_DepositoDATA().Consulta_Interface_Deposito(Consulta_Interface_Deposito, ref objTransaction).ToTable("Interface_Deposito");

                return dttDeposito.Rows[0];
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region "  Métodos             "
        public Solicitacao_DepositoDO Preencher_Deposito(DataRow dtrDeposito)
        {
            try
            {
                Solicitacao_DepositoDO dtoDeposito = new Solicitacao_DepositoDO()
                {
                    Interface_Deposito_ID = dtrDeposito["Interface_Deposito_ID"].DefaultInteger(),
                    Solicitacao_Pagamento_ID = dtrDeposito["Solicitacao_Pagamento_ID"].DefaultInteger(),
                    Cliente_CD = dtrDeposito["Cliente_CD"].DefaultInteger(),
                    Cliente_Nome = dtrDeposito["Cliente_Nome"].DefaultString(),
                    Cliente_CPFCNPJ = dtrDeposito["Cliente_CPFCNPJ"].DefaultString(),
                    Banco_CD = dtrDeposito["Banco_CD"].DefaultInteger(),
                    Banco_Agencia = dtrDeposito["Banco_Agencia"].DefaultString(),
                    Banco_Conta = dtrDeposito["Banco_Conta"].DefaultString(),
                    Banco_ContaTipo = dtrDeposito["Banco_ContaTipo"].DefaultString(),
                    Deposito_Valor = dtrDeposito["Deposito_Valor"].DefaultDecimal(),
                    Data_Vencimento = dtrDeposito["Data_Vencimento"].DefaultDateTime(),
                    Processado = dtrDeposito["Processado"].DefaultString(),
                    Data_Processamento = dtrDeposito["Data_Processamento"].DefaultDateTime(),
                    Pago = dtrDeposito["Pago"].DefaultBool(),
                };

                return dtoDeposito;
            }

            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }
}
