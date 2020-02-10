using MercadoCar.SQLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercadocar.ObjetosNegocio.DataObject
{
    [SQLinqTable("Interface_Deposito")]
    public class Solicitacao_DepositoDO
    {
        #region "   Construtores       "

        public Solicitacao_DepositoDO() : base()
        {
        }

        #endregion

        #region "   Propriedades       "

        [SQLinqColumn("Interface_Deposito_ID", Insert = false, Update = true)]
        public int Interface_Deposito_ID { get; set; }

        [SQLinqColumn("Solicitacao_Pagamento_ID")]
        public int Solicitacao_Pagamento_ID { get; set; }

        [SQLinqColumn("E2_CODCLIENTE")]
        public int Cliente_CD { get; set; }

        [SQLinqColumn("E2_NOMFAV")]
        public string Cliente_Nome { get; set; }

        [SQLinqColumn("E2_CPFCNPJ")]
        public string Cliente_CPFCNPJ { get; set; }

        [SQLinqColumn("E2_BANCO")]
        public int Banco_CD { get; set; }

        [SQLinqColumn("E2_AGENCIA")]
        public string Banco_Agencia { get; set; }

        [SQLinqColumn("E2_CONTA")]
        public string Banco_Conta { get; set; }

        [SQLinqColumn("E2_TPCONTA")]
        public string Banco_ContaTipo { get; set; }

        [SQLinqColumn("E2_VALOR")]
        public decimal Deposito_Valor { get; set; }

        [SQLinqColumn("E2_VENCTO")]
        public DateTime Data_Vencimento { get; set; }

        [SQLinqColumn("E2_PROCESSA")]
        public string Processado { get; set; }

        [SQLinqColumn("E2_DTPROCESS")]
        public DateTime? Data_Processamento { get; set; }

        [SQLinqColumn("E2_PAGO")]
        public bool Pago { get; set; }

        #endregion
    }
}