// -----------------------------------------------------------------------
// <copyright file="frmSolicitacao_Estorno_Pagamento_Propriedades.cs" company="MercadoCAR"> MercadoCar </copyright>
// <userName> cbarbosa </userName>
// <created> 10/31/2013 11:11:26 AM </created>
// -----------------------------------------------------------------------

using Mercadocar.Enumerados;
using Mercadocar.Herancas;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.Datable;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.Interfaces;
using Mercadocar.ObjetosNegocio.DataObject;
using Mercadocar.RegrasNegocio;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Mercadocar.Formularios
{
    public sealed partial class frmSolicitacao_Pagamento_Propriedades : frmPropriedades, IfrmPropriedades
    {
        #region "   Declarações        "

        private const int DIAS_MINIMOS_SOLICITACAO = -15;
        private DataTable dttSolicitacaoPagamento = new DataTable();
        private DataTable dttSolicitacaoPagamentoOriginal = new DataTable();

        private int intSolicitacaoPagamento = 0;
        private int intOrigemID = 0;

        #endregion

        #region "   Propriedades       "

        private DataRow Solicitacao_Pagamento
        {
            get { return this.dttSolicitacaoPagamento.Rows[0]; }
        }

        #endregion

        #region "   Construtor         "
        public frmSolicitacao_Pagamento_Propriedades(int intSolicitacaoPagamentoParametro, int intEnum_Origem_ID) : base()
        {
            this.InitializeComponent();

            try
            {
                this.intSolicitacaoPagamento = intSolicitacaoPagamentoParametro;
                this.intOrigemID = intEnum_Origem_ID;

                this.Load += this.Form_Load;
                this.Shown += this.Form_Shown;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Inicialização      "

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                this.Formatar_Formulario();

                this.Remover_Eventos();

                this.Preencher_Solicitacao_Pagamento();

                this.Preencher_Combo();

                this.Preencher_Formulario();

                this.Tratar_Campos_Banco();
                this.Tratar_Habilitar_Campos();

                this.Adicionar_Eventos();

                this.Atualizar_DataTable_Original();

                this.Tratar_Permissao();                
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            try
            {
                this.cboStatus.Focus();
                this.dtmlblBancoPagamentoData.MaxDate = DateTime.Now;
                this.dtmlblBancoPagamentoData.MinDate = DateTime.Now.AddDays(frmSolicitacao_Pagamento_Propriedades.DIAS_MINIMOS_SOLICITACAO);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Eventos            "

        private void Clicar_Romaneio_Credito_Click(object sender, EventArgs e)
        {
            try
            {
                if (Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, Root.Loja_Ativa.ID, typeof(frmRomaneio_Propriedades).Name, Acao_Formulario.Selecionar.DefaultString()))
                {
                    if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                    {
                        var romaneioVendaGrupo = new Romaneio_Venda_GrupoBUS().Selecionar_Dados_Origem_Romaneio_Venda_CT_Por_Grupo(this.dttSolicitacaoPagamento.Rows[0]["Romaneio_Grupo_ID"].DefaultInteger(),
                                                                                                                               this.dttSolicitacaoPagamento.Rows[0]["Lojas_ID"].DefaultInteger());

                        frmRomaneio_Propriedades frmVendaPropriedades = new frmRomaneio_Propriedades(romaneioVendaGrupo["Romaneio_Venda_Grupo_ID"].DefaultInteger(),
                                                                                                     0,
                                                                                                     this.dttSolicitacaoPagamento.Rows[0]["Lojas_ID"].DefaultInteger(),
                                                                                                     true);

                        frmVendaPropriedades.ShowDialog(this);
                    }

                    if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                    {
                        frmRomaneio_Propriedades frmRomaneioPropriedades = new frmRomaneio_Propriedades(this.dttSolicitacaoPagamento.Rows[0]["Romaneio_Grupo_ID"].DefaultInteger(),
                                                                                                this.dttSolicitacaoPagamento.Rows[0]["Objeto_Origem_ID"].DefaultInteger(),
                                                                                                this.dttSolicitacaoPagamento.Rows[0]["Lojas_ID"].DefaultInteger(),
                                                                                                true);
                        frmRomaneioPropriedades.Show(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Motivo_Recusa_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Solicitacao_Pagamento["Enum_Motivo_Recusa_Pagamento_ID"] = this.cboMotivoRecusa.SelectedValue.DefaultInteger();
                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Forma_Pagamento_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Solicitacao_Pagamento["Enum_Tipo_Pagamento_ID"] = this.cboFormaPagamento.SelectedValue.DefaultInteger();
                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Banco_Pagamento_Data_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.Solicitacao_Pagamento["Solicitacao_Pagamento_Data_Pagamento"] = this.dtmlblBancoPagamentoData.Value.ToDateTime().DefaultString();
                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Solicitacao_Pagamento_Obs_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.Solicitacao_Pagamento["Solicitacao_Pagamento_Obs"] = this.txtSolicitacaoPagamentoObs.Text;
                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Status_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Solicitacao_Pagamento["Enum_Status_ID"] = this.cboStatus.SelectedValue.DefaultInteger();

                this.Tratar_Habilitar_Motivo_Recusa();

                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Arquivo_Caminho_Completo_TextChanged(object sender, EventArgs e)
        {
            try
            {
                this.Solicitacao_Pagamento["Solicitacao_Pagamento_Comprovante_Pgto"] = Utilitario.Converte_Arquivo_Para_Array_Bytes(this.txtArquivoCaminhoCompleto.Text);
                this.Verificar_Mudancas();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Origem_ID_Click(object sender, EventArgs e)
        {
            try
            {
                if (Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID,  typeof(frmSAC_Propriedades).Name, Acao_Formulario.Selecionar.DefaultString()))
                {
                    if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                    {
                        frmSAC_Propriedades frmSACPropriedades = new frmSAC_Propriedades(this.Solicitacao_Pagamento["Objeto_Origem_ID"].DefaultInteger());
                        frmSACPropriedades.ShowDialog(this);
                    }

                    if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                    {
                        this.Clicar_Romaneio_Credito_Click(sender, e);
                    }


                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Enviar_Email_Click(object sender, EventArgs e)
        {
            try
            {
                Solicitacao_PagamentoBUS.Enviar_Email_Cliente(this.Solicitacao_Pagamento);
                MessageBox.Show(string.Format("E-mail enviado com sucesso!"), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Tratar_Download_Arquivo(object sender, EventArgs e)
        {
            try
            {

                SaveFileDialog sfdCaixaDialogoSalvarArquivo = new SaveFileDialog();

                sfdCaixaDialogoSalvarArquivo.Title = "Salver arquivo Como";
                sfdCaixaDialogoSalvarArquivo.DefaultExt = "pdf";

                sfdCaixaDialogoSalvarArquivo.FileName = Solicitacao_PagamentoBUS.Obter_Nome_Arquivo_Fisico(this.Solicitacao_Pagamento["Solicitacao_Pagamento_ID"].DefaultInteger());
                sfdCaixaDialogoSalvarArquivo.Filter = "pdf (*.pdf)|*.pdf";
                sfdCaixaDialogoSalvarArquivo.FilterIndex = 2;

                if (sfdCaixaDialogoSalvarArquivo.ShowDialog() == DialogResult.OK)
                {
                    // Salvar Arquivo
                    Utilitario.Salvar_Arquivo_Fisico((byte[])this.Solicitacao_Pagamento["Solicitacao_Pagamento_Comprovante_Pgto"], sfdCaixaDialogoSalvarArquivo.FileName);

                    if (MessageBox.Show(string.Format("Arquivo {0} criado com sucesso.\nDeseja abrí-lo agora?",
                        sfdCaixaDialogoSalvarArquivo.FileName), this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(sfdCaixaDialogoSalvarArquivo.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }

        }

        private void Tratar_Procurar_Arquivo(object sender, EventArgs e)
        {
            try
            {

                OpenFileDialog objCaixaDialogoProcurarArquivo = new OpenFileDialog();

                objCaixaDialogoProcurarArquivo.Title = "Localizar Arquivos";
                objCaixaDialogoProcurarArquivo.DefaultExt = "pdf";

                objCaixaDialogoProcurarArquivo.Filter = "PDF (*.pdf)|*.pdf";
                objCaixaDialogoProcurarArquivo.FilterIndex = 2;
                objCaixaDialogoProcurarArquivo.CheckFileExists = true;
                objCaixaDialogoProcurarArquivo.CheckPathExists = true;

                objCaixaDialogoProcurarArquivo.ReadOnlyChecked = true;
                objCaixaDialogoProcurarArquivo.ReadOnlyChecked = true;


                if (objCaixaDialogoProcurarArquivo.ShowDialog() == DialogResult.OK)
                {
                    this.txtArquivoCaminhoCompleto.Text = objCaixaDialogoProcurarArquivo.FileName;
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }

        }

        private void Clicar_Botao_Auditoria(object sender, EventArgs e)
        {
            try
            {
                frmAuditoria_Propriedades frmAuditoria = new frmAuditoria_Propriedades("Histórico de Alterações da Solicitação de Pagamento");

                frmAuditoria.Grid.AutoGenerateColumns = false;

                frmAuditoria.Grid.Adicionar_Coluna("Acao", "Ação", 80);
                frmAuditoria.Grid.Adicionar_Coluna("Data_Ultima_Alteracao", "Última Alteração", 110);
                frmAuditoria.Grid.Adicionar_Coluna("Usuario_Ultima_Alteracao", "Usuário Alteração", 110);
                frmAuditoria.Grid.Adicionar_Coluna("Enum_Origem", "Origem", 75, false, Enumerados.Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                frmAuditoria.Grid.Adicionar_Coluna("Enum_Status", "Status", 140, false, Enumerados.Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                frmAuditoria.Grid.Adicionar_Coluna("Banco_Nome", "Banco", 165, false, Enumerados.Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                frmAuditoria.Grid.Adicionar_Coluna("Solicitacao_Pagamento_Banco_Agencia", "Agência", 80);
                frmAuditoria.Grid.Adicionar_Coluna("Solicitacao_Pagamento_Banco_Conta", "Conta", 100);
                frmAuditoria.Grid.Adicionar_Coluna("Solicitacao_Pagamento_Valor", "Valor", 80);
                frmAuditoria.Grid.Adicionar_Coluna("Solicitacao_Pagamento_Obs", "Obs", 200);

                Solicitacao_PagamentoBUS busSolicitacaoPagamento = new Solicitacao_PagamentoBUS();
                DataTable dttAuditoria = busSolicitacaoPagamento.Consultar_DataTable_Auditoria(this.intSolicitacaoPagamento);

                frmAuditoria.Carregar_Grid(dttAuditoria);

                frmAuditoria.Show(this);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Métodos            "

        private void Tratar_Permissao()
        {
            try
            {
                this.btnAuditoria.Enabled = this.Obter_Permissao_Do_Usuario_Ativo(Acao_Formulario.Visualizar_Historico_De_Auditoria.DefaultString());

                if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                    this.btnAuditoria.Visible = false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Adiciona os eventos do formulário
        /// ATENÇÃO: ao adicionar o evento neste método, atualizar o metodo "Remover_Eventos"
        /// </summary>
        private void Adicionar_Eventos()
        {
            try
            {
                this.btnProcurarArquivo.Click += this.Tratar_Procurar_Arquivo;
                this.btnDownloadArquivo.Click += this.Tratar_Download_Arquivo;
                this.btnEnviarEmail.Click += this.Tratar_Enviar_Email_Click;
                this.btnAuditoria.Click += this.Clicar_Botao_Auditoria;

                this.cboFormaPagamento.SelectedValueChanged += this.Tratar_Forma_Pagamento_SelectedValueChanged;
                this.cboMotivoRecusa.SelectedValueChanged += this.Tratar_Motivo_Recusa_SelectedValueChanged;

                this.cboStatus.SelectedValueChanged += this.Tratar_Status_SelectedValueChanged;
                this.txtSolicitacaoPagamentoObs.TextChanged += this.Tratar_Solicitacao_Pagamento_Obs_TextChanged;
                this.txtArquivoCaminhoCompleto.TextChanged += this.Tratar_Arquivo_Caminho_Completo_TextChanged;

                this.dtmlblBancoPagamentoData.TextChanged += this.Tratar_Banco_Pagamento_Data_TextChanged;

                this.lnkOrigemID.Click += this.Clicar_Origem_ID_Click;
                this.lnkRomaneioCredito.Click += this.Clicar_Romaneio_Credito_Click;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Remove os eventos do formulário
        /// ATENÇÃO: ao remover o evento neste método, atualizar o metodo "Adicionar_Eventos"
        /// </summary>
        private void Remover_Eventos()
        {
            try
            {
                this.btnProcurarArquivo.Click -= this.Tratar_Procurar_Arquivo;
                this.btnDownloadArquivo.Click -= this.Tratar_Download_Arquivo;
                this.btnEnviarEmail.Click -= this.Tratar_Enviar_Email_Click;
                this.btnAuditoria.Click -= this.Clicar_Botao_Auditoria;

                this.cboFormaPagamento.SelectedValueChanged -= this.Tratar_Forma_Pagamento_SelectedValueChanged;
                this.cboMotivoRecusa.SelectedValueChanged -= this.Tratar_Motivo_Recusa_SelectedValueChanged;

                this.cboStatus.SelectedValueChanged -= this.Tratar_Status_SelectedValueChanged;
                this.txtSolicitacaoPagamentoObs.TextChanged -= this.Tratar_Solicitacao_Pagamento_Obs_TextChanged;
                this.txtArquivoCaminhoCompleto.TextChanged -= this.Tratar_Arquivo_Caminho_Completo_TextChanged;

                this.dtmlblBancoPagamentoData.TextChanged -= this.Tratar_Banco_Pagamento_Data_TextChanged;

                this.lnkOrigemID.Click -= this.Clicar_Origem_ID_Click;
                this.lnkRomaneioCredito.Click -= this.Clicar_Romaneio_Credito_Click;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Habilitar_Motivo_Recusa()
        {
            try
            {
                if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Negado.DefaultInteger())
                {
                    this.cboMotivoRecusa.Enabled = true;
                }
                else
                {
                    this.cboMotivoRecusa.SelectedValue = "0";
                    this.cboMotivoRecusa.Enabled = false;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Formulario()
        {
            try
            {

                this.lblSolicitacaoPagamento.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_ID"].DefaultString();
                this.lnkOrigemID.Text = this.Solicitacao_Pagamento["Objeto_Origem_ID"].DefaultString();
                this.cboStatus.SelectedValue = this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger();

                this.lblSolitacaoPagamentoDataCriacao.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Data_Criacao"].DefaultString();
                this.lblSolitacaoPagamentoUsuarioCriacao.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Usuario_Criacao"].DefaultString();
                this.lblSolitacaoPagamentoDataUltimaAlteracao.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Data_Ultima_Alteracao"].DefaultString();
                this.lblSolitacaoPagamentoDataUsuarioAlteracao.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Usuario_Ultima_Alteracao"].DefaultString();

                if (this.Validar_Solicitacao_Pagamento_Finalizada())
                {
                    this.lblSolitacaoPagamentoDataFinalizacao.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Data_Ultima_Alteracao"].DefaultString();
                    this.lblSolitacaoPagamentoUsuarioFinalizacao.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Usuario_Ultima_Alteracao"].DefaultString();
                }
                else
                {
                    this.lblSolitacaoPagamentoDataFinalizacao.Text = string.Empty;
                    this.lblSolitacaoPagamentoUsuarioFinalizacao.Text = string.Empty;
                }

                this.txtClienteNome.Text = this.Solicitacao_Pagamento["Cliente_Nome"].DefaultString();
                this.txtClienteDocumento.Text = DivUtil.Formatar_CPF_CNPJ( this.Solicitacao_Pagamento["Cliente_CPFCNPJ"].DefaultString());
                this.lblTelefone.Text = this.Solicitacao_Pagamento["Cliente_Telefone"].DefaultString();
                this.txtClienteEmailCapa.Text = this.Solicitacao_Pagamento["Cliente_Email"].DefaultString();


                this.lblClienteFormaPagamento.Text = this.Solicitacao_Pagamento["Cliente_Forma_Pagamento"].DefaultString();
                this.lblClientePagamentoSolicitacaoData.Text = this.Solicitacao_Pagamento["Cliente_Forma_Pagamento_Solicitacao_Data"].DefaultString();

                this.lblBancoNome.Text = this.Solicitacao_Pagamento["Banco_Nome"].DefaultString();
                this.lblBancoNomeCD.Text = this.Solicitacao_Pagamento["Banco_CD"].DefaultString();
                this.lblBancoValor.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Valor"].DefaultString();
                this.txtBancoAgencia.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Banco_Agencia"].DefaultString();
                this.txtBancoConta.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Banco_Conta"].DefaultString();

                this.cboFormaPagamento.SelectedValue = this.Solicitacao_Pagamento["Enum_Tipo_Pagamento_ID"].DefaultInteger();
                this.cboMotivoRecusa.SelectedValue = this.Solicitacao_Pagamento["Enum_Motivo_Recusa_Pagamento_ID"].DefaultInteger();
                this.txtSolicitacaoPagamentoObs.Text = this.Solicitacao_Pagamento["Solicitacao_Pagamento_Obs"].DefaultString();

                this.btnEnviarEmail.Enabled = this.Validar_Habilitar_Enviar_Email();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Solicitacao_Pagamento()
        {
            try
            {
                if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                    this.dttSolicitacaoPagamento = new Solicitacao_PagamentoBUS().Selecionar(this.intSolicitacaoPagamento);

                if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                    this.dttSolicitacaoPagamento = new Solicitacao_PagamentoBUS().Selecionar_Solicitacao_Deposito(this.intSolicitacaoPagamento);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Atualizar_DataTable_Original()
        {
            try
            {
                this.dttSolicitacaoPagamentoOriginal = this.dttSolicitacaoPagamento.Copy();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Os valores dos bancos foram tratados como TEXT para que o usuário possa copiar os valores
        /// </summary>
        private void Tratar_Campos_Banco()
        {
            try
            {
                this.txtBancoAgencia.ReadOnly = true;
                this.txtBancoConta.ReadOnly = true;
                this.txtClienteDocumento.ReadOnly = true;
                this.txtClienteNome.ReadOnly = true;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void Preencher_Combo()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Exibindo_Extenso(ref this.cboMotivoRecusa, "Motivo_Recusa_Pagamento", string.Empty, true, string.Empty, "Enum_Extenso");
                Utilitario.Preencher_ComboBox_Enumerado_Exibindo_Extenso(ref this.cboStatus, "Status_Solicitacao_Pagamento", string.Empty, false, string.Empty, "Enum_Extenso");

                if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                {
                    this.cboFormaPagamento.DisplayMember = "Enum_Extenso";
                    this.cboFormaPagamento.ValueMember = "Enum_ID";
                    this.cboFormaPagamento.DropDownStyle = ComboBoxStyle.DropDownList;
                    this.cboFormaPagamento.DataSource = SAC_CTBUS.Obter_Ecommerce_Forma_Solicitacao_Pagamento((Ecommerce_Pedido_Forma_Pagamento_Tipo)this.Solicitacao_Pagamento["Cliente_Forma_Pagamento_Tipo_ID"].DefaultInteger());
                }

                if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                    Utilitario.Preencher_ComboBox_DataObject(ref this.cboFormaPagamento,
                                                         new Solicitacao_PagamentoBUS().Obter_DataObject_Forma_Pagamento().Where(x => x.Forma_Pagamento_DS == "Depósito em Conta").ToList<Solicitacao_Pagamento_Forma_PagtoDO>(),
                                                         "Forma_Pagamento_DS",
                                                         "Forma_Pagamento_ID",
                                                         "");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Formatar_Formulario()
        {
            try
            {
                Form_Designer.Configurar_Designer_Padrao_MercadoCar(this);
                Form_Designer.Definir_Botao_Estilo_Email(this.btnEnviarEmail);
                Form_Designer.Definir_Botao_Estilo_Auditoria(this.btnAuditoria);

                this.txtClienteDocumento.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
                this.txtClienteNome.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
                this.txtBancoAgencia.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);
                this.txtBancoConta.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular);

                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtArquivoCaminhoCompleto);
                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtClienteNome);
                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtClienteEmailCapa);
                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtClienteDocumento);
                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtBancoAgencia);
                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtBancoConta);
                Form_Designer.Remover_Campo_Estilo_Eventos(this.txtArquivoCaminhoCompleto);

                this.txtArquivoCaminhoCompleto.FormatVisualDesabled();
                this.txtClienteNome.FormatVisualDesabled();
                this.txtClienteEmailCapa.FormatVisualDesabled();
                this.txtClienteDocumento.FormatVisualDesabled();
                this.txtBancoAgencia.FormatVisualDesabled();
                this.txtBancoConta.FormatVisualDesabled();
                this.txtArquivoCaminhoCompleto.FormatVisualDesabled();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Verificar_Mudancas()
        {
            try
            {
                if (this.dttSolicitacaoPagamento.EqualsValues(this.dttSolicitacaoPagamentoOriginal))
                {
                    this.btnAplicar.Enabled = false;
                    return;
                }

                this.btnAplicar.Enabled = true;
                
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
                if (this.Validar_Campos() == false)
                {
                    return false;
                }

                new Solicitacao_PagamentoBUS().Alterar(this.Solicitacao_Pagamento);

                if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Efetuado.DefaultInteger())
                {
                    if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                        MessageBox.Show(string.Concat("Pagamento efetuado com sucesso, foi gerado o romaneio de número '", 
                            this.dttSolicitacaoPagamento.Rows[0]["Solicitacao_Pagamento_Comprovante_Estorno"].DefaultString(), "'!"), 
                            this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

                    if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.Romaneio_Credito.DefaultInteger())
                        MessageBox.Show(string.Concat("Depósito efetuado referente ao romaneio de crédito ", 
                            this.dttSolicitacaoPagamento.Rows[0]["Objeto_Origem_ID"].DefaultString(), "."), 
                            this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }

                this.Preencher_Solicitacao_Pagamento();

                this.Preencher_Formulario();
                this.Tratar_Habilitar_Campos();
                this.Atualizar_DataTable_Original();
                this.Verificar_Mudancas();

                this.Registro_Alterado = true;

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Campos()
        {
            try
            {
                if (this.Validar_Solicitacao_Pagamento_Finalizada())
                {
                    if (this.Solicitacao_Pagamento["Enum_Tipo_Pagamento_ID"].DefaultInteger() == 0)
                    {
                        MessageBox.Show("O campo 'Forma de Pagamento' é obrigatório, por favor selecione um!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.cboFormaPagamento.Focus();
                        return false;
                    }

                    if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Efetuado.DefaultInteger() && this.Solicitacao_Pagamento["Solicitacao_Pagamento_Comprovante_Pgto"].DefaultString().IsNullOrEmpty())
                    {
                        MessageBox.Show("Quando a 'Status' da solicitação for '" + Status_Solicitacao_Pagamento.Efetuado.ToDescription() + "' deve-se informar o comprovante da transação, por favor anexe o comprovante da transação!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.btnProcurarArquivo.PerformClick();
                        return false;
                    }

                    if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Negado.DefaultInteger())
                    {
                        if (this.Solicitacao_Pagamento["Enum_Motivo_Recusa_Pagamento_ID"].DefaultInteger() == 0)
                        {
                            MessageBox.Show("O campo 'Motivo Recusa' é obrigatório quando o status da solicitação for '" + Status_Solicitacao_Pagamento.Negado.ToDescription() + "', por favor selecione um!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.cboMotivoRecusa.Focus();
                            return false;
                        }

                        if (this.Solicitacao_Pagamento["Solicitacao_Pagamento_Obs"].DefaultString().Length < 5)
                        {
                            MessageBox.Show("O campo 'Obs' é obrigatório quando o status da solicitação for '" + Status_Solicitacao_Pagamento.Negado.ToDescription() + "', por favor preencha o campo com o motivo!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.txtSolicitacaoPagamentoObs.Focus();
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

        private bool Validar_Solicitacao_Pagamento_Finalizada()
        {
            try
            {
                return Solicitacao_PagamentoBUS.Validar_Solicitacao_Pagamento_Finalizada(this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger());
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private bool Validar_Habilitar_Enviar_Email()
        {
            try
            {
                return (
                        this.Validar_Solicitacao_Pagamento_Finalizada() && 
                        this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() != Status_Solicitacao_Pagamento.Em_Analise.DefaultInteger() &&
                        Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Enviar_Email.DefaultString())
                        );
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void Tratar_Habilitar_Campos()
        {
            try
            {
                this.Tratar_Habilitar_Motivo_Recusa();

                if (this.Validar_Solicitacao_Pagamento_Finalizada() || this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Em_Analise.DefaultInteger())
                {
                    this.cboStatus.Enabled = false;
                    this.cboFormaPagamento.Enabled = false;
                    this.cboMotivoRecusa.Enabled = false;
                    this.dtmlblBancoPagamentoData.Enabled = false;
                    this.txtSolicitacaoPagamentoObs.Enabled = false;
                    this.btnProcurarArquivo.Enabled = false;
                    this.btnEnviarEmail.Enabled = this.Validar_Habilitar_Enviar_Email();

                    if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Em_Analise.DefaultInteger())
                    {
                        this.btnDownloadArquivo.Enabled = false;
                    }

                    if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Efetuado.DefaultInteger())
                    {

                        if (this.intOrigemID == Solicitacao_Pagamento_Objeto_Origem.SAC.DefaultInteger())
                        {
                            this.lblRomaneioCreditoCapa.Visible = true;
                            this.lnkRomaneioCredito.Visible = true;
                        }

                        this.lnkRomaneioCredito.Text = this.dttSolicitacaoPagamento.Rows[0]["Romaneio_Grupo_ID"].DefaultString();
                    }
                }
                else
                {
                    this.btnDownloadArquivo.Enabled = false;
                }

                if (this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Encaminhado_ao_financeiro.DefaultInteger())
                {
                    DataTable dttStatusSolicitacaoPagamento = Utilitario.Obter_DataTable_Enumerado_Da_Memoria("Status_Solicitacao_Pagamento", string.Empty, "Enum_Extenso");

                    for (int intContador = 0; intContador < dttStatusSolicitacaoPagamento.Rows.Count; intContador++)
                    {
                        if (dttStatusSolicitacaoPagamento.Rows[intContador]["Enum_ID"].DefaultInteger() == Status_Solicitacao_Pagamento.Em_Analise.DefaultInteger())
                        {
                            dttStatusSolicitacaoPagamento.Rows[intContador].Delete();
                            dttStatusSolicitacaoPagamento.AcceptChanges();
                            break;
                        }
                    }

                    this.cboStatus.DisplayMember = "Enum_Extenso";
                    this.cboStatus.ValueMember = "Enum_ID";
                    this.cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;

                    this.cboStatus.DataSource = dttStatusSolicitacaoPagamento;

                    this.cboStatus.SelectedValue = this.Solicitacao_Pagamento["Enum_Status_ID"].DefaultInteger();

                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #endregion

    }
}