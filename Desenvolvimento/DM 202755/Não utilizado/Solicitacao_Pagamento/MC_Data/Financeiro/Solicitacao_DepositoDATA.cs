using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.BancoDados;
using Mercadocar.InfraEstrutura.Erro;
using Mercadocar.InfraEstrutura.SQLinq;
using Mercadocar.InfraEstrutura.Validadores;
using Mercadocar.ObjetosNegocio.DataObject;
using MercadoCar.SQLinq;
using MercadoCar.SQLinq.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mercadocar.ObjetosNegocio.Data
{
    public class Solicitacao_DepositoDATA
    {
        #region "   CRUDE              "

        public void Incluir(Solicitacao_DepositoDO dtoDeposito, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Incluir(dtoDeposito, objErro, ref objTransaction);

                if (objErro.TemErro())
                    return;

                var insert = new SQLinqInsert<Solicitacao_DepositoDO>(dtoDeposito);

                SqlHelper.ExecuteScalar(objTransaction.ObjetoDeAcessoDados, insert).DefaultInteger();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Alterar(Solicitacao_DepositoDO dtoDeposito, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Alterar(dtoDeposito, objErro, ref objTransaction);

                if (objErro.TemErro())
                    return;

                var update = new SQLinqUpdate<Solicitacao_DepositoDO>(dtoDeposito)
                  .Partial(x => x.Cliente_CD)
                  .Partial(x => x.Cliente_Nome)
                  .Partial(x => x.Cliente_CPFCNPJ)
                  .Partial(x => x.Banco_CD)
                  .Partial(x => x.Banco_Agencia)
                  .Partial(x => x.Banco_Conta)
                  .Partial(x => x.Banco_ContaTipo)
                  .Partial(x => x.Data_Vencimento)
                  .Partial(x => x.Pago)
                  .Where(x => x.Interface_Deposito_ID == dtoDeposito.Interface_Deposito_ID);

                SqlHelper.Execute(objTransaction.ObjetoDeAcessoDados, update);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   CONSULTAS          "

        public IEnumerable<Solicitacao_DepositoDO> Consulta_Interface_Deposito(int intSolicitacaoPagamentoID, ref TransactionManager objTransaction)
        {
            try
            {
                var dynamicQuery = new DynamicSQLinq("Interface_Deposito")
                    .Select(
                     "Interface_Deposito_ID     AS Interface_Deposito_ID",
                     "Solicitacao_Pagamento_ID  AS Solicitacao_Pagamento_ID",
                     "E2_CODCLIENTE             AS Cliente_CD",
                     "E2_NOMFAV                 AS Cliente_Nome",
                     "E2_CPFCNPJ                AS Cliente_CPFCNPJ",
                     "E2_BANCO                  AS Banco_CD",
                     "E2_AGENCIA                AS Banco_Agencia",
                     "E2_CONTA                  AS Banco_Conta",
                     "E2_TPCONTA                AS Banco_ContaTipo",
                     "E2_VALOR                  AS Deposito_Valor",
                     "E2_VENCTO                 AS Data_Vencimento",
                     "E2_PROCESSA               AS Processado",
                     "E2_DTPROCESS              AS Data_Processamento",
                     "E2_PAGO                   AS Pago");

                dynamicQuery.Where<int>("Solicitacao_Pagamento_ID", f => f == intSolicitacaoPagamentoID).Distinct(true);

                return dynamicQuery.ToSQL().Query<Solicitacao_DepositoDO>(objTransaction.ObjetoDeAcessoDados);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Validações         "

        private void Validar_Incluir(Solicitacao_DepositoDO dtoDeposito, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Cliente_CD(dtoDeposito.Cliente_CD.DefaultString(), objErro, ref objTransaction);
                this.Validar_Cliente_Nome(dtoDeposito.Cliente_Nome.DefaultString(), objErro);
                this.Validar_Cliente_CPFCNPJ(dtoDeposito.Cliente_CPFCNPJ.DefaultString(), objErro);
                this.Validar_Banco_CD(dtoDeposito.Banco_CD.DefaultString(), objErro, ref objTransaction);
                this.Validar_Banco_Agencia(dtoDeposito.Banco_Agencia.DefaultString(), objErro);
                this.Validar_Banco_Conta(dtoDeposito.Banco_Conta.DefaultString(), objErro);
                this.Validar_Banco_ContaTipo(dtoDeposito.Banco_ContaTipo.DefaultString(), objErro);
                this.Validar_Valor(dtoDeposito.Deposito_Valor.DefaultString(), objErro);
                this.Validar_Data_Vencimento(dtoDeposito.Data_Vencimento.DefaultString(), objErro);
                this.Validar_Processado(dtoDeposito.Processado.DefaultString(), objErro);
                this.Validar_Pago(dtoDeposito.Pago.DefaultString(), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Validar_Alterar(Solicitacao_DepositoDO dtoDeposito, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Cliente_CD(dtoDeposito.Cliente_CD.DefaultString(), objErro, ref objTransaction);
                this.Validar_Cliente_Nome(dtoDeposito.Cliente_Nome.DefaultString(), objErro);
                this.Validar_Cliente_CPFCNPJ(dtoDeposito.Cliente_CPFCNPJ.DefaultString(), objErro);
                this.Validar_Banco_CD(dtoDeposito.Banco_CD.DefaultString(), objErro, ref objTransaction);
                this.Validar_Banco_Agencia(dtoDeposito.Banco_Agencia.DefaultString(), objErro);
                this.Validar_Banco_Conta(dtoDeposito.Banco_Conta.DefaultString(), objErro);
                this.Validar_Banco_ContaTipo(dtoDeposito.Banco_ContaTipo.DefaultString(), objErro);
                this.Validar_Valor(dtoDeposito.Deposito_Valor.DefaultString(), objErro);
                this.Validar_Data_Vencimento(dtoDeposito.Data_Vencimento.DefaultString(), objErro);
                this.Validar_Processado(dtoDeposito.Processado.DefaultString(), objErro);
                this.Validar_Pago(dtoDeposito.Pago.DefaultString(), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Cliente_CD(String strValorCampo, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                ValidaBanco objValidaBanco = new ValidaBanco();

                objValidaBanco.Valida_Chave_Extrangeira("Cliente_CD", "Cliente_CD", strValorCampo, "Cliente_IsAtivo", "Código do Cliente para o Depósito", "Cliente", string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro, ref objTransaction);

                if ((objErro.ObterErro("Cliente_CD") != null))
                    return;

                ValidaCampo objValidaCampo = new ValidaCampo();
                objValidaCampo.ValidaCampoObrigatorio("Cliente_CD", strValorCampo, "Código do Cliente", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Cliente_Nome(string strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaCampoObrigatorio("Cliente_Nome", strValorCampo, "Cliente", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaTamanhoMaximo("Cliente_Nome", strValorCampo, "Cliente", 150, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Cliente_CPFCNPJ(string strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaCampoObrigatorio("Cliente_CPFCNPJ", strValorCampo, "Cliente", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaTamanhoMaximo("Cliente_CPFCNPJ", strValorCampo, "Cliente", 15, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Banco_CD(string strValorCampo, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();
                objValidaCampo.ValidaCampoObrigatorio("Banco_CD", strValorCampo, "Código do Banco", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaSoNumeros("Banco_CD", strValorCampo, "Código do Banco", string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Banco_Agencia(String strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaTamanhoMaximo("Solicitacao_Pagamento_Banco_Agencia", strValorCampo, "Agência Bancária", 20, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaCampoObrigatorio("Solicitacao_Pagamento_Banco_Agencia", strValorCampo, "Agência Bancária", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Banco_Conta(String strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaTamanhoMaximo("Solicitacao_Pagamento_Banco_Conta", strValorCampo, "Conta Bancária", 30, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaCampoObrigatorio("Solicitacao_Pagamento_Banco_Conta", strValorCampo, "Conta Bancária", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Banco_ContaTipo(string strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaTamanhoMaximo("Banco_ContaTipo", strValorCampo, "Tipo de Conta Bancária", 1, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaCampoObrigatorio("Banco_ContaTipo", strValorCampo, "Tipo de Conta Bancária", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Valor(String strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaTamanhoMaximo("Solicitacao_Pagamento_Valor", strValorCampo, "Valor do crédito da Solicitação de Pagamento", 50, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
                objValidaCampo.ValidaCampoObrigatorio("Solicitacao_Pagamento_Valor", strValorCampo, "Valor do crédito da Solicitação de Pagamento", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Data_Vencimento(string strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaCampoObrigatorio("Data_Vencimento", strValorCampo, "Data Vencimento do depósito", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Processado(string strValorCampo, Erro objErro)
        {
            try
            {
                if (strValorCampo == String.Empty)
                {
                    return;
                }

                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaTamanhoMaximo("Processado", strValorCampo, "Depósito processado", 1, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Pago(string strValorCampo, Erro objErro)
        {
            try
            {
                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaCampoObrigatorio("Pago", strValorCampo, "Depósito realizado", true, string.Concat(this.GetType().Name, ".", MethodBase.GetCurrentMethod().Name), objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}
