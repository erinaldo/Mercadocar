using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Mercadocar.Componentes;
using Mercadocar.Enumerados;
using Mercadocar.Herancas;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.ObjetosNegocio.DataObject;
using Mercadocar.RegrasNegocio;
using Microsoft.VisualBasic;

namespace Mercadocar.Formularios
{
    public sealed partial class frmRecebimento_Nota_Fiscal_Propriedades_New : frmPropriedades, Interfaces.IfrmPropriedades
    {
        #region "   Declarações         "

        private Recebimento_CTDO dtoPropriedades = new Recebimento_CTDO();

        private DataSet dtsRecebimento = new DataSet();
        private DataSet dtsRecebimentoTemporario = new DataSet();

        private Condicao_Pagamento_CTDO dtoCondicaoPagamento = new Condicao_Pagamento_CTDO();
        private Peca_Codigo_FornecedorDO dtoPecaCodigoFornecedorIncluir = new Peca_Codigo_FornecedorDO();

        private DataTable dttLojas = new DataTable();
        private DataTable dttCST = null;
        private DataTable dttOrdemDesembarqueNF = new DataTable();
        private DataTable dttItensExcluidos = new DataTable();
        private DataTable dttpecaCodigosFcornecedorDesativar = new DataTable();

        private int intRecebimentoCTID = 0;
        private int intFornecedorID = 0;
        private int intPecaID = 0;
        private int intOrdemDesembarqueNFID = 0;
        private int intClassificacaoFiscalNativaPecaID = 0;

        private bool blnNotaCancelada = false;
        private bool blnSomenteLeituraParcela = false;

        private string strFornCD = string.Empty;
        private string strFornCNPJCPF = string.Empty;
        private string strClassificacaoFiscalNativaPeca = string.Empty;

        private BindingSource bdsVolumesPreRecebimento = new BindingSource();

        private const int CONDICAO_PAGAMENTO_A_VISTA_ID = 156;

        enum Status_Recebimento_Pedido_Item
        {
            Diferenca_Embalagem,
            Diferenca_Valor,
            Diferenca_Quantidade
        }

        #endregion

        #region "   Construtor          "

