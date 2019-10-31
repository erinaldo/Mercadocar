using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Mercadocar.Enumerados;
using Mercadocar.Herancas;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.ObjetosNegocio.DataObject;
using Mercadocar.RegrasNegocio;

namespace Mercadocar.Formularios
{
    public sealed partial class frmRelatorio_Creditos_Aprovados_Dinheiro_Grid : frmGrid
    {
        #region "   Declarações     "
        
        private enum Exibicao_Listagem : int
        {
            [Description("Loja")]
            Loja = 0,
            [Description("Romaneio")]
            Romaneio = 1,
            [Description("Usuário Aprovação")]
            Usuario_Aprovacao = 2
        }

        private enum Tipo_Data : int
        {
            [Description("Data Geração")]
            Data_Geracao = 0,
            [Description("Data Liberação")]
            Data_Liberacao = 1,
            [Description("Data Aprovação")]
            Data_Aprovacao = 2
        }
        

        #endregion

        #region "   Construtor      "

        public frmRelatorio_Creditos_Aprovados_Dinheiro_Grid()
            : base()
        {
            try
            {
                
                this.Load += this.Form_Load;
                this.Shown += this.Form_Shown;
                this.InitializeComponent();
                this.Grid.ContextMenuStrip = this.cmsMenu;

                this.dtpDataFinal.ValueChanged += this.Consistir_Filtro_Data;
                this.dtpDataInicial.ValueChanged += this.Consistir_Filtro_Data;

                this.radRomaneio.CheckedChanged += this.radRomaneio_CheckedChanged;
                this.radOutrosFiltros.CheckedChanged += this.radOutrosFiltros_CheckedChanged;

                this.cboTipoExibicao.SelectedValueChanged += this.cboTipoExibicao_ValueChanged;

                this.chkPeriodo.CheckedChanged += this.Mudar_Estado_CheckBox_Periodo;
                this.txtRomaneio.KeyPress += this.Tecla_Pressionada;

                this.btnCarregarDados.Click += this.Clicar_Botao_Carregar_Dados;

                this.tsmPropriedadesVenda.Click += this.Clicar_Menu_Propriedades_Venda;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Inicialização   "

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Definir_Formulario_Estilo();

                this.Preencher_Combo_Tipo_Exibicao();

                this.Preencher_Combo_Tipo_Data();

                this.Preencher_Combo_Loja();

                this.Preencher_Combo_Usuario_Aprovacao();

                this.Preencher_Data();

                this.Configurar_Grid();

                this.Tratar_Permissoes();
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

        public void Form_Shown(object sender, EventArgs e)
        {
            try
            {
                if (this.cboLoja.Enabled)
                {
                    this.cboLoja.Focus();
                }
                else
                {
                    this.cboTipoExibicao.Focus();
                }

                this.chkPeriodo.Checked = true;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }


        #endregion

        #region "   Eventos         "
        
        private void Tecla_Pressionada(object sender, KeyPressEventArgs e)
        {
            try
            {
                e.Handled = Utilitario.Validar_KeyPress_Inteiro(e, false);

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }

        }
        
        private void radRomaneio_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.radRomaneio.Checked)
                {
                    this.grbRomaneio.Enabled = true;
                    this.grbOutrosFiltros.Enabled = false;

                    this.txtRomaneio.Text = string.Empty;

                    this.txtRomaneio.Focus();

                    this.Validar_Exibir_Menu();
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void radOutrosFiltros_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.radOutrosFiltros.Checked)
                {
                    this.grbRomaneio.Enabled = false;
                    this.grbOutrosFiltros.Enabled = true;

                    this.txtRomaneio.Text = string.Empty;

                    this.cboTipoExibicao.Focus();

                    this.Validar_Exibir_Menu();
                }

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void cboTipoExibicao_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Validar_Exibir_Menu();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Mudar_Estado_CheckBox_Periodo(object sender, EventArgs e)
        {
            try
            {
                this.grbPeriodo.Enabled = this.chkPeriodo.Checked;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }
        
        private void Consistir_Filtro_Data(object sender, EventArgs e)
        {
            try
            {
                this.Validar_Data_Filtro();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        private void Clicar_Botao_Carregar_Dados(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Preencher_Grid();

                Validar_Exibir_Menu();
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

        private void Clicar_Menu_Propriedades_Venda(object sender, EventArgs e)
        {

            try
            {

                if (this.Grid.SelectedRows.Count == 0)
                    return;

                if (!this.tsmPropriedadesVenda.Enabled)
                    return;

                DataRowView objRow = (DataRowView)this.Grid.SelectedRows[0].DataBoundItem;

                frmVenda_Propriedades frmPropriedades = new frmVenda_Propriedades(Convert.ToInt32(objRow["Romaneio_Grupo_ID"]), Convert.ToInt32(objRow["Lojas ID"]));

                frmPropriedades.Show(this);
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Métodos         "

        private void Definir_Formulario_Estilo()
        {
            try
            {
                Form_Designer.Configurar_Designer_Padrao_MercadoCar(this);
                Form_Designer.Definir_Botao_Estilo_Carregar_Dados(this.btnCarregarDados);
                Form_Designer.Definir_ToolStripMenuItem_Estilo_Ajuda(this.tsmAjuda);
                Form_Designer.Definir_ToolStripMenuItem_Estilo_Propriedades(this.tsmPropriedadesVenda);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        #region "   Grid                    "

        protected override void Configurar_Grid()
        {
            try
            {
                this.Grid.AutoGenerateColumns = false;

                Exibicao_Listagem enuExibicaoListagem = (Exibicao_Listagem)this.cboTipoExibicao.SelectedValue.DefaultInteger();

                if (this.radRomaneio.Checked)
                {
                    enuExibicaoListagem = Exibicao_Listagem.Romaneio;
                }

                switch (enuExibicaoListagem)
                {
                    case Exibicao_Listagem.Loja:
                        
                        this.Grid.Adicionar_Coluna("Lojas_NM", "Loja", 100);
                        this.Grid.Adicionar_Coluna("Valor", "Valor Crédito (R$)", 120, false, Enumerados.Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Valor_Indevido", "Valor Indevido (R$)", 120, false, Enumerados.Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Qtde_Romaneio", "Qtde Romaneio Crédito", 140, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                        
                        break;

                    case Exibicao_Listagem.Romaneio:

                        this.Grid.Adicionar_Coluna("Lojas_ID", "Lojas ID", 20, false, Enumerados.Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Lojas_NM", "Loja", 100);
                        this.Grid.Adicionar_Coluna("Tipo", "Tipo", 60);
                        this.Grid.Adicionar_Coluna("Romaneio", "Romaneio", 70, false, Enumerados.Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Romaneio_Grupo_ID", "Romaneio Grupo ID", 120, false, Enumerados.Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Cliente", "Cliente", 145);
                        this.Grid.Adicionar_Coluna("Data_Geracao", "Data Geração", 100, false, Enumerados.Tipo_Coluna.Data_Tempo, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Data_Liberacao", "Data Liberação", 100, false, Enumerados.Tipo_Coluna.Data_Tempo, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Data_Venda", "Data Venda", 100, false, Enumerados.Tipo_Coluna.Data_Tempo, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Prazo_Pagamento", "Prazo Pagto.", 100);
                        this.Grid.Adicionar_Coluna("Usuario_Geracao", "Usuário Geração", 155);
                        this.Grid.Adicionar_Coluna("Valor", "Valor Crédito (R$)", 120, false, Enumerados.Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Valor_Indevido", "Valor Indevido (R$)", 120, false, Enumerados.Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Data_Aprovacao", "Data Aprovação", 100, false, Enumerados.Tipo_Coluna.Data_Tempo, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Usuario_Aprovacao", "Usuário Aprovação", 155);
                        this.Grid.Adicionar_Coluna("Motivo", "Motivo", 220, true);

                        this.Grid.Columns["Lojas_ID"].Visible = false;
                        this.Grid.Columns["Romaneio_Grupo_ID"].Visible = false;

                        break;

                    case Exibicao_Listagem.Usuario_Aprovacao:

                        this.Grid.Adicionar_Coluna("Usuario_Aprovacao", "Usuário Aprovação", 180);
                        this.Grid.Adicionar_Coluna("Valor", "Valor Crédito (R$)", 120, false, Enumerados.Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Valor_Indevido", "Valor Indevido (R$)", 120, false, Enumerados.Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight);
                        this.Grid.Adicionar_Coluna("Qtde_Romaneio", "Qtde Romaneio Crédito", 140, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight);
                        
                        break;

                    default:
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Recarregar_Grid()
        {
            try
            {
                if (this.Grid.DataSource != null)
                {
                    this.Grid.DataSource = ((DataTable)this.Grid.DataSource).Clone();
                }

                this.Grid.Columns.Clear();

                this.Configurar_Grid();

            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private void Preencher_Grid()
        {
            try
            {
                this.Recarregar_Grid();

                Creditos_Aprovados_DinheiroBUS busCreditosAprovadosDinheiro = new Creditos_Aprovados_DinheiroBUS();

                if (this.radRomaneio.Checked)
                {
                    if (this.txtRomaneio.Text == string.Empty)
                    {
                        MessageBox.Show("Informe o número do romaneio de venda.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        this.txtRomaneio.Focus();
                        return;
                    }
                    if (this.txtRomaneio.Text.ToInteger() == 0)
                    {
                        MessageBox.Show("Número do romaneio de venda inválido.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        this.txtRomaneio.Focus();
                        return;
                    }

                    this.Grid.DataSource = busCreditosAprovadosDinheiro.Consultar_DataTable_Creditos_Aprovados_Dinheiro(
                                                                                                                            Exibicao_Listagem.Romaneio.DefaultInteger(),
                                                                                                                            0,
                                                                                                                            this.txtRomaneio.Text.DefaultInteger(),
                                                                                                                            0,
                                                                                                                            0,
                                                                                                                            0,
                                                                                                                            0,
                                                                                                                            0,
                                                                                                                            new DateTime(1900, 1, 1),
                                                                                                                            new DateTime(1900, 1, 1),
                                                                                                                            new DateTime(1900, 1, 1),
                                                                                                                            new DateTime(1900, 1, 1),
                                                                                                                            new DateTime(1900, 1, 1),
                                                                                                                            new DateTime(1900, 1, 1));
                }
                else
                {
                    if (this.Validar_Filtro_Outros() == false)
                    {
                        return;
                    }

                    this.Grid.DataSource = busCreditosAprovadosDinheiro.Consultar_DataTable_Creditos_Aprovados_Dinheiro(
                                                                                            this.cboTipoExibicao.SelectedValue.DefaultInteger(),
                                                                                            this.cboLoja.SelectedValue.DefaultInteger(),
                                                                                            0,
                                                                                            this.cboUsuarioAprovacao.SelectedValue.DefaultInteger(),
                                                                                            this.txtValorInicial.Text.DefaultDecimal(),
                                                                                            this.txtValorFinal.Text.DefaultDecimal(),
                                                                                            this.txtValorIndevidoInicial.Text.DefaultDecimal(),
                                                                                            this.txtValorIndevidoFinal.Text.DefaultDecimal(),
                                                                                            (this.chkPeriodo.Checked && this.cboTipoPeriodo.SelectedValue.DefaultInteger() == Tipo_Data.Data_Geracao.DefaultInteger()) ? this.dtpDataInicial.Value : new DateTime(1900, 1, 1),
                                                                                            (this.chkPeriodo.Checked && this.cboTipoPeriodo.SelectedValue.DefaultInteger() == Tipo_Data.Data_Geracao.DefaultInteger()) ? this.dtpDataFinal.Value : new DateTime(1900, 1, 1),
                                                                                            (this.chkPeriodo.Checked && this.cboTipoPeriodo.SelectedValue.DefaultInteger() == Tipo_Data.Data_Liberacao.DefaultInteger()) ? this.dtpDataInicial.Value : new DateTime(1900, 1, 1),
                                                                                            (this.chkPeriodo.Checked && this.cboTipoPeriodo.SelectedValue.DefaultInteger() == Tipo_Data.Data_Liberacao.DefaultInteger()) ? this.dtpDataFinal.Value : new DateTime(1900, 1, 1),
                                                                                            (this.chkPeriodo.Checked && this.cboTipoPeriodo.SelectedValue.DefaultInteger() == Tipo_Data.Data_Aprovacao.DefaultInteger()) ? this.dtpDataInicial.Value : new DateTime(1900, 1, 1),
                                                                                            (this.chkPeriodo.Checked && this.cboTipoPeriodo.SelectedValue.DefaultInteger() == Tipo_Data.Data_Aprovacao.DefaultInteger()) ? this.dtpDataFinal.Value : new DateTime(1900, 1, 1));
                }


                if (this.Grid.Rows.Count == 0)
                {
                    MessageBox.Show("Não há itens a serem exibidos.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    this.Grid.Focus();
                }
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion 

        #region "   Preencher               "

        private void Preencher_Combo_Usuario_Aprovacao()
        {
            try
            {
                DataTable dttUsuarioAprovacao = new Creditos_Aprovados_DinheiroBUS().Consultar_DataTable_Usuarios_Credito_Aprovacao();
                
                Utilitario.Preencher_ComboBox_DataTable(ref this.cboUsuarioAprovacao, dttUsuarioAprovacao, "Usuario_Nome_Completo", "Usuario_Aprovacao_ID", "0", "Todos");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Combo_Loja()
        {
            try
            {
                if (Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Alterar_Loja_Ativa.ToString()))
                {
                    Utilitario.Preencher_ComboBox_Lojas_Ativas(ref this.cboLoja, Utilitario.Colunas_Loja.Lojas_Tipo, "Loja", true, "Todas", true, Root.Loja_Ativa.ID);
                }
                else
                {
                    Utilitario.Preencher_ComboBox_Lojas_Ativas(ref this.cboLoja, Utilitario.Colunas_Loja.Lojas_Tipo, "Loja", false, string.Empty, true, Root.Loja_Ativa.ID);
                }
                
                if (((DataTable)this.cboLoja.DataSource).Select("Lojas_ID=" + Root.Loja_Ativa.ID.ToString()).Length == 0)
                {
                    ((UtilidadesForm)new UtilidadesForm()).DesabilitaForm(this);
                    MessageBox.Show("O cadastro não pode ser aberto na loja " + ((LojasDO)Root.Loja_Ativa_NEW).Nome + ". Faça o login com outra loja.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Data()
        {
            try
            {
                this.dtpDataInicial.Value = DateTime.Today;
                this.dtpDataFinal.Value = DateTime.Today;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        
        #region "   Validar                 "

        private bool Validar_Filtro_Outros()
        {
            try
            {

                if ( this.cboLoja.SelectedValue.DefaultInteger() == 0
                        && this.cboUsuarioAprovacao.SelectedValue.DefaultInteger() == 0 
                        && ( this.txtValorInicial.Text == string.Empty || this.txtValorInicial.Text.DefaultDecimal() == 0)
                        && ( this.txtValorFinal.Text == string.Empty || this.txtValorFinal.Text.DefaultDecimal() == 0)
                        && this.chkPeriodo.Checked == false)
                {
                    if ((this.txtValorIndevidoInicial.Text == string.Empty || this.txtValorIndevidoInicial.Text.DefaultDecimal() == 0)
                        && (this.txtValorIndevidoFinal.Text == string.Empty || this.txtValorIndevidoFinal.Text.DefaultDecimal() == 0))
                    {
                        if (MessageBox.Show("Não foi informado nenhum filtro, essa operação pode demorar. Confirma operação?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("A consulta apenas por valor indevido pode demorar. Confirma operação?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            return false;
                        }
                    }
                }

                if (this.Validar_Data_Pesquisa() == false)
                {
                    return false;
                }

                if (this.Validar_Valor_Pesquisa() == false)
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

        private bool Validar_Valor_Pesquisa()
        {
            try
            {
                if (this.txtValorInicial.Text != string.Empty && this.txtValorFinal.Text == string.Empty)
                {
                    MessageBox.Show("Favor preencher o valor maximo do romaneio.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtValorFinal.Focus();
                    return false;
                }

                if (this.txtValorInicial.Text == string.Empty && this.txtValorFinal.Text != string.Empty)
                {
                    MessageBox.Show("Favor preencher o valor minimo do romaneio.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtValorInicial.Focus();
                    return false;
                }

                if (this.txtValorInicial.Text != string.Empty && this.txtValorFinal.Text != string.Empty && (this.txtValorInicial.Text.DefaultDecimal() > this.txtValorFinal.Text.DefaultDecimal()))
                {
                    MessageBox.Show("O valor minimo do romaneio não pode ser maior que o valor maximo.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtValorInicial.Focus();
                    return false;
                }

                if (this.txtValorIndevidoInicial.Text != string.Empty && this.txtValorIndevidoFinal.Text == string.Empty)
                {
                    MessageBox.Show("Favor preencher o valor indevido maximo do romaneio.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtValorIndevidoFinal.Focus();
                    return false;
                }

                if (this.txtValorIndevidoInicial.Text == string.Empty && this.txtValorIndevidoFinal.Text != string.Empty)
                {
                    MessageBox.Show("Favor preencher o valor indevido minimo do romaneio.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtValorIndevidoInicial.Focus();
                    return false;
                }

                if (this.txtValorIndevidoInicial.Text != string.Empty && this.txtValorIndevidoFinal.Text != string.Empty && (this.txtValorIndevidoInicial.Text.DefaultDecimal() > this.txtValorIndevidoFinal.Text.DefaultDecimal()))
                {
                    MessageBox.Show("O valor minimo indevido não pode ser maior que o valor maximo.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtValorIndevidoInicial.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        private bool Validar_Data_Pesquisa()
        {
            try
            {
                if (this.dtpDataFinal.Value < this.dtpDataInicial.Value)
                {
                    MessageBox.Show("A Data Final não pode ser menor do que a Data Inicial.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpDataInicial.Focus();
                    return false;

                }

                if (this.dtpDataFinal.Value > DateTime.Today)
                {
                    MessageBox.Show("A Data Final não pode ser maior que a data atual.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpDataFinal.Focus();
                    return false;

                }

                if (this.dtpDataInicial.Value > DateTime.Today)
                {
                    MessageBox.Show("A Data Inicial não pode ser maior que a data atual.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpDataInicial.Focus();
                    return false;

                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Validar_Data_Filtro()
        {
            try
            {
                if (this.dtpDataFinal.Value.CompareTo(this.dtpDataInicial.Value) < 0)
                {
                    MessageBox.Show("Data inicial não pode ser maior do que a data final.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    this.dtpDataInicial.Value = this.dtpDataFinal.Value;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void Validar_Exibir_Menu()
        {
            try
            {

                if (this.Grid.DataSource == null)
                {
                    this.tsmPropriedadesVenda.Enabled = false;

                    return;
                }

                if (this.Grid.Rows.Count == 0)
                {
                    this.tsmPropriedadesVenda.Enabled = false;

                    return;
                }

                if (!this.Grid.Columns.Contains("Romaneio_Grupo_ID"))
                {
                    this.tsmPropriedadesVenda.Enabled = false;

                    return;
                }

                this.tsmPropriedadesVenda.Enabled = true;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Permissões              "

        protected override void Tratar_Permissoes()
        {
            try
            {
                this.cboLoja.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, ((LojasDO)Root.Loja_Ativa_NEW).ID, this.Name, Acao_Formulario.Alterar_Loja_Ativa.ToString());
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion 

        #region "   Combo Tipo Exibicao     "

        private void Preencher_Combo_Tipo_Exibicao()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Utilizando_Enum(ref this.cboTipoExibicao, typeof(Exibicao_Listagem));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Combo_Tipo_Data()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Utilizando_Enum(ref this.cboTipoPeriodo, typeof(Tipo_Data));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Criar_Coluna_DataTable(DataTable dttTabela, string strNomeColuna, Type objTipoColuna, bool blnSomenteLeitura)
        {
            try
            {
                DataColumn dtcColuna = new DataColumn(strNomeColuna, objTipoColuna);
                dtcColuna.ReadOnly = blnSomenteLeitura;

                dttTabela.Columns.Add(dtcColuna);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Criar_Linha_DataTable(DataTable dttTabela, Dictionary<string, object> dicValores)
        {
            try
            {
                DataRow dtrLinha = dttTabela.NewRow();

                foreach (var objChaves in dicValores.Keys)
                {
                    dtrLinha[objChaves] = dicValores[objChaves];
                }

                dttTabela.Rows.Add(dtrLinha);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Manipular_Dicionario_Valores(Dictionary<string, object> dicDicionario, string strChave, string strValor)
        {
            try
            {
                dicDicionario.Add(strChave, strValor);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Ativar_Desativar_Opcao_Menu_Contexto(ToolStripMenuItem objOpcaoMenuContexto, bool blnStatus)
        {
            try
            {
                objOpcaoMenuContexto.Visible = blnStatus;
                objOpcaoMenuContexto.Enabled = blnStatus;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        
        #endregion

        #region "   Não Utilizados  "

        #endregion
    }
}
