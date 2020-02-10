using Mercadocar.Enumerados;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.BancoDados;
using Mercadocar.InfraEstrutura.Erro;
using Mercadocar.InfraEstrutura.SQLinq;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.InfraEstrutura.Validadores;
using Mercadocar.ObjetosNegocio.DataObject;
using MercadoCar.SQLinq;
using MercadoCar.SQLinq.Dynamic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mercadocar.ObjetosNegocio.Data
{
    sealed public class Solicitacao_PagamentoDATA
    {
        #region "   CONSULTAS          "

        public List<Solicitacao_Pagamento_Forma_PagtoDO> Solicitacao_Pagamento_Forma_Pagto(ref TransactionManager objTransaction)
        {
            try
            {
                var dynamicQuery = new DynamicSQLinq("Forma_Pagamento")
                   .Select(
                        "Forma_Pagamento_ID  AS Forma_Pagamento_ID",
                        "Forma_Pagamento_DS  AS Forma_Pagamento_DS");

                dynamicQuery.Where<int>("Forma_Pagamento.Forma_Pagamento_IsAtivo", f => f == 1);
                dynamicQuery.OrderBy("Forma_Pagamento.Forma_Pagamento_DS");

                return dynamicQuery.ToSQL().Query<Solicitacao_Pagamento_Forma_PagtoDO>(objTransaction.ObjetoDeAcessoDados).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }

}