using Mercadocar.Enumerados;
using Mercadocar.Herancas;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.Datable;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.Interfaces;
using Mercadocar.ObjetosNegocio.DataObject;
using Mercadocar.ObjetosNegocio.Gerencia.Pendencia_Validacao;
using Mercadocar.RegrasNegocio;
using Mercadocar.RegrasNegocio.Gerencia.Pendencia_Validacao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Mercadocar.Formularios
{
    public sealed partial class frmPendencia_Validacao_Grid : frmGrid, IfrmGrid
    {
        #region "   Declarações        "

        private bool blnPermissaoMenu;
        private int intValidacaoID = 0;

        #endregion

        #region "   Construtor         "

        public frmPendencia_Validacao_Grid()
            : base()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.InitializeComponent();

                this.Load += this.Form_Load;
                this.Shown += this.Form_Shown;

                this.cboLoja.SelectedValueChanged += this.Alterar_Valor_Loja;

                this.txtLote.KeyPress += DivUtil.Pressionar_Tecla_Permitindo_Apenas_Numerico;

                this.radPeriodoOrigem.CheckedChanged += this.Checar_RadioButton_Filtro;
                this.radPeriodoValidacao.CheckedChanged += this.Checar_RadioButton_Filtro;

                this.Grid.KeyDown += this.Grid_KeyDown;
                this.Grid.DoubleClick += this.Clicar_Menu_Propriedades;

                this.cmsMenu.Opened += this.ContextMenu_Popup;
                this.ContextMenuStrip = this.cmsMenu;
                this.tsmPropriedades.Click += this.Clicar_Menu_Propriedades;
                this.tsmValidarPendencia.Click += this.Clicar_Menu_Validar_Pendencia;

                this.btnCarregarDados.Click += this.Clicar_Botao_Carregar_Dados;

                
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

        private void Form_Shown(object sender, EventArgs e)
        {
            try
            {
                this.Grid.Focus();

            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Inicialização      "

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                this.Definir_Formulario_Estilo();

                this.Grid.MultiSelect = true;
                this.Preencher_Datas();
                this.Configurar_Grid();
                this.Tratar_Permissoes();

                this.Carregar_Combo_Loja();
                this.Carregar_Combo_Status();
                this.Carregar_Combo_Processo();
                this.Carregar_Combo_Usuario_Origem();
                this.Carregar_Combo_Usuario_Validacao();

                this.radPeriodoOrigem.Checked = true;
                this.radPeriodoValidacao.Checked = false;
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Eventos            "

        private void Alterar_Valor_Loja(Object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                this.Carregar_Combo_Usuario_Validacao();
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

        private void Checar_RadioButton_Filtro(Object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (object.ReferenceEquals(sender, this.radPeriodoOrigem))
            {
                this.Limpar_Campos();
                this.grbPeriodoOrigem.Enabled = true;
                this.grbPeriodoValidacao.Enabled = false;
            }
            else if (object.ReferenceEquals(sender, this.radPeriodoValidacao))
            {
                this.Limpar_Campos();
                this.grbPeriodoOrigem.Enabled = false;
                this.grbPeriodoValidacao.Enabled = true;
            }

            Cursor.Current = Cursors.Default;
        }

        public void Clicar_Botao_Carregar_Dados(object sender, System.EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!this.Validar_Consulta())
                {
                    return;
                }

                Pendencia_Filtro_GridDO consulta = new Pendencia_Filtro_GridDO();

                consulta.intLojaID = this.cboLoja.SelectedValue.DefaultInteger();
                consulta.intObjetoOrigemID = this.txtLote.Text.DefaultInteger();
                consulta.intStatusID = this.cboStatus.SelectedValue.DefaultInteger();
                consulta.intProcessoID = this.cboProcesso.SelectedValue.DefaultInteger();
                consulta.intUsuarioOrigemID = this.cboUsuarioOrigem.SelectedValue.DefaultInteger();
                consulta.dtDataInicialOrigem = this.radPeriodoOrigem.Checked ? this.dtpDataInicialOrigem.Value : new DateTime(1900, 1, 1);
                consulta.dtDataFinalOrigem = this.radPeriodoOrigem.Checked ? this.dtpDataFinalOrigem.Value : new DateTime(1900, 1, 1);
                consulta.intUsuarioValidacaoID = this.cboUsuarioValidacao.SelectedValue.DefaultInteger();
                consulta.dtDataInicialValidacao = this.radPeriodoValidacao.Checked ? this.dtpDataInicialValidacao.Value : new DateTime(1900, 1, 1);
                consulta.dtDataFinalValidacao = this.radPeriodoValidacao.Checked ? this.dtpDataFinalValidacao.Value : new DateTime(1900, 1, 1);


                DataTable dttResultado;

                dttResultado = new Pendencia_ValidacaoBUS().Consultar_DataTable_Grid(consulta).ToTable<Pendencia_ValidacaoDO>("Pendencia_Validacao");

                this.Grid.DataSource = dttResultado;

                if (dttResultado.Rows.Count == 0)
                {
                    MessageBox.Show("Não foram encontrados registros para os filtros informados.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                this.Grid.Focus();
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

        #region "   Menus de Contexto   "

        public void Clicar_Menu_Propriedades(object sender, System.EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.Configurar_Menus();
                if (!this.tsmPropriedades.Enabled)
                {
                    return;
                }

                if (!this.Validar_Grid())
                {
                    return;
                }

                this.Abrir_Propriedades();
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

        public void ContextMenu_Popup(object sender, System.EventArgs e)
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

        #endregion

        private void Clicar_Menu_Validar_Pendencia(object sender, System.EventArgs e)
        {
            try
            {
                UsuarioDO dtoUsuario = null;
                bool blnCarregarDados = false;

                Cursor.Current = Cursors.WaitCursor;

                foreach (DataGridViewRow dtrLinha in this.Grid.SelectedRows)
                {
                    if (dtrLinha.Cells["Status_ID"].Value.DefaultInteger() == Status_Validacao.Pendente.ToInteger())
                    {

                        if (MessageBox.Show("Confirma a validação do Lote número " + Convert.ToString(dtrLinha.Cells["Objeto_Origem_ID"].Value) + " ? ", "Validação de Lote de Pendência", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                        {
                            if (dtoUsuario == null)
                            {
                                dtoUsuario = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo);
                            }

                            if (blnPermissaoMenu == false)
                            {
                                MessageBox.Show("Usuário não possui permissão para esta ação!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            bool blnCarregarDadosTemp = this.Efetivar_Validacao(dtrLinha, dtoUsuario.ID);

                            if (blnCarregarDados == false & blnCarregarDadosTemp == true)
                            {
                                blnCarregarDados = blnCarregarDadosTemp;
                            }
                        }
                    }
                }

                if (blnCarregarDados)
                    this.btnCarregarDados.PerformClick();
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        public void Grid_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        if (this.tsmPropriedades.Enabled)
                        {
                            this.tsmPropriedades.PerformClick();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Root.Tratamento_Erro.Tratar_Erro(ex, this);
            }
        }

        #endregion

        #region "   Métodos            "

        private void Definir_Formulario_Estilo()
        {
            try
            {
                Form_Designer.Configurar_Designer_Padrao_MercadoCar(this);
                Form_Designer.Definir_Label_Estilo_Titulo_Grid(this.lblNomeTela);
                Form_Designer.Definir_Botao_Estilo_Carregar_Dados(this.btnCarregarDados);

                Form_Designer.Definir_ToolStripMenuItem_Estilo_Propriedades(this.tsmPropriedades);
                Form_Designer.Definir_ToolStripMenuItem_Estilo_Alterar(this.tsmValidarPendencia);
                Form_Designer.Definir_ToolStripMenuItem_Estilo_Ajuda(this.tsmAjuda);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void Tratar_Permissoes()
        {
            try
            {
                this.cboLoja.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(
                                                                                   Root.Funcionalidades.Usuario_Ativo,
                                                                                   Root.Loja_Ativa.ID,
                                                                                   this.Name,
                                                                                   Acao_Formulario.Alterar_Loja_Ativa.DefaultString());

                this.blnPermissaoMenu = Root.Permissao.Obter_Permissao_Do_Usuario(
                                                                                    Root.Funcionalidades.Usuario_Ativo, 
                                                                                    Root.Loja_Ativa.ID, 
                                                                                    this.Name, 
                                                                                    Acao_Formulario.Selecionar.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Configurar_Menus()
        {
            try
            {
                this.tsmPropriedades.Visible = (this.Grid.SelectedRows.Count > 0);
                this.tsmPropriedades.Enabled = this.blnPermissaoMenu;
                this.tsmValidarPendencia.Visible = (this.Grid.SelectedRows.Count > 0);
                this.tsmValidarPendencia.Enabled = (this.blnPermissaoMenu && this.Grid.SelectedRows[0].Cells["Status_ID"].Value.DefaultInteger() == Status_Validacao.Pendente.ToInteger());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Datas()
        {
            try
            {
                this.dtpDataInicialOrigem.Value = DateTime.Now.AddDays(-30).Date;
                this.dtpDataFinalOrigem.Value = DateTime.Now;

                this.dtpDataInicialValidacao.Value = DateTime.Now.AddDays(-30).Date;
                this.dtpDataFinalValidacao.Value = DateTime.Now;
            }
            catch (Exception)
            {

                throw;
            }

        }

        protected override void Configurar_Grid()
        {
            try
            {
                this.Grid.Columns.Clear();
                this.Grid.AutoGenerateColumns = false;
                this.Grid.ContextMenuStrip = this.cmsMenu;

                this.Grid.Adicionar_Coluna("Validacao_ID");
                this.Grid.Adicionar_Coluna("Status_ID");
                this.Grid.Adicionar_Coluna("Status_Descricao", "Status", 70, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.NotSet, strToolTipDescricao: "Status do crédito");
                this.Grid.Adicionar_Coluna("Lojas_ID");
                this.Grid.Adicionar_Coluna("Lojas_NM", "Loja", 100, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.NotSet, strToolTipDescricao: "Loja do crédito");
                this.Grid.Adicionar_Coluna("Processo_ID");
                this.Grid.Adicionar_Coluna("Processo_Descricao", "Processo", 140, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.NotSet, strToolTipDescricao: "Processo gerador do crédito");
                this.Grid.Adicionar_Coluna("Romaneio_Venda_Grupo_ID");
                this.Grid.Adicionar_Coluna("Objeto_Origem_ID", "Número Lote", 100, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.MiddleRight, strToolTipDescricao: "Número de lote do crédito");
                this.Grid.Adicionar_Coluna("Valor", "Valor (R$)", 110, false, Tipo_Coluna.Decimal, false, DataGridViewContentAlignment.MiddleRight, strToolTipDescricao: "Valor do crédito");
                this.Grid.Adicionar_Coluna("Usuario_Origem_ID");
                this.Grid.Adicionar_Coluna("Usuario_Origem_Nome", "Usuário Origem", 210, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.NotSet, strToolTipDescricao: "Usuário que originou o crédito");
                this.Grid.Adicionar_Coluna("Data_Origem", "Data Origem", 100, false, Tipo_Coluna.Data_Tempo, false, DataGridViewContentAlignment.MiddleRight, strToolTipDescricao: "Data da origem do crédito");
                this.Grid.Adicionar_Coluna("Usuario_Validacao_ID");
                this.Grid.Adicionar_Coluna("Usuario_Validacao_Nome", "Usuário Validação", 210, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.NotSet, strToolTipDescricao: "Usuário que autorizou o crédito");
                this.Grid.Adicionar_Coluna("Data_Validacao", "Data Validação", 100, false, Tipo_Coluna.Data_Tempo, false, DataGridViewContentAlignment.MiddleRight, strToolTipDescricao: "Data da autorização do crédito");
                this.Grid.Adicionar_Coluna("Observacao", "Observação Origem", 243, false, Tipo_Coluna.Texto, false, DataGridViewContentAlignment.NotSet, strToolTipDescricao: "Descrição sobre a pendência");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Limpar_Campos()
        {
            this.cboStatus.SelectedValue = 0;
            this.cboProcesso.SelectedValue = 0;
            this.cboUsuarioOrigem.SelectedValue = 0;
            this.cboUsuarioValidacao.SelectedValue = 0;
            this.Preencher_Datas();
        }

        private void Abrir_Propriedades()
        {
            try
            {
                if (this.Grid.SelectedRows.Count == 0 || this.blnPermissaoMenu == false)
                    return;

                frmRomaneio_Propriedades frmRomaneioPropriedades = new frmRomaneio_Propriedades(this.Grid.SelectedRows[0].Cells["Romaneio_Venda_Grupo_ID"].Value.DefaultInteger(),
                                                                                                this.Grid.SelectedRows[0].Cells["Objeto_Origem_ID"].Value.DefaultInteger(),
                                                                                                this.Grid.SelectedRows[0].Cells["Lojas_ID"].Value.DefaultInteger(),
                                                                                                true);
                frmRomaneioPropriedades.Show(this);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Efetivar_Validacao(DataGridViewRow dgrDataObject, int intUsuarioID)
        {
            try
            {
                this.intValidacaoID = dgrDataObject.Cells["Validacao_ID"].Value.DefaultInteger();

                Pendencia_ValidacaoBUS busValidacao = new Pendencia_ValidacaoBUS();

                busValidacao.Alterar(this.Obter_Dados_Tela(), intUsuarioID);

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private Pendencia_ValidacaoDO Obter_Dados_Tela()
        {
            try
            {
                Pendencia_PropriedadesBUS busPropriedadePendencia = new Pendencia_PropriedadesBUS();

                return busPropriedadePendencia.Carregar_Dados_Pendencia(intValidacaoID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region "   Filtros             "
        private void Carregar_Combo_Loja()
        {
            try
            {
                bool blnHabilitarOpcaoTodos = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo,
                                                                                        Root.Loja_Ativa.ID,
                                                                                        this.Name,
                                                                                        Acao_Formulario.Habilitar_Opcao_Todos_No_Combo.ToString());

                Utilitario.Preencher_ComboBox_Lojas_Ativas(ref this.cboLoja,
                                                           Utilitario.Colunas_Loja.Lojas_Contrata_Funcionario,
                                                           "true",
                                                           blnHabilitarOpcaoTodos,
                                                           "Todas",
                                                           true,
                                                           Root.Loja_Ativa.ID);

                this.cboLoja.Enabled = Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo,
                                                                                 Root.Loja_Ativa.ID,
                                                                                 this.Name,
                                                                                 Acao_Formulario.Alterar_Loja_Ativa.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Status()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Exibindo_Extenso(ref this.cboStatus, "Status_Validacao", "", true, "Todos", "Enum_Extenso");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Processo()
        {
            try
            {
                Utilitario.Preencher_ComboBox_Enumerado_Exibindo_Extenso(ref this.cboProcesso, "Processo_Validacao", "", true,"Todos", "Enum_Extenso");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Usuario_Origem()
        {
            try
            {
                DataTable dttUsuarios = new Pendencia_ValidacaoBUS().Consultar_Usuario_Origem().ToTable();
                Utilitario.Preencher_ComboBox_DataTable(ref this.cboUsuarioOrigem, dttUsuarios, "Usuario_Origem_Nome", "Usuario_Origem_ID", "Todos", "Todos");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_Combo_Usuario_Validacao()
        {
            try
            {
                DataTable dttResultado = new Pendencia_ValidacaoBUS().Consultar_Usuario_Validacao().ToTable();
                Utilitario.Preencher_ComboBox_DataTable(ref this.cboUsuarioValidacao, dttResultado, "Usuario_Validacao_Nome", "Usuario_Validacao_ID", "Todos", "Todos");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        private bool Validar_Grid()
        {
            if (this.Grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecione uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Grid.Focus();
                return false;
            }

            if (this.Grid.SelectedRows.Count > 1)
            {
                MessageBox.Show("Selecione somente uma linha", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Grid.Focus();
                return false;
            }

            return true;
        }

        private bool Validar_Consulta()
        {
            try
            {
                if (this.radPeriodoOrigem.Checked && this.dtpDataInicialOrigem.Value.Date > this.dtpDataFinalOrigem.Value.Date)
                {
                    MessageBox.Show("A data inicial não pode ser maior que a data final.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpDataFinalOrigem.Focus();
                    return false;
                }

                if (this.radPeriodoValidacao.Checked && this.dtpDataInicialValidacao.Value.Date > this.dtpDataFinalValidacao.Value.Date)
                {
                    MessageBox.Show("A data inicial não pode ser maior que a data final.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.dtpDataFinalValidacao.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region "   Não Utilizados     "

        public void Clicar_Menu_Excluir(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Clicar_Menu_Novo(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool Efetivar_Exclusao(DataGridViewRow DataObject)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
