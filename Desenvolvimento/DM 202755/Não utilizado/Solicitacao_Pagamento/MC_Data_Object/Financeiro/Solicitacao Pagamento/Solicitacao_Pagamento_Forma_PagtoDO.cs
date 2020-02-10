using MercadoCar.SQLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercadocar.ObjetosNegocio.DataObject
{
    [SQLinqTable("Forma_Pagamento")]
    public class Solicitacao_Pagamento_Forma_PagtoDO
    {
        #region "   Declarações         "

        #endregion

        #region "   Construtores       "
        public Solicitacao_Pagamento_Forma_PagtoDO() : base()
        {
        }
        #endregion

        #region "   Propriedades       "

        [SQLinqColumn("Forma_Pagamento_ID", Insert = false, Update = true)]
        public int Forma_Pagamento_ID { get; set; }

        [SQLinqColumn("Enum_Forma_Faturamento_Venda_ID")]
        public int Enum_Forma_Faturamento_Venda_ID { get; set; }

        [SQLinqColumn("Forma_Pagamento_DS")]
        public String Forma_Pagamento_DS { get; set; }

        [SQLinqColumn("Forma_Pagamento_Troco")]
        public bool Forma_Pagamento_Troco { get; set; }

        [SQLinqColumn("Forma_Pagamento_Limite")]
        public bool Forma_Pagamento_Limite { get; set; }

        [SQLinqColumn("Forma_Pagamento_Desconto")]
        public bool Forma_Pagamento_Desconto { get; set; }

        [SQLinqColumn("Forma_Pagamento_Emissao_Cheque")]
        public bool Forma_Pagamento_Emissao_Cheque { get; set; }

        [SQLinqColumn("Forma_Pagamento_Emissao_Cartao_Debito")]
        public bool Forma_Pagamento_Emissao_Cartao_Debito { get; set; }

        [SQLinqColumn("Forma_Pagamento_Emissao_Cartao_Credito")]
        public bool Forma_Pagamento_Emissao_Cartao_Credito { get; set; }

        [SQLinqColumn("Forma_Pagamento_Emissao_Fatura")]
        public bool Forma_Pagamento_Emissao_Fatura { get; set; }

        [SQLinqColumn("Forma_Pagamento_Desconto_Ato")]
        public bool Forma_Pagamento_Desconto_Ato { get; set; }

        [SQLinqColumn("Forma_Pagamento_IsAtivo")]
        public bool Forma_Pagamento_IsAtivo { get; set; }

        [SQLinqColumn("Forma_Pagamento_Gera_Boleto")]
        public bool Forma_Pagamento_Gera_Boleto { get; set; }

        #endregion
    }
}