        public frmRecebimento_Nota_Fiscal_Propriedades_New()
            : base()
        {
            this.InitializeComponent();
            try
            {
                this.Carregar_Eventos();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public frmRecebimento_Nota_Fiscal_Propriedades_New(int intRecebimentoCTID)
            : this()
        {
            try
            {
                this.intRecebimentoCTID = intRecebimentoCTID;

            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Inicialização       "

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Configurar_Grid_Grupo();
                this.Configurar_Grid_Pre_Recebimento();
                this.Configurar_Grid_Lote_Entrada();
                this.Configurar_Grid_Volume_Conferido();

                this.Configurar_Formulario();
                this.Configurar_Grid_NFe();
                this.Configurar_Grid_Pedido_Itens();
                this.Configurar_Grid_Pedidos_Compra();
                this.Configurar_Grid_Pedidos_Parcelas();
                this.Criar_Colunas_DataTable_Codigo_Fornecedor();

                this.Carregar_Combos_Aba_Dados_Nota_Fiscal();

                this.Carregar_Tela();

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region "   Eventos             "

        private void Pressionar_Tecla_Campos_Inteiros(object sender, KeyPressEventArgs e)
        {
            try
            {
                DivUtil objUtil = new DivUtil();

                if ((Int16)objUtil.Permitir_Digitacao_Somente_Numeros(Convert.ToInt16(Strings.Asc(e.KeyChar))) == 0)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Pressionar_Tecla_Campos_Texto(object sender, KeyPressEventArgs e)
        {
            try
            {
                DivUtil objUtil = new DivUtil();

                if ((Int16)objUtil.Permitir_Digitacao_AlfaNumerico(Convert.ToInt16(e.KeyChar), false, false) == 0)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            try
            {
                if (!object.ReferenceEquals(Cursor.Current, Cursors.WaitCursor))
                {
                    if ((object.ReferenceEquals(this.ActiveControl, this.txtOrdemDesembarqueNumero)))
                    {
                        if ((msg.WParam.ToInt32() == Convert.ToInt32(Keys.F4)))
                        {
                            this.btnPesquisarOrdemDesembarque.PerformClick();
                            return true;
                        }
                    }
                    else if ((object.ReferenceEquals(this.ActiveControl, this.txtEmitenteCNPJCPF)))
                    {
                        if ((msg.WParam.ToInt32() == Convert.ToInt32(Keys.F4)))
                        {
                            this.btnPesquisarEmitente.PerformClick();
                            return true;
                        }
                    }
                    else if ((object.ReferenceEquals(this.ActiveControl, this.txtCondicaoPagamento)))
                    {
                        if ((msg.WParam.ToInt32() == Convert.ToInt32(Keys.F4)))
                        {
                            this.btnPesquisarCondicaoPagamento.PerformClick();
                            return true;
                        }
                    }
                    else if ((object.ReferenceEquals(this.ActiveControl, this.txtCodigoClassFiscal)))
                    {
                        if ((msg.WParam.ToInt32() == Convert.ToInt32(Keys.F4)))
                        {
                            this.btnPesquisarClassFiscal.PerformClick();
                            return true;
                        }
                    }
                    else if ((object.ReferenceEquals(this.ActiveControl, this.txtCodigoFabricante)))
                    {
                        if ((msg.WParam.ToInt32() == Convert.ToInt32(Keys.F4)))
                        {
                            this.btnPesquisaCodigoFabricante.PerformClick();
                            return true;
                        }
                    }
                    else if ((object.ReferenceEquals(this.ActiveControl, this.dgvNotaFiscalItens)))
                    {
                        if ((msg.WParam.ToInt32() == Convert.ToInt32(Keys.Right)))
                        {
                            if (this.dgvNotaFiscalItens.Grid01.Visible && this.dtsRecebimento.Relations.Count < 2)
                            {
                                return true;
                            }
                        }

                    }
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
                return false;
            }
        }

        private void Perder_Foco_Campo(object sender, EventArgs e)
        {
            try
            {
                if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count == 0)
                {
                    return;
                }

                if (sender.GetType() == typeof(ComboBox))
                {
                    ComboBox objComboBox = (ComboBox)sender;

                    if (objComboBox.Name.Equals(this.cboNotaFiscalNumero.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Nota_Fiscal"] = this.cboNotaFiscalNumero.SelectedValue.DefaultInteger();
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Serie"] = this.lblNotaFiscalSerie.Text.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboDestinatarioLoja.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Loja_ID"] = this.cboDestinatarioLoja.SelectedValue.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboLojaRecebimento.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Loja_Entrega_ID"] = this.cboLojaRecebimento.SelectedValue.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboTipoRecebimento.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"] = this.cboTipoRecebimento.SelectedValue.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboTipoOperacao.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Operacao_ID"] = this.cboTipoOperacao.SelectedValue.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboNaturezaOperacao.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Natureza_Operacao1_ID"] = this.cboNaturezaOperacao.SelectedValue.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboNaturezaFinanceira.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Natureza_Financeira_ID"] = this.cboNaturezaFinanceira.SelectedValue.DefaultInteger();
                    }
                    else if (objComboBox.Name.Equals(this.cboModelo.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Modelo_ID"] = this.cboModelo.SelectedValue.DefaultInteger();
                    }
                }
                else if (sender.GetType() == typeof(MC_MaskEdit))
                {
                    MC_MaskEdit objMCMaskEdit = (MC_MaskEdit)sender;

                    if (objMCMaskEdit.Name.Equals(this.mskValorTotalFrete.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Frete"] = this.mskValorTotalFrete.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalSeguro.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Seguro"] = this.mskValorTotalSeguro.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalDesconto.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Desconto"] = this.mskValorTotalDesconto.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalProdutos.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Produtos"] = this.mskValorTotalProdutos.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalOutros.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Outros"] = this.mskValorTotalOutros.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalICMS.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_ICMS"] = this.mskValorTotalICMS.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalBaseCalculoICMSSubstituicao.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Substituicao"] = this.mskValorTotalBaseCalculoICMSSubstituicao.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalICMSSubstituicao.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_ICMS_Substituicao"] = this.mskValorTotalICMSSubstituicao.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalIPI.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_IPI"] = this.mskValorTotalIPI.Text;
                    }
                    else if (objMCMaskEdit.Name.Equals(this.mskValorTotalNotaFiscal.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Total"] = this.mskValorTotalNotaFiscal.Text;
                    }
                }
                else if (sender.GetType() == typeof(TextBox))
                {
                    TextBox txtTextBox = (TextBox)sender;

                    if (txtTextBox.Name.Equals(this.txtNotaFiscalNumeroControle.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Controle"] = this.txtNotaFiscalNumeroControle.Text.DefaultInteger();
                    }
                    else if (txtTextBox.Name.Equals(this.txtChaveAcesso.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Chave_Acesso"] = this.txtChaveAcesso.Text;
                    }
                    else if (txtTextBox.Name.Equals(this.txtObservacoes.Name))
                    {
                        this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Observacao"] = this.txtObservacoes.Text;
                    }
                    else if (txtTextBox.Name.Equals(this.txtCondicaoPagamento.Name))
                    {
                        if (this.dtoCondicaoPagamento != null)
                        {
                            this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Condicao_Pagamento_CT_ID"] = this.dtoCondicaoPagamento.ID;
                        }
                    }

                }
                else if (sender.GetType() == typeof(DateTimePicker))
                {
                    this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Data_Lancamento"] = this.lblNotaFiscalLancamento.Text.DefaultDateTime();
                    this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Data_Saida"] = this.dtpNotaFiscalSaida.Value;
                }


                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #region "   Aba Dados Nota Fiscal     "

        private void Mudar_Selecao_Aba_Principal(object sender, EventArgs e)
        {
            try
            {
                this.Habilitar_Legenda(this.tbcHerdado.SelectedTab == this.tbpCompras);
                this.Visualizar_BotaoGerarPreRecimento();
                this.Visualizar_Botao_Lote_Devolucao();

                if (this.tbcHerdado.SelectedTab == this.tbpCompras)
                {
                    if (this.dttCST == null)
                    {
                        // Carrega a situação tribuitária caso a aba de itens seja aberta a primeira vez
                        this.dttCST = Utilitario.Obter_DataTable_Enumerado_Da_Memoria("CodigoSituacaoTributaria", String.Empty, "Enum_Extenso");
                    }

                    if (this.dtsRecebimento.Tables.Count == 0)
                    {
                        return;
                    }

                    if (this.dtsRecebimento != null && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"].DefaultInteger()) != Status_Recebimento.Devolvido_Integralmente)
                    {
                        if (this.cboTipoRecebimento.SelectedValue == null && this.intRecebimentoCTID != 0)
                        {
                            MessageBox.Show("Selecione primeiro um tipo de recebimento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                            this.cboTipoRecebimento.Focus();

                            return;
                        }
                        else if (this.cboTipoRecebimento.SelectedValue == null && this.intRecebimentoCTID == 0)
                        {
                            MessageBox.Show("Selecione primeiro uma ordem de desembarque.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                            this.txtOrdemDesembarqueNumero.Focus();

                            return;
                        }

                        this.Mudar_Cor_Grid_Itens_Diferenca();

                        this.Marcar_Pedido_Do_XML();
                    }
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Botao_Visualizar_Danfe(object sender, EventArgs e)
        {
            try
            {

                if (this.dtsRecebimento.Tables.Count > 0
                        && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0
                        && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() != 0
                    )
                {
                    frmRecebimento_NFe_Rps frmNfe = new frmRecebimento_NFe_Rps(this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger());
                    frmNfe.Show(this);
                }
                else
                {
                    MessageBox.Show("Não foi encontrado o arquivo eletrônico da nota fiscal selecionada.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #region "   Nota Fiscal     "

        private void Alterar_Texto_Ordem_Desembarque(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.Buscar_Ordem_Desembarque_NF())
                {
                    this.Configurar_DataSet_Recebimento();

                    this.Preencher_Aba_Itens();

                    this.Criar_Relacionamento_Nota_Pedido();

                    this.Carregar_DataSet_Recebimento_Ordem_Desembarque();

                    this.Carregar_Ordem_Desembarque_NF(0);

                    this.Verificar_Mudancas();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Alterar_Data_Emissao(object sender, EventArgs e)
        {
            try
            {

                if (this.txtCondicaoPagamento.Text != string.Empty)
                {
                    this.Processar_Geracao_Parcelas();

                    this.Verificar_Mudancas();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Alterar_Combo_Tipo_Recebimento(object sender, EventArgs e)
        {
            try
            {
                this.cboTipoRecebimento.SelectedValueChanged -= this.Alterar_Combo_Tipo_Recebimento;

                Cursor.Current = Cursors.WaitCursor;

                this.Verificar_Processar_Tipo_Recebimento();

                if (this.dtsRecebimento.Tables.Count > 0)
                {
                    if (this.dtsRecebimento.Tables.Count > 0 && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0)
                    {
                        this.Preencher_Grid_Pedidos(this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(), 0, this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_ID"].DefaultInteger());
                    }
                    else
                    {
                        this.Preencher_Grid_Pedidos(this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(), 0, 0);
                    }

                    this.Configurar_Tela_Revenda_Encomenda_Garantia(this.Verificar_Encomenda_Com_Pedido_Complementar());
                }
                else
                {
                    this.Configurar_Tela_Revenda_Encomenda_Garantia(false);
                }


                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.cboTipoRecebimento.SelectedValueChanged += this.Alterar_Combo_Tipo_Recebimento;
            }
        }

        private void Alterar_Combo_Numero_Nota(object sender, EventArgs e)
        {
            try
            {
                this.cboNotaFiscalNumero.SelectedValueChanged -= this.Alterar_Combo_Numero_Nota;

                Cursor.Current = Cursors.WaitCursor;

                this.Carregar_Ordem_Desembarque_NF(this.cboNotaFiscalNumero.SelectedIndex);

                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.cboNotaFiscalNumero.SelectedValueChanged += this.Alterar_Combo_Numero_Nota;
            }
        }

        private void Clicar_Botao_Pesquisar_Ordem_Desembarque(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;


                frmPesquisaGrid frmPesquisa = new frmPesquisaGrid("Fornecedor", "Pesquisa de Ordens de Desembarque");
                frmPesquisa.Grid.Adicionar_Coluna("Ordem_Desembarque_Sequencial_Dia", "N. Ordem", 80, false);
                frmPesquisa.Grid.Adicionar_Coluna("Ordem_Desembarque_Data_Criacao", "Data", 100, false);
                frmPesquisa.Grid.Adicionar_Coluna("Forn_CD", "Cód. For.", 80, false);
                frmPesquisa.Grid.Adicionar_Coluna("PessoaJuridica_NmFantasia", "Fornecedor", 120, true);
                frmPesquisa.Grid.Adicionar_Coluna("Forn_ID");

                Ordem_DesembarqueBUS busOrdemDesembarque = new Ordem_DesembarqueBUS();
                if (
                        (this.txtEmitenteCNPJCPF.Text != string.Empty && this.intFornecedorID != 0)
                        || (this.cboDestinatarioLoja.SelectedValue != null && this.cboDestinatarioLoja.SelectedValue.DefaultInteger() != 0)
                    )
                {
                    frmPesquisa.Carregar_Grid(busOrdemDesembarque.Consultar_Datatable_Ordem_Desembarque_Fornecedor_Loja(this.intFornecedorID, this.cboDestinatarioLoja.SelectedValue.DefaultInteger()));
                }
                else
                {
                    frmPesquisa.Carregar_Grid(busOrdemDesembarque.Consultar_DataTable_Ordem_Desembarque_Pesquisa());
                }


                if (frmPesquisa.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (frmPesquisa.Registro == null)
                    {
                        this.dtpOrdemDesembarqueData.Focus();
                        return;
                    }

                    this.txtOrdemDesembarqueNumero.Text = frmPesquisa.Registro.Cells["Ordem_Desembarque_Sequencial_Dia"].Value.DefaultString();
                    this.dtpOrdemDesembarqueData.Value = frmPesquisa.Registro.Cells["Ordem_Desembarque_Data_Criacao"].Value.DefaultDateTime();
                    this.cboDestinatarioLoja.SelectedValue = frmPesquisa.Registro.Cells["Lojas_ID"].Value.DefaultInteger();
                    if (this.Buscar_Ordem_Desembarque_NF())
                    {
                        this.Configurar_DataSet_Recebimento();

                        this.Preencher_Aba_Itens();

                        this.Criar_Relacionamento_Nota_Pedido();

                        this.Carregar_DataSet_Recebimento_Ordem_Desembarque();

                        this.Carregar_Ordem_Desembarque_NF(0);
                    }
                }

                this.Habilitar_Campos_Parcelas(false);
                this.Verificar_Mudancas();

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region "   Destinatário    "

        private void Mudar_Selecao_Loja(object sender, EventArgs e)
        {
            try
            {
                if (this.cboDestinatarioLoja.Text != string.Empty)
                {
                    DataRow[] dtrLoja = this.dttLojas.Select("Lojas_Id = " + this.cboDestinatarioLoja.SelectedValue.ToString());
                    this.lblDestinatarioCNPJ.Text = DivUtil.Formatar_CPF_CNPJ(dtrLoja[0]["PessoaJuridica_CNPJ"].DefaultString());
                    if (dtrLoja[0].Table.Columns["PessoaJuridica_RazaoSocial"] != null)
                    {
                        this.lblDestinatarioNome.Text = dtrLoja[0]["PessoaJuridica_RazaoSocial"].DefaultString();
                    }
                    else
                    {
                        this.lblDestinatarioNome.Text = dtrLoja[0]["Lojas_NM"].DefaultString();
                    }
                }

                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Botao_Permissao_Loja_Entrega(object sender, EventArgs e)
        {
            try
            {
                UsuarioDO dtoUsuario = this.Autenticar_Usuario();

                if (dtoUsuario == null)
                {
                    MessageBox.Show("A troca da loja de entrega não pode ser feito, pois o usuário e/ou a senha são inválidos!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario((object)dtoUsuario, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Desbloquear.ToString()) == false)
                {
                    MessageBox.Show("Usuário não possui permissão para esta ação!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.cboLojaRecebimento.Enabled = true;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Emitente        "

        private void Clicar_Botao_Pesquisa_Emitente(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.txtOrdemDesembarqueNumero.Text != string.Empty && this.dtsRecebimento.Tables.Count > 0 && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0)
                {
                    if (MessageBox.Show("Já existe uma ordem de desembarque informada, as informações serão excluidas. Deseja continuar?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                    {
                        this.Limpar_Dados_Nota_Fiscal();
                    }
                }

                this.Limpar_Dados_Emitente();

                if (this.Carregar_Fornecedor() == false)
                {
                    this.txtEmitenteCNPJCPF.Focus();
                }

                this.Verificar_Mudancas();

                this.btnAplicar.Focus();
            }

            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Sair_TextBox_Emitente(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.txtEmitenteCNPJCPF.Leave -= this.Sair_TextBox_Emitente;

                if (((TextBox)sender).Text.Trim() != string.Empty)
                {
                    if (this.Validar_CNPJ_CPF() == false)
                    {
                        return;
                    }

                    if (Utilitario.Remover_Caracteres_CNPJ_CPF(this.strFornCNPJCPF) == Utilitario.Remover_Caracteres_CNPJ_CPF(this.txtEmitenteCNPJCPF.Text.Trim()))
                    {
                        this.txtEmitenteCNPJCPF.Text = DivUtil.Formatar_CPF_CNPJ(this.txtEmitenteCNPJCPF.Text);
                        return;
                    }

                    if (this.txtOrdemDesembarqueNumero.Text != string.Empty && this.dtsRecebimento.Tables.Count > 0 && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0)
                    {
                        if (MessageBox.Show("Já existe uma ordem de desembarque informada, as informações serão excluidas. Deseja continuar?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                        {
                            this.Limpar_Dados_Nota_Fiscal();
                        }
                    }

                    if (this.Carregar_Fornecedor() == false)
                    {
                        this.Limpar_Dados_Emitente();
                        this.txtEmitenteCNPJCPF.Focus();
                    }
                }
                else
                {
                    this.Limpar_Dados_Emitente();
                }

                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.txtEmitenteCNPJCPF.Leave += this.Sair_TextBox_Emitente;
            }
        }

        #endregion

        #region "   Parcelas        "

        private void Clicar_Botao_Pesquisar_Condicao_Pagamento(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                
                Condicao_Pagamento_CTDO dtoCondicaoPagamentoTemp = this.dtoCondicaoPagamento.Clone();

                this.dtoCondicaoPagamento = this.Pesquisar_Condicao_Pagamento();

                if (!this.Validar_Condicao_Pagamento(false))
                {
                    this.btnGerarParcelas.Enabled = false;
                    this.dtoCondicaoPagamento = dtoCondicaoPagamentoTemp.Clone();
                    return;
                }

                if ((this.dtoCondicaoPagamento != null))
                {
                    this.txtCondicaoPagamento.Text = this.dtoCondicaoPagamento.Codigo;
                    this.lblCondicaoPagamento.Text = this.dtoCondicaoPagamento.Descricao;
                }
                else if (this.dtoCondicaoPagamento == null && this.txtCondicaoPagamento.Text.Length != 0)
                {
                    Condicao_Pagamento_CTBUS busCondicaoPagamento = new Condicao_Pagamento_CTBUS();
                    this.dtoCondicaoPagamento = busCondicaoPagamento.Selecionar_Por_Codigo(this.txtCondicaoPagamento.Text);

                    if (this.dtoCondicaoPagamento == null)
                    {
                        MessageBox.Show("Condição de pagamento inválida!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.dtoCondicaoPagamento = null;
                        this.txtCondicaoPagamento.Text = string.Empty;
                        this.lblCondicaoPagamento.Text = string.Empty;
                        this.txtCondicaoPagamento.Focus();
                        return;
                    }
                    else
                    {
                        this.txtCondicaoPagamento.Text = this.dtoCondicaoPagamento.Codigo;
                        this.lblCondicaoPagamento.Text = this.dtoCondicaoPagamento.Descricao;
                    }
                }

                if ( this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() != 0 && 
                     this.dgvParcelas.Rows.Count > 0 &&
                     ( this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Condicao_Pagamento_CT_CD"].DefaultInteger() == this.txtCondicaoPagamento.Text.DefaultInteger()) )
                {
                    this.btnGerarParcelas.Enabled = false;
                }
                else
                {
                    this.btnGerarParcelas.Enabled = this.lblCondicaoPagamento.Text != string.Empty;
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.WaitCursor;
            }
        }

        private void Sair_TextBox_Codigo_Condicao_Pagamento(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (((TextBox)sender).ReadOnly)
                    return;

                if (((TextBox)sender).Text.Trim() != string.Empty)
                {
                    ((TextBox)sender).Text = ((TextBox)sender).Text.Trim().PadLeft(4, Convert.ToChar("0"));
                }
                else
                {
                    this.txtCondicaoPagamento.Text = string.Empty;
                    this.lblCondicaoPagamento.Text = string.Empty;

                    return;
                }

                this.Carregar_Condicao_Pagamento();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.WaitCursor;
            }
        }

        private void Clicar_Botao_Gerar_Parcelas(object sender, EventArgs e)
        {
            try
            {
                this.dgvParcelas.SelectionChanged -= this.Clicar_DataGrid_Parcelas;

                Cursor.Current = Cursors.WaitCursor;

                this.Processar_Geracao_Parcelas();

                this.Preencher_DataSet_Recebimento_Parcelas();

                this.Verificar_Mudancas();

                this.Habilitar_Campos_Parcelas(false);
                this.Limpar_Campos_Parcelas();

                this.dgvParcelas.ClearSelection();

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.dgvParcelas.SelectionChanged += this.Clicar_DataGrid_Parcelas;
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_DataGrid_Parcelas(object sender, EventArgs e)
        {
            try
            {
                if (!this.dgvParcelas.ReadOnly)
                {
                    this.Carregar_Campos_Parcelas();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Atualizar_Parcelas(object sender, EventArgs e)
        {
            try
            {
                this.Atualizar_DataGrid_Parcelas();

                this.Verificar_Mudancas();

                this.dgvParcelas.ReadOnly = this.blnSomenteLeituraParcela;
                this.blnSomenteLeituraParcela = false;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Cancelar_Parcelas(object sender, EventArgs e)
        {
            try
            {
                this.Habilitar_Campos_Parcelas(false);
                this.Limpar_Campos_Parcelas();

                this.dgvParcelas.ClearSelection();

                this.dgvParcelas.ReadOnly = this.blnSomenteLeituraParcela;
                this.blnSomenteLeituraParcela = false;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Botao_Permissao_Alterar_Parcela(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvParcelas.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecione uma linha para realizar a alteração.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                UsuarioDO dtoUsuario = this.Autenticar_Usuario();

                if (dtoUsuario == null)
                {
                    MessageBox.Show("A alteração das parcelas não pode ser feito, pois o usuário e/ou a senha são inválidos!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario((object)dtoUsuario, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Alterar_Parcelas.ToString()) == false)
                {
                    MessageBox.Show("Usuário não possui permissão para esta ação!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.blnSomenteLeituraParcela = this.dgvParcelas.ReadOnly;
                this.dgvParcelas.ReadOnly = false;
                this.Carregar_Campos_Parcelas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #endregion

        #region "   Aba Itens                 "

        private void Mudar_Indice_Selecionado_Lista_Itens(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count == 0)
                    return;

                this.Carregar_Detalhes_do_Item(this.dgvNotaFiscalItens.Grid00.SelectedRows[0]);

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Embalagens"].Value.DefaultInteger() > this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger()
                        || this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Embalagens"].Value.DefaultInteger() == 0)
                {
                    this.chkImprimirCodigoBarras.Enabled = true;
                }
                else
                {
                    this.chkImprimirCodigoBarras.Enabled = false;
                }

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Pedido_Garantia_CT_ID"].Value.DefaultInteger() > 0 && 
                    this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger() > 0)
                {
                    this.btnLoteDevolucao.Enabled = true;
                }
                else
                {
                    this.btnLoteDevolucao.Enabled = false;
                }

                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Ordernar_Grid_Nota_itens(object sender, EventArgs e)
        {
            try
            {
                this.Mudar_Cor_Grid_Itens_Diferenca();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #region "   Grid de Pedidos     "

        private void Clicar_Botao_Processar(object sender, EventArgs e)
        {
            DataSet dtsRecebimentoAntesGravacao = this.dtsRecebimento.Copy();
            try
            {
                this.chkMarcarTodosPedidos.Click -= this.Clicar_Marcar_Todos_Pedidos;

                Cursor.Current = Cursors.WaitCursor;

                if (!this.Validar_Formulario())
                {
                    return;
                }

                if (!this.Validar_Processo_Comparacao_Pedido_Nota_Fiscal())
                {
                    return;
                }

                if (MessageBox.Show("A comparação entre Nota e Pedido será registrada. Confirma operação?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                this.Processar_Comparacao_Pedido_Nota_Fiscal();

                this.Preencher_Objeto_Recebimento(true);

                DataTable dttItens = this.Montar_DataTable_Pedido_Compra();
                DataTable dttClassificacaoFiscalInformada = this.Montar_DataTable_Classificacao_Fiscal_Informada();

                DataTable dttCustoItens = new DataTable();
                dttCustoItens = this.dtsRecebimento.Tables["RecebimentoIT"].Clone();
                foreach (DataRow dtrRecebimentoIT in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido.DefaultInteger() & dtrRecebimentoIT["Encomenda_Peca_Sem_Cadastro"].DefaultBool())
                    {
                        dttCustoItens.Rows.Add(dtrRecebimentoIT.ItemArray);
                    }
                }

                Recebimento_NFeBUS busRecebimentoNFe = new Recebimento_NFeBUS();
                if (this.dtoPropriedades.ID == 0)
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                        || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        busRecebimentoNFe.Tratar_DataObject_Incluir_E_Atualizar_Garantia(this.dtoPropriedades, dttItens);
                    }
                    else
                    {
                        busRecebimentoNFe.Tratar_DataObject_Incluir(this.dtoPropriedades, dttItens, dttClassificacaoFiscalInformada, dttCustoItens, ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID, (Tipo_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger());
                    }
                    this.intRecebimentoCTID = this.dtoPropriedades.ID;
                }
                else
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                              || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        busRecebimentoNFe.Tratar_DataObject_Alterar_E_Atualizar_Garantia(this.dtoPropriedades, this.dttItensExcluidos, dttItens);
                    }
                    else
                    {
                        busRecebimentoNFe.Tratar_DataObject_Alterar(this.dtoPropriedades, this.dttItensExcluidos, dttItens, dttClassificacaoFiscalInformada, dttCustoItens,
                            ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID, (Tipo_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger());
                    }
                }

                this.Registro_Alterado = true;

                this.Carregar_Dados();

                this.Verificar_Mudancas();

                this.dgvNotaFiscalItens.Focus();

                this.Habilitar_Botao_Pre_Recebimento();

                this.Registro_Alterado = true;

                this.chkMarcarTodosPedidos.Checked = false;

            }
            catch (Exception ex)
            {
                try
                {
                    this.dtsRecebimento = dtsRecebimentoAntesGravacao.Copy();

                    this.dgvNotaFiscalItens.Grid00.DataSource = this.dtsRecebimento.Tables["RecebimentoIT"];
                    this.dgvNotaFiscalItens.Grid01.DataSource = this.dtsRecebimento.Tables["Nota_Pedido_Itens"];

                    this.Mudar_Cor_Grid_Itens_Diferenca();

                    this.Calcular_Totais_Itens();
                }
                catch (Exception)
                {
                    Root.Tratamento_Erro.Tratar_Erro(ex, this);
                }

                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosPedidos.Click += this.Clicar_Marcar_Todos_Pedidos;

                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_Marcar_Todos_Pedidos(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow dgrLinha in this.dgvPedidoCompras.Rows)
                {
                    if (this.chkMarcarTodosPedidos.Checked ||
                        this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Pedido_Compra_CT_ID = " + dgrLinha.Cells["Pedido_ID"].Value.DefaultString()).Length == 0)
                    {
                        dgrLinha.Cells["Marcado"].Value = this.chkMarcarTodosPedidos.Checked;
                    }
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Pressionar_Tecla_Grid_Pedidos(object sender, KeyEventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (e.KeyCode == Keys.Space)
                {
                    DataRow dtrPedidoSelecionado = ((DataRowView)this.dgvPedidoCompras.CurrentRow.DataBoundItem).Row;

                    this.Alterar_Macarcao_Pedido_Compra(dtrPedidoSelecionado);
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_dgvPedidoCompras(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (e.ColumnIndex == this.dgvPedidoCompras.Columns["Marcado"].Index && e.RowIndex != -1)
                {
                    DataRow dtrPedidoSelecionado = ((DataRowView)this.dgvPedidoCompras.Rows[e.RowIndex].DataBoundItem).Row;

                    this.Alterar_Macarcao_Pedido_Compra(dtrPedidoSelecionado);
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region "   Lançamento Itens    "

        private void Clicar_Botao_Pesquisa_Classificacao_Fiscal(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Consultar_Classificacao_Fiscal();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_Botao_Pesquisa_Codigo_Fabricante(object sender, EventArgs e)
        {
            try
            {
                this.txtCodigoFabricante.LostFocus -= this.Perder_Foco_Campo_CodFabricante;

                Cursor.Current = Cursors.WaitCursor;

                if (this.txtEmitenteCNPJCPF.TextLength == 0 && this.txtCodigoFabricante.Text != string.Empty)
                {
                    MessageBox.Show("Localize primeiro o fornecedor!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtCodigoFabricante.Text = string.Empty;
                    this.tbcHerdado.TabPages[this.tbpDadosNotaFiscal.Name].Show();
                    this.txtEmitenteCNPJCPF.Focus();
                    return;
                }

                if (this.txtCodigoFabricante.Text != string.Empty)
                {
                    this.Consultar_Novo_Item(this.txtCodigoFabricante.Text, string.Empty);
                }
                else
                {
                    MessageBox.Show("Informe o código do fabricante ou fornecedor para prosseguir", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }


            }

            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.txtCodigoFabricante.LostFocus += this.Perder_Foco_Campo_CodFabricante;
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_Botao_Inserir_Item(object sender, EventArgs e)
        {
            try
            {
                this.btnInserir.Click -= this.Clicar_Botao_Inserir_Item;

                Cursor.Current = Cursors.WaitCursor;

                if (this.Incluir_Grid_Novo_Item())
                {
                    this.Limpar_Campos_Itens_Detalhes();
                }

                this.Verificar_Mudancas();

                if (this.dgvNotaFiscalItens.Grid00.Rows.Count == 0)
                {
                    this.btnExcluir.Enabled = false;
                }
                else
                {
                    this.btnExcluir.Enabled = true;
                }

                this.Calcular_Totais_Itens();
                this.Configurar_Menus();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.btnInserir.Click += this.Clicar_Botao_Inserir_Item;
            }
        }

        private void Clicar_Botao_Excluir_Item(object sender, EventArgs e)
        {
            try
            {
                this.btnExcluir.Click -= this.Clicar_Botao_Excluir_Item;

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecione uma linha para a exclusão.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > 1)
                {
                    MessageBox.Show("Selecione somente uma linha para a exclusão.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("Deseja realmente excluir o item selecionado?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                if (this.Excluir_Grid_Novo_Item())
                {
                    this.Limpar_Campos_Itens_Detalhes();
                }

                this.Verificar_Mudancas();

                this.Calcular_Totais_Itens();
                this.Configurar_Menus();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.btnExcluir.Click += this.Clicar_Botao_Excluir_Item;
            }
        }

        private void Perder_Foco_Campo_CST(object sender, EventArgs e)
        {
            try
            {
                this.Validar_Selecionar_CST();

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Perder_Foco_Campo_CodFabricante(object sender, EventArgs e)
        {
            try
            {
                this.txtCodigoFabricante.LostFocus -= this.Perder_Foco_Campo_CodFabricante;

                if (this.txtCodigoFabricante.Text != string.Empty && this.txtEmitenteCNPJCPF.TextLength == 0)
                {
                    MessageBox.Show("Localize primeiro o fornecedor!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtCodigoFabricante.Text = string.Empty;
                    this.tbcHerdado.TabPages[this.tbpDadosNotaFiscal.Name].Show();
                    this.txtEmitenteCNPJCPF.Focus();
                    return;
                }
                if (this.txtCodigoFabricante.Text != string.Empty)
                {
                    this.Consultar_Novo_Item(this.txtCodigoFabricante.Text, string.Empty);

                    this.txtPecaRevenda.Focus();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.txtCodigoFabricante.LostFocus += this.Perder_Foco_Campo_CodFabricante;
            }

        }

        private void Perder_Foco_Campo_Quantidade_Nota_Fiscal(object sender, EventArgs e)
        {
            try
            {
                this.txtQuantidadeTotalNotaFiscal.LostFocus -= this.Perder_Foco_Campo_Quantidade_Nota_Fiscal;

                Cursor.Current = Cursors.WaitCursor;

                if (this.mskCustoNotaFiscal.Text != string.Empty && this.txtQuantidadeTotalNotaFiscal.Text != string.Empty)
                {
                    this.lblValorTotalItemRevenda.Text = (this.mskCustoNotaFiscal.Text.DefaultDecimal() * this.txtQuantidadeTotalNotaFiscal.Text.DefaultDecimal()).ToString("#,##0.0000");
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.txtQuantidadeTotalNotaFiscal.LostFocus += this.Perder_Foco_Campo_Quantidade_Nota_Fiscal;
            }
        }

        private void Perder_Foco_Campo_Custo_Nota_Fiscal(object sender, EventArgs e)
        {
            try
            {
                this.mskCustoNotaFiscal.LostFocus -= this.Perder_Foco_Campo_Custo_Nota_Fiscal;

                this.Calcular_Desconto();

                Cursor.Current = Cursors.WaitCursor;

                if (this.mskCustoNotaFiscal.Text != string.Empty && this.txtQuantidadeTotalNotaFiscal.Text != string.Empty)
                {
                    this.lblValorTotalItemRevenda.Text = (this.mskCustoNotaFiscal.Text.DefaultDecimal() * this.txtQuantidadeTotalNotaFiscal.Text.DefaultDecimal()).ToString("#,##0.0000");
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                this.mskCustoNotaFiscal.LostFocus += this.Perder_Foco_Campo_Custo_Nota_Fiscal;
            }
        }

        private void Alterar_Imprimir_Codigo_De_Barras_Recebimento_IT(object sender, EventArgs e)
        {
            try
            {
                if (this.txtRecebimento_IT_ID.Text.DefaultInteger() == 0 | this.dgvNotaFiscalItens.Grid00.SelectedRows.Count == 0)
                {
                    return;
                }

                Recebimento_ITBUS busRecebimentoIT = new Recebimento_ITBUS();
                busRecebimentoIT.Atualizar_Imprimir_Etiqueta(this.txtRecebimento_IT_ID.Text.DefaultInteger(), this.chkImprimirCodigoBarras.Checked);
                this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Pre_Recebimento_IT_Imprimir_Etiqueta"].Value = this.chkImprimirCodigoBarras.Checked;
                this.dtsRecebimento.Tables["RecebimentoIT"].Rows[this.dgvNotaFiscalItens.Grid00.CurrentRow.Index]["Pre_Recebimento_IT_Imprimir_Etiqueta"] = this.chkImprimirCodigoBarras.Checked;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Alterar_Peca_Embalagem_Compra_Custo(object sender, System.EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > 0 || this.intPecaID != 0)
                {
                    decimal dcmCustoUnitario = this.mskCustoNotaFiscal.Text.DefaultDecimal();

                    if (this.mskValorDescontoRevenda.Text.DefaultDecimal() > 0)
                    {
                        dcmCustoUnitario -= (this.mskValorDescontoRevenda.Text.DefaultDecimal() / this.txtQuantidadeTotalNotaFiscal.Text.DefaultDecimal()).ToFormatDecimal().DefaultDecimal();
                    }
                    dcmCustoUnitario += (this.mskCustoNotaFiscal.Text.DefaultDecimal() * (this.mskIPIRevenda.Text.DefaultDecimal() / 100)).ToFormatDecimal().DefaultDecimal();
                    dcmCustoUnitario += (this.mskCustoNotaFiscal.Text.DefaultDecimal() * (this.mskSubstituicaoRevenda.Text.DefaultDecimal() / 100)).ToFormatDecimal().DefaultDecimal();

                    if (this.intPecaID != 0)
                    {
                        int intQtdeEmbalagem = this.Retornar_Quantidade_Embalagem_Peca(this.intPecaID, this.cboEmbalagemCompra.SelectedValue.DefaultInteger());

                        if (intQtdeEmbalagem > 0)
                        {
                            this.lblCustoUnitario.Text = (dcmCustoUnitario / intQtdeEmbalagem).ToString("#,##0.00");
                        }
                    }
                    else
                    {
                        this.lblCustoUnitario.Text = 0.ToString("#,##0.00");
                    }

                    this.lblCustoEmbalagemRevenda.Text = dcmCustoUnitario.ToFormatDecimal();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Perder_Foco_Desconto(object sender, EventArgs e)
        {
            try
            {
                this.Calcular_Desconto();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Menu de Contexto    "

        private void Clicar_Menu_Procurar_Item_Nao_Cadastrado(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Selecione somente uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dgvNotaFiscalItens.Grid00.RowCount > 0 && this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Cadastro.DefaultInteger())
                {
                    this.Chamar_Tela_Procurar_Item();

                    this.Setar_Imagens_Grid();

                    this.Verificar_Mudancas();
                }
                else if (this.btnInserir.Enabled)
                {
                    this.Chamar_Tela_Procurar_Item();

                    this.Setar_Imagens_Grid();

                    this.Verificar_Mudancas();
                }
                else
                {
                    MessageBox.Show("Somente itens sem cadastro podem acessar a tela de itens não encontrados", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Pedido_Compra(object sender, EventArgs e)
        {
            try
            {
                // Fazer a chamada para a tela de Pedido de Compra
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Devolver_Item(object sender, EventArgs e)
        {
            try
            {
                if (this.btnAplicar.Enabled == true)
                {
                    MessageBox.Show("Existem itens não salvos. Salve para continuar.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Selecione somente uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Devolvido.DefaultInteger() ||
                    this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Cadastro.DefaultInteger())
                {
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario((object)Root.FuncionalidadesWindows.UsuarioDO_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Devolver.ToString()) == false)
                {
                    MessageBox.Show("Usuário não possui permissão para esta ação!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!this.Validar_Formulario())
                {
                    return;
                }

                if (!this.Validar_Mercadoria_Preparada(this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultInteger()))
                {
                    return;
                }

                int intQuantidade = this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger()
                                  - this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger();

                frmRecebimento_Nota_Fiscal_Devolucao frmDevolucaoParcial = new frmRecebimento_Nota_Fiscal_Devolucao(this.intRecebimentoCTID,
                        this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultInteger(),
                        this.cboNotaFiscalNumero.SelectedText, this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger(),
                        intQuantidade,
                        this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_NF_Descricao"].Value.DefaultString());

                frmDevolucaoParcial.ShowDialog(this);

                this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Restante"].Value = this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Restante"].Value.DefaultInteger() - frmDevolucaoParcial.QtdeDevolvida;
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Restante"].Value.DefaultInteger() < 0)
                {
                    this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Restante"].Value = 0;
                }

                this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value = this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger() + frmDevolucaoParcial.QtdeDevolvida;

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Restante"].Value.DefaultInteger() == 0)
                {

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger() == this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger())
                    {
                        this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value = Status_Recebimento_Item.Devolvido.DefaultInteger();
                    }
                    else
                    {
                        this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value = Status_Recebimento_Item.Correto.DefaultInteger();
                    }
                }

                this.Efetuar_Alteracao();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Estornar_Item_Devolvido(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.dgvNotaFiscalItens.Grid00.CurrentRow == null)
                    return;

                if (new Recebimento_Nota_FiscalBUS().Estornar_Item_Devolucao_Recebimento_Nota_Fiscal(this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_CT_ID"].Value.DefaultInteger(), 
                                                                                                 this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Pedido_Garantia_CT_ID"].Value.DefaultInteger(),
                                                                                                 this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Peca_ID"].Value.DefaultInteger()))
                {
                    MessageBox.Show("Estorno concluído com êxito!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Efetuar_Alteracao();
                }
                else
                {
                    MessageBox.Show("Não foi possível estornar o item devolvido!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_Menu_Substituir_Item(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Selecione somente uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dgvNotaFiscalItens.Grid00.RowCount > 0 && this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Pedido.DefaultInteger())
                {
                    this.Chamar_Tela_Substituir_Item();

                    this.Setar_Imagens_Grid();

                    this.Verificar_Mudancas();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Analise_Mercadologica(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Selecione somente uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dgvNotaFiscalItens.Grid00.RowCount > 0 && this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > 0)
                {
                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Cadastro.DefaultInteger())
                    {
                        MessageBox.Show("A análise mercadológica não está disponível para um item com status 'sem cadastro'!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    frmAnalise_Mercadologica_Estoque_New frmAnaliseMercadologica = new frmAnalise_Mercadologica_Estoque_New((short)this.cboDestinatarioLoja.SelectedValue.DefaultInteger(),
                                                                                                                            this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Fabricante_CD"].Value.DefaultString(),
                                                                                                                            this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Produto_CD"].Value.DefaultString(),
                                                                                                                            this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Peca_CD"].Value.DefaultString());

                    frmAnaliseMercadologica.Show();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Propriedade_Peca(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count != 1)
                {
                    MessageBox.Show("Selecione somente uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (this.dgvNotaFiscalItens.Grid00.RowCount > 0 && this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > 0)
                {
                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Peca_ID"].Value.DefaultInteger() > 0)
                    {
                        frmPecaPropriedades_NEW frmPecaProriedades = new frmPecaPropriedades_NEW(this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Peca_ID"].Value.DefaultInteger(), false);

                        frmPecaProriedades.Show();
                    }
                    else
                    {
                        MessageBox.Show("Localize primeiro a peça", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Desfazer(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > 0)
                {
                    this.Processar_Desfazer();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void ContextMenu_Popup(object sender, EventArgs e)
        {
            try
            {
                this.Configurar_Menus();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Incluir_Pedido_Compra(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Devolvido.DefaultInteger())
                {
                    return;
                }

                if (this.strFornCD != string.Empty && this.cboDestinatarioLoja.SelectedValue != null)
                {
                    frmPedido_Compra_Propriedades frmPedidoComprasPropriedades = new frmPedido_Compra_Propriedades(this.cboDestinatarioLoja.SelectedValue.DefaultString(), this.strFornCD.Trim());
                    frmPedidoComprasPropriedades.ShowDialog(this);

                    if (frmPedidoComprasPropriedades.Pedido_Compra_CT_ID == 0)
                    {
                        return;
                    }

                    // Atualizar Grid de Pedidos, marcando o pedido incluido.
                    if (this.dtsRecebimento.Tables.Count > 0 && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0)
                    {
                        this.Preencher_Grid_Pedidos(this.intFornecedorID, frmPedidoComprasPropriedades.Pedido_Compra_CT_ID, this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_ID"].DefaultInteger());
                    }
                    else
                    {
                        this.Preencher_Grid_Pedidos(this.intFornecedorID, frmPedidoComprasPropriedades.Pedido_Compra_CT_ID, 0);
                    }

                    // Processa novamente o Pedido com a Nota
                    if (MessageBox.Show("Deseja processar novamente?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        this.btnProcessar.PerformClick();
                    }

                }
            }
            catch (Exception ex)
            {

                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }

        }

        private void Clicar_Alterar_Pedido_Compra(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem objItemSelecionado = (ToolStripMenuItem)sender;

                frmPedido_Compra_Propriedades frmPedidoComprasPropriedades = new frmPedido_Compra_Propriedades(objItemSelecionado.Text.Trim().DefaultInteger(), objItemSelecionado.Tag.DefaultInteger());
                frmPedidoComprasPropriedades.ShowDialog(this);

                if (frmPedidoComprasPropriedades.Registro_Alterado == false)
                {
                    return;
                }

                if (MessageBox.Show("Deseja processar novamente?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.btnProcessar.PerformClick();
                }
            }

            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Abrir_Pedido_Compra(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem objItemSelecionado = (ToolStripMenuItem)sender;

                frmPedido_Compra_Propriedades frmPedidoComprasPropriedades = new frmPedido_Compra_Propriedades(objItemSelecionado.Text.Trim().DefaultInteger(), objItemSelecionado.Tag.DefaultInteger());
                frmPedidoComprasPropriedades.ShowDialog(this);
            }

            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Abrir_Pedido_Garantia(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem objItemSelecionado = (ToolStripMenuItem)sender;

                DataRow[] colLoja = Root.Lista_Lojas.Select("Lojas_Tipo = 'Garantia'");

                int intLoja = this.cboDestinatarioLoja.SelectedValue.DefaultInteger();

                if (colLoja.Length > 0)
                {
                    intLoja = colLoja[0]["Lojas_Id"].DefaultInteger();
                }

                frmGarantia_Lote_Propriedades frmPedidoGarantiaPropriedades = new frmGarantia_Lote_Propriedades(objItemSelecionado.Text.Trim().DefaultInteger(), intLoja);
                frmPedidoGarantiaPropriedades.ShowDialog(this);
            }

            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Reassociar_Item(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                new Recebimento_Nota_FiscalBUS().Reassociar_Itens_Nota_Fiscal_Cadastro(this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger());

                this.Carregar_Dados();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        private void Clicar_Botao_Gerar_Pre_Recebimento(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!this.Validar_Pre_Recebimento_A_Gerar(true))
                {
                    return;
                }

                if (!this.Validar_Tipo_Recebimento_Obrigatorio())
                {
                    return;
                }

                if (!this.Validar_Formulario_Pre_Recebimento())
                {
                    return;
                }

                if (!this.Efetuar_Alteracao())
                {
                    return;
                }


                this.Gerar_Pre_Recebimento();

                this.Mudar_Indice_Selecionado_Lista_Itens(sender, e);

                this.Habilitar_Botao_Preparar_Mercadoria();

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_Botao_Abrir_Lote_Devolucao(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this.Abrir_Propriedades_Lote_Garantia_Ou_Devolucao();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }


        private void Clicar_Filtro_Visualizar_Itens_Corretos(object sender, EventArgs e)
        {
            try
            {
                this.Visualizar_Itens();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Filtro_Visualizar_Itens_Divergentes(object sender, EventArgs e)
        {
            try
            {
                this.Visualizar_Itens();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Aba Pré-Recebimento       "

        private void Clicar_Botao_Preparar_Mercadoria(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Validar_Geracao_Preparacao_Mercadoria(true);

                Pre_RecebimentoBUS busPreRecebimento = new Pre_RecebimentoBUS();
                DataSet dtsPreparacaoMercadoriasPropriedades = busPreRecebimento.Consultar_DataSet_Volume_Preparacao(this.cboLojaRecebimento.SelectedValue.DefaultInteger(), this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger());

                foreach (DataRow dtrLinha in dtsPreparacaoMercadoriasPropriedades.Tables[0].Rows)
                {
                    if (dtrLinha["Pre_Recebimento_CT_Nota_Fiscal"].DefaultString() == this.cboNotaFiscalNumero.Text)
                    {
                        frmPre_Recebimento_Preparacao frmPreRecebimentoPreparacao = new frmPre_Recebimento_Preparacao(this.cboLojaRecebimento.SelectedValue.DefaultInteger(), this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(), dtrLinha["Pre_Recebimento_CT_ID"].DefaultInteger());
                        frmPreRecebimentoPreparacao.ShowDialog(this);
                        this.Carregar_Grupo();

                        break;
                    }
                }

                this.Habilitar_Botao_Preparar_Mercadoria();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Clicar_Marcar_Todos_Grupos(object sender, EventArgs e)
        {
            try
            {
                this.dgvGrupoPreRecebimento.KeyDown -= this.Pressionar_Tecla_DataGrid_Grupo;
                this.dgvGrupoPreRecebimento.CellValueChanged -= this.Clicar_DataGrid_Grupo;

                this.Tratar_Marcar_Desmarcar_Todos_Grupos(this.chkMarcarTodosGrupo.Checked);

                this.Carregar_Pre_Recebimento();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.dgvGrupoPreRecebimento.KeyDown += this.Pressionar_Tecla_DataGrid_Grupo;
                this.dgvGrupoPreRecebimento.CellValueChanged += this.Clicar_DataGrid_Grupo;
            }
        }

        private void Clicar_Marcar_Todos_Pre_Recebimento(object sender, EventArgs e)
        {
            try
            {
                this.dgvPreRecebimento.KeyDown -= this.Pressionar_Tecla_DataGrid_Pre_Recebimento;
                this.dgvPreRecebimento.CellValueChanged -= this.Clicar_DataGrid_Pre_Recebimento;

                this.Tratar_Marcar_Desmarcar_Todos_Pre_Recebimento(this.chkMarcarTodosPreRecebimento.Checked);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.dgvPreRecebimento.KeyDown += this.Pressionar_Tecla_DataGrid_Pre_Recebimento;
                this.dgvPreRecebimento.CellValueChanged += this.Clicar_DataGrid_Pre_Recebimento;
            }
        }

        private void Clicar_Celula_Grid_Grupo(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                this.chkMarcarTodosGrupo.Click -= this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click -= this.Clicar_Marcar_Todos_Pre_Recebimento;

                if (this.dgvGrupoPreRecebimento.Rows.Count < 1 || e.RowIndex < 0)
                {
                    return;
                }

                if (this.dgvGrupoPreRecebimento.Columns[e.ColumnIndex].Name.Equals("Marcado"))
                {
                    this.dgvGrupoPreRecebimento[e.ColumnIndex, e.RowIndex].Value = !this.dgvGrupoPreRecebimento[e.ColumnIndex, e.RowIndex].Value.DefaultBool();

                    this.Carregar_Pre_Recebimento();

                    this.chkMarcarTodosGrupo.Checked = this.Marcar_Check_Todos_Grupo();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;
            }
        }

        private void Clicar_Celula_Grid_Pre_Recebimento(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                this.chkMarcarTodosGrupo.Click -= this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click -= this.Clicar_Marcar_Todos_Pre_Recebimento;

                if (this.dgvGrupoPreRecebimento.Rows.Count < 1 || e.RowIndex < 0)
                {
                    return;
                }

                if (this.dgvPreRecebimento.Columns[e.ColumnIndex].Name.Equals("Marcado"))
                {
                    this.dgvPreRecebimento[e.ColumnIndex, e.RowIndex].Value = !this.dgvPreRecebimento[e.ColumnIndex, e.RowIndex].Value.DefaultBool();

                    this.Carregar_Lote_Entrada();
                    this.Carregar_Volume_Conferido();

                    this.chkMarcarTodosPreRecebimento.Checked = this.Marcar_Check_Todos_Pre_Recebimento();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;
            }
        }

        private void Pressionar_Tecla_DataGrid_Grupo(object sender, KeyEventArgs e)
        {
            try
            {
                this.chkMarcarTodosGrupo.Click -= this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click -= this.Clicar_Marcar_Todos_Pre_Recebimento;

                if (this.dgvGrupoPreRecebimento.SelectedRows.Count == 0)
                    return;

                switch (e.KeyCode)
                {
                    case Keys.Space:
                        this.dgvGrupoPreRecebimento.SelectedRows[0].Cells["Marcado"].Value = !Convert.ToBoolean(this.dgvGrupoPreRecebimento.SelectedRows[0].Cells["Marcado"].Value);
                        this.Carregar_Pre_Recebimento();
                        this.chkMarcarTodosGrupo.Checked = this.Marcar_Check_Todos_Grupo();
                        break;
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;
            }
        }

        private void Commit_Alteracao_Grid_Grupo_Pre_Recebimento(object sender, EventArgs e)
        {
            try
            {
                this.dgvGrupoPreRecebimento.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Commit_Alteracao_Grid_Pre_Recebimento(object sender, EventArgs e)
        {
            try
            {
                this.dgvPreRecebimento.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_DataGrid_Grupo(System.Object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            try
            {
                this.chkMarcarTodosGrupo.Click -= this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click -= this.Clicar_Marcar_Todos_Pre_Recebimento;

                if (e.ColumnIndex == 0 && e.RowIndex > -1) // coluna Marcado
                {
                    this.Carregar_Pre_Recebimento();
                    this.chkMarcarTodosGrupo.Checked = this.Marcar_Check_Todos_Grupo();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;
            }
        }

        private void Pressionar_Tecla_DataGrid_Pre_Recebimento(object sender, KeyEventArgs e)
        {
            try
            {
                this.chkMarcarTodosGrupo.Click -= this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click -= this.Clicar_Marcar_Todos_Pre_Recebimento;

                if (this.dgvPreRecebimento.SelectedRows.Count == 0)
                    return;

                switch (e.KeyCode)
                {
                    case Keys.Space:
                        this.dgvPreRecebimento.SelectedRows[0].Cells[0].Value = !Convert.ToBoolean(this.dgvPreRecebimento.SelectedRows[0].Cells[0].Value);
                        this.Carregar_Lote_Entrada();
                        this.Carregar_Volume_Conferido();
                        this.chkMarcarTodosPreRecebimento.Checked = this.Marcar_Check_Todos_Pre_Recebimento();
                        break;
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;
            }
        }

        private void Clicar_DataGrid_Pre_Recebimento(System.Object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            try
            {
                this.chkMarcarTodosGrupo.Click -= this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click -= this.Clicar_Marcar_Todos_Pre_Recebimento;

                if (e.ColumnIndex == 0 && e.RowIndex > -1) // coluna Marcado
                {
                    this.Carregar_Lote_Entrada();
                    this.Carregar_Volume_Conferido();
                    this.chkMarcarTodosPreRecebimento.Checked = this.Marcar_Check_Todos_Pre_Recebimento();

                    this.Trata_Datagrid_Itens_Pre_Recebimento();
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
            finally
            {
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;
            }
        }

        private void Clicar_DataGrid_Itens_Pre_Recebimento(object sender, EventArgs e)
        {
            try
            {
                this.Trata_Datagrid_Itens_Pre_Recebimento();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Menu_Lote_Guarda(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvItensVolumes.SelectedRows.Count > 0)
                {
                    frmPre_Recebimento_Guarda frmPreRecebimentosGuarda = new frmPre_Recebimento_Guarda(this.cboDestinatarioLoja.SelectedValue.DefaultInteger(),
                                                                                                        this.dgvItensVolumes.SelectedRows[0].Cells["Pre_Recebimento_Grupo_ID"].Value.DefaultInteger(),
                                                                                                        this.dgvItensVolumes.SelectedRows[0].Cells["Pre_Recebimento_Volume_CT_Numero"].Value.DefaultInteger());
                    frmPreRecebimentosGuarda.Show(this);
                }
                else
                {
                    MessageBox.Show("Seleciona um volume primeiro.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #endregion

        #region "   Métodos Privados    "

        private void Tratar_Permissoes()
        {
            try
            {
                this.tsmAnaliseMercadologica.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID,
                                                                                              typeof(frmAnalise_Mercadologica_Estoque_New).Name, Acao_Formulario.Abrir.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Eventos()
        {
            try
            {
                this.Load += this.Form_Load;
                this.cmsDetalheItens.Opened += this.ContextMenu_Popup;
                this.tbcHerdado.SelectedIndexChanged += this.Mudar_Selecao_Aba_Principal;
                this.txtEmitenteCNPJCPF.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtEmitenteCNPJCPF.Leave += this.Sair_TextBox_Emitente;
                this.btnPesquisarEmitente.Click += this.Clicar_Botao_Pesquisa_Emitente;
                this.btnPesquisarCondicaoPagamento.Click += this.Clicar_Botao_Pesquisar_Condicao_Pagamento;
                this.txtCondicaoPagamento.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtCondicaoPagamento.Leave += this.Sair_TextBox_Codigo_Condicao_Pagamento;
                this.btnGerarParcelas.Click += this.Clicar_Botao_Gerar_Parcelas;
                this.btnPesquisarOrdemDesembarque.Click += this.Clicar_Botao_Pesquisar_Ordem_Desembarque;
                this.txtOrdemDesembarqueNumero.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtOrdemDesembarqueNumero.Validated += this.Alterar_Texto_Ordem_Desembarque;
                this.cboDestinatarioLoja.SelectedIndexChanged += this.Mudar_Selecao_Loja;
                this.cboLojaRecebimento.LostFocus += this.Perder_Foco_Campo;
                this.btnDesbloqueioLojaRecebimento.Click += this.Clicar_Botao_Permissao_Loja_Entrega;
                this.btnDesbloquearParcela.Click += this.Clicar_Botao_Permissao_Alterar_Parcela;
                this.dgvNotaFiscalItens.Grid00.SelectionChanged += this.Mudar_Indice_Selecionado_Lista_Itens;
                this.dgvNotaFiscalItens.Grid00.Click += this.Mudar_Indice_Selecionado_Lista_Itens;
                this.dgvNotaFiscalItens.Grid00.Sorted += this.Ordernar_Grid_Nota_itens;
                this.dgvNotaFiscalItens.Exibir_Grid_Detalhes_Formulario += this.Exibir_Grid_Detalhes_Formulario;
                this.txtCST.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtCST.LostFocus += this.Perder_Foco_Campo_CST;
                this.btnVisualizarDanfe.Click += this.Clicar_Botao_Visualizar_Danfe;
                this.dtpNotaFiscalEmissao.ValueChanged += this.Alterar_Data_Emissao;
                this.txtCodigoFabricante.LostFocus += this.Perder_Foco_Campo_CodFabricante;
                this.btnPesquisarClassFiscal.Click += this.Clicar_Botao_Pesquisa_Classificacao_Fiscal;
                this.cboTipoRecebimento.SelectedValueChanged += this.Alterar_Combo_Tipo_Recebimento;
                this.cboNotaFiscalNumero.SelectedValueChanged += this.Alterar_Combo_Numero_Nota;
                this.txtChaveAcesso.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtNotaFiscalNumeroControle.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtCodigoClassFiscal.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.btnPesquisaCodigoFabricante.Click += this.Clicar_Botao_Pesquisa_Codigo_Fabricante;
                this.chkImprimirCodigoBarras.Click += this.Alterar_Imprimir_Codigo_De_Barras_Recebimento_IT;

                this.btnInserir.Click += this.Clicar_Botao_Inserir_Item;
                this.btnExcluir.Click += this.Clicar_Botao_Excluir_Item;
                this.txtQuantidadeTotalNotaFiscal.KeyPress += this.Pressionar_Tecla_Campos_Inteiros;
                this.txtQuantidadeTotalNotaFiscal.LostFocus += this.Perder_Foco_Campo_Quantidade_Nota_Fiscal;
                this.mskValorDescontoRevenda.LostFocus += this.Perder_Foco_Desconto;
                this.txtQuantidadeTotalNotaFiscal.LostFocus += this.Perder_Foco_Desconto;
                this.mskCustoNotaFiscal.LostFocus += this.Perder_Foco_Campo_Custo_Nota_Fiscal;

                this.btnProcessar.Click += this.Clicar_Botao_Processar;

                this.tsmProcurarItemNaoEncontrado.Click += this.Clicar_Menu_Procurar_Item_Nao_Cadastrado;
                this.tsmPedidoCompra.Click += this.Clicar_Menu_Pedido_Compra;
                this.tsmDevolverItem.Click += this.Clicar_Menu_Devolver_Item;
                this.tsmEstornarItemDevolvido.Click += this.Clicar_Menu_Estornar_Item_Devolvido;
                this.tsmSubstituirItem.Click += this.Clicar_Menu_Substituir_Item;
                this.tsmAnaliseMercadologica.Click += this.Clicar_Menu_Analise_Mercadologica;
                this.tsmPropriedadesPeca.Click += this.Clicar_Menu_Propriedade_Peca;
                this.tsmDesfazer.Click += this.Clicar_Menu_Desfazer;
                this.tsmReassociarItens.Click += this.Clicar_Menu_Reassociar_Item;
                this.chkVisualizarItensCorretos.Click += this.Clicar_Filtro_Visualizar_Itens_Corretos;
                this.chkVisualizarItensDivergentes.Click += this.Clicar_Filtro_Visualizar_Itens_Divergentes;

                this.btnLoteDevolucao.Click += this.Clicar_Botao_Abrir_Lote_Devolucao;
                this.btnGerarPreRecebimento.Click += this.Clicar_Botao_Gerar_Pre_Recebimento;
                this.btnPrepararMercadoria.Click += this.Clicar_Botao_Preparar_Mercadoria;

                this.chkMarcarTodosPedidos.Click += this.Clicar_Marcar_Todos_Pedidos;
                this.chkMarcarTodosGrupo.Click += this.Clicar_Marcar_Todos_Grupos;
                this.chkMarcarTodosPreRecebimento.Click += this.Clicar_Marcar_Todos_Pre_Recebimento;

                this.dgvGrupoPreRecebimento.KeyDown += this.Pressionar_Tecla_DataGrid_Grupo;
                this.dgvGrupoPreRecebimento.CurrentCellDirtyStateChanged += this.Commit_Alteracao_Grid_Grupo_Pre_Recebimento;
                this.dgvGrupoPreRecebimento.CellValueChanged += this.Clicar_DataGrid_Grupo;
                this.dgvPreRecebimento.KeyDown += this.Pressionar_Tecla_DataGrid_Pre_Recebimento;
                this.dgvPreRecebimento.CurrentCellDirtyStateChanged += this.Commit_Alteracao_Grid_Pre_Recebimento;
                this.dgvPreRecebimento.CellValueChanged += this.Clicar_DataGrid_Pre_Recebimento;
                this.dgvItensPreRecebimento.Click += this.Clicar_DataGrid_Itens_Pre_Recebimento;
                this.dgvItensPreRecebimento.SelectionChanged += this.Clicar_DataGrid_Itens_Pre_Recebimento;

                this.dgvPedidoCompras.KeyDown += this.Pressionar_Tecla_Grid_Pedidos;
                this.dgvPedidoCompras.CellMouseClick += this.Clicar_dgvPedidoCompras;

                this.tsmLoteGuarda.Click += this.Clicar_Menu_Lote_Guarda;
                this.tsmIncluirPedidoCompra.Click += this.Clicar_Menu_Incluir_Pedido_Compra;

                this.txtNotaFiscalNumeroControle.LostFocus += this.Perder_Foco_Campo;
                this.cboNaturezaFinanceira.LostFocus += this.Perder_Foco_Campo;
                this.cboNaturezaOperacao.LostFocus += this.Perder_Foco_Campo;
                this.cboTipoRecebimento.LostFocus += this.Perder_Foco_Campo;

                this.mskValorTotalBaseCalculoICMS.LostFocus += this.Perder_Foco_Campo;
                this.txtCondicaoPagamento.LostFocus += this.Perder_Foco_Campo;

                this.cboEmbalagemCompra.SelectedIndexChanged += this.Alterar_Peca_Embalagem_Compra_Custo;
                this.mskCustoNotaFiscal.LostFocus += this.Alterar_Peca_Embalagem_Compra_Custo;
                this.mskIPIRevenda.LostFocus += this.Alterar_Peca_Embalagem_Compra_Custo;
                this.mskSubstituicaoRevenda.LostFocus += this.Alterar_Peca_Embalagem_Compra_Custo;
                this.mskValorDescontoRevenda.LostFocus += this.Alterar_Peca_Embalagem_Compra_Custo;

                this.dgvParcelas.Click += this.Clicar_DataGrid_Parcelas;
                this.dgvParcelas.SelectionChanged += this.Clicar_DataGrid_Parcelas;
                this.btnAtualizarParcela.Click += this.Clicar_Atualizar_Parcelas;
                this.btnCancelarParcela.Click += this.Clicar_Cancelar_Parcelas;

                this.chkMarcarTodosPedidos.CheckedChanged += this.Clicar_Marcar_Todos_Pedidos;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Configurar_Menus()
        {
            try
            {
                this.tsmAlterarPedidoCompra.DropDownItems.Clear();


                this.tsmAlterarPedidoCompra.Visible = true;
                if (this.dgvNotaFiscalItens.Grid00.RowCount > 0)
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido.DefaultInteger() ||
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido_Governo.DefaultInteger() ||
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale.DefaultInteger() ||
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale_Governo.DefaultInteger() ||
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger() ||
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        this.tsmIncluirPedidoCompra.Enabled = false;
                        this.tsmAlterarPedidoCompra.Enabled = false;
                        this.tsmIncluirPedidoCompra.Visible = false;
                        this.tsmAlterarPedidoCompra.Visible = false;
                    }
                    else
                    {
                        this.tsmIncluirPedidoCompra.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((Mercadocar.ObjetosNegocio.DataObject.LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Mercadocar.Enumerados.Acao_Formulario.Incluir_Item_Pedido.ToString());
                        this.tsmAlterarPedidoCompra.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((Mercadocar.ObjetosNegocio.DataObject.LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Mercadocar.Enumerados.Acao_Formulario.Alterar_Item_Pedido.ToString());
                    }

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Restante"].Value.DefaultInteger() > 0)
                    {
                        this.tsmProcurarItemNaoEncontrado.Visible = true;

                        this.tsmDevolverItem.Enabled = true;
                        this.tsmProcurarItemNaoEncontrado.Enabled = true;

                        if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Devolvido.DefaultInteger() ||
                            this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Cadastro.DefaultInteger())
                        {
                            this.tsmDevolverItem.Visible = false;
                            this.tsmAlterarPedidoCompra.Visible = false;
                            this.tsmIncluirPedidoCompra.Visible = false;
                        }
                        else
                        {
                            this.tsmDevolverItem.Visible = true;

                            if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido.DefaultInteger() ||
                                this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido_Governo.DefaultInteger() ||
                                this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale.DefaultInteger() ||
                                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale_Governo.DefaultInteger() ||
                                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger() ||
                                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                            {
                                this.tsmIncluirPedidoCompra.Enabled = false;
                                this.tsmAlterarPedidoCompra.Enabled = false;
                                this.tsmIncluirPedidoCompra.Visible = false;
                                this.tsmAlterarPedidoCompra.Visible = false;

                                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger() ||
                                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                                {
                                    this.tsmDevolverItem.Enabled = false;
                                    this.tsmDevolverItem.Visible = false;
                                }
                            }
                            else
                            {
                                this.tsmAlterarPedidoCompra.Visible = true;
                                this.tsmIncluirPedidoCompra.Visible = true;
                            }
                        }
                    }
                    else
                    {
                        if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Pre_Recebimento_Preparado"].Value.DefaultBool())
                        {
                            this.tsmDevolverItem.Visible = false;
                            this.tsmDevolverItem.Enabled = false;
                        }
                        else
                        {
                            if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger() ==
                                this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger())
                            {
                                this.tsmDevolverItem.Visible = false;
                                this.tsmDevolverItem.Enabled = false;
                            }
                            else
                            {
                                this.tsmDevolverItem.Visible = true;
                                this.tsmDevolverItem.Enabled = true;
                            }
                        }

                        this.tsmProcurarItemNaoEncontrado.Visible = false;
                        this.tsmIncluirPedidoCompra.Visible = false;


                        this.tsmProcurarItemNaoEncontrado.Enabled = false;
                        this.tsmIncluirPedidoCompra.Enabled = false;

                        if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Devolvido.DefaultInteger()
                            || this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Correto.DefaultInteger())
                        {
                            this.tsmAlterarPedidoCompra.Visible = false;
                            this.tsmAlterarPedidoCompra.Enabled = false;
                        }
                    }

                    this.tsmSubstituirItem.Visible = (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Pedido.DefaultInteger());
                    this.tsmSubstituirItem.Enabled = (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Pedido.DefaultInteger());

                    int intStatus = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"].DefaultInteger();
                    this.tsmReassociarItens.Visible = intStatus != Status_Recebimento.Liberado.DefaultInteger()
                                                    && intStatus != Status_Recebimento.Liberacao_Parcial.DefaultInteger()
                                                    && intStatus != Status_Recebimento.Cancelado.DefaultInteger()
                                                    && intStatus != Status_Recebimento.Pendente_Ordem_Chegada.DefaultInteger();
                    this.tsmReassociarItens.Enabled = intStatus != Status_Recebimento.Liberado.DefaultInteger()
                                                    && intStatus != Status_Recebimento.Liberacao_Parcial.DefaultInteger()
                                                    && intStatus != Status_Recebimento.Cancelado.DefaultInteger()
                                                    && intStatus != Status_Recebimento.Pendente_Ordem_Chegada.DefaultInteger();

                    bool blnItemGravadoBanco = this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_CT_ID"].Value.DefaultInteger() != 0;
                    this.tsmIncluirPedidoCompra.Visible = this.tsmIncluirPedidoCompra.Visible && blnItemGravadoBanco;
                    this.tsmAlterarPedidoCompra.Visible = this.tsmAlterarPedidoCompra.Visible && blnItemGravadoBanco;
                    this.tsmDevolverItem.Visible = this.tsmDevolverItem.Visible && blnItemGravadoBanco;

                    this.tsmIncluirPedidoCompra.Enabled = this.tsmIncluirPedidoCompra.Enabled && blnItemGravadoBanco;
                    this.tsmAlterarPedidoCompra.Enabled = this.tsmAlterarPedidoCompra.Enabled && blnItemGravadoBanco;
                    this.tsmDevolverItem.Enabled = this.tsmDevolverItem.Enabled && blnItemGravadoBanco;

                    this.Configurar_Menus_tsmAlterarPedidoCompra();

                    this.tsmDesfazer.Visible = this.Habilitar_Menu_Desfazer();
                    this.tsmDesfazer.Enabled = this.Habilitar_Menu_Desfazer();

                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger() ||
                                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        this.tsmPedidoGarantia.DropDownItems.Clear();

                        this.tsmPedidoGarantia.Enabled = true;
                        this.tsmPedidoGarantia.Visible = true;

                        this.tsmPedidoCompra.Enabled = false;
                        this.tsmPedidoCompra.Visible = false;

                        this.Configurar_Menus_tsmPedidoGarantia();
                    }
                    else
                    {
                        this.tsmPedidoCompra.DropDownItems.Clear();

                        this.tsmPedidoGarantia.Enabled = false;
                        this.tsmPedidoGarantia.Visible = false;

                        this.tsmPedidoCompra.Enabled = true;
                        this.tsmPedidoCompra.Visible = true;

                        this.Configurar_Menus_tsmPedidoCompra();
                    }

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger() > 0 &&
                                                                               intStatus != Status_Recebimento.Cancelado.DefaultInteger() && 
                                                                               intStatus != Status_Recebimento.Devolvido_Integralmente.DefaultInteger() && 
                                                                               intStatus != Status_Recebimento.Liberado.DefaultInteger()) 
                    {
                        this.tsmEstornarItemDevolvido.Visible = true;
                    }
                    else
                    {
                        this.tsmEstornarItemDevolvido.Visible = false;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Configurar_Menus_tsmAlterarPedidoCompra()
        {
            try
            {
                if (this.tsmAlterarPedidoCompra.Enabled && this.tsmAlterarPedidoCompra.Visible)
                {
                    int intQtdeLinhas = 0;
                    int intContadorItens = 0;

                    intQtdeLinhas = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString()).Length;

                    if (intQtdeLinhas > 0)
                    {
                        ToolStripMenuItem[] objItems = new ToolStripMenuItem[intQtdeLinhas];
                        foreach (DataRow dtrNotaPedidoItens in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString()))
                        {
                            objItems[intContadorItens] = new ToolStripMenuItem();
                            objItems[intContadorItens].Name = "tsmAlterarPedidoCompra" + dtrNotaPedidoItens["Pedido_Compra_CT_ID"].DefaultString();
                            objItems[intContadorItens].Text = dtrNotaPedidoItens["Pedido_Compra_CT_ID"].DefaultString();
                            objItems[intContadorItens].Tag = dtrNotaPedidoItens["Pedido_Compra_IT_ID"].DefaultString().Trim();
                            objItems[intContadorItens].Click += new EventHandler(this.Clicar_Alterar_Pedido_Compra);

                            intContadorItens++;
                        }

                        this.tsmAlterarPedidoCompra.DropDownItems.AddRange(objItems);

                        if (objItems.Length == 0)
                        {
                            this.tsmAlterarPedidoCompra.Visible = false;
                        }
                    }
                    else
                    {
                        if (this.dgvPedidoCompras.Rows.Count > 0)
                        {
                            intQtdeLinhas = ((DataTable)this.dgvPedidoCompras.DataSource).Rows.Count;
                            ToolStripMenuItem[] objItems = new ToolStripMenuItem[intQtdeLinhas];
                            foreach (DataRow dgvPedidos in ((DataTable)this.dgvPedidoCompras.DataSource).Rows)
                            {
                                objItems[intContadorItens] = new ToolStripMenuItem();
                                objItems[intContadorItens].Name = "tsmAlterarPedidoCompra" + dgvPedidos["Pedido_ID"].DefaultString();
                                objItems[intContadorItens].Text = dgvPedidos["Pedido_ID"].DefaultString();
                                objItems[intContadorItens].Tag = string.Empty;
                                objItems[intContadorItens].Click += new EventHandler(this.Clicar_Alterar_Pedido_Compra);

                                intContadorItens++;
                            }

                            this.tsmAlterarPedidoCompra.DropDownItems.AddRange(objItems);

                            if (objItems.Length == 0)
                            {
                                this.tsmAlterarPedidoCompra.Visible = false;
                            }
                        }
                        else
                        {
                            this.tsmAlterarPedidoCompra.Visible = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Menus_tsmPedidoCompra()
        {
            try
            {
                if (this.tsmPedidoCompra.Enabled && this.tsmPedidoCompra.Visible)
                {
                    int intQtdeLinhas = 0;
                    int intContadorItens = 0;

                    intQtdeLinhas = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString()).Length;

                    if (intQtdeLinhas > 0)
                    {
                        ToolStripMenuItem[] objItems = new ToolStripMenuItem[intQtdeLinhas];
                        foreach (DataRow dtrNotaPedidoItens in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString()))
                        {
                            objItems[intContadorItens] = new ToolStripMenuItem();
                            objItems[intContadorItens].Name = "tsmPedidoCompra" + dtrNotaPedidoItens["Pedido_Compra_CT_ID"].DefaultString();
                            objItems[intContadorItens].Text = dtrNotaPedidoItens["Pedido_Compra_CT_ID"].DefaultString();
                            objItems[intContadorItens].Tag = dtrNotaPedidoItens["Pedido_Compra_IT_ID"].DefaultString().Trim();
                            objItems[intContadorItens].Click += new EventHandler(this.Clicar_Abrir_Pedido_Compra);

                            intContadorItens++;
                        }

                        this.tsmPedidoCompra.DropDownItems.AddRange(objItems);

                        if (objItems.Length == 0)
                        {
                            this.tsmPedidoCompra.Visible = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Menus_tsmPedidoGarantia()
        {
            try
            {
                if (this.tsmPedidoGarantia.Enabled && this.tsmPedidoGarantia.Visible)
                {
                    int intQtdeLinhas = 0;
                    int intContadorItens = 0;

                    intQtdeLinhas = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString()).Length;

                    if (intQtdeLinhas > 0)
                    {
                        ToolStripMenuItem[] objItems = new ToolStripMenuItem[intQtdeLinhas];
                        foreach (DataRow dtrNotaPedidoItens in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString()))
                        {
                            objItems[intContadorItens] = new ToolStripMenuItem();
                            objItems[intContadorItens].Name = "tsmPedidoGarantia" + dtrNotaPedidoItens["Pedido_Compra_CT_ID"].DefaultString();
                            objItems[intContadorItens].Text = dtrNotaPedidoItens["Pedido_Compra_CT_ID"].DefaultString();
                            objItems[intContadorItens].Tag = dtrNotaPedidoItens["Pedido_Compra_IT_ID"].DefaultString().Trim();
                            objItems[intContadorItens].Click += new EventHandler(this.Clicar_Abrir_Pedido_Garantia);

                            intContadorItens++;
                        }

                        this.tsmPedidoGarantia.DropDownItems.AddRange(objItems);

                        if (objItems.Length == 0)
                        {
                            this.tsmPedidoGarantia.Visible = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region "   Inicializar             "

        private void Carregar_Tela()
        {
            try
            {
                this.Carregar_Dados();

                this.Visualizar_Botao_Lote_Devolucao();
                this.Habilitar_Botao_Pre_Recebimento();
                this.Habilitar_Botao_Preparar_Mercadoria();

                this.Verificar_Mudancas();
                this.Configurar_Menus();

                this.Habilitar_Campos_Parcelas(false);
                this.Limpar_Campos_Parcelas();

                this.Tratar_Permissoes();

                this.Validar_Data_Emissao_Parcela();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Formulario()
        {
            try
            {
                // Ordem de Desembarque
                this.dtpOrdemDesembarqueData.Value = DateTime.Today.Date;

                // Dados Nota Fiscal
                this.dtpNotaFiscalEmissao.Value = DateTime.Today.Date;
                this.lblNotaFiscalLancamento.Text = DateTime.Today.Date.ToString();
                this.dtpNotaFiscalSaida.Value = DateTime.Today.Date;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Configurar_Grid_NFe()
        {
            try
            {
                this.dgvNotaFiscalItens.Grid00.Columns.Clear();
                this.dgvNotaFiscalItens.Grid00.AutoGenerateColumns = false;
                this.dgvNotaFiscalItens.Grid00.MultiSelect = true;
                this.dgvNotaFiscalItens.Grid00.ColorirAoSelecionar = true;

                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Status_Imagem", " ", 30, false, Tipo_Coluna.Imagem, false, DataGridViewContentAlignment.MiddleCenter);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Sequencia", "Seq.", 30, false, Tipo_Coluna.Inteiro, false, DataGridViewContentAlignment.TopRight);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Fabricante_CD", "Fab.", 33, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Produto_CD", "Prod.", 33, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Peca_CD", "Peça", 33, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_NF_CD_Fabricante", "Item Fornecedor NF", 100, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_NF_Descricao", "Descrição NF", 150, true, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_NCM", "NCM/SH", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Tipo_Embalagem", "Unidade", 50, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Custo_Nota_Fiscal", "Vl. Unit. (R$)", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Qtde_Nota_Fiscal", "Qtde. NF", 60, false, Tipo_Coluna.Inteiro, false, DataGridViewContentAlignment.TopRight);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Qtde_Embalagens", "Qtde. Ped.", 70, false, Tipo_Coluna.Inteiro, false, DataGridViewContentAlignment.TopRight);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Qtde_Restante", "Resta", 45, false, Tipo_Coluna.Inteiro, false, DataGridViewContentAlignment.TopRight);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Qtde_Pre_Gerado", "Qtde. Pré", 60, false, Tipo_Coluna.Inteiro, false, DataGridViewContentAlignment.TopRight);
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Qtde_Devolvida", "Qtde. Dev.", 65, false, Tipo_Coluna.Inteiro, false, DataGridViewContentAlignment.TopRight);

                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Pre_Recebimento_CT_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_CT_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Peca_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Codigo_Mercadocar");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Numero_Pedido");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Fabricante_NmFantasia");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Produto_DS");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Peca_Class_Fiscal_CD");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Peca_Class_Fiscal_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Embalagem_Compra_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Pedido_Compra_IT_Quantidade");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_CST");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Data_Liberacao");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_ICMS_Perc");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_IPI_Valor");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Perc_IPI");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_ICMS_ST_Perc");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Valor_Substituicao");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Valor_Desconto");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Observacao");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Custo_Unitario");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Custo_Total");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Recebimento_IT_Custo_Embalagem");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Enum_Tipo_Acao_Conferencia_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Pre_Recebimento_IT_Imprimir_Etiqueta");

                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Status_Recebimento_Nota_Item");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Enum_Status_Comparacao_Item_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Peca_Gerar_Etiqueta_Automatica_Recebimento");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Pre_Recebimento_Preparado");
                
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Pedido_Garantia_CT_ID");
                this.dgvNotaFiscalItens.Grid00.Adicionar_Coluna("Pedido_Garantia_IT_Lojas_ID");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Pedido_Itens()
        {
            try
            {
                this.dgvNotaFiscalItens.Grid01.Columns.Clear();
                this.dgvNotaFiscalItens.Grid01.AutoGenerateColumns = false;
                this.dgvNotaFiscalItens.Grid01.MultiSelect = false;
                this.dgvNotaFiscalItens.Grid01.ColorirAoSelecionar = true;

                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Pedido_Compra_CT_ID", "Pedido", 72, false, Tipo_Coluna.Texto);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Pedido_Compra_IT_Sequencia", "Seq.", 30, false, Tipo_Coluna.Inteiro);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("NF_CD_Fabricante", "Cód. Item Fab.", 140, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Peca_DS", "Descrição", 235, true, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Class_Fiscal_CD", "NCM/SH", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Embalagem_Compra", "Unidade", 80, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Pedido_Compra_IT_Custo_Compra", "Vl. Unitário (R$)", 90, false, Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Recebimento_IT_Pedido_IT_Qtde", "Qtde. Pedido", 70, false, Tipo_Coluna.Inteiro);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Quantidade_Disponivel", "Disponível", 70, false, Tipo_Coluna.Inteiro);
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Recebimento_IT_ID");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Pre_Recebimento_IT_ID");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Status_Recebimento_Pedido_Item");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Peca_Embalagem_Compra_ID");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Peca_ID");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Quantidade_Disponivel_Original");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Enum_Status_Comparacao_Item_ID");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Enum_Status_Comparacao_Item_ID_Original");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Recebimento_IT_Pedido_IT");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Pre_Recebimento_Gerado");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Pedido_Garantia_Peca_Substituida");
                this.dgvNotaFiscalItens.Grid01.Adicionar_Coluna("Enum_Tipo_Pedido_Compra_ID");


            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Pedidos_Compra()
        {
            try
            {
                this.dgvPedidoCompras.Columns.Clear();
                this.dgvPedidoCompras.AutoGenerateColumns = false;
                this.dgvPedidoCompras.Adicionar_Coluna("Marcado", " ", 20, false, Tipo_Coluna.CheckBox, true);
                this.dgvPedidoCompras.Adicionar_Coluna("Pedido_ID", "Pedido", 65, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvPedidoCompras.Adicionar_Coluna("Data_Geracao", "Data do Pedido", 90, false, Tipo_Coluna.Data, false, DataGridViewContentAlignment.TopRight);
                this.dgvPedidoCompras.Adicionar_Coluna("Status_NM", "Status", 120, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvPedidoCompras.Adicionar_Coluna("Usuario_NM", "Comprador", 120, true, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvPedidoCompras.Adicionar_Coluna("Data_Previsao", "Previsão de Entrega", 110, false, Tipo_Coluna.Data, false, DataGridViewContentAlignment.TopRight);
                this.dgvPedidoCompras.Adicionar_Coluna("Condicao_Pagamento_DS", "Condição de Pagamento", 138, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvPedidoCompras.Adicionar_Coluna("Encomenda_Venda_CT_ID", "Encomenda", 70, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvPedidoCompras.Adicionar_Coluna("Encomenda_Lojas_NM", "Origem", 90, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvPedidoCompras.Adicionar_Coluna("Encomenda_Loja_Origem_ID");
                this.dgvPedidoCompras.Adicionar_Coluna("Encomenda_Enum_Tipo_ID");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Pedidos_Parcelas()
        {
            try
            {

                this.dgvParcelas.Columns.Clear();
                this.dgvParcelas.AutoGenerateColumns = false;
                this.dgvParcelas.Adicionar_Coluna("Recebimento_Parcelas_ID");
                this.dgvParcelas.Adicionar_Coluna("Recebimento_CT_ID");
                this.dgvParcelas.Adicionar_Coluna("Recebimento_Parcelas_Seq");
                this.dgvParcelas.Adicionar_Coluna("Numero_Documento", "Nº Doc. Parcela", 150, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopLeft);
                this.dgvParcelas.Adicionar_Coluna("Data_Vencimento", "Data Vencimento", 100, false, Tipo_Coluna.Data);
                this.dgvParcelas.Adicionar_Coluna("Parcela_Valor", "Valor Parcela (R$)", 120, true, Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.TopRight);
                this.dgvParcelas.Adicionar_Coluna("Recebimento_Parcelas_IsEnviado_Microsiga");

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Dados()
        {
            try
            {
                if (this.intRecebimentoCTID == 0)
                {
                    this.tbcHerdado.TabPages[this.tbpCompras.Name].Enabled = false;
                    if (this.tbcHerdado.TabPages.Contains(this.tbpPreRecebimento))
                    {
                        this.tbcHerdado.TabPages[this.tbpPreRecebimento.Name].Enabled = false;
                    }

                    this.Habilitar_Campos_Dados_Nota_Fiscal(false);
                    return;
                }

                Recebimento_NFeBUS busRecebimentoNfe = new Recebimento_NFeBUS();
                this.dtsRecebimento = busRecebimentoNfe.Consultar_DataSet_Propriedades(this.intRecebimentoCTID);

                this.Criar_Relacionamento_Nota_Pedido();

                this.Preencher_Aba_Dados_Nota_Fiscal();

                this.Habilitar_Campos_Dados_Nota_Fiscal(this.intRecebimentoCTID == 0);

                this.Preencher_Aba_Itens();

                this.Habilitar_Campos_Itens_Detalhes();

                this.dtsRecebimentoTemporario = this.dtsRecebimento.Copy();

                this.Setar_Imagens_Grid();

                this.Buscar_Ordem_Desembarque_NF();

                if (this.dtsRecebimento.Tables["Fornecedor"].Rows.Count > 0 && this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].ToInteger() != 0)
                {
                    if (this.dtsRecebimento.Tables.Count > 0 && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0)
                    {
                        this.Preencher_Grid_Pedidos(this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(), 0, this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_ID"].DefaultInteger());
                    }
                    else
                    {
                        this.Preencher_Grid_Pedidos(this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(), 0, 0);
                    }
                }

                this.Carregar_Grupo();

                this.Carregar_Dados_Integracao_Microsiga();

                this.Verificar_Processar_Tipo_Recebimento();
                this.Habilitar_Botao_Pre_Recebimento();
                this.Habilitar_Botao_Preparar_Mercadoria();
                this.Habilitar_Campos_Parcelas(false);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Dados_Integracao_Microsiga()
        {
            try
            {
                Int32 intNotaFiscal = default(Int32);
                Int32 intLoja = default(Int32);

                if (this.lblRecebimentoID.Text.DefaultInteger() != 0)
                {
                    intNotaFiscal = this.lblRecebimentoID.Text.DefaultInteger();
                }
                if (this.cboDestinatarioLoja.SelectedValue.DefaultInteger() != 0)
                {
                    intLoja = this.cboDestinatarioLoja.SelectedValue.DefaultInteger();
                }

                Recebimento_Nota_FiscalBUS busConsulta = new Recebimento_Nota_FiscalBUS();
                DataSet dtsInterface = busConsulta.Consultar_Interface(intNotaFiscal, intLoja);

                if (dtsInterface.Tables[0].Rows.Count > 0)
                {
                    this.lblEnviadoInterface.Text = "Sim";
                    this.lblDataEnvioInterface.Text = Convert.ToString(dtsInterface.Tables[0].Rows[0]["Interface_Entrada_CT_Data_Inclusao"]);
                    if (string.IsNullOrEmpty(Convert.ToString(dtsInterface.Tables[0].Rows[0]["Interface_Entrada_CT_Processado"])))
                    {
                        this.lblImportadoMicrosiga.Text = "Não";
                        this.lblDataImportacaoMicrosiga.Text = string.Empty;
                    }
                    else
                    {
                        if (Convert.ToString(dtsInterface.Tables[0].Rows[0]["Interface_Entrada_CT_Processado"]) == "E")
                        {
                            this.lblImportadoMicrosiga.Text = "Não - Erro";
                        }
                        else
                        {
                            this.lblImportadoMicrosiga.Text = "Sim";
                        }
                        this.lblDataImportacaoMicrosiga.Text = Convert.ToString(dtsInterface.Tables[0].Rows[0]["Interface_Entrada_CT_Data_Processamento"]);
                    }
                }
                else
                {
                    this.lblEnviadoInterface.Text = "Não";
                    this.lblDataEnvioInterface.Text = string.Empty;
                    this.lblImportadoMicrosiga.Text = "Não";
                    this.lblDataImportacaoMicrosiga.Text = string.Empty;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Aba Nota Fiscal         "

        #region "   Carregar Combo          "

        private void Carregar_Combos_Aba_Dados_Nota_Fiscal()
        {
            try
            {
                this.Carregar_Combo_Natureza_Operacao();
                this.Carregar_Combo_Natureza_Financeira();
                this.Carregar_Combo_Modelos();
                this.Carregar_Combo_Tipo_Operacao();
                this.Carregar_Combo_Tipo_Recebimento();
                this.Carregar_Combo_Lojas();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Combo_Natureza_Operacao()
        {
            try
            {
                DataTable dttNaturezaOperacao = Utilitario.Obter_DataTable_Enumerado_Da_Memoria_Natureza_Operacao();

                this.cboNaturezaOperacao.ValueMember = "Enum_ID";
                this.cboNaturezaOperacao.DisplayMember = "Enum_Extenso";
                this.cboNaturezaOperacao.DataSource = dttNaturezaOperacao;

                this.cboNaturezaOperacao.SelectedIndex = -1;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Natureza_Financeira()
        {
            try
            {
                Natureza_FinanceiraBUS busNaturezaFinanceira = new Natureza_FinanceiraBUS();
                DataTable dttNaturezaFinanceira = busNaturezaFinanceira.Consultar_DataSet_Natureza_Financeira(Convert.ToBoolean(false)).Tables[0];

                this.cboNaturezaFinanceira.ValueMember = "ID";
                this.cboNaturezaFinanceira.DisplayMember = "Descricao";
                this.cboNaturezaFinanceira.DataSource = dttNaturezaFinanceira;

                this.cboNaturezaFinanceira.SelectedIndex = -1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Modelos()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Exibindo_Extenso(ref this.cboModelo, "ModeloNotaFiscal", String.Empty, false, String.Empty, "Enum_Extenso");

                this.cboModelo.SelectedIndex = -1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Tipo_Operacao()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Exibindo_Extenso(ref this.cboTipoOperacao, "TipoOperacao", String.Empty, false, String.Empty, "Enum_Extenso");

                this.cboTipoOperacao.SelectedIndex = -1;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Combo_Tipo_Recebimento()
        {
            try
            {
                DataSet dtsTipo = new DataSet();
                dtsTipo.Tables.Add("Tipo_Recebimento");
                dtsTipo.Tables["Tipo_Recebimento"].Columns.Add("Enum_ID", typeof(int));
                dtsTipo.Tables["Tipo_Recebimento"].Columns.Add("Enum_Extenso", typeof(string));

                DataTable dttTipos = Utilitario.Obter_DataSet_Enumerado_Da_Memoria_Escolhendo_Colunas(new List<string> { "Enum_ID", "Enum_Extenso" }, "TipoRecebimento", string.Empty, "Enum_Sigla").Tables[0];
                foreach (DataRow dtrTipo in dttTipos.Rows)
                {
                    if (dtrTipo["Enum_ID"].ToInteger() == (int)Tipo_Recebimento.Bonificacao_Pedido
                        || dtrTipo["Enum_ID"].ToInteger() == (int)Tipo_Recebimento.Encomenda_Pedido
                        || dtrTipo["Enum_ID"].ToInteger() == (int)Tipo_Recebimento.Revenda_Pedido
                        || dtrTipo["Enum_ID"].ToInteger() == (int)Tipo_Recebimento.Garantia_Pedido
                        || dtrTipo["Enum_ID"].ToInteger() == (int)Tipo_Recebimento.Consumo_Pedido)
                    {
                        DataRow dtrTipoRecebimento = dtsTipo.Tables["Tipo_Recebimento"].Rows.Add();
                        dtrTipoRecebimento["Enum_ID"] = dtrTipo["Enum_ID"];
                        dtrTipoRecebimento["Enum_Extenso"] = dtrTipo["Enum_Extenso"];
                    }
                }

                dtsTipo.Tables["Tipo_Recebimento"].DefaultView.Sort = "Enum_Extenso";

                this.cboTipoRecebimento.ValueMember = "Enum_ID";
                this.cboTipoRecebimento.DisplayMember = "Enum_Extenso";
                this.cboTipoRecebimento.DataSource = dtsTipo.Tables["Tipo_Recebimento"];

                this.cboTipoRecebimento.SelectedIndex = -1;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Combo_Lojas()
        {
            try
            {
                LojasBUS busLoja = new LojasBUS();
                this.dttLojas = busLoja.Consultar_DataTable_CNPJ_UF_Lojas_Ativas();

                this.cboDestinatarioLoja.DisplayMember = "Lojas_NM";
                this.cboDestinatarioLoja.ValueMember = "Lojas_Id";
                this.cboDestinatarioLoja.DataSource = this.dttLojas;

                this.cboDestinatarioLoja.SelectedValue = ((LojasDO)Root.Loja_Ativa_NEW).ID;

                this.cboLojaRecebimento.DisplayMember = "Lojas_NM";
                this.cboLojaRecebimento.ValueMember = "Lojas_Id";
                this.cboLojaRecebimento.DataSource = this.dttLojas.Copy();

                this.cboLojaRecebimento.SelectedValue = ((LojasDO)Root.Loja_Ativa_NEW).ID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Nota_Fiscal(DataTable dttNotaFiscal)
        {
            try
            {
                this.cboNotaFiscalNumero.SelectedValueChanged -= this.Alterar_Combo_Numero_Nota;

                this.cboNotaFiscalNumero.DisplayMember = "Ordem_Desembarque_NF_Numero";
                this.cboNotaFiscalNumero.ValueMember = "Ordem_Desembarque_NF_ID";
                this.cboNotaFiscalNumero.DataSource = dttNotaFiscal;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                this.cboNotaFiscalNumero.SelectedValueChanged += this.Alterar_Combo_Numero_Nota;
            }
        }

        #endregion

        #region "   Ordem Desembarque       "

        private void Configurar_DataSet_Recebimento()
        {
            try
            {
                this.dtsRecebimento.Reset();

                this.dtsRecebimento.Tables.Add("RecebimentoIT");
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Pre_Recebimento_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Pre_Recebimento_IT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Numero_Pedido", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Peca_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Sequencia", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Fabricante_NmFantasia", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Fabricante_CD", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Produto_CD", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Peca_CD", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Codigo_Mercadocar", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Peca_Class_Fiscal_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Peca_Class_Fiscal_CD", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Produto_DS", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_NF_CD_Fabricante", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_NF_Descricao", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Tipo_Embalagem", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Embalagem_Compra_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_CST", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_NCM", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_CFOP", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Qtde_Embalagens", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Qtde_Pre_Gerado", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Qtde_Devolvida", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Qtde_Nota_Fiscal", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Qtde_Restante", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Qtde_Total", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Custo_Embalagem", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Custo_Unitario", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Custo_Nota_Fiscal", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Custo_Total", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Custo_Efetivo", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Valor_Desconto", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_Perc", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Perc_IPI", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Valor_Base_ICMS", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Valor_Substituicao", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_ST_Perc", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Nao_Cadastrado_CDFabricante", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Nao_Cadastrado_Descricao", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Justificativa", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Observacao", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Pre_Recebimento_IT_Imprimir_Etiqueta");
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Usuario_Liberacao_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Data_Liberacao", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_IsPedidoOK", typeof(Boolean));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Enum_Tipo_Pre_Recebimento_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Enum_Status_Comparacao_Item_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Enum_Tipo_Acao_Conferencia_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_IPI_Valor", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_ST_Valor", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_IPI_Base", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_PIS_Base", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_PIS_Perc", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_PIS_Valor", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_COFINS_Base", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_COFINS_Perc", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_COFINS_Valor", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_Base", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_Valor", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_ST_MVA", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_ST_Base", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Enum_Origem_Mercadoria_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_Compoe_Total", typeof(Boolean));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Pre_Recebimento_Preparado", typeof(Boolean));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Recebimento_IT_ICMS_Perc_Reducao", typeof(Decimal));


                Recebimento_IT_Pedido_ITBUS busRecebimentoNotaPedido = new Recebimento_IT_Pedido_ITBUS();
                DataTable dttRecebimentoNotaPedido = busRecebimentoNotaPedido.Retornar_Estrutura_Tabela();
                dttRecebimentoNotaPedido.TableName = "Nota_Pedido_Itens";

                dttRecebimentoNotaPedido.Columns.Add("NF_CD_Fabricante", typeof(String));
                dttRecebimentoNotaPedido.Columns.Add("Peca_DS", typeof(String));
                dttRecebimentoNotaPedido.Columns.Add("Class_Fiscal_CD", typeof(String));
                dttRecebimentoNotaPedido.Columns.Add("Embalagem_Compra", typeof(String));
                dttRecebimentoNotaPedido.Columns.Add("Pedido_Compra_IT_Custo_Compra", typeof(Decimal));
                dttRecebimentoNotaPedido.Columns.Add("Quantidade_Disponivel", typeof(Decimal));
                dttRecebimentoNotaPedido.Columns.Add("Quantidade_Disponivel_Original", typeof(Decimal));
                dttRecebimentoNotaPedido.Columns.Add("Pre_Recebimento_IT_ID", typeof(Int32));
                dttRecebimentoNotaPedido.Columns.Add("Peca_ID", typeof(Int32));
                dttRecebimentoNotaPedido.Columns.Add("Enum_Status_Comparacao_Item_ID_Original", typeof(Int32));
                dttRecebimentoNotaPedido.Columns.Add("Recebimento_IT_Pedido_IT", typeof(Int32));
                dttRecebimentoNotaPedido.Columns.Add("Pedido_Garantia_Peca_Substituida", typeof(Boolean));
                dttRecebimentoNotaPedido.Columns.Add("Enum_Tipo_Pedido_Compra_ID", typeof(Int32));
                this.dtsRecebimento.Tables.Add(dttRecebimentoNotaPedido);

                this.dtsRecebimento.Tables.Add("RecebimentoCT");
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_ID_Recebimento_Principal", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Forn_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Modelo_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Status_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Usuario_Lancou_Nota_Fiscal_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Usuario_Modificou_Diferenca_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Usuario_Liberacao_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Tipo_Recebimento1_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Tipo_Recebimento2_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Tipo_Operacao_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Loja_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Loja_Entrega_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Natureza_Operacao1_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Natureza_Operacao2_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Natureza_Operacao3_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Condicao_Pagamento_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Condicao_Pagamento_CT_CD", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Nota_Fiscal_Entrada_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Numero_Nota_Fiscal", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Numero_Serie", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Numero_Controle", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Data_Lancamento", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Data_Saida", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Data_Emissao", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Data_Ultima_Alteracao", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Total", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Produtos", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Base_ICMS", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ICMS", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Substituicao", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ICMS_Substituicao", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_IPI", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Outros", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Seguro", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Frete", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Base_ICMS_Isento", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ICMS_07", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Base_ICMS_07", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ICMS_12", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Base_ICMS_12", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ICMS_18", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Base_ICMS_18", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ICMS_25", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Base_ICMS_25", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Desconto", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Diferenca", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Outras", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_ISS", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_Impostos_Recolhidos", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Observacao", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Justificativa", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_IsEnviado_Microsiga", typeof(Boolean));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Chave_Acesso", typeof(Boolean));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Natureza_Financeira_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Ordem_Desembarque_NF_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Ordem_Desembarque_Data_Criacao", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Ordem_Desembarque_Sequencial_Dia", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("NFe_Entrada_XML_ID", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Data_Verificacao_Concorrencia", typeof(byte[]));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Nota_Cancelada", typeof(bool));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Data_Entrada", typeof(DateTime));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_PIS", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Valor_COFINS", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Volumes_Qtde", typeof(Int32));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Volumes_Especie", typeof(String));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Peso_Bruto", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Recebimento_CT_Peso_Liquido", typeof(Decimal));
                this.dtsRecebimento.Tables["RecebimentoCT"].Columns.Add("Enum_Finalidade_Emissao_ID", typeof(Int32));


                this.dtsRecebimento.Tables.Add("Parcelas");
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Recebimento_Parcelas_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Recebimento_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Recebimento_Parcelas_Seq", typeof(Int32));
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Data_Vencimento", typeof(DateTime));
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Parcela_Valor", typeof(Decimal));
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Numero_Documento", typeof(String));
                this.dtsRecebimento.Tables["Parcelas"].Columns.Add("Recebimento_Parcelas_IsEnviado_Microsiga", typeof(Boolean));

                this.dtsRecebimento.Tables.Add("Fornecedor");
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Forn_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Pessoa_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Forn_CD", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Enum_Tipo_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Forn_IsAtivo", typeof(Boolean));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Forn_IsRevendedor", typeof(Boolean));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Forn_IsOptanteSimples", typeof(Boolean));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Condicao_Pagamento_CT_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Estado_UF_ID", typeof(Int32));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Data_Inicio_Acordo", typeof(DateTime));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Data_Termino_Acordo", typeof(DateTime));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Desconto_Percentual", typeof(Decimal));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Preco_Minimo_Pedido", typeof(Decimal));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Obs", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_ST_Percentual_Acordo", typeof(Decimal));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_IPI_Percentual_Acordo", typeof(Decimal));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Prazo_Entrega_Acordo", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Frete_Acordo", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Marketing_Acordo", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Bonificacao_Acordo", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_Nome", typeof(String));
                this.dtsRecebimento.Tables["Fornecedor"].Columns.Add("Fornecedor_CPF_CNPJ", typeof(String));

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_DataSet_Recebimento_Ordem_Desembarque()
        {
            try
            {
                foreach (DataRow dtrOrdemDesemnarqueNF in this.dttOrdemDesembarqueNF.Rows)
                {
                    DataRow dtrRecebimentoCT = this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Add();
                    dtrRecebimentoCT["Loja_ID"] = dtrOrdemDesemnarqueNF["Lojas_ID"].DefaultInteger();
                    dtrRecebimentoCT["Loja_Entrega_ID"] = dtrOrdemDesemnarqueNF["Lojas_ID"].DefaultInteger();
                    dtrRecebimentoCT["Forn_ID"] = dtrOrdemDesemnarqueNF["Fornecedor_ID"].DefaultInteger();
                    dtrRecebimentoCT["Recebimento_CT_Numero_Nota_Fiscal"] = dtrOrdemDesemnarqueNF["Ordem_Desembarque_NF_Numero"].DefaultString();
                    dtrRecebimentoCT["Enum_Tipo_Recebimento1_ID"] = dtrOrdemDesemnarqueNF["Enum_Tipo_ID"].DefaultInteger();
                    dtrRecebimentoCT["Recebimento_CT_Valor_Total"] = dtrOrdemDesemnarqueNF["Ordem_Desembarque_NF_Valor"].DefaultDecimal();
                    dtrRecebimentoCT["Recebimento_CT_ID"] = this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count;
                }

                DataRow dtrFornecedor = this.dtsRecebimento.Tables["Fornecedor"].Rows.Add();
                dtrFornecedor["Forn_ID"] = this.dttOrdemDesembarqueNF.Rows[0]["Fornecedor_ID"].DefaultInteger();

                this.intOrdemDesembarqueNFID = this.dttOrdemDesembarqueNF.Rows[0]["Ordem_Desembarque_NF_ID"].DefaultInteger();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Buscar_Ordem_Desembarque_NF()
        {
            try
            {
                if (this.cboDestinatarioLoja.SelectedValue == null)
                {
                    return false;
                }

                if (this.txtOrdemDesembarqueNumero.Text == string.Empty)
                {
                    return false;
                }

                Ordem_DesembarqueBUS busOrdemDesembarqueNF = new Ordem_DesembarqueBUS();
                this.dttOrdemDesembarqueNF = busOrdemDesembarqueNF.Consultar_DataSet_Ordem_Desembarque_NF(this.cboLojaRecebimento.SelectedValue.DefaultInteger(), this.txtOrdemDesembarqueNumero.Text.DefaultInteger(), this.dtpOrdemDesembarqueData.Value.Date).Tables[0];

                if (this.dttOrdemDesembarqueNF.Rows.Count <= 0)
                {
                    MessageBox.Show("A ordem de desembarque informada não foi encontrada!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpOrdemDesembarqueData.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Ordem_Desembarque_NF(int intIndiceNota)
        {
            try
            {
                if (this.Validar_Ordem_Desembarque(this.dttOrdemDesembarqueNF, intIndiceNota) == false)
                {
                    return;
                }

                DBUtil objUtil = new DBUtil();
                this.dtoPropriedades.Data_Emissao = objUtil.Obter_Data_do_Servidor(true, TipoServidor.Central);

                this.Carregar_Combo_Nota_Fiscal(this.dttOrdemDesembarqueNF);

                this.Preencher_Campos_Nota_Fiscal(this.dttOrdemDesembarqueNF, intIndiceNota);

                this.Preencher_Campos_Nota_Fiscal_Totais(this.dttOrdemDesembarqueNF, intIndiceNota);

                this.Preencher_Campos_Emitente(this.Buscar_Fornecedor(this.dttOrdemDesembarqueNF.Rows[intIndiceNota]["Fornecedor_ID"].DefaultInteger(), string.Empty));

                this.Preencher_Grid_Pedidos(this.dttOrdemDesembarqueNF.Rows[intIndiceNota]["Fornecedor_ID"].DefaultInteger(), 0, 0);

                this.Habilitar_Campos_Dados_Nota_Fiscal(true);

                this.dtsRecebimentoTemporario = this.dtsRecebimento.Copy();

                this.tbcHerdado.TabPages[this.tbpCompras.Name].Enabled = true;

                if (this.tbcHerdado.TabPages.Contains(this.tbpPreRecebimento))
                {
                    this.tbcHerdado.TabPages[this.tbpPreRecebimento.Name].Enabled = true;
                }

                this.cboTipoRecebimento.Focus();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Boolean Validar_Ordem_Desembarque(DataTable dttNotaFiscal, int intIndice)
        {
            try
            {
                if (dttNotaFiscal.Rows[intIndice]["Ordem_Desembarque_Liberado"].DefaultInteger() == 0)
                {
                    MessageBox.Show("A ordem de desembarque informada não foi liberada!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpOrdemDesembarqueData.Focus();
                    return false;
                }

                this.cboTipoRecebimento.SelectedValue = dttNotaFiscal.Rows[intIndice]["Enum_Tipo_ID"];

                if (this.cboTipoRecebimento.SelectedValue == null || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("Tipo de recebimento não encontrado.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.cboTipoRecebimento.Focus();
                    return false;
                }

                if (this.cboDestinatarioLoja.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar a loja de destino.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.cboDestinatarioLoja.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Limpar                  "

        private void Limpar_Dados_Emitente()
        {
            try
            {
                this.intFornecedorID = 0;
                this.strFornCNPJCPF = string.Empty;
                this.txtEmitenteCNPJCPF.Text = string.Empty;
                this.lblEmitenteRazaoSocial.Text = string.Empty;
                this.lblEmitenteEndereco.Text = string.Empty;
                this.lblEmitenteBairro.Text = string.Empty;
                this.lblEmitenteCEP.Text = string.Empty;
                this.lblEmitenteInscEstadual.Text = string.Empty;
                this.lblEmitenteMunicipio.Text = string.Empty;
                this.lblEmitenteTelefone.Text = string.Empty;
                this.lblEmitenteTipo.Text = string.Empty;
                this.lblEmitenteUFSigla.Text = string.Empty;
                this.chkEmitenteOptanteSimples.Checked = false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Limpar_Dados_Nota_Fiscal()
        {
            try
            {
                this.cboNotaFiscalNumero.SelectedValueChanged -= this.Alterar_Combo_Numero_Nota;

                this.txtOrdemDesembarqueNumero.Text = string.Empty;

                this.cboNotaFiscalNumero.DataSource = null;

                this.cboTipoRecebimento.SelectedValue = 0;

                this.txtNotaFiscalNumeroControle.Text = string.Empty;

                this.mskValorTotalNotaFiscal.Text = string.Empty;

                this.Limpar_Dados_Emitente();

                this.dgvPedidoCompras.DataSource = null;

                this.dgvParcelas.DataSource = null;
                this.txtCondicaoPagamento.Text = string.Empty;
                this.lblCondicaoPagamento.Text = string.Empty;

                this.txtObservacoes.Text = string.Empty;

                this.Limpar_Campos_Itens_Detalhes();

                this.dtsRecebimento.Tables["RecebimentoCT"].Clear();
                this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Clear();
                this.dtsRecebimento.Tables["RecebimentoIT"].Clear();

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                this.cboNotaFiscalNumero.SelectedValueChanged += this.Alterar_Combo_Numero_Nota;
            }

        }

        #endregion

        #region "   Preencher               "

        private void Preencher_Objeto_Recebimento(bool blnProcessado)
        {
            try
            {
                DBUtil objUtil = new DBUtil();

                this.dtoPropriedades = new Recebimento_CTDO();

                this.dtoPropriedades.ID = this.intRecebimentoCTID;
                this.dtoPropriedades.Ordem_Desembarque_NF_ID = this.intOrdemDesembarqueNFID;
                this.dtoPropriedades.ID_Fornecedor = this.intFornecedorID;
                this.dtoPropriedades.Status = this.Retornar_Status_Recebimento(blnProcessado);
                this.dtoPropriedades.ID_Loja = this.cboDestinatarioLoja.SelectedValue.DefaultInteger();
                this.dtoPropriedades.Loja_Entrega_ID = this.cboLojaRecebimento.SelectedValue.DefaultInteger();
                this.dtoPropriedades.ID_Modelo = this.cboModelo.SelectedValue.DefaultInteger();
                this.dtoPropriedades.ID_Tipo_Recebimento_1 = this.cboTipoRecebimento.SelectedValue.DefaultInteger();
                this.dtoPropriedades.ID_Tipo_Operacao = this.cboTipoOperacao.SelectedValue.DefaultInteger();
                this.dtoPropriedades.ID_Natureza_Operacao_1 = this.cboNaturezaOperacao.SelectedValue.DefaultInteger();
                this.dtoPropriedades.Natureza_Financeira_ID = this.cboNaturezaFinanceira.SelectedValue.DefaultInteger();
                if (this.dtoCondicaoPagamento != null)
                {
                    this.dtoPropriedades.ID_Condicao_Pagamento = this.dtoCondicaoPagamento.ID;
                }
                this.dtoPropriedades.Condicao_Pagamento = this.dtoCondicaoPagamento;
                this.dtoPropriedades.Numero_Nota_Fiscal = this.cboNotaFiscalNumero.Text;
                this.dtoPropriedades.Numero_Serie = this.lblNotaFiscalSerie.Text;
                this.dtoPropriedades.Numero_Controle = this.txtNotaFiscalNumeroControle.Text.DefaultInteger();
                this.dtoPropriedades.Data_Entrada = new DateTime(1900, 1, 1);
                this.dtoPropriedades.Data_Saida = new DateTime(1900, 1, 1);
                this.dtoPropriedades.Data_Ultima_Alteracao = objUtil.Obter_Data_do_Servidor(true, TipoServidor.Central);
                this.dtoPropriedades.ID_Usuario_Ultima_Alteracao = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID.DefaultInteger();
                this.dtoPropriedades.Data_Cancelamento = new DateTime(1900, 1, 1);
                this.dtoPropriedades.Justificativa = string.Empty;
                this.dtoPropriedades.Chave_Acesso = this.txtChaveAcesso.Text;
                this.dtoPropriedades.Fatura_Numero = string.Empty;
                this.dtoPropriedades.Motivo_Cancelamento = string.Empty;
                this.dtoPropriedades.Volumes_Especie = string.Empty;
                this.dtoPropriedades.Volumes_Marca = string.Empty;
                this.dtoPropriedades.Volumes_Numeracao = string.Empty;

                this.dtoPropriedades.Data_Emissao = this.dtpNotaFiscalEmissao.Value;
                this.dtoPropriedades.Data_Lancamento = this.lblNotaFiscalLancamento.Text.DefaultDateTime();
                this.dtoPropriedades.Data_Saida = this.dtpNotaFiscalSaida.Value;
                this.dtoPropriedades.Observacao = this.txtObservacoes.Text.Trim();

                this.dtoPropriedades.ID_Usuario_Lancou_Nota_Fiscal = this.intRecebimentoCTID != 0 ? this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Usuario_Lancou_Nota_Fiscal_ID"].DefaultInteger() : ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID.DefaultInteger();
                this.dtoPropriedades.Lancamento_Novo = true;
                this.dtoPropriedades.Data_Verificacao_Concorrencia = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Data_Verificacao_Concorrencia"].DefaultString() != string.Empty ? (byte[])this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Data_Verificacao_Concorrencia"] : null;

                this.dtoPropriedades.Data_Entrada = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Data_Entrada"].DefaultDateTime();
                this.dtoPropriedades.Valor_PIS = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_PIS"].DefaultDecimal();
                this.dtoPropriedades.Valor_COFINS = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_COFINS"].DefaultDecimal();
                this.dtoPropriedades.Volumes_Quantidade = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Volumes_Qtde"].DefaultInteger();
                this.dtoPropriedades.Volumes_Especie = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Volumes_Especie"].DefaultString();
                this.dtoPropriedades.Peso_Bruto = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Peso_Bruto"].DefaultDecimal();
                this.dtoPropriedades.Peso_Liquido = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Peso_Liquido"].DefaultDecimal();
                this.dtoPropriedades.ID_Finalidade_Emissao = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Finalidade_Emissao_ID"].DefaultInteger();

                // Valores---------------------------------------------------------------------------
                this.dtoPropriedades.Valor_Total = ((Information.IsNumeric(this.mskValorTotalNotaFiscal.Text) ? this.mskValorTotalNotaFiscal.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_Produtos = ((Information.IsNumeric(this.mskValorTotalProdutos.Text) ? this.mskValorTotalProdutos.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Base_ICMS = ((Information.IsNumeric(this.mskValorTotalBaseCalculoICMS.Text) ? this.mskValorTotalBaseCalculoICMS.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_ICMS = ((Information.IsNumeric(this.mskValorTotalICMS.Text) ? this.mskValorTotalICMS.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_Substituicao = ((Information.IsNumeric(this.mskValorTotalBaseCalculoICMSSubstituicao.Text) ? this.mskValorTotalBaseCalculoICMSSubstituicao.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_ICMS_Substituicao = ((Information.IsNumeric(this.mskValorTotalICMSSubstituicao.Text) ? this.mskValorTotalICMSSubstituicao.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_IPI = ((Information.IsNumeric(this.mskValorTotalIPI.Text) ? this.mskValorTotalIPI.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_Outros = ((Information.IsNumeric(this.mskValorTotalOutros.Text) ? this.mskValorTotalOutros.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_Seguro = ((Information.IsNumeric(this.mskValorTotalSeguro.Text) ? this.mskValorTotalSeguro.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_Frete = ((Information.IsNumeric(this.mskValorTotalFrete.Text) ? this.mskValorTotalFrete.Text : "0")).DefaultDecimal();
                this.dtoPropriedades.Valor_Desconto = ((Information.IsNumeric(this.mskValorTotalDesconto.Text) ? this.mskValorTotalDesconto.Text : "0")).DefaultDecimal();
                // -------------------------------------------------------------------------
                this.Preencher_Objeto_Recebimento_Parcelas();
                // -------------------------------------------------------------------------
                this.Preencher_Objeto_Recebimento_Itens();

                this.Preencher_Objeto_Recebimento_Nota_Pedido();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Objeto_Recebimento_Parcelas()
        {
            try
            {
                if (this.dtsRecebimento.Tables["Parcelas"].Rows.Count > 0)
                {
                    this.dtoPropriedades.Recebimento_Parcelas.Clear();

                    foreach (DataGridViewRow dgrParcela in this.dgvParcelas.Rows)
                    {
                        Recebimento_ParcelasDO dtoRecebimentoParcela = new Recebimento_ParcelasDO();

                        dtoRecebimentoParcela.ID_Recebimento = dgrParcela.Cells["Recebimento_CT_ID"].Value.DefaultInteger();
                        dtoRecebimentoParcela.Sequencia = Convert.ToInt16(dgrParcela.Cells["Recebimento_Parcelas_Seq"].Value);
                        dtoRecebimentoParcela.Data_Vencto = dgrParcela.Cells["Data_Vencimento"].Value.DefaultDateTime();
                        dtoRecebimentoParcela.Documento = dgrParcela.Cells["Numero_Documento"].Value.DefaultString();
                        dtoRecebimentoParcela.Valor = dgrParcela.Cells["Parcela_Valor"].Value.DefaultDecimal();
                        dtoRecebimentoParcela.Enviado_Microsiga = dgrParcela.Cells["Recebimento_Parcelas_IsEnviado_Microsiga"].Value.DefaultBool();

                        this.dtoPropriedades.Recebimento_Parcelas.Add(dtoRecebimentoParcela);
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Objeto_Codigo_Fornecedor(int pecaID)
        {
            try
            {
                DateTime dataAtual = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.Central);
                Peca_Codigo_FornecedorDO dtoPecaCodigoFornecedor = new Peca_Codigo_FornecedorDO();
                List<Peca_Codigo_FornecedorDO> lstPecaCodigoFornecedor = new List<Peca_Codigo_FornecedorDO>();
                lstPecaCodigoFornecedor = this.dtoPecaCodigoFornecedorIncluir.Codigo_Fornecedor;
                
                lstPecaCodigoFornecedor.RemoveAll(Item => Item.Codigo.ToString() == this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NF_CD_Fabricante"].Value.DefaultString());

                dtoPecaCodigoFornecedor.Peca_ID = pecaID;
                dtoPecaCodigoFornecedor.Fornecedor.ID = this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger();
                dtoPecaCodigoFornecedor.Usuario.ID = Root.Funcionalidades.UsuarioDO_Ativo.ID;
                dtoPecaCodigoFornecedor.Codigo = this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NF_CD_Fabricante"].Value.DefaultString();
                dtoPecaCodigoFornecedor.Data = dataAtual;
                dtoPecaCodigoFornecedor.IsAtivo = true;
                
                lstPecaCodigoFornecedor.Add(dtoPecaCodigoFornecedor);

                this.dtoPecaCodigoFornecedorIncluir.Codigo_Fornecedor = lstPecaCodigoFornecedor;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Objeto_Recebimento_Nota_Pedido()
        {

            try
            {
                if (this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Count > 0)
                {
                    DBUtil objUtil = new DBUtil();
                    this.dtoPropriedades.Recebimento_Pedido.Clear();

                    foreach (DataRow dtrNotaPedido in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Processado = " + true))
                    {
                        Recebimento_IT_Pedido_ITDO dtoRecebimentoPedido = new Recebimento_IT_Pedido_ITDO();

                        dtoRecebimentoPedido.Recebimento_IT_Pedido_IT_ID = dtrNotaPedido["Recebimento_IT_Pedido_IT_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Lojas_ID = dtrNotaPedido["Lojas_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Recebimento_CT_ID = dtrNotaPedido["Recebimento_CT_ID"] != null ? dtrNotaPedido["Recebimento_CT_ID"].DefaultInteger() : 0;
                        dtoRecebimentoPedido.Recebimento_IT_ID = dtrNotaPedido["Recebimento_IT_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Pedido_Compra_CT_ID = dtrNotaPedido["Pedido_Compra_CT_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Pedido_Compra_IT_ID = dtrNotaPedido["Pedido_Compra_IT_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Usuario_Inclusao_ID = dtrNotaPedido["Usuario_Inclusao_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Pedido_Compra_IT_Sequencia = dtrNotaPedido["Pedido_Compra_IT_Sequencia"].DefaultInteger();
                        dtoRecebimentoPedido.Quantidade = dtrNotaPedido["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger();
                        dtoRecebimentoPedido.Data_Inclusao = dtrNotaPedido["Recebimento_IT_Pedido_IT_Data_Inclusao"] == null ? dtrNotaPedido["Recebimento_IT_Pedido_IT_Data_Inclusao"].DefaultDateTime() : objUtil.Obter_Data_do_Servidor(true, TipoServidor.Central);
                        dtoRecebimentoPedido.Status_Recebimento_Item = (Status_Recebimento_Item)dtrNotaPedido["Enum_Status_Comparacao_Item_ID"].DefaultInteger();
                        dtoRecebimentoPedido.Pre_Recebimento_Gerado = dtrNotaPedido["Pre_Recebimento_Gerado"].DefaultBool();
                        dtoRecebimentoPedido.Garantia_Peca_Substituta = dtrNotaPedido["Pedido_Garantia_Peca_Substituida"].DefaultBool();

                        this.dtoPropriedades.Recebimento_Pedido.Add(dtoRecebimentoPedido);

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_DataSet_Recebimento_Parcelas()
        {
            try
            {
                if (this.dgvParcelas.Rows.Count > 0)
                {
                    if (!this.dtsRecebimento.IsExistDataTable("Parcelas") || this.dtsRecebimento.Tables["Parcelas"] == null)
                    {
                        DataTable dttParcelas = new DataTable("Parcelas");
                        dttParcelas.Columns.Add("Data_Vencimento", typeof(DateTime));
                        dttParcelas.Columns.Add("Numero_Documento", typeof(string));
                        dttParcelas.Columns.Add("Recebimento_Parcelas_Seq", typeof(Int16));
                        dttParcelas.Columns.Add("Parcela_Valor", typeof(decimal));

                        this.dtsRecebimento.Tables.Add(dttParcelas);
                        this.dtsRecebimento.Tables[this.dtsRecebimento.Tables.Count - 1].TableName = "Parcelas";
                    }

                    int intRecebimentoCTID = 0;
                    if (this.dtsRecebimento.Tables["Parcelas"].Rows.Count > 0)
                    {
                        intRecebimentoCTID = this.dtsRecebimento.Tables["Parcelas"].Rows[0]["Recebimento_CT_ID"].DefaultInteger();
                        this.dtsRecebimento.Tables["Parcelas"].Rows.Clear();
                    }

                    int intRecebimentoParcelasSeq = 0;
                    foreach (DataGridViewRow dgrRecebimentoParcelas in this.dgvParcelas.Rows)
                    {
                        DataRow dtrRecebimentoParcelas = this.dtsRecebimento.Tables["Parcelas"].Rows.Add();
                        dtrRecebimentoParcelas["Recebimento_CT_ID"] = intRecebimentoCTID;
                        intRecebimentoParcelasSeq += 1;
                        dtrRecebimentoParcelas["Recebimento_Parcelas_Seq"] = intRecebimentoParcelasSeq;
                        dtrRecebimentoParcelas["Data_Vencimento"] = dgrRecebimentoParcelas.Cells["Data_Vencimento"].Value.DefaultDateTime();
                        dtrRecebimentoParcelas["Parcela_Valor"] = dgrRecebimentoParcelas.Cells["Parcela_Valor"].Value.DefaultDecimal();
                        dtrRecebimentoParcelas["Numero_Documento"] = dgrRecebimentoParcelas.Cells["Numero_Documento"].Value.DefaultString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Aba_Dados_Nota_Fiscal()
        {
            try
            {
                this.cboDestinatarioLoja.SelectedIndexChanged -= this.Mudar_Selecao_Loja;

                if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0)
                {
                    // Dados Ordem de Desembarque
                    this.dtpOrdemDesembarqueData.Value = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Ordem_Desembarque_Data_Criacao"].DefaultDateTime();
                    this.txtOrdemDesembarqueNumero.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Ordem_Desembarque_Sequencial_Dia"].DefaultString();
                    this.intOrdemDesembarqueNFID = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Ordem_Desembarque_NF_ID"].DefaultInteger();

                    // Dados Recebimento
                    this.lblRecebimentoID.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_ID"].DefaultString();
                    this.lblStatus.Text = ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]).ToDescription();
                    this.lblNotaFiscalLancamento.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Data_Lancamento"].DefaultString();
                    this.dtpNotaFiscalSaida.Value = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Data_Saida"].DefaultDateTime();
                    this.dtpNotaFiscalEmissao.Value = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Data_Emissao"].DefaultDateTime();
                    // Dados Nota Fiscal
                    this.Inicializar_Objeto_Numero_Nota_Fiscal(this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Nota_Fiscal"].DefaultString());

                    this.cboNotaFiscalNumero.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Nota_Fiscal"].DefaultString();
                    this.lblNotaFiscalSerie.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Serie"].DefaultString();
                    this.txtNotaFiscalNumeroControle.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Numero_Controle"].DefaultString();
                    this.txtChaveAcesso.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Chave_Acesso"].DefaultString();

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger() != 0)
                    {
                        this.cboTipoRecebimento.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger();
                    }
                    else
                    {
                        this.cboTipoRecebimento.SelectedIndex = -1;
                    }

                    this.cboTipoOperacao.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Operacao_ID"].DefaultInteger();
                    this.cboNaturezaOperacao.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Natureza_Operacao1_ID"].DefaultInteger();
                    this.cboNaturezaFinanceira.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Natureza_Financeira_ID"].DefaultInteger();
                    this.cboModelo.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Modelo_ID"].DefaultInteger();
                    // Destinatário
                    this.cboDestinatarioLoja.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Loja_ID"].DefaultInteger();
                    this.cboLojaRecebimento.SelectedValue = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Loja_Entrega_ID"].DefaultInteger();
                    // Emitente
                    this.Preencher_Campos_Emitente(this.Buscar_Fornecedor(this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Forn_ID"].DefaultInteger(), string.Empty));

                    // Cobrança
                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Condicao_Pagamento_CT_CD"].DefaultString().Length > 0)
                    {
                        this.txtCondicaoPagamento.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Condicao_Pagamento_CT_CD"].DefaultString().PadLeft(4, Convert.ToChar("0"));
                        this.Carregar_Condicao_Pagamento();
                    }
                    if (this.dtsRecebimento.Tables["Parcelas"].Rows.Count > 0)
                    {
                        this.dgvParcelas.DataSource = this.dtsRecebimento.Tables["Parcelas"];
                        this.dgvParcelas.ClearSelection();
                    }
                    // Informações Adicionais
                    this.txtObservacoes.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Observacao"].DefaultString();
                    // Totais ICMS
                    this.mskValorTotalBaseCalculoICMS.Text = this.Calcular_Valor_Base_Calculo_ICMS().ToString();

                    // Totais NF
                    this.mskValorTotalFrete.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Frete"].DefaultString();
                    this.mskValorTotalSeguro.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Seguro"].DefaultString();
                    this.mskValorTotalDesconto.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Desconto"].DefaultString();
                    this.mskValorTotalProdutos.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Produtos"].DefaultString();
                    this.mskValorTotalOutros.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Outros"].DefaultString();
                    this.mskValorTotalICMS.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_ICMS"].DefaultString();
                    this.mskValorTotalBaseCalculoICMSSubstituicao.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Substituicao"].DefaultString();
                    this.mskValorTotalICMSSubstituicao.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_ICMS_Substituicao"].DefaultString();
                    this.mskValorTotalIPI.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_IPI"].DefaultString();
                    this.mskValorTotalBaseCalculoICMS.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Base_ICMS"].DefaultString();
                    this.mskValorTotalDesconto.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Desconto"].DefaultString();

                    this.mskValorTotalNotaFiscal.Text = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Total"].DefaultDecimal().ToString("#,##0.00");

                    this.blnNotaCancelada = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Nota_Cancelada"].DefaultBool();

                    if (this.blnNotaCancelada)
                    {
                        this.lblNotaCancelada.Visible = true;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.cboDestinatarioLoja.SelectedIndexChanged += this.Mudar_Selecao_Loja;
            }
        }

        private void Preencher_Campos_Emitente(DataTable dttFornecedor)
        {
            try
            {
                if (dttFornecedor.Rows.Count > 0)
                {

                    this.txtEmitenteCNPJCPF.Text = dttFornecedor.Rows[0]["Fornecedor_CPF_CNPJ"].DefaultString();
                    this.lblEmitenteRazaoSocial.Text = dttFornecedor.Rows[0]["Forn_CD"].DefaultString() + " - " + dttFornecedor.Rows[0]["Fornecedor_Nome"].DefaultString();
                    this.lblEmitenteEndereco.Text = dttFornecedor.Rows[0]["Endereco_Completo"].DefaultString();
                    this.lblEmitenteBairro.Text = dttFornecedor.Rows[0]["Endereco_Bairro"].DefaultString();
                    this.lblEmitenteCEP.Text = dttFornecedor.Rows[0]["Endereco_CEP"].DefaultString();
                    this.lblEmitenteInscEstadual.Text = dttFornecedor.Rows[0]["Inscricao_Estadual"].DefaultString();
                    this.lblEmitenteMunicipio.Text = dttFornecedor.Rows[0]["Cidade_NM"].DefaultString();
                    this.lblEmitenteTelefone.Text = dttFornecedor.Rows[0]["Telefone_DDD"].DefaultString() + ' ' + dttFornecedor.Rows[0]["Telefone"].DefaultString();
                    this.lblEmitenteTipo.Text = dttFornecedor.Rows[0]["Enum_Tipo_Fornecedor_Descricao"].DefaultString();
                    this.lblEmitenteUFSigla.Text = dttFornecedor.Rows[0]["Estado_UF_Sigla"].DefaultString();
                    this.chkEmitenteOptanteSimples.Checked = dttFornecedor.Rows[0]["Forn_IsOptanteSimples"].DefaultBool();

                    this.intFornecedorID = dttFornecedor.Rows[0]["Forn_ID"].DefaultInteger();
                    this.strFornCD = dttFornecedor.Rows[0]["Forn_CD"].DefaultString();
                    this.strFornCNPJCPF = this.txtEmitenteCNPJCPF.Text.Trim();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Campos_Nota_Fiscal(DataTable dttNotaFiscal, int intIndice)
        {
            try
            {
                if (dttNotaFiscal.Rows.Count > 0)
                {
                    this.cboTipoRecebimento.SelectedValue = dttNotaFiscal.Rows[intIndice]["Enum_Tipo_ID"];

                    this.txtNotaFiscalNumeroControle.Text = dttNotaFiscal.Rows[intIndice]["Ordem_Desembarque_NF_Numero"].DefaultString();

                    this.lblNotaFiscalSerie.Text = dttNotaFiscal.Rows[intIndice]["Ordem_Desembarque_NF_Serie"].DefaultString();

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Campos_Nota_Fiscal_Totais(DataTable dttFornecedor, int intIndice)
        {
            try
            {
                if (dttFornecedor.Rows.Count > 0)
                {
                    this.mskValorTotalNotaFiscal.Text = dttFornecedor.Rows[intIndice]["Ordem_Desembarque_NF_Valor"].DefaultDecimal().ToString("#,##0.00");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        private void Habilitar_Campos_Dados_Nota_Fiscal(bool blnHabilitar)
        {
            try
            {
                if (this.intRecebimentoCTID == 0)
                {
                    this.cboTipoRecebimento.Enabled = blnHabilitar;
                    this.dtpNotaFiscalEmissao.Enabled = blnHabilitar;
                    this.dtpNotaFiscalSaida.Enabled = blnHabilitar;
                    this.txtNotaFiscalNumeroControle.Enabled = blnHabilitar;
                    this.cboNotaFiscalNumero.Enabled = blnHabilitar;
                    this.txtChaveAcesso.Enabled = blnHabilitar;
                    this.cboNaturezaOperacao.Enabled = blnHabilitar;
                    this.cboModelo.Enabled = blnHabilitar;
                    this.cboTipoOperacao.Enabled = blnHabilitar;
                    this.cboNaturezaFinanceira.Enabled = blnHabilitar;

                    this.grbInformacoesAdicionais.Enabled = blnHabilitar;
                    this.grbTotaisImpostos.Enabled = blnHabilitar;

                    this.txtCondicaoPagamento.Enabled = blnHabilitar;
                    this.btnPesquisarCondicaoPagamento.Enabled = blnHabilitar;
                    this.btnGerarParcelas.Enabled = blnHabilitar;
                    this.btnAtualizarParcela.Enabled = blnHabilitar;
                    this.btnCancelarParcela.Enabled = blnHabilitar;
                    this.txtParcela.Enabled = blnHabilitar;
                    this.dtpVencimentoParcela.Enabled = blnHabilitar;
                    this.mskValorParcela.Enabled = blnHabilitar;
                    this.dgvParcelas.ReadOnly = !blnHabilitar;

                    this.grbOrdemDesembarque.Enabled = true;
                    this.cboDestinatarioLoja.Enabled = true;
                    this.grbEmitente.Enabled = true;

                    this.btnDesbloqueioLojaRecebimento.Enabled = blnHabilitar;
                    this.btnDesbloquearParcela.Enabled = blnHabilitar;
                }
                else
                {
                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0
                        && (((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Liberado)
                        && this.lblDataEnvioInterface.Text != string.Empty)
                    {
                        this.btnDesbloquearParcela.Enabled = false;
                    }
                    else
                    {
                        this.btnDesbloquearParcela.Enabled = true;
                    }

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && (((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Liberacao_Parcial))
                    {
                        this.btnDesbloqueioLojaRecebimento.Enabled = false;
                    }
                    else
                    {
                        this.btnDesbloqueioLojaRecebimento.Enabled = true;
                    }

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && ((((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Aguardando_Processamento
                        || ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Pendente
                        || ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Pendente_Ordem_Chegada)
                        && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() == 0))
                    {
                        this.cboTipoRecebimento.Enabled = ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Aguardando_Processamento;
                        this.dtpNotaFiscalEmissao.Enabled = true;
                        this.txtNotaFiscalNumeroControle.Enabled = true;
                        this.cboNotaFiscalNumero.Enabled = true;
                        this.txtChaveAcesso.Enabled = true;
                        this.cboModelo.Enabled = true;
                        this.cboTipoOperacao.Enabled = true;

                        this.grbTotaisImpostos.Enabled = true;
                        this.grbInformacoesAdicionais.Enabled = true;

                    }
                    else if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && ((((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Aguardando_Processamento
                        || ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Pendente
                        || ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Pendente_Ordem_Chegada)
                        && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() != 0))
                    {
                        this.cboTipoRecebimento.Enabled = ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Aguardando_Processamento;
                        this.dtpNotaFiscalEmissao.Enabled = false;
                        this.txtNotaFiscalNumeroControle.Enabled = true;
                        this.cboNotaFiscalNumero.Enabled = false;
                        this.txtChaveAcesso.Enabled = false;
                        this.cboModelo.Enabled = true;
                        this.cboTipoOperacao.Enabled = true;

                        this.grbTotaisImpostos.Enabled = false;
                        this.grbInformacoesAdicionais.Enabled = false;

                    }
                    else
                    {
                        this.cboTipoRecebimento.Enabled = blnHabilitar;
                        this.dtpNotaFiscalEmissao.Enabled = blnHabilitar;
                        this.txtNotaFiscalNumeroControle.Enabled = blnHabilitar;
                        this.cboNotaFiscalNumero.Enabled = blnHabilitar;
                        this.txtChaveAcesso.Enabled = blnHabilitar;
                        this.cboModelo.Enabled = blnHabilitar;
                        this.cboTipoOperacao.Enabled = blnHabilitar;

                        this.grbTotaisImpostos.Enabled = blnHabilitar;
                        this.grbInformacoesAdicionais.Enabled = blnHabilitar;

                        this.txtCondicaoPagamento.Enabled = blnHabilitar;
                        this.btnPesquisarCondicaoPagamento.Enabled = blnHabilitar;
                    }

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && (((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Cancelado ||
                            ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Liberado))
                    {
                        this.cboNaturezaFinanceira.Enabled = false;
                        this.txtNotaFiscalNumeroControle.Enabled = false;
                        this.cboNaturezaOperacao.Enabled = false;

                        this.btnDesbloqueioLojaRecebimento.Enabled = false;
                        this.btnDesbloquearParcela.Enabled = false;
                    }
                    else
                    {
                        this.cboNaturezaFinanceira.Enabled = true;
                        this.txtNotaFiscalNumeroControle.Enabled = true;
                        this.cboNaturezaOperacao.Enabled = true;
                    }

                    this.grbOrdemDesembarque.Enabled = blnHabilitar;
                    this.cboDestinatarioLoja.Enabled = blnHabilitar;
                    this.grbEmitente.Enabled = blnHabilitar;

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && (((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) != Status_Recebimento.Aguardando_Processamento))
                    {
                        this.grbOrdemDesembarque.Enabled = false;
                        this.cboDestinatarioLoja.Enabled = false;
                        this.grbEmitente.Enabled = false;
                    }

                    if (this.dtsRecebimento.Tables.Count > 0
                            && this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Count == 0
                            && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0
                            && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() == 0)
                    {
                        blnHabilitar = true;
                    }

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() != 0 && this.dtsRecebimento.Tables["Parcelas"].Rows.Count > 0)
                    {
                        this.btnGerarParcelas.Enabled = false;
                    }
                    else
                    {
                        this.btnGerarParcelas.Enabled = blnHabilitar;
                    }
                    this.btnAtualizarParcela.Enabled = blnHabilitar;
                    this.btnCancelarParcela.Enabled = blnHabilitar;
                    this.txtParcela.Enabled = blnHabilitar;
                    this.dtpVencimentoParcela.Enabled = blnHabilitar;
                    this.mskValorParcela.Enabled = blnHabilitar;
                    this.dgvParcelas.ReadOnly = !blnHabilitar;

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0 && (!blnHabilitar && !(
                            ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Cancelado ||
                            ((Status_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"]) == Status_Recebimento.Liberado)))
                    {
                        if (this.dtsRecebimento.Tables["Parcelas"].Rows.Count == 0)
                        {
                            this.txtCondicaoPagamento.Enabled = true;
                            this.btnPesquisarCondicaoPagamento.Enabled = true;
                            this.btnGerarParcelas.Enabled = true;
                            this.btnAtualizarParcela.Enabled = true;
                            this.btnCancelarParcela.Enabled = true;
                            this.txtParcela.Enabled = true;
                            this.dtpVencimentoParcela.Enabled = true;
                            this.mskValorParcela.Enabled = true;
                            this.dgvParcelas.Enabled = true;
                            this.dgvParcelas.ReadOnly = false;
                        }
                    }

                }

                if (this.dtsRecebimento.Tables.Count > 0
                        && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0
                        && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() != 0
                    )
                {
                    this.btnVisualizarDanfe.Enabled = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultString() == string.Empty ? false : true;
                }
                else
                {
                    this.btnVisualizarDanfe.Enabled = false;
                }


                this.cboLojaRecebimento.Enabled = false;

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Configurar_Tela_Revenda_Encomenda_Garantia(bool blnPadraoRevenda)
        {
            try
            {
                this.dgvNotaFiscalItens.Grid01.Columns["Pedido_Compra_IT_Sequencia"].Visible = true;
                this.dgvNotaFiscalItens.Grid01.Columns["Pedido_Compra_IT_Custo_Compra"].Visible = true;

                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Enumerados.Tipo_Recebimento.Encomenda_Pedido.ToInteger() && !blnPadraoRevenda)
                {
                    this.tbcHerdado.TabPages.Remove(this.tbpPreRecebimento);
                    this.btnGerarPreRecebimento.Visible = false;

                    this.dgvNotaFiscalItens.Grid00.Columns["Recebimento_IT_Qtde_Pre_Gerado"].Visible = false;
                }
                else if ((this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Enumerados.Tipo_Recebimento.Revenda_Pedido.ToInteger()) || blnPadraoRevenda)
                {
                    if (!this.tbcHerdado.TabPages.Contains(this.tbpPreRecebimento))
                    {
                        this.tbcHerdado.TabPages.Add(this.tbpPreRecebimento);
                        this.tbcHerdado.TabPages[this.tbpPreRecebimento.Name].Enabled = true;
                    }
                    if (this.dgvParcelas.Rows.Count != 0)
                    {
                        this.txtCondicaoPagamento.Enabled = false;
                        this.btnPesquisarCondicaoPagamento.Enabled = false;
                    }
                    this.btnGerarPreRecebimento.Visible = true;
                    this.dgvNotaFiscalItens.Grid00.Columns["Recebimento_IT_Qtde_Pre_Gerado"].Visible = true;
                }
                else if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Enumerados.Tipo_Recebimento.Garantia_Pedido.ToInteger())
                {
                    if (!this.tbcHerdado.TabPages.Contains(this.tbpPreRecebimento))
                    {
                        this.tbcHerdado.TabPages.Add(this.tbpPreRecebimento);
                    }

                    this.txtCondicaoPagamento.Enabled = true;
                    this.btnPesquisarCondicaoPagamento.Enabled = true;

                    this.btnGerarPreRecebimento.Visible = true;
                    this.dgvNotaFiscalItens.Grid00.Columns["Recebimento_IT_Qtde_Pre_Gerado"].Visible = true;

                    this.dgvNotaFiscalItens.Grid01.Columns["Pedido_Compra_IT_Sequencia"].Visible = false;
                    this.dgvNotaFiscalItens.Grid01.Columns["Pedido_Compra_IT_Custo_Compra"].Visible = false;
                }
                else if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Enumerados.Tipo_Recebimento.Bonificacao_Pedido.ToInteger())
                {
                    this.txtCondicaoPagamento.Enabled = true;
                    this.btnPesquisarCondicaoPagamento.Enabled = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inicializar_Objeto_Numero_Nota_Fiscal(string strNumeroNota)
        {
            try
            {
                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                this.Carregar_Combo_Nota_Fiscal(busRecebimentoNotaFiscal.Preencher_DataTable_Numero_Nota_Fiscal(strNumeroNota));
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Verificar_Processar_Tipo_Recebimento()
        {
            try
            {
                this.btnProcessar.Enabled = false;
                this.chkMarcarTodosPedidos.Enabled = false;

                if (this.dtsRecebimento.Tables.Count > 0 &&
                    this.Verificar_Existencia_Itens_Para_Processar() &&
                    this.cboTipoRecebimento.SelectedValue != null &&
                    ((Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue == Tipo_Recebimento.Consumo_Pedido
                    || (Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue == Tipo_Recebimento.Garantia_Pedido
                    || (Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue == Tipo_Recebimento.Revenda_Pedido
                    || (Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue == Tipo_Recebimento.Encomenda_Pedido
                    || (Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue == Tipo_Recebimento.Bonificacao_Pedido))
                {
                    this.btnProcessar.Enabled = true;
                    this.chkMarcarTodosPedidos.Enabled = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Processar_Geracao_Parcelas()
        {
            try
            {
                if (this.dtoCondicaoPagamento == null || this.dtoCondicaoPagamento.Forma_Faturamento == null)
                {
                    return;
                }

                if (this.mskValorTotalNotaFiscal.Text.DefaultDecimal() == 0)
                {
                    MessageBox.Show("Para gerar as parcelas é necessário que o valor da nota fiscal seja preenchido!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskValorTotalNotaFiscal.Focus();
                    return;
                }

                decimal dcmValorNota = ((Information.IsNumeric(this.mskValorTotalNotaFiscal.Text) ? this.mskValorTotalNotaFiscal.Text : "0")).DefaultDecimal();

                this.dgvParcelas.DataSource = null;

                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                this.dgvParcelas.DataSource = busRecebimentoNotaFiscal.Gerar_DataTable_Parcelas(this.dtoCondicaoPagamento, dcmValorNota, this.dtpNotaFiscalEmissao.Value.DefaultDateTime());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private decimal Calcular_Valor_Base_Calculo_ICMS()
        {
            try
            {
                decimal dcmValor = 0;
                dcmValor += this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Base_ICMS_Isento"].DefaultDecimal();
                dcmValor += this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Base_ICMS_07"].DefaultDecimal();
                dcmValor += this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Base_ICMS_12"].DefaultDecimal();
                dcmValor += this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Base_ICMS_18"].DefaultDecimal();
                dcmValor += this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Recebimento_CT_Valor_Base_ICMS_25"].DefaultDecimal();

                return dcmValor;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Validar_CNPJ_CPF()
        {
            try
            {
                this.txtEmitenteCNPJCPF.Text = Utilitario.Remover_Caracteres_CNPJ_CPF(this.txtEmitenteCNPJCPF.Text.Trim());

                if (!Utilitario.Validar_Formato_CNPJ_CPF(this.txtEmitenteCNPJCPF.Text))
                {
                    if (this.txtEmitenteCNPJCPF.Text.Length <= 11)
                    {
                        MessageBox.Show("CPF inválido!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("CNPJ inválido!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    this.txtEmitenteCNPJCPF.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Carregar_Fornecedor()
        {
            try
            {
                DataTable dttFornecedor = new DataTable();
                if (this.txtEmitenteCNPJCPF.Text == string.Empty)
                {
                    dttFornecedor = this.Retornar_Consulta_Fornecedor();
                }

                if (this.txtEmitenteCNPJCPF.Text != string.Empty)
                {
                    dttFornecedor = this.Buscar_Fornecedor(0, Utilitario.Remover_Caracteres_CNPJ_CPF(this.txtEmitenteCNPJCPF.Text.Trim()));
                    // Caso o fornecedor não seja encontrado pelo CNPJ/CPF então é aberta a tela para fazer a busca
                    if (dttFornecedor.Rows.Count == 0)
                    {
                        MessageBox.Show("Fornecedor não localizado!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dttFornecedor = this.Retornar_Consulta_Fornecedor();
                    }
                }

                if (dttFornecedor == null)
                {
                    return false;
                }

                if (dttFornecedor.Rows.Count == 0)
                {
                    MessageBox.Show("Fornecedor não localizado!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtEmitenteCNPJCPF.Focus();
                    return false;
                }

                this.Preencher_Campos_Emitente(dttFornecedor);
                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private DataTable Buscar_Fornecedor(int intFornecedorID, string strCNPJCPF)
        {
            try
            {
                FornecedorBUS busFornecedor = new FornecedorBUS();
                DataTable dttFornecedor = busFornecedor.Consultar_Dados_Fornecedor_NFe(intFornecedorID, strCNPJCPF, Root.AcessoDoServidor.ServidorCentral);
                return dttFornecedor;
            }
            catch (Exception)
            {

                throw;
            }

        }

        private DataTable Retornar_Consulta_Fornecedor()
        {
            try
            {

                frmPesquisaGrid frmPesquisa = new frmPesquisaGrid("Nome", "Fornecedores");
                frmPesquisa.Grid.Adicionar_Coluna("Forn_CD", "Codigo", 50, false);
                frmPesquisa.Grid.Adicionar_Coluna("CNPJ_CPF", "CNPJ/CPF", 100, false);
                frmPesquisa.Grid.Adicionar_Coluna("Nome_Fornecedor", "Nome", 200, true);

                FornecedorBUS busFornecedor = new FornecedorBUS();
                frmPesquisa.Carregar_Grid(busFornecedor.Consultar_DataSet_Fornecedor_NF_Venda(string.Empty, string.Empty, Root.AcessoDoServidor.ServidorCentral).Tables[0]);
                if (frmPesquisa.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    DataRow dtrFornecedor = ((DataRowView)frmPesquisa.Registro.DataBoundItem).Row;
                    return busFornecedor.Consultar_Dados_Fornecedor_NFe(0, dtrFornecedor["CNPJ_CPF"].DefaultString(), Root.AcessoDoServidor.ServidorCentral);
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Condicao_Pagamento_CTDO Pesquisar_Condicao_Pagamento()
        {
            try
            {
                Condicao_Pagamento_CTDO dtoFunctionReturnValue = null;

                Condicao_Pagamento_CTBUS busCondicaoPagamento = new Condicao_Pagamento_CTBUS();
                ArrayList colConsulta = busCondicaoPagamento.Consultar_DataObject_Com_Atribuicoes();
                ArrayList colColunas = new ArrayList();

                colColunas.Add(new Componentes.ColumnStyle(true, "Codigo", "Código", 50));
                colColunas.Add(new Componentes.ColumnStyle(true, "Descricao", "Descrição", colConsulta.Count > 11 ? 418 : 434));

                if (colConsulta.Count == 0)
                {
                    MessageBox.Show("Não há condições de pagamento disponíveis para selecionar.", "Selecionar Condição de Pagamento", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return dtoFunctionReturnValue;
                }

                frmPesquisa frmPesq = new frmPesquisa(colConsulta, colColunas);

                if (frmPesq.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }

                return dtoFunctionReturnValue = (Condicao_Pagamento_CTDO)frmPesq.DataObject;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Habilitar_Campos_Parcelas(bool blnHabilitar)
        {
            try
            {
                this.txtParcela.Enabled = blnHabilitar;
                this.dtpVencimentoParcela.Enabled = blnHabilitar;
                this.mskValorParcela.Enabled = blnHabilitar;
                this.btnAtualizarParcela.Enabled = blnHabilitar;
                this.btnCancelarParcela.Enabled = blnHabilitar;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Habilitar_Campos_Alterar_Parcelas(bool blnHabilitar)
        {
            try
            {
                this.btnPesquisarCondicaoPagamento.Enabled = blnHabilitar;
                this.btnGerarParcelas.Enabled = blnHabilitar;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void Carregar_Campos_Parcelas()
        {
            try
            {
                if (this.dgvParcelas.Rows.Count > 0
                    && this.dgvParcelas.SelectedRows.Count > 0)
                {
                    this.txtParcela.Text = this.dgvParcelas.SelectedRows[0].Cells["Numero_Documento"].Value.DefaultString();
                    this.dtpVencimentoParcela.Value = this.dgvParcelas.SelectedRows[0].Cells["Data_Vencimento"].Value.DefaultDateTime();
                    this.mskValorParcela.Text = this.dgvParcelas.SelectedRows[0].Cells["Parcela_Valor"].Value.DefaultDecimal().DefaultString();

                    this.Habilitar_Campos_Parcelas(true);
                    this.Habilitar_Campos_Alterar_Parcelas(true);
                }
                else
                {
                    this.Habilitar_Campos_Parcelas(false);
                    this.Habilitar_Campos_Alterar_Parcelas(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Limpar_Campos_Parcelas()
        {
            try
            {
                this.txtParcela.Text = string.Empty;
                this.dtpVencimentoParcela.Value = new DateTime(1900, 1, 1);
                this.mskValorParcela.Text = "0,00";
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Atualizar_DataGrid_Parcelas()
        {
            try
            {
                if (this.dgvParcelas.Rows.Count > 0
                    && this.dgvParcelas.SelectedRows.Count > 0)
                {
                    if (!this.Validar_Atualizacao_Parcela())
                    {
                        return;
                    }

                    string strFiltroBusca = string.Empty;
                    if (!this.dgvParcelas.SelectedRows[0].Cells["Numero_Documento"].Value.DefaultString().IsNullOrEmpty())
                    {
                        strFiltroBusca = strFiltroBusca + "Numero_Documento = '" + this.dgvParcelas.SelectedRows[0].Cells["Numero_Documento"].Value.DefaultString() + "'";
                    }
                    if (!this.dgvParcelas.SelectedRows[0].Cells["Recebimento_Parcelas_Seq"].Value.DefaultString().IsNullOrEmpty())
                    {
                        if (strFiltroBusca != string.Empty)
                        {
                            strFiltroBusca = strFiltroBusca + " AND ";
                        }

                        strFiltroBusca = strFiltroBusca + " Recebimento_Parcelas_Seq = " + this.dgvParcelas.SelectedRows[0].Cells["Recebimento_Parcelas_Seq"].Value.DefaultString();
                    }

                    DataRow[] colParcelas = this.dtsRecebimento.Tables["Parcelas"].Select(strFiltroBusca);

                    foreach (DataRow dtrParcela in colParcelas)
                    {
                        dtrParcela["Numero_Documento"] = this.txtParcela.Text;
                        dtrParcela["Data_Vencimento"] = this.dtpVencimentoParcela.Value;
                        dtrParcela["Parcela_Valor"] = this.mskValorParcela.Text.DefaultDecimal();
                    }

                    this.dgvParcelas.DataSource = this.dtsRecebimento.Tables["Parcelas"];


                    this.Habilitar_Campos_Parcelas(false);
                    this.Limpar_Campos_Parcelas();

                    this.dgvParcelas.ClearSelection();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Atualizacao_Parcela()
        {
            try
            {
                if (this.txtParcela.Text.IsNullOrEmpty())
                {
                    MessageBox.Show("Preencha o campo da parcela", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtParcela.Focus();
                    return false;
                }

                if (this.dtpVencimentoParcela.Value < this.dtpNotaFiscalEmissao.Value)
                {
                    MessageBox.Show("A data de vencimento não pode ser menor que a data de emissão da nota fiscal", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpVencimentoParcela.Focus();
                    return false;
                }

                if (this.mskValorParcela.Text.DefaultDecimal() == 0)
                {
                    MessageBox.Show("Preencha o campo do valor da parcela", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskValorParcela.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Condicao_Pagamento()
        {
            try
            {
                Condicao_Pagamento_CTBUS busCondicaoPagamento = new Condicao_Pagamento_CTBUS();
                this.dtoCondicaoPagamento = busCondicaoPagamento.Selecionar_Por_Codigo(this.txtCondicaoPagamento.Text);

                if (this.dtoCondicaoPagamento == null)
                {
                    MessageBox.Show("Condição de pagamento inválida!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtoCondicaoPagamento = null;
                    this.txtCondicaoPagamento.Text = string.Empty;
                    this.lblCondicaoPagamento.Text = string.Empty;
                    this.txtCondicaoPagamento.Focus();
                    return;
                }
                else
                {
                    this.txtCondicaoPagamento.Text = this.dtoCondicaoPagamento.Codigo;
                    this.lblCondicaoPagamento.Text = this.dtoCondicaoPagamento.Descricao;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Localizar_Valor_Parametro_Diferenca_Nota_Fiscal(ref decimal dcmValorMenor, ref decimal dcmValorMaior)
        {
            try
            {
                Parametros_ProcessoBUS busParametrosProcesso = new Parametros_ProcessoBUS();
                dcmValorMenor = busParametrosProcesso.Consultar_Consultar_Parametro_Processo_Valor(this.cboDestinatarioLoja.SelectedValue.DefaultInteger(),
                                                                                                   "VALOR_DIFERENCA_VALORES_RECEBIMENTO_NOTA_MENOR", Root.AcessoDoServidor.ServidorCentral).Replace('.', ',').DefaultDecimal();

                dcmValorMaior = busParametrosProcesso.Consultar_Consultar_Parametro_Processo_Valor(this.cboDestinatarioLoja.SelectedValue.DefaultInteger(),
                                                                                                   "VALOR_DIFERENCA_VALORES_RECEBIMENTO_NOTA_MAIOR", Root.AcessoDoServidor.ServidorCentral).Replace('.', ',').DefaultDecimal();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private UsuarioDO Autenticar_Usuario()
        {
            try
            {
                frmLogin frmAutenticacao = new frmLogin(TipoLogin.Autorizacao);

                if (frmAutenticacao.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    return frmAutenticacao.Usuario;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Condicao_Pagamento_Tela_E_Calculado()
        {
            try
            {
                if (this.dgvParcelas.DataSource == null || this.dgvParcelas.Rows.Count == 0)
                {
                    return true;
                }
                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Revenda_Pedido.DefaultInteger())
                {
                    this.txtCondicaoPagamento.Enabled = false;
                    this.btnPesquisarCondicaoPagamento.Enabled = false;
                }

                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                    || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Bonificacao_Pedido.DefaultInteger())
                {
                    return true;
                }

                decimal dcmValorNota = ((Information.IsNumeric(this.mskValorTotalNotaFiscal.Text) ? this.mskValorTotalNotaFiscal.Text : "0")).DefaultDecimal();
                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                DataTable dttParcelasGeradas = busRecebimentoNotaFiscal.Gerar_DataTable_Parcelas(this.dtoCondicaoPagamento, dcmValorNota, this.dtpNotaFiscalEmissao.Value.DefaultDateTime());
                dttParcelasGeradas.DefaultView.Sort = "Data_Vencimento";

                if ( dttParcelasGeradas.Rows.Count > 0 || this.dtoCondicaoPagamento.ID == CONDICAO_PAGAMENTO_A_VISTA_ID )
                {
                    DataView dtvParcelasGrid = new DataView(((DataTable)this.dgvParcelas.DataSource));
                    dtvParcelasGrid.Sort = "Data_Vencimento";
                    DataTable dttParcelasGrid = this.Retornar_Parcelas_Da_Grid_De_Faturas(dtvParcelasGrid);

                    if (dttParcelasGeradas.Rows.Count != dttParcelasGrid.Rows.Count)
                    {
                        MessageBox.Show("A quantidade de parcelas está diferente das parcelas da condição de pagamento informada.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        return false;
                    }

                    for (int intParcela = 0; intParcela < dttParcelasGrid.Rows.Count; intParcela++)
                    {
                        if (dttParcelasGrid.Rows[intParcela]["Data_Vencimento"].DefaultDateTime() <
                            dttParcelasGeradas.Rows[intParcela]["Data_Vencimento"].DefaultDateTime().AddDays(-1))
                        {
                            MessageBox.Show("A data de vencimento das parcelas está com o prazo menor do que o esperado.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                            return false;
                        }
                    }
                }
                
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Retornar_Parcelas_Da_Grid_De_Faturas(DataView dtvParcelasGrid)
        {
            DataTable dttParcelasGrid = dtvParcelasGrid.ToTable(true, new string[] { "Data_Vencimento", "Recebimento_Parcelas_Seq", "Parcela_Valor" });
            DateTime dtmDataPrimeiraParcela = dttParcelasGrid.Compute("MIN(Data_Vencimento)", string.Empty).DefaultDateTime();

            foreach (DataRow dtrRegistro in dttParcelasGrid.Select(string.Format("Data_Vencimento = '{0}' And Parcela_Valor = {1}", dtmDataPrimeiraParcela, mskValorTotalICMSSubstituicao.Text.Replace(".", "").Replace(",", "."))))
            {
               dtrRegistro.Delete();
            }
 
            return dttParcelasGrid;
        }
        #endregion

        #region "   Aba Itens               "

        #region "   Incluir/Excluir Item    "

        private bool Incluir_Grid_Novo_Item()
        {
            try
            {

                if (this.Validar_Campo_Novo_Item() == false)
                {
                    return false;
                }

                this.Perder_Foco_Campo_Custo_Nota_Fiscal(null, null);

                this.Preencher_DataSet_Novo_Item();

                this.Atualizar_Valor_Total();

                this.Setar_Imagens_Grid();

                this.Verificar_Processar_Tipo_Recebimento();

                this.txtCodigoFabricante.Focus();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Excluir_Grid_Novo_Item()
        {
            try
            {
                DataRow[] colLinhaExcluir = this.dtsRecebimento.Tables["RecebimentoIT"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Recebimento_IT_ID"].Value.DefaultString());

                if (this.dttItensExcluidos.Columns.Contains("Recebimento_IT_ID") == false)
                {
                    this.dttItensExcluidos.TableName = "Recebimento_IT_Excluido";
                    this.dttItensExcluidos.Columns.Add("Recebimento_IT_ID", typeof(int));
                }

                foreach (DataRow dtrLinha in colLinhaExcluir)
                {
                    if (dtrLinha.RowState != DataRowState.Added)
                    {
                        DataRow dtrExcluido = this.dttItensExcluidos.Rows.Add();
                        dtrExcluido["Recebimento_IT_ID"] = dtrLinha["Recebimento_IT_ID"];
                    }
                    dtrLinha.Delete();
                }

                this.dtsRecebimento.AcceptChanges();
                this.Atualizar_Valor_Total();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Campo_Novo_Item()
        {
            try
            {
                if (this.txtCodigoFabricante.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o código do fabricante da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtCodigoFabricante.Focus();
                    return false;
                }

                if (this.txtPecaRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar a descrição da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtPecaRevenda.Focus();
                    return false;
                }

                if (this.txtQuantidadeTotalNotaFiscal.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar a quantidade da peça na nota.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtQuantidadeTotalNotaFiscal.Focus();
                    return false;
                }

                if (this.txtQuantidadeTotalNotaFiscal.Text.DefaultInteger() <= 0)
                {
                    MessageBox.Show("A quantidade não pode ser menor ou igual a zero.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtQuantidadeTotalNotaFiscal.Focus();
                    return false;
                }

                if (this.mskCustoNotaFiscal.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o valor unitário da peça na nota.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskCustoNotaFiscal.Focus();
                    return false;
                }

                if (this.lblValorTotalItemRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o valor unitário da peça na nota para que possa ser calculado o valor total dos itens.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskCustoNotaFiscal.Focus();
                    return false;
                }

                if (this.mskCustoNotaFiscal.Text.DefaultDecimal() <= 0)
                {
                    MessageBox.Show("O valor unitário não pode ser menor ou igual a zero.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskCustoNotaFiscal.Focus();
                    return false;
                }

                if (this.mskValorDescontoRevenda.Text.DefaultDecimal() > this.mskCustoNotaFiscal.Text.DefaultDecimal() * this.txtQuantidadeTotalNotaFiscal.Text.DefaultInteger())
                {
                    MessageBox.Show("O valor do desconto não pode ser maior que o valor total do item.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskValorDescontoRevenda.Focus();
                    return false;
                }

                if (this.txtPecaRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar a descrição da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtPecaRevenda.Focus();
                    return false;
                }

                if (this.mskICMSRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o percentual de ICMS da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskICMSRevenda.Focus();
                    return false;
                }
                else
                {

                    if (!Root.Parametros_Sistema.Verificar_Parametro_Sistema_Valor_Por_Tipo("ICMS", this.mskICMSRevenda.Text.Replace(",", "."), 0))
                    {
                        MessageBox.Show("Favor informar um percentual válido de ICMS da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.mskICMSRevenda.Focus();
                        return false;
                    }

                }


                if (this.mskSubstituicaoRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o percentual de ST da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskSubstituicaoRevenda.Focus();
                    return false;
                }

                if (this.mskIPIRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o percentual de IPI da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskIPIRevenda.Focus();
                    return false;
                }

                if (this.txtCodigoClassFiscal.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar a classificação fiscal da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtCodigoClassFiscal.Focus();
                    return false;
                }

                if (this.txtCST.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar a CST da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtCST.Focus();
                    return false;
                }

                if (this.mskValorDescontoRevenda.Text == string.Empty)
                {
                    MessageBox.Show("Favor informar o valor do desconto da peça.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.mskValorDescontoRevenda.Focus();
                    return false;
                }

                return this.Validar_Item_Ja_Inserido(this.intPecaID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Item_Ja_Inserido(int intPecaID)
        {
            try
            {
                foreach (DataGridViewRow dgrLinha in this.dgvNotaFiscalItens.Grid00.Rows)
                {
                    if (intPecaID != 0 && dgrLinha.Cells["Peca_ID"].Value.DefaultInteger() == intPecaID)
                    {
                        if (MessageBox.Show("Esta peça já foi inserida no recebimento. Deseja inserí-la novamente?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Consultar_Novo_Item(string strCodigoFabricante, string strDescricaoPeca)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Limpar_Campos_Itens_Detalhes();

                frmPesquisaGrid frmPesquisa = new frmPesquisaGrid("Código Fabricante", "Peça por código de fabricante");
                frmPesquisa.Grid.Adicionar_Coluna("Peca_CDFabricante", "Cód. Fab.", 95, false);
                frmPesquisa.Grid.Adicionar_Coluna("Peca_Codigo_Fornecedor_Codigo", "Cód. Forn.", 95, false);
                frmPesquisa.Grid.Adicionar_Coluna("Fornecedor_Nome", "Fornecedor", 110, false);
                frmPesquisa.Grid.Adicionar_Coluna("Peca_DSTecnica", "Descrição", 200, true);
                frmPesquisa.Grid.Adicionar_Coluna("Peca_CodBarra", "Código Barras", 100, false);
                frmPesquisa.Grid.Adicionar_Coluna("Peca_ID");
                frmPesquisa.Grid.Adicionar_Coluna("Peca_Gerar_Etiqueta_Automatica_Recebimento");

                PecaBUS busPeca = new PecaBUS();
                DataTable dttItens = busPeca.Consultar_DataTable_Peca_Item_Nao_Cadastrado(0, 0, string.Empty, strCodigoFabricante, string.Empty, strDescricaoPeca, this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger());

                this.txtCodigoFabricante.Text = strCodigoFabricante;

                if (dttItens.Rows.Count == 0)
                {
                    this.cboEmbalagemCompra.Enabled = false;

                    this.Carregar_Detalhe_Itens_Novo(null, strCodigoFabricante, strDescricaoPeca);
                }
                else
                {
                    frmPesquisa.Carregar_Grid(dttItens);

                    if (frmPesquisa.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        this.Carregar_Detalhe_Itens_Novo(((DataRowView)frmPesquisa.Registro.DataBoundItem).Row, strCodigoFabricante, strDescricaoPeca);

                        if (this.txtCST.Text.Length == 3)
                        {
                            this.Validar_Selecionar_CST();
                        }

                        this.txtPecaRevenda.Focus();

                        this.cboEmbalagemCompra.Enabled = true;

                    }
                    else
                    {
                        this.cboEmbalagemCompra.Enabled = false;
                        this.Carregar_Detalhe_Itens_Novo(null, strCodigoFabricante, strDescricaoPeca);
                    }
                }

                this.Carregar_Grupo();

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Carregar_Detalhe_Itens_Novo(DataRow dtrObjeto, string strCodigoFabricante, string strDescricaoPeca)
        {
            try
            {
                if (dtrObjeto == null)
                {
                    this.intPecaID = 0;
                    this.intClassificacaoFiscalNativaPecaID = 0;
                    this.strClassificacaoFiscalNativaPeca = string.Empty;
                    this.txtFabricanteCD.Text = string.Empty;
                    this.txtProdutoCD.Text = string.Empty;
                    this.txtPecaCD.Text = string.Empty;
                    this.txtPecaRevenda.Text = strDescricaoPeca;
                    this.lblProdutoRevenda.Text = string.Empty;
                    this.lblFabricanteRevenda.Text = string.Empty;
                    this.Carregar_Combo_Embalagens(0);
                    this.cboEmbalagemCompra.Enabled = false;
                    this.lblTipoEmbalagemRevenda.Text = string.Empty;
                    this.cboEmbalagemCompra.SelectedValue = string.Empty;
                    this.txtCodigoFabricante.Text = strCodigoFabricante;
                    this.mskICMSRevenda.Text = string.Empty;
                    this.mskSubstituicaoRevenda.Text = string.Empty;
                    this.mskIPIRevenda.Text = string.Empty;
                    this.txtCST.Text = string.Empty;
                    this.lblCustoEmbalagemRevenda.Text = 0.ToString("#,##0.00");
                    this.lblCustoUnitario.Text = 0.ToString("#,##0.00");
                    this.txtCodigoClassFiscal.Text = string.Empty;
                }
                else
                {
                    this.intPecaID = dtrObjeto["Peca_ID"].DefaultInteger();
                    this.intClassificacaoFiscalNativaPecaID = dtrObjeto["Peca_Classificacao_Fiscal_ID"].DefaultInteger();
                    this.strClassificacaoFiscalNativaPeca = dtrObjeto["Peca_Classificacao_Fiscal_CD"].DefaultString();
                    this.txtFabricanteCD.Text = dtrObjeto["Fabricante_CD"].DefaultString();
                    this.txtProdutoCD.Text = dtrObjeto["Produto_CD"].DefaultString();
                    this.txtPecaCD.Text = dtrObjeto["Peca_CD"].DefaultString();
                    this.txtPecaRevenda.Text = strDescricaoPeca.IsNullOrEmpty() ? dtrObjeto["Peca_DSTecnica"].DefaultString() : strDescricaoPeca;
                    this.lblProdutoRevenda.Text = dtrObjeto["Produto_DS"].DefaultString();
                    this.lblFabricanteRevenda.Text = dtrObjeto["Fabricante_NmFantasia"].DefaultString();
                    this.Carregar_Combo_Embalagens(dtrObjeto["Peca_ID"].DefaultInteger());
                    this.lblTipoEmbalagemRevenda.Text = dtrObjeto["Enum_Tipo_Embalagem_Sigla"].DefaultString();
                    this.cboEmbalagemCompra.SelectedValue = dtrObjeto["Peca_Embalagem_ID"];
                    this.txtCodigoFabricante.Text = strCodigoFabricante.IsNullOrEmpty() ? dtrObjeto["Peca_CDFabricante"].DefaultString() : strCodigoFabricante;
                    this.mskICMSRevenda.Text = dtrObjeto["Peca_ICMS_Compra"].DefaultString();
                    this.mskSubstituicaoRevenda.Text = dtrObjeto["Peca_ICMS_Substituicao_Tributaria"].DefaultString();
                    this.mskIPIRevenda.Text = dtrObjeto["Peca_Perc_IPI"].DefaultString();
                    this.txtCST.Text = dtrObjeto["Peca_Codigo_Situacao_Tributaria"].DefaultString();
                    this.lblCustoUnitario.Text = dtrObjeto["Peca_Custo"].DefaultString("#,##0.00");
                    this.lblCustoEmbalagemRevenda.Text = dtrObjeto["Peca_Custo"].DefaultString("#,##0.00");
                    this.txtCodigoClassFiscal.Text = dtrObjeto["Peca_Classificacao_Fiscal_CD"].DefaultString();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Detalhe_Itens_Novo(PecaDO dtoObjeto)
        {
            try
            {
                this.intPecaID = dtoObjeto.ID;
                this.intClassificacaoFiscalNativaPecaID = dtoObjeto.Classificacao_Fiscal.ID.DefaultInteger();
                this.strClassificacaoFiscalNativaPeca = dtoObjeto.Classificacao_Fiscal.Codigo.DefaultString();
                this.txtPecaRevenda.Text = dtoObjeto.Descricao_Tecnica.DefaultString();
                this.lblProdutoRevenda.Text = dtoObjeto.Produto.Descricao.DefaultString();
                this.lblFabricanteRevenda.Text = dtoObjeto.Fabricante.Nome_Fantasia.DefaultString();
                this.Carregar_Combo_Embalagens(dtoObjeto.ID);

                if (dtoObjeto.Embalagens.Count > 0)
                {
                    this.lblTipoEmbalagemRevenda.Text = dtoObjeto.Embalagens[0].Descricao.DefaultString();
                    this.cboEmbalagemCompra.SelectedValue = dtoObjeto.Embalagens[0].ID;
                    this.lblCustoUnitario.Text = (dtoObjeto.Estoque_Custo.Ultimo_Custo.DefaultDecimal() * dtoObjeto.Embalagens[0].Quantidade.DefaultInteger()).ToString("#,##0.00");
                }

                this.txtCodigoFabricante.Text = dtoObjeto.Codigo_Item_Fabricante.DefaultString();
                this.mskICMSRevenda.Text = dtoObjeto.ICMS_Compra.DefaultString();
                this.mskSubstituicaoRevenda.Text = dtoObjeto.ICMS_Substituicao.DefaultString();
                this.mskIPIRevenda.Text = dtoObjeto.IPI.DefaultString();
                this.txtCodigoClassFiscal.Text = dtoObjeto.Classificacao_Fiscal.Codigo.DefaultString();
                this.txtCST.Text = dtoObjeto.Codigo_Situacao_Tributaria.DefaultString();
                this.lblCustoEmbalagemRevenda.Text = dtoObjeto.Estoque_Custo.Ultimo_Custo.ToString("#,##0.00");
                this.lblCustoUnitario.Text = (dtoObjeto.Estoque_Custo.Ultimo_Custo.DefaultDecimal() * this.Retornar_Quantidade_Embalagem_Peca(dtoObjeto.ID, this.cboEmbalagemCompra.SelectedValue.DefaultInteger())).ToString("#,##0.00");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_DataSet_Novo_Item()
        {
            try
            {
                this.dgvNotaFiscalItens.Grid00.SelectionChanged -= this.Mudar_Indice_Selecionado_Lista_Itens;

                DataRow dtrItensRecebimento = this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Add();

                int intRecebimentoITSequencia = this.dtsRecebimento.Tables["RecebimentoIT"].Compute("MAX(Recebimento_IT_Sequencia)", string.Empty).DefaultInteger() + 1;
                int intRecebimentoITID = this.dtsRecebimento.Tables["RecebimentoIT"].Compute("MAX(Recebimento_IT_ID)", string.Empty).DefaultInteger() + 1;

                dtrItensRecebimento["Recebimento_IT_ID"] = intRecebimentoITID;
                dtrItensRecebimento["Recebimento_CT_ID"] = 0;
                dtrItensRecebimento["Pre_Recebimento_IT_ID"] = 0;
                dtrItensRecebimento["Pre_Recebimento_CT_ID"] = 0;
                dtrItensRecebimento["Recebimento_IT_Numero_Pedido"] = 0;
                dtrItensRecebimento["Recebimento_IT_Sequencia"] = intRecebimentoITSequencia;
                dtrItensRecebimento["Recebimento_IT_NF_CD_Fabricante"] = this.txtCodigoFabricante.Text;
                dtrItensRecebimento["Recebimento_IT_NF_Descricao"] = this.txtPecaRevenda.Text;
                dtrItensRecebimento["Fabricante_CD"] = this.txtFabricanteCD.Text;
                dtrItensRecebimento["Produto_CD"] = this.txtProdutoCD.Text;
                dtrItensRecebimento["Peca_CD"] = this.txtPecaCD.Text;
                dtrItensRecebimento["Peca_ID"] = this.intPecaID;
                PecaBUS busPeca = new PecaBUS();
                dtrItensRecebimento["Codigo_Mercadocar"] = busPeca.Consultar_Codigo_Mcar(dtrItensRecebimento["Peca_ID"].DefaultInteger(), Root.AcessoDoServidor.ServidorCentral);
                dtrItensRecebimento["Peca_Class_Fiscal_ID"] = this.intClassificacaoFiscalNativaPecaID;
                dtrItensRecebimento["Peca_Class_Fiscal_CD"] = this.strClassificacaoFiscalNativaPeca;
                dtrItensRecebimento["Fabricante_NmFantasia"] = this.lblFabricanteRevenda.Text;
                dtrItensRecebimento["Produto_DS"] = this.lblProdutoRevenda.Text;
                dtrItensRecebimento["Recebimento_IT_Tipo_Embalagem"] = this.lblTipoEmbalagemRevenda.Text;
                dtrItensRecebimento["Embalagem_Compra_ID"] = this.cboEmbalagemCompra.SelectedValue == null ? 0 : this.cboEmbalagemCompra.SelectedValue;
                dtrItensRecebimento["Recebimento_IT_Custo_Nota_Fiscal"] = this.mskCustoNotaFiscal.Text.DefaultDecimal().ToString("#,##0.0000");
                dtrItensRecebimento["Recebimento_IT_Qtde_Nota_Fiscal"] = this.txtQuantidadeTotalNotaFiscal.Text;
                dtrItensRecebimento["Recebimento_IT_Qtde_Embalagens"] = 0;
                dtrItensRecebimento["Recebimento_IT_Qtde_Pre_Gerado"] = 0;
                dtrItensRecebimento["Recebimento_IT_Qtde_Devolvida"] = 0;
                dtrItensRecebimento["Recebimento_IT_Qtde_Restante"] = dtrItensRecebimento["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() - dtrItensRecebimento["Recebimento_IT_Qtde_Embalagens"].DefaultInteger();
                dtrItensRecebimento["Recebimento_IT_CST"] = this.txtCST.Text;
                dtrItensRecebimento["Recebimento_IT_ICMS_Perc"] = this.mskICMSRevenda.Text.DefaultDecimal();
                dtrItensRecebimento["Recebimento_IT_Perc_IPI"] = this.mskIPIRevenda.Text.DefaultDecimal();
                dtrItensRecebimento["Recebimento_IT_ICMS_ST_Perc"] = this.mskSubstituicaoRevenda.Text.DefaultDecimal();
                dtrItensRecebimento["Recebimento_IT_Valor_Desconto"] = this.mskValorDescontoRevenda.Text.DefaultDecimal();
                dtrItensRecebimento["Recebimento_IT_CST"] = this.txtCST.Text;
                dtrItensRecebimento["Recebimento_IT_NCM"] = this.txtCodigoClassFiscal.Text;
                dtrItensRecebimento["Recebimento_IT_Observacao"] = this.txtObservacaoRevenda.Text;
                dtrItensRecebimento["Recebimento_IT_Custo_Embalagem"] = this.lblCustoEmbalagemRevenda.Text;
                dtrItensRecebimento["Recebimento_IT_Custo_Unitario"] = this.lblCustoUnitario.Text;
                dtrItensRecebimento["Recebimento_IT_Custo_Total"] = this.lblValorTotalItemRevenda.Text;
                dtrItensRecebimento["Enum_Status_Comparacao_Item_ID"] = this.intPecaID == 0 ? Status_Recebimento_Item.Sem_Cadastro.DefaultInteger() : Status_Recebimento_Item.Sem_Pedido.DefaultInteger();
                dtrItensRecebimento["Recebimento_IT_IPI_Valor"] = dtrItensRecebimento["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal() * dtrItensRecebimento["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() * (dtrItensRecebimento["Recebimento_IT_Perc_IPI"].DefaultDecimal() / 100);
                dtrItensRecebimento["Recebimento_IT_Valor_Substituicao"] = dtrItensRecebimento["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal() * dtrItensRecebimento["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() * (dtrItensRecebimento["Recebimento_IT_ICMS_ST_Perc"].DefaultDecimal() / 100);
                dtrItensRecebimento["Recebimento_IT_ICMS_ST_Valor"] = dtrItensRecebimento["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal() * dtrItensRecebimento["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() * (dtrItensRecebimento["Recebimento_IT_ICMS_ST_Perc"].DefaultDecimal() / 100);

                dtrItensRecebimento["Pre_Recebimento_IT_Imprimir_Etiqueta"] = this.chkImprimirCodigoBarras.Checked;

                dtrItensRecebimento["Processado"] = true;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                this.dgvNotaFiscalItens.Grid00.SelectionChanged += this.Mudar_Indice_Selecionado_Lista_Itens;
            }
        }

        #endregion

        #region "   Habilitar               "

        private void Habilitar_Legenda(bool blnHabilitar)
        {
            try
            {
                this.grbLegenda.Visible = blnHabilitar;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Visualizar_BotaoGerarPreRecimento()
        {
            if (this.tbcHerdado.SelectedTab == this.tbpDadosNotaFiscal || this.tbcHerdado.SelectedTab == this.tbpCompras )
            {
                this.btnGerarPreRecebimento.Visible = true; 
            }
            else
            {
                this.btnGerarPreRecebimento.Visible = false;
            }
        }

        private void Visualizar_Botao_Lote_Devolucao()
        {
            if (this.tbcHerdado.SelectedTab == this.tbpCompras)
            {
                this.btnLoteDevolucao.Visible = true;
            }
            else
            {
                this.btnLoteDevolucao.Visible = false;
            }    
        }


        private bool Habilitar_Menu_Desfazer()
        {
            try
            {

                if ((this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido.DefaultInteger() ||
                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido_Governo.DefaultInteger() ||
                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale.DefaultInteger() ||
                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale_Governo.DefaultInteger())
                    && this.dtsRecebimento.Tables.Count > 0
                    && this.dtsRecebimento.Tables["RecebimentoCT"].Rows.Count > 0
                    && this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"].DefaultInteger() == Status_Recebimento.Liberado.DefaultInteger())
                {
                    return false;
                }

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows.Count == 0)
                {
                    return false;
                }

                if (this.dgvNotaFiscalItens.Grid00.SelectedRows[0].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Pedido.DefaultInteger())
                {
                    return false;
                }

                for (int indice = 0; this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > indice; indice++)
                {

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[indice].Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger() != 0)
                    {
                        return false;
                    }

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[indice].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger() != 0
                        && this.dgvNotaFiscalItens.Grid00.SelectedRows[indice].Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger() > 0)
                    {
                        return false;
                    }

                    if ((this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido.DefaultInteger() |
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido_Governo.DefaultInteger() |
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale.DefaultInteger() |
                        this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Vale_Governo.DefaultInteger()) &
                        (this.dgvNotaFiscalItens.Grid00.SelectedRows[indice].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Correto.DefaultInteger() |
                        this.dgvNotaFiscalItens.Grid00.SelectedRows[indice].Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Diferenca.DefaultInteger()) &
                        this.dgvNotaFiscalItens.Grid00.SelectedRows[indice].Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger() == 0)
                    {
                        return true;
                    }
                }

                foreach (DataGridViewRow dgrLinha in this.dgvPedidoCompras.Rows)
                {
                    if (dgrLinha.Cells["Marcado"].Value.DefaultBool())
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Preencher               "

        private void Preencher_Objeto_Recebimento_Itens()
        {
            try
            {
                if (this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count > 0)
                {
                    this.dtoPropriedades.Items.Clear();

                    foreach (DataRow dtrItem in this.dtsRecebimento.Tables["RecebimentoIT"].Select("Processado = " + true))
                    {
                        Recebimento_ITDO dtoRecebimentoIT = new Recebimento_ITDO();


                        dtoRecebimentoIT.ID = dtrItem["Recebimento_IT_ID"].DefaultInteger();

                        dtoRecebimentoIT.ID_Recebimento = dtrItem["Recebimento_CT_ID"].DefaultInteger();
                        dtoRecebimentoIT.Sequencia = dtrItem["Recebimento_IT_Sequencia"].DefaultInteger();
                        dtoRecebimentoIT.ID_Peca = dtrItem["Peca_ID"].DefaultInteger();
                        dtoRecebimentoIT.NF_CD_Fabricante = dtrItem["Recebimento_IT_NF_CD_Fabricante"].DefaultString();
                        dtoRecebimentoIT.NF_Descricao = dtrItem["Recebimento_IT_NF_Descricao"].DefaultString();
                        dtoRecebimentoIT.CST = dtrItem["Recebimento_IT_CST"].DefaultString();
                        dtoRecebimentoIT.NCM = dtrItem["Recebimento_IT_NCM"].DefaultString();
                        dtoRecebimentoIT.CFOP = dtrItem["Recebimento_IT_CFOP"].DefaultString();
                        dtoRecebimentoIT.ID_Embalagem_Compra = dtrItem["Embalagem_Compra_ID"].DefaultInteger();
                        dtoRecebimentoIT.Tipo_Embalagem = dtrItem["Recebimento_IT_Tipo_Embalagem"].DefaultString();
                        dtoRecebimentoIT.Quantidade_Embalagens = dtrItem["Recebimento_IT_Qtde_Embalagens"].DefaultInteger();
                        dtoRecebimentoIT.Quantidade_Nota_Fiscal = dtrItem["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
                        dtoRecebimentoIT.Quantidade_Total = dtrItem["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
                        dtoRecebimentoIT.Percentual_ICMS = dtrItem["Recebimento_IT_ICMS_Perc"].DefaultDecimal();
                        dtoRecebimentoIT.Percentual_IPI = dtrItem["Recebimento_IT_Perc_IPI"].DefaultDecimal();
                        dtoRecebimentoIT.ICMS_ST_Perc = dtrItem["Recebimento_IT_ICMS_ST_Perc"].DefaultDecimal();
                        dtoRecebimentoIT.Valor_Desconto = dtrItem["Recebimento_IT_Valor_Desconto"].DefaultDecimal();
                        dtoRecebimentoIT.IPI_Valor = dtrItem["Recebimento_IT_IPI_Valor"].DefaultDecimal();
                        dtoRecebimentoIT.ICMS_ST_Valor = dtrItem["Recebimento_IT_Valor_Substituicao"].DefaultDecimal();
                        dtoRecebimentoIT.Custo_Unitario = dtrItem["Recebimento_IT_Custo_Unitario"].DefaultDecimal();
                        dtoRecebimentoIT.Custo_Embalagem = dtrItem["Recebimento_IT_Custo_Embalagem"].DefaultDecimal();
                        dtoRecebimentoIT.Custo_Nota_Fiscal = dtrItem["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal();
                        dtoRecebimentoIT.Custo_Efetivo = dtrItem["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal();
                        dtoRecebimentoIT.Custo_Total = dtrItem["Recebimento_IT_Custo_Total"].DefaultDecimal();
                        dtoRecebimentoIT.Observacao = dtrItem["Recebimento_IT_Observacao"].DefaultString();
                        dtoRecebimentoIT.Imprimir_Etiqueta = dtrItem["Pre_Recebimento_IT_Imprimir_Etiqueta"].DefaultBool();
                        dtoRecebimentoIT.Status_Recebimento_Item = (Status_Recebimento_Item)dtrItem["Enum_Status_Comparacao_Item_ID"].DefaultInteger();
                        DBUtil objUtil = new DBUtil();
                        dtoRecebimentoIT.Data_Liberacao = (Status_Recebimento_Item)dtrItem["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Correto ? objUtil.Obter_Data_do_Servidor(true, TipoServidor.Central) : new DateTime(1900, 1, 1);
                        dtoRecebimentoIT.ID_Usuario_Liberacao = (Status_Recebimento_Item)dtrItem["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Correto ? ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID.DefaultInteger() : 0;
                        dtrItem["Recebimento_IT_Data_Liberacao"] = (Status_Recebimento_Item)dtrItem["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Correto ? objUtil.Obter_Data_do_Servidor(true, TipoServidor.Central) : new DateTime(1900, 1, 1);
                        dtrItem["Usuario_Liberacao_ID"] = (Status_Recebimento_Item)dtrItem["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Correto ? ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID.DefaultInteger() : 0;

                        dtoRecebimentoIT.Quantidade_Pre_Gerado = dtrItem["Recebimento_IT_Qtde_Pre_Gerado"].DefaultInteger();
                        dtoRecebimentoIT.Quantidade_Devolvida = dtrItem["Recebimento_IT_Qtde_Devolvida"].DefaultInteger();

                        dtoRecebimentoIT.IPI_Base = dtrItem["Recebimento_IT_IPI_Base"].DefaultDecimal();
                        dtoRecebimentoIT.PIS_Base = dtrItem["Recebimento_IT_PIS_Base"].DefaultDecimal();
                        dtoRecebimentoIT.PIS_Perc = dtrItem["Recebimento_IT_PIS_Perc"].DefaultDecimal();
                        dtoRecebimentoIT.PIS_Valor = dtrItem["Recebimento_IT_PIS_Valor"].DefaultDecimal();
                        dtoRecebimentoIT.COFINS_Base = dtrItem["Recebimento_IT_COFINS_Base"].DefaultDecimal();
                        dtoRecebimentoIT.COFINS_Perc = dtrItem["Recebimento_IT_COFINS_Perc"].DefaultDecimal();
                        dtoRecebimentoIT.COFINS_Valor = dtrItem["Recebimento_IT_COFINS_Valor"].DefaultDecimal();
                        dtoRecebimentoIT.ICMS_Base = dtrItem["Recebimento_IT_ICMS_Base"].DefaultDecimal();
                        dtoRecebimentoIT.ICMS_Valor = dtrItem["Recebimento_IT_ICMS_Valor"].DefaultDecimal();
                        dtoRecebimentoIT.ICMS_ST_MVA = dtrItem["Recebimento_IT_ICMS_ST_MVA"].DefaultDecimal();
                        dtoRecebimentoIT.ICMS_ST_Base = dtrItem["Recebimento_IT_ICMS_ST_Base"].DefaultDecimal();
                        dtoRecebimentoIT.Enum_Origem_Mercadoria_ID = dtrItem["Enum_Origem_Mercadoria_ID"].DefaultInteger();
                        dtoRecebimentoIT.Numero_Pedido = dtrItem["Recebimento_IT_Numero_Pedido"].DefaultString();
                        dtoRecebimentoIT.Compoe_Total = dtrItem["Recebimento_IT_Compoe_Total"].DefaultBool();
                        dtoRecebimentoIT.ICMS_Perc_Reducao = dtrItem["Recebimento_IT_ICMS_Perc_Reducao"].DefaultDecimal();

                        dtoRecebimentoIT.Tipo_Pre_Recebimento = (Tipo_Pre_Recebimento)this.Retornar_ID_Enumerado_Tipo_Recebimento((Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue.DefaultInteger());

                        if (dtrItem["Peca_ID"].DefaultInteger() > 0)
                        {
                            PecaBUS busPeca = new PecaBUS();
                            PecaDO dtoPeca = busPeca.Selecionar(dtrItem["Peca_ID"].DefaultInteger());

                            dtoRecebimentoIT.Percentual_Substituicao = dtoPeca.ICMS_Substituicao;
                        }

                        this.dtoPropriedades.Items.Add(dtoRecebimentoIT);
                    }

                    

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Aba_Itens()
        {
            try
            {
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Status_Imagem", typeof(Image));
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Status_Recebimento_Nota_Item");
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Processado");
                this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Add("Encomenda_Peca_Sem_Cadastro");

                this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Columns.Add("Status_Recebimento_Pedido_Item");
                this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Columns.Add("Processado");

                this.dgvNotaFiscalItens.Grid00.DataSource = this.dtsRecebimento.Tables["RecebimentoIT"];
                this.dgvNotaFiscalItens.Grid01.DataSource = this.dtsRecebimento.Tables["Nota_Pedido_Itens"];

                this.Preencher_Status_Diferenca();
                this.Calcular_Totais_Itens();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Abrir_Propriedades_Lote_Garantia_Ou_Devolucao()
        {
            try
            {
                if (this.dgvNotaFiscalItens.Grid00.CurrentRow == null)
                    return;


                frmGarantia_Lote_Propriedades frmPropriedades = new frmGarantia_Lote_Propriedades(Convert.ToInt32(this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Pedido_Garantia_CT_ID"].Value), 
                                                                                                  Convert.ToInt32(this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Pedido_Garantia_IT_Lojas_ID"].Value), false);
                
                frmPropriedades.Show(this);

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Preencher_Status_Diferenca()
        {
            try
            {
                if (this.dtsRecebimento.Tables.Count == 0)
                {
                    return;
                }

                decimal dcmPercentualAceitavelDiferencaValorMaior = Root.ParametrosProcesso.Retorna_Valor_Parametro("RECEBIMENTO", "PERCENTUAL_DIFERENCA_VALOR_A_MAIOR").DefaultDecimal() / 100;
                decimal dcmPercentualAceitavelDiferencaValorMenor = Root.ParametrosProcesso.Retorna_Valor_Parametro("RECEBIMENTO", "PERCENTUAL_DIFERENCA_VALOR_A_MENOR").DefaultDecimal() / 100;

                foreach (DataRow dtrNotaItens in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                {
                    foreach (DataRow dtrNotaPedidoItens in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows)
                    {
                        if (dtrNotaItens["Peca_ID"].DefaultInteger() != dtrNotaPedidoItens["Peca_ID"].DefaultInteger())
                        {
                            continue;
                        }
                        if (dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() != Status_Recebimento_Item.Diferenca.ToInteger())
                        {
                            continue;
                        }

                        decimal dcmValorUnitarioNotaFiscal = dtrNotaItens["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal();

                        if (dtrNotaItens["Recebimento_IT_Valor_Desconto"].DefaultDecimal() > 0 && dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() > 0)
                        {
                            dcmValorUnitarioNotaFiscal -= dtrNotaItens["Recebimento_IT_Valor_Desconto"].DefaultDecimal() / dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
                        }

                        if (dtrNotaItens["Recebimento_IT_IPI_Valor"].DefaultDecimal() > 0 && dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() > 0)
                        {
                            dcmValorUnitarioNotaFiscal += dtrNotaItens["Recebimento_IT_IPI_Valor"].DefaultDecimal() / dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
                        }

                        if (dtrNotaItens["Recebimento_IT_Valor_Substituicao"].DefaultDecimal() > 0 && dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() > 0)
                        {
                            dcmValorUnitarioNotaFiscal += dtrNotaItens["Recebimento_IT_Valor_Substituicao"].DefaultDecimal() / dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
                        }

                        if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                        && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger()
                        && (dcmValorUnitarioNotaFiscal < (dtrNotaPedidoItens["Pedido_Compra_IT_Custo_Compra"].DefaultDecimal() * (1 - dcmPercentualAceitavelDiferencaValorMenor))
                                  || dcmValorUnitarioNotaFiscal > (dtrNotaPedidoItens["Pedido_Compra_IT_Custo_Compra"].DefaultDecimal() * (1 + dcmPercentualAceitavelDiferencaValorMaior))))
                        {
                            dtrNotaPedidoItens["Status_Recebimento_Pedido_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Valor.DefaultInteger();
                            dtrNotaItens["Status_Recebimento_Nota_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Valor.DefaultInteger();
                        }
                        else if (dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() != dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger())
                        {
                            dtrNotaPedidoItens["Status_Recebimento_Pedido_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Quantidade.DefaultInteger();
                            dtrNotaItens["Status_Recebimento_Nota_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Quantidade.DefaultInteger();

                        }
                    }
                }

                this.Mudar_Cor_Grid_Itens_Diferenca();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        private void Visualizar_Itens()
        {
            try
            {
                BindingSource bdsItensPedidoCompraNota = new BindingSource();

                bdsItensPedidoCompraNota.DataSource = this.dtsRecebimento.Tables["RecebimentoIT"];

                if (this.chkVisualizarItensCorretos.Checked == this.chkVisualizarItensDivergentes.Checked)
                {
                    bdsItensPedidoCompraNota.RemoveFilter();
                }
                else if (this.chkVisualizarItensCorretos.Checked)
                {
                    bdsItensPedidoCompraNota.Filter = "Enum_Status_Comparacao_Item_ID = " + Status_Recebimento_Item.Correto.ToInteger();
                }
                else if (this.chkVisualizarItensDivergentes.Checked)
                {
                    bdsItensPedidoCompraNota.Filter = "Enum_Status_Comparacao_Item_ID <> " + Status_Recebimento_Item.Correto.ToInteger();
                }

                this.dgvNotaFiscalItens.Grid00.DataSource = bdsItensPedidoCompraNota;

                this.Mudar_Cor_Grid_Itens_Diferenca();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Verificar_Processamento_Automatico()
        {
            try
            {
                if (this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count == 0)
                {
                    return false;
                }

                if (this.dgvPedidoCompras.Rows.Count == 0)
                {
                    return false;
                }

                foreach (DataRow dgvPedidos in ((DataTable)this.dgvPedidoCompras.DataSource).Rows)
                {
                    if (dgvPedidos["Marcado"].ToBool())
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region "   Processar               "

        /// <summary>
        /// Prcessamento entre Pedido de compra e Nota
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	01/08/2014  Created
        /// </history>
        private void Processar_Comparacao_Pedido_Nota_Fiscal()
        {
            try
            {
                // Obtem todos os pedidos selecionadados
                DataTable dttPedidos = this.Obter_DataTable_Do_Filtro_Selecionado(this.dgvPedidoCompras);

                // Obtem os itens dos pedidos pendentes e uma listagem de codigo de fornecedor alternativo
                Pre_RecebimentoBUS_NEW busPreRecebimentoNew = new Pre_RecebimentoBUS_NEW();

                DataSet dtsPedidosItem = busPreRecebimentoNew.Consultar_Itens_Pedido_Compra_Pendentes_Por_Fornecedor(
                                    this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].ToInteger(),
                                    this.cboLojaRecebimento.SelectedValue.ToInteger(),
                                    this.lblRecebimentoID.Text.DefaultInteger(),
                                    this.cboTipoRecebimento.SelectedValue.DefaultInteger(),
                                    dttPedidos);

                bool blnDiferenca = false;
                bool blnDiferencaValor = false;
                bool blnComPedido = false;

                foreach (DataRow dtrNotaItens in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                {
                    if ((Status_Recebimento_Item)dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Correto
                            || (Status_Recebimento_Item)dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Sem_Cadastro
                            || (Status_Recebimento_Item)dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Devolvido)
                    {
                        continue;
                    }

                    blnDiferenca = false;
                    blnComPedido = false;

                    foreach (DataRow dtrPedidoItens in dtsPedidosItem.Tables["Pedidos"].Rows)
                    {
                        // Identifica a Peça e se ainda existe quantidade pendente
                        if (dtrNotaItens["Peca_ID"].DefaultInteger() == dtrPedidoItens["Peca_ID"].DefaultInteger()
                                  && (dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger() > 0))
                        {
                            DataRow dtrNotaPedidoItens = this.Obter_Pedido_Item(dtrPedidoItens, dtrNotaItens);

                            if (dtrNotaPedidoItens == null) continue;

                            blnComPedido = true;
                            blnDiferencaValor = false;

                            dtrNotaPedidoItens["Lojas_ID"] = this.cboDestinatarioLoja.SelectedValue.DefaultInteger();
                            dtrNotaPedidoItens["Recebimento_CT_ID"] = dtrNotaItens["Recebimento_CT_ID"];
                            dtrNotaPedidoItens["Pedido_Compra_CT_ID"] = dtrPedidoItens["Pedido_Compra_CT_ID"];
                            dtrNotaPedidoItens["Pedido_Compra_IT_ID"] = dtrPedidoItens["Pedido_Compra_IT_ID"];
                            dtrNotaPedidoItens["Recebimento_IT_ID"] = dtrNotaItens["Recebimento_IT_ID"];
                            dtrNotaPedidoItens["Peca_ID"] = dtrNotaItens["Peca_ID"];
                            dtrNotaPedidoItens["Usuario_Inclusao_ID"] = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID;
                            dtrNotaPedidoItens["Pedido_Compra_IT_Sequencia"] = dtrPedidoItens["Pedido_Compra_IT_Sequencia"];
                            dtrNotaPedidoItens["NF_CD_Fabricante"] = dtrPedidoItens["Peca_CDFabricante"];
                            dtrNotaPedidoItens["Peca_DS"] = dtrPedidoItens["Peca_DSTecnica"];
                            dtrNotaPedidoItens["Class_Fiscal_CD"] = dtrPedidoItens["Class_Fiscal_CD"];
                            dtrNotaPedidoItens["Embalagem_Compra"] = dtrPedidoItens["Peca_Embalagem_Tipo_Descricao"];
                            dtrNotaPedidoItens["Pedido_Compra_IT_Custo_Compra"] = dtrPedidoItens["Pedido_Compra_IT_Custo_Compra"];
                            dtrNotaPedidoItens["Quantidade_Disponivel"] = dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"];
                            dtrNotaPedidoItens["Quantidade_Disponivel_Original"] = dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"];
                            dtrNotaPedidoItens["Pedido_Garantia_Peca_Substituida"] = dtrPedidoItens["Pedido_Garantia_Peca_Substituida"];
                            dtrNotaPedidoItens["Enum_Tipo_Pedido_Compra_ID"] = dtrPedidoItens["Enum_Tipo_Pedido_ID"];
                            dtrNotaPedidoItens["Processado"] = true;

                            // Ajuste da embalagem no item do recebimento
                            dtrNotaItens["Embalagem_Compra_ID"] = dtrPedidoItens["Peca_Embalagem_Compra_ID"];

                            decimal dcmValorUnitarioNotaFiscal = this.Calcular_Valor_Unitario_Nota_Fiscal(dtrNotaItens);

                            // Joga a quantidade calculada com os impostos no campo de custo unitário
                            int intQtdeEmbalagem = this.Retornar_Quantidade_Embalagem_Peca(dtrNotaItens["Peca_ID"].DefaultInteger(),
                                                                                            dtrNotaItens["Embalagem_Compra_ID"].DefaultInteger());

                            if (intQtdeEmbalagem == 0)
                            {
                                intQtdeEmbalagem++;
                            }

                            dtrNotaItens["Recebimento_IT_Custo_Unitario"] = dcmValorUnitarioNotaFiscal / intQtdeEmbalagem;
                            dtrNotaItens["Recebimento_IT_Custo_Embalagem"] = dcmValorUnitarioNotaFiscal;
                            dtrNotaItens["Encomenda_Peca_Sem_Cadastro"] = dtrPedidoItens["Encomenda_Peca_Sem_Cadastro"].DefaultBool();

                            // Compara o Valor
                            if (this.Verificar_Diferenca_Preco(dcmValorUnitarioNotaFiscal, dtrPedidoItens["Pedido_Compra_IT_Custo_Compra"].DefaultDecimal()))
                            {
                                blnDiferenca = true;
                                blnDiferencaValor = true;
                                dtrNotaPedidoItens["Status_Recebimento_Pedido_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Valor.DefaultInteger();
                                dtrNotaItens["Status_Recebimento_Nota_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Valor.DefaultInteger();

                                dtrNotaPedidoItens["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Diferenca;
                            }
                            else
                            {
                                dtrNotaPedidoItens["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Correto;
                            }

                            // Monta as informações de quantidade
                            if (dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger() > 0
                                && dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger() > 0
                                && this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Compute("SUM(Recebimento_IT_Pedido_IT_Qtde)",
                                        "Recebimento_IT_ID = " + dtrNotaItens["Recebimento_IT_ID"].ToInteger()).DefaultInteger() != (dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() - dtrNotaItens["Recebimento_IT_Qtde_Devolvida"].DefaultInteger()))
                            {
                                if (dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger() > dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger())
                                {
                                    dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"] = dtrNotaItens["Recebimento_IT_Qtde_Restante"];
                                }
                                else
                                {
                                    dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"] = dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"];
                                }
                            }

                            if (dtrNotaPedidoItens["Enum_Status_Comparacao_Item_ID_Original"].DefaultInteger() == 0)
                            {
                                dtrNotaItens["Recebimento_IT_Qtde_Embalagens"] = dtrNotaItens["Recebimento_IT_Qtde_Embalagens"].DefaultInteger() + dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger();
                                dtrNotaItens["Recebimento_IT_Qtde_Restante"] = dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger() - dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger();
                                dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"] = dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger() - dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger();
                                dtrNotaPedidoItens["Quantidade_Disponivel"] = dtrNotaPedidoItens["Quantidade_Disponivel"].DefaultInteger() - dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger();
                            }

                            // Comparação das quantidades
                            if (!blnDiferencaValor)
                            {
                                if (dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger() == 0)
                                {
                                    dtrNotaPedidoItens["Status_Recebimento_Pedido_Item"] = null;
                                    dtrNotaItens["Status_Recebimento_Nota_Item"] = null;
                                    blnDiferenca = false;
                                    break;
                                }
                                else
                                {
                                    blnDiferenca = true;
                                    dtrNotaPedidoItens["Status_Recebimento_Pedido_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Quantidade.DefaultInteger();
                                    dtrNotaItens["Status_Recebimento_Nota_Item"] = Status_Recebimento_Pedido_Item.Diferenca_Quantidade.DefaultInteger();
                                }
                            }

                            if (dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Sem_Pedido.DefaultInteger()
                              && dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger() == 0)
                            {
                                blnComPedido = false;
                                blnDiferenca = false;
                                blnDiferencaValor = false;

                                this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Remove(dtrNotaPedidoItens);
                            }
                        }
                        else if (dtrNotaItens["Peca_ID"].DefaultInteger() == dtrPedidoItens["Peca_ID"].DefaultInteger()
                                  &&
                                 (!this.Verificar_Diferenca_Preco(this.Calcular_Valor_Unitario_Nota_Fiscal(dtrNotaItens),  
                                                              dtrPedidoItens["Pedido_Compra_IT_Custo_Compra"].DefaultDecimal()))
                                  && 
                                 (dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Diferenca.ToInteger()))
                        {
                            DataRow dtrNotaPedidoItens = this.Obter_Pedido_Item(dtrPedidoItens, dtrNotaItens);

                            if (dtrNotaPedidoItens == null) continue;

                            dtrNotaPedidoItens["Processado"] = true;

                            dtrNotaPedidoItens["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Correto;

                            dtrNotaPedidoItens["Status_Recebimento_Pedido_Item"] = null;

                            dtrNotaItens["Status_Recebimento_Nota_Item"] = null;

                            blnDiferenca = false;

                            blnComPedido = true;
                        }
                    }

                    if (blnComPedido)
                    {
                        dtrNotaItens["Processado"] = true;
                        if (blnDiferenca)
                        {
                            dtrNotaItens["Status_Imagem"] = Recebimento_Nota_FiscalBUS.Obter_Imagem_Status_Recebimento_Item(Status_Recebimento_Item.Diferenca);
                            dtrNotaItens["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Diferenca.ToInteger();
                        }
                        else
                        {
                            dtrNotaItens["Status_Imagem"] = Recebimento_Nota_FiscalBUS.Obter_Imagem_Status_Recebimento_Item(Status_Recebimento_Item.Correto);
                            dtrNotaItens["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Correto.ToInteger();
                        }
                    }

                }

                this.Calcular_Totais_Itens();

                this.Configurar_Tela_Revenda_Encomenda_Garantia(this.Verificar_Encomenda_Com_Pedido_Complementar());
            }
            catch (Exception)
            {
                throw;
            }
        }


        private bool Verificar_Pedido_Existente(DataRow dtrNotaItens, DataRow dtrNotaPedidoItens)
        {
            return (dtrNotaItens["Enum_Status_Comparacao_Item_ID"].DefaultInteger() == Status_Recebimento_Item.Sem_Pedido.DefaultInteger()
                               && dtrNotaPedidoItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger() == 0);
        }

        private DataRow Obter_Pedido_Item(DataRow dtrPedidoItens, DataRow dtrNotaItens)
        {
            DataRow dtrNotaPedidoItens;

            if (dtrPedidoItens["Recebimento_IT_Pedido_IT_ID"].DefaultInteger() != 0
                                          && dtrPedidoItens["Recebimento_IT_ID"].DefaultInteger() == dtrNotaItens["Recebimento_IT_ID"].DefaultInteger())
            {
                DataRow[] colLinhas = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Pedido_Compra_IT_ID = " + dtrPedidoItens["Pedido_Compra_IT_ID"].DefaultString()
                                                        + " AND Recebimento_IT_ID = " + dtrNotaItens["Recebimento_IT_ID"].DefaultString()
                                                        + " AND Recebimento_IT_Pedido_IT_ID = " + dtrPedidoItens["Recebimento_IT_Pedido_IT_ID"].DefaultInteger());
                if (colLinhas.Length == 1)
                {
                    if (dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger() > 0
                            && dtrPedidoItens["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger() > 0
                            && this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Compute("SUM(Recebimento_IT_Pedido_IT_Qtde)", "Recebimento_IT_ID = " + dtrNotaItens["Recebimento_IT_ID"].ToInteger()).DefaultInteger() != (dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() - dtrNotaItens["Recebimento_IT_Qtde_Devolvida"].DefaultInteger()))
                    {
                        dtrNotaPedidoItens = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Add();
                    }
                    else
                    {
                        dtrNotaPedidoItens = colLinhas[0];
                    }
                }
                else
                {
                    dtrNotaPedidoItens = null;
                }
            }
            else
            {
                dtrNotaPedidoItens = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Add();
            }

            return dtrNotaPedidoItens;
        }

        private bool Verificar_Diferenca_Preco(decimal dcmValorUnitarioNotaFiscal, decimal dcmCustoCompra)
        {
            decimal dcmPercentualAceitavelDiferencaValorMaior = Root.ParametrosProcesso.Retorna_Valor_Parametro("RECEBIMENTO", "PERCENTUAL_DIFERENCA_VALOR_A_MAIOR").DefaultDecimal() / 100;
            decimal dcmPercentualAceitavelDiferencaValorMenor = Root.ParametrosProcesso.Retorna_Valor_Parametro("RECEBIMENTO", "PERCENTUAL_DIFERENCA_VALOR_A_MENOR").DefaultDecimal() / 100;

            // Compara o Valor
            return (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
               && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger()
               && ((dcmValorUnitarioNotaFiscal < (dcmCustoCompra * (1 - dcmPercentualAceitavelDiferencaValorMenor)))
                   || (dcmValorUnitarioNotaFiscal > (dcmCustoCompra * (1 + dcmPercentualAceitavelDiferencaValorMaior)))));
        }

        private decimal Calcular_Valor_Unitario_Nota_Fiscal(DataRow dtrNotaItens)
        {
            decimal dcmValorUnitarioNotaFiscal = dtrNotaItens["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal();

            if (dtrNotaItens["Recebimento_IT_Valor_Desconto"].DefaultDecimal() > 0 && dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() > 0)
            {
                dcmValorUnitarioNotaFiscal -= dtrNotaItens["Recebimento_IT_Valor_Desconto"].DefaultDecimal() / dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
            }

            if (dtrNotaItens["Recebimento_IT_IPI_Valor"].DefaultDecimal() > 0 && dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() > 0)
            {
                dcmValorUnitarioNotaFiscal += dtrNotaItens["Recebimento_IT_IPI_Valor"].DefaultDecimal() / dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
            }

            if (dtrNotaItens["Recebimento_IT_Valor_Substituicao"].DefaultDecimal() > 0 && dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger() > 0)
            {
                dcmValorUnitarioNotaFiscal += dtrNotaItens["Recebimento_IT_Valor_Substituicao"].DefaultDecimal() / dtrNotaItens["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
            }

            return dcmValorUnitarioNotaFiscal;
        }

        private bool Verificar_Encomenda_Com_Pedido_Complementar()
        {
            try
            {
                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Enumerados.Tipo_Recebimento.Encomenda_Pedido.ToInteger())
                {
                    return false;
                }

                if (this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Enum_Tipo_Pedido_Compra_ID = " + Tipo_Pedido.Pedido.ToInteger().DefaultString()).Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Mudar_Cor_Grid_Itens_Diferenca()
        {
            try
            {
                DataGridViewCellStyle objStyle = new DataGridViewCellStyle();
                objStyle.BackColor = Color.Red;
                objStyle.ForeColor = Color.White;

                foreach (DataGridViewRow dtrNotaItens in this.dgvNotaFiscalItens.Grid00.Rows)
                {

                    if ((Status_Recebimento_Item)dtrNotaItens.Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Correto
                            || (Status_Recebimento_Item)dtrNotaItens.Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Sem_Cadastro)
                    {
                        continue;
                    }

                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                    && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger()
                    && dtrNotaItens.Cells["Status_Recebimento_Nota_Item"].Value.DefaultInteger() == Status_Recebimento_Pedido_Item.Diferenca_Valor.DefaultInteger())
                    {
                        dtrNotaItens.Cells["Recebimento_IT_Custo_Nota_Fiscal"].Style = objStyle;
                    }
                    else if (dtrNotaItens.Cells["Status_Recebimento_Nota_Item"].Value.DefaultInteger() == Status_Recebimento_Pedido_Item.Diferenca_Quantidade.DefaultInteger())
                    {
                        dtrNotaItens.Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Style = objStyle;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Mudar_Cor_Grid_Itens_Diferenca_Detalhe()
        {
            try
            {
                DataGridViewCellStyle objStyle = new DataGridViewCellStyle();
                objStyle.BackColor = Color.Red;
                objStyle.ForeColor = Color.White;

                foreach (DataGridViewRow dgrPedidoItens in this.dgvNotaFiscalItens.Grid01.Rows)
                {

                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                    && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger()
                    && dgrPedidoItens.Cells["Status_Recebimento_Pedido_Item"].Value.ToString() == Status_Recebimento_Pedido_Item.Diferenca_Valor.ToString())
                    {
                        dgrPedidoItens.Cells["Pedido_Compra_IT_Custo_Compra"].Style = objStyle;
                    }
                    else if (dgrPedidoItens.Cells["Status_Recebimento_Pedido_Item"].Value.ToString() == Status_Recebimento_Pedido_Item.Diferenca_Quantidade.ToString())
                    {
                        dgrPedidoItens.Cells["Recebimento_IT_Pedido_IT_Qtde"].Style = objStyle;
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Validar_Processo_Comparacao_Pedido_Nota_Fiscal()
        {

            try
            {
                // Valida se os campos para fazer o processamento foram preenchidos
                if (this.cboDestinatarioLoja.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário selecionar o destinatário da nota para executar o processamento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.cboDestinatarioLoja.Focus();
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    return false;
                }

                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário selecionar o tipo de recebimento da nota para executar o processamento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.cboTipoRecebimento.Focus();
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    return false;
                }

                if (this.dtsRecebimento.Tables["Fornecedor"].Rows.Count <= 0 || this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].ToInteger() == 0)
                {
                    MessageBox.Show("É necessário informar o emitente da nota para executar o processamento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtEmitenteCNPJCPF.Focus();
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    return false;
                }

                if (this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count <= 0)
                {
                    MessageBox.Show("É necessário informar no mínimo um item da nota para que a conferência dos itens possa ser processada.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dgvNotaFiscalItens.Focus();
                    return false;
                }

                // Valida se existe algum pedido selecionado
                bool blnExistePedidoSelecionado = false;
                for (int i = 0; i < this.dgvPedidoCompras.Rows.Count; i++)
                {
                    if (this.dgvPedidoCompras.Rows[i].Cells["Marcado"].Value.DefaultBool() == true)
                    {
                        blnExistePedidoSelecionado = true;
                        break;
                    }
                }

                if (blnExistePedidoSelecionado == false)
                {
                    MessageBox.Show("É necessário selecionar no mínimo um pedido para que a conferência dos itens possa ser processada.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dgvPedidoCompras.Focus();
                    return false;
                }

                if (!this.Validar_Condicao_Pagamento(true))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Validar_Condicao_Pagamento(bool blnAtualizarCondicaoPagamento)
        {
            try
            {
                DataTable dttPedidos = this.Obter_DataTable_Do_Filtro_Selecionado(this.dgvPedidoCompras);

                if (dttPedidos.Rows.Count == 0
                    || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                    || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Bonificacao_Pedido.DefaultInteger())
                {
                    return true;
                }

                // Obtem os itens dos pedidos pendentes e uma listagem de codigo de fornecedor alternativo
                Pre_RecebimentoBUS_NEW busPreRecebimentoNew = new Pre_RecebimentoBUS_NEW();
                DataSet dtsPedidos = busPreRecebimentoNew.Consultar_Itens_Pedido_Compra_Pendentes_Por_Fornecedor(this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].ToInteger(), this.cboLojaRecebimento.SelectedValue.ToInteger(), this.lblRecebimentoID.Text.DefaultInteger(), this.cboTipoRecebimento.SelectedValue.DefaultInteger(), dttPedidos);

                DataTable dttResultado = new DataTable();
                dttResultado.Columns.Add("Pedido_Compra_CT_ID", typeof(int));
                dttResultado.Columns.Add("Valor", typeof(decimal));

                foreach (DataRow dtrNotaItens in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                {
                    if (dtrNotaItens["Peca_ID"].DefaultInteger() == 0)
                    {
                        continue;
                    }

                    DataRow[] colNotaPedidoItens = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + dtrNotaItens["Recebimento_IT_ID"].DefaultString());

                    foreach (DataRow dtrNotaPedidoItem in colNotaPedidoItens)
                    {
                        this.Adicionar_Valor_DataTable(ref dttResultado,
                                                       dtrNotaPedidoItem["Pedido_Compra_CT_ID"].DefaultInteger(),
                                                       dtrNotaItens["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal() * dtrNotaPedidoItem["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger());
                    }

                    if (dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger() > 0)
                    {
                        int intQtdeRestante = dtrNotaItens["Recebimento_IT_Qtde_Restante"].DefaultInteger();

                        DataRow[] colPedidos = dtsPedidos.Tables["Pedidos"].Select("Peca_ID = " + dtrNotaItens["Peca_ID"].DefaultString());

                        foreach (DataRow dtrPedido in colPedidos)
                        {
                            if (intQtdeRestante == 0)
                            {
                                break;
                            }

                            if (dtrPedido["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger() == 0)
                            {
                                continue;
                            }

                            if (dtrPedido["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger() > intQtdeRestante)
                            {
                                this.Adicionar_Valor_DataTable(ref dttResultado,
                                                               dtrPedido["Pedido_Compra_CT_ID"].DefaultInteger(),
                                                               dtrNotaItens["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal() * intQtdeRestante);

                                intQtdeRestante = 0;
                            }
                            else
                            {
                                this.Adicionar_Valor_DataTable(ref dttResultado,
                                                               dtrPedido["Pedido_Compra_CT_ID"].DefaultInteger(),
                                                               dtrNotaItens["Recebimento_IT_Custo_Nota_Fiscal"].DefaultDecimal() * dtrPedido["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger());

                                intQtdeRestante -= dtrPedido["Pedido_Compra_IT_Quantidade_Disponivel"].DefaultInteger();
                            }
                        }
                    }
                }

                DataRow[] colMaiorValor = dttResultado.Select("Valor = MAX(Valor)", string.Empty);

                if (colMaiorValor.Length > 0)
                {
                    Pedido_CompraBUS busPedidoCompra = new Pedido_CompraBUS();
                    Pedido_Compra_CTDO dtoPedidoCompra = busPedidoCompra.Selecionar(colMaiorValor[0]["Pedido_Compra_CT_ID"].DefaultInteger());

                    if (this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger() != Tipo_Recebimento.Encomenda_Pedido.ToInteger())
                    {
                        if (this.dgvParcelas.Rows.Count != 0 && blnAtualizarCondicaoPagamento)
                        {
                            this.dtoCondicaoPagamento.ID = dtoPedidoCompra.ID_Condicao_Pagamento.DefaultInteger();
                            this.txtCondicaoPagamento.Enabled = false;
                        }
                    }

                    if (this.dtoCondicaoPagamento == null || this.dtoCondicaoPagamento.ID == 0)
                    {
                        MessageBox.Show("Informe uma condição de pagamento",
                                        this.Text,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);

                        return false;
                    }

                    if (dtoPedidoCompra.ID_Condicao_Pagamento != this.dtoCondicaoPagamento.ID)
                    {
                        MessageBox.Show("A condição de pagamento da nota fiscal difere do pedido de compra nº " + dtoPedidoCompra.ID.DefaultString(),
                                        this.Text,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);

                        if (!blnAtualizarCondicaoPagamento)
                            return blnAtualizarCondicaoPagamento;
                        
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Adicionar_Valor_DataTable(ref DataTable dttResultado, int intPedidoCompraCTID, decimal dcmValor)
        {
            try
            {
                DataRow[] colPedidos = dttResultado.Select("Pedido_Compra_CT_ID = " + intPedidoCompraCTID.DefaultString());

                if (colPedidos.Length == 0)
                {
                    dttResultado.Rows.Add(intPedidoCompraCTID, dcmValor);
                }
                else
                {
                    colPedidos[0]["Valor"] = colPedidos[0]["Valor"].DefaultDecimal() + dcmValor;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Obter_DataTable_Do_Filtro_Selecionado(DataGridView dgvObjeto)
        {
            try
            {
                DataTable dttObjeto = new DataTable();
                dttObjeto.Columns.Add(new DataColumn("Pedido_ID", typeof(int)));

                for (int i = 0; i < dgvObjeto.RowCount; i++)
                {
                    if (dgvObjeto.Rows[i].Cells["Marcado"].Value.DefaultBool() == true)
                    {
                        dttObjeto.Rows.Add(dgvObjeto.Rows[i].Cells["Pedido_ID"].Value);
                    }
                }

                return dttObjeto;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Calcular_Totais_Itens()
        {
            try
            {
                decimal dcmValorTotalNota = 0;
                decimal dcmValorTotalLiberado = 0;
                int intTotalItens = 0;
                int intTotalConferido = 0;
                int intTotalDiferente = 0;
                int intTotalDevolvido = 0;

                foreach (DataGridViewRow dgrItensNota in this.dgvNotaFiscalItens.Grid00.Rows)
                {
                    dcmValorTotalNota += dgrItensNota.Cells["Recebimento_IT_Custo_Total"].Value.DefaultDecimal();
                    dcmValorTotalLiberado += dgrItensNota.Cells["Recebimento_IT_Custo_Nota_Fiscal"].Value.DefaultDecimal() * dgrItensNota.Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger();
                    intTotalItens += 1;

                    if (dgrItensNota.Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Correto.DefaultInteger())
                    {
                        intTotalConferido += 1;
                    }
                    else if (dgrItensNota.Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Devolvido.DefaultInteger())
                    {
                        intTotalDevolvido += 1;
                    }
                    else
                    {
                        intTotalDiferente += 1;
                    }
                }

                this.lblValorTotalItensNotaFiscal.Text = dcmValorTotalNota.ToString("#,##0.00");
                this.lblTotalItensLiberados.Text = dcmValorTotalLiberado.ToString("#,##0.00");
                this.lblItens.Text = intTotalItens.ToString();
                this.lblItensCorretos.Text = intTotalConferido.ToString();
                this.lblItensPendentes.Text = intTotalDiferente.ToString();
                this.lblItensDevolvidos.Text = intTotalDevolvido.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Boolean Verificar_Existencia_Itens_Para_Processar()
        {
            try
            {
                if (this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count > 0)
                {
                    Int32 intQtdeItensTratados = this.dtsRecebimento.Tables["RecebimentoIT"].Select("Enum_Status_Comparacao_Item_ID = " + Status_Recebimento_Item.Correto.ToInteger() + " OR " +
                                                                                                    "Enum_Status_Comparacao_Item_ID = " + Status_Recebimento_Item.Devolvido.ToInteger()).Length;

                    return this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count > intQtdeItensTratados;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Alterar_Macarcao_Pedido_Compra(DataRow dtrPedidoSelecionado)
        {
            try
            {
                if (dtrPedidoSelecionado != null)
                {
                    Int32 intQtdeNotaUtilizamPedido = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Pedido_Compra_CT_ID = " + dtrPedidoSelecionado["Pedido_ID"].ToString()).Length;

                    if (intQtdeNotaUtilizamPedido > 0)
                    {
                        MessageBox.Show("O pedido não pode ser desmarcado pois está sendo utilizado para nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        dtrPedidoSelecionado["Marcado"] = !Convert.ToBoolean(dtrPedidoSelecionado["Marcado"]);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        #endregion

        #region "   Pré Recebimento         "

        private bool Validar_Pre_Recebimento_A_Gerar(bool blnExibeMensagem)
        {
            try
            {
                if (this.lblRecebimentoID.Text == string.Empty)
                {
                    if (blnExibeMensagem)
                    {
                        MessageBox.Show("Não é possível gerar o pré-recebimento. Para realizar a geração do pré-recebimento é necessário clicar antes no botão Aplicar.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.btnProcessar.Focus();
                    }
                    return false;
                }

                if (blnExibeMensagem && this.blnNotaCancelada)
                {
                    MessageBox.Show("Não é possível gerar o pré-recebimento, pois a nota fiscal foi cancelada pelo fornecedor.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                foreach (DataGridViewRow dgrLinha in this.dgvNotaFiscalItens.Grid01.Rows)
                {
                    if (!dgrLinha.Cells["Pre_Recebimento_Gerado"].Value.DefaultBool()
                        && dgrLinha.Cells["Enum_Status_Comparacao_Item_ID"].Value.DefaultInteger() == Status_Recebimento_Item.Correto.DefaultInteger()
                        && dgrLinha.Cells["Enum_Tipo_Pedido_Compra_ID"].Value.DefaultInteger() != Tipo_Pedido.Pedido_De_Encomenda.DefaultInteger())
                    {
                        return true;
                    }
                }

                if (blnExibeMensagem)
                {
                    MessageBox.Show("Não foi encontrado nenhum item pendente de gerar o pré-recebimento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Formulario_Pre_Recebimento()
        {
            try
            {
                // Nota Fiscal
                if (this.cboNotaFiscalNumero.Text == string.Empty)
                {
                    MessageBox.Show("É necessário informar o numero da Nota Fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboNotaFiscalNumero.Focus();
                    return false;
                }

                if (this.lblNotaFiscalSerie.Text == string.Empty)
                {
                    MessageBox.Show("É necessário informar a série da Nota Fiscal. Altere primeiro a Ordem de Desembarque.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    return false;
                }

                if (this.txtChaveAcesso.Text != string.Empty && this.txtChaveAcesso.Text.Length < this.txtChaveAcesso.MaxLength)
                {
                    MessageBox.Show("A chave informada é menor que " + this.txtChaveAcesso.MaxLength.ToString() + " caracteres.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.txtChaveAcesso.Focus();
                    return false;
                }

                if (this.cboNaturezaOperacao.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar a natureza da operação.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboNaturezaOperacao.Focus();
                    return false;
                }

                if (this.cboNaturezaFinanceira.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar a natureza financeira.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboNaturezaFinanceira.Focus();
                    return false;
                }

                if (this.cboModelo.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar o modelo da nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboModelo.Focus();
                    return false;
                }

                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar o tipo de recebimento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboTipoRecebimento.Focus();
                    return false;
                }

                if (this.cboTipoOperacao.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar o tipo de operação.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboTipoOperacao.Focus();
                    return false;
                }
                // Destinatário
                if (this.cboDestinatarioLoja.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar a loja de destino.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboDestinatarioLoja.Focus();
                    return false;
                }
                // Emitente
                if (this.intFornecedorID == 0)
                {
                    MessageBox.Show("É necessário informar o emissor da nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.txtEmitenteCNPJCPF.Focus();
                    return false;
                }

                if (!this.Validar_Data_Emissao_Parcela())
                {
                    return false;
                }

                if (this.dtoCondicaoPagamento != null && this.dtoCondicaoPagamento.Gerar_Contas_A_Pagar && this.dgvParcelas.Rows.Count == 0)
                {
                    MessageBox.Show("Não foram gerados as parcelas para esta nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.btnGerarParcelas.Focus();
                    return false;
                }

                decimal dcmDiferencaValorNotaFiscalMenor = 0;
                decimal dcmDiferencaValorNotaFiscalMaior = 0;
                this.Localizar_Valor_Parametro_Diferenca_Nota_Fiscal(ref dcmDiferencaValorNotaFiscalMenor, ref dcmDiferencaValorNotaFiscalMaior);

                // Cobrança
                if (this.txtCondicaoPagamento.Text.IsNullOrEmpty())
                {
                    MessageBox.Show("É necessário informar uma condição de pagamento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.txtCondicaoPagamento.Focus();

                    return false;
                }
                else
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Bonificacao_Pedido.DefaultInteger()
                       && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Bonificacao_Vale.DefaultInteger()
                       && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                       && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        decimal dcmSoma = 0;

                        foreach (DataGridViewRow dgrParcela in this.dgvParcelas.Rows)
                        {
                            dcmSoma += dgrParcela.Cells["Parcela_Valor"].Value.DefaultDecimal();
                        }



                        if (((dcmSoma >= this.mskValorTotalNotaFiscal.Text.DefaultDecimal() - dcmDiferencaValorNotaFiscalMenor)
                                 && (dcmSoma <= this.mskValorTotalNotaFiscal.Text.DefaultDecimal() + dcmDiferencaValorNotaFiscalMaior)) == false)
                        {
                            MessageBox.Show("Soma do valor das parcelas difere do Valor Total da Nota Fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                            this.mskValorTotalNotaFiscal.Focus();
                            return false;
                        }
                    }
                }

                if (this.mskValorTotalProdutos.Text.DefaultDecimal() == 0)
                {
                    MessageBox.Show("Valor total dos produtos não pode ser Zero.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.mskValorTotalProdutos.Focus();
                    return false;
                }

                // Totais
                if (this.mskValorTotalNotaFiscal.Text.DefaultDecimal() == 0)
                {
                    MessageBox.Show("Valor total da nota fiscal não pode ser Zero.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.mskValorTotalNotaFiscal.Focus();
                    return false;
                }
                else
                {
                    decimal dcmValorTotalNota =
                        this.mskValorTotalProdutos.Text.DefaultDecimal() +
                        this.mskValorTotalOutros.Text.DefaultDecimal() +
                        this.mskValorTotalSeguro.Text.DefaultDecimal() +
                        this.mskValorTotalFrete.Text.DefaultDecimal() +
                        this.mskValorTotalICMSSubstituicao.Text.DefaultDecimal() +
                        this.mskValorTotalIPI.Text.DefaultDecimal() -
                        this.mskValorTotalDesconto.Text.DefaultDecimal();

                    if ((dcmValorTotalNota < this.mskValorTotalNotaFiscal.Text.DefaultDecimal() - dcmDiferencaValorNotaFiscalMenor)
                            || (dcmValorTotalNota > this.mskValorTotalNotaFiscal.Text.DefaultDecimal() + dcmDiferencaValorNotaFiscalMaior))
                    {
                        MessageBox.Show("Valor total da nota fiscal diferente do somatório.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.mskValorTotalNotaFiscal.Focus();
                        return false;
                    }

                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Ordem_Desembarque_Valida()
        {
            try
            {
                Ordem_DesembarqueBUS busOrdemDesembarqueNF = new Ordem_DesembarqueBUS();
                DataTable dttOrdemDesembarque = busOrdemDesembarqueNF.Consultar_DataTable_Ordem_Desembarque_NF(this.cboNotaFiscalNumero.Text, this.lblNotaFiscalSerie.Text, this.intFornecedorID);

                if (dttOrdemDesembarque == null || dttOrdemDesembarque.Rows.Count == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Habilitar_Botao_Pre_Recebimento()
        {
            try
            {
                this.btnGerarPreRecebimento.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Gerar_Pre_Recebimento.ToString())
                                                        && this.Validar_Pre_Recebimento_A_Gerar(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Montar_Filtro_Grupo()
        {
            try
            {
                DataTable dttFiltro = new DataTable();
                dttFiltro.Columns.Add("Valor", typeof(int));

                foreach (DataGridViewRow dgrLinha in this.dgvGrupoPreRecebimento.Rows)
                {
                    if (Convert.ToBoolean(dgrLinha.Cells["Marcado"].Value.DefaultInteger()))
                    {
                        dttFiltro.Rows.Add(dgrLinha.Cells["Pre_Recebimento_Grupo_ID"].Value.DefaultInteger());
                    }
                }

                return dttFiltro;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Montar_Filtro_Pre_Recebimento()
        {
            try
            {
                DataTable dttFiltro = new DataTable();
                dttFiltro.Columns.Add("Valor", typeof(int));

                foreach (DataGridViewRow dgrLinha in this.dgvPreRecebimento.Rows)
                {
                    if (Convert.ToBoolean(dgrLinha.Cells["Marcado"].Value.DefaultInteger()))
                    {
                        dttFiltro.Rows.Add(dgrLinha.Cells["Pre_Recebimento_CT_ID"].Value.DefaultInteger());
                    }
                }

                return dttFiltro;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int Retornar_Ordem_Desembarque_NF_ID()
        {
            try
            {
                if (this.cboNotaFiscalNumero.Items.Count > 0)
                {
                    return this.dttOrdemDesembarqueNF.Select(string.Concat("Ordem_Desembarque_NF_Numero =",
                                                                            this.cboNotaFiscalNumero.SelectedValue))[0]["Ordem_Desembarque_NF_ID"].DefaultInteger();
                }

                return this.dttOrdemDesembarqueNF.Rows[0]["Ordem_Desembarque_NF_ID"].DefaultInteger();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Montar_Pre_Recebimento_CT(DataSet dtsPreRecebimento)
        {
            try
            {
                dtsPreRecebimento.Tables["Capa"].Columns.Add("Enum_Tipo_Recebimento_ID", typeof(int));

                DBUtil objData = new DBUtil();
                DataRow dtrData = dtsPreRecebimento.Tables["Capa"].NewRow();

                dtrData["Pre_Recebimento_CT_ID"] = 0;
                dtrData["Pre_Recebimento_Grupo_ID"] = 0;
                if (this.dttOrdemDesembarqueNF != null && this.dttOrdemDesembarqueNF.Rows.Count > 0)
                {
                    dtrData["Ordem_Desembarque_NF_ID"] = this.Retornar_Ordem_Desembarque_NF_ID();
                }
                else
                {
                    dtrData["Ordem_Desembarque_NF_ID"] = 0;
                }
                dtrData["Loja_ID"] = this.cboLojaRecebimento.SelectedValue;
                dtrData["Forn_ID"] = this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger();

                if (dtrData["Ordem_Desembarque_NF_ID"].DefaultInteger() != 0)
                {
                    Ordem_DesembarqueBUS busOrdemDesembarque = new Ordem_DesembarqueBUS();
                    DataRow dtrOrdemDesembarque = busOrdemDesembarque.Selecionar_Pela_Ordem_Desembarque_NF(dtrData["Ordem_Desembarque_NF_ID"].DefaultInteger());

                    if (dtrOrdemDesembarque != null && dtrOrdemDesembarque["Ordem_Desembarque_Liberado"].DefaultBool())
                    {
                        dtrData["Enum_Status_ID"] = Enumerados.Status_Pre_Recebimento.Pendente_Preparacao.DefaultInteger();
                    }
                    else
                    {
                        dtrData["Enum_Status_ID"] = Enumerados.Status_Pre_Recebimento.Aguardando_Mercadoria.DefaultInteger();
                    }
                }
                else
                {
                    dtrData["Enum_Status_ID"] = Enumerados.Status_Pre_Recebimento.Aguardando_Mercadoria.DefaultInteger();
                }

                dtrData["Enum_Tipo_ID"] = this.Retornar_ID_Enumerado_Tipo_Recebimento((Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue);
                dtrData["Usuario_Criacao_ID"] = Root.Funcionalidades.UsuarioDO_Ativo.ID;
                dtrData["Usuario_Conferencia_NF_ID"] = Root.Funcionalidades.UsuarioDO_Ativo.ID;
                dtrData["Usuario_Preparacao_ID"] = 0;
                dtrData["Usuario_Finalizacao_ID"] = 0;
                dtrData["Pre_Recebimento_CT_Nota_Fiscal"] = this.cboNotaFiscalNumero.Text;
                dtrData["Pre_Recebimento_CT_Data_Criacao"] = objData.Obter_Data_do_Servidor(true, TipoServidor.Central);
                dtrData["Pre_Recebimento_CT_Data_Conferencia_NF"] = objData.Obter_Data_do_Servidor(true, TipoServidor.Central);
                dtrData["Pre_Recebimento_CT_Devolucao"] = 0;
                dtrData["Recebimento_CT_ID"] = this.lblRecebimentoID.Text;
                dtrData["Enum_Tipo_Recebimento_ID"] = this.cboTipoRecebimento.SelectedValue.DefaultInteger();

                dtsPreRecebimento.Tables["Capa"].Rows.Add(dtrData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int Retornar_ID_Enumerado_Tipo_Recebimento(Tipo_Recebimento enuTipoRecebimento)
        {
            try
            {
                switch (enuTipoRecebimento)
                {
                    case Tipo_Recebimento.Bonificacao_Pedido:
                        return Tipo_Pre_Recebimento.Bonificacao.DefaultInteger();
                    case Tipo_Recebimento.Bonificacao_Vale:
                        return Tipo_Pre_Recebimento.Bonificacao.DefaultInteger();
                    case Tipo_Recebimento.Consignacao_Pedido:
                        return Tipo_Pre_Recebimento.Consignacao.DefaultInteger();
                    case Tipo_Recebimento.Consignacao_Vale:
                        return Tipo_Pre_Recebimento.Consignacao.DefaultInteger();
                    case Tipo_Recebimento.Consumo_Pedido:
                        return Tipo_Pre_Recebimento.Consumo.DefaultInteger();
                    case Tipo_Recebimento.Consumo_Vale:
                        return Tipo_Pre_Recebimento.Consumo.DefaultInteger();
                    case Tipo_Recebimento.Encomenda_Pedido:
                        return Tipo_Pre_Recebimento.Revenda.DefaultInteger();
                    case Tipo_Recebimento.Encomenda_Pedido_Governo:
                        return Tipo_Pre_Recebimento.Revenda.DefaultInteger();
                    case Tipo_Recebimento.Encomenda_Vale:
                        return Tipo_Pre_Recebimento.Revenda.DefaultInteger();
                    case Tipo_Recebimento.Encomenda_Vale_Governo:
                        return Tipo_Pre_Recebimento.Revenda.DefaultInteger();
                    case Tipo_Recebimento.Garantia_Pedido:
                        return Tipo_Pre_Recebimento.Garantia.DefaultInteger();
                    case Tipo_Recebimento.Garantia_Vale:
                        return Tipo_Pre_Recebimento.Garantia.DefaultInteger();
                    case Tipo_Recebimento.Revenda_Pedido:
                        return Tipo_Pre_Recebimento.Revenda.DefaultInteger();
                    case Tipo_Recebimento.Revenda_Vale:
                        return Tipo_Pre_Recebimento.Revenda.DefaultInteger();
                    default:
                        return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Montar_Pre_Recebimento_IT(DataSet dtsPreRecebimento, DataTable dttItensAjusteQuantidade)
        {
            try
            {
                dtsPreRecebimento.Tables["Itens"].Columns.Add("Recebimento_IT_ID", typeof(int));

                foreach (DataGridViewRow dgrLinha in this.dgvNotaFiscalItens.Grid00.Rows)
                {
                    if (dgrLinha.Cells["Recebimento_IT_Qtde_Embalagens"].Value.DefaultInteger() > dgrLinha.Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger())
                    {
                        foreach (DataRow dtrLinha01 in this.dtsRecebimento.Tables["Nota_Pedido_Itens"]
                            .Select("Recebimento_IT_ID = " + dgrLinha.Cells["Recebimento_IT_ID"].Value.DefaultString() +
                                    " AND Enum_Tipo_Pedido_Compra_ID <> " + Tipo_Pedido.Pedido_De_Encomenda.DefaultInteger().DefaultString()))
                        {
                            if (dtrLinha01["Pre_Recebimento_Gerado"].DefaultInteger() == 0
                                && dtrLinha01["Status_Recebimento_Pedido_Item"].DefaultInteger() != Status_Recebimento_Pedido_Item.Diferenca_Valor.DefaultInteger()
                                && dtrLinha01["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger() > 0)
                            {
                                DataRow[] colLinhas;

                                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger() &&
                                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                                {
                                    colLinhas = dtsPreRecebimento.Tables["Itens"].Select("Pedido_Origem_ID = " + dtrLinha01["Pedido_Compra_CT_ID"].DefaultString()
                                                                                            + " AND Pre_Recebimento_IT_Seq_Item_Pedido = " + dtrLinha01["Pedido_Compra_IT_Sequencia"].DefaultString()
                                                                                            + " AND Objeto_Origem_ID = " + dgrLinha.Cells["Peca_ID"].Value.DefaultString());
                                }
                                else
                                {
                                    colLinhas = dtsPreRecebimento.Tables["Itens"].Select("Pedido_Origem_ID = " + dtrLinha01["Pedido_Compra_CT_ID"].DefaultString()
                                                                                            + " AND Objeto_Origem_ID = " + dgrLinha.Cells["Peca_ID"].Value.DefaultString());
                                }

                                DataRow dtrData;
                                bool blnNovaLinha = false;

                                if (colLinhas.Length == 1)
                                {
                                    dtrData = colLinhas[0];
                                }
                                else
                                {
                                    dtrData = dtsPreRecebimento.Tables["Itens"].NewRow();
                                    blnNovaLinha = true;
                                }

                                dtrData["Pedido_IT_ID"] = 0;
                                dtrData["Objeto_Origem_ID"] = dgrLinha.Cells["Peca_ID"].Value;
                                dtrData["Pedido_Origem_ID"] = dtrLinha01["Pedido_Compra_CT_ID"];

                                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger() &&
                                    this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                                {
                                    dtrData["Pre_Recebimento_IT_Seq_Item_Pedido"] = dtrLinha01["Pedido_Compra_IT_Sequencia"].DefaultInteger();
                                }

                                dtrData["Pedido_IT_Quantidade"] = dtrData["Pedido_IT_Quantidade"].DefaultInteger() + dtrLinha01["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger();
                                dtrData["Pre_Recebimento_IT_Qtde_NF"] =
                                    dtrData["Pre_Recebimento_IT_Qtde_NF"].DefaultInteger() +
                                    (dtrLinha01["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger() * this.Retornar_Quantidade_Embalagem_Peca(dgrLinha.Cells["Peca_ID"].Value.DefaultInteger(), dgrLinha.Cells["Embalagem_Compra_ID"].Value.DefaultInteger()));

                                dtrData["Pre_Recebimento_IT_Qtde_Fisico"] = 0;
                                dtrData["Pre_Recebimento_IT_Imprimir_Etiqueta"] = dgrLinha.Cells["Pre_Recebimento_IT_Imprimir_Etiqueta"].Value.DefaultBool();
                                dtrData["Peca_Embalagem_Venda_ID"] = this.Retornar_Peca_Embalagem_ID_Cadastro(dgrLinha.Cells["Peca_ID"].Value.DefaultInteger());
                                dtrData["Pre_Recebimento_CT_ID"] = 0;
                                dtrData["Pre_Recebimento_IT_ID"] = 0;
                                dtrData["Lojas_ID"] = this.cboLojaRecebimento.SelectedValue;
                                dtrData["Enum_Tipo_Objeto_ID"] = Enumerados.Tipo_Objeto.Peca.DefaultInteger();
                                dtrData["Estoque_Transito_Ja_Tratado"] = 0;
                                dtrData["Pre_Recebimento_IT_Improcedente"] = 0;
                                dtrData["NFe_Entrada_IT_ID"] = 0;
                                dtrData["Peca_Gerar_Etiqueta_Automatica_Recebimento"] = dgrLinha.Cells["Peca_Gerar_Etiqueta_Automatica_Recebimento"].Value.DefaultBool();

                                dtrData["Recebimento_IT_ID"] = dgrLinha.Cells["Recebimento_IT_ID"].Value;

                                dttItensAjusteQuantidade.Rows.Add(
                                                    dgrLinha.Cells["Recebimento_IT_ID"].Value.DefaultInteger(),
                                                    dtrLinha01["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger(),
                                                    dtrLinha01["Recebimento_IT_Pedido_IT_ID"].DefaultInteger(),
                                                    dtrLinha01["Pre_Recebimento_Gerado"].DefaultBool());

                                if (blnNovaLinha)
                                {
                                    dtsPreRecebimento.Tables["Itens"].Rows.Add(dtrData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int Retornar_Quantidade_Embalagem_Peca(int intPecaID, int intEmbalagemPecaID)
        {
            try
            {
                int intQtdeEmbalagem = 0;
                Peca_EmbalagemBUS busPecaEmbalagem = new Peca_EmbalagemBUS();
                List<Peca_Embalagem_DO> colPecaEmbalagem = busPecaEmbalagem.Consultar_Embalagem_Qtde(intPecaID, true);
                foreach (Peca_Embalagem_DO dtoPecaEmbalagem in colPecaEmbalagem)
                {
                    if (dtoPecaEmbalagem.ID == intEmbalagemPecaID)
                    {
                        intQtdeEmbalagem = dtoPecaEmbalagem.Quantidade;
                        break;
                    }
                }

                return intQtdeEmbalagem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int Retornar_Peca_Embalagem_ID_Cadastro(int intPecaID)
        {
            try
            {
                int intPecaEmbalagemID = 0;
                Peca_EmbalagemBUS busPecaEmbalagem = new Peca_EmbalagemBUS();
                List<Peca_Embalagem_DO> colPecaEmbalagem = busPecaEmbalagem.Consultar_Embalagem_Qtde(intPecaID, true);
                foreach (Peca_Embalagem_DO dtoPecaEmbalagem in colPecaEmbalagem)
                {
                    if (dtoPecaEmbalagem.Tipo_Embalagem_ID == Tipo_Embalagem.Peca_Unidade
                        && dtoPecaEmbalagem.Quantidade == 1)
                    {
                        intPecaEmbalagemID = dtoPecaEmbalagem.ID;
                        break;
                    }
                }

                return intPecaEmbalagemID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inserir_Item_Nao_Localizado(int intRecebimentoITID, int intSequencia, int intPedidoID, int intPecaID, decimal dcmCusto, int intQtde, int intQtdeDisponivel)
        {
            try
            {
                PecaBUS busPeca = new PecaBUS();
                PecaDO dtoPeca = busPeca.Selecionar_Com_Atribuicoes(intPecaID);

                DataRow dtrLinha = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].NewRow();

                dtrLinha["Recebimento_IT_ID"] = intRecebimentoITID;
                dtrLinha["Pedido_Compra_IT_Sequencia"] = intSequencia;
                dtrLinha["Pedido_Compra_CT_ID"] = intPedidoID;
                dtrLinha["NF_CD_Fabricante"] = dtoPeca.Codigo_Item_Fabricante;
                dtrLinha["Peca_DS"] = dtoPeca.Descricao_Tecnica;
                dtrLinha["Class_Fiscal_CD"] = dtoPeca.Classificacao_Fiscal.Codigo;
                dtrLinha["Embalagem_Compra"] = Utilitario.Obter_DataRow_Enumerado_Da_Memoria(dtoPeca.ID_Tipo_Embalagem)["Enum_Extenso"];
                dtrLinha["Pedido_Compra_IT_Custo_Compra"] = dcmCusto;
                dtrLinha["Recebimento_IT_Pedido_IT_Qtde"] = intQtde;
                dtrLinha["Quantidade_Disponivel"] = intQtdeDisponivel;
                dtrLinha["Quantidade_Disponivel_Original"] = intQtdeDisponivel;

                this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Add(dtrLinha);

                Recebimento_ITBUS busRecebimentoIT = new Recebimento_ITBUS();

                Recebimento_ITDO dtoRecebimentoIT = busRecebimentoIT.Selecionar(intRecebimentoITID);
                dtoRecebimentoIT.ID_Peca = dtoPeca.ID;
                dtoRecebimentoIT.Status_Recebimento_Item = Status_Recebimento_Item.Sem_Pedido;

                busRecebimentoIT.Alterar(dtoRecebimentoIT);

                this.dgvNotaFiscalItens.Grid01.DataSource = this.dtsRecebimento.Tables["Nota_Pedido_Itens"];

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Jogar_Quantidade_Pre_Gerado(DataTable dttItens)
        {
            try
            {
                foreach (DataRow dtrLinha in dttItens.Rows)
                {
                    DataRow[] colLinha = this.dtsRecebimento.Tables["RecebimentoIT"].Select("Recebimento_IT_ID = " + dtrLinha["Recebimento_IT_ID"].DefaultString());

                    if (colLinha.Length > 0)
                    {
                        colLinha[0]["Recebimento_IT_Qtde_Pre_Gerado"] = colLinha[0]["Recebimento_IT_Qtde_Pre_Gerado"].DefaultInteger() + dtrLinha["Quantidade"].DefaultInteger();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        private void Limpar_Campos_Itens_Detalhes()
        {
            try
            {
                this.intPecaID = 0;
                this.lblFabricanteRevenda.Text = string.Empty;
                this.txtCodigoFabricante.Text = string.Empty;
                this.lblProdutoRevenda.Text = string.Empty;
                this.txtPecaRevenda.Text = string.Empty;
                this.lblTipoEmbalagemRevenda.Text = string.Empty;
                this.cboEmbalagemCompra.DataSource = null;
                this.txtQuantidadeTotalNotaFiscal.Text = string.Empty;
                this.mskCustoNotaFiscal.Text = string.Empty;
                this.chkImprimirCodigoBarras.Checked = false;
                this.lblCustoEmbalagemRevenda.Text = string.Empty;
                this.lblCustoUnitario.Text = string.Empty;
                this.lblValorTotalItemRevenda.Text = string.Empty;
                this.mskICMSRevenda.Text = string.Empty;
                this.mskIPIRevenda.Text = string.Empty;
                this.lblPercentualDescontoRevenda.Text = "0,00%";
                this.mskValorDescontoRevenda.Text = string.Empty;
                this.mskSubstituicaoRevenda.Text = string.Empty;
                this.txtCodigoClassFiscal.Text = string.Empty;
                this.txtCST.Text = string.Empty;
                this.lblCST.Text = string.Empty;
                this.txtObservacaoRevenda.Text = string.Empty;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Processar_Desfazer()
        {
            try
            {
                int[] intRecebimentoITID = new int[this.dgvNotaFiscalItens.Grid00.SelectedRows.Count];

                for (int intIndice = 0; this.dgvNotaFiscalItens.Grid00.SelectedRows.Count > intIndice; intIndice++)
                {

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[intIndice].Cells["Recebimento_IT_Qtde_Pre_Gerado"].Value.DefaultInteger() != 0)
                    {
                        MessageBox.Show("Só é permitido desfazer o processamento para itens sem pre recebimento gerado.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }

                    if (this.dgvNotaFiscalItens.Grid00.SelectedRows[intIndice].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger() != 0
                        && this.dgvNotaFiscalItens.Grid00.SelectedRows[intIndice].Cells["Recebimento_IT_Qtde_Devolvida"].Value.DefaultInteger()
                           == this.dgvNotaFiscalItens.Grid00.SelectedRows[intIndice].Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger())
                    {
                        MessageBox.Show("Só é permitido desfazer o processamento para itens sem quantidade devolvida.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }

                    intRecebimentoITID[intIndice] = this.dgvNotaFiscalItens.Grid00.SelectedRows[intIndice].Cells["Recebimento_IT_ID"].Value.DefaultInteger();
                }

                DataTable dttRecebimentoItens = new DataTable();
                dttRecebimentoItens.Columns.Add("Pedido_Compra_CT_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Peca_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Quantidade_Recebida", typeof(int));
                dttRecebimentoItens.Columns.Add("Recebimento_IT_Pedido_IT_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Recebimento_IT_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Recebimento_CT_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Quantidade_Embalagem", typeof(int));
                dttRecebimentoItens.Columns.Add("Pedido_Garantia_CT_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Pedido_Garantia_IT_ID", typeof(int));
                dttRecebimentoItens.Columns.Add("Pedido_Garantia_Peca_Substituida", typeof(bool));

                for (int intIndice = 0; intRecebimentoITID.Length > intIndice; intIndice++)
                {
                    foreach (DataRow dtrPedidoCompraItens in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Select("Recebimento_IT_ID = " + intRecebimentoITID[intIndice] +
                                                                                                                " AND (Enum_Status_Comparacao_Item_ID = " + (int)Status_Recebimento_Item.Correto +
                                                                                                                " OR Enum_Status_Comparacao_Item_ID = " + (int)Status_Recebimento_Item.Diferenca + ")"))
                    {
                        dttRecebimentoItens.Rows.Add(dtrPedidoCompraItens["Pedido_Compra_CT_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Peca_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Recebimento_IT_Pedido_IT_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Recebimento_IT_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Recebimento_CT_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Recebimento_IT_Pedido_IT_Qtde"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Pedido_Compra_CT_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Pedido_Compra_IT_ID"].DefaultInteger(),
                                                        dtrPedidoCompraItens["Pedido_Garantia_Peca_Substituida"].DefaultBool());
                    }

                }

                // Excluir relacionamento
                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                busRecebimentoNotaFiscal.Excluir_Recebimento_IT_Pedido_IT((Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue.DefaultInteger(),
                                                                        dttRecebimentoItens,
                                                                        ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID,
                                                                        (byte[])this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Data_Verificacao_Concorrencia"]);

                this.Registro_Alterado = true;

                this.Carregar_Dados();

                this.btnAplicar.Enabled = false;

                return true;

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Atualizar_Valor_Total()
        {
            try
            {
                decimal dcmValorTotalProdutos = 0;
                decimal dcmValorTotalDesconto = 0;

                foreach (DataGridViewRow dgrItensNota in this.dgvNotaFiscalItens.Grid00.Rows)
                {
                    dcmValorTotalProdutos += dgrItensNota.Cells["Recebimento_IT_Custo_Total"].Value.DefaultDecimal();
                    dcmValorTotalDesconto += dgrItensNota.Cells["Recebimento_IT_Valor_Desconto"].Value.DefaultDecimal();
                }

                this.mskValorTotalProdutos.Text = dcmValorTotalProdutos.ToString("#,##0.00");
                this.mskValorTotalDesconto.Text = dcmValorTotalDesconto.ToString("#,##0.00");
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Criar_Relacionamento_Nota_Pedido()
        {
            try
            {
                DataColumn[] dtcNota = { this.dtsRecebimento.Tables["RecebimentoIT"].Columns["Recebimento_IT_ID"] };
                DataColumn[] dtcPedido = { this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Columns["Recebimento_IT_ID"] };

                DataRelation dtrRelacionamento = new DataRelation("Nota_Pedido", dtcNota, dtcPedido);
                this.dtsRecebimento.Relations.Add(dtrRelacionamento);
                this.dgvNotaFiscalItens.PopularGrid(this.dtsRecebimento);

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Consultar_Classificacao_Fiscal()
        {
            try
            {
                frmPesquisaGrid frmPesquisa = new frmPesquisaGrid("Class_Fiscal_CD", "Classificações Fiscais");
                frmPesquisa.Grid.Adicionar_Coluna("Class_Fiscal_CD", "Código", 70, false);
                frmPesquisa.Grid.Adicionar_Coluna("Class_Fiscal_DS", "Descrição", 120, true);

                ClassFiscalBUS busClassFiscal = new ClassFiscalBUS();

                frmPesquisa.Carregar_Grid(busClassFiscal.Consultar_DataSet(Root.Boleano.Verdadeiro).Tables[0]);

                if (frmPesquisa.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    this.txtCodigoClassFiscal.Text = Convert.ToString(frmPesquisa.Registro.Cells["Class_Fiscal_CD"].Value);
                }
                else
                {
                    this.txtCodigoClassFiscal.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Detalhes_do_Item(DataGridViewRow dgrItem)
        {
            try
            {
                if (dgrItem == null)
                {
                    this.Limpar_Campos_Itens_Detalhes();
                    return;
                }

                this.intPecaID = dgrItem.Cells["Peca_ID"].Value.DefaultInteger();

                this.txtFabricanteCD.Text = dgrItem.Cells["Fabricante_CD"].Value.DefaultString();
                this.txtProdutoCD.Text = dgrItem.Cells["Produto_CD"].Value.DefaultString();
                this.txtPecaCD.Text = dgrItem.Cells["Peca_CD"].Value.DefaultString();

                this.txtRecebimento_IT_ID.Text = dgrItem.Cells["Recebimento_IT_ID"].Value.DefaultString();
                this.lblFabricanteRevenda.Text = dgrItem.Cells["Fabricante_NmFantasia"].Value.DefaultString();
                this.txtCodigoFabricante.Text = dgrItem.Cells["Recebimento_IT_NF_CD_Fabricante"].Value.DefaultString();
                this.lblProdutoRevenda.Text = dgrItem.Cells["Produto_DS"].Value.DefaultString();
                this.txtPecaRevenda.Text = dgrItem.Cells["Recebimento_IT_NF_Descricao"].Value.DefaultString();
                this.lblTipoEmbalagemRevenda.Text = dgrItem.Cells["Recebimento_IT_Tipo_Embalagem"].Value.DefaultString();

                this.Selecionar_Embalagem_Compra(dgrItem.Cells["Peca_ID"].Value.DefaultInteger(), dgrItem.Cells["Embalagem_Compra_ID"].Value.DefaultInteger());

                this.txtQuantidadeTotalNotaFiscal.Text = dgrItem.Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultString();
                this.mskCustoNotaFiscal.Text = dgrItem.Cells["Recebimento_IT_Custo_Nota_Fiscal"].Value.DefaultString();
                this.chkImprimirCodigoBarras.Checked = dgrItem.Cells["Pre_Recebimento_IT_Imprimir_Etiqueta"].Value.DefaultBool();
                this.lblCustoEmbalagemRevenda.Text = dgrItem.Cells["Recebimento_IT_Custo_Embalagem"].Value.DefaultDecimal().ToString("#,##0.00");
                this.lblCustoUnitario.Text = dgrItem.Cells["Recebimento_IT_Custo_Unitario"].Value.DefaultDecimal().ToString("#,##0.00");
                this.lblValorTotalItemRevenda.Text = dgrItem.Cells["Recebimento_IT_Custo_Total"].Value.DefaultDecimal().ToString("#,##0.0000");

                this.mskIPIRevenda.Text = this.chkEmitenteOptanteSimples.Checked ? "0" : dgrItem.Cells["Recebimento_IT_Perc_IPI"].Value.DefaultString();
                this.mskICMSRevenda.Text = this.chkEmitenteOptanteSimples.Checked ? "0" : dgrItem.Cells["Recebimento_IT_ICMS_Perc"].Value.DefaultString();
                this.mskValorDescontoRevenda.Text = dgrItem.Cells["Recebimento_IT_Valor_Desconto"].Value.DefaultString();

                if (dgrItem.Cells["Recebimento_IT_Custo_Total"].Value.DefaultDecimal() > 0)
                {
                    this.lblPercentualDescontoRevenda.Text = (dgrItem.Cells["Recebimento_IT_Valor_Desconto"].Value.DefaultDecimal() / dgrItem.Cells["Recebimento_IT_Custo_Total"].Value.DefaultDecimal()).ToString("p");
                }
                else
                {
                    this.lblPercentualDescontoRevenda.Text = "0,00%";
                }

                this.mskSubstituicaoRevenda.Text = dgrItem.Cells["Recebimento_IT_ICMS_ST_Perc"].Value.DefaultString();
                this.txtCodigoClassFiscal.Text = dgrItem.Cells["Recebimento_IT_NCM"].Value.DefaultString();
                this.txtCST.Text = dgrItem.Cells["Recebimento_IT_CST"].Value.DefaultString();
                this.txtObservacaoRevenda.Text = dgrItem.Cells["Recebimento_IT_Observacao"].Value.DefaultString();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Selecionar_Embalagem_Compra(int intPecaID, int intPecaEmbalagemVendaID)
        {
            try
            {
                if (intPecaID != 0)
                {
                    this.Carregar_Combo_Embalagens(intPecaID);

                    if (this.cboEmbalagemCompra.Items.Count > 0)
                    {
                        this.cboEmbalagemCompra.SelectedValue = intPecaEmbalagemVendaID;
                    }
                }
                else
                {
                    this.cboEmbalagemCompra.SelectedIndex = -1;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Validar_Selecionar_CST()
        {
            try
            {
                if (this.txtCST.Text == string.Empty)
                {
                    this.lblCST.Text = string.Empty;
                    return;
                }

                if (this.txtCST.Text.Length < 3)
                {
                    MessageBox.Show("Código de Situação Tributária incompleto. Preencha novamente!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.lblCST.Text = string.Empty;
                    this.txtCST.Focus();
                    return;
                }

                if (this.dttCST.Select("Enum_Sigla = '" + this.txtCST.Text.Substring(0, 1) + "'").Length < 1)
                {
                    MessageBox.Show("Código de Situação Tributária incorreto.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.lblCST.Text = string.Empty;
                    this.txtCST.Focus();
                    return;
                }

                if (this.dttCST.Select("Enum_Sigla = '" + this.txtCST.Text.Substring(0, 1) + "'").Length == 1 && this.dttCST.Select("Enum_Sigla = '" + this.txtCST.Text.Substring(1, 2) + "'").Length < 1)
                {

                    MessageBox.Show("Código de Situação Tributária incorreto.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.lblCST.Text = string.Empty;
                    this.txtCST.Focus();
                    return;
                }

                this.lblCST.Text = this.dttCST.Select("Enum_Sigla = '" + this.txtCST.Text.Substring(0, 1) + "'")[0][1].DefaultString();

                if (this.dttCST.Select("Enum_Sigla = '" + this.txtCST.Text.Substring(1, this.txtCST.Text.Length - 1) + "'").Length == 1)
                {
                    string strDescricao = this.lblCST.Text + " / " + this.dttCST.Select("Enum_Sigla = '" + this.txtCST.Text.Substring(1, this.txtCST.Text.Length - 1) + "'")[0][1].DefaultString();
                    this.lblCST.Text = strDescricao;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Embalagens(int intPecaID)
        {
            try
            {
                Peca_EmbalagemBUS busPecaEmbalagem = new Peca_EmbalagemBUS();
                DataTable dttPecaEmbalagem = busPecaEmbalagem.Consultar_DataTable_Embalagem(intPecaID);
                this.cboEmbalagemCompra.DisplayMember = "Peca_Embalagem_Descricao";
                this.cboEmbalagemCompra.ValueMember = "Peca_Embalagem_ID";
                this.cboEmbalagemCompra.DataSource = dttPecaEmbalagem;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Setar_Imagens_Grid()
        {
            try
            {

                foreach (DataRow dtrRecebimentoIT in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                {
                    dtrRecebimentoIT["Status_Imagem"] = Recebimento_Nota_FiscalBUS.Obter_Imagem_Status_Recebimento_Item((Status_Recebimento_Item)dtrRecebimentoIT["Enum_Status_Comparacao_Item_ID"].ToInteger());
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Habilitar_Campos_Itens_Detalhes()
        {
            try
            {
                bool blnHabilitar = true;

                if (this.dtsRecebimento.Tables.Count > 0
                        && (this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Count > 0
                        || this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Status_ID"].DefaultInteger() != Status_Recebimento.Aguardando_Processamento.DefaultInteger()
                        || this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["NFe_Entrada_XML_ID"].DefaultInteger() != 0))
                {
                    blnHabilitar = false;
                }

                this.txtCodigoFabricante.Enabled = blnHabilitar;
                this.txtPecaRevenda.Enabled = blnHabilitar;
                this.cboEmbalagemCompra.Enabled = blnHabilitar;
                this.txtQuantidadeTotalNotaFiscal.Enabled = blnHabilitar;
                this.mskCustoNotaFiscal.Enabled = blnHabilitar;
                this.chkImprimirCodigoBarras.Enabled = blnHabilitar;
                this.mskICMSRevenda.Enabled = this.chkEmitenteOptanteSimples.Checked ? false : blnHabilitar;
                this.mskIPIRevenda.Enabled = this.chkEmitenteOptanteSimples.Checked ? false : blnHabilitar;
                this.mskValorDescontoRevenda.Enabled = blnHabilitar;
                this.mskSubstituicaoRevenda.Enabled = blnHabilitar;
                this.txtCodigoClassFiscal.Enabled = blnHabilitar;
                this.btnPesquisarClassFiscal.Enabled = blnHabilitar;
                this.btnPesquisarClassFiscal.Enabled = blnHabilitar;
                this.txtCST.Enabled = blnHabilitar;
                this.txtObservacaoRevenda.Enabled = blnHabilitar;
                this.btnPesquisaCodigoFabricante.Enabled = blnHabilitar;

                this.btnInserir.Enabled = blnHabilitar;
                this.btnExcluir.Enabled = blnHabilitar;

                if (blnHabilitar && this.dgvNotaFiscalItens.Grid00.Rows.Count == 0)
                {
                    this.btnExcluir.Enabled = false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Grid_Pedidos(int intFornecedorID, int intPedidoCompraCT, int intRecebimentoCTID)
        {
            try
            {
                if (this.cboTipoRecebimento.SelectedValue == null)
                {
                    return;
                }

                this.dgvPedidoCompras.DataSource = null;

                // Trazer todos os pedidos do fornecedor
                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                DataTable dttPedidos = busRecebimentoNotaFiscal.Consultar_DataSet_Pedidos_Pendentes_Recebimento_Fornecedor(intFornecedorID, (Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue, this.cboDestinatarioLoja.SelectedValue.DefaultInteger(), intRecebimentoCTID).Tables["Recebimento_Pendente"];
                if (dttPedidos != null)
                {
                    this.Atribuir_Coluna_Marcado(ref dttPedidos, intPedidoCompraCT);

                    this.dgvPedidoCompras.DataSource = dttPedidos;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Atribuir_Coluna_Marcado(ref DataTable dttObjeto, int intPedidoCompraCT)
        {
            try
            {
                if (this.dtsRecebimento.Tables.Count <= 0 || this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count <= 0)
                {
                    return;
                }

                if (intPedidoCompraCT != 0)
                {
                    foreach (DataRow dtrPedidoItem in dttObjeto.Rows)
                    {
                        if (dtrPedidoItem["Pedido_ID"].ToInteger() == intPedidoCompraCT)
                        {
                            dtrPedidoItem["Marcado"] = true;
                            break;
                        }
                    }
                }

                foreach (DataRow dtrRecebimentoItem in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows)
                {
                    foreach (DataRow dtrPedidoItem in dttObjeto.Rows)
                    {
                        if (dtrRecebimentoItem["Pedido_Compra_CT_ID"].ToInteger() == dtrPedidoItem["Pedido_ID"].ToInteger())
                        {
                            dtrPedidoItem["Marcado"] = true;
                            break;
                        }
                    }
                }

                dttObjeto.AcceptChanges();
            }
            catch (Exception)
            {
                throw;
            }

        }

        private DataTable Montar_DataTable_Pedido_Compra()
        {
            try
            {
                DataTable dttItens = new DataTable();
                dttItens.Columns.Add("Pedido_Compra_CT_ID", typeof(int));
                dttItens.Columns.Add("Peca_ID", typeof(int));
                dttItens.Columns.Add("Quantidade_Recebida", typeof(int));
                dttItens.Columns.Add("Pedido_Garantia_CT_ID", typeof(int));
                dttItens.Columns.Add("Pedido_Garantia_IT_ID", typeof(int));
                dttItens.Columns.Add("Pedido_Garantia_Peca_Substituida", typeof(bool));

                foreach (DataRow dtrLinha in this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows)
                {
                    if (dtrLinha["Quantidade_Disponivel"].DefaultInteger() != dtrLinha["Quantidade_Disponivel_Original"].DefaultInteger())
                    {
                        dttItens.Rows.Add(dtrLinha["Pedido_Compra_CT_ID"].DefaultInteger(),
                                            dtrLinha["Peca_ID"].DefaultInteger(),
                                            dtrLinha["Quantidade_Disponivel_Original"].DefaultInteger() - dtrLinha["Quantidade_Disponivel"].DefaultInteger(),
                                            dtrLinha["Pedido_Compra_CT_ID"].DefaultInteger(),
                                            dtrLinha["Pedido_Compra_IT_ID"].DefaultInteger(),
                                            dtrLinha["Pedido_Garantia_Peca_Substituida"].DefaultBool());
                    }
                }
                return dttItens;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Montar_DataTable_Classificacao_Fiscal_Informada()
        {
            try
            {


                DataTable dttItens = new DataTable();

                dttItens.Columns.Add("Peca_ID", typeof(int));
                dttItens.Columns.Add("Class_Fiscal_ID", typeof(int));
                dttItens.Columns.Add("Recebimento_IT_CST", typeof(int));
                dttItens.Columns.Add("Recebimento_IT_NCM", typeof(string));
                dttItens.Columns.Add("Recebimento_IT_Data_Liberacao", typeof(DateTime));
                dttItens.Columns.Add("Forn_ID", typeof(int));
                dttItens.Columns.Add("Forn_CD", typeof(string));
                dttItens.Columns.Add("Recebimento_CT_Numero_Nota_Fiscal", typeof(string));

                DataRow dtrFornecedor = this.dtsRecebimento.Tables["Fornecedor"].Rows[0];
                DataRow dtrRecebimentoCT = this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0];

                foreach (DataRow dtrLinha in this.dtsRecebimento.Tables["RecebimentoIT"].Select("Enum_Status_Comparacao_Item_ID = " + Status_Recebimento_Item.Correto.ToInteger()))
                {
                    if (dtrLinha["Peca_Class_Fiscal_CD"].ToString().Equals(dtrLinha["Recebimento_IT_NCM"].DefaultString()) == false)
                    {
                        dttItens.Rows.Add(dtrLinha["Peca_ID"].DefaultInteger(),
                                          dtrLinha["Peca_Class_Fiscal_ID"].DefaultInteger(),
                                          dtrLinha["Recebimento_IT_CST"].DefaultInteger(),
                                          dtrLinha["Recebimento_IT_NCM"].DefaultString(),
                                          dtrLinha["Recebimento_IT_Data_Liberacao"].DefaultDateTime(),
                                          dtrFornecedor["Forn_ID"].DefaultInteger(),
                                          dtrFornecedor["Forn_CD"].DefaultString(),
                                          dtrRecebimentoCT["Recebimento_CT_Numero_Nota_Fiscal"]);
                    }
                }

                return dttItens;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Chamar_Tela_Procurar_Item()
        {
            try
            {
                frmPre_Recebimento_NFe_Itens_Nao_Encontrados frmItensNaoEncontrados;

                frmItensNaoEncontrados = new frmPre_Recebimento_NFe_Itens_Nao_Encontrados(this.cboLojaRecebimento.SelectedValue.ToInteger(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Pre_Recebimento_CT_ID"].Value.DefaultInteger(),
                                                                                                this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Peca_ID"].Value.DefaultInteger(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NF_CD_Fabricante"].Value.DefaultString(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NF_Descricao"].Value.DefaultString(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Tipo_Embalagem"].Value.DefaultString(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Qtde_Restante"].Value.DefaultInteger(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Custo_Nota_Fiscal"].Value.DefaultDecimal(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NCM"].Value.DefaultString(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_ICMS_Perc"].Value.DefaultDecimal(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Perc_IPI"].Value.DefaultDecimal(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_ICMS_ST_Perc"].Value.DefaultDecimal(),
                                                                                                this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_CST"].Value.DefaultString(),
                                                                                                this.lblRecebimentoID.Text,
                                                                                                this.txtOrdemDesembarqueNumero.Text,
                                                                                                this.cboTipoRecebimento.SelectedValue.DefaultInteger(),
                                                                                                this.dtpOrdemDesembarqueData.Value);


                frmItensNaoEncontrados.ShowDialog(this);

                if (frmItensNaoEncontrados.Peca_ID == null)
                {
                    return;
                }

                for (int intIndice = 0; intIndice < frmItensNaoEncontrados.Peca_ID.Length; intIndice++)
                {
                    DataRow[] colLinha = this.dtsRecebimento.Tables["RecebimentoIT"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_ID"].Value.DefaultString());

                    if (colLinha.Length > 0)
                    {
                        PecaBUS busPeca = new PecaBUS();
                        PecaDO dtoPeca = busPeca.Selecionar_Com_Atribuicoes(frmItensNaoEncontrados.Peca_ID[intIndice]);
                        colLinha[0]["Codigo_Mercadocar"] = dtoPeca.Codigo_Mercadocar();
                        colLinha[0]["Peca_ID"] = frmItensNaoEncontrados.Peca_ID[intIndice];
                        colLinha[0]["Fabricante_CD"] = dtoPeca.Fabricante.Codigo;
                        colLinha[0]["Produto_CD"] = dtoPeca.Produto.Codigo;
                        colLinha[0]["Peca_CD"] = dtoPeca.Codigo;
                        colLinha[0]["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Sem_Pedido.DefaultInteger();
                        colLinha[0]["Processado"] = true;

                        if (frmItensNaoEncontrados.Pedido_ID[intIndice].DefaultInteger() != 0)
                        {
                            foreach (DataGridViewRow dgrPedido in this.dgvPedidoCompras.Rows)
                            {
                                if (dgrPedido.Cells["Pedido_ID"].Value.DefaultInteger() == frmItensNaoEncontrados.Pedido_ID[intIndice])
                                {
                                    dgrPedido.Cells["Marcado"].Value = true;
                                    break;
                                }
                            }
                        }

                        this.Preencher_Objeto_Codigo_Fornecedor(frmItensNaoEncontrados.Peca_ID[intIndice]);

                        this.dgvNotaFiscalItens.Grid00.DataSource = this.dtsRecebimento.Tables["RecebimentoIT"];
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Chamar_Tela_Substituir_Item()
        {
            try
            {
                frmPre_Recebimento_NFe_Itens_Nao_Encontrados frmSubstituirItens;

                frmSubstituirItens = new frmPre_Recebimento_NFe_Itens_Nao_Encontrados(this.cboDestinatarioLoja.SelectedValue.ToInteger(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Pre_Recebimento_CT_ID"].Value.DefaultInteger(),
                                                                                      this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger(),
                                                                                      0,
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NF_CD_Fabricante"].Value.DefaultString(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NF_Descricao"].Value.DefaultString(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Tipo_Embalagem"].Value.DefaultString(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Qtde_Restante"].Value.DefaultInteger(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Qtde_Nota_Fiscal"].Value.DefaultInteger(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Custo_Nota_Fiscal"].Value.DefaultDecimal(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_NCM"].Value.DefaultString(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_ICMS_Perc"].Value.DefaultDecimal(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_Perc_IPI"].Value.DefaultDecimal(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_ICMS_ST_Perc"].Value.DefaultDecimal(),
                                                                                      this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_CST"].Value.DefaultString(),
                                                                                      this.lblRecebimentoID.Text,
                                                                                      this.txtOrdemDesembarqueNumero.Text,
                                                                                      this.cboTipoRecebimento.SelectedValue.DefaultInteger(),
                                                                                      this.dtpOrdemDesembarqueData.Value,
                                                                                      "Substituir Item");


                frmSubstituirItens.ShowDialog(this);

                if (frmSubstituirItens.Peca_ID == null)
                {
                    return;
                }

                for (int intIndice = 0; intIndice < frmSubstituirItens.Peca_ID.Length; intIndice++)
                {
                    DataRow[] colLinha = this.dtsRecebimento.Tables["RecebimentoIT"].Select("Recebimento_IT_ID = " + this.dgvNotaFiscalItens.Grid00.CurrentRow.Cells["Recebimento_IT_ID"].Value.DefaultString());

                    if (colLinha.Length > 0)
                    {

                        this.dttpecaCodigosFcornecedorDesativar.Rows.Add(colLinha[0]["Peca_ID"], colLinha[0]["Recebimento_IT_NF_CD_Fabricante"]);

                        PecaBUS busPeca = new PecaBUS();
                        PecaDO dtoPeca = busPeca.Selecionar_Com_Atribuicoes(frmSubstituirItens.Peca_ID[intIndice]);
                        colLinha[0]["Codigo_Mercadocar"] = dtoPeca.Codigo_Mercadocar();
                        colLinha[0]["Peca_ID"] = frmSubstituirItens.Peca_ID[intIndice];
                        colLinha[0]["Fabricante_CD"] = dtoPeca.Fabricante.Codigo;
                        colLinha[0]["Produto_CD"] = dtoPeca.Produto.Codigo;
                        colLinha[0]["Peca_CD"] = dtoPeca.Codigo;
                        colLinha[0]["Enum_Status_Comparacao_Item_ID"] = Status_Recebimento_Item.Sem_Pedido.DefaultInteger();
                        colLinha[0]["Processado"] = true;

                        if (frmSubstituirItens.Pedido_ID[intIndice].DefaultInteger() != 0)
                        {
                            foreach (DataGridViewRow dgrPedido in this.dgvPedidoCompras.Rows)
                            {
                                if (dgrPedido.Cells["Pedido_ID"].Value.DefaultInteger() == frmSubstituirItens.Pedido_ID[intIndice])
                                {
                                    dgrPedido.Cells["Marcado"].Value = true;
                                    break;
                                }
                            }
                        }

                        this.Preencher_Objeto_Codigo_Fornecedor(frmSubstituirItens.Peca_ID[intIndice]);

                        this.dgvNotaFiscalItens.Grid00.DataSource = this.dtsRecebimento.Tables["RecebimentoIT"];
                    }

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Tipo_Recebimento_Obrigatorio()
        {
            try
            {
                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("Selecione um tipo de recebimento!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboTipoRecebimento.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Gerar_Pre_Recebimento()
        {
            try
            {
                Pre_RecebimentoBUS busPreRecebimento = new Pre_RecebimentoBUS();
                DataSet dtsPreRecebimento = busPreRecebimento.Consultar_Pre_Recebimento_CT_Propriedades(0);
                DataSet dtsPreRecebimentoOriginal = dtsPreRecebimento.Copy();

                DataTable dttItensAjusteQuantidade = new DataTable();
                dttItensAjusteQuantidade.Columns.Add("Recebimento_IT_ID", typeof(int));
                dttItensAjusteQuantidade.Columns.Add("Quantidade", typeof(int));
                dttItensAjusteQuantidade.Columns.Add("Recebimento_IT_Pedido_IT_ID", typeof(int));
                dttItensAjusteQuantidade.Columns.Add("Pre_Recebimento_Gerado", typeof(bool));

                this.Montar_Pre_Recebimento_CT(dtsPreRecebimento);

                this.Montar_Pre_Recebimento_IT(dtsPreRecebimento, dttItensAjusteQuantidade);

                if (dtsPreRecebimento.Tables["Itens"].Rows.Count > 0)
                {
                    if (busPreRecebimento.Trata_Dataset_E_Recebimento(dtsPreRecebimento, dtsPreRecebimentoOriginal, this.dtsRecebimento.Tables["RecebimentoCT"], dttItensAjusteQuantidade, false, (Tipo_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger(), (byte[])this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Data_Verificacao_Concorrencia"]))
                    {
                        MessageBox.Show("Criado o pré-recebimento nº " + dtsPreRecebimento.Tables["Capa"].Rows[0]["Pre_Recebimento_CT_ID"].DefaultString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Não foi encontrado nenhum item para ser gerado o pré-recebimento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Carregar_Dados();

                this.Habilitar_Botao_Pre_Recebimento();
                this.Habilitar_Botao_Preparar_Mercadoria();

                this.Registro_Alterado = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Exibir_Grid_Detalhes_Formulario()
        {
            try
            {
                this.Mudar_Cor_Grid_Itens_Diferenca_Detalhe();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Status_Recebimento Retornar_Status_Recebimento(bool blnProcessado)
        {
            try
            {
                int intQtdeRelacionamento = 0;
                if (this.dtsRecebimento.Tables.Count > 0)
                {
                    intQtdeRelacionamento = this.dtsRecebimento.Tables["Nota_Pedido_Itens"].Rows.Count;
                }

                int intQtdeTotal = 0;
                int intQtdePreGerado = 0;
                int intQtdeDevolvida = 0;

                foreach (DataRow dtrItem in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                {
                    intQtdeTotal += dtrItem["Recebimento_IT_Qtde_Nota_Fiscal"].DefaultInteger();
                    intQtdePreGerado += dtrItem["Recebimento_IT_Qtde_Pre_Gerado"].DefaultInteger();
                    intQtdeDevolvida += dtrItem["Recebimento_IT_Qtde_Devolvida"].DefaultInteger();
                }

                Recebimento_NFeBUS busRecebimentoNotaFiscal = new Recebimento_NFeBUS();
                return busRecebimentoNotaFiscal.Retornar_Status_Recebimento(blnProcessado,
                                                                            this.dtoPropriedades.ID,
                                                                            intQtdeRelacionamento,
                                                                            intQtdeTotal,
                                                                            intQtdePreGerado,
                                                                            intQtdeDevolvida,
                                                                            this.intOrdemDesembarqueNFID,
                                                                            this.dtsRecebimento.Tables["RecebimentoIT"],
                                                                            (Tipo_Recebimento)this.cboTipoRecebimento.SelectedValue.DefaultInteger());

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Alterar_Imprimir_Codigo_De_Barras_Recebimento_IT(Int32 intRecebimentoITID, bool blnImprimirCodigoBarras)
        {
            try
            {
                Recebimento_ITBUS busRecebimentoIT = new Recebimento_ITBUS();
                busRecebimentoIT.Atualizar_Imprimir_Etiqueta(intRecebimentoITID, blnImprimirCodigoBarras);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Marcar_Pedido_Do_XML()
        {
            try
            {
                if (this.dtsRecebimento != null
                    && this.dtsRecebimento.Tables.Count > 0
                    && this.dtsRecebimento.Tables["RecebimentoIT"].Rows.Count > 0)
                {
                    foreach (DataRow dtrLinha in this.dtsRecebimento.Tables["RecebimentoIT"].Rows)
                    {
                        int intNumeroPedido = 0;
                        if (!int.TryParse(dtrLinha["Recebimento_IT_Numero_Pedido"].DefaultString(), out intNumeroPedido))
                        {
                            return;
                        }

                        if (dtrLinha["Recebimento_IT_Numero_Pedido"].DefaultInteger() != 0)
                        {
                            foreach (DataGridViewRow dgrPedido in this.dgvPedidoCompras.Rows)
                            {
                                if (dgrPedido.Cells["Pedido_ID"].Value.DefaultInteger() == dtrLinha["Recebimento_IT_Numero_Pedido"].DefaultInteger())
                                {
                                    dgrPedido.Cells["Marcado"].Value = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Calcular_Desconto()
        {
            try
            {
                if (this.mskCustoNotaFiscal.Text.DefaultDecimal() * this.txtQuantidadeTotalNotaFiscal.Text.DefaultInteger() > 0)
                {
                    this.lblPercentualDescontoRevenda.Text = (this.mskValorDescontoRevenda.Text.DefaultDecimal() / (this.mskCustoNotaFiscal.Text.DefaultDecimal() * this.txtQuantidadeTotalNotaFiscal.Text.DefaultInteger())).ToString("p");
                }
                else
                {
                    this.lblPercentualDescontoRevenda.Text = "0,00%";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Mercadoria_Preparada(Int32 intRecebimentoITID)
        {
            try
            {
                Recebimento_ITBUS busRecebimentoIT = new Recebimento_ITBUS();

                if (busRecebimentoIT.Validar_Mercadoria_Preparada(intRecebimentoITID))
                {
                    MessageBox.Show("Não é possível realizar a devolução. A peça selecionada teve a preparação de mercadoria realizada. Recarregue novamente os dados da tela.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Aba Pré-Recebimento     "

        private void Habilitar_Botao_Preparar_Mercadoria()
        {
            try
            {
                bool blnHabilitar = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Preparar.ToString())
                                                    && this.Validar_Geracao_Preparacao_Mercadoria(false);

                this.btnPrepararMercadoria.Enabled = blnHabilitar;

                if (blnHabilitar)
                {
                    if (this.tbcHerdado.TabPages.Count > 2)
                    {
                        this.tbcHerdado.TabPages[this.tbpPreRecebimento.Name].Enabled = blnHabilitar;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Geracao_Preparacao_Mercadoria(bool blnExibirMensagem)
        {
            try
            {
                if (this.dtsRecebimento.Tables.Count == 0)
                {
                    return false;
                }

                Pre_RecebimentoBUS busPreRecebimento = new Pre_RecebimentoBUS();
                DataSet dtsPreparacaoMercadoriasPropriedades = busPreRecebimento.Consultar_DataSet_Volume_Preparacao(this.cboLojaRecebimento.SelectedValue.DefaultInteger(), this.dtsRecebimento.Tables["Fornecedor"].Rows[0]["Forn_ID"].DefaultInteger());

                if (dtsPreparacaoMercadoriasPropriedades == null)
                {
                    if (blnExibirMensagem)
                    {
                        MessageBox.Show("Não existe nenhum pré-recebimento pendente de preparação de mercadoria", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Grupo()
        {
            try
            {
                this.dgvGrupoPreRecebimento.AutoGenerateColumns = false;
                this.dgvGrupoPreRecebimento.Adicionar_Coluna("Marcado", " ", 25, false, Tipo_Coluna.CheckBox, true);
                this.dgvGrupoPreRecebimento.Adicionar_Coluna("Pre_Recebimento_Grupo_ID", "Grupo", 80, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvGrupoPreRecebimento.Adicionar_Coluna("Enum_Tipo_Pre_Recebimento", "Tipo", 100, false);
                this.dgvGrupoPreRecebimento.Adicionar_Coluna_Data_Tempo("Pre_Recebimento_Grupo_Data_Preparacao", "Preparação", 100, false);
                this.dgvGrupoPreRecebimento.Adicionar_Coluna("Usuario_Nome_Completo", "Preparado por", 300, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Pre_Recebimento()
        {
            try
            {
                this.dgvPreRecebimento.AutoGenerateColumns = false;
                this.dgvPreRecebimento.Adicionar_Coluna("Marcado", " ", 25, false, Tipo_Coluna.CheckBox, true);
                this.dgvPreRecebimento.Adicionar_Coluna("Pre_Recebimento_CT_ID", "Pré", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvPreRecebimento.Adicionar_Coluna("Pre_Recebimento_Grupo_ID", "Grupo", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvPreRecebimento.Adicionar_Coluna("Recebimento_CT_Numero_Nota_Fiscal", "Nota", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvPreRecebimento.Adicionar_Coluna_Inteiro("Recebimento_CT_Numero_Serie", "Série", 50, false);
                this.dgvPreRecebimento.Adicionar_Coluna_Data_Tempo("Pre_Recebimento_CT_Data_Criacao", "Criação", 100, false);
                this.dgvPreRecebimento.Adicionar_Coluna("Usuario_Nome_Completo_Criacao", "Criado por", 150, false);
                this.dgvPreRecebimento.Adicionar_Coluna_Inteiro("Quantidade_Volumes", "Volumes", 50, false);
                this.dgvPreRecebimento.Adicionar_Coluna_Data_Tempo("Pre_Recebimento_CT_Data_Conferencia_NF", "Conferência", 100, false);
                this.dgvPreRecebimento.Adicionar_Coluna("Usuario_Nome_Completo_Conferencia_NF", "Conferido por", 150, false);
                this.dgvPreRecebimento.Adicionar_Coluna("Enum_Status_ID");
                this.dgvPreRecebimento.Adicionar_Coluna("Status_Pre_Recebimento_CT", "Status", 70, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Lote_Entrada()
        {
            try
            {
                this.dgvItensPreRecebimento.AutoGenerateColumns = false;
                this.dgvItensPreRecebimento.Adicionar_Coluna("Fabricante_CD", "Fab.", 37, false);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Produto_CD", "Pro.", 37, false);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Peca_CD", "Pec.", 37, false);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Peca_CDFabricante", "Código Item Fabricante", 150, false);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Peca_DSTecnica", "Descrição", 220, true);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Pre_Recebimento_CT_ID", "Pré", 80, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Pedido_Compra_CT_ID", "Pedido", 80, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvItensPreRecebimento.Adicionar_Coluna_Inteiro("Pre_Recebimento_IT_Qtde_NF", "Quantidade NF", 100, false);
                this.dgvItensPreRecebimento.Adicionar_Coluna_Inteiro("Pre_Recebimento_IT_Qtde_Fisico", "Quantidade Físico", 100, false);
                this.dgvItensPreRecebimento.Adicionar_Coluna("Peca_ID");
                this.dgvItensPreRecebimento.Adicionar_Coluna("Pre_Recebimento_Grupo_ID");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Configurar_Grid_Volume_Conferido()
        {
            try
            {
                this.dgvItensVolumes.AutoGenerateColumns = false;
                this.dgvItensVolumes.Adicionar_Coluna("Pre_Recebimento_Grupo_ID", "Grupo", 80, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvItensVolumes.Adicionar_Coluna("Pre_Recebimento_Volume_CT_Numero", "Volume", 60, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.TopRight);
                this.dgvItensVolumes.Adicionar_Coluna("Status_Pre_Recebimento_Volume_CT", "Status", 150, false);
                this.dgvItensVolumes.Adicionar_Coluna("Usuario_Nome_Completo_Conferencia_Fisica", "Usuário Separação", 250, true);
                this.dgvItensVolumes.Adicionar_Coluna_Inteiro("Pre_Recebimento_Volume_IT_Qtde", "Qtde. Separada", 120, false);
                this.dgvItensVolumes.Adicionar_Coluna_Inteiro("Pre_Recebimento_Volume_IT_Qtde_Guardada", "Qtde. Guardada", 120, false);
                this.dgvItensVolumes.Adicionar_Coluna_Inteiro("Pre_Recebimento_Volume_IT_Qtde_Cross", "Qtde. Cross", 100, false);
                this.dgvItensVolumes.Adicionar_Coluna("Peca_ID");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Criar_Colunas_DataTable_Codigo_Fornecedor()
        {
            try
            {
                this.dttpecaCodigosFcornecedorDesativar.Columns.Add("Peca_id", typeof(int));
                this.dttpecaCodigosFcornecedorDesativar.Columns.Add("Recebimento_IT_NF_CD_Fabricante", typeof(string));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Grupo()
        {
            try
            {
                if (this.dgvGrupoPreRecebimento.Rows.Count > 0)
                {
                    this.chkMarcarTodosGrupo.Checked = false;
                    this.dgvGrupoPreRecebimento.DataSource = null;
                    this.dgvGrupoPreRecebimento.Rows.Clear();
                }
                if (this.dgvPreRecebimento.Rows.Count > 0)
                {
                    this.chkMarcarTodosPreRecebimento.Checked = false;
                    this.dgvPreRecebimento.DataSource = null;
                    this.dgvPreRecebimento.Rows.Clear();
                }
                if (this.dgvItensPreRecebimento.Rows.Count > 0)
                {
                    this.dgvItensPreRecebimento.DataSource = null;
                    this.dgvItensPreRecebimento.Rows.Clear();
                }
                if (this.dgvItensVolumes.Rows.Count > 0)
                {
                    this.dgvItensVolumes.DataSource = null;
                    this.dgvItensVolumes.Rows.Clear();
                }

                this.txtVolumesTotalConferido.Text = string.Empty;
                this.txtVolumesTotalItens.Text = string.Empty;
                this.txtVolumesPendentes.Text = string.Empty;
                this.txtVolumesExcedentes.Text = string.Empty;

                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                DataSet dtsConsulta = busRecebimentoNotaFiscal.Consultar_DataSet_Dados_Pre_Recebimento(this.intFornecedorID,
                                                                                                        this.cboNotaFiscalNumero.Text,
                                                                                                        this.Montar_Filtro_Grupo(),
                                                                                                        this.Montar_Filtro_Pre_Recebimento());

                this.dgvGrupoPreRecebimento.DataSource = dtsConsulta.Tables["Pre_Recebimento_Grupo"];

                if (dtsConsulta.Tables["Pre_Recebimento_Grupo"].Rows.Count > 0)
                {
                    this.dgvPreRecebimento.DataSource = dtsConsulta.Tables["Pre_Recebimento_CT"];
                    this.chkMarcarTodosGrupo.Checked = true;
                    this.Tratar_Marcar_Desmarcar_Todos_Grupos(this.chkMarcarTodosGrupo.Checked);
                    this.chkMarcarTodosPreRecebimento.Checked = true;
                    this.Tratar_Marcar_Desmarcar_Todos_Pre_Recebimento(this.chkMarcarTodosPreRecebimento.Checked);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Pre_Recebimento()
        {
            try
            {
                if (this.dgvPreRecebimento.Rows.Count > 0)
                {
                    this.chkMarcarTodosPreRecebimento.Checked = false;
                    this.dgvPreRecebimento.DataSource = null;
                    this.dgvPreRecebimento.Rows.Clear();
                }
                if (this.dgvItensPreRecebimento.Rows.Count > 0)
                {
                    this.dgvItensPreRecebimento.DataSource = null;
                    this.dgvItensPreRecebimento.Rows.Clear();
                    this.dgvItensVolumes.DataSource = null;
                    this.dgvItensVolumes.Rows.Clear();
                }

                this.txtVolumesTotalConferido.Text = string.Empty;
                this.txtVolumesTotalItens.Text = string.Empty;
                this.txtVolumesPendentes.Text = string.Empty;
                this.txtVolumesExcedentes.Text = string.Empty;

                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                DataSet dtsConsulta = busRecebimentoNotaFiscal.Consultar_DataSet_Dados_Pre_Recebimento(this.intFornecedorID,
                                                                                                        this.cboNotaFiscalNumero.Text,
                                                                                                        this.Montar_Filtro_Grupo(),
                                                                                                        this.Montar_Filtro_Pre_Recebimento());

                this.dgvPreRecebimento.DataSource = dtsConsulta.Tables["Pre_Recebimento_CT"];
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Lote_Entrada()
        {
            try
            {
                if (this.dgvItensPreRecebimento.Rows.Count > 0)
                {
                    this.dgvItensPreRecebimento.DataSource = null;
                    this.dgvItensPreRecebimento.Rows.Clear();
                    this.dgvItensVolumes.DataSource = null;
                    this.dgvItensVolumes.Rows.Clear();
                }

                Recebimento_Nota_FiscalBUS busRecebimentoNotaFiscal = new Recebimento_Nota_FiscalBUS();
                DataSet dtsConsulta = busRecebimentoNotaFiscal.Consultar_DataSet_Dados_Pre_Recebimento(this.intFornecedorID,
                                                                                                        this.cboNotaFiscalNumero.Text,
                                                                                                        this.Montar_Filtro_Grupo(),
                                                                                                        this.Montar_Filtro_Pre_Recebimento());

                this.dgvItensPreRecebimento.DataSource = dtsConsulta.Tables["Pre_Recebimento_IT"];
                this.bdsVolumesPreRecebimento.DataSource = dtsConsulta.Tables["Pre_Recebimento_Volume"];

                this.txtVolumesTotalConferido.Text = string.Empty;
                this.txtVolumesTotalItens.Text = string.Empty;
                this.txtVolumesPendentes.Text = string.Empty;
                this.txtVolumesExcedentes.Text = string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Volume_Conferido()
        {
            try
            {
                if (this.dgvItensPreRecebimento.SelectedRows.Count > 0)
                {
                    this.bdsVolumesPreRecebimento.Filter = "Pre_Recebimento_Grupo_ID = " + this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_Grupo_ID"].Value.DefaultString()
                                                    + " AND Peca_ID = " + this.dgvItensPreRecebimento.SelectedRows[0].Cells["Peca_ID"].Value.DefaultString();

                    this.dgvItensVolumes.DataSource = this.bdsVolumesPreRecebimento.DataSource;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Marcar_Check_Todos_Grupo()
        {
            try
            {
                bool blnMarcado = true;

                foreach (DataGridViewRow dgrLinha in this.dgvGrupoPreRecebimento.Rows)
                {
                    if (!Convert.ToBoolean(dgrLinha.Cells["Marcado"].Value))
                    {
                        blnMarcado = false;
                        break;
                    }
                }

                return blnMarcado;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Marcar_Check_Todos_Pre_Recebimento()
        {
            try
            {
                bool blnMarcado = true;

                foreach (DataGridViewRow dgrLinha in this.dgvPreRecebimento.Rows)
                {
                    if (!Convert.ToBoolean(dgrLinha.Cells["Marcado"].Value))
                    {
                        blnMarcado = false;
                        break;
                    }
                }

                return blnMarcado;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Trata_Datagrid_Itens_Pre_Recebimento()
        {
            try
            {
                this.Carregar_Volume_Conferido();

                if (this.dgvItensPreRecebimento.SelectedRows.Count > 0)
                {
                    this.txtVolumesTotalConferido.Text = Convert.ToString(this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_Fisico"].Value.DefaultInteger());
                    this.txtVolumesTotalItens.Text = Convert.ToString(this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_NF"].Value.DefaultInteger());

                    if (this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_NF"].Value.DefaultInteger() > this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_Fisico"].Value.DefaultInteger())
                    {
                        this.txtVolumesPendentes.Text = Convert.ToString(this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_NF"].Value.DefaultInteger() - this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_Fisico"].Value.DefaultInteger());
                        this.txtVolumesExcedentes.Text = "0";
                    }
                    else
                    {
                        this.txtVolumesPendentes.Text = "0";
                        this.txtVolumesExcedentes.Text = Convert.ToString(this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_Fisico"].Value.DefaultInteger() - this.dgvItensPreRecebimento.SelectedRows[0].Cells["Pre_Recebimento_IT_Qtde_NF"].Value.DefaultInteger());
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Marcar_Desmarcar_Todos_Grupos(bool blnMarcarDesmarcar)
        {
            try
            {
                foreach (DataGridViewRow dgrLinha in this.dgvGrupoPreRecebimento.Rows)
                {
                    dgrLinha.Cells["Marcado"].Value = blnMarcarDesmarcar;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Marcar_Desmarcar_Todos_Pre_Recebimento(bool blnMarcarDesmarcar)
        {
            try
            {
                foreach (DataGridViewRow dgrLinha in this.dgvPreRecebimento.Rows)
                {
                    dgrLinha.Cells["Marcado"].Value = blnMarcarDesmarcar;
                }

                this.Carregar_Lote_Entrada();
                this.Carregar_Volume_Conferido();

                this.Trata_Datagrid_Itens_Pre_Recebimento();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        private bool Validar_Data_Emissao_Parcela()
        {
            try
            {
                DateTime dtmData = new DateTime(1900, 1, 1);

                foreach (DataGridViewRow dgrLinha in this.dgvParcelas.Rows)
                {
                    if (dtmData == new DateTime(1900, 1, 1) || (dtmData != new DateTime(1900, 1, 1) && dtmData > dgrLinha.Cells["Data_Vencimento"].Value.DefaultDateTime()))
                    {
                        dtmData = dgrLinha.Cells["Data_Vencimento"].Value.DefaultDateTime().ToShortDateString().DefaultDateTime();
                    }
                }

                if (this.dgvParcelas.Rows.Count > 0 && this.dtpNotaFiscalEmissao.Value.ToShortDateString().DefaultDateTime() > dtmData)
                {
                    MessageBox.Show("A parcela tem a data de vencimento menor que a data de emissão da nota fiscal. Entre em contato com o Fornecedor.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.dtpNotaFiscalEmissao.Focus();
                    return false;
                }

                return this.Validar_Condicao_Pagamento_Tela_E_Calculado();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Métodos Públicos    "

        public void Verificar_Mudancas()
        {
            try
            {
                if (this.dtsRecebimento.Tables.Count == 0 || this.dtsRecebimentoTemporario.Tables.Count == 0)
                {
                    return;
                }

                if (this.dtsRecebimento.Tables["RecebimentoIT"].Columns.Contains("Status_Imagem") == false)
                {
                    return;
                }

                if (this.dtsRecebimentoTemporario.Tables["RecebimentoIT"].Columns.Contains("Status_Imagem"))
                {
                    this.dtsRecebimentoTemporario.Tables["RecebimentoIT"].Columns.Remove("Status_Imagem");
                }

                DataSet dtsRecebimentoClone = this.dtsRecebimento.Copy();
                DataSet dtsRecebimentoTemporarioClone = this.dtsRecebimentoTemporario.Copy();

                dtsRecebimentoClone.Tables["RecebimentoIT"].Columns.Remove("Status_Imagem");
                dtsRecebimentoClone.Tables["RecebimentoIT"].Columns.Remove("Pre_Recebimento_IT_Imprimir_Etiqueta");
                dtsRecebimentoTemporarioClone.Tables["RecebimentoIT"].Columns.Remove("Pre_Recebimento_IT_Imprimir_Etiqueta");

                this.btnAplicar.Enabled = (dtsRecebimentoClone.GetXml() != dtsRecebimentoTemporarioClone.GetXml());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override bool Efetuar_Alteracao()
        {

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!this.Validar_Formulario())
                    return false;

                if (!this.Validar_Condicao_Pagamento(true))
                    return false;

                this.Preencher_Objeto_Recebimento(false);

                DataTable dttItens = this.Montar_DataTable_Pedido_Compra();
                DataTable dttClassificaoFiscalInformada = this.Montar_DataTable_Classificacao_Fiscal_Informada();

                Peca_Codigo_FornecedorBUS busCodigoFornecedor = new Peca_Codigo_FornecedorBUS();

                foreach (DataRow dtrPecaCodigoFcornecedorDesativar in this.dttpecaCodigosFcornecedorDesativar.Rows)
                {
                    busCodigoFornecedor.Desativar_Por_Substuicao(dtrPecaCodigoFcornecedorDesativar["Peca_ID"].ToInteger(), dtrPecaCodigoFcornecedorDesativar["Recebimento_IT_NF_CD_Fabricante"].ToString());
                }
                this.dttpecaCodigosFcornecedorDesativar.Clear();

                foreach (Peca_Codigo_FornecedorDO dtoPecaCodigoFornecedorIncluir in this.dtoPecaCodigoFornecedorIncluir.Codigo_Fornecedor)
                {
                    busCodigoFornecedor.Incluir(dtoPecaCodigoFornecedorIncluir);
                }
                this.dtoPecaCodigoFornecedorIncluir.Codigo_Fornecedor.Clear();

                Recebimento_NFeBUS busRecebimentoNFe = new Recebimento_NFeBUS();
                if (this.dtoPropriedades.ID == 0)
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                        || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        busRecebimentoNFe.Tratar_DataObject_Incluir_E_Atualizar_Garantia(this.dtoPropriedades, dttItens);
                    }
                    else
                    {
                        busRecebimentoNFe.Tratar_DataObject_Incluir(this.dtoPropriedades, dttItens, dttClassificaoFiscalInformada, null,
                                                                                 ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID, (Tipo_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger());
                    }

                    this.intRecebimentoCTID = this.dtoPropriedades.ID;
                }
                else
                {
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                           || this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        busRecebimentoNFe.Tratar_DataObject_Alterar_E_Atualizar_Garantia(this.dtoPropriedades, this.dttItensExcluidos, dttItens);
                    }
                    else
                    {
                        busRecebimentoNFe.Tratar_DataObject_Alterar(this.dtoPropriedades, this.dttItensExcluidos, dttItens, dttClassificaoFiscalInformada, null,
                                                                                     ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID, (Tipo_Recebimento)this.dtsRecebimento.Tables["RecebimentoCT"].Rows[0]["Enum_Tipo_Recebimento1_ID"].DefaultInteger());
                    }
                }

                this.Registro_Alterado = true;

                this.Carregar_Dados();

                this.btnAplicar.Enabled = false;

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public override bool Validar_Formulario()
        {
            try
            {
                // Nota Fiscal
                if (this.cboNotaFiscalNumero.Text == string.Empty)
                {
                    MessageBox.Show("É necessário informar o numero da Nota Fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboNotaFiscalNumero.Focus();
                    return false;
                }

                if (this.lblNotaFiscalSerie.Text == string.Empty)
                {
                    MessageBox.Show("É necessário informar a série da Nota Fiscal. Vá até a tela de Ordem de Desembarque.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    return false;
                }
                if (this.txtChaveAcesso.Text != string.Empty && this.txtChaveAcesso.Text.Length < this.txtChaveAcesso.MaxLength)
                {
                    MessageBox.Show("A chave informada é menor que " + this.txtChaveAcesso.MaxLength.ToString() + " caracteres.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.txtChaveAcesso.Focus();
                    return false;
                }

                DBUtil objUtil = new DBUtil();
                if (this.dtpNotaFiscalEmissao.Value > objUtil.Obter_Data_do_Servidor(false, TipoServidor.Central))
                {
                    MessageBox.Show("A data de emissão maior que a data atual.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.dtpNotaFiscalEmissao.Focus();
                    return false;
                }

                int intDiasMaximoRecebimentoNF = Root.ParametrosProcesso.Retorna_Valor_Parametro("RECEBIMENTO", "QTDE_DIAS_MAXIMO_RECEBIMENTO_NOTA_FISCAL").DefaultInteger();
                DateTime dtmDataMinimaRecebimentoNF = DateTime.Now;

                if (this.dtpNotaFiscalEmissao.Value < dtmDataMinimaRecebimentoNF.AddDays((intDiasMaximoRecebimentoNF * (-1))))
                {
                    MessageBox.Show("A data de emissão está menor que o permitido. Não é possível receber uma nota fiscal com menos de " + intDiasMaximoRecebimentoNF.DefaultString() + " dias da data atual.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.dtpNotaFiscalEmissao.Focus();
                    return false;
                }

                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == Tipo_Recebimento.Encomenda_Pedido.ToInteger())
                {
                    if (this.cboNaturezaOperacao.SelectedValue.DefaultInteger() == 0)
                    {
                        MessageBox.Show("É necessário informar a natureza da operação.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.cboNaturezaOperacao.Focus();
                        return false;
                    }

                    if (this.cboNaturezaFinanceira.SelectedValue.DefaultInteger() == 0)
                    {
                        MessageBox.Show("É necessário informar a natureza financeira.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.cboNaturezaFinanceira.Focus();
                        return false;
                    }

                    if (this.cboModelo.SelectedValue.DefaultInteger() == 0)
                    {
                        MessageBox.Show("É necessário informar o modelo da nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.cboModelo.Focus();
                        return false;
                    }

                    if (this.cboTipoOperacao.SelectedValue.DefaultInteger() == 0)
                    {
                        MessageBox.Show("É necessário informar o tipo de operação.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.cboTipoOperacao.Focus();
                        return false;
                    }

                    if (this.dtoCondicaoPagamento != null && this.dtoCondicaoPagamento.Gerar_Contas_A_Pagar && this.dgvParcelas.Rows.Count == 0)
                    {
                        MessageBox.Show("Não foram gerados as parcelas para esta nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.btnGerarParcelas.Focus();
                        return false;
                    }

                    decimal dcmDiferencaValorNotaFiscalMenor = 0;
                    decimal dcmDiferencaValorNotaFiscalMaior = 0;

                    this.Localizar_Valor_Parametro_Diferenca_Nota_Fiscal(ref dcmDiferencaValorNotaFiscalMenor, ref dcmDiferencaValorNotaFiscalMaior);

                    // Cobrança
                    if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Bonificacao_Pedido.DefaultInteger()
                       && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Bonificacao_Vale.DefaultInteger()
                       && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                       && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                    {
                        decimal dcmSoma = 0;

                        foreach (DataGridViewRow dgrParcela in this.dgvParcelas.Rows)
                        {
                            dcmSoma += dgrParcela.Cells["Parcela_Valor"].Value.DefaultDecimal();
                        }

                        if (((dcmSoma >= this.mskValorTotalNotaFiscal.Text.DefaultDecimal() - dcmDiferencaValorNotaFiscalMenor)
                             && (dcmSoma <= this.mskValorTotalNotaFiscal.Text.DefaultDecimal() + dcmDiferencaValorNotaFiscalMaior)) == false)
                        {
                            MessageBox.Show("Soma do valor das parcelas difere do Valor Total da Nota Fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                            this.mskValorTotalNotaFiscal.Focus();
                            return false;
                        }
                    }


                    if (this.mskValorTotalProdutos.Text.DefaultDecimal() == 0)
                    {
                        MessageBox.Show("Valor total dos produtos não pode ser Zero.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.mskValorTotalProdutos.Focus();
                        return false;
                    }

                    // Totais
                    if (this.mskValorTotalNotaFiscal.Text.DefaultDecimal() == 0)
                    {
                        MessageBox.Show("Valor total da nota fiscal não pode ser Zero.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        this.mskValorTotalNotaFiscal.Focus();
                        return false;
                    }
                    else
                    {
                        decimal dcmValorTotalNota =
                            this.mskValorTotalProdutos.Text.DefaultDecimal() +
                            this.mskValorTotalOutros.Text.DefaultDecimal() +
                            this.mskValorTotalSeguro.Text.DefaultDecimal() +
                            this.mskValorTotalFrete.Text.DefaultDecimal() +
                            this.mskValorTotalICMSSubstituicao.Text.DefaultDecimal() +
                            this.mskValorTotalIPI.Text.DefaultDecimal() -
                            this.mskValorTotalDesconto.Text.DefaultDecimal();

                        if ((dcmValorTotalNota < this.mskValorTotalNotaFiscal.Text.DefaultDecimal() - dcmDiferencaValorNotaFiscalMenor)
                                || (dcmValorTotalNota > this.mskValorTotalNotaFiscal.Text.DefaultDecimal() + dcmDiferencaValorNotaFiscalMaior))
                        {
                            MessageBox.Show("Valor total da nota fiscal diferente do somatório.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                            this.mskValorTotalNotaFiscal.Focus();
                            return false;
                        }

                    }

                    if (!this.Validar_Data_Emissao_Parcela())
                    {
                        return false;
                    }
                }

                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar o tipo de recebimento.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboTipoRecebimento.Focus();
                    return false;
                }

                // Destinatário
                if (this.cboDestinatarioLoja.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar a loja de destino.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboDestinatarioLoja.Focus();
                    return false;
                }

                // Loja de Entrega
                if (this.cboLojaRecebimento.SelectedValue.DefaultInteger() == 0)
                {
                    MessageBox.Show("É necessário informar a loja de entrega.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.cboLojaRecebimento.Focus();
                    return false;
                }

                // Emitente
                if (this.intFornecedorID == 0)
                {
                    MessageBox.Show("É necessário informar o emissor da nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                    this.txtEmitenteCNPJCPF.Focus();
                    return false;
                }

                //fazer a validação desconsiderando notas do tipo garantia e bonificação
                if (this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Bonificacao_Pedido.DefaultInteger()
                    && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Bonificacao_Vale.DefaultInteger()
                    && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Pedido.DefaultInteger()
                    && this.cboTipoRecebimento.SelectedValue.DefaultInteger() != Tipo_Recebimento.Garantia_Vale.DefaultInteger())
                {
                    if (this.mskValorTotalNotaFiscal.Text.DefaultDecimal() != this.Somar_Parcelas())
                    {
                        MessageBox.Show("A soma das parcelas não pode ser diferente que o total da nota fiscal.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.tbcHerdado.SelectedTab = this.tbpDadosNotaFiscal;
                        return false;
                    }
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private decimal Somar_Parcelas()
        {
            decimal somaParcelas = 0;
            foreach (DataGridViewRow item in this.dgvParcelas.Rows)
            {
                somaParcelas += item.Cells["Parcela_Valor"].Value.DefaultDecimal();
            }

            return somaParcelas;
        }

        #endregion

        #region "   Não implementado    "

        public object DataObject
        {
            get { return null; }
        }

        #endregion
    }
}
