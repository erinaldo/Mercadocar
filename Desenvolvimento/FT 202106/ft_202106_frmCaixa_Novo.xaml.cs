using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using Mercadocar.Constantes;
using Mercadocar.Enumerados;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.Caixa;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.ObjetosNegocio.DataObject;
using Mercadocar.RegrasNegocio;
using Mercadocar.Sitef;
using Mercadocar.Sat_Caixa_Fiscal;
using System.ServiceModel;
using Mercadocar.InfraEstrutura.Erro;
using System.Drawing.Printing;
using System.Windows.Interop;
using Mercadocar.InfraEstrutura.BancoDados;

namespace Mercadocar.Formularios.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    /// <history>
    /// [mmukuno] 28/10/2013 Created
    /// </history>
    public partial class frmCaixa_Novo : Window
    {

        #region "   Declarações     "

        #region "   Constantes      "

        ///Forma de pagamento para a impressora fiscal. Caso seja o simulador fiscal (exclusivamente para teste - ambiente de desenvolvimento, 
        ///utilizar "C. Crédito". Caso contrário, utilizar "CARTAO"
        ///"C. Crédito"
        private const string CARTAO_TEF_DS_FININVEST = "FININVEST";
        private const string CARTAO_DEBITO_BANDEIRA_AMERICAN_EXPRESS = "00004";
        private const string CARTAO_DEBITO_BANDEIRA_VISA = "00125";

        // Mensagens
        private const string MENSAGEM_OPERACAO_CAIXA_DISPONIVEL = "Caixa disponível. Próximo cliente";
        private const string MENSAGEM_OPERACAO_INVALIDA = "Operação inválida.";
        private const string MENSAGEM_OPERACAO_INVALIDA_CAIXA_ABERTO = "Operação inválida. Caixa Aberto.";
        private const string MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO = "Operação inválida. Caixa Fechado.";
        private const string MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO = "Operação inválida. Cupom Impresso.";
        private const string MENSAGEM_OPERACAO_INVALIDA_TIPO_SAT = "Operação inválida. Tipo Operação SAT.";
        private const string MENSAGEM_OPERACAO_SAT_VENDA_COM_FALHA = "Venda rejeitada. Encaminhar ao Caixa Central.";
        private const string MENSAGEM_OPERACAO_SAT_VENDA_VALOR_LIMITE = "Venda acima do limite SAT. Emitir Nota Fiscal? (S\\N)";

        private const string CODIGO_ERRO_COMUNICACAO_SAT_ENVIO = "163";
        private const string CODIGO_ERRO_COMUNICACAO_SAT_CONSULTA = "164";
        private const string CODIGO_ERRO_COMUNICACAO_SAT = "165";

        private const int ESTORNO_PAGAMENTO_DINHEIRO = 1;
        private const int ESTORNO_PAGAMENTO_A_VISTA = 1;
        private const int NUMERO_PADRAO_CARACTERES = 14;
        private const int NUMERO_PADRAO_CARACTERES_SERVICO = 3;
        private const int CARACTER_VIRGULA = 44;
        private const int NUMERO_MAX_LINHA = 40;
        private const int CODIGO_BARRAS_ALTURA = 8;
        private const int CODIGO_BARRAS_LARGURA = 2;
        private const int TENTATIVAS_COMUNICACAO_IMPRESSORA = 2;
        private const int TENTATIVAS_COMUNICACAO_SAT = 30;
        private const int TEMPO_AGUARDO_TENTATIVA_COMUNICACAO_SAT = 1000;

        private const string EXEC_MERCADOCAR = "Mercadocar.exe";
        private const string FORM_PARAMETRO = "CaixaNovo";

        const string MENSAGEM_ORIGEM = "CaixaNovo";

        private int QUANTIDADE_LINHAS_VISIVEIS = 18;

        private const int CODIGO_TIPO_OBJETO_PARA_SERVICO = 508;
        #endregion

        #region "   Enumerado       "

        private enum Tipo_Comprovante
        {
            Pacote,
            Servico
        }

        private enum Operacao
        {
            Operacao_Inicial,
            Autenticar_Abertura_Caixa,
            Autenticar_Cancelar_Cupom,
            Autenticar_Cancelar_Cupom_Suspender,
            Autenticar_Cancelar_Cupom_Item,
            Autenticar_Desconto,
            Autenticar_Estorno_Pagamento,
            Autenticar_Estorno_Pagamento_POS,
            Autenticar_Estorno_Pagamento_Credito,
            Autenticar_Fechamento_Operadora,
            Autenticar_Fechamento_Fiscal,
            Autenticar_Liberar_Credito_Em_Dinheiro,
            Autenticar_Liberar_POS,
            Autenticar_Reducao_Z,
            Autenticar_Sangria,
            Autenticar_Suspender,
            Cancelar_Item,
            Cancelar_Item_Repetido,
            Cancelar_Venda,
            Confirma_Fechamento_Venda,
            Confirma_Guiche,
            Confirma_Excluir_Pagamento,
            Confirma_Desconto_Valor,
            Confirma_Codigo_Cancelameto_POS,
            Confirma_CPF_CNPJ,
            Confirma_Venda_Limite_Sat,
            Comanda,
            Desconto_Item,
            Desconto_Valor,
            Especial,
            Estorno_Pagamento,
            Estorno_Pagamento_POS,
            Estorno_Pagamento_Cartao,
            Estorno_Pagamento_Cartao_Parte1,
            Estorno_Pagamento_Cartao_Parte2,
            Estorno_Pagamento_Cartao_POS_Parte1,
            Estorno_Pagamento_Cartao_POS_Parte2,
            Estorno_Pagamento_Resta,
            Excluir_Pagamento,
            Falha_Autenticar,
            Fechamento_Confirma_Sangria,
            Fechamento_Pagamentos_Confirma_Diferenca,
            Fechamento_Pagamentos_Confirma_Fiscal,
            Fechamento_Pagamentos_Confirma_Operadora,
            Fechamento_Pagamentos_Diferenca_Valores,
            Fechamento_Pagamentos_Diferenca_Valores_Sitef,
            Fechamento_Pagamentos_Fiscal_Valores,
            Fechamento_Pagamentos_Fiscal_Valores_Sitef,
            Fechamento_Pagamentos_Valores,
            Fechamento_Pagamentos_Valores_Sitef,
            Informa_CPF_CNPJ,
            Informa_Operadora_Cartao,
            Leitura_X,
            Numero_Guiche,
            Numero_Parcela,
            Pendente_POS,
            Reducao_Z,
            Retornar_Itens,
            Romaneio,
            Saldo_Inicial,
            Sangria_Valor_Operadora,
            Sangria_Valor_Fiscal,
            Sangria_Confirma_Valor_Operadora,
            Sangria_Confirma_Valor_Fiscal,
            Senha_Abertura_Caixa,
            Senha_Cancelar_Cupom,
            Senha_Cancelar_Cupom_Suspender,
            Senha_Cancelar_Cupom_Item,
            Senha_Desconto,
            Senha_Estorno_Pagamento,
            Senha_Estorno_Pagamento_POS,
            Senha_Estorno_Pagamento_Credito,
            Senha_Fechamento_Operadora,
            Senha_Fechamento_Fiscal,
            Senha_Liberar_Credito_Em_Dinheiro,
            Senha_Liberar_POS,
            Senha_Reducao_Z,
            Senha_Sangria,
            Senha_Suspender,
            Ticket_Estacionamento,
            Valida_CPF_CNPJ,
            Validar_Ticket_Estacionamento,
            Valor_Parcela,
            Verifica_Documento,
            Solicita_Documento,
            Confirmar_Produto_Reciclavel_Auto_Servico,
            Informar_Quantidade_Produto_Reciclavel_Auto_Servico,
            Confirmar_Produto_Reciclavel_Romaneio,
            Informar_Quantidade_Produto_Reciclavel_Romaneio
        }

        private enum Tipo_Estorna_Pagamento
        {
            Sitef,
            Dinheiro,
            POS,
            Outros
        }

        #endregion

        List<Key> colPressedKeys = new List<Key>();

        int intLojaID;
        int intUsuario;
        int intUltimaOperacao;
        int intCondicaoPagtoTroco;
        int intCondicaoPagtoID;
        int intFormaPagamentoID;
        int intOrcamentoCtID;
        int intTipoDocumento;
        int intItemGridCupom = 0;
        int intItemDesconto = 0;
        int intRomaneioComanda;
        int intItemPagamentoEstorno;
        int intEstornoCartaoCreditoID;
        int intQtdeRomaneiosComanda;
        int intUsuarioAprovacaoID = 0;
        int intCodigoDoItemSendoCancelado = 0;

        string strCaixa;
        string strCOO;
        string strECF;
        string strCpfCnpj;
        string strCpfCnpjNotaFiscalPaulista;
        string strClienteNotaFiscalPaulistaID;
        string strClienteNome;
        string strClienteID;
        string strDocumentoNumero;
        string strEstornoPagamentoRomaneioPreVendaCTID = string.Empty;
        string strItemPagamentoVenda;
        string strCodCancelamentoPOS;
        string strMensagemAutenticar;

        decimal dcmDescontoComercial;
        decimal dcmDescontoItemTotal;
        decimal dcmDescontoItem;
        decimal dcmSaldoInicial;
        decimal dcmTotalVenda;
        decimal dcmSangriaValorOperadora;
        decimal dcmSangriaValorFiscal;
        decimal dcmValorDinheiro = 0;
        decimal dcmParametroProcessoLimiteVendaSat = 0;
        decimal dcmValorLimiteAprovacao = 0;
        decimal dcmParametroProcessoValorSolicitaDocumento = 0;
        decimal dcmParamentroLojaPercentualDesconto = 0;
        decimal dcmParametroProcessoValorLimiteSangria = 0;
        decimal dcmPercentualDescontoFuncionario = 0;

        DateTime dtmDataLiberacao;
        DateTime dtmDataMovimento = new DateTime(1900, 1, 1);

        bool blnIsLiberacaoCreditoResta;

        bool blnCaixaAberto;
        bool blnCompraVista;
        bool blnCupomAberto;
        bool blnUtilizaNFp = false;
        bool blnNaoUtilizaNFp = false;
        bool blnUtilizaNFpRomaneio = false;
        bool blnTipoPessoaFisica = true;
        bool blnPortaImpressoraFiscal = true;
        bool blnVendaConsumo;
        bool blnPagamentoLiberado;
        bool blnRomaneioComanda;
        bool blnSitefAtivo = true;
        bool blnRomaneioEstorno = false;
        bool blnEstornoFinalizado = false;
        bool blnItemDigitado = false;
        bool blnPendentePOS = false;
        bool blnRomaneioEspecial = false;
        bool blnFechamentoSangria;
        bool blnAbrirCaixaSangria;
        bool blnFechamento;
        bool blnComando = false;
        bool blnSolicitarDocumento = false;
        bool blnPendenciaAtualizaDataMovimento = false;
        bool blnSangriaRealizada = true;
        bool blnFechamentoBloquearCaixa = false;
        bool blnEnvioVendaSatRealizada = false;
        bool blnParametroProcessoCriarLog = false;
        bool blnIsFuncionario = false;
        bool blnUtilizaControladorSat = false;
        bool blnParametroHistoricoProcesso = false;
        bool blnConsultarItemPorCodigoPecaouCodigoServico = false;


        //Impressao Procedimentro Garantia
        bool blnParametroImprimeProcedimentoGarantia = false;

        bool produtoReciclavel = false;
        bool blnAcordoProdutoReciclavel = false;
        bool blnUtilizaProdutoReciclavel = false;

        // Guiche
        bool blnProximoCliente;
        int intNumeroGuiche;
        int intPortaPainelGuiche;
        string strIPPainelGuiche;

        // Controle da Cancela
        bool blnControleCancela;

        // Trata erro impressora fiscal;
        bool blnErroImpressoraFiscal;

        Operacao enuSituacao;
        Operacao enuSituacaoSub;

        SitefDO dtoSitefEstorno;
        SAT_VendaDO dtoCaixaSatVenda;
        Tipo_Processo_Sat_Fiscal_Factory objTipoProcessoSatFiscal;
        SatDO dtoSatCaixa;

        DataTable dttContato;
        DataTable dttCupomFiscal;
        DataTable dttCupomFiscalFechamento;
        DataTable dttDadosCartao;
        DataTable dttOperadoraCartao;
        DataTable dttOperadoraCartaoRegra;

        DataSet dtsFormaXCliente;
        DataSet dtsCadastroCartao;
        DataSet dtsCaixaTemporario;
        DataSet dtsCaixaOperacao;
        DataSet dtsCliente;
        DataSet dtsConsultaPecaItens;

        DataSet dtsGridVenda;
        DataSet dtsCondicaoPagto;
        DataSet dtsRomaneioEstorno;

        DataSet dtsOrcamentoIt; // Auto Serviço
        DataSet dtsPreVendaTemporario; // Pre Venda
        DataSet dtsPreVendaEscolhido;
        DataSet dtsRomaneioTemporario; // Romaneio 

        DataSet dtsFechamentoSistema;
        DataSet dtsFechamentoPagamentosInformados;
        DataSet dtsFechamentoPagamentosCadastrados;

        Caixa_Comunicacao_Impressora_Fiscal objComunicacaoImpressoraFiscal;
        Caixa_Tipo_Impressora_Fiscal objTipoImpressoraFiscal;

        CaixaFactory objImpressaoFiscal;
        UsuarioDO dtoUsuario;
        UsuarioDO dtoUsuarioAutenticar;

        DispatcherTimer tmrMonitorSitef = new DispatcherTimer();
        DispatcherTimer tmrRelogio;
        Stopwatch tmrTeclado;

        TimeSpan objTempoTeclado;
        private bool cpfCnpjNfpAlterado;

        private delegate void DelegateThread_Status_Sitef();
        private delegate void DelegateSetarImagemIconeSitef(string source, string TextToolTip);

        #endregion

        #region "   Construtor      "

        public frmCaixa_Novo()
        {
            this.InitializeComponent();

            try
            {
                this.Loaded += this.Window_Loaded;
                this.KeyDown += this.Window_KeyDown;
                this.KeyUp += this.Window_KeyUp;

                this.txtComando.KeyUp += this.Perder_Foco_Campo_Comando;
                this.txtComando.KeyDown += this.Pressionar_Tecla_Permitindo_Apenas_Numerico;

                this.txtSenha.KeyUp += this.Perder_Foco_Campo_Senha;
                this.txtSenha.KeyDown += this.Pressionar_Tecla_Permitindo_Apenas_Numerico;

                this.txtMatricula.KeyUp += this.Perder_Foco_Campo_Matricula;
                this.txtMatricula.KeyDown += this.Pressionar_Tecla_Permitindo_Apenas_Numerico;

                this.txtCodigoItemFabricante.KeyUp += this.Perder_Foco_Campo_Codigo_Item_Fabricante;

                this.txtQuantidade.KeyUp += this.Perder_Foco_Campo_Quantidade;
                this.txtQuantidade.KeyDown += this.Pressionar_Tecla_Permitindo_Apenas_Numerico;

                this.blnErroImpressoraFiscal = false;

            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }
        }

        #endregion

        #region "   Inicialização   "

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;

                this.Localizar_LojaAtiva();

                this.Preencher_Dados_Rodape();

                this.Inicializar_Paramentros();

                // Identifica o usuário logado
                this.intUsuario = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID;

                this.Inicilizar_Objetos_Comunicacao_Impressora_Fiscal();

                this.Criar_Inicilizar_Objetos();

                if (!this.Validar_Sat())
                {
                    return;
                }

                this.blnCaixaAberto = false;
                this.Name = "frmCaixa_Novo";
                this.txtSenha.Visibility = Visibility.Hidden;
                this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;

                this.Inicializar_Relogio();
                this.Inicializar_Monitor_Sitef();

                if (!this.Abrir_Porta_Impressora_Fiscal())
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }
            finally
            {
                this.Cursor = Cursors.None;
            }
        }

        #endregion

        #region "   Eventos         "

        #region     "   Timer   "

        private void tmrRelogio_Elapsed(object sender, EventArgs e)
        {
            try
            {
                this.txtRelogio.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }
        }

        private void trmMonitor_Sitef_Elapsed(object sender, EventArgs e)
        {
            try
            {
                // Verificar o status da tabela Sitef_Status 
                DataRow dtrSitefStatus = new Sitef_StatusBUS().Selecionar_Por_Loja(this.intLojaID);

                if (dtrSitefStatus["Enum_Status_ID"].DefaultInteger() == Status_Sitef.Online.DefaultInteger())
                {
                    this.Setar_Imagem_Icone_Sitef("/MC_Formularios_Wpf;component/Images/MDI/Icone_Online.png", "Sitef Ativo");
                }
                else
                {
                    this.Setar_Imagem_Icone_Sitef("/MC_Formularios_Wpf;component/Images/MDI/Icone_Offline.png", "Sitef Inativo");
                }
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }
        }

        #endregion

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {

                if (this.colPressedKeys.Contains(e.Key))
                {
                    return;
                }

                this.colPressedKeys.Add(e.Key);
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                this.Tratar_Operacao_Desligar();
                this.Tratar_Operacao_Reiniciar();

                if (this.enuSituacao != Operacao.Senha_Abertura_Caixa
                        && this.enuSituacao != Operacao.Senha_Suspender
                        && this.enuSituacao != Operacao.Senha_Cancelar_Cupom_Suspender
                        && this.enuSituacao != Operacao.Senha_Cancelar_Cupom
                        && this.enuSituacao != Operacao.Senha_Cancelar_Cupom_Item
                        && this.enuSituacao != Operacao.Senha_Liberar_Credito_Em_Dinheiro
                        && this.enuSituacao != Operacao.Senha_Reducao_Z
                        && this.enuSituacao != Operacao.Senha_Desconto
                        && this.enuSituacao != Operacao.Senha_Estorno_Pagamento
                        && this.enuSituacao != Operacao.Senha_Estorno_Pagamento_Credito
                        && this.enuSituacao != Operacao.Senha_Liberar_POS
                        && this.enuSituacao != Operacao.Senha_Estorno_Pagamento_POS
                        && this.enuSituacao != Operacao.Senha_Sangria
                        && this.enuSituacao != Operacao.Senha_Fechamento_Operadora
                        && this.enuSituacao != Operacao.Senha_Fechamento_Fiscal
                    )
                {
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                }

                this.Cursor = Cursors.Wait;

                switch (e.Key)
                {
                    case Key.C:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Abertura_Caixa();
                            }
                        }
                        break;
                    case Key.P:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa && this.strCpfCnpjNotaFiscalPaulista.IsNullOrEmpty())
                            {
                                this.Tratar_Operacao_Documento();
                            }
                        }
                        break;
                    case Key.Q:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Quantidade_Peca();
                            }
                        }
                        break;
                    case Key.D:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Comanda();
                            }
                        }
                        break;
                    case Key.J:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Romaneio();
                            }
                        }
                        break;
                    case Key.K:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Desconto_Peca();
                            }
                        }
                        break;
                    case Key.V:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Sangria();
                            }
                        }
                        break;
                    case Key.M:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Fechamento();
                            }
                        }
                        break;
                    case Key.Back:
                        this.Tratar_Operacao_Limpar_Campo();
                        break;
                    case Key.U:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Suspender();
                            }
                        }
                        break;
                    case Key.Y:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Voltar();
                            }
                        }
                        break;
                    case Key.S:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            this.Tratar_Operacao_Confirmacao();
                        }
                        break;
                    case Key.N:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            this.Tratar_Operacao_Negacao();
                        }
                        break;
                    case Key.L:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Liberar_Venda();
                            }
                        }
                        break;
                    case Key.R:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Pagamento_Credito();
                            }
                        }
                        break;
                    case Key.H:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Pagamento_Dinheiro();
                            }
                        }
                        break;
                    case Key.B:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Pagamento_Debito();
                            }
                        }
                        break;
                    case Key.E:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Excluir_Pagamento();
                            }
                        }
                        break;
                    case Key.F:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Finalizar_Venda();
                                this.Registrar_Historico_Processo(Historico_Operacao.Fim_Atendimento);
                            }
                        }
                        break;
                    case Key.I:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Cancelar_Item_Cupom();
                            }
                        }
                        break;
                    case Key.A:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Cancelar_Venda();
                            }
                        }
                        break;
                    case Key.Z:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Reducao_Z();
                            }
                        }
                        break;
                    case Key.X:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Leitura_X();
                            }
                        }
                        break;
                    case Key.T:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Ticket_Estacionamento();
                            }
                        }
                        break;
                    case Key.W:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Solicitar_Documento();
                            }
                        }
                        break;
                    case Key.G:
                        if (this.blnComando)
                        {
                            this.blnComando = false;
                            if (!this.blnFechamentoBloquearCaixa)
                            {
                                this.Tratar_Operacao_Painel_Guiche();
                            }
                        }
                        break;
                    case Key.LeftAlt:
                        this.blnComando = true;
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {

                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
                this.blnFechamentoBloquearCaixa = false;
            }
            finally
            {
                // this.colPressedKeys.Clear();
                this.Cursor = Cursors.None;
            }
        }

        private void Perder_Foco_Campo_Comando(object sender, KeyEventArgs e)
        {
            try

            {
                this.txtComando.KeyUp -= this.Perder_Foco_Campo_Comando;

                this.Cursor = Cursors.Wait;

                this.Tratar_Operacao_Desligar();
                this.Tratar_Operacao_Reiniciar();

                if (this.txtComando.Text != string.Empty && e.Key == Key.Enter)
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, false, false);

                    switch (this.enuSituacao)
                    {
                        case Operacao.Autenticar_Abertura_Caixa:
                        case Operacao.Autenticar_Suspender:
                        case Operacao.Autenticar_Cancelar_Cupom_Suspender:
                        case Operacao.Autenticar_Cancelar_Cupom:
                        case Operacao.Autenticar_Cancelar_Cupom_Item:
                        case Operacao.Autenticar_Liberar_Credito_Em_Dinheiro:
                        case Operacao.Autenticar_Reducao_Z:
                        case Operacao.Autenticar_Desconto:
                        case Operacao.Autenticar_Estorno_Pagamento:
                        case Operacao.Autenticar_Estorno_Pagamento_POS:
                        case Operacao.Autenticar_Estorno_Pagamento_Credito:
                        case Operacao.Autenticar_Liberar_POS:
                        case Operacao.Autenticar_Sangria:
                        case Operacao.Autenticar_Fechamento_Operadora:
                        case Operacao.Autenticar_Fechamento_Fiscal:
                            this.Tratar_Campo_Comando_Autenticar();
                            break;

                        case Operacao.Sangria_Valor_Operadora:
                            this.Tratar_Campo_Comando_Sangria_Operadora();
                            break;

                        case Operacao.Sangria_Confirma_Valor_Operadora:
                            this.Tratar_Campo_Comando_Confirma_Sangria_Operadora();
                            break;

                        case Operacao.Sangria_Valor_Fiscal:
                            this.Tratar_Campo_Comando_Sangria_Fiscal();
                            break;

                        case Operacao.Sangria_Confirma_Valor_Fiscal:
                            this.Tratar_Campo_Comando_Confirma_Sangria_Fiscal();
                            break;
                        case Operacao.Saldo_Inicial:
                            this.Tratar_Campo_Comando_Saldo_Inicial();
                            break;

                        case Operacao.Valida_CPF_CNPJ:
                            this.Tratar_Campo_Comando_Validar_Documento();
                            break;

                        case Operacao.Confirma_CPF_CNPJ:
                            this.Tratar_Campo_Comando_Confirmar_Documento();
                            break;

                        case Operacao.Informa_CPF_CNPJ:
                            this.Tratar_Campo_Comando_Informar_Documento();
                            break;

                        case Operacao.Verifica_Documento:
                            this.Registrar_Historico_Processo(Historico_Operacao.Inicio_Atendimento);
                            this.Tratar_Campo_Comando_Verificar_Documento();
                            break;

                        case Operacao.Comanda:
                            this.Tratar_Campo_Comando_Comanda();
                            break;

                        case Operacao.Romaneio:
                            this.Tratar_Campo_Comando_Romaneio();
                            break;

                        case Operacao.Confirma_Fechamento_Venda:
                            this.Tratar_Campo_Comando_Confirmar_Fechamento_Venda();
                            break;

                        case Operacao.Numero_Parcela:
                            this.Tratar_Campo_Comando_Numero_Parcela();
                            break;

                        case Operacao.Valor_Parcela:
                            this.Tratar_Campo_Comando_Valor_Parcela();
                            break;

                        case Operacao.Cancelar_Item:
                            this.Tratar_Campo_Comando_Cancelar_Item_Pelo_Codigo_Barras();
                            break;

                        case Operacao.Cancelar_Item_Repetido:
                            this.Tratar_Campo_Comando_Cancelar_Item();
                            this.intCodigoDoItemSendoCancelado = 0;
                            break;

                        case Operacao.Desconto_Item:
                            this.Tratar_Campo_Comando_Desconto_Item();
                            break;

                        case Operacao.Desconto_Valor:
                            this.Tratar_Campo_Comando_Desconto_Valor();
                            break;

                        case Operacao.Confirma_Desconto_Valor:
                            this.Tratar_Campo_Comando_Confirmar_Desconto();
                            break;

                        case Operacao.Leitura_X:
                            this.Tratar_Campo_Comando_Leitura_X();
                            break;

                        case Operacao.Excluir_Pagamento:
                            this.Tratar_Campo_Comando_Excluir_Pagamento();
                            break;

                        case Operacao.Confirma_Excluir_Pagamento:
                            this.Tratar_Campo_Comando_Confirma_Excluir_Pagamento();
                            break;

                        case Operacao.Validar_Ticket_Estacionamento:
                            this.Tratar_Campo_Comando_Validar_Ticket_Estacionamento();
                            break;

                        case Operacao.Solicita_Documento:
                            this.Tratar_Campo_Comando_Solicitar_Documento();
                            break;

                        case Operacao.Ticket_Estacionamento:
                            this.Tratar_Campo_Comando_Ticket_Estacionamento();
                            break;

                        case Operacao.Numero_Guiche:
                            this.Tratar_Campo_Comando_Numero_Guiche();

                            break;
                        case Operacao.Confirma_Guiche:
                            this.Tratar_Campo_Comando_Confirma_Guiche();
                            break;

                        case Operacao.Estorno_Pagamento:
                            this.Tratar_Campo_Comando_Estorno_Pagamento();
                            break;

                        case Operacao.Estorno_Pagamento_Cartao_Parte1:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_Cartao_Parte1();
                            break;

                        case Operacao.Estorno_Pagamento_Cartao_Parte2:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_Cartao_Parte2();
                            break;

                        case Operacao.Estorno_Pagamento_Cartao_POS_Parte1:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_Cartao_POS_Parte1();
                            break;

                        case Operacao.Estorno_Pagamento_Cartao_POS_Parte2:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_Cartao_POS_Parte2();
                            break;

                        case Operacao.Estorno_Pagamento_POS:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_POS();
                            break;

                        case Operacao.Confirma_Codigo_Cancelameto_POS:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_Confirma_Codigo_Cancelameto_POS();
                            break;

                        case Operacao.Estorno_Pagamento_Resta:
                            this.Tratar_Campo_Comando_Estorno_Pagamento_Resta();
                            break;

                        case Operacao.Pendente_POS:
                            this.Tratar_Campo_Comando_Pendente_POS();
                            break;

                        case Operacao.Cancelar_Venda:
                            this.Tratar_Campo_Comando_Cancelar_Venda();
                            break;

                        case Operacao.Fechamento_Confirma_Sangria:
                            this.Tratar_Campo_Comando_Fechamento_Confirma_Sangria();
                            break;
                        case Operacao.Fechamento_Pagamentos_Valores:
                        case Operacao.Fechamento_Pagamentos_Valores_Sitef:
                            this.Tratar_Campo_Comando_Fechamento_Pagamentos_Valores();
                            break;

                        case Operacao.Fechamento_Pagamentos_Diferenca_Valores:
                        case Operacao.Fechamento_Pagamentos_Diferenca_Valores_Sitef:
                            this.Tratar_Campo_Comando_Fechamento_Pagamentos_Diferenca_Valores();
                            break;

                        case Operacao.Fechamento_Pagamentos_Fiscal_Valores:
                        case Operacao.Fechamento_Pagamentos_Fiscal_Valores_Sitef:
                            this.Tratar_Campo_Comando_Fechamento_Pagamentos_Fiscal_Valores();
                            break;

                        case Operacao.Fechamento_Pagamentos_Confirma_Diferenca:
                            this.Tratar_Campo_Comando_Fechamento_Pagamentos_Confirma_Diferenca();
                            break;

                        case Operacao.Fechamento_Pagamentos_Confirma_Operadora:
                            this.Tratar_Campo_Comando_Fechamento_Pagamentos_Confirma_Operadora();
                            break;
                        case Operacao.Fechamento_Pagamentos_Confirma_Fiscal:
                            this.Tratar_Campo_Comando_Fechamento_Pagamentos_Confirma_Fiscal();
                            break;
                        case Operacao.Retornar_Itens:
                            this.Tratar_Campo_Comando_Retornar_Itens();
                            break;
                        case Operacao.Informa_Operadora_Cartao:
                            this.Tratar_Campo_Comando_Informa_Operadora_Cartao();
                            break;
                        case Operacao.Confirma_Venda_Limite_Sat:
                            this.Tratar_Campo_Comando_Confirma_Venda_Limite_Sat();
                            break;
                        case Operacao.Confirmar_Produto_Reciclavel_Auto_Servico:
                            this.Tratar_Campo_Comando_Confirma_Reciclavel_Auto_Servico();
                            break;
                        case Operacao.Informar_Quantidade_Produto_Reciclavel_Auto_Servico:
                            this.Tratar_Campo_Comando_Confirmar_Quantidade_Reciclavel_Auto_Servico();
                            break;
                        case Operacao.Confirmar_Produto_Reciclavel_Romaneio:
                            this.Tratar_Campo_Comando_Confirmar_Reciclavel_Romneio();
                            break;
                        case Operacao.Informar_Quantidade_Produto_Reciclavel_Romaneio:
                            this.Tratar_Campo_Comando_Confirmar_Quantidade_Reciclaval_Romaneio();
                            break;

                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
                this.blnFechamentoBloquearCaixa = false;
                this.txtComando.Text = string.Empty;
                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            finally
            {
                if (e.Key == Key.Enter && this.enuSituacao != Operacao.Confirma_CPF_CNPJ && this.enuSituacao != Operacao.Valida_CPF_CNPJ && this.enuSituacao != Operacao.Valor_Parcela && this.enuSituacao != Operacao.Informa_Operadora_Cartao)
                {
                    this.txtComando.Clear();
                }
                this.Cursor = Cursors.None;
                this.colPressedKeys.Clear();
                this.txtComando.KeyUp += this.Perder_Foco_Campo_Comando;
            }
        }

        private void Perder_Foco_Campo_Senha(object sender, KeyEventArgs e)
        {
            try
            {
                this.txtSenha.KeyUp -= this.Perder_Foco_Campo_Senha;

                this.Cursor = Cursors.Wait;

                this.Tratar_Operacao_Desligar();
                this.Tratar_Operacao_Reiniciar();

                if (this.txtSenha.Password != string.Empty && e.Key == Key.Enter)
                {
                    if (this.Validar_Usuario_Mercadocar_AD(this.dtoUsuarioAutenticar.Login.ToLower(), this.txtSenha.Password.Trim()))
                    {
                        switch (this.enuSituacao)
                        {
                            case Operacao.Senha_Abertura_Caixa:
                                this.Tratar_Campo_Senha_Abertura_Caixa();
                                break;

                            case Operacao.Senha_Suspender:
                                this.Tratar_Campo_Senha_Suspender();
                                break;

                            case Operacao.Senha_Cancelar_Cupom_Suspender:
                                this.Tratar_Campo_Senha_Cancelar_Cupom_Suspender();
                                break;

                            case Operacao.Senha_Cancelar_Cupom:
                                this.Registrar_Historico_Processo(Historico_Operacao.Cancelmaneto_Atendimento);
                                this.Tratar_Campo_Senha_Cancelar_Cupom();
                                break;

                            case Operacao.Senha_Cancelar_Cupom_Item:
                                this.Tratar_Campo_Senha_Cancelar_Cupom_Item();
                                break;

                            case Operacao.Senha_Liberar_Credito_Em_Dinheiro:
                                this.Tratar_Campo_Senha_Liberar_Credito_Em_Dinheiro();
                                break;

                            case Operacao.Senha_Reducao_Z:
                                this.Tratar_Campo_Senha_Reducao_Z();
                                break;

                            case Operacao.Senha_Desconto:
                                this.Tratar_Campo_Senha_Desconto();
                                break;

                            case Operacao.Senha_Estorno_Pagamento:
                                this.Tratar_Campo_Senha_Estorno_Pagamento();
                                break;

                            case Operacao.Senha_Estorno_Pagamento_Credito:
                                this.Tratar_Campo_Senha_Estorno_Pagamento_Credito();
                                break;

                            case Operacao.Senha_Estorno_Pagamento_POS:
                                this.Tratar_Campo_Senha_Estorno_Pagamento_POS();
                                break;

                            case Operacao.Senha_Liberar_POS:
                                this.Tratar_Campo_Senha_Liberar_POS();
                                break;

                            case Operacao.Senha_Sangria:
                                this.Tratar_Campo_Senha_Sangria();
                                break;

                            case Operacao.Senha_Fechamento_Operadora:
                                this.Tratar_Campo_Senha_Fechamento_Operadora();
                                break;

                            case Operacao.Senha_Fechamento_Fiscal:
                                this.Tratar_Campo_Senha_Fechamento_Fiscal();
                                break;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        this.txtMenu.Content = "Login ou senha incorretos";
                        this.txtMatricula.Clear();
                        this.enuSituacao = Operacao.Falha_Autenticar;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                    }
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
                this.blnFechamentoBloquearCaixa = false;
                this.txtSenha.Clear();
                this.txtSenha.Visibility = Visibility.Hidden;
                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            finally
            {
                this.Cursor = Cursors.None;
                this.colPressedKeys.Clear();
                this.txtSenha.KeyUp += this.Perder_Foco_Campo_Senha;
            }
        }

        private void Perder_Foco_Campo_Matricula(object sender, KeyEventArgs e)
        {
            try
            {
                this.txtMatricula.KeyUp -= this.Perder_Foco_Campo_Matricula;

                this.Cursor = Cursors.Wait;

                this.Tratar_Operacao_Desligar();
                this.Tratar_Operacao_Reiniciar();

                if (this.txtMatricula.Password != string.Empty && e.Key == Key.Enter)
                {
                    switch (this.enuSituacao)
                    {
                        case Operacao.Autenticar_Abertura_Caixa:
                        case Operacao.Autenticar_Suspender:
                        case Operacao.Autenticar_Cancelar_Cupom_Suspender:
                        case Operacao.Autenticar_Cancelar_Cupom:
                        case Operacao.Autenticar_Cancelar_Cupom_Item:
                        case Operacao.Autenticar_Liberar_Credito_Em_Dinheiro:
                        case Operacao.Autenticar_Reducao_Z:
                        case Operacao.Autenticar_Desconto:
                        case Operacao.Autenticar_Estorno_Pagamento:
                        case Operacao.Autenticar_Estorno_Pagamento_POS:
                        case Operacao.Autenticar_Estorno_Pagamento_Credito:
                        case Operacao.Autenticar_Liberar_POS:
                        case Operacao.Autenticar_Sangria:
                        case Operacao.Autenticar_Fechamento_Operadora:
                        case Operacao.Autenticar_Fechamento_Fiscal:
                            this.Tratar_Campo_Comando_Autenticar();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (e.Key == Key.Enter && this.enuSituacao.Equals(Operacao.Falha_Autenticar))
                    {
                        this.txtMenu.Content = this.strMensagemAutenticar;
                        this.enuSituacao = this.enuSituacaoSub;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
                this.blnFechamentoBloquearCaixa = false;
                this.txtMatricula.Clear();
                this.txtMatricula.Visibility = Visibility.Hidden;
                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            finally
            {
                this.Cursor = Cursors.None;
                this.colPressedKeys.Clear();
                this.txtMatricula.KeyUp += this.Perder_Foco_Campo_Matricula;
            }
        }

        private void Perder_Foco_Campo_Codigo_Item_Fabricante(object sender, KeyEventArgs e)
        {
            try
            {
                this.txtCodigoItemFabricante.KeyUp -= this.Perder_Foco_Campo_Codigo_Item_Fabricante;

                this.Cursor = Cursors.Wait;

                this.Tratar_Operacao_Desligar();
                this.Tratar_Operacao_Reiniciar();

                if (e.Key == Key.Enter)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.txtCodigoItemFabricante.Text == string.Empty)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnRomaneioEspecial)
                    {
                        this.txtMenu.Content = "Operação inválida. Romaneio especial.";
                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    else
                    {
                        this.tmrTeclado.Stop();
                        this.objTempoTeclado = this.tmrTeclado.Elapsed;
                        this.tmrTeclado.Reset();
                        if (this.objTempoTeclado.TotalSeconds > 1)
                        {
                            this.blnItemDigitado = true;
                        }
                    }

                    if (!this.blnUtilizaNFp && !this.blnNaoUtilizaNFp)
                    {
                        if (this.Verifica_Cupom_Fiscal_Aberto())
                        {
                            this.blnCupomAberto = true;
                            this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                            return;
                        }

                        if (this.Validar_Peca_Servico() && this.Validar_Peca_Cadastrada())
                        {
                            this.txtMenu.Content = "Deseja Nota Fiscal Paulista? (S\\N)";
                            this.enuSituacao = Operacao.Verifica_Documento;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            if (this.Buscar_Preencher_Peca_Por_Codigo_Barras())
                            {
                                this.Preencher_DataRow_ItemOrcamento_Novo();

                                if (this.Preenchar_Grid_Itens_Venda("Orcamento"))
                                {
                                    this.Calcular_Valor_Total_Orcamento();

                                    if (this.Verificar_Produto_Reciclavel_Por_Auto_Servico())
                                    {
                                        return;
                                    }
                                }
                            }

                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.txtQuantidade.Text = string.Empty;
                            this.enuSituacao = Operacao.Operacao_Inicial;
                            this.txtCodigoItemFabricante.Focus();
                        }
                    }
                    else
                    {
                        if (this.Buscar_Preencher_Peca_Por_Codigo_Barras())
                        {
                            this.Preencher_DataRow_ItemOrcamento_Novo();

                            if (this.Preenchar_Grid_Itens_Venda("Orcamento"))
                            {
                                this.Calcular_Valor_Total_Orcamento();

                                if (this.Verificar_Produto_Reciclavel_Por_Auto_Servico())
                                {
                                    return;
                                }
                            }

                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.txtQuantidade.Text = string.Empty;
                            this.txtCodigoItemFabricante.Focus();
                        }
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
                else if (this.txtCodigoItemFabricante.Text != string.Empty)
                {
                    if (this.enuSituacao.Equals(Operacao.Operacao_Inicial))
                    {
                        this.txtMenu.Content = string.Empty;
                    }

                    if (this.txtCodigoItemFabricante.Text.Length <= 1)
                    {

                        this.tmrTeclado = new Stopwatch();
                        this.tmrTeclado.Start();
                    }
                }

            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }
            finally
            {
                this.colPressedKeys.Clear();
                this.Cursor = Cursors.None;
                this.txtCodigoItemFabricante.KeyUp += this.Perder_Foco_Campo_Codigo_Item_Fabricante;
            }
        }

        private void Perder_Foco_Campo_Quantidade(object sender, KeyEventArgs e)
        {
            try
            {
                this.txtQuantidade.KeyUp -= this.Perder_Foco_Campo_Quantidade;

                if (this.txtQuantidade.Text != string.Empty && e.Key == Key.Enter)
                {
                    this.txtCodigoItemFabricante.Focus();
                }
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }
            finally
            {
                this.txtQuantidade.KeyUp += this.Perder_Foco_Campo_Quantidade;
            }
        }

        private void Pressionar_Tecla_Permitindo_Apenas_Numerico(object sender, KeyEventArgs e)
        {
            try
            {
                Int16 intKeyAscii = 0;
                DivUtil objUtil = new DivUtil();

                this.colPressedKeys.Add(e.Key);

                // Campo Decimal. Permite apenas duas casas decimais.
                if (this.enuSituacao == Operacao.Saldo_Inicial
                    || this.enuSituacao == Operacao.Valor_Parcela
                    || this.enuSituacao == Operacao.Desconto_Valor
                    || this.enuSituacao == Operacao.Sangria_Valor_Operadora
                    || this.enuSituacao == Operacao.Sangria_Valor_Fiscal
                    || this.enuSituacao == Operacao.Fechamento_Pagamentos_Valores
                    || this.enuSituacao == Operacao.Fechamento_Pagamentos_Diferenca_Valores
                    || this.enuSituacao == Operacao.Fechamento_Pagamentos_Fiscal_Valores)
                {
                    if (this.txtComando.Text.Contains(","))
                    {
                        intKeyAscii = (Int16)objUtil.Permitir_Digitacao_Somente_Numeros(Convert.ToInt16(Strings.Asc(Utilitario.GetCharFromWpfKey(e.Key))));

                        if (this.txtComando.Text.Substring(this.txtComando.Text.IndexOf(",", 0) + 1).Length >= 2)
                        {
                            intKeyAscii = 0;
                        }
                    }
                    else
                    {
                        intKeyAscii = (Int16)objUtil.Permitir_Digitacao_Somente_Numeros_Com_Virgula(Convert.ToInt16(Strings.Asc(Utilitario.GetCharFromWpfKey(e.Key))));

                        // Exige informar um numero antes a virgula
                        if (intKeyAscii == CARACTER_VIRGULA && this.txtComando.Text.Length == 0)
                        {
                            intKeyAscii = 0;
                        }
                    }
                }
                else if (this.enuSituacao.Equals(Operacao.Leitura_X)
                            || this.enuSituacao.Equals(Operacao.Confirma_Guiche)
                            || this.enuSituacao.Equals(Operacao.Validar_Ticket_Estacionamento)
                            || this.enuSituacao.Equals(Operacao.Valida_CPF_CNPJ)
                            || this.enuSituacao.Equals(Operacao.Verifica_Documento)
                            || this.enuSituacao.Equals(Operacao.Confirma_Fechamento_Venda)
                            || this.enuSituacao.Equals(Operacao.Estorno_Pagamento_Resta)
                            || this.enuSituacao.Equals(Operacao.Pendente_POS)
                            || this.enuSituacao.Equals(Operacao.Cancelar_Venda)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Diferenca)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Fiscal)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Operadora)
                            || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Operadora)
                            || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Fiscal)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Confirma_Sangria)
                            || this.enuSituacao.Equals(Operacao.Confirma_Venda_Limite_Sat)
                            || this.enuSituacao.Equals(Operacao.Confirmar_Produto_Reciclavel_Auto_Servico)
                            || this.enuSituacao.Equals(Operacao.Confirmar_Produto_Reciclavel_Romaneio)
                    )
                {
                    if (!(e.Key.Equals(Key.S) || e.Key.Equals(Key.N)))
                    {
                        intKeyAscii = 0;
                    }
                }
                else if (this.enuSituacao.Equals(Operacao.Falha_Autenticar))
                {
                    if (!e.Key.Equals(Key.Return))
                    {
                        intKeyAscii = 0;
                    }
                }
                else
                {
                    intKeyAscii = (Int16)objUtil.Permitir_Digitacao_Somente_Numeros(Convert.ToInt16(Strings.Asc(Utilitario.GetCharFromWpfKey(e.Key))));
                }
                if (intKeyAscii == 0)
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                string strErro = Root.Tratamento_Erro.Tratar_Erro_Caixa_Novo(ex, this.blnErroImpressoraFiscal);
                this.txtMenu.Content = strErro;
                this.blnErroImpressoraFiscal = false;
            }

        }


        #endregion

        #region "   Metodos         "

        #region "   Operações           "

        private void Tratar_Operacao_Desligar()
        {
            try
            {
                if (!(this.colPressedKeys.Contains(Key.System) && this.colPressedKeys.Contains(Key.NumPad8) && this.colPressedKeys.Contains(Key.Return)))
                {
                    return;
                }
                if (this.blnCupomAberto || this.blnPagamentoLiberado)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtCodigoItemFabricante.Focus();
                    return;
                }
                if (this.blnRomaneioEstorno || this.blnFechamento)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtCodigoItemFabricante.Focus();
                    return;
                }
                if (this.blnCaixaAberto)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_ABERTO;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtCodigoItemFabricante.Focus();
                    return;
                }

                const string strExecParamentro = "shutdown.exe";
                Process.Start(strExecParamentro, "-s -f -t 00");

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Tratar_Operacao_Reiniciar()
        {
            try
            {
                if (!(this.colPressedKeys.Contains(Key.System) && this.colPressedKeys.Contains(Key.NumPad0) && this.colPressedKeys.Contains(Key.Return)))
                {
                    return;
                }
                if (this.blnCupomAberto || this.blnPagamentoLiberado)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtCodigoItemFabricante.Focus();
                    return;
                }
                if (this.blnRomaneioEstorno || this.blnFechamento)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtCodigoItemFabricante.Focus();
                    return;
                }
                if (this.blnCaixaAberto)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_ABERTO;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtCodigoItemFabricante.Focus();
                    return;
                }
                // Reiniciar
                Root.FinalizarSistema();

                Process.Start(EXEC_MERCADOCAR, '"' + Root.Loja_Ativa.Nome + '"' + " " + FORM_PARAMETRO);

            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Tratar_Operacao_Abertura_Caixa()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = "Caixa já está aberto";
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        this.txtCodigoItemFabricante.Focus();
                        return;
                    }

                    if (this.blnPortaImpressoraFiscal)
                    {
                        this.txtMenu.Content = "Entre com a operadora";
                        this.enuSituacao = Operacao.Autenticar_Abertura_Caixa;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                    }
                    else
                    {
                        this.txtMenu.Content = "Reconectando com a impressora fiscal";
                        Utilitario.Processar_Mensagens_Interface_WPF();

                        if (!this.Abrir_Porta_Impressora_Fiscal())
                        {
                            if (this.blnUtilizaControladorSat)
                            {
                                this.txtMenu.Content = "Erro de comunicação da impressora fiscal.";
                                Utilitario.Processar_Mensagens_Interface_WPF();
                            }
                        }
                        else
                        {
                            this.txtMenu.Content = "Entre com a operadora";
                            this.enuSituacao = Operacao.Autenticar_Abertura_Caixa;
                            this.txtMatricula.Visibility = Visibility.Visible;
                            this.txtMatricula.Focus();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Documento()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        this.txtCodigoItemFabricante.Focus();
                        return;
                    }

                    if (this.blnRomaneioEstorno || this.blnRomaneioEspecial || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        this.txtCodigoItemFabricante.Focus();
                        return;
                    }

                    if (this.blnPagamentoLiberado)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        this.txtCodigoItemFabricante.Focus();
                        return;
                    }

                    if (this.Validar_Inclusao_CPF_CNPJ())
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        this.txtCodigoItemFabricante.Focus();
                        return;
                    }

                    if (this.blnUtilizaNFp)
                    {
                        this.txtMenu.Content = "Já informado o CPF. Incluir novamente? (S\\N)";
                        this.enuSituacao = Operacao.Verifica_Documento;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else
                    {
                        this.txtMenu.Content = "Confirma a inclusão do CPF\\CNPJ? (S\\N)";
                        this.enuSituacao = Operacao.Verifica_Documento;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Quantidade_Peca()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.blnRomaneioEstorno)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Comanda()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (!this.blnPagamentoLiberado && !this.blnRomaneioEstorno && !this.blnFechamento)
                    {
                        this.txtQuantidade.Clear();
                        this.txtCodigoItemFabricante.Clear();

                        this.txtMenu.Content = "Informe o número da comanda";
                        this.enuSituacao = Operacao.Comanda;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Romaneio()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (!this.blnPagamentoLiberado && !this.blnRomaneioEstorno && !this.blnFechamento)
                    {
                        this.txtQuantidade.Clear();
                        this.txtCodigoItemFabricante.Clear();

                        this.txtMenu.Content = "Informe o número do romaneio";
                        this.enuSituacao = Operacao.Romaneio;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Tratar_Operacao_Desconto_Peca()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnCupomAberto && !this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        this.txtMenu.Content = "Autenticar desconto(Gerência)";
                        this.enuSituacao = Operacao.Autenticar_Desconto;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                        return;
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Sangria()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (!this.blnPagamentoLiberado && !this.blnRomaneioEstorno && !this.blnFechamento)
                    {
                        this.txtMenu.Content = "Informe o valor da Sangria";
                        this.enuSituacao = Operacao.Sangria_Valor_Operadora;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        return;
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Fechamento()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (!this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        if (this.Validar_Pagamento_Pendente_POS())
                        {
                            this.txtMenu.Content = "Fechamento.Confirma a Sangria? (S\\N)";
                            this.enuSituacao = Operacao.Fechamento_Confirma_Sangria;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);

                        }
                        else
                        {
                            this.txtMenu.Content = "Pagamento pendente de POS";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        }
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Limpar_Campo()
        {
            try
            {
                if (this.txtCodigoItemFabricante.IsFocused)
                {
                    this.txtCodigoItemFabricante.Clear();
                    this.txtQuantidade.Clear();
                }
                else
                {
                    if (this.enuSituacao == Operacao.Informa_CPF_CNPJ)
                    {
                        return;
                    }

                    if (this.txtComando.Text != string.Empty && this.enuSituacao != Operacao.Valida_CPF_CNPJ)
                    {
                        this.txtComando.Clear();
                        this.txtComando.Focus();
                    }
                    else if (this.txtMatricula.Visibility == Visibility.Visible)
                    {
                        this.txtMatricula.Password = string.Empty;
                        this.txtMatricula.Focus();
                    }
                    else
                    {
                        this.txtSenha.Password = string.Empty;
                        this.txtSenha.Focus();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Suspender()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.blnCaixaAberto)
                    {
                        this.Fechar_Formulario();
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Voltar()
        {
            try
            {
                if (this.blnFechamento)
                {
                    this.txtMenu.Content = "Confirma cancelamento do fechamento? (S\\N)";
                    this.enuSituacao = Operacao.Cancelar_Venda;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
                else if (this.blnPagamentoLiberado && this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    this.txtMenu.Content = "Retornar aos itens? (S\\N)";
                    this.enuSituacao = Operacao.Retornar_Itens;
                    this.txtComando.Visibility = Visibility.Visible;
                    this.txtComando.Focus();
                }
                else if (this.blnCaixaAberto)
                {
                    this.txtMenu.Content = string.Empty;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
                else
                {
                    this.txtMenu.Content = "Caixa Fechado.";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
                this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Confirmacao()
        {
            try
            {
                // Operações permitidas sem que o caixa esteja aberto
                if (this.enuSituacao.Equals(Operacao.Leitura_X)
                    || this.enuSituacao.Equals(Operacao.Confirma_Guiche)
                    || this.enuSituacao.Equals(Operacao.Validar_Ticket_Estacionamento)
                    || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Operadora)
                    || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Fiscal)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Diferenca)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Fiscal)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Operadora)
                    || this.enuSituacao.Equals(Operacao.Cancelar_Venda)
                    || this.enuSituacao.Equals(Operacao.Solicita_Documento)
                    || this.enuSituacao.Equals(Operacao.Confirmar_Produto_Reciclavel_Auto_Servico)
                    || this.enuSituacao.Equals(Operacao.Confirmar_Produto_Reciclavel_Romaneio))
                {
                    this.txtComando.Text = "S";
                    this.Perder_Foco_Campo_Comando(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                    this.txtComando.Clear();
                    return;
                }

                if (this.enuSituacao.Equals(Operacao.Falha_Autenticar))
                {
                    return;
                }

                if (!this.blnCaixaAberto)
                {
                    this.txtMenu.Content = "Caixa Fechado.";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    return;
                }

                if (this.enuSituacao.Equals(Operacao.Valida_CPF_CNPJ)
                            || this.enuSituacao.Equals(Operacao.Verifica_Documento)
                            || this.enuSituacao.Equals(Operacao.Confirma_Fechamento_Venda)
                            || this.enuSituacao.Equals(Operacao.Estorno_Pagamento_Resta)
                            || this.enuSituacao.Equals(Operacao.Confirma_Excluir_Pagamento)
                            || this.enuSituacao.Equals(Operacao.Confirma_Desconto_Valor)
                            || this.enuSituacao.Equals(Operacao.Pendente_POS)
                            || this.enuSituacao.Equals(Operacao.Confirma_Codigo_Cancelameto_POS)
                            || this.enuSituacao.Equals(Operacao.Confirma_CPF_CNPJ)
                            || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Operadora)
                            || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Fiscal)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Diferenca)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Fiscal)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Operadora)
                            || this.enuSituacao.Equals(Operacao.Fechamento_Confirma_Sangria)
                            || this.enuSituacao.Equals(Operacao.Confirma_Venda_Limite_Sat)
                            || this.enuSituacao.Equals(Operacao.Retornar_Itens))
                {
                    this.txtComando.Text = "S";

                    this.Perder_Foco_Campo_Comando(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));

                    if (!this.enuSituacao.Equals(Operacao.Valida_CPF_CNPJ) && !this.enuSituacao.Equals(Operacao.Confirma_CPF_CNPJ))
                        this.txtComando.Clear();
                }
            }
            catch (Exception)
            {
                this.blnFechamentoBloquearCaixa = false;
                throw;
            }
        }

        private void Tratar_Operacao_Negacao()
        {
            try
            {
                // Operações permitidas sem que o caixa esteja aberto
                if (this.enuSituacao.Equals(Operacao.Leitura_X)
                    || this.enuSituacao.Equals(Operacao.Confirma_Guiche)
                    || this.enuSituacao.Equals(Operacao.Validar_Ticket_Estacionamento)
                    || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Operadora)
                    || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Fiscal)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Diferenca)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Fiscal)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Operadora)
                    || this.enuSituacao.Equals(Operacao.Cancelar_Venda)
                    || this.enuSituacao.Equals(Operacao.Solicita_Documento)
                    || this.enuSituacao.Equals(Operacao.Confirmar_Produto_Reciclavel_Auto_Servico)
                    || this.enuSituacao.Equals(Operacao.Confirmar_Produto_Reciclavel_Romaneio))
                {
                    this.txtComando.Text = "N";
                    this.Perder_Foco_Campo_Comando(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                    this.txtComando.Clear();
                    return;
                }

                if (this.enuSituacao.Equals(Operacao.Falha_Autenticar))
                {
                    return;
                }

                if (!this.blnCaixaAberto)
                {
                    this.txtMenu.Content = "Caixa Fechado.";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    return;
                }

                if (this.enuSituacao.Equals(Operacao.Valida_CPF_CNPJ)
                    || this.enuSituacao.Equals(Operacao.Verifica_Documento)
                    || this.enuSituacao.Equals(Operacao.Confirma_Fechamento_Venda)
                    || this.enuSituacao.Equals(Operacao.Estorno_Pagamento_Resta)
                    || this.enuSituacao.Equals(Operacao.Confirma_Excluir_Pagamento)
                    || this.enuSituacao.Equals(Operacao.Confirma_Desconto_Valor)
                    || this.enuSituacao.Equals(Operacao.Pendente_POS)
                    || this.enuSituacao.Equals(Operacao.Confirma_Codigo_Cancelameto_POS)
                    || this.enuSituacao.Equals(Operacao.Confirma_CPF_CNPJ)
                    || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Operadora)
                    || this.enuSituacao.Equals(Operacao.Sangria_Confirma_Valor_Fiscal)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Diferenca)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Fiscal)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Pagamentos_Confirma_Operadora)
                    || this.enuSituacao.Equals(Operacao.Fechamento_Confirma_Sangria)
                    || this.enuSituacao.Equals(Operacao.Confirma_Venda_Limite_Sat)
                    || this.enuSituacao.Equals(Operacao.Retornar_Itens))
                {
                    this.txtComando.Text = "N";
                    this.Perder_Foco_Campo_Comando(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                    this.txtComando.Clear();
                }
            }
            catch (Exception)
            {
                this.blnFechamentoBloquearCaixa = false;
                throw;
            }
        }

        private void Tratar_Operacao_Liberar_Venda()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0 && !this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        if (this.Validar_Saldo_Pagamento())
                        {
                            this.txtMenu.Content = "Confirma fechamento da venda? (S\\N)";
                            this.blnPagamentoLiberado = false;
                            this.enuSituacao = Operacao.Confirma_Fechamento_Venda;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.txtMenu.Content = "Sem saldo a pagar";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        }
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Pagamento_Credito()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    this.dttOperadoraCartaoRegra = new Operadora_Cartao_RegrasBUS().Consultar_DataTable(TipoServidor.LojaAtual);

                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0 && this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        if (this.Validar_Saldo_Pagamento())
                        {
                            this.txtMenu.Content = "Informe nº parcelas";
                            this.enuSituacao = Operacao.Numero_Parcela;
                            this.intFormaPagamentoID = Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO;
                            this.txtComando.MaxLength = 10;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.txtMenu.Content = "Sem saldo a pagar";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        }

                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Pagamento_Dinheiro()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0 && this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        if (this.Validar_Saldo_Pagamento())
                        {
                            this.txtMenu.Content = "Dinheiro.Informe o valor";
                            this.enuSituacao = Operacao.Valor_Parcela;
                            this.intFormaPagamentoID = Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO;
                            this.txtComando.MaxLength = 10;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.txtMenu.Content = "Sem saldo a pagar";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        }
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Pagamento_Debito()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0 && this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        if (this.Validar_Saldo_Pagamento())
                        {
                            this.txtMenu.Content = "Débito.Informe o valor";
                            this.txtComando.Text = this.txtAPagar.Text;
                            this.enuSituacao = Operacao.Valor_Parcela;
                            this.intFormaPagamentoID = Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO;
                            this.txtComando.MaxLength = 10;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.txtMenu.Content = "Sem saldo a pagar";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        }
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Excluir_Pagamento()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnRomaneioEstorno)
                    {
                        this.txtMenu.Content = "Informe o item(Estorno pgto.)";
                        this.enuSituacao = Operacao.Estorno_Pagamento;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0 && this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {
                        if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count > 0)
                        {
                            this.txtMenu.Content = "Informe o item(Excluir)";
                            this.enuSituacao = Operacao.Excluir_Pagamento;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.txtMenu.Content = "Não existe pagamento";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        }
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Finalizar_Venda()
        {

            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0 && this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                    {

                        if (this.dcmTotalVenda > 0 && (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count == 0 || this.dcmTotalVenda > this.Calcular_Valor_Total_Pagamento()))
                        {
                            this.txtMenu.Content = "Entre com o pagamento";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                            return;
                        }

                        if (this.Verifica_Cupom_Fiscal_Aberto() && this.Venda_Exclusiva_De_Servicos())
                        {
                            this.Cancelar_Cupom();
                            this.dttCupomFiscal.Clear();
                            this.dttCupomFiscalFechamento.Clear();
                            this.blnCupomAberto = false;
                        }

                        this.txtMenu.Content = "Finalizando venda";
                        // Venda Consumo ou apenas serviço (Cupom não aberto)
                        if (this.blnVendaConsumo || this.blnCupomAberto == false)
                        {
                            this.strCaixa = string.Empty;
                            this.intTipoDocumento = 0;
                        }
                        else
                        {
                            this.intTipoDocumento = (Int32)TipoDocumento.Cupom;
                            this.strCaixa = "C";
                        }

                        this.Validar_Se_Houve_Concorrencia_Liberacao_Romaneio();

                        this.Liberar_Venda_Confirmado();

                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Venda_Especial()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnPagamentoLiberado || this.blnRomaneioEstorno || this.blnUtilizaNFp || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    this.intTipoDocumento = 0;
                    this.strCaixa = string.Empty;
                    this.blnRomaneioEspecial = true;
                    this.txtMenu.Content = "Venda especial habilitada";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Cancelar_Item_Cupom()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if ((this.blnCupomAberto && !this.blnPagamentoLiberado && !this.blnRomaneioEstorno)
                          || this.Verifica_Existencia_Somente_De_Servicos())
                    {
                        this.txtMenu.Content = "Autenticar cancelar item (Fiscal)";
                        this.enuSituacao = Operacao.Autenticar_Cancelar_Cupom_Item;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                    }
                    else
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private bool Verifica_Existencia_Somente_De_Servicos()
        {
            DataRow[] dtrExisteServicos = this.dtsGridVenda.Tables["Venda_It"].Select(string.Concat("Tipo_Objeto = ",
                                                                                                    CODIGO_TIPO_OBJETO_PARA_SERVICO,
                                                                                                    " And Cancelado = False"));
            return dtrExisteServicos.Length > 0 ? true : false;
        }

        private bool Venda_Exclusiva_De_Servicos()
        {
            DataRow[] dtrExisteServicos = this.dtsGridVenda.Tables["Venda_It"].Select(string.Concat("Tipo_Objeto = ",
                                                                                                    CODIGO_TIPO_OBJETO_PARA_SERVICO,
                                                                                                    " And Cancelado = False"));

            DataRow[] dtrExisteOutros = this.dtsGridVenda.Tables["Venda_It"].Select(string.Concat("not (Tipo_Objeto = ",
                                                                                                    CODIGO_TIPO_OBJETO_PARA_SERVICO,
                                                                                                    " And Cancelado = False) And Cancelado = False"));
            return dtrExisteServicos.Length > 0 && dtrExisteOutros.Length == 0 ? true : false;
        }

        private void Tratar_Operacao_Cancelar_Venda()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = "Autenticar cancelar cupom (Fiscal)";
                        this.enuSituacao = Operacao.Autenticar_Cancelar_Cupom;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                    }
                    else
                    {
                        if (this.blnRomaneioEstorno)
                        {
                            this.txtMenu.Content = "Confirma cancelamento do estorno? (S\\N)";
                        }
                        else
                        {
                            this.txtMenu.Content = "Confirma cancelamento da venda? (S\\N)";
                        }
                        this.enuSituacao = Operacao.Cancelar_Venda;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Reducao_Z()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_TIPO_SAT;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_ABERTO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnPagamentoLiberado || this.blnRomaneioEstorno || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }


                    this.txtMenu.Content = "Autenticar Redução Z";
                    this.enuSituacao = Operacao.Autenticar_Reducao_Z;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Leitura_X()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_TIPO_SAT;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnPagamentoLiberado || this.blnRomaneioEstorno || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    this.txtMenu.Content = "Confirma impressão da Leitura X? (S\\N)";
                    this.enuSituacao = Operacao.Leitura_X;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Operacao_Ticket_Estacionamento()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnPagamentoLiberado || this.blnRomaneioEstorno || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    this.txtMenu.Content = this.Validar_Loja_Estacionamento();

                    if (this.txtMenu.Content.Equals(string.Empty))
                    {
                        this.txtMenu.Content = "Validar o ticket de estacionamento? (S\\N)";
                        this.enuSituacao = Operacao.Validar_Ticket_Estacionamento;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Solicitar_Documento()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (this.blnCupomAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }
                    if (this.blnPagamentoLiberado || this.blnRomaneioEstorno || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    this.txtMenu.Content = "Anotou RG e telefone do cliente? (S\\N)";
                    this.enuSituacao = Operacao.Solicita_Documento;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Operacao_Painel_Guiche()
        {
            try
            {
                if (this.enuSituacao == Operacao.Operacao_Inicial)
                {
                    if (!this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CAIXA_FECHADO;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    if (this.blnRomaneioEstorno || this.blnFechamento)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        return;
                    }

                    this.txtMenu.Content = this.Proximo_Cliente_Painel_Guiche();

                    if (!this.blnCupomAberto && !this.blnPagamentoLiberado)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                    }

                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Campo Comando       "

        private void Tratar_Campo_Comando_Autenticar()
        {
            try
            {
                if (this.Validar_Texto_Campo_Numerico(this.txtMatricula.Password))
                {
                    if (!this.Autenticar_Usuario())
                    {
                        this.enuSituacao = Operacao.Falha_Autenticar;
                        this.txtMatricula.Password = string.Empty;
                        this.txtMatricula.Visibility = Visibility.Visible;
                        this.txtMatricula.Focus();
                    }
                }
                else
                {
                    this.strMensagemAutenticar = this.txtMenu.Content.ToString();
                    this.enuSituacaoSub = this.enuSituacao;
                    this.txtMenu.Content = "Usuário inválido";
                    this.enuSituacao = Operacao.Falha_Autenticar;
                    this.txtMatricula.Password = string.Empty;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Sangria_Operadora()
        {
            try
            {
                if (this.Validar_Valor_Sangria(this.txtComando.Text))
                {
                    this.dcmSangriaValorOperadora = Convert.ToDecimal(this.txtComando.Text);
                    this.enuSituacao = Operacao.Sangria_Confirma_Valor_Operadora;
                    this.txtMenu.Content = "Sangria no valor de R$ " + this.dcmSangriaValorOperadora.ToString("#,##0.00") + ". Confirma? (S\\N)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
                else
                {
                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirma_Sangria_Operadora()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    this.enuSituacao = Operacao.Autenticar_Sangria;
                    this.txtMenu.Content = "Autenticar sangria (Fiscal)";
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
                else
                {
                    this.blnAbrirCaixaSangria = false;
                    this.blnFechamentoSangria = false;
                    this.dcmSangriaValorOperadora = 0;
                    this.txtMenu.Content = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Sangria_Fiscal()
        {
            try
            {
                if (this.Validar_Valor_Sangria(this.txtComando.Text))
                {
                    this.dcmSangriaValorFiscal = Convert.ToDecimal(this.txtComando.Text);
                    this.enuSituacao = Operacao.Sangria_Confirma_Valor_Fiscal;
                    this.txtMenu.Content = "Sangria no valor de R$ " + this.dcmSangriaValorFiscal.ToString("#,##0.00") + ". Confirma? (S\\N)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
                else
                {
                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirma_Sangria_Fiscal()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    if (this.Processar_Sangria())
                    {
                        if (this.blnFechamentoSangria)
                        {
                            this.blnFechamentoSangria = false;
                            this.blnAbrirCaixaSangria = false;
                            this.txtMenu.Content = "Fechamento.Informe a operadora";
                            this.enuSituacao = Operacao.Autenticar_Fechamento_Operadora;
                            this.txtMatricula.Visibility = Visibility.Visible;
                            this.txtMatricula.Focus();
                        }
                        else if (this.blnAbrirCaixaSangria)
                        {
                            this.Inicializar_Fechamento();
                            this.Solicitar_Valores_Fechamento("Fechamento obrigatório.");
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.txtMenu.Content = "Sangria realizada com sucesso.";
                            this.txtComando.Text = string.Empty;
                            this.txtCodigoItemFabricante.Focus();
                            this.enuSituacao = Operacao.Operacao_Inicial;
                        }
                    }
                    else
                    {
                        this.blnAbrirCaixaSangria = false;
                        this.blnFechamentoSangria = false;
                        this.txtComando.Text = string.Empty;
                        this.txtCodigoItemFabricante.Focus();
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
                else
                {
                    this.blnAbrirCaixaSangria = false;
                    this.blnFechamentoSangria = false;
                    this.dcmSangriaValorFiscal = 0;
                    this.txtMenu.Content = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Saldo_Inicial()
        {
            try
            {
                if (this.Validar_Preenche_Saldo_Inicial())
                {
                    if (this.blnProximoCliente)
                    {
                        if (this.intNumeroGuiche == 0)
                        {
                            this.enuSituacao = Operacao.Numero_Guiche;
                            this.txtMenu.Content = "Informe o número do guichê";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.enuSituacao = Operacao.Confirma_Guiche;
                            this.txtMenu.Content = "Guichê " + Convert.ToString(this.intNumeroGuiche) + ". Confirma? (S\\N)";
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                    }
                    else
                    {
                        this.blnCaixaAberto = true;
                        this.txtCodigoItemFabricante.Focus();

                        this.txtMenu.Content = "Verificando pendências do Sitef.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Verificar_Transacaoes_Pendentes_Sitef();

                        // Verifica se já existe o caixa aberto em outra maquina
                        if (!this.Validar_Caixa_Aberto())
                        {
                            this.enuSituacao = Operacao.Operacao_Inicial;
                            this.blnCaixaAberto = false;
                            return;
                        }

                        this.txtMenu.Content = "Preenchendo identificação do Caixa.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Preencher_Identificacao_Caixa_Sat();

                        this.txtMenu.Content = "Registrando abertura do Caixa.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Gravar_Abertura_Caixa();

                        this.txtUsuario.Text = this.dtoUsuario.Nome_Completo;
                        this.imgStatusUsuario.Source = new BitmapImage(new Uri("/MC_Formularios_Wpf;component/Images/MDI/Icone_Usuario.png", UriKind.Relative));

                        this.txtMenu.Content = "Verificando pendência de Sangria.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Verificar_Alerta_Sangria();

                        this.txtMenu.Content = "Inicializando vendas.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Inicializar_Nova_Venda();

                        this.txtMenu.Content = "Atualizar Data Movimento.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Atualizar_Data_Movimento_Impressora_Fiscal();

                        this.txtMenu.Content = string.Empty;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Numero_Guiche()
        {
            try
            {
                if (this.txtComando.Text != string.Empty)
                {
                    if (this.txtComando.Text.IsNumber())
                    {
                        this.intNumeroGuiche = Convert.ToInt32(this.txtComando.Text);
                        if (this.intNumeroGuiche > 0)
                        {
                            RegistroWindows.Inserir_Dados_no_Registro("Guiche_Caixa", this.intNumeroGuiche.ToString());
                        }
                        else
                        {
                            this.txtMenu.Content = "Número do guichê inválido.";
                            this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                            this.txtComando.Text = string.Empty;

                            return;
                        }

                    }
                    this.blnCaixaAberto = true;
                    this.txtComando.Text = string.Empty;
                    this.txtComando.MaxLength = 14;

                    this.txtMenu.Content = "Verificando pendências do Sitef.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Verificar_Transacaoes_Pendentes_Sitef();

                    // Verifica se já existe o caixa aberto em outra maquina
                    if (!this.Validar_Caixa_Aberto())
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        this.blnCaixaAberto = false;
                        return;
                    }

                    this.txtMenu.Content = "Preenchendo identificação do Caixa.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Preencher_Identificacao_Caixa_Sat();

                    this.txtMenu.Content = "Registrando abertura do Caixa.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Gravar_Abertura_Caixa();

                    this.txtUsuario.Text = this.dtoUsuario.Nome_Completo;
                    this.imgStatusUsuario.Source = new BitmapImage(new Uri("/MC_Formularios_Wpf;component/Images/MDI/Icone_Usuario.png", UriKind.Relative));

                    this.txtMenu.Content = "Verificando pendência de Sangria.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Verificar_Alerta_Sangria();

                    this.txtMenu.Content = "Inicializando vendas.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Inicializar_Nova_Venda();

                    this.txtMenu.Content = "Atualizar Data Movimento.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Atualizar_Data_Movimento_Impressora_Fiscal();

                    this.txtCodigoItemFabricante.Focus();

                    this.txtMenu.Content = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirma_Guiche()
        {
            try
            {
                if (this.txtComando.Text == "N")
                {
                    this.enuSituacao = Operacao.Numero_Guiche;
                    this.txtMenu.Content = "Informe o número do guichê";
                    this.txtComando.MaxLength = 3;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
                else
                {

                    this.txtMenu.Content = "Verificando pendências do Sitef.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Verificar_Transacaoes_Pendentes_Sitef();

                    // Verifica se já existe o caixa aberto em outra maquina
                    if (!this.Validar_Caixa_Aberto())
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        this.blnCaixaAberto = false;
                        return;
                    }

                    this.txtMenu.Content = "Preenchendo identificação do Caixa.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Preencher_Identificacao_Caixa_Sat();

                    this.txtMenu.Content = "Registrando abertura do Caixa.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Gravar_Abertura_Caixa();
                    this.txtCodigoItemFabricante.Focus();
                    this.txtComando.Text = string.Empty;
                    this.blnCaixaAberto = true;

                    this.txtUsuario.Text = this.dtoUsuario.Nome_Completo;
                    this.imgStatusUsuario.Source = new BitmapImage(new Uri("/MC_Formularios_Wpf;component/Images/MDI/Icone_Usuario.png", UriKind.Relative));

                    this.txtMenu.Content = "Verificando pendência de Sangria.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Verificar_Alerta_Sangria();

                    this.txtMenu.Content = "Inicializando vendas.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Inicializar_Nova_Venda();

                    this.txtMenu.Content = "Atualizar Data Movimento.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Atualizar_Data_Movimento_Impressora_Fiscal();

                    this.txtMenu.Content = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Validar_Documento()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {

                    // Sugerir documento no PinPad
                    this.txtMenu.Content = "Confirma o documento...";
                    Utilitario.Processar_Mensagens_Interface_WPF();

                    SitefDO dtoSitef = new SitefDO(ref this.objImpressaoFiscal, this.objComunicacaoImpressoraFiscal, this.objTipoImpressoraFiscal);

                    dtoSitef.Encerrar_Transacao_Pendente();

                    dtoSitef.Configura_Sitef(false);

                    if (dtoSitef.Verifica_Presenca_PinPad())
                    {
                        this.txtComando.Text = string.Empty;
                        Utilitario.Processar_Mensagens_Interface_WPF();

                        if (this.blnUtilizaNFp)
                        {
                            this.txtMenu.Content = string.Empty;
                            return;
                        }

                        bool blnStatusConfirmarCPFPinPad = false;
                        blnStatusConfirmarCPFPinPad = dtoSitef.Escreve_Mensagem_PinPad(this.strCpfCnpjNotaFiscalPaulista + "\n Confirma?");

                        // Cliente confirmou o documento, emite o cupom fiscal
                        if (blnStatusConfirmarCPFPinPad)
                        {
                            this.Preencher_Detalhes_Cliente_Nota_Fiscal_Paulista(this.strCpfCnpjNotaFiscalPaulista);

                            this.Processar_Venda_Funcionario();

                            if (this.txtCodigoItemFabricante.Text != string.Empty)
                            {
                                this.Perder_Foco_Campo_Codigo_Item_Fabricante(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                            }
                            else
                            {
                                if (this.dtsPreVendaEscolhido.Tables.Count > 0
                                    && this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0)
                                {
                                    if (this.intRomaneioComanda > 0)
                                    {
                                        if (this.blnRomaneioComanda)
                                        {
                                            this.Incluir_Romaneio(this.intRomaneioComanda, false);
                                        }
                                        else
                                        {
                                            this.Incluir_Comanda(this.intRomaneioComanda);
                                        }

                                        if (this.Preenchar_Grid_Itens_Venda("Romaneio"))
                                        {
                                            this.Calcular_Valor_Total_Orcamento();

                                            if (this.blnIsFuncionario)
                                            {
                                                this.Atualizar_Tela_Venda_Funcionario();
                                            }

                                            if (this.Verificar_Produto_Reciclavel_Por_Romaneio())
                                            {
                                                return;
                                            }
                                        }

                                        this.txtCodigoItemFabricante.Text = string.Empty;
                                        this.txtQuantidade.Text = string.Empty;

                                        this.txtCodigoItemFabricante.Focus();

                                    }
                                }
                            }
                            if (this.blnIsFuncionario == false)
                            {
                                this.txtMenu.Content = string.Empty;
                            }
                            this.txtCodigoItemFabricante.Focus();
                        }

                        else
                        {
                            this.txtMenu.Content = "Informe o documento";
                            this.enuSituacao = Operacao.Informa_CPF_CNPJ;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                        }
                    }
                    else
                    {
                        // Confirmar documento na tela
                        this.txtMenu.Content = "Sem Pinpad.Confirma documento? (S\\N)";
                        this.txtComando.Text = this.strCpfCnpjNotaFiscalPaulista;
                        Utilitario.Processar_Mensagens_Interface_WPF();

                        // Cliente confirmou o documento, emite o cupom fiscal
                        this.enuSituacao = Operacao.Confirma_CPF_CNPJ;

                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
                else if (this.txtComando.Text == "N")
                {
                    this.txtMenu.Content = "Informe o documento";
                    this.enuSituacao = Operacao.Informa_CPF_CNPJ;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirmar_Documento()
        {
            try
            {
                if (this.txtComando.Text == "N")
                {
                    this.txtMenu.Content = "Informe o documento";
                    this.enuSituacao = Operacao.Informa_CPF_CNPJ;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else if (this.txtComando.Text == "S")
                {

                    this.Preencher_Detalhes_Cliente_Nota_Fiscal_Paulista(this.strCpfCnpjNotaFiscalPaulista);

                    this.Processar_Venda_Funcionario();

                    if (this.txtCodigoItemFabricante.Text != string.Empty)
                    {
                        this.Perder_Foco_Campo_Codigo_Item_Fabricante(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                    }
                    else
                    {
                        if (this.dtsPreVendaEscolhido.Tables.Count > 0
                            && this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0)
                        {
                            if (this.intRomaneioComanda > 0)
                            {
                                if (this.blnRomaneioComanda)
                                {
                                    this.Incluir_Romaneio(this.intRomaneioComanda, false);
                                }
                                else
                                {
                                    this.Incluir_Comanda(this.intRomaneioComanda);
                                }

                                if (this.Preenchar_Grid_Itens_Venda("Romaneio"))
                                {

                                    this.Calcular_Valor_Total_Orcamento();

                                    if (this.blnIsFuncionario)
                                    {
                                        this.Atualizar_Tela_Venda_Funcionario();
                                    }

                                    if (this.Verificar_Produto_Reciclavel_Por_Romaneio())
                                    {
                                        return;
                                    }
                                }

                                this.txtCodigoItemFabricante.Text = string.Empty;
                                this.txtQuantidade.Text = string.Empty;

                                this.txtCodigoItemFabricante.Focus();

                            }
                        }
                    }
                    if (this.blnIsFuncionario == false)
                    {
                        this.txtMenu.Content = string.Empty;
                    }
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    this.txtCodigoItemFabricante.Focus();
                }
                else
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Informar_Documento()
        {
            try
            {
                this.strCpfCnpjNotaFiscalPaulista = this.txtComando.Text;

                if (this.Validar_CPF_CNPJ(this.strCpfCnpjNotaFiscalPaulista))
                {
                    // Sugerir documento no PinPad
                    this.txtMenu.Content = "Confirma o documento...";
                    Utilitario.Processar_Mensagens_Interface_WPF();

                    SitefDO dtoSitef = new SitefDO(ref this.objImpressaoFiscal, this.objComunicacaoImpressoraFiscal, this.objTipoImpressoraFiscal);

                    dtoSitef.Encerrar_Transacao_Pendente();

                    dtoSitef.Configura_Sitef(false);

                    if (dtoSitef.Verifica_Presenca_PinPad())
                    {
                        this.txtComando.Text = string.Empty;
                        Utilitario.Processar_Mensagens_Interface_WPF();

                        if (this.blnUtilizaNFp)
                        {
                            this.txtMenu.Content = string.Empty;
                            return;
                        }

                        bool blnStatusConfirmarCPFPinPad = false;
                        blnStatusConfirmarCPFPinPad = dtoSitef.Escreve_Mensagem_PinPad(this.strCpfCnpjNotaFiscalPaulista + "\n Confirma?");

                        // Cliente confirmou o documento, emite o cupom fiscal
                        if (blnStatusConfirmarCPFPinPad)
                        {
                            this.Preencher_Detalhes_Cliente_Nota_Fiscal_Paulista(this.strCpfCnpjNotaFiscalPaulista);

                            this.Processar_Venda_Funcionario();

                            if (this.txtCodigoItemFabricante.Text != string.Empty)
                            {
                                this.Perder_Foco_Campo_Codigo_Item_Fabricante(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                            }
                            else
                            {
                                if (this.dtsPreVendaEscolhido.Tables.Count > 0
                                    && this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0)
                                {
                                    if (this.intRomaneioComanda > 0)
                                    {
                                        if (this.blnRomaneioComanda)
                                        {
                                            this.Incluir_Romaneio(this.intRomaneioComanda, false);
                                        }
                                        else
                                        {
                                            this.Incluir_Comanda(this.intRomaneioComanda);
                                        }

                                        if (this.Preenchar_Grid_Itens_Venda("Romaneio"))
                                        {

                                            this.Calcular_Valor_Total_Orcamento();

                                            if (this.blnIsFuncionario)
                                            {
                                                this.Atualizar_Tela_Venda_Funcionario();
                                            }

                                            if (this.Verificar_Produto_Reciclavel_Por_Romaneio())
                                            {
                                                return;
                                            }
                                        }

                                        this.txtCodigoItemFabricante.Text = string.Empty;
                                        this.txtQuantidade.Text = string.Empty;

                                        this.txtCodigoItemFabricante.Focus();
                                    }
                                }
                            }
                            if (this.blnIsFuncionario == false)
                            {
                                this.txtMenu.Content = string.Empty;
                            }
                            this.enuSituacao = Operacao.Operacao_Inicial;
                            this.txtCodigoItemFabricante.Focus();
                        }

                        else
                        {
                            this.txtMenu.Content = "Informe o documento";
                            this.enuSituacao = Operacao.Informa_CPF_CNPJ;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                        }
                    }
                    else
                    {
                        // Confirmar documento na tela
                        this.txtMenu.Content = "Sem Pinpad.Confirma documento? (S\\N)";
                        this.txtComando.Text = this.strCpfCnpjNotaFiscalPaulista;
                        Utilitario.Processar_Mensagens_Interface_WPF();

                        // Cliente confirmou o documento, emite o cupom fiscal
                        this.enuSituacao = Operacao.Confirma_CPF_CNPJ;

                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
                else
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Verificar_Documento()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    this.Setar_Cliente_Consumidor_Final();
                    if (this.dtsPreVendaEscolhido.Tables.Count > 0
                        && this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0
                        && Convert.ToString(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"]).Trim() != string.Empty
                        && Convert.ToString(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"]).Trim() != "000.000.000-00"
                        && Convert.ToString(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"]).Trim() != "00000000000")
                    {
                        this.strCpfCnpjNotaFiscalPaulista = Convert.ToString(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"]);
                        this.txtMenu.Content = "Confirma o documento? (S\\N)";
                        this.txtComando.Text = this.strCpfCnpjNotaFiscalPaulista;
                        this.enuSituacao = Operacao.Valida_CPF_CNPJ;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }

                    else
                    {
                        this.txtMenu.Content = "Informe o documento";
                        this.enuSituacao = Operacao.Informa_CPF_CNPJ;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                    }
                }
                else
                {
                    this.blnNaoUtilizaNFp = true;
                    this.txtMenu.Content = string.Empty;

                    if (this.txtCodigoItemFabricante.Text != string.Empty)
                    {
                        this.Perder_Foco_Campo_Codigo_Item_Fabricante(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter));
                    }
                    else
                    {
                        if (this.intRomaneioComanda > 0)
                        {
                            if (this.blnRomaneioComanda)
                            {
                                this.Incluir_Romaneio(this.intRomaneioComanda, false);
                            }
                            else
                            {
                                this.Incluir_Comanda(this.intRomaneioComanda);
                            }

                            if (this.Preenchar_Grid_Itens_Venda("Romaneio"))
                            {
                                this.Calcular_Valor_Total_Orcamento();

                                if (this.blnIsFuncionario)
                                {
                                    this.Atualizar_Tela_Venda_Funcionario();
                                }

                                if (this.Verificar_Produto_Reciclavel_Por_Romaneio())
                                {
                                    return;
                                }
                            }

                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.txtQuantidade.Text = string.Empty;

                            this.txtCodigoItemFabricante.Focus();
                        }
                    }
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    this.txtCodigoItemFabricante.Focus();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Comanda()
        {
            try
            {
                this.txtMenu.Content = string.Empty;
                if (this.Validar_Texto_Campo_Numerico(this.txtComando.Text))
                {
                    if (this.Validar_Comanda(Convert.ToInt32(this.txtComando.Text)))
                    {
                        this.blnRomaneioComanda = false;
                        this.intRomaneioComanda = Convert.ToInt32(this.txtComando.Text);

                        if (!this.blnUtilizaNFp && !this.blnNaoUtilizaNFp && !this.blnUtilizaNFpRomaneio && !this.blnRomaneioEspecial)
                        {
                            if (this.Verifica_Cupom_Fiscal_Aberto())
                            {
                                this.blnCupomAberto = true;
                                this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                                this.txtComando.Text = string.Empty;
                                return;
                            }

                            this.txtMenu.Content = "Deseja Nota Fiscal Paulista? (S\\N)";
                            this.enuSituacao = Operacao.Verifica_Documento;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.Incluir_Comanda(Convert.ToInt32(this.txtComando.Text));

                            if (this.Preenchar_Grid_Itens_Venda("Romaneio"))
                            {
                                this.Calcular_Valor_Total_Orcamento();

                                if (this.blnIsFuncionario)
                                {
                                    this.Atualizar_Tela_Venda_Funcionario();
                                }

                                if (this.Verificar_Produto_Reciclavel_Por_Romaneio())
                                {
                                    return;
                                }
                            }

                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.txtQuantidade.Text = string.Empty;

                            this.txtCodigoItemFabricante.Focus();
                        }
                    }
                }
                else
                {
                    this.txtMenu.Content = "Comanda inválida";
                    this.txtComando.Text = string.Empty;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Romaneio()
        {
            try
            {
                if (this.Validar_Texto_Campo_Numerico(this.txtComando.Text))
                {
                    if (this.Validar_Romaneio(Convert.ToInt32(this.txtComando.Text)))
                    {
                        this.blnRomaneioComanda = true;
                        this.intRomaneioComanda = Convert.ToInt32(this.txtComando.Text);

                        if (!this.blnUtilizaNFp && !this.blnNaoUtilizaNFp && !this.blnUtilizaNFpRomaneio && !this.blnRomaneioEspecial)
                        {
                            if (this.Verifica_Cupom_Fiscal_Aberto())
                            {
                                this.blnCupomAberto = true;
                                this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                                this.txtComando.Text = string.Empty;
                                return;
                            }

                            this.txtMenu.Content = "Deseja Nota Fiscal Paulista? (S\\N)";
                            this.enuSituacao = Operacao.Verifica_Documento;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            if (this.blnRomaneioEstorno)
                            {

                                if (!Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Permite Estorno Caixa Novo", "Sim"))
                                {
                                    this.Inicializar_Nova_Venda();
                                    this.txtMenu.Content = "Loja não permite realizar estorno no caixa";
                                    return;
                                }

                                if (this.Validar_Romaneios_Pagamento_Pendente())
                                {
                                    this.Buscar_Dados_Romaneio_Estorno();

                                    this.Buscar_Romaneios_Estorno_Staus();

                                    if (this.blnEstornoFinalizado)
                                    {
                                        this.Inicializar_Nova_Venda();
                                        this.txtMenu.Content = "Romaneio de estorno finalizado.";
                                        return;
                                    }

                                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0)
                                    {
                                        this.Inicializar_Nova_Venda();
                                        this.txtMenu.Content = "Romeneio de estorno com venda junta";
                                        return;
                                    }

                                    this.objVendaItemOrc.Visibility = Visibility.Hidden;
                                    this.objPagamentosEstorno.Visibility = Visibility.Visible;

                                    this.grdItens.Visibility = Visibility.Hidden;

                                    this.objPagamentosEstorno.DataContext = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"];

                                    this.txtMenu.Content = "Informe o item(Estorno pgto.)";
                                    this.enuSituacao = Operacao.Estorno_Pagamento;
                                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                                }
                                else
                                {
                                    this.blnRomaneioEstorno = false;
                                    this.Inicializar_Nova_Venda();
                                    this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                                }
                            }
                            else
                            {
                                this.Incluir_Romaneio(Convert.ToInt32(this.txtComando.Text), false);

                                if (this.Preenchar_Grid_Itens_Venda("Romaneio"))
                                {
                                    this.Calcular_Valor_Total_Orcamento();

                                    if (this.blnIsFuncionario)
                                    {
                                        this.Atualizar_Tela_Venda_Funcionario();
                                    }

                                    if (this.Verificar_Produto_Reciclavel_Por_Romaneio())
                                    {
                                        return;
                                    }
                                }
                            }

                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.txtQuantidade.Text = string.Empty;

                            this.txtCodigoItemFabricante.Focus();
                        }

                    }
                }
                else
                {
                    this.txtMenu.Content = "Romaneio inválido";
                    this.txtComando.Text = string.Empty;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirmar_Fechamento_Venda()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    if (this.Validar_Valor_Limite_Venda_SAT() == false)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_SAT_VENDA_VALOR_LIMITE;
                        this.enuSituacao = Operacao.Confirma_Venda_Limite_Sat;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else
                    {
                        this.objVendaItemOrc.Visibility = Visibility.Hidden;
                        this.objPagamentos.Visibility = Visibility.Visible;

                        this.grdPagamento.Visibility = Visibility.Visible;

                        this.grdItens.Visibility = Visibility.Hidden;

                        this.txtDescricaoProduto.Text = string.Empty;
                        this.txtValorProduto.Text = string.Empty;
                        this.txtTroco.Text = "0,00";
                        this.txtAPagar.Text = this.dcmTotalVenda.ToString("#,##0.00");
                        this.txtTotal.Text = this.dcmTotalVenda.ToString("#,##0.00");

                        this.blnPagamentoLiberado = true;
                        if (this.dcmTotalVenda <= 0)
                        {
                            // Atualiza a forma de pagamento padrão Dinheiro
                            this.Inicializar_Forma_Pagamento();

                            this.txtMenu.Content = this.dcmTotalVenda < 0 ? "Aguarde. Gerando o resta" : "Aguarde. Finalizando venda";
                            Utilitario.Processar_Mensagens_Interface_WPF();

                            this.enuSituacao = Operacao.Operacao_Inicial;
                            this.blnComando = true;
                            this.Window_KeyUp(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.F));
                            if (this.enuSituacao != Operacao.Autenticar_Liberar_Credito_Em_Dinheiro)
                            {
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                            }
                        }
                        else
                        {
                            this.txtMenu.Content = "Entre com o pagamento";
                            this.enuSituacao = Operacao.Operacao_Inicial;
                        }
                    }

                }
                else
                {
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtMenu.Content = string.Empty;

                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Numero_Parcela()
        {
            try
            {
                if (this.txtComando.Text.Length <= 2)
                {

                    if (this.Validar_Parcela())
                    {
                        this.txtMenu.Content = "Crédito.Informe o valor";
                        this.txtComando.Text = this.txtAPagar.Text;
                        this.enuSituacao = Operacao.Valor_Parcela;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else
                    {
                        this.txtMenu.Content = "Nº parcela inválida";
                        this.txtComando.Text = string.Empty;
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
                else
                {
                    this.txtMenu.Content = "Nº parcela inválida";
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Valor_Parcela()
        {
            try
            {
                if (this.Validar_Texto_Campo_Numerico(this.txtComando.Text))
                {
                    string strMSgRetorno = string.Empty;
                    string strMsgDocumento = string.Empty;
                    strMSgRetorno = this.Validar_Valor_Parcela();
                    strMsgDocumento = this.Validar_Valor_Solicitar_Documentos();

                    if (strMSgRetorno == string.Empty)
                    {
                        if (!this.txtComando.Text.Contains(","))
                        {
                            this.txtComando.Text = this.txtComando.Text + ",00";
                        }
                        else if (this.txtComando.Text.Substring(this.txtComando.Text.IndexOf(",", 0) + 1).Length == 1)
                        {
                            this.txtComando.Text = this.txtComando.Text + "0";
                        }
                        this.Preencher_DataRow_Condicao_Pgto();
                        this.txtMenu.Content = strMsgDocumento;
                        this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                        this.enuSituacao = Operacao.Operacao_Inicial;

                    }
                    else
                    {
                        this.txtMenu.Content = strMSgRetorno;
                        this.txtComando.Text = string.Empty;
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Cancelar_Item_Pelo_Codigo_Barras()
        {
            try

            {

                this.blnConsultarItemPorCodigoPecaouCodigoServico = (this.txtComando.Text.Length == NUMERO_PADRAO_CARACTERES_SERVICO);

                List<int> colIDs = this.Localizar_ID_Pelo_Codigo_Barras();
                this.intCodigoDoItemSendoCancelado = this.Retornar_ID_Peca_Ou_Servico();

                if (colIDs.Count == 0)
                {
                    this.txtMenu.Content = "Peça não identificada";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, false);
                }
                else if (colIDs.Count == 1)
                {
                    this.txtComando.Text = colIDs[0].DefaultString();
                    this.Tratar_Campo_Comando_Cancelar_Item();
                    this.intCodigoDoItemSendoCancelado = 0;
                }
                else
                {
                    if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT)
                    {
                        if (this.Validar_Impressao_Comprovante_Itens_A_Cancelar(colIDs))
                        {
                            DataRow[] colPecas = this.dtsOrcamentoIt.Tables["Orcamento_It"].Select("Orcamento_It_Sequencial = " + colIDs[0].DefaultString());

                            this.Imprimir_Comprovante_Itens_Cancelamento(this.Carregar_Itens_A_Cancelar(colPecas[0]["Codigo"].DefaultInteger()));
                        }
                    }

                    this.txtMenu.Content = "Informe o item(Cancelar item)";
                    this.enuSituacao = Operacao.Cancelar_Item_Repetido;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Cancelar_Item()
        {
            try
            {
                this.Cancelar_Item_Cupom_Fiscal();

                this.txtCodigoItemFabricante.Focus();

                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Desconto_Item()
        {
            try
            {
                this.txtMenu.Content = this.Validar_Item_Cupom_Fiscal_Desconto(this.txtComando.Text);

                if (Convert.ToString(this.txtMenu.Content).Equals(string.Empty))
                {
                    this.intItemDesconto = Convert.ToInt32(this.txtComando.Text);
                    this.txtMenu.Content = "Informe o novo preço unitário do item";
                    this.enuSituacao = Operacao.Desconto_Valor;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();

                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Desconto_Valor()
        {
            try
            {
                this.txtMenu.Content = this.Confirmar_Desconto_Item_Cupom_Fiscal(this.txtComando.Text);
                this.dcmDescontoItem = Convert.ToDecimal(this.txtComando.Text);
                this.enuSituacao = Operacao.Confirma_Desconto_Valor;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirmar_Desconto()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {

                    this.txtMenu.Content = this.Desconto_Item_Cupom_Fiscal(this.dcmDescontoItem.ToString());

                    if (Convert.ToString(this.txtMenu.Content).Equals(string.Empty))
                    {
                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.txtQuantidade.Text = string.Empty;

                        this.txtCodigoItemFabricante.Focus();
                    }

                    this.txtCodigoItemFabricante.Focus();
                }
                else
                {
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtMenu.Content = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                }
                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Leitura_X()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    string strHorarioVerao = string.Empty;

                    if (this.Leitura_X(ref strHorarioVerao))
                    {
                        this.txtComando.Text = string.Empty;
                        this.txtMenu.Content = "Leitura X impressa. " + strHorarioVerao;

                        this.txtCodigoItemFabricante.Focus();
                    }
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
                else
                {
                    this.txtComando.Text = string.Empty;
                    this.txtMenu.Content = string.Empty;

                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Excluir_Pagamento()
        {
            try
            {
                if (this.Validar_Exclui_Pagamento())
                {
                    this.strItemPagamentoVenda = this.txtComando.Text;
                    this.txtMenu.Content = "Confirma exclusão do item " + this.strItemPagamentoVenda + "? (S\\N)";
                    this.enuSituacao = Operacao.Confirma_Excluir_Pagamento;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirma_Excluir_Pagamento()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    if (this.Excluir_Pagamento())
                    {
                        this.txtMenu.Content = "Pagamento " + this.strItemPagamentoVenda + " excluído";
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
                else
                {
                    this.txtMenu.Content = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirma_Venda_Limite_Sat()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    // Atualiza a forma de pagamento padrão Dinheiro
                    this.Inicializar_Forma_Pagamento();

                    // Preencher o DataSet com os itens de Auto-Serviço
                    this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows.Clear();
                    this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_It"].Rows.Clear();

                    this.Preencher_DataRow_Ct_Pre_Venda();
                    this.Preencher_DataRow_It_Pre_Venda();

                    // Gerar o romaneio de Pré-Venda
                    this.Gerar_Pre_Venda();

                    // Inicializar uma nova venda
                    this.Cancelar_Cupom();

                    this.Inicializar_Nova_Venda();
                }
                else
                {
                    this.txtMenu.Content = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirma_Reciclavel_Auto_Servico()
        {
            try
            {
                DataRow dtrOrcamentoIt = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[this.Ultima_Linha_Orcamento()];
                if (this.txtComando.Text == "S")
                {
                    if (dtrOrcamentoIt["Descricao"].DefaultString().Trim().Length >= 21)
                    {
                        this.txtMenu.Content = "Qtde Recicláveis " + dtrOrcamentoIt["Descricao"].DefaultString().Trim().Left(21) + " ?";
                    }
                    else
                    {
                        this.txtMenu.Content = "Qtde Recicláveis " + dtrOrcamentoIt["Descricao"].DefaultString().Trim() + " ?";
                    }

                    this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Auto_Servico;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {

                    dtrOrcamentoIt["Acordo_Realizado"] = true;

                    this.txtMenu.Content = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirmar_Quantidade_Reciclavel_Auto_Servico()
        {
            try
            {
                DataRow dtrVendaIT = this.dtsGridVenda.Tables["Venda_IT"].Rows[this.dtsGridVenda.Tables["Venda_IT"].Rows.Count - 1];

                if (this.txtComando.Text == string.Empty)
                {
                    if (dtrVendaIT["Descricao"].DefaultString().Trim().Length >= 21)
                    {
                        this.txtMenu.Content = "Qtde Recicláveis " + dtrVendaIT["Descricao"].DefaultString().Trim().Left(21) + " ?";
                    }
                    else
                    {
                        this.txtMenu.Content = "Qtde Recicláveis " + dtrVendaIT["Descricao"].DefaultString().Trim() + " ?";
                    }


                    this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Auto_Servico;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    return;
                }

                if (this.txtComando.Text.Length <= 3 && this.txtComando.Text.IsNumber() && this.txtComando.Text.DefaultInteger() >= 0)
                {
                    if (dtrVendaIT["Qtde"].DefaultInteger() >= this.txtComando.Text.DefaultInteger())
                    {
                        this.blnAcordoProdutoReciclavel = true;
                        dtrVendaIT["Acordo_Realizado"] = true;
                        dtrVendaIT["Qtde_Reciclavel"] = this.txtComando.Text.DefaultInteger();

                        DataRow dtrOrcamentoIt = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[this.Ultima_Linha_Orcamento()];

                        dtrOrcamentoIt["Acordo_Realizado"] = true;
                        dtrOrcamentoIt["Qtde_Reciclavel"] = this.txtComando.Text.DefaultInteger();

                        if (dtrOrcamentoIt["Produto_Reciclavel_Desconto"].DefaultBool())
                        {
                            dtrVendaIT["Total"] = Convert.ToDecimal(Convert.ToInt32(dtrVendaIT["Qtde"]) - Convert.ToInt32(dtrVendaIT["Qtde_Reciclavel"]))
                                                * Convert.ToDecimal(dtrVendaIT["Preco_Unitario"])
                                                * Convert.ToInt32(dtrVendaIT["Peca_Embalagem_Quantidade"]);
                            dtrVendaIT["Preco_Unitario"] = (dtrVendaIT["Total"].DefaultDecimal() / Convert.ToInt32(dtrVendaIT["Qtde"])).ToDecimalRound(2);

                            dtrOrcamentoIt["Preco_Total"] = ((Convert.ToInt32(dtrOrcamentoIt["Orcamento_It_Qtde"]) - Convert.ToInt32(dtrVendaIT["Qtde_Reciclavel"])
                                                    * Convert.ToInt32(dtrOrcamentoIt["Peca_Embalagem_Quantidade"]))
                                                    * Convert.ToDecimal(dtrOrcamentoIt["Orcamento_It_Preco_Pago"])).ToString("#,##0.00");
                            dtrOrcamentoIt["Orcamento_It_Preco_Pago"] = (dtrOrcamentoIt["Preco_Total"].DefaultDecimal() / Convert.ToInt32(dtrOrcamentoIt["Orcamento_It_Qtde"])).ToDecimalRound(2);
                            dtrOrcamentoIt["Orcamento_It_Valor_Desconto"] = ((Convert.ToInt32(dtrVendaIT["Qtde_Reciclavel"])
                                                    * Convert.ToInt32(dtrOrcamentoIt["Peca_Embalagem_Quantidade"]))
                                                    * Convert.ToDecimal(dtrOrcamentoIt["Orcamento_It_Preco_Pago"])).ToString("#,##0.00");

                            this.Calcular_Valor_Total_Orcamento();
                        }
                        this.txtMenu.Content = string.Empty;
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.txtQuantidade.Text = string.Empty;
                        this.txtCodigoItemFabricante.Focus();
                    }
                    else
                    {
                        if (dtrVendaIT["Descricao"].DefaultString().Trim().Length >= 12)
                        {
                            this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrVendaIT["Descricao"].DefaultString().Trim().Left(12) + " ?";
                        }
                        else
                        {
                            this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrVendaIT["Descricao"].DefaultString().Trim() + " ?";
                        }
                        this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Auto_Servico;
                        this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
                else
                {
                    if (dtrVendaIT["Descricao"].DefaultString().Trim().Length >= 12)
                    {
                        this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrVendaIT["Descricao"].DefaultString().Trim().Left(12) + " ?";
                    }
                    else
                    {
                        this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrVendaIT["Descricao"].DefaultString().Trim() + " ?";
                    }

                    this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Auto_Servico;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirmar_Reciclavel_Romneio()
        {
            try
            {
                DataRow[] colRomaneioIT = this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select("Produto_Reciclavel = true AND Acordo_Realizado = false AND (Romaneio_It_Qtde - Romaneio_Pre_Venda_It_Qtde_Reciclavel) > 0");

                if (this.txtComando.Text == "S")
                {
                    if (colRomaneioIT[0]["Descricao"].DefaultString().Trim().Length >= 21)
                    {
                        this.txtMenu.Content = "Qtde Recicláveis " + colRomaneioIT[0]["Descricao"].DefaultString().Trim().Left(21) + "?";
                    }
                    else
                    {
                        this.txtMenu.Content = "Qtde Recicláveis " + colRomaneioIT[0]["Descricao"].DefaultString().Trim() + "?";
                    }

                    this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Romaneio;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {

                    foreach (DataRow dtrRomaneioIT in colRomaneioIT)
                    {
                        dtrRomaneioIT["Acordo_Realizado"] = 1;
                    }

                    this.txtMenu.Content = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Confirmar_Quantidade_Reciclaval_Romaneio()
        {
            try
            {
                if (this.txtComando.Text == string.Empty)
                {
                    return;
                }

                DataRow[] dtrRomaneioIT = this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select("Produto_Reciclavel = true AND Acordo_Realizado = false AND (Romaneio_It_Qtde - Romaneio_Pre_Venda_It_Qtde_Reciclavel) > 0");

                if (this.txtComando.Text.Length <= 3 && this.txtComando.Text.IsNumber() && this.txtComando.Text.DefaultInteger() >= 0)
                {
                    //Alterar qtde do item reciclavel                        
                    if ((dtrRomaneioIT[0]["Romaneio_IT_Qtde"].DefaultInteger() - dtrRomaneioIT[0]["Romaneio_Pre_Venda_It_Qtde_Reciclavel"].DefaultInteger()) >= this.txtComando.Text.DefaultInteger())
                    {
                        this.blnAcordoProdutoReciclavel = true;
                        dtrRomaneioIT[0]["Qtde_Reciclavel"] = this.txtComando.Text.DefaultInteger();
                        dtrRomaneioIT[0]["Acordo_Realizado"] = 1;

                        if (dtrRomaneioIT[0]["Produto_Reciclavel_Desconto"].DefaultBool())
                        {
                            decimal dcmPrecoTotal = (dtrRomaneioIT[0]["Romaneio_It_Preco_Lista"].DefaultDecimal() * ((dtrRomaneioIT[0]["Romaneio_IT_Qtde"].DefaultInteger() - dtrRomaneioIT[0]["Romaneio_Pre_Venda_It_Qtde_Reciclavel"].DefaultInteger()) - dtrRomaneioIT[0]["Qtde_Reciclavel"].DefaultInteger()));
                            dtrRomaneioIT[0]["Romaneio_It_Valor_Desconto"] = (dtrRomaneioIT[0]["Romaneio_It_Preco_Pago"].DefaultDecimal() * (dtrRomaneioIT[0]["Romaneio_IT_Qtde"].DefaultInteger() - dtrRomaneioIT[0]["Romaneio_Pre_Venda_It_Qtde_Reciclavel"].DefaultInteger()));
                            if ((dtrRomaneioIT[0]["Romaneio_IT_Qtde"].DefaultInteger() - dtrRomaneioIT[0]["Romaneio_Pre_Venda_It_Qtde_Reciclavel"].DefaultInteger() - dtrRomaneioIT[0]["Qtde_Reciclavel"].DefaultInteger()) == 0)
                            {
                                dtrRomaneioIT[0]["Romaneio_It_Preco_Pago"] = 0;

                            }
                            else
                            {
                                dtrRomaneioIT[0]["Romaneio_It_Preco_Pago"] = (dcmPrecoTotal / (dtrRomaneioIT[0]["Romaneio_IT_Qtde"].DefaultInteger() - dtrRomaneioIT[0]["Romaneio_Pre_Venda_It_Qtde_Reciclavel"].DefaultInteger() - dtrRomaneioIT[0]["Qtde_Reciclavel"].DefaultInteger())).ToDecimalRound(2);
                            }

                            DataRow[] dtrVendaIT = this.dtsGridVenda.Tables["Venda_IT"].Select("Romaneio_Pre_Venda_IT_ID = " + dtrRomaneioIT[0]["Romaneio_It_ID"]);

                            dtrVendaIT[0]["Total"] = dcmPrecoTotal;
                            dtrVendaIT[0]["Preco_Unitario"] = dtrRomaneioIT[0]["Romaneio_It_Preco_Pago"];

                            DataRow[] dtrRomaneioCT = this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Select("Lojas_ID = " + dtrRomaneioIT[0]["Lojas_ID"] + " AND Romaneio_CT_ID = " + dtrRomaneioIT[0]["Romaneio_CT_ID"]);

                            dtrRomaneioCT[0]["Romaneio_CT_Valor_Total_Pago"] = dtrRomaneioCT[0]["Romaneio_CT_Valor_Total_Pago"].DefaultDecimal() - dtrRomaneioIT[0]["Romaneio_It_Valor_Desconto"].DefaultDecimal();

                            this.Calcular_Valor_Total_Orcamento();
                        }

                        DataRow[] dtrRomaneioItPendentesReciclagem = this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select("Produto_Reciclavel = true AND Acordo_Realizado = false AND (Romaneio_It_Qtde - Romaneio_Pre_Venda_It_Qtde_Reciclavel) > 0");
                        if (dtrRomaneioItPendentesReciclagem.Length > 0)
                        {
                            if (dtrRomaneioItPendentesReciclagem[0]["Descricao"].DefaultString().Trim().Length >= 21)
                            {
                                this.txtMenu.Content = "Qtde Recicláveis " + dtrRomaneioItPendentesReciclagem[0]["Descricao"].DefaultString().Trim().Left(21) + "?";
                            }
                            else
                            {
                                this.txtMenu.Content = "Qtde Recicláveis " + dtrRomaneioItPendentesReciclagem[0]["Descricao"].DefaultString().Trim() + "?";
                            }

                            this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Romaneio;
                            this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {

                            this.txtMenu.Content = string.Empty;
                            this.enuSituacao = Operacao.Operacao_Inicial;
                            this.txtCodigoItemFabricante.Text = string.Empty;
                            this.txtQuantidade.Text = string.Empty;
                            this.txtCodigoItemFabricante.Focus();
                        }

                    }
                    else
                    {
                        if (dtrRomaneioIT[0]["Descricao"].DefaultString().Trim().Length >= 12)
                        {
                            this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrRomaneioIT[0]["Descricao"].DefaultString().Trim().Left(12) + "?";
                        }
                        else
                        {
                            this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrRomaneioIT[0]["Descricao"].DefaultString().Trim() + "?";
                        }

                        this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Romaneio;
                        this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }

                }
                else
                {
                    if (dtrRomaneioIT[0]["Descricao"].DefaultString().Trim().Length >= 12)
                    {
                        this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrRomaneioIT[0]["Descricao"].DefaultString().Trim().Left(12) + "?";
                    }
                    else
                    {
                        this.txtMenu.Content = "Qtde Recicláveis inválida." + dtrRomaneioIT[0]["Descricao"].DefaultString().Trim() + "?";
                    }
                    this.enuSituacao = Operacao.Informar_Quantidade_Produto_Reciclavel_Romaneio;
                    this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Validar_Ticket_Estacionamento()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    this.txtMenu.Content = "Informe o ticket de estacionamento";
                    this.txtComando.Width = 420;
                    this.txtComando.MaxLength = 20;
                    this.enuSituacao = Operacao.Ticket_Estacionamento;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    if (this.blnCaixaAberto)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                        this.txtCodigoItemFabricante.Focus();
                    }
                    else
                    {
                        this.txtComando.Text = string.Empty;
                        this.txtMenu.Content = string.Empty;

                        this.txtCodigoItemFabricante.Focus();
                    }
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Solicitar_Documento()
        {
            try
            {

                if (this.blnCaixaAberto)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                    this.txtCodigoItemFabricante.Focus();
                }
                else
                {
                    this.txtComando.Text = string.Empty;
                    this.txtMenu.Content = string.Empty;

                    this.txtCodigoItemFabricante.Focus();
                }
                this.blnSolicitarDocumento = false;
                this.enuSituacao = Operacao.Operacao_Inicial;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Ticket_Estacionamento()
        {
            try
            {
                this.txtMenu.Content = this.Validar_Codigo_Ticket(this.txtComando.Text);

                this.txtComando.Width = 280;
                this.txtComando.MaxLength = NUMERO_PADRAO_CARACTERES;
                this.txtComando.Text = string.Empty;

                this.txtCodigoItemFabricante.Focus();

                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento()
        {
            try
            {
                DataRow[] dtPgtoEstornoGrid = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.txtComando.Text));

                if (dtPgtoEstornoGrid.Length > 0)
                {
                    if (Convert.ToInt32(dtPgtoEstornoGrid[0]["Enum_Status_Estorno_ID"]) == Status_Estorno.Estorno_Efetuado.ToInteger())
                    {
                        this.txtMenu.Content = "Pagamento já estornado. Selecione novamente";
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                        return;
                    }

                    if (Convert.ToInt32(dtPgtoEstornoGrid[0]["Enum_Status_Estorno_ID"]) == Status_Estorno.Encaminhado_Financeiro.ToInteger())
                    {
                        this.txtMenu.Content = "Encaminhado ao financeiro. Selecione novamente";
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                        return;
                    }

                    if (Convert.ToInt32(dtPgtoEstornoGrid[0]["Enum_Status_Estorno_ID"]) != Status_Estorno.Estorno_Em_Aberto.ToInteger())
                    {
                        this.txtMenu.Content = "Pagamento inválido. Selecione novamente";
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                        return;
                    }

                    if ((Convert.ToBoolean(dtPgtoEstornoGrid[0]["Forma_Pagamento_Emissao_Cartao_Debito"]) == true
                            || Convert.ToBoolean(dtPgtoEstornoGrid[0]["Forma_Pagamento_Emissao_Cartao_Credito"]) == true)
                            && Convert.ToInt32(dtPgtoEstornoGrid[0]["Cartao_TEF_ID"]) == 0)
                    {
                        this.txtMenu.Content = "Bandeira não identificada. Selecione novamente";
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                        return;
                    }


                    this.intItemPagamentoEstorno = Convert.ToInt32(this.txtComando.Text);

                    switch (this.Identifica_Tipo_Estorna_Pagamento())
                    {
                        case Tipo_Estorna_Pagamento.Sitef:
                            // Verifica tipo do estorno (PARCIAL/ TOTAL)
                            if (this.Validar_Estorno_Pagamento_Total() && this.Validar_Prazo_Realizar_Estorno_Cartao_Credito())
                            {
                                this.txtMenu.Content = "Autenticar estorno pgto no cartão (Fiscal)";
                                this.txtMatricula.Clear();
                                this.enuSituacao = Operacao.Autenticar_Estorno_Pagamento;
                                this.txtMatricula.Visibility = Visibility.Visible;
                                this.txtMatricula.Focus();
                            }
                            else
                            {
                                this.txtMenu.Content = "Só é permitido o estorno total";
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                            }
                            break;
                        case Tipo_Estorna_Pagamento.Dinheiro:
                            this.txtMenu.Content = "Autenticar estorno pgto em dinheiro (Fiscal)";
                            this.txtMatricula.Clear();
                            this.enuSituacao = Operacao.Autenticar_Estorno_Pagamento_Credito;
                            this.txtMatricula.Visibility = Visibility.Visible;
                            this.txtMatricula.Focus();

                            break;
                        case Tipo_Estorna_Pagamento.POS:
                            // Verifica tipo do estorno (PARCIAL/ TOTAL)
                            if (this.Validar_Estorno_Pagamento_Total() && this.Validar_Prazo_Realizar_Estorno_Cartao_Credito())
                            {
                                this.txtMenu.Content = "Autenticar estorno pgto no cartão POS (Fiscal)";
                                this.txtMatricula.Clear();
                                this.enuSituacao = Operacao.Autenticar_Estorno_Pagamento_POS;
                                this.txtMatricula.Visibility = Visibility.Visible;
                                this.txtMatricula.Focus();
                            }
                            else
                            {
                                this.txtMenu.Content = "Só é permitido o estorno total.";
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                            }
                            break;
                        case Tipo_Estorna_Pagamento.Outros:
                            this.txtMenu.Content = "Encaminhar ao Caixa Central";

                            this.enuSituacao = Operacao.Operacao_Inicial;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    this.txtMenu.Content = "Selecione pagamento válido";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_Cartao_Parte1()
        {
            try
            {
                if (this.txtComando.Text.Length == 6)
                {
                    DataRow[] dtPgtoEstornoCartaoParte1 = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                    if (dtPgtoEstornoCartaoParte1.Length > 0)
                    {
                        dtPgtoEstornoCartaoParte1[0]["Cartao_Numero_Parte1"] = this.txtComando.Text;

                        this.txtMenu.Content = "Informe o número do cartão (4 últimos)";
                        this.enuSituacao = Operacao.Estorno_Pagamento_Cartao_Parte2;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                    }
                }
                else
                {
                    this.txtMenu.Content = "Inválido.Informe novamente (6 primeiros)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_Cartao_Parte2()
        {
            try
            {
                if (this.txtComando.Text.Length == 4)
                {
                    DataRow[] dtPgtoEstornoCartaoParte2 = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                    if (dtPgtoEstornoCartaoParte2.Length > 0)
                    {
                        dtPgtoEstornoCartaoParte2[0]["Cartao_Numero_Parte2"] = this.txtComando.Text;

                        this.Processar_Estorno(true, false);

                        // Falha no processamento do Sitef. Confirmar a geração do resta.
                        if (Convert.ToInt32(dtPgtoEstornoCartaoParte2[0]["Enum_Status_Estorno_ID"]) == Status_Estorno.Estorno_Em_Aberto.ToInteger())
                        {
                            this.txtMenu.Content = "Falha no sitef. Gerar resta? (S\\N)";
                            this.enuSituacao = Operacao.Estorno_Pagamento_Resta;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                            return;
                        }

                        // Continuar o processo de estorno, caso exista pagamento em aberto.
                        if (this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Enum_Status_Estorno_ID = " + Status_Estorno.Estorno_Em_Aberto.ToInteger()).Length > 0)
                        {
                            this.txtMenu.Content = "Informe o item(Estorno pgto.)";
                            this.enuSituacao = Operacao.Estorno_Pagamento;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.Buscar_Romaneios_Estorno_Staus();

                            // Não existe mais pagamentos, mas ainda existe crédito. Gera o resta
                            if (!this.blnEstornoFinalizado)
                            {
                                this.txtMenu.Content = "Gerando o romaneio de crédito";
                                Utilitario.Processar_Mensagens_Interface_WPF();
                                this.Gerar_Resta_Origem_Credito();
                            }

                            this.Inicializar_Nova_Venda();
                            this.enuSituacao = Operacao.Operacao_Inicial;
                        }
                    }
                }
                else
                {
                    this.txtMenu.Content = "Inválido.Informe novamente (4 últimos)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_Cartao_POS_Parte1()
        {
            try
            {
                if (this.txtComando.Text.Length == 6)
                {
                    DataRow[] dtPgtoEstornoCartaoParte1 = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                    if (dtPgtoEstornoCartaoParte1.Length > 0)
                    {
                        dtPgtoEstornoCartaoParte1[0]["Cartao_Numero_Parte1"] = this.txtComando.Text;

                        this.txtMenu.Content = "Informe o número do cartão (4 últimos)";
                        this.enuSituacao = Operacao.Estorno_Pagamento_Cartao_POS_Parte2;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                    }
                }
                else
                {
                    this.txtMenu.Content = "Inválido.Informe novamente (6 primeiros)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_Cartao_POS_Parte2()
        {
            try
            {
                if (this.txtComando.Text.Length == 4)
                {
                    DataRow[] dtPgtoEstornoCartaoParte2 = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                    if (dtPgtoEstornoCartaoParte2.Length > 0)
                    {
                        dtPgtoEstornoCartaoParte2[0]["Cartao_Numero_Parte2"] = this.txtComando.Text;
                        this.txtMenu.Content = "Informe o código de cancelamento POS";
                        this.enuSituacao = Operacao.Estorno_Pagamento_POS;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                    }
                }
                else
                {
                    this.txtMenu.Content = "Inválido.Informe novamente (4 últimos)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_POS()
        {
            try
            {
                if (this.txtComando.Text != string.Empty)
                {
                    DataRow[] dtrPgtoEstornoPOSCodCancelamento = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                    if (dtrPgtoEstornoPOSCodCancelamento.Length > 0)
                    {
                        this.strCodCancelamentoPOS = this.txtComando.Text;
                        this.txtMenu.Content = "Confirma o código " + this.strCodCancelamentoPOS + "? (S\\N)";
                        this.enuSituacao = Operacao.Confirma_Codigo_Cancelameto_POS;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_Confirma_Codigo_Cancelameto_POS()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    DataRow[] dtrPgtoEstornoPOSCodCancelamento = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                    if (dtrPgtoEstornoPOSCodCancelamento.Length > 0)
                    {
                        this.Processar_Estorno(false, true);

                        // Continua o processo de estorno, caso exista pagamento em aberto.
                        if (this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Enum_Status_Estorno_ID = " + Status_Estorno.Estorno_Em_Aberto.ToInteger()).Length > 0)
                        {
                            this.txtMenu.Content = "Informe o item(Estorno pgto.)";
                            this.enuSituacao = Operacao.Estorno_Pagamento;
                            this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                        }
                        else
                        {
                            this.Buscar_Romaneios_Estorno_Staus();

                            // Não existe mais pagamentos, mas ainda existe crédito. Gera o resta
                            if (!this.blnEstornoFinalizado)
                            {
                                this.txtMenu.Content = "Gerando o romaneio de crédito.";
                                Utilitario.Processar_Mensagens_Interface_WPF();
                                this.Gerar_Resta_Origem_Credito();
                            }

                            this.Inicializar_Nova_Venda();
                            this.enuSituacao = Operacao.Operacao_Inicial;
                        }
                    }
                }
                else
                {
                    this.txtMenu.Content = "Informe o código de cancelamento POS";
                    this.enuSituacao = Operacao.Estorno_Pagamento_POS;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Estorno_Pagamento_Resta()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    this.txtMenu.Content = "Autenticar estorno de pagamento crédito";
                    this.txtMatricula.Clear();
                    this.enuSituacao = Operacao.Autenticar_Estorno_Pagamento_Credito;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
                else
                {
                    this.txtMenu.Content = "Encaminhar ao Caixa Central criação do estorno";

                    this.Inicializar_Nova_Venda();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Pendente_POS()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    this.txtMenu.Content = "Autenticar liberação pagamento POS";
                    this.txtMatricula.Clear();
                    this.enuSituacao = Operacao.Autenticar_Liberar_POS;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
                else
                {
                    this.txtMenu.Content = "Finalizar venda";
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Cancelar_Venda()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    if (this.blnFechamento)
                    {
                        if (this.blnCaixaAberto)
                        {
                            this.Inicializar_Nova_Venda();

                            this.Limpar_Fechamento();

                            this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                        }
                        else
                        {
                            this.Fechar_Porta_Impressora_Fiscal();

                            this.Limpar_Dados();
                            this.Limpar_Tela();
                            this.Limpar_Variaveis();

                            this.Limpar_Fechamento();

                            this.Limpar_Tela_Fechamento();
                        }
                    }
                    else
                    {
                        this.Inicializar_Nova_Venda();

                        this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                    }

                }
                else
                {
                    if (this.blnFechamento)
                    {
                        this.txtMenu.Content = this.strMensagemAutenticar;
                        this.enuSituacao = this.enuSituacaoSub;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);

                    }
                    else if (this.blnRomaneioEstorno)
                    {
                        this.txtMenu.Content = "Informe o item(Estorno pgto.)";
                        this.enuSituacao = Operacao.Estorno_Pagamento;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                    else
                    {
                        this.txtMenu.Content = string.Empty;
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, false, false);
                        this.txtSenha.Visibility = Visibility.Hidden;
                        this.txtCodigoItemFabricante.Focus();
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Confirma_Sangria()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    this.blnFechamentoSangria = true;
                    this.txtMenu.Content = "Fechamento.Valor da Sangria";
                    this.enuSituacao = Operacao.Sangria_Valor_Operadora;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
                else
                {
                    this.txtMenu.Content = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Pagamentos_Valores()
        {
            try
            {
                if (this.Processar_Fechamento())
                {
                    this.Preencher_Valores_Fechamento();

                    this.Excluir_Fechamento_Pagamento();

                    this.Solicitar_Valores_Fechamento(string.Empty);

                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Pagamentos_Diferenca_Valores()
        {
            try
            {
                if (this.Processar_Fechamento())
                {
                    this.Preencher_Valores_Segundo_Fechamento();

                    this.Excluir_Fechamento_Diferenca_Pagamento();

                    this.Solicitar_Valores_Diferenca_Fechamento(true);

                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Pagamentos_Fiscal_Valores()
        {
            try
            {

                if (this.Processar_Fechamento())
                {
                    this.Preencher_Valores_Fiscal_Fechamento();

                    this.Excluir_Fechamento_Diferenca_Pagamento();

                    this.Solicitar_Valores_Diferenca_Fechamento(false);

                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Pagamentos_Confirma_Diferenca()
        {
            try
            {
                if (this.txtComando.Text == "S" || this.blnFechamentoBloquearCaixa)
                {
                    if (this.txtComando.Text == "S")
                    {
                        this.Solicitar_Valores_Diferenca_Fechamento(true);
                    }
                    else
                    {
                        this.txtMenu.Content = "Diferença no fechamento.Pressione 'S' para continuar.";
                    }
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Inicializar_Nova_Venda();
                    this.Limpar_Fechamento();
                    this.txtMenu.Content = "Fechamento cancelado.";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Pagamentos_Confirma_Operadora()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    // Confirma a autentitcação da fiscal de caixa. 
                    this.txtMenu.Content = "Autenticar fechamento(Fiscal)";
                    this.enuSituacao = Operacao.Autenticar_Fechamento_Fiscal;
                    this.txtMatricula.Clear();
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
                else if (this.blnFechamentoBloquearCaixa)
                {
                    this.txtMenu.Content = "Confirma fechamento.Pressione 'S' para Continuar.";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Inicializar_Nova_Venda();
                    this.Limpar_Fechamento();
                    this.txtMenu.Content = "Fechamento cancelado.";
                }
                this.strMensagemAutenticar = this.txtMenu.Content.ToString();
                this.enuSituacaoSub = this.enuSituacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Fechamento_Pagamentos_Confirma_Fiscal()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    // Grava os registros de fechamento
                    this.Processar_Caixa_Fechamento();

                    this.Fechar_Porta_Impressora_Fiscal();

                    this.Limpar_Dados();
                    this.Limpar_Tela();
                    this.Limpar_Variaveis();

                    this.Limpar_Tela_Fechamento();

                }
                else if (this.blnFechamentoBloquearCaixa)
                {
                    this.txtMenu.Content = "Confirma fechamento.Pressione 'S' para Continuar.";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                }
                else
                {
                    this.Inicializar_Nova_Venda();
                    this.Limpar_Fechamento();
                    this.txtMenu.Content = "Fechamento cancelado.";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Retornar_Itens()
        {
            try
            {
                if (this.txtComando.Text == "S")
                {
                    if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count == 0)
                    {
                        this.blnPagamentoLiberado = false;
                        this.objPagamentos.Visibility = Visibility.Hidden;
                        this.grdPagamento.Visibility = Visibility.Hidden;
                        this.objVendaItemOrc.Visibility = Visibility.Visible;
                        this.grdItens.Visibility = Visibility.Visible;
                        this.txtTroco.Text = string.Empty;
                        this.txtAPagar.Text = string.Empty;
                        this.txtTotal.Text = string.Empty;

                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.txtQuantidade.Text = string.Empty;
                        this.txtMenu.Content = string.Empty;

                        this.txtCodigoItemFabricante.Focus();
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                    else
                    {
                        this.txtMenu.Content = "Existem pagtos lançados.Exclua-os primeiro.";
                        this.enuSituacao = Operacao.Operacao_Inicial;
                    }
                }
                else
                {
                    this.txtMenu.Content = "Entre com o pagamento";
                    this.enuSituacao = Operacao.Operacao_Inicial;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Informa_Operadora_Cartao()
        {
            try
            {
                if (this.txtComando.Text.IsNumber())
                {
                    DataRow[] dtrOperadoraCartao = this.dttOperadoraCartao.Select("Operadora_Cartao_ID = " + this.txtComando.Text);

                    if (dtrOperadoraCartao.Length > 0)
                    {
                        DataRow[] dtrPagamentoCartao = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"]
                                                        .Select("(Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito = True OR Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito = True) AND OperadoraCartao = '' AND Romaneio_Pagamento_Venda_Liberada_Dia_Parcela = 0");

                        dtrPagamentoCartao[0]["OperadoraCartao"] = dtrOperadoraCartao[0]["Operadora_Cartao_Nome"].DefaultString();
                    }

                }

                int intItemPagamento = 0;
                if (this.Retorna_Pagamento_Cartao(ref intItemPagamento))
                {
                    this.txtMenu.Content = "Informar a operadora do cartão do pagamento " + intItemPagamento.DefaultString() + " :";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                    this.enuSituacao = Operacao.Informa_Operadora_Cartao;

                    frmCaixa_Listagem_Operadora_Cartao frmCaixaListagemOperadoraCartao = new frmCaixa_Listagem_Operadora_Cartao();
                    frmCaixaListagemOperadoraCartao.ShowDialog();

                    this.txtComando.Text = frmCaixaListagemOperadoraCartao.OperadoraCartaoID.DefaultString();
                }
                else
                {
                    if (!this.Liberar_Venda_Sem_Sitef())
                    {
                        return;
                    }

                    this.Verificar_Alerta_Sangria();

                    if (this.blnControleCancela)
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        // Valida ticket de estacionamento (se houver)
                        this.blnComando = true;
                        this.Window_KeyUp(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.T));
                    }
                    if (this.blnSolicitarDocumento)
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        this.blnComando = true;
                        this.Window_KeyUp(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.W));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(bool blnLimpar, bool blnVisivel, bool blnFoco)
        {
            if (blnLimpar)
            {
                this.txtComando.Text = string.Empty;
            }

            this.txtComando.Visibility = blnVisivel ? Visibility.Visible : Visibility.Hidden;

            if (blnFoco)
            {
                this.txtComando.Focus();
            }
        }

        #endregion

        #region "   Campo Senha         "

        private void Tratar_Campo_Senha_Abertura_Caixa()
        {
            try
            {
                this.dtoUsuario = this.dtoUsuarioAutenticar;
                this.intUsuario = this.dtoUsuario.ID;
                Root.Funcionalidades.Usuario_Ativo = this.dtoUsuario;
                this.Abrir_Caixa();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Suspender()
        {
            try
            {
                this.Suspender();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Cancelar_Cupom_Suspender()
        {
            try
            {
                if (this.Cancelar_Cupom_Fiscal())
                {
                    this.Suspender();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Cancelar_Cupom()
        {
            try
            {
                if (this.Cancelar_Cupom_Fiscal())
                {
                    if (this.blnPendenciaAtualizaDataMovimento)
                    {
                        this.Atualizar_Data_Movimento_Impressora_Fiscal();
                    }
                    this.Inicializar_Nova_Venda();
                    return;
                }
                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Cancelar_Cupom_Item()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem autorização para cancelar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Cancelar_Item.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para cancelar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.txtMenu.Content = "Leia o código de barras do item(Cancelar item)";
                this.enuSituacao = Operacao.Cancelar_Item;
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Liberar_Credito_Em_Dinheiro()
        {
            try
            {
                string strLiberar = this.Liberar_Credito_Em_Dinheiro();

                if (string.IsNullOrEmpty(strLiberar))
                    this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                else
                    this.txtMenu.Content = strLiberar;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Reducao_Z()
        {
            try
            {
                if (this.Reducao_Z())
                {
                    this.txtMenu.Content = "Redução Z impressa";
                }
                this.txtComando.Text = string.Empty;
                this.txtCodigoItemFabricante.Focus();
                this.enuSituacao = Operacao.Operacao_Inicial;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Desconto()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem autorização gerar desconto";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Liberar_Desconto.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para gerar desconto";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.intUsuarioAprovacaoID = this.dtoUsuarioAutenticar.ID;
                this.txtMenu.Content = "Informe o item(Desconto)";
                this.enuSituacao = Operacao.Desconto_Item;
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Estorno_Pagamento()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem permissão para estornar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Estornar_Pagamento.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem permissão para estornar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.txtMenu.Content = "Informe o número do cartão (6 primeiros)";
                this.enuSituacao = Operacao.Estorno_Pagamento_Cartao_Parte1;
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Estorno_Pagamento_POS()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem permissão para estornar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Mercadocar.Enumerados.Acao_Formulario.Estornar_Pagamento.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem permissão para estornar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.txtMenu.Content = "Informe o número do cartão (6 primeiros)";
                this.enuSituacao = Operacao.Estorno_Pagamento_Cartao_POS_Parte1;
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Tratar_Campo_Senha_Estorno_Pagamento_Credito()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem permissão para gerar crédito";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }
                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Gerar_Credito.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem permissão para gerar crédito";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.txtMenu.Content = "Gerado o romaneio de crédito.";
                Utilitario.Processar_Mensagens_Interface_WPF();
                this.Processar_Estorno(false, false);
                this.Processar_Estorno_Credito_Novas_Tabelas(0);

                // Continua o processo de estorno, caso exista pagamento em aberto.
                if (this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Enum_Status_Estorno_ID = " + Status_Estorno.Estorno_Em_Aberto.ToInteger()).Length > 0)
                {
                    this.txtMenu.Content = "Informe o item(Estorno pgto.)";
                    this.enuSituacao = Operacao.Estorno_Pagamento;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }
                else
                {
                    this.Buscar_Romaneios_Estorno_Staus();

                    // Não existe mais pagamentos, mas ainda existe crédito. Gera o resta
                    if (!this.blnEstornoFinalizado)
                    {
                        this.txtMenu.Content = "Gerando o romaneio de crédito.";
                        Utilitario.Processar_Mensagens_Interface_WPF();
                        this.Gerar_Resta_Origem_Credito();
                    }

                    this.Inicializar_Nova_Venda();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Liberar_POS()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem permissão para liberar no POS";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }
                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Liberar_Venda_Pos.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem permissão para liberar no POS";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                int intItemPagamento = 0;
                this.Retorna_Pagamento_Cartao(ref intItemPagamento);

                this.txtMenu.Content = "Informar a operadora do cartão do pagamento " + intItemPagamento.DefaultString() + " :";
                Utilitario.Processar_Mensagens_Interface_WPF();
                this.txtSenha.Clear();
                this.txtSenha.Visibility = Visibility.Hidden;
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(true, true, true);
                this.enuSituacao = Operacao.Informa_Operadora_Cartao;

                frmCaixa_Listagem_Operadora_Cartao frmCaixaListagemOperadoraCartao = new frmCaixa_Listagem_Operadora_Cartao();
                frmCaixaListagemOperadoraCartao.ShowDialog();

                this.txtComando.Text = frmCaixaListagemOperadoraCartao.OperadoraCartaoID.DefaultString();

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Sangria()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem autorização para conferir a sangria";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Caixa_Conferir_Sangria.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para conferir a sangria";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.txtMenu.Content = "Informe o valor da sangria";
                this.enuSituacao = Operacao.Sangria_Valor_Fiscal;
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Fechamento_Operadora()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não identificado.";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (this.dtoUsuarioAutenticar.ID != this.dtoUsuario.ID)
                {
                    this.txtMenu.Content = "Usuário diferente do operador do caixa.";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.Inicializar_Fechamento();

                this.Solicitar_Valores_Fechamento(string.Empty);

                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Tratar_Campo_Senha_Fechamento_Fiscal()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    this.txtMenu.Content = "Usuário não tem autorização para realizar o fechamento.";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Liberar_Fechamento_Diferenca.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para encerrar o fechamento.";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return;
                }

                this.Preencher_Fechamento_Confirmados();

                // Exibe os valores com diferença
                this.objPagamentosFechamento.DataContext = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Confirmados"];

                this.Preencher_Fechamento_Diferenca_Valor();

                this.Solicitar_Valores_Diferenca_Fechamento(false);
                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Sangria             "

        private bool Processar_Sangria()
        {
            try
            {
                if (this.Verifica_Cupom_Fiscal_Aberto())
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_INVALIDA_CUPOM_IMPRESSO;
                    return false;
                }

                string strMensagem = string.Empty;
                
                strMensagem = this.Validar_Sangria();
                if (strMensagem != string.Empty)
                {
                    this.txtMenu.Content = strMensagem;
                    return false;
                }

                DateTime dtmCaixaOperacaoDataHoraOperacao = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                int intCaixaSangriaID = 0;
                // Gravar na base de dados
                this.Confirmar_Sangria(dtmCaixaOperacaoDataHoraOperacao, ref intCaixaSangriaID);

                // Imprimir 1º via
                if (this.Processar_Impressao_Comprovante_Sangria(dtmCaixaOperacaoDataHoraOperacao, intCaixaSangriaID, true))
                {
                    // Imprimir 2º via
                    this.Processar_Impressao_Comprovante_Sangria(dtmCaixaOperacaoDataHoraOperacao, intCaixaSangriaID, false);
                }
                else
                {
                    return false;
                }

                this.txtUsuario.Text = this.txtUsuario.Text.Replace("_", string.Empty);

                this.blnSangriaRealizada = true;

                this.Verificar_Alerta_Sangria();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Validar_Sangria()
        {
            try
            {
                if (this.dcmSangriaValorFiscal != this.dcmSangriaValorOperadora)
                {
                    return "Valor da sangria diferente da operadora";
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Confirmar_Sangria(DateTime dtmCaixaOperacaoDataHoraOperacao, ref int intCaixaSangriaID)
        {
            try
            {
                // Criar DataTable da CaixaOperacao
                DataTable dttCaixaOperacaoSangria = this.Criar_DataSet_Caixa_Operacao_Sangria();

                this.Preencher_DataRow_Caixa_Operacao(dttCaixaOperacaoSangria, dtmCaixaOperacaoDataHoraOperacao);

                // Criar DataTable da CaixaSangria
                DataTable dttCaixaSangria = new Caixa_SangriaBUS().Retornar_Estrutura_Tabela();

                this.Preencher_DataRow_Sangria(dttCaixaSangria);

                new CaixaBUS().Registrar_Sangria_Caixa(dttCaixaOperacaoSangria, dttCaixaSangria);

                // Retorna o ID da Caixa_Sangria para emitir o código de barras do comprovante
                DataRow dtrCaixaSangria = dttCaixaSangria.Rows[0];
                intCaixaSangriaID = dtrCaixaSangria["Caixa_Sangria_ID"].DefaultInteger();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Criar_DataSet_Caixa_Operacao_Sangria()
        {
            try
            {
                DataTable dttCaixaOperacao = new DataTable("Caixa_Operacao");

                dttCaixaOperacao.Columns.Add("Caixa_Operacao_ID", typeof(int));
                dttCaixaOperacao.Columns.Add("Lojas_ID", typeof(int));
                dttCaixaOperacao.Columns.Add("Usuario_Caixa_Operacao_ID", typeof(int));
                dttCaixaOperacao.Columns.Add("Enum_Caixa_Tipo_Operacao_ID", typeof(int));
                dttCaixaOperacao.Columns.Add("Caixa_Operacao_Responsavel_Sangria", typeof(string));
                dttCaixaOperacao.Columns.Add("Caixa_Operacao_Data_Hora_Operacao", typeof(DateTime));
                dttCaixaOperacao.Columns.Add("Caixa_Operacao_Saldo", typeof(decimal));
                dttCaixaOperacao.Columns.Add("Caixa_Operacao_Nome_Computador", typeof(string));

                return dttCaixaOperacao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Sangria(DataTable dttCaixaSangria)
        {
            try
            {
                DataRow dtrSangria = dttCaixaSangria.NewRow();

                dtrSangria["Caixa_Sangria_ID"] = 0;
                dtrSangria["Caixa_Operacao_ID"] = 0;
                dtrSangria["Lojas_ID"] = this.intLojaID;
                dtrSangria["Enum_Status_ID"] = Status_Caixa_Sangria.Aguardando_Conferencia;
                dtrSangria["Usuario_Ultima_Alteracao_ID"] = this.intUsuario;
                dtrSangria["Caixa_Sangria_Valor"] = this.dcmSangriaValorFiscal;
                dtrSangria["Caixa_Sangria_Qtde_Correcoes"] = 0;

                dttCaixaSangria.Rows.Add(dtrSangria);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Caixa_Operacao(DataTable dttCaixaOperacao, DateTime dtmCaixaOperacaoDataHoraOperacao)
        {
            try
            {
                DataRow dtrCaixaOperacao = dttCaixaOperacao.NewRow();


                dtrCaixaOperacao["Caixa_Operacao_ID"] = 0;
                dtrCaixaOperacao["Lojas_ID"] = this.intLojaID;
                dtrCaixaOperacao["Usuario_Caixa_Operacao_ID"] = this.intUsuario;
                dtrCaixaOperacao["Enum_Caixa_Tipo_Operacao_ID"] = OperacaoCaixa.Sangria;
                dtrCaixaOperacao["Caixa_Operacao_Responsavel_Sangria"] = this.dtoUsuarioAutenticar.ID;
                dtrCaixaOperacao["Caixa_Operacao_Data_Hora_Operacao"] = dtmCaixaOperacaoDataHoraOperacao;
                dtrCaixaOperacao["Caixa_Operacao_Saldo"] = this.dcmSangriaValorFiscal;

                dtrCaixaOperacao["Caixa_Operacao_Nome_Computador"] = Dns.GetHostName().ToUpper();

                dttCaixaOperacao.Rows.Add(dtrCaixaOperacao);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Verificar_Alerta_Sangria()
        {
            try
            {
                Decimal dcmValorSangria = 0;
                if (this.blnSangriaRealizada)
                {
                    DataSet dtsSaldoSangria = new CaixaBUS().Consultar_DataSet_Saldo_Sangria(this.intLojaID, this.intUsuario, DateTime.Today);

                    if (dtsSaldoSangria.Tables["Caixa_Operacao_Dia_Saldo_Sangria"].Rows.Count == 0)
                    {
                        return;
                    }

                    this.blnSangriaRealizada = false;
                    this.dcmValorDinheiro = dtsSaldoSangria.Tables["Caixa_Operacao_Dia_Saldo_Sangria"].Rows[0]["Saldo_Sangria"].DefaultDecimal();
                    dcmValorSangria = dtsSaldoSangria.Tables["Caixa_Operacao_Dia_Saldo_Sangria"].Rows[0]["Saldo_Sangria"].DefaultDecimal();
                }
                else
                {
                    dcmValorSangria = this.dcmValorDinheiro;
                }

                Decimal dcmValorLimiteSangria = 0;

                if (this.Verificar_Necessidade_Sangria_Operadora(dcmValorSangria, ref dcmValorLimiteSangria) && !this.txtUsuario.Text.Contains("_"))
                {
                    this.txtUsuario.Text = "_" + this.txtUsuario.Text;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Verificar_Necessidade_Sangria_Operadora(Decimal dcmValorSangria, ref Decimal dcmValorLimiteSangria)
        {
            try
            {
                dcmValorLimiteSangria = this.dcmParametroProcessoValorLimiteSangria;

                if (dcmValorSangria >= this.dcmParametroProcessoValorLimiteSangria)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Fechamento          "

        private bool Processar_Fechamento()
        {
            try
            {

                string strMensagem = string.Empty;

                strMensagem = this.Validar_Fechamento();
                if (strMensagem != string.Empty)
                {
                    this.txtMenu.Content = strMensagem;
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Validar_Fechamento()
        {
            try
            {
                if (this.txtComando.Text.ToDouble() > Constantes.Constantes_Caixa.VALOR_MAXIMO)
                {
                    return "Valor inválido.Digite novamente.";
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inicializar_Fechamento()
        {
            try
            {
                this.objVendaItemOrc.Visibility = Visibility.Hidden;
                this.objPagamentosFechamento.Visibility = Visibility.Visible;

                this.grdItens.Visibility = Visibility.Hidden;

                this.imgLogoMercadocarFechado.Visibility = Visibility.Hidden;
                this.imgLogoMercadocar.Visibility = Visibility.Visible;

                this.blnFechamento = true;

                CaixaBUS busCaixa = new CaixaBUS();
                this.dtsFechamentoSistema = busCaixa.Consultar_DataSet_Fechamento_Caixa_Sistema(this.dtoUsuario.ID, this.intLojaID);

                this.dtsFechamentoPagamentosInformados = busCaixa.Criar_DataSet_Pagamentos();

                this.Preencher_Caixa_Fechamento();

                this.dtsFechamentoPagamentosCadastrados = busCaixa.Criar_DataSet_Pagamentos_Cadastrados();

                this.Preencher_Fechamento_Pagamentos_Cadastrados();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Solicitar_Valores_Fechamento(string strMensagem)
        {
            try
            {
                if (this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows.Count > 0)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows[0];

                    this.enuSituacao = Operacao.Fechamento_Pagamentos_Valores;
                    if (Convert.ToInt32(dtrFormaPagamento["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        this.txtMenu.Content = strMensagem + "Dinheiro. Valor R$";
                    }
                    else if (Convert.ToBoolean(dtrFormaPagamento["Cartao_Sitef"]) == true)
                    {
                        this.enuSituacao = Operacao.Fechamento_Pagamentos_Valores_Sitef;
                        this.txtMenu.Content = Convert.ToString(dtrFormaPagamento["Forma_Pagamento_DS"]) + ". Qtde. Vias";
                    }
                    else
                    {
                        this.txtMenu.Content = Convert.ToString(dtrFormaPagamento["Forma_Pagamento_DS"]) + ". Valor R$";
                    }
                }
                else
                {

                    this.Preencher_Fechamento_Diferenca_Valor();

                    if (this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Count > 0)
                    {
                        this.txtMenu.Content = "Diferença no fechamento.Pressione 'S' para continuar.";
                        this.blnFechamentoBloquearCaixa = true;
                        this.enuSituacao = Operacao.Fechamento_Pagamentos_Confirma_Diferenca;
                    }
                    else
                    {
                        this.Preencher_Fechamento_Confirmados();

                        // Grava os registros de fechamento
                        this.Processar_Caixa_Fechamento();

                        this.Fechar_Porta_Impressora_Fiscal();

                        this.Limpar_Dados();
                        this.Limpar_Tela();
                        this.Limpar_Variaveis();
                        this.Limpar_Tela_Fechamento();

                    }
                }
                this.strMensagemAutenticar = this.txtMenu.Content.ToString();
                this.enuSituacaoSub = this.enuSituacao;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Preencher_Valores_Fechamento()
        {
            try
            {
                DataRow dtrCartao = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows[0];

                DataRow dtrPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].NewRow();

                dtrPagamentoInformado["Forma_Pagamento_DS"] = dtrCartao["Forma_Pagamento_DS"];
                dtrPagamentoInformado["Forma_Pagamento_ID"] = dtrCartao["Forma_Pagamento_ID"];
                dtrPagamentoInformado["Cartao_Sitef"] = dtrCartao["Cartao_Sitef"];

                if (!this.txtComando.Text.Contains(","))
                {
                    this.txtComando.Text = this.txtComando.Text + ",00";
                }
                else if (this.txtComando.Text.Substring(this.txtComando.Text.IndexOf(",", 0) + 1).Length == 1)
                {
                    this.txtComando.Text = this.txtComando.Text + "0";
                }

                dtrPagamentoInformado["Valor"] = this.txtComando.Text;

                this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Rows.Add(dtrPagamentoInformado);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Fechamento_Pagamentos_Cadastrados()
        {
            try
            {
                DataRow dtrFormaPagamentoDinheiro = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].NewRow();

                dtrFormaPagamentoDinheiro["Forma_Pagamento_DS"] = "Dinheiro";
                dtrFormaPagamentoDinheiro["Forma_Pagamento_ID"] = Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO;

                this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows.Add(dtrFormaPagamentoDinheiro);

                DataRow dtrFormaPagamentoDebitoPOS = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].NewRow();

                dtrFormaPagamentoDebitoPOS["Forma_Pagamento_DS"] = "Cartão Débito - POS";
                dtrFormaPagamentoDebitoPOS["Forma_Pagamento_ID"] = Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO;
                dtrFormaPagamentoDebitoPOS["Cartao_Sitef"] = false;

                this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows.Add(dtrFormaPagamentoDebitoPOS);

                DataRow dtrFormaPagamentoCreditoPOS = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].NewRow();

                dtrFormaPagamentoCreditoPOS["Forma_Pagamento_DS"] = "Cartão Crédito - POS";
                dtrFormaPagamentoCreditoPOS["Forma_Pagamento_ID"] = Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO;
                dtrFormaPagamentoCreditoPOS["Cartao_Sitef"] = false;

                this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows.Add(dtrFormaPagamentoCreditoPOS);

                DataRow dtrFormaPagamentoDebitoSitef = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].NewRow();

                dtrFormaPagamentoDebitoSitef["Forma_Pagamento_DS"] = "Cartão Débito - Sitef";
                dtrFormaPagamentoDebitoSitef["Forma_Pagamento_ID"] = Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO;
                dtrFormaPagamentoDebitoSitef["Cartao_Sitef"] = true;

                this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows.Add(dtrFormaPagamentoDebitoSitef);

                DataRow dtrFormaPagamentoCreditoSitef = this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].NewRow();

                dtrFormaPagamentoCreditoSitef["Forma_Pagamento_DS"] = "Cartão Crédito - Sitef";
                dtrFormaPagamentoCreditoSitef["Forma_Pagamento_ID"] = Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO;
                dtrFormaPagamentoCreditoSitef["Cartao_Sitef"] = true;

                this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows.Add(dtrFormaPagamentoCreditoSitef);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Excluir_Fechamento_Pagamento()
        {
            try
            {
                this.dtsFechamentoPagamentosCadastrados.Tables["Fechamento_Pagamentos_Cadastrados"].Rows[0].Delete();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Solicitar_Valores_Diferenca_Fechamento(bool blnConferenciaOperadora)
        {
            try
            {
                if (this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Count > 0)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows[0];

                    this.enuSituacao = blnConferenciaOperadora ? Operacao.Fechamento_Pagamentos_Diferenca_Valores : Operacao.Fechamento_Pagamentos_Fiscal_Valores;
                    if (Convert.ToInt32(dtrFormaPagamento["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        this.txtMenu.Content = "Dinheiro. Valor R$";
                    }
                    else if (Convert.ToBoolean(dtrFormaPagamento["Cartao_Sitef"]) == true)
                    {
                        this.enuSituacao = blnConferenciaOperadora ? Operacao.Fechamento_Pagamentos_Diferenca_Valores_Sitef : Operacao.Fechamento_Pagamentos_Fiscal_Valores_Sitef;
                        this.txtMenu.Content = Convert.ToString(dtrFormaPagamento["Forma_Pagamento_DS"]) + ". Qtde. Vias";
                    }
                    else
                    {
                        this.txtMenu.Content = Convert.ToString(dtrFormaPagamento["Forma_Pagamento_DS"]) + ". Valor R$";
                    }
                }
                else
                {
                    if (blnConferenciaOperadora)
                    {
                        this.Preencher_Fechamento_Diferenca_Valor_Segundo_Pagamento();

                        if (this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Count > 0)
                        {
                            this.enuSituacao = Operacao.Fechamento_Pagamentos_Confirma_Operadora;
                        }
                        else
                        {
                            this.Preencher_Fechamento_Confirmados();
                            this.enuSituacao = Operacao.Fechamento_Pagamentos_Confirma_Fiscal;
                        }
                    }
                    else
                    {
                        this.enuSituacao = Operacao.Fechamento_Pagamentos_Confirma_Fiscal;
                    }
                    this.txtMenu.Content = "Confirma fechamento.Pressione 'S' para Continuar.";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }

                this.strMensagemAutenticar = this.txtMenu.Content.ToString();
                this.enuSituacaoSub = this.enuSituacao;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Preencher_Valores_Segundo_Fechamento()
        {
            try
            {
                DataRow dtrCartao = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows[0];

                DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].NewRow();

                dtrFormaPagamento["Forma_Pagamento_DS"] = dtrCartao["Forma_Pagamento_DS"];
                dtrFormaPagamento["Forma_Pagamento_ID"] = dtrCartao["Forma_Pagamento_ID"];
                dtrFormaPagamento["Cartao_Sitef"] = dtrCartao["Cartao_Sitef"];

                if (!this.txtComando.Text.Contains(","))
                {
                    this.txtComando.Text = this.txtComando.Text + ",00";
                }
                else if (this.txtComando.Text.Substring(this.txtComando.Text.IndexOf(",", 0) + 1).Length == 1)
                {
                    this.txtComando.Text = this.txtComando.Text + "0";
                }

                dtrFormaPagamento["Valor"] = this.txtComando.Text;

                this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Rows.Add(dtrFormaPagamento);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Valores_Fiscal_Fechamento()
        {
            try
            {
                DataRow dtrCartao = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows[0];

                DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].NewRow();

                dtrFormaPagamento["Forma_Pagamento_DS"] = dtrCartao["Forma_Pagamento_DS"];
                dtrFormaPagamento["Forma_Pagamento_ID"] = dtrCartao["Forma_Pagamento_ID"];
                dtrFormaPagamento["Cartao_Sitef"] = dtrCartao["Cartao_Sitef"];

                if (!this.txtComando.Text.Contains(","))
                {
                    this.txtComando.Text = this.txtComando.Text + ",00";
                }
                else if (this.txtComando.Text.Substring(this.txtComando.Text.IndexOf(",", 0) + 1).Length == 1)
                {
                    this.txtComando.Text = this.txtComando.Text + "0";
                }

                dtrFormaPagamento["Valor"] = this.txtComando.Text;

                this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Rows.Add(dtrFormaPagamento);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Excluir_Fechamento_Diferenca_Pagamento()
        {
            try
            {
                this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows[0].Delete();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Preencher_Fechamento_Diferenca_Valor()
        {
            try
            {

                decimal dcmValorPermitido = 0;
                dcmValorPermitido = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "VALOR_DIFERENCA_VALORES_FECHAMENTO_CAIXA").ToDecimal();

                this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Clear();

                DataRow[] dtrPagamentosInformadosDinheiro = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO).ToString());

                DataRow dtrPagamentosSistemaDinheiro = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Dinheiro"].Rows[0];

                if (((dtrPagamentosSistemaDinheiro["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformadosDinheiro[0]["Valor"].DefaultDecimal()
                            && (dtrPagamentosSistemaDinheiro["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformadosDinheiro[0]["Valor"].DefaultDecimal()) == false)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosDinheiro[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosDinheiro[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosDinheiro[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosDebitoPOS = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO).ToString() + " AND Cartao_Sitef = 0");

                DataRow dtrPagamentosSistemaDebitoPOS = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_POS"].Rows[0];

                if (((dtrPagamentosSistemaDebitoPOS["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformadosDebitoPOS[0]["Valor"].DefaultDecimal()
                           && (dtrPagamentosSistemaDebitoPOS["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformadosDebitoPOS[0]["Valor"].DefaultDecimal()) == false)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosDebitoPOS[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosDebitoPOS[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosDebitoPOS[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosCreditoPos = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString() + " AND Cartao_Sitef = 0");

                DataRow dtrPagamentosSistemaCreditoPos = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_POS"].Rows[0];

                if (((dtrPagamentosSistemaCreditoPos["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformadosCreditoPos[0]["Valor"].DefaultDecimal()
                           && (dtrPagamentosSistemaCreditoPos["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformadosCreditoPos[0]["Valor"].DefaultDecimal()) == false)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosCreditoPos[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosCreditoPos[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosCreditoPos[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosDebitoSitef = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO).ToString() + " AND Cartao_Sitef = 1");

                DataRow dtrPagamentosSistemaDebitoSitef = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_Sitef"].Rows[0];

                if (Convert.ToDecimal(dtrPagamentosInformadosDebitoSitef[0]["Valor"]) != Convert.ToDecimal(dtrPagamentosSistemaDebitoSitef["Numero_Vias"]))
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosDebitoSitef[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosDebitoSitef[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosDebitoSitef[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosCreditoSitef = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString() + " AND Cartao_Sitef = 1");

                DataRow dtrPagamentosSistemaCreditoSitef = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_Sitef"].Rows[0];

                if (Convert.ToDecimal(dtrPagamentosInformadosCreditoSitef[0]["Valor"]) != Convert.ToDecimal(dtrPagamentosSistemaCreditoSitef["Numero_Vias"]))
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosCreditoSitef[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosCreditoSitef[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosCreditoSitef[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }


            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Preencher_Fechamento_Diferenca_Valor_Segundo_Pagamento()
        {
            try
            {

                decimal dcmValorPermitido = 0;
                dcmValorPermitido = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "VALOR_DIFERENCA_VALORES_FECHAMENTO_CAIXA").ToDecimal();

                this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Clear();

                DataRow[] dtrPagamentosInformadosDinheiro = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO).ToString());

                DataRow dtrPagamentosSistemaDinheiro = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Dinheiro"].Rows[0];

                if (dtrPagamentosInformadosDinheiro.Length > 0 &&
                    ((dtrPagamentosSistemaDinheiro["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformadosDinheiro[0]["Valor"].DefaultDecimal()
                            && (dtrPagamentosSistemaDinheiro["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformadosDinheiro[0]["Valor"].DefaultDecimal()) == false)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosDinheiro[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosDinheiro[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosDinheiro[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosDebitoPOS = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO).ToString() + " AND Cartao_Sitef = 0");

                DataRow dtrPagamentosSistemaDebitoPOS = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_POS"].Rows[0];

                if (dtrPagamentosInformadosDebitoPOS.Length > 0
                            && ((dtrPagamentosSistemaDebitoPOS["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformadosDebitoPOS[0]["Valor"].DefaultDecimal()
                                && (dtrPagamentosSistemaDebitoPOS["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformadosDebitoPOS[0]["Valor"].DefaultDecimal()) == false)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosDebitoPOS[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosDebitoPOS[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosDebitoPOS[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosCreditoPos = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString() + " AND Cartao_Sitef = 0");

                DataRow dtrPagamentosSistemaCreditoPos = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_POS"].Rows[0];

                if (dtrPagamentosInformadosCreditoPos.Length > 0
                            && ((dtrPagamentosSistemaCreditoPos["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformadosCreditoPos[0]["Valor"].DefaultDecimal()
                                && (dtrPagamentosSistemaCreditoPos["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformadosCreditoPos[0]["Valor"].DefaultDecimal()) == false)
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosCreditoPos[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosCreditoPos[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosCreditoPos[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosDebitoSitef = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO).ToString() + " AND Cartao_Sitef = 1");

                DataRow dtrPagamentosSistemaDebitoSitef = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_Sitef"].Rows[0];

                if (dtrPagamentosInformadosDebitoSitef.Length > 0 && (Convert.ToDecimal(dtrPagamentosInformadosDebitoSitef[0]["Valor"]) != Convert.ToDecimal(dtrPagamentosSistemaDebitoSitef["Numero_Vias"])))
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosDebitoSitef[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosDebitoSitef[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosDebitoSitef[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }

                DataRow[] dtrPagamentosInformadosCreditoSitef = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString() + " AND Cartao_Sitef = 1");

                DataRow dtrPagamentosSistemaCreditoSitef = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_Sitef"].Rows[0];

                if (dtrPagamentosInformadosCreditoSitef.Length > 0 && (Convert.ToDecimal(dtrPagamentosInformadosCreditoSitef[0]["Valor"]) != Convert.ToDecimal(dtrPagamentosSistemaCreditoSitef["Numero_Vias"])))
                {
                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].NewRow();

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformadosCreditoSitef[0]["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformadosCreditoSitef[0]["Cartao_Sitef"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformadosCreditoSitef[0]["Forma_Pagamento_ID"];

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Diferenca"].Rows.Add(dtrFormaPagamento);
                }


            }
            catch (Exception)
            {
                throw;
            }

        }


        private void Preencher_Fechamento_Confirmados()
        {
            try
            {
                DataRow[] dtrPagamentosSistema;
                DataRow[] dtrSegundoPagamentoInformado;

                foreach (DataRow dtrPagamentosInformados in this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Primeiro_Pagamento"].Rows)
                {
                    dtrPagamentosSistema = null;
                    dtrSegundoPagamentoInformado = null;

                    DataRow dtrFormaPagamento = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Confirmados"].NewRow();

                    if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Dinheiro"].Select();

                        dtrSegundoPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select(" Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO).ToString());

                        dtrFormaPagamento["Valor_Sistema"] = dtrPagamentosSistema[0]["Valor_Venda"];
                        dtrFormaPagamento["Valor_Primeiro_Informado"] = dtrPagamentosInformados["Valor"];
                        dtrFormaPagamento["Valor_Segundo_Informado"] = dtrSegundoPagamentoInformado.Length > 0 ? dtrSegundoPagamentoInformado[0]["Valor"] : 0;

                        dtrFormaPagamento["Valor_Sistema_Sitef"] = 0;
                        dtrFormaPagamento["Valor_Primeiro_Informado_Sitef"] = 0;
                        dtrFormaPagamento["Valor_Segundo_Informado_Sitef"] = 0;

                        dtrFormaPagamento["Diferenca_Primeiro"] = Convert.ToDecimal(dtrPagamentosInformados["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal());
                        dtrFormaPagamento["Diferenca_Segundo"] = dtrSegundoPagamentoInformado.Length > 0 ? Convert.ToDecimal(dtrSegundoPagamentoInformado[0]["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal()) : 0;
                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO
                                && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == false)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_POS"].Select();

                        dtrSegundoPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO).ToString() + " AND Cartao_Sitef = 0");


                        dtrFormaPagamento["Valor_Sistema"] = dtrPagamentosSistema[0]["Valor_Venda"];
                        dtrFormaPagamento["Valor_Primeiro_Informado"] = dtrPagamentosInformados["Valor"];
                        dtrFormaPagamento["Valor_Segundo_Informado"] = dtrSegundoPagamentoInformado.Length > 0 ? dtrSegundoPagamentoInformado[0]["Valor"] : 0;

                        dtrFormaPagamento["Valor_Sistema_Sitef"] = 0;
                        dtrFormaPagamento["Valor_Primeiro_Informado_Sitef"] = 0;
                        dtrFormaPagamento["Valor_Segundo_Informado_Sitef"] = 0;

                        dtrFormaPagamento["Diferenca_Primeiro"] = Convert.ToDecimal(dtrPagamentosInformados["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal());
                        dtrFormaPagamento["Diferenca_Segundo"] = dtrSegundoPagamentoInformado.Length > 0 ? Convert.ToDecimal(dtrSegundoPagamentoInformado[0]["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal()) : 0;
                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO
                                && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == false)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_POS"].Select();

                        dtrSegundoPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString() + " AND Cartao_Sitef = 0");

                        dtrFormaPagamento["Valor_Sistema"] = dtrPagamentosSistema[0]["Valor_Venda"];
                        dtrFormaPagamento["Valor_Primeiro_Informado"] = dtrPagamentosInformados["Valor"];
                        dtrFormaPagamento["Valor_Segundo_Informado"] = dtrSegundoPagamentoInformado.Length > 0 ? dtrSegundoPagamentoInformado[0]["Valor"] : 0;

                        dtrFormaPagamento["Valor_Sistema_Sitef"] = 0;
                        dtrFormaPagamento["Valor_Primeiro_Informado_Sitef"] = 0;
                        dtrFormaPagamento["Valor_Segundo_Informado_Sitef"] = 0;

                        dtrFormaPagamento["Diferenca_Primeiro"] = Convert.ToDecimal(dtrPagamentosInformados["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal());
                        dtrFormaPagamento["Diferenca_Segundo"] = dtrSegundoPagamentoInformado.Length > 0 ? Convert.ToDecimal(dtrSegundoPagamentoInformado[0]["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal()) : 0;
                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO
                            && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == true)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_Sitef"].Select();

                        dtrSegundoPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO).ToString() + " AND Cartao_Sitef = 1");

                        dtrFormaPagamento["Valor_Sistema_Sitef"] = dtrPagamentosSistema[0]["Numero_Vias"];
                        dtrFormaPagamento["Valor_Primeiro_Informado_Sitef"] = dtrPagamentosInformados["Valor"];
                        dtrFormaPagamento["Valor_Segundo_Informado_Sitef"] = dtrSegundoPagamentoInformado.Length > 0 ? dtrSegundoPagamentoInformado[0]["Valor"] : 0;

                        dtrFormaPagamento["Valor_Sistema"] = 0;
                        dtrFormaPagamento["Valor_Primeiro_Informado"] = 0;
                        dtrFormaPagamento["Valor_Segundo_Informado"] = 0;

                        dtrFormaPagamento["Diferenca_Primeiro"] = Convert.ToInt32(dtrPagamentosInformados["Valor"].DefaultInteger() - dtrPagamentosSistema[0]["Numero_Vias"].DefaultInteger());
                        dtrFormaPagamento["Diferenca_Segundo"] = dtrSegundoPagamentoInformado.Length > 0 ? Convert.ToDecimal(dtrSegundoPagamentoInformado[0]["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Numero_Vias"].DefaultDecimal()) : 0;
                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO
                                && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == true)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_Sitef"].Select();

                        dtrSegundoPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Segundo_Pagamento"].Select(" Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString() + " AND Cartao_Sitef = 1");

                        dtrFormaPagamento["Valor_Sistema_Sitef"] = dtrPagamentosSistema[0]["Numero_Vias"];
                        dtrFormaPagamento["Valor_Primeiro_Informado_Sitef"] = dtrPagamentosInformados["Valor"];
                        dtrFormaPagamento["Valor_Segundo_Informado_Sitef"] = dtrSegundoPagamentoInformado.Length > 0 ? dtrSegundoPagamentoInformado[0]["Valor"] : 0;

                        dtrFormaPagamento["Valor_Sistema"] = 0;
                        dtrFormaPagamento["Valor_Primeiro_Informado"] = 0;
                        dtrFormaPagamento["Valor_Segundo_Informado"] = 0;

                        dtrFormaPagamento["Diferenca_Primeiro"] = Convert.ToInt32(dtrPagamentosInformados["Valor"].DefaultInteger() - dtrPagamentosSistema[0]["Numero_Vias"].DefaultInteger());
                        dtrFormaPagamento["Diferenca_Segundo"] = dtrSegundoPagamentoInformado.Length > 0 ? Convert.ToDecimal(dtrSegundoPagamentoInformado[0]["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Numero_Vias"].DefaultDecimal()) : 0;
                    }

                    dtrFormaPagamento["Forma_Pagamento_DS"] = dtrPagamentosInformados["Forma_Pagamento_DS"];
                    dtrFormaPagamento["Forma_Pagamento_ID"] = dtrPagamentosInformados["Forma_Pagamento_ID"];
                    dtrFormaPagamento["Cartao_Sitef"] = dtrPagamentosInformados["Cartao_Sitef"];

                    if (Convert.ToDecimal(dtrFormaPagamento["Diferenca_Primeiro"]) != 0
                        || Convert.ToDecimal(dtrFormaPagamento["Diferenca_Segundo"]) != 0)
                    {
                        dtrFormaPagamento["Cor"] = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        dtrFormaPagamento["Cor"] = new SolidColorBrush(Colors.Black);
                    }

                    this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Confirmados"].Rows.Add(dtrFormaPagamento);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Caixa_Fechamento()
        {
            try
            {
                DBUtil objUtil = new DBUtil();
                DataRow dtrCaixaFechamento = this.dtsFechamentoPagamentosInformados.Tables["Caixa_Fechamento"].NewRow();

                dtrCaixaFechamento["Caixa_Fechamento_Data_Inicio"] = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                this.dtsFechamentoPagamentosInformados.Tables["Caixa_Fechamento"].Rows.Add(dtrCaixaFechamento);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Processar_Caixa_Fechamento()
        {
            try
            {
                DataTable dttCaixaFechamento;
                Caixa_FechamentoBUS busCaixaFechamento = new Caixa_FechamentoBUS();
                dttCaixaFechamento = busCaixaFechamento.Retornar_Estrutura_Tabela();

                DataTable dttCaixaFechamentoDetalhes;
                Caixa_Fechamento_DetalhesBUS busCaixaFechamentoDetalhes = new Caixa_Fechamento_DetalhesBUS();
                dttCaixaFechamentoDetalhes = busCaixaFechamentoDetalhes.Retornar_Estrutura_Tabela();
                dttCaixaFechamentoDetalhes.Columns.Add("Forma_Pagamento_DS", typeof(string));

                DataTable dttCaixaFechamentoPagamentos;
                Caixa_Fechamento_Pagamento_ConferidoBUS busCaixaFechamentoPagamento = new Caixa_Fechamento_Pagamento_ConferidoBUS();
                dttCaixaFechamentoPagamentos = busCaixaFechamentoPagamento.Retornar_Estrutura_Tabela();

                DataTable dttCaixaFechamentoOperacao = new DataTable();
                this.Criar_DataSet_Caixa_Operacao_Fechamento(ref dttCaixaFechamentoOperacao);

                decimal dcmValorFinalDinheiro = 0;
                this.Preencher_DataTable_Caixa_Fechamento(dttCaixaFechamento);
                this.Preencher_DataTable_Caixa_Fechamento_Detalhes(dttCaixaFechamentoDetalhes, ref dcmValorFinalDinheiro);
                this.Preencher_DataTable_Caixa_Fechamento_Romaneios_Pagamentos(dttCaixaFechamentoPagamentos);
                this.Preencher_DataTable_Caixa_Operacao(dttCaixaFechamentoOperacao, dcmValorFinalDinheiro);

                CaixaBUS busCaixa = new CaixaBUS();
                busCaixa.Registrar_Fechamento_Caixa(dttCaixaFechamento, dttCaixaFechamentoDetalhes, dttCaixaFechamentoPagamentos, dttCaixaFechamentoOperacao);

                this.Processar_Impressao_Comprovante_Fechamento(dttCaixaFechamento, dttCaixaFechamentoDetalhes);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.Limpar_Fechamento();
            }
        }

        private void Preencher_DataTable_Caixa_Fechamento(DataTable dttCaixaFechamento)
        {
            try
            {
                DataRow dtrCaixaFechamento = dttCaixaFechamento.NewRow();

                dtrCaixaFechamento["Caixa_Fechamento_ID"] = 0;
                dtrCaixaFechamento["Lojas_ID"] = this.intLojaID;
                dtrCaixaFechamento["Usuario_Caixa_ID"] = this.intUsuario;
                dtrCaixaFechamento["Usuario_Autorizacao_ID"] = this.dtoUsuarioAutenticar.ID;
                dtrCaixaFechamento["Usuario_Auditoria_ID"] = 0;
                dtrCaixaFechamento["Caixa_Fechamento_Data_Abertura"] = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Abertura"].Rows[0]["Data_Ultima_Abertura"];
                dtrCaixaFechamento["Caixa_Fechamento_Data_Inicio"] = this.dtsFechamentoPagamentosInformados.Tables["Caixa_Fechamento"].Rows[0]["Caixa_Fechamento_Data_Inicio"];

                DBUtil objUtil = new DBUtil();
                dtrCaixaFechamento["Caixa_Fechamento_Data_Fim"] = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                dtrCaixaFechamento["Caixa_Fechamento_Data_Auditoria"] = new DateTime(1900, 1, 1);

                if (this.Verificar_Diferenca_Sistema_Fiscal())
                {
                    dtrCaixaFechamento["Enum_Status_Fechamento_ID"] = Status_Caixa_Fechamento.Pendente_de_Justificativa;
                }
                else
                {
                    if (this.blnFechamentoBloquearCaixa)
                    {
                        dtrCaixaFechamento["Enum_Status_Fechamento_ID"] = Status_Caixa_Fechamento.Aguardando_Auditoria; ///Se o caixa foi bloqueado, o operador errou os valores na primeira tentativa de fechamento, então precisa de auditoria.
                    }
                    else
                    {
                        dtrCaixaFechamento["Enum_Status_Fechamento_ID"] = Status_Caixa_Fechamento.Auditado; ///Se não há divergência nos valores informados no fechamento do caixa, então já está auditado.
                        dtrCaixaFechamento["Caixa_Fechamento_Data_Auditoria"] = dtrCaixaFechamento["Caixa_Fechamento_Data_Fim"]; ///A data da auditoria é a mesma do fim do fechamento.
                        dtrCaixaFechamento["Usuario_Auditoria_ID"] = Constantes.Usuario_Sistema.USUARIO_SISTEMA_ID;
                    }
                }

                dttCaixaFechamento.Rows.Add(dtrCaixaFechamento);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataTable_Caixa_Fechamento_Detalhes(DataTable dttCaixaFechamentoDetalhes, ref Decimal dcmValorFinalDinheiro)
        {
            try
            {
                DataRow[] dtrFiscalPagamentoInformado;

                foreach (DataRow dtrPagamentosInformados in this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Pagamentos_Confirmados"].Rows)
                {
                    dtrFiscalPagamentoInformado = null;

                    DataRow dtrCaixaFechamento = dttCaixaFechamentoDetalhes.NewRow();

                    dtrCaixaFechamento["Lojas_ID"] = this.intLojaID;
                    dtrCaixaFechamento["Usuario_Justificativa_ID"] = 0;
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Justificativa"] = string.Empty;
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Justificativa_Data"] = new DateTime(1900, 1, 1);
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Observacao_Auditoria"] = string.Empty;

                    dtrCaixaFechamento["Forma_Pagamento_ID"] = dtrPagamentosInformados["Forma_Pagamento_ID"];
                    dtrCaixaFechamento["Forma_Pagamento_DS"] = dtrPagamentosInformados["Forma_Pagamento_DS"];
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Valor_Sistema"] = dtrPagamentosInformados["Valor_Sistema"].DefaultDecimal();
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Valor_Operadora_1"] = dtrPagamentosInformados["Valor_Primeiro_Informado"].DefaultDecimal();
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Valor_Operadora_2"] = dtrPagamentosInformados["Valor_Segundo_Informado"].DefaultDecimal();
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Qtde_Vias_Sistema"] = dtrPagamentosInformados["Valor_Sistema_Sitef"];
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Qtde_Vias_1"] = dtrPagamentosInformados["Valor_Primeiro_Informado_Sitef"];
                    dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Qtde_Vias_2"] = dtrPagamentosInformados["Valor_Segundo_Informado_Sitef"];

                    if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        dtrCaixaFechamento["Enum_TipoTransacao_ID"] = 0;
                        dtrFiscalPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Select(" Forma_Pagamento_ID = " + dtrPagamentosInformados["Forma_Pagamento_ID"].ToString());

                        if (dtrFiscalPagamentoInformado.Length > 0)
                        {
                            dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Valor_Fiscal"] = Convert.ToDecimal(dtrFiscalPagamentoInformado[0]["Valor"]);
                            dcmValorFinalDinheiro = Convert.ToDecimal(dtrFiscalPagamentoInformado[0]["Valor"]);
                        }
                    }
                    else
                    {
                        dtrCaixaFechamento["Enum_TipoTransacao_ID"] = Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) ? TipoTransacaoTEF.SITEF : TipoTransacaoTEF.POS;
                        dtrFiscalPagamentoInformado = this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Select("Forma_Pagamento_ID = " + dtrPagamentosInformados["Forma_Pagamento_ID"].ToString()
                                                                                                                                                    + " AND Cartao_Sitef = " + dtrPagamentosInformados["Cartao_Sitef"].ToString());
                        if (dtrFiscalPagamentoInformado.Length > 0)
                        {
                            dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Qtde_Vias_Fiscal"] = Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) ? Convert.ToInt32(dtrFiscalPagamentoInformado[0]["Valor"]) : 0;
                            dtrCaixaFechamento["Caixa_Fechamento_Detalhes_Valor_Fiscal"] = Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) ? 0 : Convert.ToDecimal(dtrFiscalPagamentoInformado[0]["Valor"]);
                        }
                    }
                    dttCaixaFechamentoDetalhes.Rows.Add(dtrCaixaFechamento);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataTable_Caixa_Fechamento_Romaneios_Pagamentos(DataTable dttCaixaFechamentoPagamentos)
        {
            try
            {
                foreach (DataRow dtrPagamentos in this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Romaneio_Pagamento"].Rows)
                {
                    DataRow dtrCaixaFechamentoRomaneioPagamentos = dttCaixaFechamentoPagamentos.NewRow();

                    dtrCaixaFechamentoRomaneioPagamentos["Lojas_ID"] = this.intLojaID;
                    dtrCaixaFechamentoRomaneioPagamentos["Romaneio_Venda_Pagamento_ID"] = dtrPagamentos["Romaneio_Venda_Pagamento_ID"];

                    dttCaixaFechamentoPagamentos.Rows.Add(dtrCaixaFechamentoRomaneioPagamentos);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Limpar_Fechamento()
        {
            try
            {
                this.blnFechamento = false;


                this.dtsFechamentoSistema.Clear();
                this.dtsFechamentoPagamentosInformados.Clear();
                this.dtsFechamentoPagamentosCadastrados.Clear();

                this.objPagamentosFechamento.DataContext = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Criar_DataSet_Caixa_Operacao_Fechamento(ref DataTable dttCaixaFechamentoOperacao)
        {
            try
            {
                dttCaixaFechamentoOperacao = new DataTable("Caixa_Operacao");

                dttCaixaFechamentoOperacao.Columns.Add("Lojas_ID", typeof(int));
                dttCaixaFechamentoOperacao.Columns.Add("Usuario_Caixa_Operacao_ID", typeof(int));
                dttCaixaFechamentoOperacao.Columns.Add("Caixa_Tipo_Operacao_ID", typeof(bool));
                dttCaixaFechamentoOperacao.Columns.Add("Caixa_Operacao_Saldo", typeof(decimal));
                dttCaixaFechamentoOperacao.Columns.Add("Caixa_Operacao_Nome_Computador", typeof(string));

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataTable_Caixa_Operacao(DataTable dttCaixaFechamentoOperacao, decimal dcmValorFinalDinheiro)
        {
            try
            {
                DataRow dtrCaixaFechamento = dttCaixaFechamentoOperacao.NewRow();

                dtrCaixaFechamento["Lojas_ID"] = this.intLojaID;
                dtrCaixaFechamento["Usuario_Caixa_Operacao_ID"] = this.intUsuario;
                dtrCaixaFechamento["Caixa_Tipo_Operacao_ID"] = false;

                dtrCaixaFechamento["Caixa_Operacao_Saldo"] = dcmValorFinalDinheiro;
                dtrCaixaFechamento["Caixa_Operacao_Nome_Computador"] = Dns.GetHostName().ToUpper();

                dttCaixaFechamentoOperacao.Rows.Add(dtrCaixaFechamento);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Verificar_Diferenca_Sistema_Fiscal()
        {
            try
            {
                decimal dcmValorPermitido = 0;
                dcmValorPermitido = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "VALOR_DIFERENCA_VALORES_FECHAMENTO_CAIXA").ToDecimal();

                DataRow[] dtrPagamentosSistema;
                bool blnVerificaDiferenca = false;

                foreach (DataRow dtrPagamentosInformados in this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Rows)
                {
                    dtrPagamentosSistema = null;

                    if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Dinheiro"].Select();

                        if (((dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformados["Valor"].DefaultDecimal()
                          && (dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformados["Valor"].DefaultDecimal()) == false)
                        {
                            blnVerificaDiferenca = true;
                            break;
                        }

                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO
                                && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == false)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_POS"].Select();

                        if (((dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformados["Valor"].DefaultDecimal()
                          && (dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformados["Valor"].DefaultDecimal()) == false)
                        {
                            blnVerificaDiferenca = true;
                            break;
                        }
                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO
                                && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == false)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_POS"].Select();

                        if (((dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal() + dcmValorPermitido) >= dtrPagamentosInformados["Valor"].DefaultDecimal()
                          && (dtrPagamentosSistema[0]["Valor_Venda"].DefaultDecimal() - dcmValorPermitido) <= dtrPagamentosInformados["Valor"].DefaultDecimal()) == false)
                        {
                            blnVerificaDiferenca = true;
                            break;
                        }

                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DEBITO
                            && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == true)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Debito_Sitef"].Select();

                        if (Convert.ToDecimal(dtrPagamentosInformados["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Numero_Vias"].DefaultDecimal()) != 0)
                        {
                            blnVerificaDiferenca = true;
                            break;
                        }

                    }
                    else if (Convert.ToInt32(dtrPagamentosInformados["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO
                                && Convert.ToBoolean(dtrPagamentosInformados["Cartao_Sitef"]) == true)
                    {
                        dtrPagamentosSistema = this.dtsFechamentoSistema.Tables["Fechamento_Caixa_Credito_Sitef"].Select();

                        if (Convert.ToDecimal(dtrPagamentosInformados["Valor"].DefaultDecimal() - dtrPagamentosSistema[0]["Numero_Vias"].DefaultDecimal()) != 0)
                        {
                            blnVerificaDiferenca = true;
                            break;
                        }
                    }
                }

                return blnVerificaDiferenca;
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

        #region "   Abertura/fechamento caixa  "

        private void Inicializar_Paramentros()
        {

            try
            {
                this.dcmParametroProcessoLimiteVendaSat = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "VALOR_LIMITE_VENDA_SAT").DefaultDecimal();
                if (this.dcmParametroProcessoLimiteVendaSat == 0)
                {
                    this.dcmParametroProcessoLimiteVendaSat = Constantes_Caixa.VALOR_LIMITE_VENDA_SAT;
                }


                this.dcmParametroProcessoValorSolicitaDocumento = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "VALOR_SOLICITA_DOCUMENTO_CAIXA", Root.Loja_Ativa.ID).DefaultDecimal();

                this.blnParametroProcessoCriarLog = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "Criar_Log_Processo_Caixa_Liberar_Venda_Novas_Tabelas").DefaultBool();

                this.dcmParamentroLojaPercentualDesconto = Convert.ToDecimal(Root.Lojas_Parametros.Retorna_Valor_Parametro_Descritivo(Root.Loja_Ativa.ID, "Percentual Desconto ECF")) / 100;

                this.dcmParametroProcessoValorLimiteSangria = Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "SALDO_LIMITE_SANGRIA").ToDecimal();

                this.blnParametroHistoricoProcesso = Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Utilizar historico de processo no caixa", "Sim");

                this.blnParametroImprimeProcedimentoGarantia = Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Imprimir Procedimento Garantia No Caixa", "Sim");

                object objRetorno = new object();
                objRetorno = Root.Lojas_Parametros.Retorna_Valor_Parametro_Descritivo(Root.Loja_Ativa.ID, "Valor Máximo para Liberação de Crédito sem Autorização Gerente");

                if (objRetorno == null)
                    objRetorno = 0;

                this.dcmValorLimiteAprovacao = Convert.ToDecimal(Convert.ToString(objRetorno).Replace(".", ","));

                this.blnUtilizaProdutoReciclavel = Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Verificar Produtos Reciclaveis", "Sim");

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Abrir_Caixa()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    return false;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Abrir_Caixa.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para abrir o caixa";
                    return false;
                }

                // Verifica se já existe o caixa aberto em outra maquina
                if (!this.Validar_Caixa_Aberto())
                {
                    return false;
                }

                // Verifica o Status da impressora
                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal;
                enuRetornoImpressoraFiscal = this.objImpressaoFiscal.Verificar_Status_Impressora();
                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    if (enuRetornoImpressoraFiscal == Retorno_Impressora_Fiscal.Impressora_Fiscal_Bloqueio_Reducao_Z
                            && this.Verifica_Cupom_Fiscal_Aberto())
                    {
                        this.Cancelar_Cupom();
                    }

                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    return false;
                }

                // Buscar os dados do cartão para gravar o ID correspondente ao retorno do Sitef
                this.dtsCadastroCartao = new Cartao_TEFBUS().Consultar_DataSet(string.Empty, false, 0, TipoServidor.LojaAtual);

                this.dttOperadoraCartao = new Operadora_CartaoBUS().Consultar_DataTable_Operadora_Cartao_Ativo(TipoServidor.LojaAtual);

                DBUtil objUtil = new DBUtil();

                this.dtsCaixaOperacao = new CaixaBUS().Abrir_DataSet_Caixa(this.intLojaID, this.intUsuario, Dns.GetHostName().ToUpper());

                bool blnFechouTela = false;
                // Usuário já abriu caixa alguma vez
                if (this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows.Count > 0)
                {
                    this.intUltimaOperacao = Convert.ToInt32(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Ultima_Operacao_Caixa_Dia"]);

                    // Tratamento para a Barra Funda
                    TimeSpan objDataIntervalo = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual) - Convert.ToDateTime(this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Data_Abertura"]);

                    // Se o caixa foi suspenso, desabilitar saldo inicial
                    if (Convert.ToString(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Caixa_Operacao_Data_Hora_Suspenso"]) != string.Empty)
                    {
                        if (this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows.Count > 0)
                        {
                            this.dcmSaldoInicial = Convert.ToDecimal(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Saldo"]);
                        }
                    }
                    else
                    {
                        blnFechouTela = true;

                        // Força o fechamento caso o caixa não tenha sido fechado corretamente no dia anterior e realizado apenas a sangria.
                        if (Strings.Format(this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Data_Abertura"], "dd/MM/yyyy") !=
                            Strings.Format(objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual), "dd/MM/yyyy"))
                        {
                            if (objDataIntervalo.TotalHours > this.Obter_Tolerancia_Hora_Limite_Fechamento_Caixa())
                            {
                                if (Convert.ToInt32(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Enum_Caixa_Tipo_Operacao_ID"]) == (Int32)OperacaoCaixa.Fechamento)
                                {
                                    this.enuSituacao = Operacao.Saldo_Inicial;
                                    this.txtMenu.Content = "Informe o saldo inicial";
                                }
                                else
                                {
                                    this.blnAbrirCaixaSangria = true;
                                    this.txtMenu.Content = "Sangria obrigatória. Informe o valor";
                                    this.enuSituacao = Operacao.Sangria_Valor_Operadora;

                                }
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                            }
                            else
                            {
                                if (this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows.Count > 0)
                                {
                                    this.dcmSaldoInicial = Convert.ToDecimal(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Saldo"]);
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Enum_Caixa_Tipo_Operacao_ID"]) == (Int32)OperacaoCaixa.Fechamento)
                            {
                                this.enuSituacao = Operacao.Saldo_Inicial;
                                this.txtMenu.Content = "Informe o saldo inicial";
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                            }
                            else
                            {
                                if (this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows.Count > 0)
                                {
                                    this.dcmSaldoInicial = Convert.ToDecimal(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Saldo"]);
                                }
                            }
                        }
                    }

                    if (blnFechouTela == false)
                    {
                        // Força o fechamento caso o caixa não tenha sido fechado corretamente no dia anterior
                        if (Strings.Format(this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Data_Abertura"], "dd/MM/yyyy") !=
                            Strings.Format(objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual), "dd/MM/yyyy") &
                            Strings.Trim(Convert.ToString(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Caixa_Operacao_Data_Hora_Suspenso"])) != string.Empty)
                        {
                            if (objDataIntervalo.TotalHours > this.Obter_Tolerancia_Hora_Limite_Fechamento_Caixa())
                            {
                                this.blnAbrirCaixaSangria = true;
                                this.txtMenu.Content = "Sangria obrigatória. Informe o valor";
                                this.enuSituacao = Operacao.Sangria_Valor_Operadora;
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                            }
                            else
                            {
                                if (this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows.Count > 0)
                                {
                                    this.dcmSaldoInicial = Convert.ToDecimal(this.dtsCaixaOperacao.Tables["Caixa_Ultima_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Saldo"]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 1a. vez que abre o caixa com este login
                    this.intUltimaOperacao = 0;
                    this.enuSituacao = Operacao.Saldo_Inicial;
                    this.txtMenu.Content = "Informe o saldo inicial";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                }

                // Caso a loja possua painel para chamar proximo cliente pelo SIM
                this.blnProximoCliente = Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Utiliza Painel Próximo Caixa", "Sim");

                if (this.blnProximoCliente)
                {
                    string strValorGuicheRegistro = RegistroWindows.Retorna_Valor_Persistido_no_Registro("Guiche_Caixa");
                    this.intNumeroGuiche = Convert.ToInt32((strValorGuicheRegistro == string.Empty ? "0" : strValorGuicheRegistro));
                    this.Carregar_IP_e_Porta_Painel_Guiche();

                    if (this.enuSituacao != Operacao.Saldo_Inicial
                            && this.enuSituacao != Operacao.Sangria_Valor_Operadora
                            && this.enuSituacao != Operacao.Fechamento_Pagamentos_Valores)
                    {
                        if (this.intNumeroGuiche == 0)
                        {
                            this.enuSituacao = Operacao.Numero_Guiche;
                            this.txtMenu.Content = "Informe o número do guichê";
                        }
                        else
                        {
                            this.enuSituacao = Operacao.Confirma_Guiche;
                            this.txtMenu.Content = "Guichê " + Convert.ToString(this.intNumeroGuiche) + ". Confirma? (S\\N)";
                        }
                        this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    }
                }
                else if (this.enuSituacao != Operacao.Saldo_Inicial
                            && this.enuSituacao != Operacao.Sangria_Valor_Operadora
                            && this.enuSituacao != Operacao.Fechamento_Pagamentos_Valores)
                {
                    this.blnCaixaAberto = true;
                    this.txtCodigoItemFabricante.Focus();

                    this.txtMenu.Content = "Verificando pendências do Sitef.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Verificar_Transacaoes_Pendentes_Sitef();

                    // Verifica se já existe o caixa aberto em outra maquina
                    if (!this.Validar_Caixa_Aberto())
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        this.blnCaixaAberto = false;
                        return false;
                    }

                    this.txtMenu.Content = "Preenchendo identificação do Caixa.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Preencher_Identificacao_Caixa_Sat();

                    this.txtMenu.Content = "Registrando abertura do Caixa.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Gravar_Abertura_Caixa();

                    this.txtUsuario.Text = this.dtoUsuario.Nome_Completo;
                    this.imgStatusUsuario.Source = new BitmapImage(new Uri("/MC_Formularios_Wpf;component/Images/MDI/Icone_Usuario.png", UriKind.Relative));

                    this.txtMenu.Content = "Verificando pendência de Sangria.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Verificar_Alerta_Sangria();

                    this.txtMenu.Content = "Inicializando vendas.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Inicializar_Nova_Venda();

                    this.txtMenu.Content = "Atualizar Data Movimento.";
                    Utilitario.Processar_Mensagens_Interface_WPF();
                    this.Atualizar_Data_Movimento_Impressora_Fiscal();

                    this.txtMenu.Content = string.Empty;

                }
                // Verifica se a loja utiliza controle por cancela
                this.blnControleCancela = Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Estacionamento", "Sim");

                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        private bool Suspender()
        {
            try
            {

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Suspender_Caixa.ToString()) == true)
                {
                    if (this.blnCaixaAberto == true)
                    {
                        new CaixaBUS().Fechar_Caixa(this.intLojaID, this.intUsuario, Dns.GetHostName().ToUpper(), true, 0);
                    }

                    this.Fechar_Porta_Impressora_Fiscal();

                    this.Limpar_Dados();
                    this.Limpar_Tela();
                    this.Limpar_Variaveis();
                    this.Limpar_Tela_Fechamento();

                }
                else
                {
                    this.txtMenu.Content = "Usuário não tem permissão para suspender";
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Fechar_Formulario()
        {
            try
            {
                // Se tiver cupom fiscal aberto, cancelar cupom antes de fechar o caixa.
                if (this.blnCupomAberto == true)
                {
                    this.txtMenu.Content = "Autenticar suspender, cancelar cupom";
                    this.enuSituacao = Operacao.Autenticar_Cancelar_Cupom_Suspender;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
                else
                {
                    this.txtMenu.Content = "Autenticar suspender caixa";
                    this.enuSituacao = Operacao.Autenticar_Suspender;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Gravar_Abertura_Caixa()
        {
            try
            {
                bool blnTemCaixaDia = false;
                bool blnTemOperacaoHoje = false;

                if (this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows.Count > 0)
                {
                    if (this.dtsCaixaOperacao.Tables.Count > 1)
                    {
                        blnTemOperacaoHoje = false;
                    }
                    else
                    {
                        blnTemOperacaoHoje = true;
                    }

                    blnTemCaixaDia = this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["TemCaixaDia"].DefaultBool();
                }

                this.Preencher_DataRow_Abertura_Caixa(blnTemOperacaoHoje);

                new CaixaBUS().Registrar_Abertura_Caixa(this.dtsCaixaTemporario, blnTemCaixaDia);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Dados_Rodape()
        {
            try
            {
                this.txtNomeEstacao.Text = Dns.GetHostName().ToUpper();
                this.txtVersao.Text = Root.Versao_Sistema_SIM();
                this.txtLoja.Text = Root.Loja_Ativa.Nome;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Localizar_LojaAtiva()
        {
            try
            {
                this.intLojaID = Root.Loja_Ativa.ID;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private int Obter_Tolerancia_Hora_Limite_Fechamento_Caixa()
        {
            try
            {
                return Root.ParametrosProcesso.Retorna_Valor_Parametro("CAIXA", "HORA_LIMITE_FECHAMENTO_CAIXA").ToInteger();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Criar objeto        "

        private void Criar_Inicilizar_Objetos()
        {
            try
            {
                // DataTable
                this.dttContato = new DataTable();
                this.dttCupomFiscal = new DataTable();
                this.dttCupomFiscalFechamento = new DataTable();
                this.dttDadosCartao = new DataTable();

                // DataSet
                this.dtsFormaXCliente = new DataSet();
                this.dtsCadastroCartao = new DataSet();
                this.dtsCaixaTemporario = new DataSet();
                this.dtsCaixaOperacao = new DataSet();
                this.dtsCliente = new DataSet();
                this.dtsConsultaPecaItens = new DataSet();
                this.dtsOrcamentoIt = new DataSet();
                this.dtsGridVenda = new DataSet();
                this.dtsPreVendaTemporario = new DataSet();
                this.dtsPreVendaEscolhido = new DataSet();
                this.dtsRomaneioTemporario = new DataSet();
                this.dtsCondicaoPagto = new DataSet();
                this.dtsRomaneioEstorno = new DataSet();

                // Objetos
                this.objImpressaoFiscal = new CaixaFactory(this.objComunicacaoImpressoraFiscal, this.objTipoImpressoraFiscal);
                this.dtoUsuario = new UsuarioDO();
                this.dtoUsuarioAutenticar = new UsuarioDO();
                this.dtoCaixaSatVenda = new SAT_VendaDO();

                this.objTipoProcessoSatFiscal = new Tipo_Processo_Sat_Fiscal_Factory(this.blnUtilizaControladorSat, 0, this.dtoSatCaixa);

                // DataSet Personalizado
                CaixaBUS busCaixa = new CaixaBUS();
                this.dtsOrcamentoIt = busCaixa.Criar_DataSet_Item_Orcamento();
                this.dtsCaixaTemporario = busCaixa.Criar_DataSet_Caixa();
                this.dtsConsultaPecaItens = busCaixa.Criar_DataSet_Peca();
                this.dtsGridVenda = busCaixa.Criar_DataSet_Venda();
                this.dtsPreVendaTemporario = busCaixa.Criar_DataSet_Pre_Venda();
                this.dttCupomFiscal = busCaixa.Criar_DataTable_Cupom_Fiscal();
                this.dttCupomFiscalFechamento = busCaixa.Criar_DataTable_Cupom_Fiscal_Fechamento();
                this.dtsRomaneioTemporario = busCaixa.Criar_DataSet_Romaneio();
                this.dtsCondicaoPagto = busCaixa.Criar_DataSet_Condicao_Pagto();
                this.dttDadosCartao = busCaixa.Criar_DataTable_Dados_Cartao();
                this.dttOperadoraCartaoRegra = busCaixa.Criar_DataTable_Operadora_Cartao_Regra();

            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Preencher objeto    "

        private void Preencher_DataRow_Abertura_Caixa(bool blnTemOperacaoHoje)
        {
            try
            {
                DataRow dtrCaixa = this.dtsCaixaTemporario.Tables["Caixa_Operacao"].NewRow();

                dtrCaixa["Lojas_ID"] = this.intLojaID;
                dtrCaixa["Usuario_Caixa_Operacao_ID"] = this.intUsuario;
                if (this.intUltimaOperacao == (Int32)OperacaoCaixa.Fechamento)
                {
                    dtrCaixa["Enum_Caixa_Tipo_Operacao_ID"] = OperacaoCaixa.Abertura;
                }
                else if (this.intUltimaOperacao == 0)
                {
                    dtrCaixa["Enum_Caixa_Tipo_Operacao_ID"] = (Int32)OperacaoCaixa.Abertura;
                }
                else
                {
                    dtrCaixa["Enum_Caixa_Tipo_Operacao_ID"] = (Int32)OperacaoCaixa.Reinicio;
                }
                dtrCaixa["Caixa_Operacao_Responsavel_Sangria"] = string.Empty;
                DBUtil objUtil = new DBUtil();
                dtrCaixa["Caixa_Operacao_Data_Hora_Operacao"] = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
                dtrCaixa["Caixa_Operacao_Saldo"] = this.dcmSaldoInicial.ToString();
                dtrCaixa["Caixa_Operacao_Nome_Computador"] = Dns.GetHostName().ToUpper();

                this.dtsCaixaTemporario.Tables["Caixa_Operacao"].Rows.Add(dtrCaixa);

                DataRow dtrCaixaDia = this.dtsCaixaTemporario.Tables["Caixa_Operacao_Dia"].NewRow();
                if (blnTemOperacaoHoje == false | (blnTemOperacaoHoje == true & this.intUltimaOperacao == (Int32)OperacaoCaixa.Fechamento))
                {

                    dtrCaixaDia["Lojas_ID"] = this.intLojaID;
                    dtrCaixaDia["Usuario_Caixa_Operacao_Dia_ID"] = this.intUsuario;
                    dtrCaixaDia["Enum_Caixa_Tipo_Operacao_Dia_ID"] = OperacaoCaixa.Abertura;
                    dtrCaixaDia["Caixa_Operacao_Dia_Data_Abertura"] = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
                    dtrCaixaDia["Caixa_Operacao_Dia_Data_Hora_Operacao"] = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
                    dtrCaixaDia["Caixa_Operacao_Dia_Saldo"] = this.dcmSaldoInicial.ToString();
                    dtrCaixaDia["Caixa_Operacao_Dia_Nome_Computador"] = Dns.GetHostName().ToUpper();

                }
                else
                {

                    dtrCaixaDia["Lojas_ID"] = this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Lojas_ID"];
                    dtrCaixaDia["Usuario_Caixa_Operacao_Dia_ID"] = this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Usuario_Caixa_Operacao_Dia_ID"];
                    if (this.intUltimaOperacao == (Int32)OperacaoCaixa.Fechamento)
                    {
                        dtrCaixaDia["Enum_Caixa_Tipo_Operacao_Dia_ID"] = (Int32)OperacaoCaixa.Abertura;
                    }
                    else
                    {
                        dtrCaixaDia["Enum_Caixa_Tipo_Operacao_Dia_ID"] = (Int32)OperacaoCaixa.Reinicio;
                    }
                    dtrCaixaDia["Caixa_Operacao_Dia_Data_Abertura"] = this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Data_Abertura"];
                    dtrCaixaDia["Caixa_Operacao_Dia_Data_Hora_Operacao"] = this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Data_Hora_Operacao"];
                    dtrCaixaDia["Caixa_Operacao_Dia_Saldo"] = this.dtsCaixaOperacao.Tables["Caixa_Operacao_Dia"].Rows[0]["Caixa_Operacao_Dia_Saldo"];
                    dtrCaixaDia["Caixa_Operacao_Dia_Nome_Computador"] = Dns.GetHostName().ToUpper();
                }

                this.dtsCaixaTemporario.Tables["Caixa_Operacao_Dia"].Rows.Add(dtrCaixaDia);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataTable_Cabecalho_Cupom()
        {
            try
            {
                this.dttCupomFiscal.Clear();
                this.dttCupomFiscalFechamento.Clear();
                DataRow dtrCupomFiscal = this.dttCupomFiscal.NewRow();

                if (this.blnUtilizaNFp)
                {
                    if (this.txtNomeCliente.Text.Length > 30)
                        dtrCupomFiscal["Cliente"] = this.txtNomeCliente.Text.Left(30);
                    else
                        dtrCupomFiscal["Cliente"] = this.txtNomeCliente.Text;

                    dtrCupomFiscal["Documento"] = DivUtil.Formatar_CPF_CNPJ(this.strCpfCnpjNotaFiscalPaulista);

                }
                else
                {
                    dtrCupomFiscal["Documento"] = "000.000.000-00";
                }

                this.dttCupomFiscal.Rows.Add(dtrCupomFiscal);

            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Preencher_DataTable_Item_Cupom()
        {

            try
            {
                foreach (DataRow dtrExclusao in this.dttCupomFiscal.Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                {
                    dtrExclusao.Delete();
                }

                foreach (DataRow dtrExclusao in this.dttCupomFiscalFechamento.Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                {
                    dtrExclusao.Delete();
                }

                int intUltimaLinha = 0;

                intUltimaLinha = this.dtsGridVenda.Tables["Venda_It"].Rows.Count - 1;

                if (Convert.ToInt32(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Tipo_Objeto"]) != Convert.ToInt32(Enumerados.Tipo_Objeto.Servico))
                {
                    DataRow dtrCupomFiscal = this.dttCupomFiscal.NewRow();

                    dtrCupomFiscal["Codigo"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Codigo"]);
                    dtrCupomFiscal["Tipo_Objeto"] = (Int32)Tipo_Objeto.Peca;
                    dtrCupomFiscal["Descricao"] = this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Descricao"].ToString();
                    dtrCupomFiscal["Imposto"] = this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Imposto"];
                    dtrCupomFiscal["Qtde"] = (Int32)this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Qtde"];
                    dtrCupomFiscal["Preco_Unitario"] = Convert.ToDecimal(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Preco_Unitario"]);
                    dtrCupomFiscal["Total"] = Convert.ToDecimal(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Total"]);
                    dtrCupomFiscal["IsCupomFiscal"] = true;
                    dtrCupomFiscal["IsRomaneio"] = this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["IsRomaneio"];

                    dtrCupomFiscal["Peca_Codigo_CFOP"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Codigo_CFOP"]);
                    dtrCupomFiscal["Class_Fiscal_ICMS"] = Convert.ToDecimal(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Class_Fiscal_ICMS"]);
                    dtrCupomFiscal["Peca_Codigo_Situacao_Tributaria"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Codigo_Situacao_Tributaria"]);
                    dtrCupomFiscal["Peca_ICMS_Substituicao_Tributaria"] = Convert.ToDecimal(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_ICMS_Substituicao_Tributaria"]);
                    dtrCupomFiscal["Peca_Percentual_Pis"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Percentual_Pis"]);
                    dtrCupomFiscal["Peca_Percentual_Cofins"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Percentual_Cofins"]);
                    dtrCupomFiscal["Peca_Codigo_Situacao_Tributaria_Pis"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Codigo_Situacao_Tributaria_Pis"]);
                    dtrCupomFiscal["Peca_Codigo_Situacao_Tributaria_Cofins"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Codigo_Situacao_Tributaria_Cofins"]);

                    dtrCupomFiscal["Class_Fiscal_CD"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Class_Fiscal_CD"]);
                    dtrCupomFiscal["Peca_Origem_Mercadoria"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Peca_Origem_Mercadoria"]);
                    dtrCupomFiscal["Class_Fiscal_IPI"] = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha]["Class_Fiscal_IPI"]);

                    this.dttCupomFiscal.Rows.Add(dtrCupomFiscal);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataRow_ItemOrcamento_Novo()
        {
            try
            {
                DataRow dtrOrcamentoIt = this.dtsOrcamentoIt.Tables["Orcamento_It"].NewRow();

                dtrOrcamentoIt["Qtde_Reciclavel"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Qtde_Reciclavel"];
                dtrOrcamentoIt["Acordo_Realizado"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Acordo_Realizado"];
                dtrOrcamentoIt["Produto_Reciclavel"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Produto_Reciclavel"];
                dtrOrcamentoIt["Produto_Reciclavel_Desconto"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Produto_Reciclavel_Desconto"];
                dtrOrcamentoIt["Objeto_ID"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_ID"];
                dtrOrcamentoIt["Peca_CodBarra_CdBarras"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_CodBarra_CdBarras"];
                if (this.txtCodigoItemFabricante.Text.Length <= NUMERO_PADRAO_CARACTERES_SERVICO)
                {
                    dtrOrcamentoIt["Codigo"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Codigo_Impressao_Fiscal"].ToString().PadLeft(NUMERO_PADRAO_CARACTERES_SERVICO, Convert.ToChar("0"));
                }
                {
                    dtrOrcamentoIt["Codigo"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Codigo_Impressao_Fiscal"].ToString();
                }
                dtrOrcamentoIt["Descricao"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Descricao_Impressao_Fiscal"];

                dtrOrcamentoIt["Orcamento_It_Qtde"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Quantidade"];
                dtrOrcamentoIt["Orcamento_It_Preco_Pago"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Preco_Valor"];
                dtrOrcamentoIt["Orcamento_It_Preco_Lista"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Preco_Lista"];
                dtrOrcamentoIt["Preco_Total"] = ((Convert.ToInt32(this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Quantidade"])
                                                    * Convert.ToInt32(this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Embalagem_Quantidade"]))
                                                    * Convert.ToDecimal(this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Preco_Valor"])).ToString("#,##0.00");
                dtrOrcamentoIt["Preco_Total_Lista"] = ((Convert.ToInt32(this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Quantidade"])
                                                        * Convert.ToInt32(this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Embalagem_Quantidade"]))
                                                        * Convert.ToDecimal(this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Preco_Lista"])).ToString("#,##0.00");
                dtrOrcamentoIt["Enum_Objeto_Tipo_ID"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Enum_Objeto_Tipo_ID"];
                dtrOrcamentoIt["Orcamento_It_Preco_Vista"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Preco_Valor"];
                dtrOrcamentoIt["Orcamento_It_Valor_Desconto"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Desconto"];
                dtrOrcamentoIt["Orcamento_It_Valor_Comissao"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Valor_Comissao"];
                dtrOrcamentoIt["Orcamento_It_Custo_Reposicao"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Preco_Custo_Reposicao"];
                dtrOrcamentoIt["Orcamento_It_Custo_Reposicao_Efetivo"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Estoque_Custo_Reposicao_Efetivo"];
                dtrOrcamentoIt["Orcamento_It_Custo_Medio"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Estoque_Custo_Medio"];
                dtrOrcamentoIt["Orcamento_It_Custo_Unitario"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Estoque_Custo_Ultimo_Custo"];
                dtrOrcamentoIt["QtdeMinVenda"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_QtMinimaVenda"];
                dtrOrcamentoIt["Orcamento_It_Sequencial"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Item"];
                dtrOrcamentoIt["CodigoFabricante"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Fabricante_CD"];
                dtrOrcamentoIt["CodigoProduto"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Produto_CD"];
                dtrOrcamentoIt["CodigoPeca"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_CD"];
                dtrOrcamentoIt["CodItemFabricante"] = string.Empty;
                dtrOrcamentoIt["NomeFabricante"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Fabricante_NmFantasia"];
                dtrOrcamentoIt["NomeProduto"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Produto_Ds"];
                dtrOrcamentoIt["Endereco_Peca"] = string.Empty;
                dtrOrcamentoIt["Lojas_ID"] = this.intLojaID;
                dtrOrcamentoIt["Imposto"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Imposto"];
                dtrOrcamentoIt["Peca_Embalagem_Quantidade"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Embalagem_Quantidade"];

                dtrOrcamentoIt["Peca_Codigo_CFOP"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Codigo_CFOP"];
                dtrOrcamentoIt["Class_Fiscal_ICMS"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Class_Fiscal_ICMS"];
                dtrOrcamentoIt["Peca_Codigo_Situacao_Tributaria"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Codigo_Situacao_Tributaria"];
                dtrOrcamentoIt["Peca_ICMS_Substituicao_Tributaria"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_ICMS_Substituicao_Tributaria"];
                dtrOrcamentoIt["Peca_Percentual_Pis"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Percentual_Pis"];
                dtrOrcamentoIt["Peca_Percentual_Cofins"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Percentual_Cofins"];
                dtrOrcamentoIt["Peca_Codigo_Situacao_Tributaria_Pis"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Codigo_Situacao_Tributaria_Pis"];
                dtrOrcamentoIt["Peca_Codigo_Situacao_Tributaria_Cofins"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Codigo_Situacao_Tributaria_Cofins"];
                dtrOrcamentoIt["Class_Fiscal_CD"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Class_Fiscal_CD"];
                dtrOrcamentoIt["Comissao_Percentual"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Comissao_Percentual"];
                dtrOrcamentoIt["Peca_Origem_Mercadoria"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Peca_Origem_Mercadoria"];
                dtrOrcamentoIt["Class_Fiscal_IPI"] = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows[0]["Class_Fiscal_IPI"];

                dtrOrcamentoIt["Sequencial_Cupom"] = this.intItemGridCupom + 1;

                dtrOrcamentoIt["Item_Digitado"] = false;
                this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Add(dtrOrcamentoIt);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataRow_Condicao_Pgto()
        {
            try
            {
                DataRow[] dtrCondicaoPagtoCliente = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select(" Forma_Pagamento_ID = " + this.intFormaPagamentoID.ToString() + " AND Condicao_Pagamento_ID = " + this.intCondicaoPagtoID.ToString());
                DataRow[] dtrCondicaoPagtoPrazo = this.dtsFormaXCliente.Tables["Prazo_Pagamento_Cliente_Tipo"].Select(" Condicao_Pagamento_ID = " + this.intCondicaoPagtoID.ToString());
                DataRow[] dtrCondicaoPagtoParcelas = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Consumidor_Final"].Select(" Forma_Pagamento_ID = " + this.intFormaPagamentoID.ToString() + " AND Condicao_Pagamento_ID = " + this.intCondicaoPagtoID.ToString());

                decimal dcmValorTotalVendaPrazo = 0;
                dcmValorTotalVendaPrazo = Convert.ToDecimal(this.txtComando.Text);

                decimal dcmJuros = 0;
                decimal dcmValorEntrada = 0;
                int intNrParcela = 1;
                bool blnComEntrada = false;
                if (this.intFormaPagamentoID == Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO)
                {
                    if (Convert.ToInt32(dtrCondicaoPagtoPrazo[0]["Prazo_Pagamento_Parcela_Dias"]) == 0)
                    {
                        blnComEntrada = true;
                        dcmValorEntrada = ((Convert.ToDecimal(this.txtComando.Text) * Convert.ToDecimal(dtrCondicaoPagtoPrazo[0]["Prazo_Pagamento_Parcela_Percentual"])) / 100);
                    }
                    else
                    {
                        blnComEntrada = false;
                        dcmValorEntrada = 0;
                    }


                    intNrParcela = Convert.ToInt32(dtrCondicaoPagtoParcelas[0]["Nr_Parcelas"]);
                    dcmJuros = Convert.ToDecimal(dtrCondicaoPagtoPrazo[0]["Condicao_Pagamento_Percentual_Juros"]);
                    // Calcular juros
                    if (dcmJuros > 0)
                    {
                        int intCont;
                        // Calcular juros apenas no valor restante a pagar
                        if (blnComEntrada == true)
                        {
                            dcmValorTotalVendaPrazo = Convert.ToDecimal(this.txtComando.Text) - dcmValorEntrada;
                            for (intCont = 1; intCont <= intNrParcela - 1; intCont++)
                            {
                                dcmValorTotalVendaPrazo = dcmValorTotalVendaPrazo + ((dcmValorTotalVendaPrazo * dcmJuros) / 100);
                            }
                        }
                        else
                        {
                            for (intCont = 1; intCont <= intNrParcela; intCont++)
                            {
                                dcmValorTotalVendaPrazo = dcmValorTotalVendaPrazo + ((dcmValorTotalVendaPrazo * dcmJuros) / 100);
                            }
                        }
                        dcmValorTotalVendaPrazo = dcmValorTotalVendaPrazo + dcmValorEntrada;
                    }
                }

                DataRow[] dtrCondicaoPgtoGrid = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Condicao_Pagamento_ID = " + this.intCondicaoPagtoID.ToString() + " AND Forma_Pagamento_ID = " + this.intFormaPagamentoID.ToString());
                if (dtrCondicaoPgtoGrid.Length > 0 && this.intFormaPagamentoID == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                {
                    dtrCondicaoPgtoGrid[0]["Valor_Informado"] = Convert.ToDecimal(dtrCondicaoPgtoGrid[0]["Valor_Informado"]) + Convert.ToDecimal(this.txtComando.Text);
                    dtrCondicaoPgtoGrid[0]["Valor_Parcela"] = Convert.ToDecimal(dtrCondicaoPgtoGrid[0]["Valor_Parcela"]) + Convert.ToDecimal(this.txtComando.Text);

                    dtrCondicaoPgtoGrid[0]["Troco"] = this.dcmTotalVenda > Convert.ToDecimal(dtrCondicaoPgtoGrid[0]["Valor_Informado"]) ? 0 : (Convert.ToDecimal(dtrCondicaoPgtoGrid[0]["Valor_Informado"]) - this.dcmTotalVenda);

                    this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].AcceptChanges();
                }
                else
                {
                    DataRow dtrCondicaoPgto = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].NewRow();

                    dtrCondicaoPgto["Item"] = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count + 1;
                    dtrCondicaoPgto["Condicao_Pagamento_ID"] = this.intCondicaoPagtoID;
                    dtrCondicaoPgto["Forma_Pagamento_ID"] = this.intFormaPagamentoID;
                    dtrCondicaoPgto["Condicao_Pagamento_Nome"] = dtrCondicaoPagtoCliente[0]["Condicao_Pagamento_Descricao"];

                    dtrCondicaoPgto["Data"] = ((DateTime)DateTime.Today.AddDays(Convert.ToInt32(dtrCondicaoPagtoPrazo[0]["Prazo_Pagamento_Parcela_Dias"]))).ToString("dd/MM/yyyy");
                    dtrCondicaoPgto["Dia_Parcela"] = dtrCondicaoPagtoPrazo[0]["Prazo_Pagamento_Parcela_Dias"];
                    dtrCondicaoPgto["Valor_Parcela"] = ((Convert.ToDecimal(dcmValorTotalVendaPrazo) * Convert.ToDecimal(dtrCondicaoPagtoPrazo[0]["Prazo_Pagamento_Parcela_Percentual"])) / 100).ToString("#,##0.00");
                    dtrCondicaoPgto["Valor_Informado"] = Convert.ToDecimal(this.txtComando.Text);
                    dtrCondicaoPgto["Troco"] = this.dcmTotalVenda > (this.Calcular_Valor_Total_Pagamento() + Convert.ToDecimal(this.txtComando.Text)) ? 0 : ((this.Calcular_Valor_Total_Pagamento() + Convert.ToDecimal(this.txtComando.Text)) - this.dcmTotalVenda);

                    dtrCondicaoPgto["Bandeira"] = string.Empty;
                    dtrCondicaoPgto["Numero_de_Parcelas"] = intNrParcela;
                    dtrCondicaoPgto["Cartao_Debito"] = dtrCondicaoPagtoCliente[0]["Forma_Pagamento_Emissao_Cartao_Debito"];
                    dtrCondicaoPgto["Cartao_Credito"] = dtrCondicaoPagtoCliente[0]["Forma_Pagamento_Emissao_Cartao_Credito"];
                    dtrCondicaoPgto["Cheque"] = dtrCondicaoPagtoCliente[0]["Forma_Pagamento_Emissao_Cheque"];
                    dtrCondicaoPgto["Prazo_Pagamento_Parcela_Percentual"] = dtrCondicaoPagtoPrazo[0]["Prazo_Pagamento_Parcela_Percentual"].DefaultDecimal();

                   this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Add(dtrCondicaoPgto);

                }
                this.txtTroco.Text = this.dcmTotalVenda > this.Calcular_Valor_Total_Pagamento() ? "0,00" : (this.Calcular_Valor_Total_Pagamento() - this.dcmTotalVenda).ToString("#,##0.00");
                this.txtAPagar.Text = this.dcmTotalVenda > this.Calcular_Valor_Total_Pagamento() ? (this.dcmTotalVenda - this.Calcular_Valor_Total_Pagamento()).ToString("#,##0.00") : "0,00";

                this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].DefaultView.Sort = "Item";

                this.objPagamentos.DataContext = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"];

                if (this.objPagamentos.Items.Count > 0)
                {
                    this.objPagamentos.ScrollIntoView(this.objPagamentos.Items[this.objPagamentos.Items.Count - 1]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataRow_Romaneio_Pre_Venda_Ct(int intNumeroRomaneio)
        {
            try
            {
                DataRow dtrRomaneioCt = this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].NewRow();

                DataRow[] dtrEscolhido = null;
                dtrEscolhido = this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Select("Romaneio_Pre_Venda_Ct_ID = " + intNumeroRomaneio);

                dtrRomaneioCt["Comanda_Interna_ID"] = dtrEscolhido[0]["Comanda_Interna_ID"];
                dtrRomaneioCt["Comanda_Externa_ID"] = dtrEscolhido[0]["Comanda_Externa_ID"];
                dtrRomaneioCt["Romaneio_Ct_ID"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_ID"];
                dtrRomaneioCt["Romaneio_Pre_Venda_Ct_ID"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_ID"];
                dtrRomaneioCt["Lojas_ID"] = dtrEscolhido[0]["Lojas_ID"];
                dtrRomaneioCt["Cliente_ID"] = dtrEscolhido[0]["Cliente_ID"];
                dtrRomaneioCt["Pessoa_Autorizada_ID"] = dtrEscolhido[0]["Pessoa_Autorizada_ID"];
                dtrRomaneioCt["Enum_Romaneio_Tipo_ID"] = dtrEscolhido[0]["Enum_Romaneio_Tipo_ID"];
                dtrRomaneioCt["Enum_Romaneio_Status_ID"] = Convert.ToInt32(Enumerados.StatusRomaneioVenda.Liberado);
                dtrRomaneioCt["Usuario_Vendedor_ID"] = dtrEscolhido[0]["Usuario_Vendedor_ID"];
                dtrRomaneioCt["Usuario_Gerente_ID"] = dtrEscolhido[0]["Usuario_Gerente_ID"];
                dtrRomaneioCt["Condicao_Pagamento_ID"] = dtrEscolhido[0]["Condicao_Pagamento_ID"];

                dtrRomaneioCt["Romaneio_Grupo_Origem_Resta_ID"] = dtrEscolhido[0]["Romaneio_Grupo_Origem_Resta_ID"];
                dtrRomaneioCt["Loja_Origem_Resta_ID"] = dtrEscolhido[0]["Loja_Origem_Resta_ID"];

                dtrRomaneioCt["Romaneio_Ct_Data_Geracao"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_Data_Geracao"];
                dtrRomaneioCt["Romaneio_Ct_Cliente_CNPJCPF"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"];
                dtrRomaneioCt["Romaneio_Ct_Cliente_Nome"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_Cliente_Nome"];
                dtrRomaneioCt["Romaneio_Ct_Cliente_Telefone"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_Cliente_Telefone"];
                dtrRomaneioCt["Romaneio_Ct_Valor_Total_Pago"] = dtrEscolhido[0]["Romaneio_Pre_Venda_CT_Valor_Total_Pago"];
                dtrRomaneioCt["Romaneio_Ct_Valor_Total_Lista"] = dtrEscolhido[0]["Romaneio_Pre_Venda_CT_Valor_Total_Lista"];

                dtrRomaneioCt["Romaneio_Ct_Valor_Real"] = 0.0;

                dtrRomaneioCt["Romaneio_CT_Observacao"] = dtrEscolhido[0]["Romaneio_Pre_Venda_CT_Observacao"];

                this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows.Add(dtrRomaneioCt);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataRow_Romaneio_Pre_Venda_It(int intNumeroRomaneio)
        {
            try
            {
                DataRow[] dtrEscolhido = null;
                dtrEscolhido = this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_It"].Select("Romaneio_Pre_Venda_Ct_ID = " + intNumeroRomaneio);

                int intPecaKITCompraCTID = 0;
                decimal dcmCustoTotalKit = 0;


                foreach (DataRow dtrEscolhidoLinhaAtual in dtrEscolhido)
                {
                    if (dtrEscolhidoLinhaAtual["Peca_KIT_Compra_CT_ID"].DefaultInteger() != 0 && dtrEscolhidoLinhaAtual["Peca_KIT_Compra_CT_ID"].DefaultInteger() != intPecaKITCompraCTID)
                    {
                        intPecaKITCompraCTID = dtrEscolhidoLinhaAtual["Peca_KIT_Compra_CT_ID"].DefaultInteger();
                        dcmCustoTotalKit = 0;
                        foreach (DataRow dtrItensKit in dtrEscolhido)
                        {
                            if (dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_ID"].DefaultInteger() != dtrItensKit["Romaneio_Pre_Venda_It_ID"].DefaultInteger())
                            {
                                continue;
                            }

                            // Não considera o kit, apenas o componente
                            if (dtrItensKit["Peca_Kit_Compra_IT_ID"].DefaultInteger() == 0)
                            {
                                continue;
                            }

                            dcmCustoTotalKit = dcmCustoTotalKit + dtrItensKit["Romaneio_Pre_Venda_It_Custo_Reposicao"].DefaultDecimal();
                        }
                    }

                    DataRow dtrRomaneioIt = this.dtsRomaneioTemporario.Tables["Romaneio_It"].NewRow();

                    dtrRomaneioIt["Romaneio_It_ID"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_ID"];
                    dtrRomaneioIt["Romaneio_Ct_ID"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_Ct_ID"];
                    dtrRomaneioIt["Lojas_ID"] = dtrEscolhidoLinhaAtual["Lojas_ID"];
                    dtrRomaneioIt["Objeto_ID"] = dtrEscolhidoLinhaAtual["Objeto_ID"];
                    dtrRomaneioIt["Enum_Objeto_Tipo_ID"] = dtrEscolhidoLinhaAtual["Enum_Objeto_Tipo_ID"];
                    dtrRomaneioIt["Peca_Kit_Ct_ID"] = dtrEscolhidoLinhaAtual["Peca_Kit_Ct_ID"];
                    dtrRomaneioIt["Romaneio_It_Sequencial"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Sequencial"];
                    dtrRomaneioIt["Romaneio_It_Qtde"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Qtde"];
                    if (dtrEscolhidoLinhaAtual["Peca_KIT_Compra_CT_ID"].DefaultInteger() != 0 && dtrEscolhidoLinhaAtual["Peca_Kit_Compra_IT_ID"].DefaultInteger() != 0)
                    {
                        dtrRomaneioIt["Romaneio_It_Preco_Pago"] = ((dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Custo_Reposicao"].DefaultDecimal() / dcmCustoTotalKit) * dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Preco_Pago"].DefaultDecimal()).ToDecimalRound(2);
                        dtrRomaneioIt["Romaneio_It_Preco_Lista"] = ((dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Custo_Reposicao"].DefaultDecimal() / dcmCustoTotalKit) * dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Preco_Lista"].DefaultDecimal()).ToDecimalRound(2);
                    }
                    else
                    {
                        dtrRomaneioIt["Romaneio_It_Preco_Pago"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Preco_Pago"];
                        dtrRomaneioIt["Romaneio_It_Preco_Lista"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Preco_Lista"];
                    }
                    dtrRomaneioIt["Romaneio_It_Valor_Desconto"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Valor_Desconto"];
                    dtrRomaneioIt["Romaneio_It_Valor_Comissao"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Valor_Comissao"];
                    dtrRomaneioIt["Romaneio_It_Custo_Reposicao"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Custo_Reposicao"];
                    dtrRomaneioIt["Romaneio_It_Custo_Reposicao_Efetivo"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Custo_Reposicao_Efetivo"];
                    dtrRomaneioIt["Romaneio_It_Custo_Medio"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Custo_Medio"];
                    dtrRomaneioIt["Romaneio_It_Custo_Unitario"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Custo_Unitario"];
                    dtrRomaneioIt["Enum_Romaneio_Tipo_ID"] = dtrEscolhidoLinhaAtual["Enum_Romaneio_Tipo_ID"];
                    dtrRomaneioIt["ItemImpresso"] = false;
                    dtrRomaneioIt["Romaneio_It_Instalado"] = 0;

                    // Telepreço
                    dtrRomaneioIt["Romaneio_Telepreco_IT_ID"] = dtrEscolhidoLinhaAtual["Romaneio_Telepreco_IT_ID"];
                    dtrRomaneioIt["Lojas_Telepreco_ID"] = dtrEscolhidoLinhaAtual["Lojas_Telepreco_ID"];
                    dtrRomaneioIt["Usuario_Telepreco_ID"] = dtrEscolhidoLinhaAtual["Usuario_Telepreco_ID"];
                    dtrRomaneioIt["Romaneio_It_Valor_Comissao_Telepreco"] = dtrEscolhidoLinhaAtual["Romaneio_Telepreco_It_Valor_Comissao"];
                    if (dtrEscolhidoLinhaAtual["Enum_Romaneio_Tipo_ID"] != null &&
                        Convert.ToInt32(dtrEscolhidoLinhaAtual["Enum_Romaneio_Tipo_ID"]).Equals(Convert.ToInt32(Enumerados.TipoRomaneio.Troca)))
                    {
                        dtrRomaneioIt["Romaneio_It_Valor_Comissao_Telepreco"] =
                            Convert.ToDouble(dtrRomaneioIt["Romaneio_It_Valor_Comissao_Telepreco"]) * -1.0;
                    }

                    // Cupom
                    dtrRomaneioIt["Romaneio_Pre_Venda_Ct_ID"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_Ct_ID"];
                    dtrRomaneioIt["Codigo"] = dtrEscolhidoLinhaAtual["Codigo"];
                    dtrRomaneioIt["Descricao"] = dtrEscolhidoLinhaAtual["Descricao"];
                    dtrRomaneioIt["Imposto"] = dtrEscolhidoLinhaAtual["Imposto"];

                    dtrRomaneioIt["Peca_Codigo_CFOP"] = dtrEscolhidoLinhaAtual["Peca_Codigo_CFOP"];
                    dtrRomaneioIt["Class_Fiscal_ICMS"] = dtrEscolhidoLinhaAtual["Class_Fiscal_ICMS"];
                    dtrRomaneioIt["Peca_Codigo_Situacao_Tributaria"] = dtrEscolhidoLinhaAtual["Peca_Codigo_Situacao_Tributaria"];
                    dtrRomaneioIt["Peca_ICMS_Substituicao_Tributaria"] = dtrEscolhidoLinhaAtual["Peca_ICMS_Substituicao_Tributaria"];
                    dtrRomaneioIt["Peca_Percentual_Pis"] = dtrEscolhidoLinhaAtual["Peca_Percentual_Pis"];
                    dtrRomaneioIt["Peca_Percentual_Cofins"] = dtrEscolhidoLinhaAtual["Peca_Percentual_Cofins"];
                    dtrRomaneioIt["Peca_Codigo_Situacao_Tributaria_Pis"] = dtrEscolhidoLinhaAtual["Peca_Codigo_Situacao_Tributaria_Pis"];
                    dtrRomaneioIt["Peca_Codigo_Situacao_Tributaria_Cofins"] = dtrEscolhidoLinhaAtual["Peca_Codigo_Situacao_Tributaria_Cofins"];
                    dtrRomaneioIt["Class_Fiscal_CD"] = dtrEscolhidoLinhaAtual["Class_Fiscal_CD"];

                    dtrRomaneioIt["Peca_KIT_Compra_CT_ID"] = dtrEscolhidoLinhaAtual["Peca_KIT_Compra_CT_ID"];
                    dtrRomaneioIt["Peca_Kit_Compra_IT_ID"] = dtrEscolhidoLinhaAtual["Peca_Kit_Compra_IT_ID"];

                    dtrRomaneioIt["Romaneio_Pre_Venda_It_Qtde_Reciclavel"] = dtrEscolhidoLinhaAtual["Romaneio_Pre_Venda_It_Qtde_Reciclavel"];
                    dtrRomaneioIt["Produto_Reciclavel"] = dtrEscolhidoLinhaAtual["Produto_Reciclavel"];
                    dtrRomaneioIt["Qtde_Reciclavel"] = dtrEscolhidoLinhaAtual["Qtde_Reciclavel"];
                    dtrRomaneioIt["Acordo_Realizado"] = dtrEscolhidoLinhaAtual["Acordo_Realizado"];
                    dtrRomaneioIt["Produto_Reciclavel_Desconto"] = dtrEscolhidoLinhaAtual["Produto_Reciclavel_Desconto"];
                    dtrRomaneioIt["Reciclagem_Loja_ID"] = dtrEscolhidoLinhaAtual["Reciclagem_Loja_ID"];
                    dtrRomaneioIt["Reciclagem_IT_ID"] = dtrEscolhidoLinhaAtual["Reciclagem_IT_ID"];

                    dtrRomaneioIt["Peca_Origem_Mercadoria"] = dtrEscolhidoLinhaAtual["Peca_Origem_Mercadoria"].DefaultString();
                    dtrRomaneioIt["Class_Fiscal_IPI"] = dtrEscolhidoLinhaAtual["Class_Fiscal_IPI"].DefaultString();

                    this.dtsRomaneioTemporario.Tables["Romaneio_It"].Rows.Add(dtrRomaneioIt);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataRow_Tela_Inclusao_Romaneio(int intNumeroRomaneio)
        {
            try
            {

                DataRow dtrTela = this.dtsPreVendaTemporario.Tables["Tela"].NewRow();

                DataRow[] dtrEscolhido = null;
                dtrEscolhido = this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Select("Romaneio_Pre_Venda_Ct_ID = " + intNumeroRomaneio);

                dtrTela["Comanda_Interna_ID"] = dtrEscolhido[0]["Comanda_Interna_ID"];
                dtrTela["Comanda_Externa_ID"] = dtrEscolhido[0]["Comanda_Externa_ID"];
                dtrTela["Romaneio_Pre_Venda_Ct_ID"] = dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_ID"];
                dtrTela["Cliente_Nome"] = dtrEscolhido[0]["Cliente_Nome"];
                dtrTela["Romaneio_Pre_Venda_CT_Valor_Total_Pago"] = dtrEscolhido[0]["Romaneio_Pre_Venda_CT_Valor_Total_Pago"];
                dtrTela["Condicao_Pagamento_Nome"] = dtrEscolhido[0]["Condicao_Pagamento_Nome"];
                dtrTela["Usuario_Vendedor_Nome"] = dtrEscolhido[0]["Usuario_Vendedor_Nome"];
                dtrTela["Enum_Romaneio_Tipo_Extenso"] = dtrEscolhido[0]["Enum_Romaneio_Tipo_Extenso"];
                dtrTela["Romaneio_Pre_Venda_Ct_Data_Geracao"] = Strings.Format(dtrEscolhido[0]["Romaneio_Pre_Venda_Ct_Data_Geracao"], "dd/MM/yy");
                dtrTela["Romaneio_Pre_Venda_CT_Valor_Total_Lista"] = dtrEscolhido[0]["Romaneio_Pre_Venda_CT_Valor_Total_Lista"];
                dtrTela["Condicao_Pagamento_ID"] = dtrEscolhido[0]["Condicao_Pagamento_ID"];
                dtrTela["PagtoDinheiro"] = dtrEscolhido[0]["PagtoDinheiro"];
                dtrTela["CNPJCPF_Cliente"] = dtrEscolhido[0]["CNPJCPF_Cliente"];
                dtrTela["Cliente_ID"] = dtrEscolhido[0]["Cliente_ID"];
                dtrTela["ValorVendaResta"] = dtrEscolhido[0]["ValorVendaResta"];

                this.dtsPreVendaTemporario.Tables["Tela"].Rows.Add(dtrTela);

            }
            catch (Exception)
            {
                throw;
            }
        }

        // Gera Pre-venda CT para autoserviço
        private bool Preencher_DataRow_Ct_Pre_Venda()
        {
            try
            {
                if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                {
                    DataRow dtrIncluirCt = this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].NewRow();

                    dtrIncluirCt["Comanda_Interna_ID"] = 0;
                    dtrIncluirCt["Comanda_Externa_ID"] = 0;
                    dtrIncluirCt["Romaneio_Pre_Venda_Ct_ID"] = 0;

                    dtrIncluirCt["Lojas_ID"] = this.intLojaID;
                    dtrIncluirCt["Cliente_ID"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strClienteNotaFiscalPaulistaID : this.strClienteID;

                    dtrIncluirCt["Pessoa_Autorizada_ID"] = string.Empty;

                    dtrIncluirCt["Enum_Romaneio_Tipo_ID"] = Convert.ToInt32(Enumerados.TipoRomaneio.Auto_Servico);
                    dtrIncluirCt["Enum_Romaneio_Status_ID"] = Convert.ToInt32(Enumerados.StatusRomaneioVenda.Liberado);
                    dtrIncluirCt["Usuario_Vendedor_ID"] = this.intUsuario;
                    dtrIncluirCt["Usuario_Gerente_ID"] = 0;
                    dtrIncluirCt["Usuario_Separador_ID"] = 0;
                    dtrIncluirCt["Condicao_Pagamento_ID"] = this.intCondicaoPagtoID;
                    dtrIncluirCt["Romaneio_Pre_Venda_Ct_Data_Geracao"] = this.dtmDataLiberacao;
                    dtrIncluirCt["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strCpfCnpjNotaFiscalPaulista : this.strCpfCnpj;

                    decimal dcmValorTotalPago = (decimal)this.dtsOrcamentoIt.Tables["Orcamento_It"].Compute("SUM(Preco_Total)", string.Empty);
                    decimal dcmValorTotalLista = (decimal)this.dtsOrcamentoIt.Tables["Orcamento_It"].Compute("SUM(Preco_Total_Lista)", string.Empty);

                    dtrIncluirCt["Romaneio_Pre_Venda_CT_Valor_Total_Pago"] = dcmValorTotalPago;
                    dtrIncluirCt["Romaneio_Pre_Venda_CT_Valor_Total_Lista"] = dcmValorTotalLista;
                    dtrIncluirCt["Romaneio_Pre_Venda_Ct_Apresentou_NF"] = false;

                    dtrIncluirCt["Romaneio_Pre_Venda_Ct_Cliente_Nome"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.txtNomeCliente.Text : this.strClienteNome;

                    this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows.Add(dtrIncluirCt);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Gera Pre-venda IT para autoserviço
        private bool Preencher_DataRow_It_Pre_Venda()
        {
            try
            {
                if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                {
                    foreach (DataRow dtrOrcamento in this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows)
                    {
                        DataRow dtrIncluirIt = this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_It"].NewRow();

                        dtrIncluirIt["Romaneio_Pre_Venda_Ct_ID"] = 0;
                        dtrIncluirIt["Lojas_ID"] = dtrOrcamento["Lojas_ID"];
                        dtrIncluirIt["Objeto_ID"] = dtrOrcamento["Objeto_ID"];
                        dtrIncluirIt["Enum_Objeto_Tipo_ID"] = dtrOrcamento["Enum_Objeto_Tipo_ID"];
                        dtrIncluirIt["Peca_Kit_Ct_ID"] = 0;
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Sequencial"] = dtrOrcamento["Orcamento_It_Sequencial"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Qtde"] = Convert.ToInt32(dtrOrcamento["Orcamento_It_Qtde"].ToInteger() * dtrOrcamento["Peca_Embalagem_Quantidade"].ToInteger());
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Preco_Pago"] = dtrOrcamento["Orcamento_It_Preco_Pago"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Preco_Lista"] = dtrOrcamento["Orcamento_It_Preco_Lista"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Valor_Desconto"] = dtrOrcamento["Orcamento_It_Valor_Desconto"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Valor_Comissao"] = dtrOrcamento["Orcamento_It_Valor_Comissao"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Custo_Reposicao"] = dtrOrcamento["Orcamento_It_Custo_Reposicao"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Custo_Reposicao_Efetivo"] = dtrOrcamento["Orcamento_It_Custo_Reposicao_Efetivo"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Custo_Medio"] = dtrOrcamento["Orcamento_It_Custo_Medio"];
                        dtrIncluirIt["Romaneio_Pre_Venda_It_Custo_Unitario"] = dtrOrcamento["Orcamento_It_Custo_Unitario"];
                        dtrIncluirIt["Item_Digitado"] = dtrOrcamento["Item_Digitado"];
                        dtrIncluirIt["Desconto_Caixa"] = dtrOrcamento["Desconto_Caixa"];
                        dtrIncluirIt["Usuario_Aprovacao_Desconto_ID"] = dtrOrcamento["Usuario_Aprovacao_Desconto_ID"];

                        // Cupom
                        dtrIncluirIt["Codigo"] = dtrOrcamento["Codigo"];
                        dtrIncluirIt["Descricao"] = dtrOrcamento["Descricao"];
                        dtrIncluirIt["Imposto"] = dtrOrcamento["Imposto"];

                        dtrIncluirIt["Romaneio_Pre_Venda_It_Qtde_Reciclavel"] = dtrOrcamento["Romaneio_Pre_Venda_It_Qtde_Reciclavel"];
                        dtrIncluirIt["Qtde_Reciclavel"] = dtrOrcamento["Qtde_Reciclavel"];
                        dtrIncluirIt["Produto_Reciclavel"] = dtrOrcamento["Produto_Reciclavel"];
                        dtrIncluirIt["Acordo_Realizado"] = dtrOrcamento["Acordo_Realizado"];

                        this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_It"].Rows.Add(dtrIncluirIt);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Peca_Digitada()
        {
            try
            {
                if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                {
                    foreach (DataRow dtrOrcamento in this.dtsOrcamentoIt.Tables["Orcamento_It"].Select("Item_Digitado = 1"))
                    {
                        DataRow dtrIncluirPecaDigitada = this.dtsPreVendaTemporario.Tables["Peca_Digitada"].NewRow();

                        dtrIncluirPecaDigitada["Lojas_ID"] = dtrOrcamento["Lojas_ID"];
                        dtrIncluirPecaDigitada["Enum_Tipo_Origem_ID"] = Pecas_Digitadas_Por_Processo.Caixa.DefaultInteger();
                        dtrIncluirPecaDigitada["Peca_ID"] = dtrOrcamento["Objeto_ID"];

                        this.dtsPreVendaTemporario.Tables["Peca_Digitada"].Rows.Add(dtrIncluirPecaDigitada);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preenchar_Grid_Itens_Venda(string strTipoVenda)
        {
            try
            {
                int intUltimaLinha = 0;

                this.objVendaItemOrc.DataContext = this.dtsGridVenda.Tables["Venda_It"];

                if (strTipoVenda == "Orcamento")
                {
                    DataRow dtrVendaIt = this.dtsGridVenda.Tables["Venda_It"].NewRow();

                    intUltimaLinha = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count - 1;

                    dtrVendaIt["Produto_Reciclavel"] = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Produto_Reciclavel"];
                    dtrVendaIt["Codigo"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Codigo"]);
                    dtrVendaIt["Tipo_Objeto"] = Convert.ToInt32(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Enum_Objeto_Tipo_ID"]);
                    dtrVendaIt["Descricao"] = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Descricao"].ToString().Trim();
                    dtrVendaIt["Imposto"] = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Imposto"];
                    dtrVendaIt["Qtde"] = Convert.ToInt32(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Orcamento_It_Qtde"]) * Convert.ToInt32(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Embalagem_Quantidade"]);
                    dtrVendaIt["Preco_Unitario"] = Convert.ToDecimal(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Orcamento_It_Preco_Pago"]);
                    dtrVendaIt["Total"] = Convert.ToDecimal(Convert.ToInt32(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Orcamento_It_Qtde"])
                                            * Convert.ToDecimal(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Orcamento_It_Preco_Pago"])
                                            * Convert.ToInt32(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Embalagem_Quantidade"]));

                    dtrVendaIt["Peca_Codigo_CFOP"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Codigo_CFOP"]);
                    dtrVendaIt["Class_Fiscal_ICMS"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Class_Fiscal_ICMS"]);
                    dtrVendaIt["Peca_Codigo_Situacao_Tributaria"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Codigo_Situacao_Tributaria"]);
                    dtrVendaIt["Peca_ICMS_Substituicao_Tributaria"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_ICMS_Substituicao_Tributaria"]);
                    dtrVendaIt["Peca_Percentual_Pis"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Percentual_Pis"]);
                    dtrVendaIt["Peca_Percentual_Cofins"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Percentual_Cofins"]);
                    dtrVendaIt["Peca_Codigo_Situacao_Tributaria_Pis"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Codigo_Situacao_Tributaria_Pis"]);
                    dtrVendaIt["Peca_Codigo_Situacao_Tributaria_Cofins"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Codigo_Situacao_Tributaria_Cofins"]);
                    dtrVendaIt["Class_Fiscal_CD"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Class_Fiscal_CD"]);
                    dtrVendaIt["Peca_Embalagem_Quantidade"] = Convert.ToInt32(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Embalagem_Quantidade"]);

                    dtrVendaIt["Peca_Origem_Mercadoria"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Peca_Origem_Mercadoria"]);
                    dtrVendaIt["Class_Fiscal_IPI"] = Convert.ToString(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Class_Fiscal_IPI"]);

                    dtrVendaIt["Desconto"] = 0;
                    dtrVendaIt["IsCupomFiscal"] = true;
                    dtrVendaIt["Cancelado"] = false;
                    dtrVendaIt["IsRomaneio"] = false;
                    dtrVendaIt["Cor"] = new SolidColorBrush(Colors.Black);

                    if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Enum_Objeto_Tipo_ID"] != null && (Enumerados.Tipo_Objeto)this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Enum_Objeto_Tipo_ID"] == Enumerados.Tipo_Objeto.Servico)
                    {
                        dtrVendaIt["Cor"] = new SolidColorBrush(Colors.Green);
                        dtrVendaIt["IsCupomFiscal"] = false;

                        dtrVendaIt["Item"] = this.intItemGridCupom + 1;

                    }
                    else
                    {
                        // Atualiza item digitado
                        this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Item_Digitado"] = this.blnItemDigitado;

                        dtrVendaIt["Item"] = this.intItemGridCupom + 1;

                    }

                    // Reseta marcação do item digitado
                    this.blnItemDigitado = false;

                    // Atualiza a tela
                    this.txtDescricaoProduto.Text = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Descricao"].ToString().Trim();
                    this.txtValorProduto.Text = "R$ " + Convert.ToDecimal(this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha]["Orcamento_It_Preco_Pago"]).ToString();

                    this.dtsGridVenda.Tables["Venda_It"].Rows.Add(dtrVendaIt);

                    this.intItemGridCupom += 1;

                    // Preenche o DataSet do Cupom Fiscal e imprime
                    if (Convert.ToDecimal(dtrVendaIt["Preco_Unitario"]) > 0 && Convert.ToBoolean(dtrVendaIt["IsCupomFiscal"]))
                    {
                        //this.intItemGridCupom += 1;
                        if (!this.Imprimir_Item_Orc_Cupom_Fiscal())
                        {
                            return false;
                        }

                        foreach (DataRow dtrItem in this.dtsGridVenda.Tables["Venda_It"].Select(" Item = " + this.intItemGridCupom))
                        {
                            dtrItem["Numero_Item_Cupom_Fiscal"] = this.objImpressaoFiscal.Retornar_Numero_Cupom_Fiscal();
                        }
                    }

                    this.produtoReciclavel = dtrVendaIt["Produto_Reciclavel"].DefaultBool();

                    this.dtsConsultaPecaItens.Clear();
                }
                else if (strTipoVenda == "Romaneio")
                {
                    this.txtMenu.Content = "Inserindo itens no cupom fiscal.";
                    Utilitario.Processar_Mensagens_Interface_WPF();

                    foreach (DataRow dtrRomaneioIT in this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select(" ItemImpresso = 0"))
                    {
                        if (dtrRomaneioIT["Peca_KIT_Compra_CT_ID"].DefaultInteger() != 0 && dtrRomaneioIT["Peca_Kit_Compra_IT_ID"].DefaultInteger() == 0)
                        {
                            continue;
                        }

                        if (dtrRomaneioIT["Peca_KIT_Compra_CT_ID"].DefaultInteger() == 0)
                        {
                            DataRow[] dtrItemDuplicado = this.dtsGridVenda.Tables["Venda_It"].Select("Romaneio_Pre_Venda_It_ID = " + Convert.ToString(dtrRomaneioIT["Romaneio_It_ID"].DefaultInteger()));

                            if (dtrItemDuplicado.Length > 0 && dtrRomaneioIT["Romaneio_It_ID"].DefaultInteger() != 0)
                            {
                                continue;
                            }
                        }

                        DataRow dtrVendaRomaneioIt = this.dtsGridVenda.Tables["Venda_It"].NewRow();

                        dtrVendaRomaneioIt["Romaneio_Pre_Venda_Ct_ID"] = dtrRomaneioIT["Romaneio_Pre_Venda_Ct_ID"].DefaultInteger();
                        dtrVendaRomaneioIt["Romaneio_Pre_Venda_It_ID"] = dtrRomaneioIT["Romaneio_It_ID"].DefaultInteger();
                        dtrVendaRomaneioIt["Codigo"] = dtrRomaneioIT["Codigo"].DefaultString();
                        dtrVendaRomaneioIt["Tipo_Objeto"] = dtrRomaneioIT["Enum_Objeto_Tipo_ID"];
                        dtrVendaRomaneioIt["Descricao"] = dtrRomaneioIT["Descricao"].ToString().Trim();
                        dtrVendaRomaneioIt["Imposto"] = dtrRomaneioIT["Imposto"];
                        dtrVendaRomaneioIt["Qtde"] = dtrRomaneioIT["Romaneio_It_Qtde"].DefaultInteger();
                        dtrVendaRomaneioIt["Preco_Unitario"] = dtrRomaneioIT["Romaneio_It_Preco_Pago"].DefaultDecimal();
                        dtrVendaRomaneioIt["Total"] = Convert.ToDecimal(dtrRomaneioIT["Romaneio_It_Qtde"].DefaultInteger() * dtrRomaneioIT["Romaneio_It_Preco_Pago"].DefaultDecimal());
                        dtrVendaRomaneioIt["Desconto"] = 0;
                        dtrVendaRomaneioIt["Cancelado"] = false;
                        dtrVendaRomaneioIt["IsRomaneio"] = true;

                        dtrVendaRomaneioIt["Peca_Codigo_CFOP"] = dtrRomaneioIT["Peca_Codigo_CFOP"].DefaultString();
                        dtrVendaRomaneioIt["Class_Fiscal_ICMS"] = dtrRomaneioIT["Class_Fiscal_ICMS"].DefaultString();
                        dtrVendaRomaneioIt["Peca_Codigo_Situacao_Tributaria"] = dtrRomaneioIT["Peca_Codigo_Situacao_Tributaria"].DefaultString();
                        dtrVendaRomaneioIt["Peca_ICMS_Substituicao_Tributaria"] = dtrRomaneioIT["Peca_ICMS_Substituicao_Tributaria"].DefaultString();
                        dtrVendaRomaneioIt["Peca_Percentual_Pis"] = dtrRomaneioIT["Peca_Percentual_Pis"].DefaultString();
                        dtrVendaRomaneioIt["Peca_Percentual_Cofins"] = dtrRomaneioIT["Peca_Percentual_Cofins"].DefaultString();
                        dtrVendaRomaneioIt["Peca_Codigo_Situacao_Tributaria_Pis"] = dtrRomaneioIT["Peca_Codigo_Situacao_Tributaria_Pis"].DefaultString();
                        dtrVendaRomaneioIt["Peca_Codigo_Situacao_Tributaria_Cofins"] = dtrRomaneioIT["Peca_Codigo_Situacao_Tributaria_Cofins"].DefaultString();
                        dtrVendaRomaneioIt["Class_Fiscal_CD"] = dtrRomaneioIT["Class_Fiscal_CD"].DefaultString();
                        dtrVendaRomaneioIt["Produto_Reciclavel"] = dtrRomaneioIT["Produto_Reciclavel"];

                        dtrVendaRomaneioIt["Peca_Origem_Mercadoria"] = dtrRomaneioIT["Peca_Origem_Mercadoria"].DefaultString();
                        dtrVendaRomaneioIt["Class_Fiscal_IPI"] = dtrRomaneioIT["Class_Fiscal_IPI"].DefaultString();

                        // Atualiza a linha inserida para que não seja incluido novamente
                        dtrRomaneioIT["ItemImpresso"] = true;


                        // Se romaneio do tipo resta ou troca, apresenta em VERMELHO
                        if ((TipoRomaneio)dtrRomaneioIT["Enum_Romaneio_Tipo_ID"] == TipoRomaneio.Resta
                            || (TipoRomaneio)dtrRomaneioIT["Enum_Romaneio_Tipo_ID"] == TipoRomaneio.Troca)
                        {
                            dtrVendaRomaneioIt["Cor"] = new SolidColorBrush(Colors.Red);
                            dtrVendaRomaneioIt["IsCupomFiscal"] = false;
                            if ((Enumerados.TipoRomaneio)dtrRomaneioIT["Enum_Romaneio_Tipo_ID"] == TipoRomaneio.Resta)
                            {
                                dtrRomaneioIT.Delete();
                            }

                        }
                        // Itens de serviço são apresentados em VERDE
                        else if (dtrRomaneioIT["Enum_Objeto_Tipo_ID"] != null && (Tipo_Objeto)dtrRomaneioIT["Enum_Objeto_Tipo_ID"] == Tipo_Objeto.Servico)
                        {
                            dtrVendaRomaneioIt["Item"] = this.intItemGridCupom + 1;
                            dtrVendaRomaneioIt["Cor"] = new SolidColorBrush(Colors.Green);
                            dtrVendaRomaneioIt["IsCupomFiscal"] = false;
                        }
                        else
                        {
                            dtrVendaRomaneioIt["Item"] = this.intItemGridCupom + 1;
                            dtrVendaRomaneioIt["Cor"] = new SolidColorBrush(Colors.Black);
                            dtrVendaRomaneioIt["IsCupomFiscal"] = this.blnRomaneioEspecial ? false : true;
                        }

                        this.intItemGridCupom += 1;

                        this.dtsGridVenda.Tables["Venda_It"].Rows.Add(dtrVendaRomaneioIt);

                        // Preenche o DataSet do Cupom Fiscal e imprime
                        if (Convert.ToDecimal(dtrVendaRomaneioIt["Preco_Unitario"]) > 0 && Convert.ToBoolean(dtrVendaRomaneioIt["IsCupomFiscal"]))
                        {
                            // Atualiza a tela
                            this.txtDescricaoProduto.Text = dtrRomaneioIT["Descricao"].ToString().Trim();
                            this.txtValorProduto.Text = "R$ " + Convert.ToDecimal(dtrRomaneioIT["Romaneio_It_Preco_Pago"]).ToString();

                            //this.intItemGridCupom += 1;
                            if (!this.Imprimir_Item_Orc_Cupom_Fiscal())
                            {
                                return false;
                            }
                        }

                        if (Convert.ToDecimal(dtrVendaRomaneioIt["Preco_Unitario"]) > 0)
                        {
                            if (dtrRomaneioIT["Peca_KIT_Compra_CT_ID"].DefaultInteger() != 0 && dtrRomaneioIT["Peca_Kit_Compra_IT_ID"].DefaultInteger() != 0)
                            {
                                dtrRomaneioIT.Delete();
                                this.dtsRomaneioTemporario.Tables["Romaneio_It"].AcceptChanges();
                            }
                        }

                    }
                }
                if (this.objVendaItemOrc.Items.Count > 0)
                {
                    this.objVendaItemOrc.ScrollIntoView(this.objVendaItemOrc.Items[this.objVendaItemOrc.Items.Count - 1]);
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Ct_Romaneio()
        {
            try
            {
                // Incluir no datatable os romaneios escolhidos
                if (this.dtsPreVendaTemporario.Tables.Count > 0)
                {
                    foreach (DataRow dtrPreVenda in this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows)
                    {
                        DataRow dtrIncluir = this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].NewRow();

                        dtrIncluir["Comanda_Interna_ID"] = dtrPreVenda["Comanda_Interna_ID"];
                        dtrIncluir["Comanda_Externa_ID"] = dtrPreVenda["Comanda_Externa_ID"];
                        dtrIncluir["Romaneio_Ct_ID"] = Convert.ToInt32(dtrPreVenda["Romaneio_Pre_Venda_Ct_ID"]) == 0 ? this.intOrcamentoCtID : dtrPreVenda["Romaneio_Pre_Venda_Ct_ID"];
                        dtrIncluir["Lojas_ID"] = dtrPreVenda["Lojas_ID"];
                        dtrIncluir["Pessoa_Autorizada_ID"] = dtrPreVenda["Pessoa_Autorizada_ID"];
                        dtrIncluir["Enum_Romaneio_Tipo_ID"] = dtrPreVenda["Enum_Romaneio_Tipo_ID"];
                        dtrIncluir["Enum_Romaneio_Status_ID"] = (Int32)StatusRomaneioVenda.Liberado;
                        dtrIncluir["Condicao_Pagamento_ID"] = dtrPreVenda["Condicao_Pagamento_ID"];
                        dtrIncluir["Usuario_Vendedor_ID"] = dtrPreVenda["Usuario_Vendedor_ID"];
                        dtrIncluir["Usuario_Gerente_ID"] = dtrPreVenda["Usuario_Gerente_ID"];
                        dtrIncluir["Romaneio_Grupo_ID"] = 0;
                        dtrIncluir["Romaneio_Pre_Venda_Ct_ID"] = Convert.ToInt32(dtrPreVenda["Romaneio_Pre_Venda_Ct_ID"]) == 0 ? this.intOrcamentoCtID : dtrPreVenda["Romaneio_Pre_Venda_Ct_ID"];
                        dtrIncluir["Romaneio_Ct_Data_Geracao"] = dtrPreVenda["Romaneio_Pre_Venda_Ct_Data_Geracao"];
                        dtrIncluir["Romaneio_CT_Valor_Total_Pago"] = dtrPreVenda["Romaneio_Pre_Venda_CT_Valor_Total_Pago"];
                        dtrIncluir["Romaneio_CT_Valor_Total_Lista"] = dtrPreVenda["Romaneio_Pre_Venda_CT_Valor_Total_Lista"];
                        dtrIncluir["Romaneio_Ct_Valor_Real"] = 0.0;

                        dtrIncluir["Romaneio_Grupo_Origem_Resta_ID"] = dtrPreVenda["Romaneio_Grupo_Origem_Resta_ID"];
                        dtrIncluir["Loja_Origem_Resta_ID"] = dtrPreVenda["Loja_Origem_Resta_ID"];

                        dtrIncluir["Cliente_ID"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strClienteNotaFiscalPaulistaID : this.strClienteID;
                        dtrIncluir["Romaneio_Ct_Cliente_CNPJCPF"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strCpfCnpjNotaFiscalPaulista : this.strCpfCnpj;
                        dtrIncluir["Romaneio_Ct_Cliente_Nome"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.txtNomeCliente.Text : this.strClienteNome;

                        dtrIncluir["Romaneio_Ct_Cliente_Telefone"] = dtrPreVenda["Romaneio_Pre_Venda_Ct_Cliente_Telefone"];
                        dtrIncluir["Romaneio_Pre_Venda_Ct_Apresentou_NF"] = dtrPreVenda["Romaneio_Pre_Venda_Ct_Apresentou_NF"];

                        this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows.Add(dtrIncluir);
                    }
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_It_Romaneio()
        {
            try
            {
                // incluir no datatable os itens dos romaneios escolhidos para serem liberados na venda
                foreach (DataRow dtrPreVenda in this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_It"].Rows)
                {
                    DataRow dtrIncluir = this.dtsRomaneioTemporario.Tables["Romaneio_It"].NewRow();

                    dtrIncluir["Romaneio_It_ID"] = 0;
                    dtrIncluir["Romaneio_Ct_ID"] = Convert.ToInt32(dtrPreVenda["Romaneio_Pre_Venda_Ct_ID"]) == 0 ? this.intOrcamentoCtID : dtrPreVenda["Romaneio_Pre_Venda_Ct_ID"];
                    dtrIncluir["Lojas_ID"] = dtrPreVenda["Lojas_ID"];
                    dtrIncluir["Objeto_ID"] = dtrPreVenda["Objeto_ID"];
                    dtrIncluir["Enum_Objeto_Tipo_ID"] = dtrPreVenda["Enum_Objeto_Tipo_ID"];
                    dtrIncluir["Peca_Kit_Ct_ID"] = 0;
                    dtrIncluir["Romaneio_It_Sequencial"] = dtrPreVenda["Romaneio_Pre_Venda_It_Sequencial"];
                    dtrIncluir["Romaneio_It_Qtde"] = dtrPreVenda["Romaneio_Pre_Venda_It_Qtde"];
                    dtrIncluir["Romaneio_It_Preco_Pago"] = dtrPreVenda["Romaneio_Pre_Venda_It_Preco_Pago"];
                    dtrIncluir["Romaneio_It_Preco_Lista"] = dtrPreVenda["Romaneio_Pre_Venda_It_Preco_Lista"];
                    dtrIncluir["Romaneio_It_Valor_Desconto"] = dtrPreVenda["Romaneio_Pre_Venda_It_Valor_Desconto"];
                    dtrIncluir["Romaneio_It_Valor_Comissao"] = dtrPreVenda["Romaneio_Pre_Venda_It_Valor_Comissao"];
                    dtrIncluir["Romaneio_It_Custo_Reposicao"] = dtrPreVenda["Romaneio_Pre_Venda_It_Custo_Reposicao"];
                    dtrIncluir["Romaneio_It_Custo_Reposicao_Efetivo"] = dtrPreVenda["Romaneio_Pre_Venda_It_Custo_Reposicao_Efetivo"];
                    dtrIncluir["Romaneio_It_Custo_Medio"] = dtrPreVenda["Romaneio_Pre_Venda_It_Custo_Medio"];
                    dtrIncluir["Romaneio_It_Custo_Unitario"] = dtrPreVenda["Romaneio_Pre_Venda_It_Custo_Unitario"];
                    dtrIncluir["Romaneio_It_Instalado"] = 0;
                    dtrIncluir["Item_Digitado"] = dtrPreVenda["Item_Digitado"];
                    dtrIncluir["Desconto_Caixa"] = dtrPreVenda["Desconto_Caixa"];
                    dtrIncluir["Usuario_Aprovacao_Desconto_ID"] = dtrPreVenda["Usuario_Aprovacao_Desconto_ID"];

                    dtrIncluir["Qtde_Reciclavel"] = dtrPreVenda["Qtde_Reciclavel"];
                    dtrIncluir["Produto_Reciclavel"] = dtrPreVenda["Produto_Reciclavel"];
                    dtrIncluir["Acordo_Realizado"] = dtrPreVenda["Acordo_Realizado"];

                    this.dtsRomaneioTemporario.Tables["Romaneio_It"].Rows.Add(dtrIncluir);
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Documento_Romameio()
        {
            try
            {
                if (this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0)
                {
                    // incluir no datatable os documentos do romaneio gerado do Auto Serviço
                    DataRow dtrIncluirPreVendaCt = this.dtsPreVendaTemporario.Tables["Romaneio_Documento"].NewRow();

                    dtrIncluirPreVendaCt["Lojas_ID"] = this.intLojaID;
                    dtrIncluirPreVendaCt["Enum_Documento_Tipo_ID"] = this.intTipoDocumento;
                    dtrIncluirPreVendaCt["Enum_Documento_Status_ID"] = (Int32)StatusRomaneioVenda.Liberado;
                    dtrIncluirPreVendaCt["Usuario_Emissao_ID"] = this.intUsuario;
                    dtrIncluirPreVendaCt["Romaneio_Grupo_ID"] = 0;
                    dtrIncluirPreVendaCt["Romaneio_Pre_Venda_Ct_ID"] = this.intOrcamentoCtID;
                    dtrIncluirPreVendaCt["Romaneio_Documento_Numero"] = string.Empty;
                    dtrIncluirPreVendaCt["Romaneio_Documento_Data_Emissao"] = this.dtmDataLiberacao;
                    dtrIncluirPreVendaCt["Romaneio_Documento_Data_Movimento"] = this.dtmDataMovimento;

                    this.dtsPreVendaTemporario.Tables["Romaneio_Documento"].Rows.Add(dtrIncluirPreVendaCt);
                }
                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Grupo_Romaneio()
        {
            try
            {

                DataRow dtrIncluir = this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].NewRow();

                dtrIncluir["Lojas_ID"] = this.intLojaID;
                dtrIncluir["Usuario_Caixa_ID"] = this.dtoUsuario.ID;
                dtrIncluir["Enum_Grupo_Status_ID"] = (Int32)StatusRomaneioVenda.Liberado;
                dtrIncluir["Cliente_ID"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strClienteNotaFiscalPaulistaID : this.strClienteID;
                dtrIncluir["Condicao_Pagamento_ID"] = this.intCondicaoPagtoID;
                dtrIncluir["Romaneio_Grupo_Data_Liberacao"] = this.dtmDataLiberacao;
                dtrIncluir["Romaneio_Grupo_Cliente_CNPJCPF"] = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strCpfCnpjNotaFiscalPaulista : this.strCpfCnpj;

                this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows.Add(dtrIncluir);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_DataRow_Documento()
        {
            try
            {
                if (this.dtsRomaneioTemporario.Tables.Count > 0)
                {
                    // incluir no datatable os documentos dos romaneios escolhidos para serem liberados na venda
                    foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows)
                    {
                        DataRow dtrIncluir = this.dtsPreVendaTemporario.Tables["Romaneio_Documento"].NewRow();

                        dtrIncluir["Lojas_ID"] = dtrRomaneio["Lojas_ID"];
                        dtrIncluir["Enum_Documento_Tipo_ID"] = this.intTipoDocumento;
                        dtrIncluir["Enum_Documento_Status_ID"] = (Int32)StatusRomaneioVenda.Liberado;
                        dtrIncluir["Usuario_Emissao_ID"] = this.intUsuario;
                        dtrIncluir["Romaneio_Grupo_ID"] = 0;
                        dtrIncluir["Romaneio_Pre_Venda_Ct_ID"] = dtrRomaneio["Romaneio_Pre_Venda_Ct_ID"];
                        dtrIncluir["Romaneio_Documento_Numero"] = string.Empty;
                        dtrIncluir["Romaneio_Documento_Data_Emissao"] = this.dtmDataLiberacao;
                        dtrIncluir["Romaneio_Documento_Data_Movimento"] = this.dtmDataMovimento;

                        this.dtsPreVendaTemporario.Tables["Romaneio_Documento"].Rows.Add(dtrIncluir);
                    }
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }
        
        private int Operadora_Cartao_Regra_Parcelamento(int pagamentoItemId)
        {
            int enumRegraRedundanciaID;
            int enumRegraParcelamentoID;
            DataRow[] dtrDadosCartao = this.dttDadosCartao.Select("Pagamento_Item=" + pagamentoItemId.DefaultString());
            CaixaBUS.Operadora_Cartao_Regras(out enumRegraParcelamentoID, out enumRegraRedundanciaID, dtrDadosCartao[0]["Cartao_TEF_ID"].DefaultInteger(), dtrDadosCartao[0]["Operadora_Cartao_ID"].DefaultInteger(), this.dttOperadoraCartaoRegra);

            return enumRegraParcelamentoID;
        }

        private int Operadora_Cartao_Regra_Redundancia(int pagamentoItemId)
        {
            int enumRegraParcelamentoID;
            int enumRegraRedundanciaID;
            DataRow[] dtrDadosCartao = this.dttDadosCartao.Select("Pagamento_Item=" + pagamentoItemId.DefaultString());
            CaixaBUS.Operadora_Cartao_Regras(out enumRegraParcelamentoID, out enumRegraRedundanciaID, dtrDadosCartao[0]["Cartao_TEF_ID"].DefaultInteger(), dtrDadosCartao[0]["Operadora_Cartao_ID"].DefaultInteger(), this.dttOperadoraCartaoRegra);

            return enumRegraRedundanciaID;
        }

        private bool Preencher_DataRow_Pagamento_Venda_Liberada()
        {
            try
            {
                // Incluir no datatable as informações de pagamento

                if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count > 0)
                {
                    // Limpar informações já criadas
                    this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Clear();

                    CaixaBUS busCaixa = new CaixaBUS();

                    foreach (DataRow dtrPagamentos in this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                    {
                        // Montar grid com informações de pagamento
                        foreach (DataRow dtr in this.dtsFormaXCliente.Tables["Prazo_Pagamento_Cliente_Tipo"].Select("Condicao_Pagamento_ID = " + Convert.ToString(dtrPagamentos["Condicao_Pagamento_ID"])))
                        {
                            DataRow dtrIncluir = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].NewRow();
                            dtrIncluir["Romaneio_Grupo_ID"] = 0;
                            dtrIncluir["Lojas_ID"] = this.intLojaID;
                            dtrIncluir["Item"] = dtrPagamentos["Item"];
                            dtrIncluir["Condicao_Pagamento_ID"] = dtrPagamentos["Condicao_Pagamento_ID"];
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Valor_Informado"] = Convert.ToInt32(dtrPagamentos["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO ? Convert.ToDecimal(dtrPagamentos["Valor_Informado"].ToDecimal() - dtrPagamentos["Troco"].ToDecimal()) : Convert.ToDecimal(dtrPagamentos["Valor_Informado"]);
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Data"] = busCaixa.Retornar_Data_Vencimento_Dia_Util(((DateTime)DateTime.Today.AddDays(Convert.ToInt32(dtr["Prazo_Pagamento_Parcela_Dias"]))), this.intLojaID).ToString("dd/MM/yyyy");
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Dia_Parcela"] = dtr["Prazo_Pagamento_Parcela_Dias"];
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Valor"] = Convert.ToInt32(dtrPagamentos["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO ? Convert.ToDecimal(dtrPagamentos["Valor_Parcela"].ToDecimal() - dtrPagamentos["Troco"].ToDecimal()) : ((Convert.ToDecimal(dtrPagamentos["Valor_Informado"]) * Convert.ToDecimal(dtr["Prazo_Pagamento_Parcela_Percentual"])) / 100).ToDecimalRound(2);
                            dtrIncluir["Condicao_Pagamento_Nome"] = dtrPagamentos["Condicao_Pagamento_Nome"];
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas"] = dtrPagamentos["Numero_de_Parcelas"];
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito"] = dtrPagamentos["Cartao_Debito"];
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito"] = dtrPagamentos["Cartao_Credito"];
                            dtrIncluir["Romaneio_Pagamento_Venda_Liberada_Emite_Cheque"] = dtrPagamentos["Cheque"];
                            dtrIncluir["OperadoraCartao"] = string.Empty;
                            dtrIncluir["Prazo_Pagamento_Parcela_Percentual"] = dtr["Prazo_Pagamento_Parcela_Percentual"].DefaultDecimal();

                            this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Add(dtrIncluir);
                        }
                    }

                    // Realizar a reduncancia para os valores das parcelas
                    foreach (DataRow dtrPagamentos in this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString()))
                    {
                        decimal dcmTotalPagamento = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Compute("Sum(Romaneio_Pagamento_Venda_Liberada_Valor)", "Condicao_Pagamento_ID = " + Convert.ToString(dtrPagamentos["Condicao_Pagamento_ID"] + "AND Item = " + dtrPagamentos["Item"])).ToDecimal();

                        if (Convert.ToDecimal(dtrPagamentos["Valor_Informado"]) != dcmTotalPagamento)
                        {
                            DataRow[] dtrRomaneioPagamentoVendaLiberada = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Condicao_Pagamento_ID = " + Convert.ToString(dtrPagamentos["Condicao_Pagamento_ID"]) + "AND Item = " + dtrPagamentos["Item"]);
                            decimal dcmDiferenca = Convert.ToDecimal(dtrPagamentos["Valor_Informado"]) - dcmTotalPagamento;
                            dtrRomaneioPagamentoVendaLiberada[dtrRomaneioPagamentoVendaLiberada.Length - 1]["Romaneio_Pagamento_Venda_Liberada_Valor"] = dtrRomaneioPagamentoVendaLiberada[dtrRomaneioPagamentoVendaLiberada.Length - 1]["Romaneio_Pagamento_Venda_Liberada_Valor"].ToDecimal() + dcmDiferenca;
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

        private void Preencher_DataObject_Sat_Venda(string strXML)
        {
            try
            {
                this.dtoCaixaSatVenda.SAT_ID = 0;
                this.dtoCaixaSatVenda.Lojas_ID = this.intLojaID;
                this.dtoCaixaSatVenda.XML = strXML;
                this.dtoCaixaSatVenda.Enum_Status_ID = Caixa_Sat_Venda_Status.Gerado.DefaultInteger();
                this.dtoCaixaSatVenda.Enum_Status_Transmissao_Sefaz_ID = Sat_Status_Transmissao_Sefaz.Nao_Transmitido.DefaultInteger();
                this.dtoCaixaSatVenda.Usuario_Criacao_ID = this.intUsuario;
                DBUtil objUtil = new DBUtil();
                this.dtoCaixaSatVenda.Data_Geracao = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region "   Liberação           "

        private bool Liberar_Venda_Com_Sitef(DataRow dtrFormaPagamento)
        {
            try
            {

                // =======================================================================
                this.Criar_Log_Processo("INICIO DO PROCESSO DE VENDA SITEF");
                // =======================================================================

                SitefDO dtoSitef = new SitefDO(ref this.objImpressaoFiscal, this.objComunicacaoImpressoraFiscal, this.objTipoImpressoraFiscal);

                // =======================================================================
                this.Criar_Log_Processo("Encerrar_Transacao_Pendente");
                // =======================================================================

                dtoSitef.Encerrar_Transacao_Pendente();

                // =======================================================================
                this.Criar_Log_Processo("Configura_Sitef");
                // =======================================================================

                dtoSitef.Configura_Sitef(false);

                this.strCaixa = "C";

                // =======================================================================
                this.Criar_Log_Processo("Preencher SITEF");
                // =======================================================================

                this.Preencher_Sitef(ref dtoSitef, dtrFormaPagamento);

                // =======================================================================
                this.Criar_Log_Processo("Gerar Cupom Fiscal");
                // =======================================================================

                this.Finalizar_Cupom_Fiscal();

                if (!this.blnCupomAberto)
                {
                    dtoSitef.Imprime_Somente_Romaneio = true;
                }

                frmSitef_Menu_Principal frmSitefMenuPrincipal = new frmSitef_Menu_Principal((Int32)Acoes_Sitef.Seleciona_Forma_Pagamento, ref dtoSitef, this.dttCupomFiscal, this.dttCupomFiscalFechamento, this.dcmDescontoComercial, this.dcmDescontoItemTotal, MENSAGEM_ORIGEM, this.dttOperadoraCartao);

                // =======================================================================
                this.Criar_Log_Processo("Iniciei processo com SITEF");
                // =======================================================================

                if (frmSitefMenuPrincipal.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // =======================================================================
                    this.Criar_Log_Processo("Passei pelo sitef com sucesso");
                    // =======================================================================

                    this.strCOO = dtoSitef.Codigo_COO;

                    if (dtoSitef.Codigo_COO != frmSitefMenuPrincipal.Numero_COO)
                    {
                        this.strCOO = frmSitefMenuPrincipal.Numero_COO;
                    }

                    // =======================================================================
                    this.Criar_Log_Processo("Codigo da transação" + this.strCOO);
                    // =======================================================================

                    this.dttDadosCartao.Clear();

                    int intQtdeTransacoesSitef = 0;

                    int intFormaPagtoID = 0;
                    int intCartaoTEFID = 0;
                    string strOperadora = string.Empty;
                    string strBandeira = string.Empty;
                    // Gravar informações do sitef - cartão
                    foreach (Sitef_TransacaoDO objTransacao in dtoSitef.Lista_Transacoes)
                    {
                        // =======================================================================
                        this.Criar_Log_Processo(string.Format("Inicia Gravação das informações da transação SITEF - {0}/{1}", ++intQtdeTransacoesSitef, dtoSitef.Lista_Transacoes.Count));
                        // =======================================================================

                        // =======================================================================
                        this.Criar_Log_Processo(string.Format("Forma de pagamento - {0}", objTransacao.Codigo_Modalidade_Pagamento.ToInteger()));
                        // =======================================================================

                        // Só registra pagamentos em cartão.
                        if (objTransacao.Codigo_Modalidade_Pagamento == Modalidade_Pagamento_Sitef.Nao_Tef.ToInteger()
                            || objTransacao.Codigo_Modalidade_Pagamento == Modalidade_Pagamento_Sitef.Cheque.ToInteger())
                        {
                            continue;
                        }

                        strBandeira = string.Empty;
                        strBandeira = Strings.Mid(objTransacao.Codigo_Administradora, 1, 5);

                        // =======================================================================
                        this.Criar_Log_Processo(string.Format("Bandeira - {0}", strBandeira));
                        // =======================================================================

                        strOperadora = string.Empty;
                        strOperadora = Strings.Mid(objTransacao.Instituicao, 1, 5);

                        // =======================================================================
                        this.Criar_Log_Processo(string.Format("Operadora - {0}", strOperadora));
                        // =======================================================================

                        DataRow dtrDetCartao = this.dttDadosCartao.NewRow();
                        dtrDetCartao["Pagamento_Item"] = objTransacao.Pagamento_Item;
                        dtrDetCartao["Romaneio_Detalhe_Cartao_ID"] = string.Empty;
                        dtrDetCartao["Lojas_ID"] = this.intLojaID;
                        dtrDetCartao["Romaneio_Grupo_ID"] = 0;
                        dtrDetCartao["Enum_Tipo_Juros_ID"] = (Int32)TipoJurosCartao.Loja;

                        DataRow[] dtrFormaPagto;
                        // Identificar qual é a forma de pagamento do SIM correspondente com a forma de pagamento do Sitef
                        if (objTransacao.Codigo_Modalidade_Pagamento == (Int32)Modalidade_Pagamento_Sitef.Cartao_Credito)
                        {
                            dtrFormaPagto = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select(" Forma_Pagamento_Emissao_Cartao_Credito = 1");
                        }
                        else
                        {
                            dtrFormaPagto = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select(" Forma_Pagamento_Emissao_Cartao_Debito = 1");
                        }

                        intFormaPagtoID = Convert.ToInt32(dtrFormaPagto[0]["Forma_Pagamento_ID"]);

                        intCartaoTEFID = 0;
                        if (!string.IsNullOrEmpty(strBandeira))
                        {
                            foreach (DataRow dtrCartao in this.dtsCadastroCartao.Tables["Cartao_TEF"].Select(" Cartao_TEF_Cd = '" + strBandeira + "' And Forma_Pagamento_ID = " + Convert.ToString(intFormaPagtoID)))
                            {
                                intCartaoTEFID = Convert.ToInt32(dtrCartao["Cartao_TEF_ID"]);
                                break;
                            }
                        }

                        // Administradora não cadastrada - forçar código de outras
                        if (intCartaoTEFID == 0)
                        {
                            foreach (DataRow dtrCartao in this.dtsCadastroCartao.Tables["Cartao_TEF"].Select(" Cartao_TEF_Cd = '00000' And Forma_Pagamento_ID = " + Convert.ToString(intFormaPagtoID)))
                            {
                                intCartaoTEFID = Convert.ToInt32(dtrCartao["Cartao_TEF_ID"]);
                                break;
                            }

                        }

                        DataRow[] dtrCondicaoPagto;
                        // Buscar a condicao_pagamento_id correspondente do cartão
                        dtrCondicaoPagto = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Consumidor_Final"].Select(" Forma_Pagamento_ID = " + Convert.ToString(intFormaPagtoID) + " AND Nr_Parcelas = " + Convert.ToString(objTransacao.Numero_Parcelas));
                        // Sempre vai ter porque é SITEF, mas para garantir...
                        if (dtrCondicaoPagto.Length == 0)
                        {
                            dtrDetCartao["Condicao_Pagamento_ID"] = 0;
                        }
                        else
                        {
                            dtrDetCartao["Condicao_Pagamento_ID"] = dtrCondicaoPagto[0]["Condicao_Pagamento_ID"];
                        }

                        dtrDetCartao["Cartao_TEF_ID"] = intCartaoTEFID;
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Valor"] = objTransacao.Valor_Transacao;
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Numero_Parcela"] = objTransacao.Numero_Parcelas;
                        string strDataTransacao = string.Empty;
                        strDataTransacao = Strings.Mid(objTransacao.DataHora_Transacao, 1, 4)
                                                        + "-"
                                                        + Strings.Mid(objTransacao.DataHora_Transacao, 5, 2)
                                                        + "-"
                                                        + Strings.Mid(objTransacao.DataHora_Transacao, 7, 2)
                                                        + " "
                                                        + Strings.Mid(objTransacao.DataHora_Transacao, 9, 2)
                                                        + ":"
                                                        + Strings.Mid(objTransacao.DataHora_Transacao, 11, 2)
                                                        + ":"
                                                        + Strings.Mid(objTransacao.DataHora_Transacao, 13, 2);
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Data_Transacao"] = strDataTransacao;
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Estorno"] = 0;
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Sitef"] = 1;
                        string[] strCartaoNumeroAutorizacao;
                        strCartaoNumeroAutorizacao = objTransacao.Codigo_Autorizacao.Split(Convert.ToChar("\n"));
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Numero_Autorizacao"] = strCartaoNumeroAutorizacao[0];
                        dtrDetCartao["Romaneio_Detalhe_Cartao_Numero_Autorizacao"] = Strings.Mid(objTransacao.Codigo_Autorizacao, 1, 30);
                        dtrDetCartao["Romaneio_Detalhe_Cartao_NSU_Sitef"] = Strings.Mid(objTransacao.Codigo_NSU_Sitef, 1, 30);
                        dtrDetCartao["Romaneio_Detalhe_Cartao_NSU_Autorizador"] = Strings.Mid(objTransacao.Codigo_NSU_Autorizador, 1, 30);
                        dtrDetCartao["Operadora_Cartao_Codigo_Sitef"] = strOperadora;
                        this.dttDadosCartao.Rows.Add(dtrDetCartao);

                    }

                    // =======================================================================
                    this.Criar_Log_Processo("Retorna Impressora Cupom");
                    // =======================================================================

                    this.strECF = this.Retornar_Numero_Caixa();

                    // =======================================================================
                    this.Criar_Log_Processo("Retornar_Impressora() = " + this.strECF);
                    // =======================================================================

                    List<Sitef_ComprovantesPagamentoDO> colComprovantesDePagamento = frmSitefMenuPrincipal.Lista_Comprovantes_De_Pagamento;

                    frmSitefMenuPrincipal.Dispose();

                    // =======================================================================
                    this.Criar_Log_Processo("Tela Sitef Dispose");
                    // =======================================================================

                    this.Alterar_Data_Vencimento_Cartao_Credito();

                    // =======================================================================
                    this.Criar_Log_Processo("Atualizar Data Vencimento do Cartão");
                    // =======================================================================

                    this.Alterar_Parcela_Redundancia_Cartao_Credito();

                    // =======================================================================
                    this.Criar_Log_Processo("Atualizar Valor Parcela(Redundancia) do Cartão");
                    // =======================================================================

                    if (this.Inicializar_Venda_Sat() == false)
                    {
                        return false;
                    }

                    // =======================================================================
                    this.Criar_Log_Processo("Inicializa Venda SAT");
                    // =======================================================================

                    if (this.Processar_Venda_Sat() == false)
                    {
                        this.txtMenu.Content = MENSAGEM_OPERACAO_SAT_VENDA_COM_FALHA;
                        return false;
                    }

                    // =======================================================================
                    this.Criar_Log_Processo("Processar Venda SAT");
                    // =======================================================================

                    if (this.Confirmar_Venda(true))
                    {
                        // =======================================================================
                        this.Criar_Log_Processo("Venda confirmada");
                        // =======================================================================

                        // =======================================================================
                        this.Criar_Log_Processo("Iniciarei impressao do comprovante de pagamento");
                        // =======================================================================

                        if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT && colComprovantesDePagamento.Count > 0)
                        {
                            Impressao_Romaneio objImpRomaneio = new Impressao_Romaneio();

                            foreach (Sitef_ComprovantesPagamentoDO ComprovantesDePagamento in colComprovantesDePagamento)
                            {
                                if (!ComprovantesDePagamento.Via_do_Cliente)
                                {
                                    objImpRomaneio.Imprimir_Comprovante_Nao_Fiscal(ComprovantesDePagamento.Comprovante_Pagamento, string.Empty, false, true);
                                }
                            }
                        }

                        // =======================================================================
                        this.Criar_Log_Processo("Iniciarei impressao do comprovante do pacote");
                        // =======================================================================
                        this.Processar_Impressao_Comprovante_Pacote_e_Servico();

                        // =======================================================================
                        this.Criar_Log_Processo("Passei pela impressao do comprovante do pacote");

                        // =======================================================================
                        this.Criar_Log_Processo("Iniciarei impressao do cupom SAT");
                        // =======================================================================

                        this.Imprimir_Comprovante_Sat(false);

                        if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT && colComprovantesDePagamento.Count > 0)
                        {
                            Impressao_Romaneio objImpRomaneio = new Impressao_Romaneio();

                            RawPrinterHelper.SendStringToPrinter(RawPrinterHelper.GetDefaultSystemPrinter(), string.Concat("------------------------------------------------", "\r", "\n", "\xA\xA"));

                            foreach (Sitef_ComprovantesPagamentoDO ComprovantesDePagamento in colComprovantesDePagamento)
                            {
                                if (ComprovantesDePagamento.Via_do_Cliente) // Cliente
                                {
                                    objImpRomaneio.Imprimir_Comprovante_Nao_Fiscal(ComprovantesDePagamento.Comprovante_Pagamento, string.Empty, false, false);
                                }
                            }

                            RawPrinterHelper.SendStringToPrinter(RawPrinterHelper.GetDefaultSystemPrinter(), string.Concat("\xA\xA", "------------------------------------------------", "\r", "\n", "\xA\xA\xA\xA"));
                        }

                        // =======================================================================
                        this.Criar_Log_Processo("Imprimir o procedimento de Garantia das peças");
                        //===========================================================================

                        this.Processar_Impressao_Procedimento_Garantia(this.blnParametroImprimeProcedimentoGarantia,
                            this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows[0]["Romaneio_Grupo_ID"].ToInteger());

                        // =======================================================================
                        this.Criar_Log_Processo("Passei pela impressao do procedimento de garanatia e vou imprimir o resta");
                        // =======================================================================
                        this.Processar_Impressao_Comprovante_Resta();

                        //===============================================================================
                        this.Criar_Log_Processo("Iniciar uma nova venda");
                        // =======================================================================
                        this.Inicializar_Nova_Venda();
                    }

                    // ===========================================
                    this.Criar_Log_Processo("Confirmar venda");
                    // ===========================================
                }
                else
                {
                    this.enuSituacao = Operacao.Pendente_POS;
                    this.txtMenu.Content = "Falha Sitef. Processar no POS? (S\\N)";
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);

                    return false;
                }
                // =======================================================================
                this.Criar_Log_Processo("Sitef DialogResult = " + frmSitefMenuPrincipal.DialogResult.ToString());
                this.Criar_Log_Processo("--------------------------------------------------------------------------");
                // =======================================================================
                return true;
            }
            catch (Exception ex)
            {

                // =======================================================================
                this.Criar_Log_Processo(string.Format("Erro Message: {0} - \n\n Stack Trace : {1} ", ex.Message, ex.StackTrace));
                // =======================================================================

                throw;
            }
        }

        private void Alterar_Parcela_Redundancia_Cartao_Credito()
        {
            try
            {
                if (this.dttDadosCartao.Rows.Count <= 0)
                {
                    return;
                }

                this.dttOperadoraCartaoRegra = new Operadora_Cartao_RegrasBUS().Consultar_DataTable(TipoServidor.LojaAtual);

                foreach (DataRow dtrNovoPagamento in this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select(string.Empty, string.Empty, DataViewRowState.Added))
                {
                    if (dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito"].DefaultBool())
                    {
                        DataRow[] dtrDadosCartaos = this.dttDadosCartao.Select("Pagamento_Item = " + dtrNovoPagamento["Item"].DefaultInteger());

                        if (dtrDadosCartaos[0]["Operadora_Cartao_ID"].DefaultString() != string.Empty && dtrDadosCartaos[0]["Cartao_TEF_ID"].DefaultInteger() != 0)
                        {
                            int intPagamentoItemId = dtrNovoPagamento["Item"].DefaultInteger();
                            dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Valor"] = CaixaBUS.Calcular_Parcela(dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Valor_Informado"].DefaultDecimal(), 
                                                                                                                                      dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas"].DefaultInteger());
                        }
                    }
                }

                foreach (DataRow dtrPagamentos in this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Forma_Pagamento_ID = " + ((int)Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO).ToString()))
                {
                    decimal dcmTotalPagamento = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Compute("Sum(Romaneio_Pagamento_Venda_Liberada_Valor)", "Condicao_Pagamento_ID = " + dtrPagamentos["Condicao_Pagamento_ID"].DefaultString() + "AND Item = " + dtrPagamentos["Item"].DefaultString()).ToDecimal();

                    if (dtrPagamentos["Valor_Informado"].DefaultDecimal() != dcmTotalPagamento)
                    {
                        DataRow[] dtrRomaneioPagamentoVendaLiberada = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Condicao_Pagamento_ID = " + dtrPagamentos["Condicao_Pagamento_ID"].DefaultString() + "AND Item = " + dtrPagamentos["Item"].DefaultString());
                        decimal dcmDiferenca = dtrPagamentos["Valor_Informado"].DefaultDecimal() - dcmTotalPagamento;
                        int intParcelaIndex = CaixaBUS.Parcela_Redundancia_Index(this.Operadora_Cartao_Regra_Redundancia(dtrPagamentos["Item"].DefaultInteger()), dtrRomaneioPagamentoVendaLiberada.Length - 1);
                        dtrRomaneioPagamentoVendaLiberada[intParcelaIndex]["Romaneio_Pagamento_Venda_Liberada_Valor"] = dtrRomaneioPagamentoVendaLiberada[intParcelaIndex]["Romaneio_Pagamento_Venda_Liberada_Valor"].DefaultDecimal() + dcmDiferenca;
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Alterar_Data_Vencimento_Cartao_Credito()
        {

            try
            {
                if (this.dttDadosCartao.Rows.Count <= 0)
                {
                    return;
                }

                DataTable dttOperadoraCartaoRegra = new Operadora_Cartao_RegrasBUS().Consultar_DataTable(TipoServidor.LojaAtual);

                CaixaBUS busCaixa = new CaixaBUS();
                DateTime dtmDataBaseVencimento;
                int intParcela;
                DataRow[] dtrRowsPagamentoVendaLiberada = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select(string.Empty, string.Empty, DataViewRowState.Added);

                foreach (DataRow dtrDadosCartao in this.dttDadosCartao.Rows)
                {
                    this.Obter_Operadora_Cartao_Credito(dtrDadosCartao);

                    dtmDataBaseVencimento = new DateTime(1900, 1, 1);
                    intParcela = 1;

                    foreach (DataRow dtrNovoPagamento in dtrRowsPagamentoVendaLiberada)
                    {
                        if (dtrNovoPagamento["Item"].DefaultInteger() != dtrDadosCartao["Pagamento_Item"].DefaultInteger())
                        {
                            continue;
                        }

                        if (dtmDataBaseVencimento.Equals(new DateTime(1900, 1, 1)))
                        {
                            dtmDataBaseVencimento = dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Data"].DefaultDateTime();
                        }

                        if (dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito"].DefaultBool())
                        {
                            dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Data"] = dtmDataBaseVencimento.AddDays(1);
                        }

                        if (dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito"].DefaultBool())
                        {
                            Regra_Cartao_Credito_Data_Vencimento EnuRegraDataVencimentoID = Regra_Cartao_Credito_Data_Vencimento.Dias_Corridos;

                            if (dtrDadosCartao["Operadora_Cartao_ID"].DefaultString() != string.Empty && dtrDadosCartao["Cartao_TEF_ID"].DefaultInteger() != 0)
                            {
                                DataRow[] dtrOperadoraCartaoRegra = dttOperadoraCartaoRegra.Select(" Operadora_Cartao_ID = " + dtrDadosCartao["Operadora_Cartao_ID"] + " AND Cartao_TEF_ID = " + dtrDadosCartao["Cartao_TEF_ID"]);
                                EnuRegraDataVencimentoID = dtrOperadoraCartaoRegra.Length > 0 ? (Regra_Cartao_Credito_Data_Vencimento)dtrOperadoraCartaoRegra[0]["Enum_Regras_Data_Vencimento_ID"] : Regra_Cartao_Credito_Data_Vencimento.Dias_Corridos;
                            }

                            dtrNovoPagamento["Romaneio_Pagamento_Venda_Liberada_Data"] = busCaixa.Retorna_Data_Vencimento_Cartao(EnuRegraDataVencimentoID, dtmDataBaseVencimento, intParcela, Root.Loja_Ativa.ID);

                            intParcela++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Obter_Operadora_Cartao_Credito(DataRow dtrDadosCartao)
        {
            try
            {
                DataRow[] dtrOperadoraCartao = this.dttOperadoraCartao.Select("Operadora_Cartao_Codigo_Sitef = " + dtrDadosCartao["Operadora_Cartao_Codigo_Sitef"].ToString());

                if (dtrOperadoraCartao.Length > 0)
                {
                    dtrDadosCartao["Operadora_Cartao_ID"] = dtrOperadoraCartao[0]["Operadora_Cartao_ID"];
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Liberar_Venda_Sem_Sitef()
        {
            try
            {
                if (this.blnIsLiberacaoCreditoResta)
                {
                    this.txtMenu.Content = "Autenticar devolução em dinheiro (Fiscal)";
                    this.enuSituacao = Operacao.Autenticar_Liberar_Credito_Em_Dinheiro;
                    this.txtMatricula.Visibility = Visibility.Visible;
                    this.txtMatricula.Focus();
                    return false;
                }

                this.Finalizar_Cupom_Fiscal();

                if ((this.Imprimir_Documento_Fiscal() == false) && (!this.Venda_Exclusiva_De_Servicos()))
                {
                    return false;
                }

                if (this.Inicializar_Venda_Sat() == false)
                {
                    return false;
                }

                if (this.Processar_Venda_Sat() == false)
                {
                    this.txtMenu.Content = MENSAGEM_OPERACAO_SAT_VENDA_COM_FALHA;
                    return false;
                }

                if (this.Confirmar_Venda(false) == false)
                {
                    return false;
                }

                string strRetornoImpressaoPacoteServico = this.Processar_Impressao_Comprovante_Pacote_e_Servico();

                this.Imprimir_Comprovante_Sat(false);

                this.Processar_Impressao_Procedimento_Garantia(this.blnParametroImprimeProcedimentoGarantia,
                                                               this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows[0]["Romaneio_Grupo_ID"].ToInteger());

                string strRetornoImpressaoResta = this.Processar_Impressao_Comprovante_Resta();

                // Reinicializa a tela e variaveis para a proxima venda.
                this.Inicializar_Nova_Venda();

                // Reimprime a mensagem de erro do comprovante, caso exista.
                if (strRetornoImpressaoPacoteServico != string.Empty)
                {
                    this.txtMenu.Content = strRetornoImpressaoPacoteServico;
                }
                else if (strRetornoImpressaoResta != string.Empty)
                {
                    this.txtMenu.Content = strRetornoImpressaoResta;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Liberar_Venda_Confirmado()
        {
            try
            {
                bool blnFaturado = false;
                bool blnTemRomaneioVenda = false;
                bool blnTemRomaneioAutorizado = false;
                if (this.dtsPreVendaTemporario.Tables.Count > 0)
                {
                    decimal dcmValorVendadoResta = 0;
                    decimal dcmValorResta = 0;
                    foreach (DataRow dtrRegistro in this.dtsPreVendaTemporario.Tables["Tela"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                    {
                        if (Convert.ToString(dtrRegistro["Enum_Romaneio_Tipo_Extenso"]) == "Troca" | Convert.ToString(dtrRegistro["Enum_Romaneio_Tipo_Extenso"]) == "Resta")
                        {
                            dcmValorVendadoResta = dcmValorVendadoResta + Convert.ToDecimal(dtrRegistro["ValorVendaResta"].DefaultDecimal());
                            dcmValorResta = dcmValorResta + Convert.ToDecimal(dtrRegistro["Romaneio_Pre_Venda_Ct_Valor_Total_Pago"].DefaultDecimal());

                            blnTemRomaneioAutorizado = (!string.IsNullOrEmpty(Convert.ToString(dtrRegistro["PagtoDinheiro"])) & Convert.ToString(dtrRegistro["PagtoDinheiro"]) != "0") || (dcmValorResta * -1) <= this.dcmValorLimiteAprovacao;
                        }
                        else
                        {
                            blnTemRomaneioVenda = true;
                            blnTemRomaneioAutorizado = false;
                            break;
                        }
                    }

                    if (blnTemRomaneioAutorizado & blnTemRomaneioVenda == false & !this.Venda_Possui_AutoServico())
                    {
                        this.intTipoDocumento = 0;
                        this.strCaixa = string.Empty;
                    }

                    DataRow[] dtrFaturado;
                    dtrFaturado = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select("Condicao_Pagamento_ID = " + Convert.ToString(this.intCondicaoPagtoID));

                    blnFaturado = Convert.ToBoolean(dtrFaturado[0]["Forma_Pagamento_Emissao_Fatura"]);

                    if (blnFaturado == false)
                    {
                        if (!this.Venda_Possui_AutoServico() & blnTemRomaneioVenda == false & blnTemRomaneioAutorizado == false & Convert.ToDecimal(this.dcmTotalVenda) <= 0)
                        {
                            this.txtMenu.Content = "Sem autorização de pagamento em dinheiro";
                            return false;
                        }
                    }
                }

                if (!this.Venda_Possui_AutoServico() & blnFaturado == false & blnTemRomaneioVenda == false & (blnTemRomaneioAutorizado == true))
                {
                    if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count >= 1)
                    {
                        this.txtMenu.Content = "Não é permitido incluir pagamento";
                        return false;
                    }
                    else if (this.intFormaPagamentoID != Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        this.txtMenu.Content = "Só é permitido pagamento em Dinheiro";
                        return false;
                    }

                    this.blnIsLiberacaoCreditoResta = true;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Liberar_Venda_Confirmado()
        {
            try
            {
                
                this.dtmDataLiberacao = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                if (this.Validar_Liberar_Venda_Confirmado())
                {
                    this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows.Clear();

                    this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows.Clear();
                    this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_It"].Rows.Clear();
                    this.dtsPreVendaTemporario.Tables["Romaneio_Documento"].Rows.Clear();
                    this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Clear();
                    this.dtsPreVendaTemporario.Tables["Peca_Digitada"].Rows.Clear();

                    this.Atualizar_Cliente_Romaneio();

                    if (
                        this.Preencher_DataRow_Ct_Pre_Venda() &&
                        this.Preencher_DataRow_It_Pre_Venda() &&
                        this.Preencher_DataRow_Grupo_Romaneio() &&
                        this.Preencher_DataRow_Documento() &&
                        this.Preencher_DataRow_Pagamento_Venda_Liberada() &&
                        this.Preencher_DataRow_Peca_Digitada()
                        )
                    {

                        bool blnProcessaSitef = false;
                        if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count > 0)
                        {
                            foreach (DataRow dtrPagamento in this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                            {
                                if (dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito"].DefaultBool() == true ||
                                      dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito"].DefaultBool() == true ||
                                      dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cheque"].DefaultBool() == true)
                                {
                                    blnProcessaSitef = true;
                                    break;
                                }
                            }
                        }

                        DataRow[] dtrSitef = null;
                        dtrSitef = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select("Condicao_Pagamento_ID = " + Conversion.Str(this.intCondicaoPagtoID));

                        if (Convert.ToBoolean(dtrSitef[0]["Forma_Pagamento_Emissao_Cheque"]) == true
                                | Convert.ToBoolean(dtrSitef[0]["Forma_Pagamento_Emissao_Cartao_Debito"]) == true
                                | Convert.ToBoolean(dtrSitef[0]["Forma_Pagamento_Emissao_Cartao_Credito"]) == true
                                | blnProcessaSitef == true)
                        {
                            if (this.blnSitefAtivo == false)
                            {
                                this.enuSituacao = Operacao.Pendente_POS;
                                this.txtMenu.Content = "Sitef Inativo. Processar no POS? (S\\N)";
                                this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                                return;
                            }
                            else if (!this.Liberar_Venda_Com_Sitef(dtrSitef[0]))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (!this.Liberar_Venda_Sem_Sitef())
                            {
                                return;
                            }
                        }
                    }

                    this.Verificar_Alerta_Sangria();

                    if (this.blnControleCancela)
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        // Valida ticket de estacionamento (se houver)
                        this.blnComando = true;
                        this.Window_KeyUp(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.T));

                    }
                    if (this.blnSolicitarDocumento)
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        this.blnComando = true;
                        this.Window_KeyUp(new object(), new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.W));
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Gerar_Pre_Venda()
        {
            try
            {
                
                if (this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0 & this.intOrcamentoCtID == 0)
                {
                    new Romaneio_Pre_VendaBUS().Confirmar_PreVenda(0, this.intLojaID, ref this.dtsPreVendaTemporario);
                    this.intOrcamentoCtID = this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Romaneio_Pre_Venda_Ct_ID"].DefaultInteger();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Confirmar_Venda(bool blnProcessadoSitef)
        {
            try
            {
                this.Gerar_Pre_Venda();

                // =======================================================================
                this.Criar_Log_Processo("   - Confirmar Venda");
                // =======================================================================

                // =======================================================================
                this.Criar_Log_Processo("   - Preencher_DataRow_Ct_Romaneio");
                // =======================================================================

                this.Preencher_DataRow_Ct_Romaneio();

                // =======================================================================
                this.Criar_Log_Processo("   - Preencher_DataRow_It_Romaneio");
                // =======================================================================
                this.Preencher_DataRow_It_Romaneio();

                // =======================================================================
                this.Criar_Log_Processo("   - Preencher_DataRow_Documento_Romameio");
                // =======================================================================

                this.Preencher_DataRow_Documento_Romameio();

                if (!string.IsNullOrEmpty(this.strECF))
                {

                    this.strDocumentoNumero = this.strECF + this.strCOO;
                }

                // =====================================================
                this.Criar_Log_Processo("   - Liberando Venda" + this.strDocumentoNumero);
                // =====================================================

                RomaneioBUS busRomaneio = new RomaneioBUS();
                if (busRomaneio.Liberar_Venda_Caixa(this.dtsPreVendaTemporario,
                                                        this.dtsRomaneioTemporario,
                                                        this.intLojaID,
                                                        this.intOrcamentoCtID,
                                                        this.strCaixa,
                                                        this.intTipoDocumento,
                                                        this.strDocumentoNumero,
                                                        this.strCOO,
                                                        string.Empty,
                                                        this.strECF,
                                                        ref this.dttDadosCartao,
                                                        ref this.dtoCaixaSatVenda) == false)
                {
                    this.txtMenu.Content = "Cancelar Cupom";
                    return false;
                }

                try
                {

                    // =====================================================
                    this.Criar_Log_Processo("       - Atualiza o saldo em dinheiro");
                    // =====================================================

                    DataRow[] dtrPagamentoDinheiro = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito = false AND Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito = false");

                    if (dtrPagamentoDinheiro.Length > 0)
                    {
                        this.dcmValorDinheiro = this.dcmValorDinheiro + dtrPagamentoDinheiro[0]["Romaneio_Pagamento_Venda_Liberada_Valor_Informado"].DefaultDecimal();
                    }

                    // =====================================================
                    this.Criar_Log_Processo("       - blnProcessadoSitef = " + blnProcessadoSitef.ToString());
                    // =====================================================

                    if (blnProcessadoSitef == false)
                    {
                        // =====================================================
                        this.Criar_Log_Processo("Não processou no SITEF");
                        // =====================================================

                        DataRow[] dtrPagamentoCartao = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito = true or Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito = true");

                        if (dtrPagamentoCartao.Length > 0)
                        {
                            this.blnPendentePOS = true;
                        }
                    }

                    /// P0050.3 - NOTA: Libera a venda apenas com os processos que envolvem as tabelas da nova estrutura do caixa
                    this.Liberar_Venda_Novas_Tabelas();
                }
                catch (Exception)
                {
                    this.Inicializar_Nova_Venda();
                    throw;
                }

                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Finalizar_Cupom_Fiscal()
        {
            try
            {
                this.Preencher_DataTable_CupomFiscal_Fechamento();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inicializar_Nova_Venda()
        {
            try
            {
                this.Limpar_Dados();
                this.Limpar_Tela();
                this.Limpar_Variaveis();

                // Inicializa para ser cliente consumidor final e dinheiro   
                this.Setar_Cliente_Consumidor_Final();
                this.Localizar_Forma_Pagamento_Troco();

                this.enuSituacao = Operacao.Operacao_Inicial;
                this.txtMenu.Content = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                this.Mensagem_Atualizacao_Sistema_Pendente();
                this.txtCodigoItemFabricante.Focus();

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Mensagem_Atualizacao_Sistema_Pendente()
        {
            try
            {
                if (Root.Mensagem_Atualizacao_Caixa != string.Empty)
                {
                    this.txtMenu.Content = Root.Mensagem_Atualizacao_Caixa;
                    this.txtVersao.Text = "Atualização Pendente.";
                    this.txtVersao.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Venda_Possui_AutoServico()
        {
            try
            {
                if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count == 0)
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

        private bool Venda_Possui_Romaneio()
        {
            try
            {
                if (this.dtsPreVendaEscolhido.Tables.Count == 0)
                {
                    return false;
                }

                if (this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count == 0)
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

        private void Criar_Log_Processo(string strMensagem)
        {
            try
            {
                string strRomaneioCT = string.Empty;

                if (this.dtsRomaneioTemporario.Tables.Count > 0 & this.dtsRomaneioTemporario.Tables.Contains("Romaneio_CT") & this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows.Count > 0)
                {
                    if (this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Romaneio_Ct_ID"] == null)
                        strRomaneioCT = " - Romaneio NULL";
                    else
                        strRomaneioCT = " - " + this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Romaneio_Ct_ID"].ToString();
                }

                Log.Info("LOG_LIBERACAO_SITEF", Root.Loja_Ativa.ID, string.Concat(strMensagem, strRomaneioCT));
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Usuário             "

        private bool Autenticar_Usuario()
        {
            try
            {
                this.Consultar_Usuario();

                if (this.dtoUsuarioAutenticar == null)
                {
                    FuncionarioDO dtoFuncionario = new FuncionarioBUS().Selecionar_Funcionario_Por_CD(this.txtMatricula.Password.Trim().DefaultInteger(), Enumerados.TipoServidor.LojaAtual);

                    this.strMensagemAutenticar = this.txtMenu.Content.ToString();
                    this.enuSituacaoSub = this.enuSituacao;

                    if (dtoFuncionario == null)
                    {
                        txtMenu.Content = "Funcionário não cadastrado";
                        return false;
                    }
                    else if (dtoFuncionario.isAtivo == false)
                    {
                        txtMenu.Content = "Funcionário desativado";
                        return false;
                    }
                    else
                    {
                        txtMenu.Content = "Usuário não cadastrado";
                        return false;
                    }

                }

                this.strMensagemAutenticar = this.txtMenu.Content.ToString();
                this.enuSituacaoSub = this.enuSituacao;

                switch (this.enuSituacao)
                {
                    case Operacao.Autenticar_Abertura_Caixa:
                        this.enuSituacao = Operacao.Senha_Abertura_Caixa;
                        break;
                    case Operacao.Autenticar_Suspender:
                        this.enuSituacao = Operacao.Senha_Suspender;
                        break;
                    case Operacao.Autenticar_Cancelar_Cupom_Suspender:
                        this.enuSituacao = Operacao.Senha_Cancelar_Cupom_Suspender;
                        break;
                    case Operacao.Autenticar_Cancelar_Cupom:
                        this.enuSituacao = Operacao.Senha_Cancelar_Cupom;
                        break;
                    case Operacao.Autenticar_Cancelar_Cupom_Item:
                        this.enuSituacao = Operacao.Senha_Cancelar_Cupom_Item;
                        break;
                    case Operacao.Autenticar_Liberar_Credito_Em_Dinheiro:
                        this.enuSituacao = Operacao.Senha_Liberar_Credito_Em_Dinheiro;
                        break;
                    case Operacao.Autenticar_Reducao_Z:
                        this.enuSituacao = Operacao.Senha_Reducao_Z;
                        break;
                    case Operacao.Autenticar_Desconto:
                        this.enuSituacao = Operacao.Senha_Desconto;
                        break;
                    case Operacao.Autenticar_Estorno_Pagamento:
                        this.enuSituacao = Operacao.Senha_Estorno_Pagamento;
                        break;
                    case Operacao.Autenticar_Estorno_Pagamento_POS:
                        this.enuSituacao = Operacao.Senha_Estorno_Pagamento_POS;
                        break;
                    case Operacao.Autenticar_Estorno_Pagamento_Credito:
                        this.enuSituacao = Operacao.Senha_Estorno_Pagamento_Credito;
                        break;
                    case Operacao.Autenticar_Liberar_POS:
                        this.enuSituacao = Operacao.Senha_Liberar_POS;
                        break;
                    case Operacao.Autenticar_Sangria:
                        this.enuSituacao = Operacao.Senha_Sangria;
                        break;
                    case Operacao.Autenticar_Fechamento_Operadora:
                        this.enuSituacao = Operacao.Senha_Fechamento_Operadora;
                        break;
                    case Operacao.Autenticar_Fechamento_Fiscal:
                        this.enuSituacao = Operacao.Senha_Fechamento_Fiscal;
                        break;
                    default:
                        break;
                }

                this.txtMatricula.Visibility = Visibility.Hidden;
                this.txtMatricula.Clear();
                this.txtMenu.Content = "Informe a senha";
                this.txtSenha.Visibility = Visibility.Visible;
                this.txtSenha.Focus();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Consultar_Usuario()
        {
            try
            {
                if (this.txtMatricula.Password == string.Empty)
                    return;

                this.dtoUsuarioAutenticar = new UsuarioBUS().Selecionar_Por_Funcionario(this.txtMatricula.Password.Trim().DefaultInteger(), true);

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Cliente             "

        private void Setar_Cliente_Consumidor_Final()
        {
            try
            {
                this.strClienteID = Constantes_Caixa.ID_CONSUMIDOR_FINAL;

                this.dtsCliente = new ClienteBUS().Consultar_DataSet_Dados_Cliente_para_Venda(this.strCpfCnpj, this.intLojaID, this.strClienteID);

                this.txtNomeCliente.Text = this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["Nome_Cliente"].DefaultString();

                if (this.Verifica_Cupom_Fiscal_Aberto() == false)
                {
                    this.strCpfCnpj = string.Empty;
                    this.blnUtilizaNFp = false;
                    this.imgNotaFiscalPaulista.Visibility = Visibility.Hidden;
                    this.strClienteNome = string.Empty;
                }

                this.blnCompraVista = false;

                this.Carregar_Dados_Contatos_Cliente(this.strClienteID);

                // Atualiza condição de pagamento de acordo com o tipo do cliente
                this.Carregar_CondicaoPagto(this.strClienteID);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Setar_Cliente()
        {
            try
            {
                if (this.strClienteID != Constantes_Caixa.ID_CONSUMIDOR_FINAL)
                {
                    return;
                }

                if (this.dtsRomaneioTemporario.Tables.Count == 0)
                {
                    return;
                }

                this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].DefaultView.Sort = "Romaneio_Ct_Cliente_CNPJCPF DESC";

                foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows)
                {
                    if (this.strClienteID != dtrRomaneio["Cliente_ID"].DefaultString().ToUpper())
                    {
                        this.strClienteID = dtrRomaneio["Cliente_ID"].DefaultString();
                        this.strCpfCnpj = dtrRomaneio["Romaneio_Ct_Cliente_CNPJCPF"].DefaultString();
                        this.strClienteNome = dtrRomaneio["Romaneio_Ct_Cliente_Nome"].DefaultString();
                        break;
                    }
                }

                if (this.strCpfCnpj != string.Empty)
                {
                    this.Preencher_Detalhes_Cliente(this.strCpfCnpj);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Carregar_Dados_Contatos_Cliente(string strClienteID)
        {
            try
            {
                this.dttContato = new ClienteBUS().Consultar_DataTable_Contatos_Por_Cliente(strClienteID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Carregar_CondicaoPagto(string strClienteVendaID)
        {
            try
            {
                this.dtsFormaXCliente = new Forma_Pagamento_Tipo_ClienteBUS().Consultar_DataSet_CondicaoPagto_pelo_TipoCliente(strClienteVendaID, this.blnCompraVista, this.intLojaID, false);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_Detalhes_Cliente(string strCNPJCPF)
        {
            try
            {
                
                this.dtsCliente = new ClienteBUS().Consultar_DataSet_Dados_Cliente_para_Venda(strCNPJCPF, this.intLojaID, string.Empty);

                if (this.dtsCliente.Tables["Cliente_Detalhe"].Rows.Count == 0)
                {
                    return true;
                }

                this.strClienteID = string.Empty;

                if (!this.Validar_Status_Cliente(ref this.blnCompraVista, ref this.dtsCliente))
                {
                    this.blnCompraVista = true;
                }

                this.strCpfCnpj = strCNPJCPF;
                this.strClienteID = this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["Cliente_ID"].DefaultString();

                this.Carregar_Dados_Contatos_Cliente(this.strClienteID);

                // Atualiza condição de pagamento de acordo com o tipo do cliente
                this.Carregar_CondicaoPagto(this.strClienteID);
                this.Localizar_Forma_Pagamento_Troco();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preencher_Detalhes_Cliente_Nota_Fiscal_Paulista(string strCNPJCPF)
        {
            try
            {
                this.dtsCliente = new ClienteBUS().Consultar_DataSet_Dados_Cliente_para_Venda(strCNPJCPF, this.intLojaID, string.Empty);

                if (this.dtsCliente.Tables["Cliente_Detalhe"].Rows.Count == 0)
                {
                    if (!(this.txtComando.Text == "00000000000" || this.txtComando.Text == "000.000.000-00"))
                    {
                        this.strCpfCnpjNotaFiscalPaulista = strCNPJCPF;
                        this.strClienteNotaFiscalPaulistaID = Constantes_Caixa.ID_CONSUMIDOR_FINAL;
                        this.blnUtilizaNFp = true;
                        this.imgNotaFiscalPaulista.Visibility = Visibility.Visible;

                        this.Preparar_Alterar_Cupom();
                    }

                    return true;
                }

                this.strClienteNotaFiscalPaulistaID = string.Empty;
                this.txtNomeCliente.Text = string.Empty;

                this.strCpfCnpjNotaFiscalPaulista = strCNPJCPF;

                this.blnUtilizaNFp = true;
                this.imgNotaFiscalPaulista.Visibility = Visibility.Visible;

                this.strClienteNotaFiscalPaulistaID = this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["Cliente_ID"].DefaultString();
                this.txtNomeCliente.Text = this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["Nome_Cliente"].DefaultString();

                this.Preparar_Alterar_Cupom();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Alterar_Dados_Cupom_Fiscal()
        {
            try
            {
                if (this.dttCupomFiscal.Rows.Count > 0)
                {
                    this.cpfCnpjNfpAlterado = !this.dttCupomFiscal.Rows[0]["Documento"].DefaultString().Equals(DivUtil.Formatar_CPF_CNPJ(this.strCpfCnpjNotaFiscalPaulista));

                    this.dttCupomFiscal.Rows[0]["Cliente"] = this.txtNomeCliente.Text;
                    this.dttCupomFiscal.Rows[0]["Documento"] = DivUtil.Formatar_CPF_CNPJ(this.strCpfCnpjNotaFiscalPaulista);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Processar_Venda_Funcionario()
        {
            try
            {
                this.Identificar_Venda_Funcionario_Por_Cliente();

                if (this.blnIsFuncionario)
                {
                    this.Atualizar_Itens_Servico_Venda_Funcionario();

                    this.Atualizar_Tela_Venda_Funcionario();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Identificar_Venda_Funcionario_Por_Cliente()
        {
            try
            {
                if (this.dtsCliente == null 
                    || this.dtsCliente.Tables.Count == 0 
                    || this.dtsCliente.Tables["Cliente_Detalhe"].Rows.Count == 0 
                    || this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["Funcionario_ID"].DefaultInteger() == 0)
                {
                    return;
                }

                this.Atualiza_Paramentros_Venda_Funcionario();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Identificar_Venda_Funcionario_Por_Romaneio()
        {
            try
            {
                if (this.dtsPreVendaEscolhido == null 
                    || this.dtsPreVendaEscolhido.Tables.Count == 0 
                    || this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count == 0 
                    || this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Funcionario_ID"].DefaultInteger() == 0)
                {
                    return;
                }

                this.Atualiza_Paramentros_Venda_Funcionario();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Atualiza_Paramentros_Venda_Funcionario()
        {
            try
            {
                this.blnIsFuncionario = true;
                this.dcmPercentualDescontoFuncionario = this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["Cliente_Tipo_Percentual_Desconto"].DefaultDecimal();

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Atualizar_Itens_Servico_Venda_Funcionario()
        {
            try
            {
                if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count == 0 || this.cpfCnpjNfpAlterado)
                {
                    return;
                }

                foreach (DataRow dtrOrcamentoItem in this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows)
                {
                    if (dtrOrcamentoItem["Enum_Objeto_Tipo_ID"].DefaultInteger() == Tipo_Objeto.Servico.DefaultInteger())
                    {

                        dtrOrcamentoItem["Orcamento_It_Preco_Pago"] = (dtrOrcamentoItem["Orcamento_It_Preco_Pago"].DefaultDecimal()
                                                                  * (1 - (this.dcmPercentualDescontoFuncionario / 100))).ToDecimalRound(2);
                        dtrOrcamentoItem["Orcamento_It_Preco_Vista"] = dtrOrcamentoItem["Orcamento_It_Preco_Pago"];

                        dtrOrcamentoItem["Preco_Total"] = ((dtrOrcamentoItem["Orcamento_It_Qtde"].DefaultInteger()
                                                            * dtrOrcamentoItem["QtdeMinVenda"].DefaultInteger())
                                                            * dtrOrcamentoItem["Orcamento_It_Preco_Pago"].DefaultDecimal()).ToString("#,##0.00");

                        dtrOrcamentoItem["Orcamento_It_Valor_Desconto"] = (dtrOrcamentoItem["Preco_Total_Lista"].DefaultDecimal()
                                                                * (this.dcmPercentualDescontoFuncionario / 100)).ToDecimalRound(2);
                        dtrOrcamentoItem["Orcamento_It_Valor_Comissao"] = (dtrOrcamentoItem["Orcamento_It_Preco_Pago"].DefaultDecimal() * (dtrOrcamentoItem["Comissao_Percentual"].DefaultDecimal() / 100)).ToDecimalRound(2);
                    }
                }

                foreach (DataRow dtrVendaIt in this.dtsGridVenda.Tables["Venda_It"].Rows)
                {
                    if (dtrVendaIt["Preco_Unitario"].DefaultDecimal() > 0 && !dtrVendaIt["Cancelado"].ToDefaultBoolean())
                    {
                        dtrVendaIt["Preco_Unitario"] = (dtrVendaIt["Preco_Unitario"].DefaultDecimal() * (1 - (this.dcmPercentualDescontoFuncionario / 100))).ToDecimalRound(2);

                        var pecaEmbalagemQtde = dtrVendaIt["Peca_Embalagem_Quantidade"];

                        dtrVendaIt["Total"] = (dtrVendaIt["Qtde"].DefaultInteger()
                                                * dtrVendaIt["Preco_Unitario"].DefaultDecimal()
                                                * (pecaEmbalagemQtde.DefaultString().IsNullOrEmpty() ? 1 : pecaEmbalagemQtde.DefaultDecimal())).ToDecimalRound(2);
                    }
                }

                this.Calcular_Valor_Total_Orcamento();

            }
            catch (Exception)
            {

                throw;
            }


        }

        private void Atualizar_Tela_Venda_Funcionario()
        {
            try
            {

                this.txtMenu.Content = "Venda Funcionário. Solicitar documento.";
                this.txtMenu.Foreground = new SolidColorBrush(Colors.Black);
                this.txtComando.Foreground = new SolidColorBrush(Colors.Black);
                this.txtMatricula.Foreground = new SolidColorBrush(Colors.Black);
                this.txtSenha.Foreground = new SolidColorBrush(Colors.Black);

                this.rtgPadrao.Visibility = Visibility.Hidden;
                this.rtgIdentificacaoFuncionario.Visibility = Visibility.Visible;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Atualizar_Tela_Venda_Consumidor_Final()
        {
            try
            {

                this.txtMenu.Foreground = new SolidColorBrush(Colors.White);
                this.txtComando.Foreground = new SolidColorBrush(Colors.White);
                this.txtMatricula.Foreground = new SolidColorBrush(Colors.White);
                this.txtSenha.Foreground = new SolidColorBrush(Colors.White);

                this.rtgPadrao.Visibility = Visibility.Visible;
                this.rtgIdentificacaoFuncionario.Visibility = Visibility.Hidden;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Validar_Status_Cliente(ref bool blnCompraVista, ref DataSet dtsSourceCliente)
        {
            try
            {

                if (this.dtsCliente.Tables["Cliente_Detalhe"].Rows.Count <= 0)
                {
                    this.txtMenu.Content = "Cliente não identificado";
                    return false;
                }

                if (Convert.ToString(this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["CNPJ_CPF"]) == "00000000000" & Convert.ToString(this.dtsCliente.Tables["Cliente_Detalhe"].Rows[0]["CNPJ_CPF"]) == "00000000000000")
                {
                    return false;
                }

                if (!Convert.ToBoolean(dtsSourceCliente.Tables["Cliente_Detalhe"].Rows[0]["Cliente_IsAtivo"]))
                {
                    this.txtMenu.Content = "O cliente está desativado";
                    return false;
                }

                if (Convert.ToInt32(dtsSourceCliente.Tables["Cliente_Detalhe"].Rows[0]["Enum_Status_ID"]) == (Int32)Status_Cliente.Inconsistencia_Cadastral
                        | Convert.ToInt32(dtsSourceCliente.Tables["Cliente_Detalhe"].Rows[0]["Enum_Status_ID"]) == (Int32)Status_Cliente.Inadimplente_e_Inconsistencia_Cadastral)
                {
                    this.txtMenu.Content = "Cliente bloqueado, inconsistência no cadastro";
                    return false;
                }

                if (!Convert.ToBoolean(dtsSourceCliente.Tables["Cliente_Detalhe"].Rows[0]["Cliente_Faturado"]))
                {
                    blnCompraVista = true;
                    return true;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_CPF_CNPJ(string strCPF)
        {
            try
            {
                if (Utilitario.Validar_Formato_CNPJ_CPF(strCPF))
                {
                    this.txtMenu.Content = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    return true;
                }
                else
                {
                    if (this.blnTipoPessoaFisica)
                    {
                        this.txtMenu.Content = "CPF inválido. Digite novamente";
                    }
                    else
                    {
                        this.txtMenu.Content = "CNPJ inválido. Digite novamente";
                    }
                    this.txtComando.Text = string.Empty;
                    return false;
                }
            }
            catch
            {
                throw;
            }
        }

        private bool Validar_Inclusao_CPF_CNPJ()
        {
            try
            {
                // Só permite a inclusão do CPF ou CNPJ caso exista uma peça diferente de serviço

                if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count == 0)
                {
                    return false;
                }

                foreach (DataRow dtrGridVendaIT in this.dtsGridVenda.Tables["Venda_It"].Rows)
                {
                    if (Convert.ToInt32(dtrGridVendaIT["Tipo_Objeto"]) != Convert.ToInt32(Tipo_Objeto.Servico))
                    {
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

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Metodo atualiza as informações do cliente, verificando se o romaneio 
        ///     já está identificado,  e se o cliente foi identificado na nota fiscal
        ///     paulista.
        /// </summary>
        /// <history>
        /// 	[msisiliani] 	28/08/2015      Modified
        /// 	    Foi incluso uma validação, para que, se o romaneio for uma troca, a 
        /// 	informação não deve ser atualizada.
        /// 	[msisiliani] 	27/01/2016      Modified
        /// 	    Inclusa validação para não atualizar as informações de romaneios de 
        /// 	 resta
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Atualizar_Cliente_Romaneio()
        {
            try
            {
                Boolean blnConsumidorFinalUtilizaNFp = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp;

                foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows)
                {
                    if (dtrRomaneio["Enum_Romaneio_Tipo_ID"].ToInteger() != TipoRomaneio.Troca.ToInteger() &&
                        dtrRomaneio["Enum_Romaneio_Tipo_ID"].ToInteger() != TipoRomaneio.Resta.ToInteger())
                    {
                        // se optou por um CPF para NFp substitui os dois no romaneio
                        if (blnConsumidorFinalUtilizaNFp)
                        {
                            dtrRomaneio["Romaneio_Ct_Cliente_CNPJCPF"] = this.strCpfCnpjNotaFiscalPaulista;
                            dtrRomaneio["Romaneio_Ct_Cliente_Nome"] = this.txtNomeCliente.Text;
                            dtrRomaneio["Cliente_ID"] = this.strClienteNotaFiscalPaulistaID;
                        }

                        // verifica se no romaneio está identificado o cliente, senão assume o identificado no caixa, ou se estiver como Consumidor Final, também substitui pelo CPF de um de um dos romaneios
                        if (String.IsNullOrEmpty(dtrRomaneio["Romaneio_Ct_Cliente_CNPJCPF"].DefaultString()) ||
                             dtrRomaneio["Romaneio_Ct_Cliente_CNPJCPF"].DefaultString() == "00000000000" ||
                             dtrRomaneio["Romaneio_Ct_Cliente_CNPJCPF"].DefaultString() == "000.000.000-00")
                        {
                            dtrRomaneio["Romaneio_Ct_Cliente_CNPJCPF"] = this.strCpfCnpj;
                            dtrRomaneio["Romaneio_Ct_Cliente_Nome"] = this.strClienteNome;
                            dtrRomaneio["Cliente_ID"] = this.strClienteID;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region "   Limpar              "

        private void Limpar_Variaveis()
        {

            try
            {
                this.blnVendaConsumo = false;
                this.blnRomaneioEstorno = false;
                this.blnEstornoFinalizado = false;
                this.blnItemDigitado = false;
                this.blnPendentePOS = false;
                this.blnRomaneioEspecial = false;
                this.blnIsLiberacaoCreditoResta = false;
                this.blnUtilizaNFp = false;
                this.blnNaoUtilizaNFp = false;
                this.blnPagamentoLiberado = false;
                this.blnCupomAberto = false;
                this.blnCompraVista = false;
                this.blnAbrirCaixaSangria = false;
                this.blnFechamentoSangria = false;
                this.blnEnvioVendaSatRealizada = false;
                this.blnAcordoProdutoReciclavel = false;

                this.dcmTotalVenda = 0;
                this.dcmDescontoComercial = 0;
                this.dcmDescontoItem = 0;
                this.dcmDescontoItemTotal = 0;
                this.intTipoDocumento = 0;
                this.intOrcamentoCtID = 0;
                this.intRomaneioComanda = 0;
                this.intItemGridCupom = 0;
                this.intItemPagamentoEstorno = 0;
                this.intEstornoCartaoCreditoID = 0;
                this.intUsuarioAprovacaoID = 0;

                this.strDocumentoNumero = string.Empty;
                this.strCOO = string.Empty;
                this.strECF = string.Empty;
                this.strCpfCnpj = string.Empty;
                this.strCpfCnpjNotaFiscalPaulista = string.Empty;
                this.blnFechamentoBloquearCaixa = false;
                this.blnIsFuncionario = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Limpar_Dados()
        {

            try
            {
                this.dtsFormaXCliente.Clear();
                this.dtsCliente.Clear();
                this.dttContato.Clear();
                this.dtsConsultaPecaItens.Clear();
                this.dtsOrcamentoIt.Clear();
                this.dtsGridVenda.Clear();

                this.dtsPreVendaTemporario.Clear();
                this.dtsPreVendaEscolhido.Clear();
                this.dtsRomaneioTemporario.Clear();
                this.dttCupomFiscal.Clear();
                this.dttCupomFiscalFechamento.Clear();
                this.dtsCondicaoPagto.Clear();

                this.dtsRomaneioEstorno.Clear();
                this.dttDadosCartao.Clear();

                this.dtoCaixaSatVenda = new SAT_VendaDO();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Limpar_Tela()
        {
            try
            {
                this.txtNomeCliente.Text = string.Empty;
                this.txtQuantidade.Clear();
                this.txtCodigoItemFabricante.Clear();
                this.txtMenu.Content = string.Empty;
                this.txtTotalVenda.Clear();
                this.txtTroco.Clear();
                this.txtAPagar.Clear();
                this.txtTotal.Clear();
                this.txtDescricaoProduto.Text = string.Empty;
                this.txtValorProduto.Text = string.Empty;

                this.txtComando.Clear();
                this.txtSenha.Clear();
                this.objVendaItemOrc.DataContext = null;
                this.objPagamentos.DataContext = null;

                this.objVendaItemOrc.Visibility = Visibility.Visible;
                this.grdItens.Visibility = Visibility.Visible;
                this.imgLogoMercadocar.Visibility = Visibility.Visible;

                this.objPagamentos.Visibility = Visibility.Hidden;
                this.objPagamentosEstorno.Visibility = Visibility.Hidden;
                this.objPagamentosFechamento.Visibility = Visibility.Hidden;
                this.grdPagamento.Visibility = Visibility.Hidden;

                this.imgNotaFiscalPaulista.Visibility = Visibility.Hidden;

                this.imgLogoMercadocarFechado.Visibility = Visibility.Hidden;

                this.txtLegenda.Visibility = Visibility.Hidden;

                this.Atualizar_Tela_Venda_Consumidor_Final();
            }
            catch
            {
                throw;
            }
        }

        private void Limpar_Tela_Fechamento()
        {
            try
            {
                this.dtsCaixaOperacao.Clear();
                this.dtsCaixaTemporario.Clear();

                this.txtUsuario.Text = "Não identificado";
                this.imgStatusUsuario.Source = new BitmapImage(new Uri("/MC_Formularios_Wpf;component/Images/MDI/Icone_Usuario_Offline.png", UriKind.Relative));
                this.txtMenu.Content = "Caixa Fechado";
                this.enuSituacao = Operacao.Operacao_Inicial;
                this.blnCaixaAberto = false;

                this.objVendaItemOrc.Visibility = Visibility.Hidden;
                this.objPagamentosFechamento.Visibility = Visibility.Hidden;
                this.grdItens.Visibility = Visibility.Hidden;
                this.imgLogoMercadocar.Visibility = Visibility.Hidden;

                this.imgLogoMercadocarFechado.Visibility = Visibility.Visible;
                this.txtLegenda.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Excluir_Item_Tab()
        {

            try
            {
                DataRow dtrExclusao;
                if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0)
                {
                    int intUltimaLinha = 0;

                    intUltimaLinha = this.dtsGridVenda.Tables["Venda_It"].Rows.Count - 1;

                    dtrExclusao = this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha];

                    if (Convert.ToBoolean(dtrExclusao["IsRomaneio"]) == true && this.dtsPreVendaEscolhido.Tables.Count > 0 && this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0)
                    {
                        DataRow[] dtrPreVendaEscolhido = this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Select();

                        foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                        {
                            if (Convert.ToInt32(dtrPreVendaEscolhido[0]["Romaneio_Pre_Venda_Ct_ID"]) == Convert.ToInt32(dtrRomaneio["Romaneio_Pre_Venda_Ct_ID"]))
                            {
                                dtrRomaneio.Delete();
                            }
                        }

                        foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                        {
                            if (Convert.ToInt32(dtrPreVendaEscolhido[0]["Romaneio_Pre_Venda_Ct_ID"]) == Convert.ToInt32(dtrRomaneio["Romaneio_Pre_Venda_Ct_ID"]))
                            {
                                dtrRomaneio.Delete();
                            }
                        }

                        if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0)
                        {
                            foreach (DataRow dtrVendaItem in this.dtsGridVenda.Tables["Venda_It"].Select("IsRomaneio = 1"))
                            {
                                if (Convert.ToInt32(dtrPreVendaEscolhido[0]["Romaneio_Pre_Venda_Ct_ID"]) == Convert.ToInt32(dtrVendaItem["Romaneio_Pre_Venda_Ct_ID"]))
                                {
                                    dtrVendaItem.Delete();
                                }
                            }
                        }

                        this.dtsPreVendaEscolhido.Clear();
                    }
                    else
                    {
                        if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0)
                        {

                            intUltimaLinha = this.dtsGridVenda.Tables["Venda_It"].Rows.Count - 1;

                            DataRow dtrVendaExclusao = this.dtsGridVenda.Tables["Venda_It"].Rows[intUltimaLinha];
                            dtrVendaExclusao.Delete();
                            this.intItemGridCupom -= 1;

                            this.dtsGridVenda.Tables["Venda_It"].AcceptChanges();

                        }
                        if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                        {

                            intUltimaLinha = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count - 1;

                            DataRow dtrOrcamentoExclusao = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows[intUltimaLinha];
                            dtrOrcamentoExclusao.Delete();

                            this.dtsOrcamentoIt.Tables["Orcamento_It"].AcceptChanges();
                        }
                    }
                }

                if (this.dttCupomFiscal.Rows.Count > 0)
                {
                    this.dttCupomFiscal.Clear();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Pagamento           "

        private void Localizar_Forma_Pagamento_Troco()
        {
            try
            {
                if (this.dtsFormaXCliente.Tables.Count == 0)
                    return;

                DataRow[] dtrItem = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select("Forma_Pagamento_Troco = 1");

                if (dtrItem.Length > 0)
                {
                    this.intCondicaoPagtoTroco = Convert.ToInt32(dtrItem[0]["Condicao_Pagamento_ID"]);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inicializar_Forma_Pagamento()
        {
            try
            {
                this.intFormaPagamentoID = Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO;

                DataRow[] dtrParcela = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select(" Forma_Pagamento_ID = " + this.intFormaPagamentoID.ToString());

                this.intCondicaoPagtoID = Convert.ToInt32(dtrParcela[0]["Condicao_Pagamento_ID"]);

            }
            catch
            {
                throw;
            }
        }

        private bool Excluir_Pagamento()
        {
            try
            {
                DataRow[] dtrCondicaoPgtoGrid = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Item = " + this.strItemPagamentoVenda);

                if (dtrCondicaoPgtoGrid.Length > 0)
                {
                    dtrCondicaoPgtoGrid[0].Delete();

                    this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].AcceptChanges();

                    // Atualiza as informações de Troco e À Pagar
                    this.txtTroco.Text = this.dcmTotalVenda > this.Calcular_Valor_Total_Pagamento() ? "0,00" : (this.Calcular_Valor_Total_Pagamento() - this.dcmTotalVenda).ToString("#,##0.00");
                    this.txtAPagar.Text = this.dcmTotalVenda > this.Calcular_Valor_Total_Pagamento() ? (this.dcmTotalVenda - this.Calcular_Valor_Total_Pagamento()).ToString("#,##0.00") : "0,00";

                    // Atualiza sequencial do item
                    int intSequencialPagto = 0;

                    this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].DefaultView.Sort = "Item";
                    foreach (DataRow dtrPagamentos in this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows)
                    {
                        dtrPagamentos["Item"] = intSequencialPagto + 1;
                        intSequencialPagto += 1;
                    }

                    // Atualiza o grid de pagamentos
                    this.objPagamentos.DataContext = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"];

                    if (this.objPagamentos.Items.Count > 0)
                    {
                        this.objPagamentos.ScrollIntoView(this.objPagamentos.Items[this.objPagamentos.Items.Count - 1]);
                    }
                    return true;
                }

                this.txtMenu.Content = "Pagamento não localizado";
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Exclui_Pagamento()
        {
            try
            {
                DataRow[] dtrCondicaoPgtoGrid = this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select("Item = " + this.txtComando.Text);

                if (dtrCondicaoPgtoGrid.Length > 0)
                {
                    return true;
                }

                this.txtMenu.Content = "Pagamento não localizado";
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Calcular_Valor_Total_Orcamento()
        {
            try
            {
                this.dcmTotalVenda = 0;
                if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count > 0)
                {

                    foreach (DataRow dtrItens in this.dtsGridVenda.Tables["Venda_It"].Select("Cancelado = False", string.Empty, DataViewRowState.CurrentRows))
                    {
                        this.dcmTotalVenda += Convert.ToDecimal(dtrItens["Total"]);
                    }

                    this.txtTotalVenda.Text = this.dcmTotalVenda.ToString("#,##0.00");
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private decimal Calcular_Valor_Total_Pagamento()
        {
            try
            {
                decimal dcmTotalPagamento = 0;
                if (this.dtsCondicaoPagto.Tables.Count > 0)
                {
                    foreach (DataRow dtrPagamentos in this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                    {
                        dcmTotalPagamento += Convert.ToDecimal(dtrPagamentos["Valor_Informado"]);
                    }
                }
                return dcmTotalPagamento;
            }
            catch
            {
                throw;
            }
        }

        private decimal Calcular_Valor_Total_Item_Impresso()
        {
            try
            {
                decimal dcmTotalItemImpresso = 0;
                if (this.dtsGridVenda.Tables.Count > 0)
                {
                    foreach (DataRow dtrItemImpresso in this.dtsGridVenda.Tables["Venda_It"].Select("IsCupomFiscal = 1"))
                    {
                        dcmTotalItemImpresso += dtrItemImpresso["Total"].DefaultDecimal();
                        dcmTotalItemImpresso += dtrItemImpresso["Desconto"].DefaultDecimal();
                    }
                }
                return dcmTotalItemImpresso;
            }
            catch
            {
                throw;
            }
        }

        private bool Retorna_Pagamento_Cartao(ref int intNumeroPagamento)
        {

            try
            {
                DataRow[] dtrPagamentoCartao = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"]
                                                    .Select("(Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito = True OR Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito = True) AND OperadoraCartao = '' AND Romaneio_Pagamento_Venda_Liberada_Dia_Parcela = 0");

                if (dtrPagamentoCartao.Length > 0)
                {
                    intNumeroPagamento = dtrPagamentoCartao[0]["Item"].DefaultInteger();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #region "   Estorno Pagamento       "

        private Tipo_Estorna_Pagamento Identifica_Tipo_Estorna_Pagamento()
        {
            try
            {
                DataRow[] dtPgtoEstornoGrid = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                if (dtPgtoEstornoGrid.Length > 0)
                {
                    if (Convert.ToInt32(dtPgtoEstornoGrid[0]["Enum_Tipo_Transacao_TEF_ID"]) == Convert.ToInt32(TipoTransacaoTEF.SITEF))
                    {
                        return Tipo_Estorna_Pagamento.Sitef;
                    }
                    else if (Convert.ToBoolean(dtPgtoEstornoGrid[0]["Forma_Pagamento_Emissao_Cartao_Debito"]) == false && Convert.ToBoolean(dtPgtoEstornoGrid[0]["Forma_Pagamento_Emissao_Cartao_Credito"]) == false)
                    {
                        return Tipo_Estorna_Pagamento.Dinheiro;
                    }
                    else if (Convert.ToInt32(dtPgtoEstornoGrid[0]["Enum_Tipo_Transacao_TEF_ID"]) == Convert.ToInt32(TipoTransacaoTEF.POS))
                    {
                        return Tipo_Estorna_Pagamento.POS;
                    }
                    else
                    {
                        // Encaminhar para o Caixa Central
                        return Tipo_Estorna_Pagamento.Outros;
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Verificar_Se_Item_Foi_Atualizado()
        {
            try
            {
                Estorno_Cartao_CreditoBUS_NEW busEstornoCartaoCredito = new Estorno_Cartao_CreditoBUS_NEW();

                if (busEstornoCartaoCredito.Verificar_Se_Item_Foi_Atualizado(this.dtsRomaneioEstorno))
                {
                    this.txtMenu.Content = "O registro selecionado está desatualizado.";
                    return true;
                }

                return false;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Processar_Estorno(bool blnPagamentoSitef, bool blnPagamentoPOS)
        {
            try
            {
                DataRow[] dtPgtoEstorno = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));
                if (dtPgtoEstorno.Length > 0)
                {
                    if (this.Verificar_Se_Item_Foi_Atualizado() == false)
                    {
                        string[] strCodCancelamento;
                        Boolean blnOperacaoRealizada = true;

                        if (blnPagamentoSitef)
                        {
                            blnOperacaoRealizada = this.Processar_Estorno_Sitef(dtPgtoEstorno[0]);

                            strCodCancelamento = this.dtoSitefEstorno.Lista_Transacoes[0].Codigo_NSU_Autorizador.Split(Convert.ToChar("\0"));
                            dtPgtoEstorno[0]["Estorno_Cartao_Credito_IT_Cod_Cancelamento"] = strCodCancelamento[0];
                        }
                        else if (blnPagamentoPOS)
                        {
                            blnOperacaoRealizada = true;

                            dtPgtoEstorno[0]["Estorno_Cartao_Credito_IT_Cod_Cancelamento"] = this.strCodCancelamentoPOS;
                        }
                        else
                        {
                            blnOperacaoRealizada = this.Processar_Estorno_Credito(0);
                        }

                        if (blnOperacaoRealizada)
                        {

                            this.Atualizar_Dados_Atualizacao_Estornos(dtPgtoEstorno[0], Status_Estorno.Estorno_Efetuado, dtPgtoEstorno[0]["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToDecimal());

                            Estorno_Cartao_CreditoBUS_NEW busEstornoCartaoCredito = new Estorno_Cartao_CreditoBUS_NEW();

                            if (this.Validar_Se_Tipo_Processo_Inclusao())
                            {
                                busEstornoCartaoCredito.Processar_Inclusao_Estorno_Cartao_Credito(this.dtsRomaneioEstorno);

                                this.intEstornoCartaoCreditoID = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Estorno_Cartao_Credito_CT_ID"].ToInteger();
                            }
                            else
                            {
                                busEstornoCartaoCredito.Processar_Alteracao_Estorno_Cartao_Credito(this.dtsRomaneioEstorno, false);
                            }

                            ///P0050.3 - NOTA: Atualizar as novas tabelas da nova estrutura do caixa.
                            this.Processar_Estorno_Atualizacao_Novas_Tabelas(false);

                            this.objPagamentosEstorno.DataContext = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"];

                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Processar_Estorno_Sitef(DataRow dtrItemSelecionado)
        {
            try
            {
                this.dtoSitefEstorno = new SitefDO(ref this.objImpressaoFiscal, this.objComunicacaoImpressoraFiscal, this.objTipoImpressoraFiscal);

                this.dtoSitefEstorno.Configura_Sitef();

                this.Preencher_Item_Transacao_SITEF(dtrItemSelecionado);

                frmSitef_Menu_Principal frmSitefMenuPrincipal = new frmSitef_Menu_Principal((Int32)Acoes_Sitef.Cancelamento_Normal, ref this.dtoSitefEstorno, this.dttCupomFiscal, this.dttCupomFiscalFechamento, 0, 0, MENSAGEM_ORIGEM, this.dttOperadoraCartao);

                if (frmSitefMenuPrincipal.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    frmSitefMenuPrincipal.Dispose();

                    return true;
                }
                else
                {
                    frmSitefMenuPrincipal.Dispose();

                    return false;
                }



            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Preencher_Item_Transacao_SITEF(DataRow dtrItemSelecionado)
        {
            try
            {
                this.dtoSitefEstorno.Multiplo_Pagamento = false;
                this.dtoSitefEstorno.Valor_Total_Venda = dtrItemSelecionado["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToString();

                DBUtil objDbUtil = new DBUtil();

                String strDataHoraServidor = objDbUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual).ToString("dd/MM/yy HH:mm:ss");

                strDataHoraServidor = strDataHoraServidor.Replace("/", string.Empty);
                strDataHoraServidor = strDataHoraServidor.Replace(":", string.Empty);

                String strData = strDataHoraServidor.Substring(0, 6);
                String strHora = strDataHoraServidor.Substring(7, 6);

                this.dtoSitefEstorno.Data_Transacao = strData;
                this.dtoSitefEstorno.Hora_Transacao = strHora;

                Sitef_TransacaoDO dtoTransacao = new Sitef_TransacaoDO();
                dtoTransacao.IsTipo_Transacao_Estorno = true;
                dtoTransacao.DataHora_Transacao = DateTime.Today.ToShortDateString().Replace("/", string.Empty);
                if (Convert.ToString(dtrItemSelecionado["Cartao_TEF_DS"]).ToUpper().IndexOf(CARTAO_TEF_DS_FININVEST) > 0)
                {
                    dtoTransacao.Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Fininvest;
                }
                else
                {
                    if (Convert.ToBoolean(dtrItemSelecionado["Forma_Pagamento_Emissao_Cartao_Credito"]) == true)
                    {
                        dtoTransacao.Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Credito;
                    }
                }

                dtoTransacao.Modalidade_Pagamento = "CARTAO";

                dtoTransacao.Valor_Transacao = dtrItemSelecionado["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToString();
                this.dtoSitefEstorno.Imprime_Somente_Romaneio = true;

                dtoTransacao.Numero_Cupom = string.Empty;
                dtoTransacao.Operador = this.dtoUsuario.Login;

                dtoTransacao.Numero_Parcelas = dtrItemSelecionado["Estorno_Cartao_Credito_IT_Numero_Parcelas"].ToString();
                dtoTransacao.Codigo_Autorizacao = dtrItemSelecionado["Estorno_Cartao_Credito_IT_Cod_Autorizacao"].ToString();
                dtoTransacao.Instituicao = dtrItemSelecionado["Cartao_TEF_ID"].ToString();

                this.dtoSitefEstorno.Lista_Transacoes.Add(dtoTransacao);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Atualizar_Dados_Atualizacao_Estornos(DataRow dtrItemSelecionado, Status_Estorno enuStatusEstorno, Decimal dcmValorEstornar)
        {
            try
            {
                this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Usuario_ID"] = this.intUsuario;
                this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Usuario_Nome_Completo"] = this.txtUsuario;

                dtrItemSelecionado["Enum_Status_Estorno_ID"] = enuStatusEstorno;
                dtrItemSelecionado["Enum_Status_Estorno_DS"] = enuStatusEstorno.ToDescription();
                dtrItemSelecionado["Estorno_Cartao_Credito_IT_Valor_Estorno"] = dcmValorEstornar;

                dtrItemSelecionado["Enum_Romaneio_Tipo_Estorno_ID"] = Convert.ToInt32(Romaneio_Tipo_Estorno.Estorno_Total);

                if (dtrItemSelecionado["Cartao_Numero_Parte1"].ToString() != string.Empty && dtrItemSelecionado["Cartao_Numero_Parte2"].ToString() != string.Empty)
                {
                    dtrItemSelecionado["Estorno_Cartao_Credito_IT_Numero_Cartao"] = dtrItemSelecionado["Cartao_Numero_Parte1"].ToString() + "******" + dtrItemSelecionado["Cartao_Numero_Parte2"].ToString();
                }
                else
                {
                    dtrItemSelecionado["Estorno_Cartao_Credito_IT_Numero_Cartao"] = string.Empty;
                }

                string[] strCodCancelamento;
                strCodCancelamento = Convert.ToString(dtrItemSelecionado["Estorno_Cartao_Credito_IT_Cod_Cancelamento"]).Split(Convert.ToChar("\0"));

                dtrItemSelecionado["Estorno_Cartao_Credito_IT_Cod_Cancelamento"] = strCodCancelamento[0];
                dtrItemSelecionado["Estorno_Cartao_Credito_IT_Data_Venda_Origem"] = dtrItemSelecionado["Estorno_Cartao_Credito_IT_Data_Venda_Origem"];
                dtrItemSelecionado["Romaneio_Venda_CT_ID"] = this.strEstornoPagamentoRomaneioPreVendaCTID.Equals(string.Empty) ? 0 : this.strEstornoPagamentoRomaneioPreVendaCTID.ToInteger();
                dtrItemSelecionado["Usuario_ID"] = this.intUsuario;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Processar_Estorno_Credito(decimal dcmValorEstornar)
        {
            try
            {
                DataRow[] dtPgtoEstornoGrid = new DataRow[1];

                if (this.intItemPagamentoEstorno > 0)
                {
                    dtPgtoEstornoGrid = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));
                }

                DataTable dttPreVendaCT = new DataTable("Romaneio_Pre_Venda_Ct");
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Lojas_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Cliente_ID", typeof(string));
                dttPreVendaCT.Columns.Add("Pessoa_Autorizada_ID", typeof(string));
                dttPreVendaCT.Columns.Add("Enum_Romaneio_Tipo_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Enum_Romaneio_Status_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Usuario_Vendedor_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Usuario_Gerente_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Usuario_Separador_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Condicao_Pagamento_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_Data_Geracao", typeof(DateTime));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF", typeof(string));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_Cliente_Nome", typeof(string));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_Cliente_Telefone", typeof(string));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_CT_Valor_Total_Pago", typeof(decimal));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_CT_Valor_Total_Lista", typeof(decimal));
                dttPreVendaCT.Columns.Add("Romaneio_Grupo_Origem_Resta_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Loja_Origem_Resta_ID", typeof(int));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_Apresentou_NF", typeof(bool));
                dttPreVendaCT.Columns.Add("Romaneio_Pre_Venda_Ct_Observacao", typeof(string));

                DataRow dtrIncluirCt = dttPreVendaCT.NewRow();

                RomaneioBUS busRomaneio = new RomaneioBUS();
                DataRow dtrVendaCT = busRomaneio.Selecionar(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Romaneio_Venda_CT_Origem_ID"].ToInteger(), this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"].ToInteger());
                dtrIncluirCt["Romaneio_Pre_Venda_Ct_ID"] = 0;
                dtrIncluirCt["Lojas_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"];
                dtrIncluirCt["Cliente_ID"] = dtrVendaCT["Cliente_ID"];
                dtrIncluirCt["Pessoa_Autorizada_ID"] = dtrVendaCT["Pessoa_Autorizada_ID"];
                dtrIncluirCt["Enum_Romaneio_Tipo_ID"] = Enumerados.TipoRomaneio.Resta;
                dtrIncluirCt["Enum_Romaneio_Status_ID"] = StatusRomaneioVenda.Confirmado;
                dtrIncluirCt["Usuario_Vendedor_ID"] = dtrVendaCT["Usuario_Vendedor_ID"];
                dtrIncluirCt["Usuario_Gerente_ID"] = dtrVendaCT["Usuario_Gerente_ID"];
                dtrIncluirCt["Usuario_Separador_ID"] = 0;
                Condicao_Pagamento_VendaBUS busCondicaoPagamentoVenda = new Condicao_Pagamento_VendaBUS();
                dtrIncluirCt["Condicao_Pagamento_ID"] = busCondicaoPagamentoVenda.Consultar_ID_Condicao_Pagamento_Por_Loja_E_Forma_Pagamento(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"].ToInteger(),
                                                                                                     ESTORNO_PAGAMENTO_DINHEIRO, ESTORNO_PAGAMENTO_A_VISTA);
                dtrIncluirCt["Romaneio_Pre_Venda_Ct_Data_Geracao"] = (new DBUtil()).Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
                dtrIncluirCt["Romaneio_Pre_Venda_Ct_Cliente_CNPJCPF"] = dtrVendaCT["Romaneio_Ct_Cliente_CNPJCPF"];
                dtrIncluirCt["Romaneio_Pre_Venda_Ct_Cliente_Nome"] = dtrVendaCT["Romaneio_Ct_Cliente_Nome"];
                dtrIncluirCt["Romaneio_Pre_Venda_Ct_Cliente_Telefone"] = dtrVendaCT["Romaneio_Ct_Cliente_Telefone"];
                dtrIncluirCt["Romaneio_Grupo_Origem_Resta_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Romaneio_Grupo_Origem_ID"];
                dtrIncluirCt["Loja_Origem_Resta_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"];

                dtrIncluirCt["Romaneio_Pre_Venda_CT_Valor_Total_Pago"] = this.intItemPagamentoEstorno > 0 ? dtPgtoEstornoGrid[0]["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToDecimal() * (-1) : dcmValorEstornar * (-1);
                dtrIncluirCt["Romaneio_Pre_Venda_CT_Valor_Total_Lista"] = this.intItemPagamentoEstorno > 0 ? dtPgtoEstornoGrid[0]["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToDecimal() * (-1) : dcmValorEstornar * (-1);
                dtrIncluirCt["Romaneio_Pre_Venda_Ct_Apresentou_NF"] = false;

                dttPreVendaCT.Rows.Add(dtrIncluirCt);

                Romaneio_Pre_VendaBUS busRomaneioPreVenda = new Romaneio_Pre_VendaBUS();
                this.strEstornoPagamentoRomaneioPreVendaCTID = busRomaneioPreVenda.Incluir_Romaneio_Pre_Venda_Ct(dtrIncluirCt).ToString();

                // Faz a inclusão na tabela Romaneio_Venda_Origem

                DataTable dttRomaneioVendaOrigem = new DataTable("Romaneio_Venda_Origem");

                dttRomaneioVendaOrigem.Columns.Add("Romaneio_Venda_Origem_ID", typeof(string));
                dttRomaneioVendaOrigem.Columns.Add("Lojas_ID", typeof(int));
                dttRomaneioVendaOrigem.Columns.Add("Romaneio_Venda_Origem_Pre_Venda_Ct_ID", typeof(int));
                dttRomaneioVendaOrigem.Columns.Add("Romaneio_Pre_Venda_Ct_ID", typeof(int));
                dttRomaneioVendaOrigem.Columns.Add("Lojas_Origem_ID", typeof(int));

                DataRow dtrRomaneioVendaOrigem = dttRomaneioVendaOrigem.NewRow();

                dtrRomaneioVendaOrigem["Romaneio_Venda_Origem_ID"] = string.Empty;
                dtrRomaneioVendaOrigem["Lojas_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"];
                dtrRomaneioVendaOrigem["Romaneio_Venda_Origem_Pre_Venda_Ct_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Romaneio_CT_ID"];
                dtrRomaneioVendaOrigem["Romaneio_Pre_Venda_Ct_ID"] = this.strEstornoPagamentoRomaneioPreVendaCTID.ToInteger();
                dtrRomaneioVendaOrigem["Lojas_Origem_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_Origem_ID"];

                busRomaneioPreVenda.Incluir_Romaneio_Venda_Origem(dtrRomaneioVendaOrigem);

                DataSet dtsResta;
                Romaneio_Pre_VendaBUS busPreVenda = new Romaneio_Pre_VendaBUS();
                dtsResta = busPreVenda.Imprimir_DataSet_Romaneio(0, 0, Convert.ToInt32(dtrIncluirCt["Romaneio_Grupo_Origem_Resta_ID"]), Convert.ToInt32(dtrIncluirCt["Loja_Origem_Resta_ID"]));

                Parametros_SistemaBUS busParametrosSistema = new Parametros_SistemaBUS();
                DataSet dtsLayout = busParametrosSistema.Ver_DataSet_Parametro_Com_Loja("LAYOUT_ROMANEIO", Convert.ToInt32(dtrIncluirCt["Lojas_ID"]));

                string strTipoImpressao = string.Empty;
                if (dtsLayout.Tables["Parametros_Sistema"].Rows.Count > 0)
                {
                    strTipoImpressao = Convert.ToString(dtsLayout.Tables["Parametros_Sistema"].Rows[0]["Parametros_Sistema_Valor"]);
                }

                if (this.Imprimir_Comprovante_De_Resta(dtsResta, strTipoImpressao) != string.Empty)
                {
                    Impressao_Romaneio objImpRomaneio = new Impressao_Romaneio();
                    objImpRomaneio.ImprimirRomaneioCredito(dtsResta, strTipoImpressao, true);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        private void Gerar_Resta_Origem_Credito()
        {
            try
            {
                decimal dcmValorTotal = 0;
                decimal dcmValorEstornado = 0;
                decimal dcmValorEstornar = 0;

                this.Preencher_Valores_Estorno(ref dcmValorTotal, ref dcmValorEstornado, ref dcmValorEstornar);

                this.intItemPagamentoEstorno = 0;

                if (this.Processar_Estorno_Credito(dcmValorEstornar))
                {
                    Estorno_Cartao_CreditoBUS_NEW busEstornoCartaoCredito = new Estorno_Cartao_CreditoBUS_NEW();
                    this.Processar_Estorno_Credito_Novas_Tabelas(dcmValorEstornar);
                    busEstornoCartaoCredito.Processar_Alteracao_Estorno_Cartao_Credito(this.dtsRomaneioEstorno, true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Estorno Pagamento  : Novas Tabelas  "

        private void Processar_Estorno_Atualizacao_Novas_Tabelas(bool blnRestaGeradoOrigemCredito)
        {
            try
            {
                DataSet dtsRomaneioVenda = this.Preencher_Romaneio_Venda_Estorno();
                Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();

                busRomaneioVenda.Atualizar_Romaneio_Venda_Estorno(dtsRomaneioVenda,
                                                                  Convert.ToDecimal(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Compute("Sum(Estorno_Cartao_Credito_IT_Valor_Estorno)", string.Empty)),
                                                                  Convert.ToDecimal(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Romaneio_CT_Valor_Total_Estorno"].ToString()),
                                                                  blnRestaGeradoOrigemCredito);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet Preencher_Romaneio_Venda_Estorno()
        {
            try
            {
                DataSet dtsRomaneioVenda = this.Criar_DataSet_Romaneio_Estorno_Venda();

                this.Preencher_Romaneio_Venda_Estorno_Capa_Liberacao(dtsRomaneioVenda);
                this.Preencher_Romaneio_Venda_Estorno_Grupo(dtsRomaneioVenda);
                this.Preencher_Romaneio_Venda_Estorno_Pagamento(dtsRomaneioVenda);

                return dtsRomaneioVenda;

            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método cria o DataSet de Romaneio de Venda adicionando as tabelas de 
        ///     Romaneio_Venda_CT para o resta que será criado;
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private DataSet Criar_DataSet_Romaneio_Estorno_Venda()
        {
            try
            {
                DataSet dtsRomaneioVenda = new DataSet();

                Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();
                dtsRomaneioVenda.Tables.Add(busRomaneioVenda.Retornar_Estrutura_Tabela().Copy());

                Romaneio_Venda_GrupoBUS busRomaneioVendaGrupo = new Romaneio_Venda_GrupoBUS();
                dtsRomaneioVenda.Tables.Add(busRomaneioVendaGrupo.Retornar_Estrutura_Tabela().Copy());

                Romaneio_Venda_PagamentoBUS busRomaneioVendaPagamento = new Romaneio_Venda_PagamentoBUS();
                dtsRomaneioVenda.Tables.Add(busRomaneioVendaPagamento.Retornar_Estrutura_Tabela().Copy());

                return dtsRomaneioVenda;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche os dados da capa do Romaneio de Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Estorno_Capa_Liberacao(DataSet dtsRomaneioVenda)
        {
            try
            {
                foreach (DataRow dtrRomaneioCTItem in this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows)
                {
                    Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();

                    DataRow dtrRomaneioVendaCT = busRomaneioVenda.Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_CT(dtsRomaneioVenda.Tables["Romaneio_Venda_CT"]);

                    ///P0050.3 - NOTA: Campo temporário. Gravação do ID da Pré-Venda
                    dtrRomaneioVendaCT["Romaneio_Pre_Venda_CT_ID"] = dtrRomaneioCTItem["Romaneio_CT_ID"];
                    dtrRomaneioVendaCT["Lojas_ID"] = dtrRomaneioCTItem["Lojas_ID"];
                    dtrRomaneioVendaCT["Cliente_ID"] = dtrRomaneioCTItem["Cliente_ID"];
                    dtrRomaneioVendaCT["Pessoa_Autorizada_ID"] = dtrRomaneioCTItem["Pessoa_Autorizada_ID"];
                    dtrRomaneioVendaCT["Enum_Tipo_ID"] = TipoRomaneio.Estorno;
                    dtrRomaneioVendaCT["Enum_Status_ID"] = Status_Romaneio_Venda.Liberado;
                    dtrRomaneioVendaCT["Condicao_Pagamento_ID"] = dtrRomaneioCTItem["Condicao_Pagamento_ID"];
                    dtrRomaneioVendaCT["Usuario_Vendedor_ID"] = dtrRomaneioCTItem["Usuario_Vendedor_ID"];
                    dtrRomaneioVendaCT["Usuario_Gerente_ID"] = dtrRomaneioCTItem["Usuario_Gerente_ID"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_CNPJCPF"] = dtrRomaneioCTItem["Romaneio_Ct_Cliente_CNPJCPF"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_Nome"] = dtrRomaneioCTItem["Romaneio_Ct_Cliente_Nome"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_Telefone"] = dtrRomaneioCTItem["Romaneio_Ct_Cliente_Telefone"];
                    Decimal dcmValorEstornar = Convert.ToDecimal(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Compute("Sum(Estorno_Cartao_Credito_IT_Valor_Estorno)", string.Empty));
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Valor_Pago"] = dcmValorEstornar * (-1);
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Valor_Lista"] = dtrRomaneioCTItem["Romaneio_Ct_Valor_Total_Lista"].ToDecimal() * (-1);

                    dtrRomaneioVendaCT["Romaneio_Venda_CT_ID"] = busRomaneioVenda.Selecionar_Romaneio_Venda_CT_ID_Por_Pre_Venda(dtrRomaneioCTItem["Romaneio_Ct_ID"].ToInteger(), dtrRomaneioCTItem["Lojas_ID"].ToInteger());

                    dtsRomaneioVenda.Tables["Romaneio_Venda_CT"].Rows.Add(dtrRomaneioVendaCT);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche os dados do grupo do Romaneio Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Estorno_Grupo(DataSet dtsRomaneioVenda)
        {
            try
            {
                Romaneio_Venda_GrupoBUS busRomaneioVendaGrupo = new Romaneio_Venda_GrupoBUS();

                DataRow dtrRomaneioVendaGrupo = busRomaneioVendaGrupo.Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_Grupo(dtsRomaneioVenda.Tables["Romaneio_Venda_Grupo"]);
                DataRow dtrRomaneioEstorno = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0];

                dtrRomaneioVendaGrupo["Lojas_ID"] = dtrRomaneioEstorno["Lojas_ID"];
                dtrRomaneioVendaGrupo["Usuario_Caixa_ID"] = this.intUsuario;
                dtrRomaneioVendaGrupo["Enum_Status_ID"] = Status_Romaneio_Venda_Grupo.Liberado.ToInteger();
                dtrRomaneioVendaGrupo["Cliente_ID"] = dtrRomaneioEstorno["Cliente_ID"];
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Data_Liberacao"] = dtrRomaneioEstorno["Estorno_Cartao_Credito_CT_Data_Ultima_Alteracao"];

                dtsRomaneioVenda.Tables["Romaneio_Venda_Grupo"].Rows.Add(dtrRomaneioVendaGrupo);

            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche os dados do pagamento do Romaneio Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Estorno_Pagamento(DataSet dtsRomaneioVendaTemporario)
        {
            try
            {
                Romaneio_Venda_PagamentoBUS busRomaneioVendaPagamento = new Romaneio_Venda_PagamentoBUS();

                DataRow dtrRomaneioEstorno = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0];
                DataRow dtrRomaneioVendaPagamento = busRomaneioVendaPagamento.Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_Pagamento(dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_Pagamento"]);
                Decimal dcmValorEstornar = Convert.ToDecimal(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Compute("Sum(Estorno_Cartao_Credito_IT_Valor_Estorno)", string.Empty));

                dtrRomaneioVendaPagamento["Lojas_ID"] = dtrRomaneioEstorno["Lojas_ID"];
                dtrRomaneioVendaPagamento["Condicao_Pagamento_ID"] = dtrRomaneioEstorno["Condicao_Pagamento_ID"];
                dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Valor"] = dcmValorEstornar;
                dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Dias_Parcela"] = 0;
                dtrRomaneioVendaPagamento["Usuario_Ultima_Alteracao_ID"] = this.intUsuario;
                dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Valor"] = dcmValorEstornar;

                dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_Pagamento"].Rows.Add(dtrRomaneioVendaPagamento);

            }
            catch (Exception)
            {
                throw;
            }
        }

        #region "   Estorno Pagamento Resta  "

        private void Processar_Estorno_Credito_Novas_Tabelas(decimal dcmValorEstornar)
        {
            try
            {
                DataTable dttRomaneioVendaTemporario = this.Preencher_Romaneio_Venda_Resta(dcmValorEstornar);
                Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();

                busRomaneioVenda.Incluir_Credito_Estorno(dttRomaneioVendaTemporario.Rows[0],
                                                         this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Romaneio_Grupo_Origem_ID"].ToInteger(),
                                                         this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"].ToInteger());


            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Preencher_Romaneio_Venda_Resta(decimal dcmValorEstornar)
        {
            try
            {
                Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();

                DataTable dttRomaneioVendaTemporario = busRomaneioVenda.Retornar_Estrutura_Tabela().Copy();

                this.Preencher_Romaneio_Venda_Capa_Credito(dttRomaneioVendaTemporario, dcmValorEstornar);

                return dttRomaneioVendaTemporario;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Romaneio_Venda_Capa_Credito(DataTable dttRomaneioVendaCT, decimal dcmValorEstornar)
        {
            try
            {

                Romaneio_VendaBUS busRomaneioVendaCT = new Romaneio_VendaBUS();

                DataRow dtrRomaneioVendaOrigemCT = busRomaneioVendaCT.Selecionar_Por_Pre_Venda(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Romaneio_Venda_CT_Origem_ID"].ToInteger(),
                                                                                               this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"].ToInteger());

                DataRow dtrIncluirCT = busRomaneioVendaCT.Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_CT(dttRomaneioVendaCT);

                DataRow[] dtPgtoEstornoGrid = new DataRow[1];

                if (this.intItemPagamentoEstorno > 0)
                {
                    dtPgtoEstornoGrid = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));
                }

                dtrIncluirCT["Romaneio_Pre_Venda_Ct_ID"] = 0;
                dtrIncluirCT["Lojas_ID"] = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"];
                dtrIncluirCT["Cliente_ID"] = dtrRomaneioVendaOrigemCT["Cliente_ID"];
                dtrIncluirCT["Pessoa_Autorizada_ID"] = dtrRomaneioVendaOrigemCT["Pessoa_Autorizada_ID"];
                dtrIncluirCT["Enum_Tipo_ID"] = Enumerados.TipoRomaneio.Resta;
                dtrIncluirCT["Enum_Status_ID"] = StatusRomaneioVenda.Confirmado;
                dtrIncluirCT["Usuario_Vendedor_ID"] = dtrRomaneioVendaOrigemCT["Usuario_Vendedor_ID"];
                dtrIncluirCT["Usuario_Gerente_ID"] = dtrRomaneioVendaOrigemCT["Usuario_Gerente_ID"];
                dtrIncluirCT["Usuario_Separador_ID"] = 0;
                Condicao_Pagamento_VendaBUS busCondicaoPagamentoVenda = new Condicao_Pagamento_VendaBUS();
                dtrIncluirCT["Condicao_Pagamento_ID"] = busCondicaoPagamentoVenda.Consultar_ID_Condicao_Pagamento_Por_Loja_E_Forma_Pagamento(this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"].ToInteger(),
                                                                                                     ESTORNO_PAGAMENTO_DINHEIRO, ESTORNO_PAGAMENTO_A_VISTA);
                dtrIncluirCT["Romaneio_Venda_Ct_Data_Geracao"] = (new DBUtil()).Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
                dtrIncluirCT["Romaneio_Venda_Ct_Cliente_CNPJCPF"] = dtrRomaneioVendaOrigemCT["Romaneio_Venda_CT_Cliente_CNPJCPF"];
                dtrIncluirCT["Romaneio_Venda_Ct_Cliente_Nome"] = dtrRomaneioVendaOrigemCT["Romaneio_Venda_CT_Cliente_Nome"];
                dtrIncluirCT["Romaneio_Venda_Ct_Cliente_Telefone"] = dtrRomaneioVendaOrigemCT["Romaneio_Venda_CT_Cliente_Telefone"];

                dtrIncluirCT["Romaneio_Venda_CT_Valor_Pago"] = this.intItemPagamentoEstorno > 0 ? dtPgtoEstornoGrid[0]["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToDecimal() * (-1) : dcmValorEstornar * (-1);
                dtrIncluirCT["Romaneio_Venda_CT_Valor_Lista"] = this.intItemPagamentoEstorno > 0 ? dtPgtoEstornoGrid[0]["Estorno_Cartao_Credito_IT_Valor_Venda_Origem"].ToDecimal() * (-1) : dcmValorEstornar * (-1);

                dtrIncluirCT["Romaneio_Pre_Venda_CT_ID"] = this.strEstornoPagamentoRomaneioPreVendaCTID.ToInteger();

                dttRomaneioVendaCT.Rows.Add(dtrIncluirCT);

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #endregion

        #endregion

        #region "   Peça                "

        private bool Validar_Peca_Servico()
        {
            try
            {
                if (this.txtCodigoItemFabricante.Text.Length <= NUMERO_PADRAO_CARACTERES_SERVICO)
                {
                    DataSet dtsServico = new ServicoBUS().Consultar_Dados_Servico_para_Venda(this.txtCodigoItemFabricante.Text.PadLeft(4, '0'), this.intLojaID, 0);

                    if (dtsServico != null && dtsServico.Tables["Itens_Servico"].Rows.Count > 0)
                    {
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

        private bool Validar_Peca_Cadastrada()
        {
            try
            {
                
                DataRow dtrPecaCodBarra = new PecaBUS().Selecionar_DataRow_Por_Codigo_Barras(this.txtCodigoItemFabricante.Text, Root.AcessoDoServidor.ServidorLocal);

                if (dtrPecaCodBarra.Table.Rows.Count <= 0)
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

        private bool Buscar_Preencher_Peca_Por_Codigo_Barras()
        {
            try
            {
                if (this.txtCodigoItemFabricante.Text.Length == NUMERO_PADRAO_CARACTERES_SERVICO)
                {
                    return this.Buscar_Preencher_Servico_Por_Codigo_Barras();
                }

                PecaBUS busPeca = new PecaBUS();

                DataRow dtrPecaCodBarra = busPeca.Selecionar_DataRow_Por_Codigo_Barras(this.txtCodigoItemFabricante.Text, Root.AcessoDoServidor.ServidorLocal);

                if (dtrPecaCodBarra.Table.Rows.Count <= 0)
                {
                    this.txtMenu.Content = "Não existe peça cadastrada";
                    this.blnItemDigitado = false;
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    return false;
                }

                DataSet dtsPeca = busPeca.Consultar_Dados_Peca_Embalagem_para_Venda(this.intLojaID, dtrPecaCodBarra["Peca_ID"].DefaultInteger(), dtrPecaCodBarra["Peca_Embalagem_ID"].DefaultInteger());

                if (dtsPeca.Tables["Peca_Venda_Embalagem"].Rows.Count == 0)
                {
                    this.txtMenu.Content = "Peça não identificada";

                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.blnItemDigitado = false;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                    return false;
                }
                else
                {
                    if (dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_KIT_Compra_CT_ID"].DefaultInteger() != 0)
                    {
                        this.txtMenu.Content = "Peça de Kit. Venda somente com romaneio ou comanda.";
                        this.txtQuantidade.Text = string.Empty;
                        this.txtQuantidade.Focus();
                        return false;
                    }
                    decimal dcmPrecoPeca = 0;
                    int intQtdeMinima = 0;

                    this.txtQuantidade.Text = this.txtQuantidade.Text == string.Empty ? "1" : this.txtQuantidade.Text;
                    intQtdeMinima = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_QtMinimaVenda"].DefaultInteger();
                    dcmPrecoPeca = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Preco_Valor"].DefaultDecimal();

                    if (Convert.ToInt32(this.txtQuantidade.Text) < intQtdeMinima)
                    {
                        this.txtMenu.Content = "A quantidade não pode ser menor do que " + intQtdeMinima.DefaultString();
                        this.blnItemDigitado = false;
                        this.txtQuantidade.Text = string.Empty;
                        this.txtQuantidade.Focus();
                        return false;
                    }

                    if (dcmPrecoPeca == 0)
                    {
                        this.txtMenu.Content = "Peça sem preço";
                        this.blnItemDigitado = false;
                        this.txtCodigoItemFabricante.Text = string.Empty;
                        this.txtQuantidade.Text = string.Empty;
                        this.txtCodigoItemFabricante.Focus();
                        return false;
                    }

                    DataRow dtrPecasItens = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].NewRow();

                    dtrPecasItens["Item"] = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count + 1;
                    dtrPecasItens["Peca_CodBarra_CdBarras"] = this.txtCodigoItemFabricante.Text.Trim();
                    dtrPecasItens["Fabricante_ID"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Fabricante_ID"];
                    dtrPecasItens["Produto_ID"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Produto_ID"];
                    dtrPecasItens["Peca_ID"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_ID"];
                    dtrPecasItens["Peca_CDFabricante"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_CDFabricante"];
                    dtrPecasItens["Fabricante_NmFantasia"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Fabricante_NmFantasia"];
                    dtrPecasItens["Fabricante_CD"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Fabricante_CD"];
                    dtrPecasItens["Produto_DS"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Produto_DS"];
                    dtrPecasItens["Produto_CD"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Produto_CD"];
                    dtrPecasItens["Peca_CD"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_CD"];
                    dtrPecasItens["Peca_DsTecnica"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_DsTecnica"];
                    dtrPecasItens["Peca_CDFabricante"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_CDFabricante"];

                    if (this.blnIsFuncionario)
                    {
                        dtrPecasItens["Peca_Preco_Valor"] = (dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Preco_Valor"].DefaultDecimal()
                                                                * (1 - (this.dcmPercentualDescontoFuncionario / 100))).ToDecimalRound(2);
                        dtrPecasItens["Peca_Desconto"] = (dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Preco_Valor"].DefaultDecimal()
                                                                * (this.dcmPercentualDescontoFuncionario / 100)).ToDecimalRound(2);
                        dtrPecasItens["Valor_Comissao"] = (dtrPecasItens["Peca_Preco_Valor"].DefaultDecimal() * (dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Comissao_Percentual"].DefaultDecimal() / 100)).ToDecimalRound(2);
                    }
                    else
                    {
                        dtrPecasItens["Peca_Preco_Valor"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Preco_Valor"];
                        dtrPecasItens["Peca_Desconto"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Desconto"];
                        dtrPecasItens["Valor_Comissao"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Valor_Comissao"];
                    }

                    dtrPecasItens["Peca_QtMinimaVenda"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_QtMinimaVenda"];
                    dtrPecasItens["Estoque_Custo_Medio"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Estoque_Custo_Medio"];
                    dtrPecasItens["Peca_Preco_Custo_Reposicao"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Preco_Custo_Reposicao"];
                    dtrPecasItens["Estoque_Custo_Ultimo_Custo"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Estoque_Custo_Ultimo_Custo"];
                    dtrPecasItens["Quantidade"] = this.txtQuantidade.Text.DefaultInteger();

                    dtrPecasItens["Preco_Lista"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Preco_Lista"];
                    dtrPecasItens["Estoque_Custo_Reposicao_Efetivo"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Estoque_Custo_Reposicao_Efetivo"];
                    dtrPecasItens["Enum_Objeto_Tipo_ID"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Enum_Objeto_Tipo_ID"];

                    dtrPecasItens["Endereco_Peca"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Endereco_Peca"];
                    dtrPecasItens["Imposto"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Imposto"];
                    dtrPecasItens["Peca_Embalagem_Quantidade"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Embalagem_Quantidade"];

                    dtrPecasItens["Peca_Codigo_CFOP"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Codigo_CFOP"];
                    dtrPecasItens["Class_Fiscal_ICMS"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Class_Fiscal_ICMS"];
                    dtrPecasItens["Peca_Codigo_Situacao_Tributaria"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Codigo_Situacao_Tributaria"];
                    dtrPecasItens["Peca_ICMS_Substituicao_Tributaria"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_ICMS_Substituicao_Tributaria"];
                    dtrPecasItens["Peca_Percentual_Pis"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Percentual_Pis"];
                    dtrPecasItens["Peca_Percentual_Cofins"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Percentual_Cofins"];
                    dtrPecasItens["Peca_Codigo_Situacao_Tributaria_Pis"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Codigo_Situacao_Tributaria_Pis"];
                    dtrPecasItens["Peca_Codigo_Situacao_Tributaria_Cofins"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Codigo_Situacao_Tributaria_Cofins"];

                    dtrPecasItens["Class_Fiscal_CD"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Class_Fiscal_CD"];

                    dtrPecasItens["Peca_Codigo_Impressao_Fiscal"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Codigo_Impressao_Fiscal"];
                    dtrPecasItens["Peca_Descricao_Impressao_Fiscal"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Descricao_Impressao_Fiscal"];

                    dtrPecasItens["Peca_Origem_Mercadoria"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Peca_Origem_Mercadoria"];
                    dtrPecasItens["Class_Fiscal_IPI"] = dtsPeca.Tables["Peca_Venda_Embalagem"].Rows[0]["Class_Fiscal_IPI"];

                    this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows.Add(dtrPecasItens);

                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Buscar_Preencher_Servico_Por_Codigo_Barras()
        {

            try
            {
                DataSet dtsServico = new ServicoBUS().Consultar_Dados_Servico_para_Venda(this.txtCodigoItemFabricante.Text.PadLeft(4,'0'), this.intLojaID, 0);

                if (dtsServico != null && dtsServico.Tables["Itens_Servico"].Rows.Count > 0)
                {
                    this.txtQuantidade.Text = this.txtQuantidade.Text == string.Empty ? "1" : this.txtQuantidade.Text;

                    DataRow dtrPecasItens = this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].NewRow();

                    dtrPecasItens["Item"] = this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count + 1;
                    dtrPecasItens["Peca_CodBarra_CdBarras"] = this.txtCodigoItemFabricante.Text.Trim();

                    dtrPecasItens["Fabricante_ID"] = 0;
                    dtrPecasItens["Produto_ID"] = 0;
                    dtrPecasItens["Fabricante_CD"] = "    ";
                    dtrPecasItens["Produto_CD"] = "    ";
                    dtrPecasItens["Enum_Objeto_Tipo_ID"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Enum_Objeto_Tipo_ID"];
                    dtrPecasItens["Peca_ID"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_ID"];
                    dtrPecasItens["Peca_CD"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Cd"];
                    dtrPecasItens["Peca_DsTecnica"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Descricao"];
                    dtrPecasItens["Peca_QtMinimaVenda"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_QtMinimaVenda"];
                    dtrPecasItens["Quantidade"] = Convert.ToInt32(this.txtQuantidade.Text);
                    dtrPecasItens["Peca_Preco_Custo_Reposicao"] = 0;

                    if (this.blnIsFuncionario)
                    {
                        dtrPecasItens["Peca_Preco_Valor"] = (dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Preco_Valor"].DefaultDecimal()
                                                                * (1 - (this.dcmPercentualDescontoFuncionario / 100))).ToDecimalRound(2);
                        dtrPecasItens["Peca_Desconto"] = (dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Preco_Valor"].DefaultDecimal()
                                                                * (this.dcmPercentualDescontoFuncionario / 100)).ToDecimalRound(2);
                        dtrPecasItens["Valor_Comissao"] = (dtrPecasItens["Peca_Preco_Valor"].DefaultDecimal() * (dtsServico.Tables["Itens_Servico"].Rows[0]["Comissao_Percentual"].DefaultDecimal() / 100)).ToDecimalRound(2);
                    }
                    else
                    {
                        dtrPecasItens["Peca_Preco_Valor"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Preco_Valor"];
                        dtrPecasItens["Peca_Desconto"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Desconto"];
                        dtrPecasItens["Valor_Comissao"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Valor_Comissao"];
                    }

                    dtrPecasItens["Preco_Lista"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Preco_Lista"];
                    dtrPecasItens["Enum_Objeto_Tipo_ID"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Enum_Objeto_Tipo_ID"];
                    dtrPecasItens["Imposto"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Imposto"];
                    dtrPecasItens["Peca_Embalagem_Quantidade"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_QtMinimaVenda"];

                    dtrPecasItens["Peca_Codigo_CFOP"] = string.Empty;
                    dtrPecasItens["Class_Fiscal_ICMS"] = 0.0;
                    dtrPecasItens["Peca_Codigo_Situacao_Tributaria"] = string.Empty;
                    dtrPecasItens["Peca_ICMS_Substituicao_Tributaria"] = 0.0;
                    dtrPecasItens["Peca_Percentual_Pis"] = string.Empty;
                    dtrPecasItens["Peca_Percentual_Cofins"] = string.Empty;
                    dtrPecasItens["Peca_Codigo_Situacao_Tributaria_Pis"] = string.Empty;
                    dtrPecasItens["Peca_Codigo_Situacao_Tributaria_Cofins"] = string.Empty;

                    dtrPecasItens["Class_Fiscal_CD"] = string.Empty;

                    dtrPecasItens["Peca_Codigo_Impressao_Fiscal"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Cd"];
                    dtrPecasItens["Peca_Descricao_Impressao_Fiscal"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Servico_Descricao"];
                    dtrPecasItens["Comissao_Percentual"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Comissao_Percentual"];
                    dtrPecasItens["Produto_Reciclavel"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Produto_Reciclavel"];
                    dtrPecasItens["Produto_Reciclavel_Desconto"] = dtsServico.Tables["Itens_Servico"].Rows[0]["Produto_Reciclavel_Desconto"];

                    this.dtsConsultaPecaItens.Tables["Itens_Orcamento"].Rows.Add(dtrPecasItens);

                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private List<int> Localizar_ID_Pelo_Codigo_Barras()
        {
            try
            {
                List<int> colIDs = new List<int>();

                int intPecaID = this.Retornar_ID_Peca_Ou_Servico();

                if (intPecaID == 0)
                {
                    this.txtMenu.Content = "Não existe peça cadastrada";
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtCodigoItemFabricante.Focus();
                }

                DataRow[] colLinhas = this.Carregar_Itens_A_Cancelar(intPecaID);

                foreach (DataRow dtrLinha in colLinhas)
                {
                    colIDs.Add(dtrLinha["Item"].DefaultInteger());
                }

                return colIDs;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int Retornar_ID_Peca_Ou_Servico()
        {
            int intPecaID = 0;

            if (this.blnConsultarItemPorCodigoPecaouCodigoServico)
            {
                ServicoDO dtoServico = new ServicoBUS().Selecionar_Por_Codigo_Servico(this.txtComando.Text.PadLeft(4, '0').Trim(), Root.AcessoDoServidor.ServidorLocal);

                intPecaID = dtoServico == null ? 0 : dtoServico.Codigo.DefaultInteger();
            }
            else
            {
                DataRow dtrPecaCodBarra = new PecaBUS().Selecionar_DataRow_Por_Codigo_Barras(this.txtComando.Text.Trim(), Root.AcessoDoServidor.ServidorLocal);
                intPecaID = dtrPecaCodBarra == null ? 0 : dtrPecaCodBarra["Peca_ID"].DefaultInteger();
            }

            return intPecaID;
        }

        private DataRow[] Carregar_Itens_A_Cancelar(int intPecaID)
        {
            try
            {
                return this.dtsGridVenda.Tables["Venda_It"].Select("convert(Codigo,System.Int32) = '" + intPecaID.DefaultString() + "' AND Cancelado = False");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Romaneio            "

        private bool Incluir_Comanda(int intNumeroRomaneio)
        {
            try
            {
                DataRow dtrComanda = new Comanda_ExternaBUS().Selecionar(intNumeroRomaneio, Root.Loja_Ativa.ID);
                DataTable dttRomaneios = new Comanda_Externa_RomaneioBUS().Consultar_DataTable(dtrComanda["Lojas_ID"].DefaultInteger(), 
                                                                                                dtrComanda["Comanda_Externa_ID"].DefaultInteger(),
                                                                                                dtrComanda["Comanda_Externa_Sequencia"].DefaultInteger(), 0);
                foreach (DataRow dtrRomaneio in dttRomaneios.Rows)
                {
                    this.Incluir_Romaneio(dtrRomaneio["Romaneio_CT_ID"].DefaultInteger(), true);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Incluir_Romaneio(int intNumeroRomaneio, bool blnPesquisaPorComanda)
        {
            try
            {
                this.Buscar_Dados_Romaneio(intNumeroRomaneio);

                this.Identificar_Venda_Funcionario_Por_Romaneio();

                this.Incluir_Dados_Romaneio_DataSet(intNumeroRomaneio);

                this.Setar_Cliente();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Buscar_Dados_Romaneio(int intNumeroRomaneio)
        {
            try
            {
                this.dtsPreVendaEscolhido = new Romaneio_Pre_VendaBUS().Consultar_DataSet_Caixa_Novo_Pre_Venda_Pendente(Root.Loja_Ativa.ID, intNumeroRomaneio, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Buscar_Validar_Dados_Romaneio(int intNumeroRomaneio)
        {
            try
            {
                this.dtsPreVendaEscolhido = new Romaneio_Pre_VendaBUS().Consultar_DataSet_Caixa_Novo_Pre_Venda_Pendente(Root.Loja_Ativa.ID, intNumeroRomaneio, true);

                string strValidacao = this.Validar_Carregamento_Romaneio_PreVenda();

                if (strValidacao != string.Empty)
                {
                    this.txtMenu.Content = strValidacao;
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Incluir_Dados_Romaneio_DataSet(int intNumeroRomaneio)
        {
            try
            {
                DataRow[] dtrRomaneioDuplicado = this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Select("Romaneio_Pre_Venda_Ct_ID = " + intNumeroRomaneio.DefaultString());

                if (dtrRomaneioDuplicado.Length == 0)
                {
                    this.Preencher_DataRow_Romaneio_Pre_Venda_Ct(intNumeroRomaneio);
                    this.Preencher_DataRow_Romaneio_Pre_Venda_It(intNumeroRomaneio);
                    this.Preencher_DataRow_Tela_Inclusao_Romaneio(intNumeroRomaneio);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        #region "   Estorno     "

        private void Buscar_Dados_Romaneio_Estorno()
        {
            try
            {
                Estorno_Cartao_CreditoBUS_NEW busEstornoCartaoCredito = new Estorno_Cartao_CreditoBUS_NEW();

                this.dtsRomaneioEstorno = busEstornoCartaoCredito.Consultar_DataSet_Estorno_Propriedades_Para_Inclusao_Caixa_Novo(this.intRomaneioComanda, Root.Loja_Ativa.ID);

                this.intEstornoCartaoCreditoID = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows.Count > 0 ?
                    this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Estorno_Cartao_Credito_CT_ID"].ToInteger() : 0;

                this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Columns.Add("Cartao_Numero_Parte1", typeof(string));
                this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Columns.Add("Cartao_Numero_Parte2", typeof(string));

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Buscar_Romaneios_Estorno_Staus()
        {
            try
            {
                decimal dcmValorTotal = 0;
                decimal dcmValorEstornado = 0;
                decimal dcmValorEstornar = 0;

                this.Preencher_Valores_Estorno(ref dcmValorTotal, ref dcmValorEstornado, ref dcmValorEstornar);

                if (dcmValorTotal == dcmValorEstornado)
                {
                    this.blnEstornoFinalizado = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Valores_Estorno(ref Decimal dcmValorTotal, ref Decimal dcmValorEstornado, ref Decimal dcmValorEstornar)
        {
            try
            {
                dcmValorTotal = this.dtsRomaneioEstorno.Tables["Itens_Romaneio"].Compute("Sum(Romaneio_It_Preco_Total)", "Romaneio_It_Preco_Total > 0").ToString() == string.Empty
                    ? 0 : this.dtsRomaneioEstorno.Tables["Itens_Romaneio"].Compute("Sum(Romaneio_It_Preco_Total)", "Romaneio_It_Preco_Total > 0").ToDecimal();

                Romaneio_Pre_VendaBUS busRomaneioPreVenda = new Romaneio_Pre_VendaBUS();
                if (busRomaneioPreVenda.Verificar_Romaneio_Venda_Origem(this.intRomaneioComanda, this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_CT"].Rows[0]["Lojas_ID"].ToInteger()))
                {
                    dcmValorEstornado = dcmValorTotal;
                }
                else
                {
                    dcmValorEstornado = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Compute("Sum(Estorno_Cartao_Credito_IT_Valor_Estorno)", "Estorno_Cartao_Credito_IT_Valor_Estorno > 0").ToString() == string.Empty
                       ? 0 : this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Compute("Sum(Estorno_Cartao_Credito_IT_Valor_Estorno)", "Estorno_Cartao_Credito_IT_Valor_Estorno > 0").ToDecimal();
                }
                dcmValorEstornar = dcmValorTotal - dcmValorEstornado;
                dcmValorEstornar = dcmValorEstornar < 0 ? dcmValorEstornar * (-1) : dcmValorEstornar;

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #endregion

        #region "   Validação           "

        public bool Validar_Usuario_Mercadocar_AD(string strLogin, string strSenha)
        {
            try
            {
                bool blnUsuarioExiste = false;

                if (strSenha != string.Empty)
                {
                    PrincipalContext objValidateCredentials = new PrincipalContext(ContextType.Domain);
                    blnUsuarioExiste = objValidateCredentials.ValidateCredentials(strLogin, strSenha);
                }

                return blnUsuarioExiste;
            }
            catch (Exception ex)
            {
                if (ex.Source == "System.DirectoryServices" && ex.Message.Contains("Falha de logon: nome de usuário desconhecido ou senha incorreta.") || ex.Message.Contains("Logon failure: unknown user name or bad password."))
                {
                    return false;
                }
                else
                    throw;
            }
        }

        private bool Validar_Comanda(int intNumeroRomaneio)
        {
            try
            {
                // Verifica se o número da comanda informado é diferente ZERO
                if (intNumeroRomaneio == 0)
                {
                    this.txtMenu.Content = "O número da comanda não pode ser zero";
                    return false;
                }

                DataRow dtrComanda = new Comanda_ExternaBUS().Selecionar_Comanda_Externa_Com_Qtde_Romaneios_Vinculados(intNumeroRomaneio, Root.Loja_Ativa.ID);

                // Verifica se existe a comanda informada
                if (Convert.ToString(dtrComanda["Comanda_Externa_ID"]) == string.Empty || Convert.ToInt32(dtrComanda["Comanda_Externa_ID"]) == 0)
                {
                    this.txtMenu.Content = "Número de comanda não encontrada";
                    return false;
                }

                // Verifica se existe algum romaneio na comanda informada
                if (dtrComanda["Enum_Status_ID"].DefaultInteger() == Status_Comanda_Externa.Livre.DefaultInteger())
                {
                    this.txtMenu.Content = "A comanda informada não possui nenhum romaneio";
                    return false;
                }

                this.intQtdeRomaneiosComanda = dtrComanda["Qtde_Romaneios"].DefaultInteger();

                DataTable dttRomaneios = new Comanda_Externa_RomaneioBUS().Consultar_DataTable(dtrComanda["Lojas_ID"].DefaultInteger(), dtrComanda["Comanda_Externa_ID"].DefaultInteger(), dtrComanda["Comanda_Externa_Sequencia"].DefaultInteger(), 0);
                foreach (DataRow dtrRomaneio in dttRomaneios.Rows)
                {
                    if (!this.Validar_Romaneio(dtrRomaneio["Romaneio_CT_ID"].DefaultInteger()))
                    {
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

        private bool Validar_Romaneio(int intNumeroRomaneio)
        {
            try
            {
                // Verifica se caixa já não incluiu a pré-venda
                if (this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows).Length != 0)
                {
                    foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                    {
                        if (intNumeroRomaneio == Convert.ToInt32(dtrRomaneio["Romaneio_Pre_Venda_Ct_ID"]))
                        {
                            if (this.enuSituacao == Operacao.Romaneio)
                            {
                                this.txtMenu.Content = "O romaneio " + intNumeroRomaneio + " já foi incluído";
                            }
                            else
                            {
                                this.txtMenu.Content = "A comanda " + dtrRomaneio["Comanda_Externa_ID"].ToString() + " já foi incluída";
                            }
                            this.enuSituacao = Operacao.Operacao_Inicial;
                            return false;
                        }
                    }
                }

                if (!this.Buscar_Validar_Dados_Romaneio(intNumeroRomaneio))
                {
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Validar_Carregamento_Romaneio_PreVenda()
        {
            try
            {
                if (this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count == 0)
                {
                    return "O romaneio informado não existe";
                }

                if (Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Auto_Servico
                    && Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Resta
                    && Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Tecnica
                    && Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Telepreco
                    && Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Troca
                    && Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Especial
                    && Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Estorno)
                {
                    return "Operação inválida. Tipo de romaneio não identificado.";
                }

                this.blnUtilizaNFpRomaneio = false;
                // Se o romaneio é do tipo troca, resta ou estorno não solicita a NFp
                if (Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) == (Int32)TipoRomaneio.Troca
                    || Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) == (Int32)TipoRomaneio.Resta)
                {
                    this.blnUtilizaNFpRomaneio = true;
                }

                if (Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["Enum_Romaneio_Tipo_ID"]) == (Int32)TipoRomaneio.Estorno)
                {
                    this.blnUtilizaNFpRomaneio = true;
                    this.blnRomaneioEstorno = true;
                }

                // Romaneio só possui itens de serviço, não solicitar a NFp
                // Se comanda possui mais de um romaneio e o romaneio no momento é igual a serviço então emite NFp

                if (this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_It"].Rows.Count > 0)
                {
                    DataRow[] dtrRomaneioPreVendaIt = this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_It"].Select("Enum_Objeto_Tipo_ID = " + Convert.ToInt32(Tipo_Objeto.Servico).ToString());


                    if (this.intQtdeRomaneiosComanda > 1 && dtrRomaneioPreVendaIt.Length > 0)
                    {
                        this.blnUtilizaNFpRomaneio = false;
                    }
                    else
                    {
                        if (this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_It"].Rows.Count == dtrRomaneioPreVendaIt.Length)
                        {
                            this.blnUtilizaNFpRomaneio = true;
                        }
                    }
                }



                bool blnTemCredito = false;
                if (this.blnRomaneioEspecial)
                {
                    if (this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Rows.Count > 0)
                    {
                        foreach (DataRow dtrRegistro in this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_Ct"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                        {
                            if ((Int32)dtrRegistro["Enum_Romaneio_Tipo_ID"] == (Int32)Enumerados.TipoRomaneio.Troca
                                || (Int32)dtrRegistro["Enum_Romaneio_Tipo_ID"] == (Int32)Enumerados.TipoRomaneio.Resta)
                            {
                                blnTemCredito = true;
                            }
                        }
                    }

                    if (blnTemCredito & this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                    {
                        return "Operação inválida. Romaneio de crédito";
                    }

                    if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                    {
                        return "Operação inválida. Itens de Auto Serviço.";
                    }
                }

                bool blnPagtoDinheiro = false;
                bool blnVenda = false;
                int intPagtoDinheiro = 0;

                if (this.dtsPreVendaTemporario.Tables["Tela"].Rows.Count > 0)
                {
                    // Se tiver alguma compra com romaneio de troca autorizado a pagar em dinheiro, não deixar liberar
                    foreach (DataRow dtrRegistro in this.dtsPreVendaTemporario.Tables["Tela"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                    {
                        intPagtoDinheiro = Convert.ToInt32(dtrRegistro["PagtoDinheiro"]);
                        if (intPagtoDinheiro == 0)
                        {
                            blnVenda = true;
                        }
                        else
                        {
                            blnPagtoDinheiro = true;
                        }
                    }


                    intPagtoDinheiro = Convert.ToInt32(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Rows[0]["PagtoDinheiro"]);
                    if (intPagtoDinheiro == 0)
                    {
                        blnVenda = true;
                    }
                    else
                    {
                        blnPagtoDinheiro = true;
                    }

                    if (blnVenda == true & blnPagtoDinheiro == true)
                    {
                        return "Liberar a devolução em dinheiro separadamente.";
                    }
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Texto_Campo_Numerico(string strTexto)
        {
            try
            {
                if (strTexto.Length >= 10)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool Validar_Parcela()
        {
            try
            {
                if (this.txtComando.Text != string.Empty && Convert.ToInt32(this.txtComando.Text) != 0)
                {
                    DataRow[] dtrParcela = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Consumidor_Final"].Select(" Forma_Pagamento_ID = " + this.intFormaPagamentoID.ToString() + " AND Nr_Parcelas = " + this.txtComando.Text);
                    if (dtrParcela.Length > 0)
                    {
                        this.intCondicaoPagtoID = Convert.ToInt32(dtrParcela[0]["Condicao_Pagamento_ID"]);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        private string Validar_Valor_Parcela()
        {
            try
            {

                if (string.IsNullOrEmpty(this.txtComando.Text) || Convert.ToDecimal(this.txtComando.Text) == 0 || Convert.ToDecimal(this.txtComando.Text) < 0)
                {
                    return "Informe corretamente o valor recebido";
                }

                if (this.intFormaPagamentoID != Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO &&
                    Convert.ToDecimal(this.txtComando.Text) > Convert.ToDecimal(this.txtAPagar.Text))
                {
                    return "Valor recebido maior que a venda";
                }

                if (this.intFormaPagamentoID != Formas_Pagamento.ID_FORMA_PAGAMENTO_CREDITO)
                {
                    DataRow[] dtrParcela = this.dtsFormaXCliente.Tables["Forma_Pagamento_Cliente_Tipo"].Select(" Forma_Pagamento_ID = " + this.intFormaPagamentoID.ToString());
                    if (dtrParcela.Length == 0)
                    {
                        return "Informe corretamente o valor recebido";
                    }
                    this.intCondicaoPagtoID = Convert.ToInt32(dtrParcela[0]["Condicao_Pagamento_ID"]);
                }
                return string.Empty;
            }
            catch
            {
                throw;
            }
        }

        private string Validar_Valor_Solicitar_Documentos()
        {
            try
            {

                if (this.intFormaPagamentoID != Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO &&
                    Convert.ToDecimal(this.txtComando.Text) >= this.dcmParametroProcessoValorSolicitaDocumento &&
                    this.dcmParametroProcessoValorSolicitaDocumento > 0)
                {
                    this.blnSolicitarDocumento = true;
                    return "Anotar RG e telefone do cliente na via do cartão";
                }

                return string.Empty;
            }
            catch
            {
                throw;
            }
        }

        private bool Validar_Saldo_Pagamento()
        {
            try
            {
                if (this.dcmTotalVenda > 0)
                {
                    if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count == 0)
                    {
                        return true;
                    }

                    if (this.dcmTotalVenda > this.Calcular_Valor_Total_Pagamento())
                    {
                        return true;
                    }
                }
                else if (this.dcmTotalVenda == 0)
                {

                    if (this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows.Count > 0 || this.blnAcordoProdutoReciclavel)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        private bool Validar_Caixa_Aberto()
        {
            try
            {
                DataSet dtsCaixa = new CaixaBUS().Ver_DataSet_Caixa_Aberto(this.intLojaID, this.intUsuario, Dns.GetHostName().ToUpper());

                if (dtsCaixa.Tables["Caixa_Aberto_Nome_Estacao"].Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dtsCaixa.Tables["Caixa_Aberto_Nome_Estacao"].Rows[0]["Caixa_Operacao_Dia_Nome_Computador"])))
                    {
                        this.txtMenu.Content = "Caixa aberto na máquina " + Convert.ToString(dtsCaixa.Tables["Caixa_Aberto_Nome_Estacao"].Rows[0]["Caixa_Operacao_Dia_Nome_Computador"]);
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

        private bool Validar_Preenche_Saldo_Inicial()
        {
            try
            {
                if (!this.txtComando.Text.IsNumber())
                {
                    this.txtMenu.Content = "Valor informado inválido!";
                    return false;
                }
                if (this.txtComando.Text.ToDouble() > Constantes.Constantes_Caixa.VALOR_MAXIMO)
                {
                    this.txtMenu.Content = "O Saldo Inicial não pode ser superior a R$" + Constantes.Constantes_Caixa.VALOR_MAXIMO.ToString();
                    return false;
                }

                this.dcmSaldoInicial = this.txtComando.Text.ToDecimal();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Validar_Se_Houve_Concorrencia_Liberacao_Romaneio()
        {
            try
            {
                if (this.Venda_Possui_Romaneio())
                {
                    new RomaneioBUS().Verificar_Concorrencia_Liberacao_Romaneios(this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_Ct"].Select());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Valor_Sangria(string strValorSangria)
        {
            try
            {
                if (!strValorSangria.IsNumber())
                {
                    this.txtMenu.Content = "Valor informado inválido!";
                    return false;
                }
                if (strValorSangria.ToDouble() > Constantes.Constantes_Caixa.VALOR_MAXIMO)
                {
                    this.txtMenu.Content = "A sangria não pode ser superior a R$" + Constantes.Constantes_Caixa.VALOR_MAXIMO.ToString();
                    return false;
                }

                if (strValorSangria.ToDecimal() == 0)
                {
                    this.txtMenu.Content = "A sangria não pode ser R$0,00";
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Pagamento_Pendente_POS()
        {
            try
            {
                DataTable dttRomaneioVendaGrupo = new Romaneio_Venda_GrupoBUS().Selecionar_Romaneio_Venda_Grupo_Por_Usuario(this.dtoUsuario.ID, this.intLojaID);

                if (dttRomaneioVendaGrupo.Rows.Count > 0)
                {
                    DataRow[] dtrRomaneioVendaGrupo = dttRomaneioVendaGrupo.Select("Enum_Status_ID = " + Convert.ToString((int)Status_Romaneio_Venda_Grupo.Pendente_POS));

                    if (dtrRomaneioVendaGrupo.Length > 0)
                    {
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

        private bool Validar_Valor_Limite_Venda_SAT()
        {
            try
            {
                // Verificar se a operação é SAT e se o limite é superior ao paramentro
                if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT && this.dcmTotalVenda >= this.dcmParametroProcessoLimiteVendaSat)
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

        #region "   Estorno     "

        private bool Validar_Romaneios_Pagamento_Pendente()
        {
            try
            {
                string strMensagem = new Estorno_Cartao_CreditoBUS_NEW().Validar_Romaneios_Pagamento_Pendente(this.intRomaneioComanda, Root.Loja_Ativa.ID);

                if (strMensagem.Length > 0)
                {
                    this.txtMenu.Content = strMensagem;
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Estorno_Pagamento_Total()
        {
            try
            {
                DataRow[] dtPgtoEstornoGrid = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                if (dtPgtoEstornoGrid.Length > 0)
                {
                    // Só permite se o tipo de estorno for Total
                    if (dtPgtoEstornoGrid[0]["Enum_Romaneio_Tipo_Estorno_ID"].ToInteger().Equals(0))
                    {
                        if (dtPgtoEstornoGrid[0]["Operadora_Cartao_Regras_Tipo"].ToString() != "T")
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(dtPgtoEstornoGrid[0]["Enum_Romaneio_Tipo_Estorno_ID"]) != Convert.ToInt32(Enumerados.Romaneio_Tipo_Estorno.Estorno_Total))
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

        private bool Validar_Prazo_Realizar_Estorno_Cartao_Credito()
        {
            try
            {
                DataRow[] dtPgtoEstornoGrid = this.dtsRomaneioEstorno.Tables["Estorno_Cartao_Credito_IT"].Select("Numero_Registro = " + Convert.ToString(this.intItemPagamentoEstorno));

                if (dtPgtoEstornoGrid.Length > 0)
                {

                    int intQuantidadeDias = dtPgtoEstornoGrid[0]["Operadora_Cartao_Permite_Estorno_Dias_Limite"].ToInteger();

                    if (intQuantidadeDias.Equals(-1) == false)
                    {
                        DateTime dtmDataMaxima = dtPgtoEstornoGrid[0]["Estorno_Cartao_Credito_IT_Data_Venda_Origem"].ToDateTime().AddDays(intQuantidadeDias);

                        if (DateTime.Today > dtmDataMaxima)
                        {
                            this.txtMenu.Content = "A data máxima para o estorno ultrapassada.";
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

        private bool Validar_Se_Tipo_Processo_Inclusao()
        {
            try
            {
                if (this.intEstornoCartaoCreditoID == 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #endregion

        #region "   Cancelar Item Cupom Fiscal   "

        private bool Cancelar_Item_Cupom_Fiscal()
        {
            try
            {
                string strValidaCancelamento = string.Empty;
                strValidaCancelamento = this.Validar_Item_Cupom_Fiscal_Cancelar(this.txtComando.Text);

                string strNumItem = this.txtComando.Text.Trim();
                string strNumItemFiscal = string.Empty;

                if (!this.blnConsultarItemPorCodigoPecaouCodigoServico)
                {
                    DataRow[] dtrItem = this.dtsGridVenda.Tables["Venda_It"].Select("Item = " + this.txtComando.Text.Trim());
                    strNumItemFiscal = dtrItem[0]["Numero_Item_Cupom_Fiscal"].DefaultString();
                }

                if (strValidaCancelamento == string.Empty && this.Cancelar_Item_Cupom_Fiscal(strNumItem, strNumItemFiscal))
                {
                    this.txtMenu.Content = "Item " + strNumItem + " cancelado";
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;

                    this.txtCodigoItemFabricante.Focus();
                }

                else
                {
                    this.txtMenu.Content = strValidaCancelamento;
                    this.txtComando.Text = string.Empty;
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Cancelar_Item_Cupom_Fiscal(string strNumItem, string strNumItemCupomFiscal)
        {
            try
            {
                if (!this.blnConsultarItemPorCodigoPecaouCodigoServico)
                {
                    Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Interface_Cancelar_Item_Cupom_Fiscal(strNumItemCupomFiscal.Trim());

                    if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                    {
                        this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                        ;
                        return false;
                    }
                }

                // Insere o item cancelado no Grid
                DataRow[] dtrVendaItem = this.Buscar_Registro_Da_Venda_Por_Item_Ou_Codigo(strNumItem);

                DataRow dtrVendaItemCancelado = this.dtsGridVenda.Tables["Venda_It"].NewRow();

                dtrVendaItemCancelado["Codigo"] = Convert.ToString(dtrVendaItem[0]["Codigo"]);
                dtrVendaItemCancelado["Tipo_Objeto"] = (Int32)Tipo_Objeto.Peca;
                dtrVendaItemCancelado["Descricao"] = "Cancelamento do item " + strNumItem.Trim();
                dtrVendaItemCancelado["Imposto"] = dtrVendaItem[0]["Imposto"];
                dtrVendaItemCancelado["Qtde"] = Convert.ToInt32(dtrVendaItem[0]["Qtde"]);
                dtrVendaItemCancelado["Preco_Unitario"] = Convert.ToDecimal(dtrVendaItem[0]["Preco_Unitario"]) * -1;
                dtrVendaItemCancelado["Total"] = Convert.ToDecimal(dtrVendaItem[0]["Total"]) * -1;
                dtrVendaItemCancelado["IsCupomFiscal"] = this.blnConsultarItemPorCodigoPecaouCodigoServico ? false : true;
                dtrVendaItemCancelado["Desconto"] = 0;
                dtrVendaItemCancelado["Cor"] = new SolidColorBrush(Colors.Purple);

                this.dtsGridVenda.Tables["Venda_It"].Rows.Add(dtrVendaItemCancelado);
                if (this.objVendaItemOrc.Items.Count > 0)
                {
                    this.objVendaItemOrc.ScrollIntoView(this.objVendaItemOrc.Items[this.objVendaItemOrc.Items.Count - 1]);
                }

                // Atualiza o grid de Vendas
                dtrVendaItem[0]["Cancelado"] = true;
                dtrVendaItem[0]["Desconto"] = 0;

                // Atualiza o DataSet de Itens do Orçamento
                foreach (DataRow dtrOrcamentoItem in this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows)
                {
                    if ((this.blnConsultarItemPorCodigoPecaouCodigoServico && dtrVendaItem[0]["Codigo"].DefaultInteger() == dtrOrcamentoItem["Codigo"].DefaultInteger()
                            && dtrVendaItem[0]["Cancelado"].DefaultBool())
                            ||
                            ((Convert.ToString(dtrVendaItem[0]["Codigo"]) == Convert.ToString(dtrOrcamentoItem["Codigo"])
                            && Convert.ToString(dtrVendaItem[0]["Item"]) == Convert.ToString(dtrOrcamentoItem["Sequencial_Cupom"])
                            && (Convert.ToString(dtrVendaItem[0]["Qtde"]) == Convert.ToString(dtrOrcamentoItem["Orcamento_It_Qtde"])
                            || Convert.ToString(dtrVendaItem[0]["Qtde"]) == Convert.ToString(dtrOrcamentoItem["Peca_Embalagem_Quantidade"].ToInteger() * dtrOrcamentoItem["Orcamento_It_Qtde"].ToInteger())))))
                    {
                        dtrOrcamentoItem.Delete();
                        break;
                    }
                }

                this.Calcular_Valor_Total_Orcamento();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Validar_Item_Cupom_Fiscal_Cancelar(string strItemCupom)
        {
            try
            {
                // Só é permitido excluir o item de Auto - Serviço
                DataRow[] dtrVendaItem = this.Buscar_Registro_Da_Venda_Por_Item_Ou_Codigo(strItemCupom);

                if (dtrVendaItem.Length > 0)
                {
                    if (Convert.ToBoolean(dtrVendaItem[0]["Cancelado"]))
                    {
                        return "Operação inválida. Item já cancelado";
                    }
                    if (Convert.ToBoolean(dtrVendaItem[0]["IsRomaneio"]))
                    {
                        return "Operação inválida. Item de romaneio";
                    }
                }
                else
                {
                    return "Operação inválida. Item não localizado";
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataRow[] Buscar_Registro_Da_Venda_Por_Item_Ou_Codigo(string itemCupom)
        {
            try
            {
                string strFiltro = string.Empty;

                strFiltro = string.Concat(" Cancelado = false ",
                                          " And Item = ",
                                          itemCupom.Trim(),
                                          " And Codigo = ",
                                          this.intCodigoDoItemSendoCancelado);

                return this.dtsGridVenda.Tables["Venda_It"].Select(strFiltro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Validar_Impressao_Comprovante_Itens_A_Cancelar(List<int> colIDs)
        {
            try
            {
                int intQuantidadeLinhasNaoVisiveis = this.dtsGridVenda.Tables["Venda_It"].Rows.Count - QUANTIDADE_LINHAS_VISIVEIS;

                if (colIDs[0] <= intQuantidadeLinhasNaoVisiveis)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Redução Z e Leitura X        "

        private bool Validar_Reducao_Z()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar == null))
                {
                    return false;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Imprimir_Reducao_Z.ToString()) != true)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para redução Z";
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return false;
                }

                if (this.blnPortaImpressoraFiscal == false)
                {
                    if (!this.Abrir_Porta_Impressora_Fiscal())
                    {
                        this.txtMenu.Content = "Erro de comunicação da impressora fiscal.";
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

        private void Preencher_DataObject_Reducao_Z(Caixa_Reducao_ZDO dtoReducaoZ)
        {
            try
            {
                DateTime dtmData = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                dtoReducaoZ.Lojas_ID = Root.Loja_Ativa.ID;
                dtoReducaoZ.Usuario_Reducao_Z_ID = this.dtoUsuarioAutenticar.ID;
                dtoReducaoZ.Data_Processamento = dtmData;
                dtoReducaoZ.Nome_Computador = Environment.MachineName;
                dtoReducaoZ.Contador_Reducao_Z = 0;
                dtoReducaoZ.Numero_ECF = 0;
                dtoReducaoZ.COO_Inicial = 0;
                dtoReducaoZ.COO_Final = 0;
                dtoReducaoZ.Data_Movimento = new DateTime(1900, 1, 1);
                dtoReducaoZ.Serie_ECF = string.Empty;
                dtoReducaoZ.Nome_Arquivo = string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_DataObject_Reducao_MFD(Caixa_Reducao_MFDDO dtoReducaoMFD)
        {
            try
            {
                DateTime dtmData = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                dtoReducaoMFD.Lojas_ID = Root.Loja_Ativa.ID;
                dtoReducaoMFD.Usuario_Reducao_MFD_ID = this.dtoUsuarioAutenticar.ID;
                dtoReducaoMFD.Data_Processamento = dtmData;
                dtoReducaoMFD.Nome_Computador = Environment.MachineName;
                dtoReducaoMFD.Nome_Arquivo_NFP = string.Empty;
                dtoReducaoMFD.Arquivo_NFP = null;
                dtoReducaoMFD.Lote_NFP = string.Empty;
                dtoReducaoMFD.Lote_Codigo_NFP = string.Empty;
                dtoReducaoMFD.Lote_Descricao_NFP = string.Empty;
                dtoReducaoMFD.Data_Recebimento_NFP = new DateTime(1900, 1, 1);
                dtoReducaoMFD.Enum_Status_ID = Status_Caixa_NFP.Nao_Transmitido.ToInteger();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Reducao_Z()
        {
            try
            {
                if (this.Validar_Reducao_Z() == false)
                {
                    return false;
                }

                // Imprimir o relatório gerencial em branco para atualizar a data da impressora. Situação em que não houve venda.
                new CaixaBUS().Imprimir_Relatorio_Gerencial(this.objComunicacaoImpressoraFiscal, string.Empty, string.Empty, false, false, this.objTipoImpressoraFiscal, this.objImpressaoFiscal);

                Caixa_Reducao_ZDO dtoReducaoZ = new Caixa_Reducao_ZDO();
                this.Preencher_DataObject_Reducao_Z(dtoReducaoZ);

                Caixa_Reducao_MFDDO dtoReducaoMFD = new Caixa_Reducao_MFDDO();
                this.Preencher_DataObject_Reducao_MFD(dtoReducaoMFD);

                Caixa_Reducao_ZBUS busReducaoZ = new Caixa_Reducao_ZBUS();
                // Se a comunicação com a impressora for Ventana, deve preencher o objeto antes de imprimir a redução Z
                if (this.objComunicacaoImpressoraFiscal == Caixa_Comunicacao_Impressora_Fiscal.Ventana)
                {
                    // Preenche o objeto da Redução Z
                    busReducaoZ.Preencher_Dados_Reducao_Z(this.objImpressaoFiscal, dtoReducaoZ);
                }

                // Incluir a redução Z na base de dados, para que caso ocorra falha na impressora, fique registrado a tentativa da operação
                busReducaoZ.Incluir(dtoReducaoZ);

                // Imprime a redução Z
                string strMensagem = busReducaoZ.Impressao_Reducao_Z(this.objImpressaoFiscal);

                if (strMensagem != string.Empty)
                {
                    this.txtMenu.Content = strMensagem;
                    return false;
                }

                this.txtMenu.Content = "Aguarde. Extraindo o arquivo Nota Fiscal Paulista.";
                Utilitario.Processar_Mensagens_Interface_WPF();

                // Gera o arquivo MFD e grava a redução Z na base de dados.
                busReducaoZ.Importar_Dados_Extracao_MFD(dtoReducaoZ, dtoReducaoMFD, this.objComunicacaoImpressoraFiscal, this.objImpressaoFiscal, AppDomain.CurrentDomain.BaseDirectory);

                // Gerar arquivo NFP retroativo
                new Caixa_Reducao_MFDBUS().Processar_Arquivo_NFP_Retroativo_Pendente(this.objImpressaoFiscal, dtoReducaoZ.Lojas_ID, dtoReducaoZ.Serie_ECF, dtoReducaoZ.Numero_ECF);

                this.Fechar_Porta_Impressora_Fiscal();

                return true;
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private bool Validar_Leitura_X()
        {
            try
            {
                if (this.blnPortaImpressoraFiscal == false)
                {
                    if (!this.Abrir_Porta_Impressora_Fiscal())
                    {
                        this.txtMenu.Content = "Erro de comunicação da impressora fiscal.";
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

        private bool Leitura_X(ref string strHorarioVerao)
        {
            try
            {
                if (this.Validar_Leitura_X() == false)
                {
                    return false;
                }

                // Atualiza o horario de verão
                strHorarioVerao = this.Retorna_Mudanca_Horario_Verao();

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Impressao_Leitura_X();

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    return false;
                }

                // Fecha a porta de comunicação com a impressora.
                if (this.blnCaixaAberto == false)
                {
                    this.Fechar_Porta_Impressora_Fiscal();
                }
                return true;
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        #endregion

        #region "   Desconto no Item    "

        private string Validar_Item_Cupom_Fiscal_Desconto(string strItemCupom)
        {
            try
            {
                // Só é permitido dar desconto para item de Auto - Serviço
                if (this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count > 0)
                {
                    DataRow[] dtrVendaItem = this.dtsGridVenda.Tables["Venda_It"].Select("Item = " + strItemCupom.Trim());

                    if (dtrVendaItem.Length > 0)
                    {
                        if (Convert.ToBoolean(dtrVendaItem[0]["Cancelado"]))
                        {
                            return "Operação inválida. Item cancelado";
                        }

                        if (Convert.ToBoolean(dtrVendaItem[0]["IsRomaneio"]))
                        {
                            return "Operação inválida. Item de romaneio";
                        }
                        if (Convert.ToString(dtrVendaItem[0]["Desconto"]) != "0")
                        {
                            return "Operação inválida. Já há desconto no item " + strItemCupom.Trim();
                        }
                        if (dtrVendaItem[0]["Tipo_Objeto"].DefaultInteger() == Tipo_Objeto.Servico.DefaultInteger())
                        {
                            return "Operação inválida. Item de serviço não possui desconto. ";
                        }
                    }
                    else
                    {
                        return "Operação inválida. Item não localizado";
                    }
                }
                else
                {
                    return "Operação inválida. Item de romaneio";
                }
                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Confirmar_Desconto_Item_Cupom_Fiscal(string strValorItemCupom)
        {
            try
            {
                decimal dcmValotItemCupom = strValorItemCupom.DefaultDecimal();

                if (dcmValotItemCupom <= 0)
                {
                    return "Valor incorreto.";
                }

                DataRow[] dtrVendaItem = this.dtsGridVenda.Tables["Venda_It"].Select("Item = " + this.intItemDesconto.ToString());

                if (dtrVendaItem.Length > 0)
                {
                    if (dcmValotItemCupom > dtrVendaItem[0]["Preco_Unitario"].DefaultDecimal())
                    {
                        return "Operação Inválida. Valor superior ao original.";
                    }

                    if (dcmValotItemCupom == dtrVendaItem[0]["Preco_Unitario"].DefaultDecimal())
                    {
                        return "Operação Inválida. Valor igual ao original.";
                    }


                    return "Confirma valor R$ " + dcmValotItemCupom.ToString("N2") + " para o item " + this.intItemDesconto.ToString() + "? (S\\N)";
                }


                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Desconto_Item_Cupom_Fiscal(string strValorItemCupom)
        {
            try
            {
                decimal dcmValotItemCupom = strValorItemCupom.DefaultDecimal();

                if (dcmValotItemCupom <= 0)
                {
                    return "Valor incorreto.";
                }

                DataRow[] dtrVendaItem = this.dtsGridVenda.Tables["Venda_It"].Select("Item = " + this.intItemDesconto.ToString());

                if (dtrVendaItem.Length > 0)
                {
                    if (dcmValotItemCupom > Convert.ToDecimal(dtrVendaItem[0]["Preco_Unitario"]))
                    {
                        return "Operação Inválida. Valor superior ao original.";
                    }

                    if (dcmValotItemCupom == Convert.ToDecimal(dtrVendaItem[0]["Preco_Unitario"]))
                    {
                        return "Operação Inválida. Valor igual ao original.";
                    }

                    dtrVendaItem[0]["Desconto"] = (Convert.ToDecimal(dtrVendaItem[0]["Preco_Unitario"]) - dcmValotItemCupom) * Convert.ToInt32(dtrVendaItem[0]["Qtde"]);
                    dtrVendaItem[0]["Preco_Unitario"] = dcmValotItemCupom;
                    dtrVendaItem[0]["Total"] = Convert.ToDecimal(dcmValotItemCupom * Convert.ToInt32(dtrVendaItem[0]["Qtde"]));

                    if (this.dtsGridVenda.Tables["Venda_It"].Rows.Count == this.intItemDesconto)
                    {
                        this.txtValorProduto.Text = "R$ " + dcmValotItemCupom.ToString("#,##0.00");
                    }

                    foreach (DataRow dtrOrcamentoItem in this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows)
                    {
                        if (Convert.ToString(dtrVendaItem[0]["Codigo"]) == Convert.ToString(dtrOrcamentoItem["Codigo"])
                            && Convert.ToString(dtrVendaItem[0]["Item"]) == Convert.ToString(dtrOrcamentoItem["Sequencial_Cupom"])
                            && (Convert.ToString(dtrVendaItem[0]["Qtde"]) == Convert.ToString(dtrOrcamentoItem["Orcamento_It_Qtde"])
                            || Convert.ToString(dtrVendaItem[0]["Qtde"]) == Convert.ToString(dtrOrcamentoItem["Peca_Embalagem_Quantidade"].ToInteger() * dtrOrcamentoItem["Orcamento_It_Qtde"].ToInteger())))
                        {
                            dtrOrcamentoItem["Desconto_Caixa"] = dtrVendaItem[0]["Desconto"];
                            dtrOrcamentoItem["Usuario_Aprovacao_Desconto_ID"] = this.intUsuarioAprovacaoID;

                            dtrOrcamentoItem["Orcamento_It_Preco_Pago"] = dcmValotItemCupom;
                            dtrOrcamentoItem["Preco_Total"] = Convert.ToDecimal(dcmValotItemCupom * Convert.ToInt32(dtrOrcamentoItem["Orcamento_It_Qtde"]) * Convert.ToInt32(dtrOrcamentoItem["Peca_Embalagem_Quantidade"]));

                            break;
                        }
                    }

                    int intNumeroItemCupomFiscal = dtrVendaItem[0]["Numero_Item_Cupom_Fiscal"].DefaultInteger();

                    this.Aplicar_Desconto_Item(intNumeroItemCupomFiscal, dcmValotItemCupom);
                    this.Calcular_Valor_Total_Orcamento();

                    return "Desconto aplicado no item " + this.intItemDesconto.ToString();
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Ticket Estacionamento       "

        private string Validar_Loja_Estacionamento()
        {
            try
            {
                if (!this.blnControleCancela)
                {
                    return "Esta loja não possui controle de cancela.";
                }

                // Enviar o nome do formulario,frmValidar_Estacionamento, via string(fixo). Já que o mesmo pertence a outro projeto(MC_Formularios).
                if (!Root.Permissao.Obter_Permissao_Do_Usuario(Root.Funcionalidades.Usuario_Ativo, Root.Loja_Ativa.ID, "frmValidar_Estacionamento", Acao_Formulario.Abrir.ToString()))
                {
                    return "Sem permissão para validação de estacionamento";
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Validar_Codigo_Ticket(string strCodigoBarras)
        {
            try
            {
                if (!strCodigoBarras.Length.Equals(20))
                {
                    return "Erro na leitura do ticket, tente novamente.";
                }

                string strMensagemRetorno = new Controle_EstacionamentoBUS().Validar_Codigo_Ticket(strCodigoBarras);

                return "Ticket validado com sucesso. Validado até " + strMensagemRetorno.Substring(20, 18);

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   SiTef               "

        private void Setar_Imagem_Icone_Sitef(string strSource, string strTextToolTip)
        {
            try
            {
                if (imgStatusSitef.Dispatcher.Thread == Thread.CurrentThread)
                {
                    imgStatusSitef.Source = new BitmapImage(new Uri(strSource, UriKind.Relative));
                }
                else
                {
                    this.imgStatusSitef.Dispatcher.BeginInvoke(new DelegateSetarImagemIconeSitef(this.Setar_Imagem_Icone_Sitef), new object[] { strSource, strTextToolTip });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Sitef(ref SitefDO dtoSitef, DataRow dtrFormaPagamento)
        {
            try
            {
                if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count == 1)
                {
                    this.Preencher_Sitef_Unico_Pagamento(ref dtoSitef, dtrFormaPagamento);
                }
                else
                {
                    this.Preencher_Sitef_Multiplo_Pagamento(ref dtoSitef);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Sitef_Unico_Pagamento(ref SitefDO dtoSitef, DataRow dtrFormaPagamento)
        {
            try
            {
                StringBuilder stbData = new StringBuilder(Strings.Space(6));
                StringBuilder stbHora = new StringBuilder(Strings.Space(6));

                this.Preencher_Data_Hora_Fiscal(ref stbData, ref stbHora);

                dtoSitef.Lista_Transacoes.Add(new Sitef_TransacaoDO());
                dtoSitef.Multiplo_Pagamento = false;
                dtoSitef.Valor_Total_Venda = this.dcmTotalVenda.ToString();
                dtoSitef.Codigo_COO = this.Retornar_COO_Sitef();
                dtoSitef.Data_Transacao = stbData.ToString();
                dtoSitef.Hora_Transacao = stbHora.ToString();

                dtoSitef.Lista_Transacoes[0].Pagamento_Item = 1;

                if (Convert.ToString(dtrFormaPagamento["Condicao_Pagamento_Descricao"]).ToUpper().IndexOf(CARTAO_TEF_DS_FININVEST) > 0)
                {
                    dtoSitef.Lista_Transacoes[0].Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Fininvest;
                    dtoSitef.Lista_Transacoes[0].Modalidade_Pagamento = Formas_Pagamento_Descricao.FORMA_CARTAO;
                }
                else
                {
                    if (Convert.ToBoolean(dtrFormaPagamento["Forma_Pagamento_Emissao_Cheque"]) == true)
                    {
                        dtoSitef.Lista_Transacoes[0].Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cheque;
                        dtoSitef.Lista_Transacoes[0].Modalidade_Pagamento = Formas_Pagamento_Descricao.FORMA_CHEQUE;
                    }
                    else if (Convert.ToBoolean(dtrFormaPagamento["Forma_Pagamento_Emissao_Cartao_Debito"]) == true)
                    {
                        dtoSitef.Lista_Transacoes[0].Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Debito;
                        dtoSitef.Lista_Transacoes[0].Modalidade_Pagamento = this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT ? Formas_Pagamento_Descricao.FORMA_CARTAO_DEBITO : Formas_Pagamento_Descricao.FORMA_CARTAO_DEBITO_OPERADORA;
                    }
                    else if (Convert.ToBoolean(dtrFormaPagamento["Forma_Pagamento_Emissao_Cartao_Credito"]) == true)
                    {
                        dtoSitef.Lista_Transacoes[0].Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Credito;
                        dtoSitef.Lista_Transacoes[0].Modalidade_Pagamento = this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT ? Formas_Pagamento_Descricao.FORMA_CARTAO_CREDITO : Formas_Pagamento_Descricao.FORMA_CARTAO_CREDITO_OPERADORA;
                    }
                }

                dtoSitef.Lista_Transacoes[0].Valor_Transacao = this.dcmTotalVenda.ToString("#,##0.00");
                dtoSitef.Imprime_Somente_Romaneio = this.blnRomaneioEspecial;

                dtoSitef.Lista_Transacoes[0].Numero_Cupom = this.Retornar_COO();
                dtoSitef.Lista_Transacoes[0].Operador = this.dtoUsuario.Login;

                if (this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows[0]["Numero_de_Parcelas"].ToString().Length == 1)
                {
                    dtoSitef.Lista_Transacoes[0].Numero_Parcelas = "0" + this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows[0]["Numero_de_Parcelas"].ToString();
                }
                else
                {
                    dtoSitef.Lista_Transacoes[0].Numero_Parcelas = Convert.ToString(this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows[0]["Numero_de_Parcelas"]);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Sitef_Multiplo_Pagamento(ref SitefDO dtoSitef)
        {
            try
            {
                string[] strCamposTabela = new string[8];

                strCamposTabela[0] = "Condicao_Pagamento_ID";
                strCamposTabela[1] = "Condicao_Pagamento_Nome";
                strCamposTabela[2] = "Romaneio_Pagamento_Venda_Liberada_Valor_Informado";
                strCamposTabela[3] = "Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas";
                strCamposTabela[4] = "Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito";
                strCamposTabela[5] = "Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito";
                strCamposTabela[6] = "Romaneio_Pagamento_Venda_Liberada_emite_Cheque";
                strCamposTabela[7] = "Item";

                DataTable dttPagamentoSitef = this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].DefaultView.ToTable(true, strCamposTabela);

                StringBuilder stbData = new StringBuilder(Strings.Space(6));
                StringBuilder stbHora = new StringBuilder(Strings.Space(6));

                this.Preencher_Data_Hora_Fiscal(ref stbData, ref stbHora);

                var objSitefPagamento = dtoSitef;
                objSitefPagamento.Multiplo_Pagamento = true;
                objSitefPagamento.Imprime_Somente_Romaneio = this.blnRomaneioEspecial;
                objSitefPagamento.Valor_Total_Venda = this.dcmTotalVenda.ToString();
                objSitefPagamento.Codigo_COO = this.Retornar_COO_Sitef();
                objSitefPagamento.Data_Transacao = stbData.ToString();
                objSitefPagamento.Hora_Transacao = stbHora.ToString();
                foreach (DataRow dtrPagamento in dttPagamentoSitef.Rows)
                {
                    Sitef_TransacaoDO dtoTransacao = new Sitef_TransacaoDO();

                    dtoTransacao.Pagamento_Item = dtrPagamento["Item"].DefaultInteger();
                    dtoTransacao.Valor_Transacao = Convert.ToDecimal(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Valor_Informado"]).ToString("#,##0.00");
                    dtoTransacao.Numero_Cupom = this.Retornar_COO();
                    dtoTransacao.Operador = this.dtoUsuario.Login;

                    if (Convert.ToString(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas"]).Length == 1)
                    {
                        dtoTransacao.Numero_Parcelas = "0" + Convert.ToString(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas"]);
                    }
                    else
                    {
                        dtoTransacao.Numero_Parcelas = Convert.ToString(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas"]);
                    }

                    if (Convert.ToString(dtrPagamento["Condicao_Pagamento_Nome"]).ToUpper().IndexOf(CARTAO_TEF_DS_FININVEST) > 0)
                    {
                        dtoTransacao.Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Fininvest;
                        dtoTransacao.Modalidade_Pagamento = Formas_Pagamento_Descricao.FORMA_CARTAO;
                    }
                    else
                    {
                        if (Convert.ToBoolean(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_emite_Cheque"]) == true)
                        {
                            dtoTransacao.Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cheque;
                            dtoTransacao.Modalidade_Pagamento = Formas_Pagamento_Descricao.FORMA_CHEQUE;
                        }
                        else if (Convert.ToBoolean(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito"]) == true)
                        {
                            dtoTransacao.Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Debito;
                            dtoTransacao.Modalidade_Pagamento = this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT ? Formas_Pagamento_Descricao.FORMA_CARTAO_DEBITO : Formas_Pagamento_Descricao.FORMA_CARTAO_DEBITO_OPERADORA;
                        }
                        else if (Convert.ToBoolean(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito"]) == true)
                        {
                            dtoTransacao.Codigo_Modalidade_Pagamento = (Int32)Modalidade_Pagamento_Sitef.Cartao_Credito;
                            dtoTransacao.Modalidade_Pagamento = this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT ? Formas_Pagamento_Descricao.FORMA_CARTAO_CREDITO : Formas_Pagamento_Descricao.FORMA_CARTAO_CREDITO_OPERADORA;
                        }
                    }

                    objSitefPagamento.Lista_Transacoes.Add(dtoTransacao);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Verificar_Transacaoes_Pendentes_Sitef()
        {
            try
            {
                SitefDO dtoSitef = new SitefDO(ref this.objImpressaoFiscal, this.objComunicacaoImpressoraFiscal, this.objTipoImpressoraFiscal);

                dtoSitef.Configura_Sitef();

                string strMensagemSitef = dtoSitef.Verifica_Transacoes_Pendentes();
                if (strMensagemSitef != string.Empty)
                {
                    txtMenu.Content = strMensagemSitef;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Liberação - Novas tabelas   "
        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche dados e grava as informações da liberação da venda nas 
        ///     novas tabelas de romaneio. Possui tratamento de erro especial.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Liberar_Venda_Novas_Tabelas()
        {
            try
            {
                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Inicio de gravação da liberação de uma venda para as tabelas novas.");
                // =======================================================================

                DataSet dtsRomaneioVendaTemporario = this.Preencher_Romaneio_Venda_Liberacao(this.blnParametroProcessoCriarLog);
                /// P0050.3 - NOTA: Liberação da Venda 
                new Romaneio_VendaBUS().Liberar_Venda_Caixa(dtsRomaneioVendaTemporario);

                // Atualiza DataObject do Sat
                this.Atualizar_DataObject_Sat_Venda_Romaneio(dtsRomaneioVendaTemporario);

                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Fim da gravação.");
                // =======================================================================
            }
            catch (Exception ex)
            {
                // =================================================================================
                Log.Erro("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, ex);
                // =================================================================================

            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche os dados no DataSet no Romaneio de Venda para a liberação 
        ///     do romaneio.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private DataSet Preencher_Romaneio_Venda_Liberacao(Boolean blnParametroProcessoCriarLog)
        {
            try
            {
                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Criar dataset de romaneio de venda.");
                // =======================================================================

                DataSet dtsRomaneioVendaTemporario = this.Criar_DataSet_Romaneio_Venda_Liberacao();

                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Preencher as informações da capa.");
                // =======================================================================

                this.Preencher_Romaneio_Venda_Capa_Liberacao(dtsRomaneioVendaTemporario, blnParametroProcessoCriarLog);

                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Preencher as informações dos itens.");
                // =======================================================================

                this.Preencher_Romaneio_Venda_Itens_Liberacao(dtsRomaneioVendaTemporario);

                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Preencher as informações do grupo.");
                // =======================================================================

                this.Preencher_Romaneio_Venda_Grupo(dtsRomaneioVendaTemporario);

                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Preencher as informações do pagamento de venda.");
                // =======================================================================
                this.Preencher_Romaneio_Venda_Pagamento(dtsRomaneioVendaTemporario);

                // =======================================================================
                Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Preencher as informações do Sat da venda.");
                // =======================================================================
                this.Preencher_Sat_Venda(dtsRomaneioVendaTemporario);

                return dtsRomaneioVendaTemporario;

            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método cria o DataSet de Romaneio de Venda adicionando as tabelas de 
        ///     Romaneio Venda Grupo E Romaneio Venda Pagamento.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private DataSet Criar_DataSet_Romaneio_Venda_Liberacao()
        {
            try
            {
                DataSet dtsRomaneioVendaTemporario = new DataSet();

                Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();
                dtsRomaneioVendaTemporario.Tables.Add(busRomaneioVenda.Retornar_Estrutura_Tabela().Copy());
                dtsRomaneioVendaTemporario.Tables.Add(busRomaneioVenda.Retornar_Estrutura_Tabela_Item().Copy());

                ///Coluna de ligação da venda com os itens.
                dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_IT"].Columns.Add("Romaneio_Pre_Venda_CT_ID", typeof(Int64));

                dtsRomaneioVendaTemporario.Tables.Add(new Romaneio_Venda_GrupoBUS().Retornar_Estrutura_Tabela().Copy());

                dtsRomaneioVendaTemporario.Tables.Add(new Romaneio_Venda_PagamentoBUS().Retornar_Estrutura_Tabela().Copy());

                dtsRomaneioVendaTemporario.Tables.Add(new SAT_VendaBUS().Retornar_Estrutura_Tabela().Copy());

                return dtsRomaneioVendaTemporario;
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método cria o DataSet de Romaneio de Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Capa_Liberacao(DataSet dtsRomaneioVendaTemporario, Boolean blnParametroProcessoCriarLog)
        {
            try
            {
                foreach (DataRow dtrRomaneioCTItem in this.dtsRomaneioTemporario.Tables["Romaneio_Ct"].Rows)
                {
                    Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();

                    // =======================================================================
                    Log.Info("LOG_LIBERACAO_REFORMULACAO_CAIXA_FORM", Root.Loja_Ativa.ID, "Incluir informações do romaneio: " + dtrRomaneioCTItem["Romaneio_Ct_ID"].ToString());
                    // =======================================================================

                    DataRow dtrRomaneioVendaCT = busRomaneioVenda.Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_CT(dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_CT"]);

                    dtrRomaneioVendaCT["Lojas_ID"] = dtrRomaneioCTItem["Lojas_ID"];
                    dtrRomaneioVendaCT["Cliente_ID"] = dtrRomaneioCTItem["Cliente_ID"];
                    dtrRomaneioVendaCT["Pessoa_Autorizada_ID"] = dtrRomaneioCTItem["Pessoa_Autorizada_ID"];
                    dtrRomaneioVendaCT["Enum_Tipo_ID"] = dtrRomaneioCTItem["Enum_Romaneio_Tipo_ID"];
                    dtrRomaneioVendaCT["Enum_Status_ID"] = busRomaneioVenda.Retorna_Status_Romaneio_Venda((StatusRomaneioVenda)dtrRomaneioCTItem["Enum_Romaneio_Status_ID"]);
                    dtrRomaneioVendaCT["Condicao_Pagamento_ID"] = dtrRomaneioCTItem["Condicao_Pagamento_ID"];
                    dtrRomaneioVendaCT["Usuario_Vendedor_ID"] = dtrRomaneioCTItem["Usuario_Vendedor_ID"];
                    dtrRomaneioVendaCT["Usuario_Gerente_ID"] = dtrRomaneioCTItem["Usuario_Gerente_ID"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Valor_Pago"] = dtrRomaneioCTItem["Romaneio_CT_Valor_Total_Pago"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Valor_Lista"] = dtrRomaneioCTItem["Romaneio_CT_Valor_Total_Lista"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_CNPJCPF"] = dtrRomaneioCTItem["Romaneio_Ct_Cliente_CNPJCPF"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_Nome"] = dtrRomaneioCTItem["Romaneio_Ct_Cliente_Nome"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_Telefone"] = dtrRomaneioCTItem["Romaneio_Ct_Cliente_Telefone"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Data_Geracao"] = dtrRomaneioCTItem["Romaneio_Ct_Data_Geracao"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_Data_Liberacao"] = this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_Data_Liberacao"];

                    ///P0050.3 - NOTA: Campo temporário. Gravação do ID da Pré-Venda
                    dtrRomaneioVendaCT["Romaneio_Pre_Venda_CT_ID"] = dtrRomaneioCTItem["Romaneio_Ct_ID"];
                    dtrRomaneioVendaCT["Romaneio_Venda_CT_ID"] = busRomaneioVenda.Selecionar_Romaneio_Venda_CT_ID_Por_Pre_Venda(dtrRomaneioCTItem["Romaneio_Ct_ID"].ToInteger(), dtrRomaneioCTItem["Lojas_ID"].ToInteger());

                    if (dtrRomaneioCTItem["Enum_Romaneio_Tipo_ID"].DefaultInteger() == TipoRomaneio.Auto_Servico.DefaultInteger()
                        && dtrRomaneioCTItem["Romaneio_Ct_Cliente_CNPJCPF"].DefaultString() != string.Empty
                        && dtrRomaneioCTItem["Cliente_ID"].DefaultString() != Constantes.Constantes_Caixa.ID_CONSUMIDOR_FINAL.DefaultString())
                    {
                        dtrRomaneioVendaCT["Romaneio_Venda_CT_Cliente_Identificado_Caixa"] = 1;
                    }

                    dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_CT"].Rows.Add(dtrRomaneioVendaCT);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche os dados dos itens do Romaneio Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Itens_Liberacao(DataSet dtsRomaneioVendaTemporario)
        {
            try
            {
                Romaneio_VendaBUS busRomaneioVenda = new Romaneio_VendaBUS();

                foreach (DataRow dtrRomaneioIT in this.dtsRomaneioTemporario.Tables["Romaneio_It"].Rows)
                {
                    DataRow dtrRomaneioVendaIT = busRomaneioVenda.Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_IT(dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_IT"]);

                    dtrRomaneioVendaIT["Lojas_ID"] = dtrRomaneioIT["Lojas_ID"];
                    dtrRomaneioVendaIT["Objeto_ID"] = dtrRomaneioIT["Objeto_ID"];
                    dtrRomaneioVendaIT["Enum_Objeto_Tipo_ID"] = dtrRomaneioIT["Enum_Objeto_Tipo_ID"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Sequencial"] = dtrRomaneioIT["Romaneio_It_Sequencial"].DefaultInteger();
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Qtde"] = dtrRomaneioIT["Romaneio_It_Qtde"].DefaultInteger();
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Preco_Pago"] = dtrRomaneioIT["Romaneio_It_Preco_Pago"].DefaultDecimal();
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Preco_Lista"] = dtrRomaneioIT["Romaneio_It_Preco_Lista"].DefaultDecimal();
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Valor_Desconto"] = dtrRomaneioIT["Romaneio_It_Valor_Desconto"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Valor_Comissao"] = dtrRomaneioIT["Romaneio_It_Valor_Comissao"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Instalado"] = dtrRomaneioIT["Romaneio_IT_Instalado"];
                    dtrRomaneioVendaIT["Romaneio_Pre_Venda_CT_ID"] = dtrRomaneioIT["Romaneio_Ct_ID"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Valor_Comissao_Telepreco"] = dtrRomaneioIT["Romaneio_It_Valor_Comissao_Telepreco"];
                    dtrRomaneioVendaIT["Usuario_Telepreco_ID"] = dtrRomaneioIT["Usuario_Telepreco_ID"].ToString().Equals(string.Empty) ? 0 : dtrRomaneioIT["Usuario_Telepreco_ID"];
                    dtrRomaneioVendaIT["Romaneio_Telepreco_IT_ID"] = dtrRomaneioIT["Romaneio_Telepreco_IT_ID"].ToString().Equals(string.Empty) ? 0 : dtrRomaneioIT["Romaneio_Telepreco_IT_ID"];
                    dtrRomaneioVendaIT["Lojas_Telepreco_ID"] = dtrRomaneioIT["Lojas_Telepreco_ID"].ToString().Equals(string.Empty) ? 0 : dtrRomaneioIT["Lojas_Telepreco_ID"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Telepreco_Identificado_Caixa"] = dtrRomaneioIT["Romaneio_IT_Telepreco_Identificado_Caixa"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Cod_Barras_Digitado"] = dtrRomaneioIT["Item_Digitado"];
                    dtrRomaneioVendaIT["Usuario_Aprovacao_Desconto_ID"] = dtrRomaneioIT["Usuario_Aprovacao_Desconto_ID"];
                    dtrRomaneioVendaIT["Romaneio_Venda_IT_Valor_Desconto_Caixa"] = dtrRomaneioIT["Desconto_Caixa"];

                    dtrRomaneioVendaIT["Romaneio_Venda_CT_ID"] = busRomaneioVenda.Selecionar_Romaneio_Venda_CT_ID_Por_Pre_Venda(dtrRomaneioIT["Romaneio_Ct_ID"].ToInteger(), dtrRomaneioIT["Lojas_ID"].ToInteger());

                    dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_IT"].Rows.Add(dtrRomaneioVendaIT);

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método cria o DataSet de Romaneio de Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Grupo(DataSet dtsRomaneioVendaTemporario)
        {
            try
            {
                
                DataRow dtrRomaneioVendaGrupo = new Romaneio_Venda_GrupoBUS().Retornar_DataRow_Com_Valores_Padrao_Romaneio_Venda_Grupo(dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_Grupo"]);
                DataRow dtrRomaneioDocumento = this.dtsPreVendaTemporario.Tables["Romaneio_Documento"].Rows[0];
                DataRow dtrRomaneioGrupo = this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0];

                Decimal dcmRomaneioVendaGrupoValorPago = 0;
                Decimal dcmRomaneioVendaGrupoValorLista = 0;

                dcmRomaneioVendaGrupoValorPago = (decimal)dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_CT"].Compute("SUM(Romaneio_Venda_CT_Valor_Pago)", string.Empty);
                dcmRomaneioVendaGrupoValorLista = (decimal)dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_CT"].Compute("SUM(Romaneio_Venda_CT_Valor_Lista)", string.Empty);

                dtrRomaneioVendaGrupo["Lojas_ID"] = dtrRomaneioDocumento["Lojas_ID"];
                dtrRomaneioVendaGrupo["Enum_Documento_Tipo_ID"] = dtrRomaneioDocumento["Enum_Documento_Tipo_ID"];
                dtrRomaneioVendaGrupo["Enum_Status_ID"] = this.blnPendentePOS ? Status_Romaneio_Venda_Grupo.Pendente_POS : Status_Romaneio_Venda_Grupo.Liberado;
                dtrRomaneioVendaGrupo["Cliente_ID"] = dtrRomaneioGrupo["Cliente_ID"];
                dtrRomaneioVendaGrupo["Usuario_Caixa_ID"] = dtrRomaneioGrupo["Usuario_Caixa_ID"];
                if (blnEnvioVendaSatRealizada)
                {
                    dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Numero_Documento"] = this.dtoCaixaSatVenda.Numero_Documento;
                }
                else
                {
                    dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Numero_Documento"] = this.strDocumentoNumero;
                }
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Data_Liberacao"] = dtrRomaneioGrupo["Romaneio_Grupo_Data_Liberacao"];
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Valor_Pago"] = dcmRomaneioVendaGrupoValorPago;
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Valor_Lista"] = dcmRomaneioVendaGrupoValorLista;
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Valor_Desconto_Comercial"] = dtrRomaneioGrupo["Romaneio_Grupo_Valor_Desconto_Comercial"];
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Valor_Credito_Utilizado"] = dtrRomaneioGrupo["Romaneio_Grupo_Valor_Credito_Utilizado"];
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Cliente_CNPJCPF"] = dtrRomaneioGrupo["Romaneio_Grupo_Cliente_CNPJCPF"];
                dtrRomaneioVendaGrupo["Romaneio_Grupo_ID"] = dtrRomaneioGrupo["Romaneio_Grupo_ID"];
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Data_Movimento"] = dtrRomaneioDocumento["Romaneio_Documento_Data_Movimento"];
                dtrRomaneioVendaGrupo["Romaneio_Venda_Grupo_Numero_Guiche"] = this.intNumeroGuiche;

                dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_Grupo"].Rows.Add(dtrRomaneioVendaGrupo);

            }
            catch (Exception)
            {
                throw;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        ///     Método preenche os dados dos itens do Romaneio Venda.
        /// </summary>
        /// <history>
        /// 	[mmukuno] 	07/01/2014 Created
        /// </history>
        ///-----------------------------------------------------------------------------
        private void Preencher_Romaneio_Venda_Pagamento(DataSet dtsRomaneioVendaTemporario)
        {
            try
            {
                foreach (DataRow dtrRomaneioPagamentoVendaLiberada in this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows)
                {
                    DataRow dtrRomaneioVendaPagamento = dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_Pagamento"].NewRow();

                    dtrRomaneioVendaPagamento["Lojas_ID"] = dtrRomaneioPagamentoVendaLiberada["Lojas_ID"];
                    dtrRomaneioVendaPagamento["Condicao_Pagamento_ID"] = dtrRomaneioPagamentoVendaLiberada["Condicao_Pagamento_ID"];
                    dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Valor"] = dtrRomaneioPagamentoVendaLiberada["Romaneio_Pagamento_Venda_Liberada_Valor"];
                    dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Dias_Parcela"] = dtrRomaneioPagamentoVendaLiberada["Romaneio_Pagamento_Venda_Liberada_Dia_Parcela"];
                    dtrRomaneioVendaPagamento["Usuario_Ultima_Alteracao_ID"] = this.intUsuario;
                    dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Data_Vencimento"] = dtrRomaneioPagamentoVendaLiberada["Romaneio_Pagamento_Venda_Liberada_Data"];

                    dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_Data_Ultima_Alteracao"] = new DateTime(1900, 1, 1);
                    dtrRomaneioVendaPagamento["Romaneio_Venda_Grupo_ID"] = 0;
                    dtrRomaneioVendaPagamento["Romaneio_Venda_Pagamento_ID"] = 0;
                    dtrRomaneioVendaPagamento["Cliente_Tipo_ID"] = 0;
                    dtrRomaneioVendaPagamento["Fatura_ID"] = 0;
                    dtrRomaneioVendaPagamento["Fatura_CD"] = 0;
                    dtrRomaneioVendaPagamento["Romaneio_Detalhe_Cartao_ID"] = dtrRomaneioPagamentoVendaLiberada["Romaneio_Detalhe_Cartao_ID"];

                    dtsRomaneioVendaTemporario.Tables["Romaneio_Venda_Pagamento"].Rows.Add(dtrRomaneioVendaPagamento);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Preencher_Sat_Venda(DataSet dtsRomaneioVendaTemporario)
        {
            try
            {
                if (this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    return;
                }

                DataRow dtrSatVenda = dtsRomaneioVendaTemporario.Tables["SAT_Venda"].NewRow();

                dtrSatVenda["Lojas_ID"] = this.dtoCaixaSatVenda.Lojas_ID;
                dtrSatVenda["SAT_Venda_ID"] = this.dtoCaixaSatVenda.SAT_Venda_ID;

                dtsRomaneioVendaTemporario.Tables["SAT_Venda"].Rows.Add(dtrSatVenda);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Atualizar_DataObject_Sat_Venda_Romaneio(DataSet dtsRomaneioVenda)
        {
            try
            {
                if (this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    return;
                }

                if (dtsRomaneioVenda.Tables["SAT_Venda"].Rows.Count > 0)
                {
                    this.dtoCaixaSatVenda.Romaneio_Venda_Grupo_ID = dtsRomaneioVenda.Tables["SAT_Venda"].Rows[0]["Romaneio_Venda_Grupo_ID"].DefaultInteger();

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private int Ultima_Linha_Orcamento()
        {
            return this.dtsOrcamentoIt.Tables["Orcamento_It"].Rows.Count - 1;
        }

        #endregion

        #region "   Imprimir Comprovantes       "

        #region "   Sangria         "

        private bool Processar_Impressao_Comprovante_Sangria(DateTime dtmCaixaOperacaoDataHoraOperacao, int intCaixaSangriaID, bool blnImprimeValorSangria)
        {
            try
            {
                string strImpressaoRetorno = this.Impressao_Comprovante_Sangria(this.Montar_Comprovante_Sangria(dtmCaixaOperacaoDataHoraOperacao, blnImprimeValorSangria), intCaixaSangriaID);

                if (strImpressaoRetorno != string.Empty)
                {
                    this.txtMenu.Content = "Falha imprimir Sangria." + strImpressaoRetorno;
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Montar_Comprovante_Sangria(DateTime dtmCaixaOperacaoDataHoraOperacao, bool blnImprimeValorSangria)
        {
            try
            {

                return new CaixaBUS().Montar_Comprovante_Sangria(dtmCaixaOperacaoDataHoraOperacao, Root.Loja_Ativa.Nome, this.dtoUsuario.Nome_Completo, this.dtoUsuarioAutenticar.Nome_Completo, this.dcmSangriaValorFiscal, blnImprimeValorSangria);

            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion

        #region "   Fechamento      "

        private bool Processar_Impressao_Comprovante_Fechamento(DataTable dttCaixaFechamento, DataTable dttCaixaFechamentoDetalhes)
        {
            try
            {
                string strCabecalho = string.Empty;
                string strPagamentos = string.Empty;
                string strTotalizador = string.Empty;

                StringBuilder stbCabecalho = new StringBuilder();
                StringBuilder stbPagamentos = new StringBuilder();
                StringBuilder stbTotalizador = new StringBuilder();

                this.Montar_Comprovante_Fechamento(dttCaixaFechamento, dttCaixaFechamentoDetalhes, ref stbCabecalho, ref stbPagamentos, ref stbTotalizador);

                strCabecalho = stbCabecalho.ToString();
                strPagamentos = stbPagamentos.ToString();
                strTotalizador = stbTotalizador.ToString();

                DataRow dtrFechamentoCaixa = dttCaixaFechamento.Rows[0];

                string strImpressaoRetorno = this.Impressao_Comprovante_Fechamento(strCabecalho, strPagamentos, strTotalizador, Convert.ToInt32(dtrFechamentoCaixa["Caixa_Fechamento_ID"]));

                if (strImpressaoRetorno != string.Empty)
                {
                    this.txtMenu.Content = "Falha imprimir Fechamento." + strImpressaoRetorno;
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Montar_Comprovante_Fechamento(DataTable dttCaixaFechamento, DataTable dttCaixaFechamentoDetalhes, ref StringBuilder stbCabecalho, ref StringBuilder stbPagamentos, ref StringBuilder stbTotalizador)
        {
            try
            {
                DataRow dtrFechamentoCaixa = dttCaixaFechamento.Rows[0];

                stbCabecalho.Append("Data Fechamento: " + dtrFechamentoCaixa["Caixa_Fechamento_Data_Fim"] + "\r\n");

                stbCabecalho.Append("Loja: " + Root.Loja_Ativa.Nome + "\r\n");

                stbCabecalho.Append("Caixa: " + this.dtoUsuario.Nome_Completo + "\r\n");

                stbCabecalho.Append("Fiscal: " + this.dtoUsuarioAutenticar.Nome_Completo + "\r\r\n\n");

                stbCabecalho.Append("Forma de Pagamento" + "\r\r\n");

                stbCabecalho.Append("Sistema               Informado" + "\r\n");

                Decimal dcmValorPagamento;

                Decimal dcmTotalSistemaValor = 0;
                Decimal dcmTotalInformadoValor = 0;
                Decimal dcmDiferencaValor = 0;

                int intTotalSistemaVias = 0;
                int intTotalInformadoVias = 0;
                int intDiferencaVias = 0;

                foreach (DataRow dtrCaixaFechamentoDetalhes in dttCaixaFechamentoDetalhes.Rows)
                {
                    if (Convert.ToInt32(dtrCaixaFechamentoDetalhes["Forma_Pagamento_ID"]) == Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO)
                    {
                        stbPagamentos.Append("Dinheiro" + "\r\n");

                        stbPagamentos.Append(dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Sistema"].ToString() + "                       ");

                        if (this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Select("Forma_Pagamento_ID = " + Formas_Pagamento.ID_FORMA_PAGAMENTO_DINHEIRO).Length > 0)
                        {
                            dcmValorPagamento = dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Fiscal"].DefaultDecimal();

                            stbPagamentos.Append(dcmValorPagamento.ToString("#,##0.00") + "\r\n");
                            dcmTotalInformadoValor += dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Fiscal"].DefaultDecimal();
                        }
                        else
                        {
                            dcmValorPagamento = dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Operadora_2"].DefaultDecimal() > 0 ?
                                                    dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Operadora_2"].DefaultDecimal() :
                                                    dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Operadora_1"].DefaultDecimal();

                            stbPagamentos.Append(dcmValorPagamento.ToString("#,##0.00") + "\r\n");
                            dcmTotalInformadoValor += dcmValorPagamento;
                        }

                        dcmTotalSistemaValor += dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Sistema"].DefaultDecimal();

                    }
                    else if (Convert.ToInt32(dtrCaixaFechamentoDetalhes["Enum_TipoTransacao_ID"]) == (int)TipoTransacaoTEF.SITEF)
                    {
                        stbPagamentos.Append(Convert.ToString(dtrCaixaFechamentoDetalhes["Forma_Pagamento_DS"]) + "\r\n");
                        stbPagamentos.Append(dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_Sistema"].ToString() + "                       ");


                        intTotalSistemaVias += dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_Sistema"].DefaultInteger();

                        if (this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Select("Cartao_Sitef = 1  AND Forma_Pagamento_ID = " + dtrCaixaFechamentoDetalhes["Forma_Pagamento_ID"].ToString()).Length > 0)
                        {
                            intTotalInformadoVias += dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_Fiscal"].DefaultInteger();
                            stbPagamentos.Append(dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_Fiscal"].ToString() + "\r\n");
                        }
                        else
                        {
                            int intViasInformada = dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_2"].DefaultInteger() > 0 ?
                                                         dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_2"].DefaultInteger() :
                                                          dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Qtde_Vias_1"].DefaultInteger();
                            intTotalInformadoVias += intViasInformada;
                            stbPagamentos.Append(intViasInformada.DefaultString() + "\r\n");
                        }


                    }
                    else
                    {
                        stbPagamentos.Append(Convert.ToString(dtrCaixaFechamentoDetalhes["Forma_Pagamento_DS"]) + "\r\n");
                        stbPagamentos.Append(dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Sistema"].ToString() + "                       ");

                        dcmTotalSistemaValor += dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Sistema"].DefaultDecimal();
                        if (this.dtsFechamentoPagamentosInformados.Tables["Fechamento_Fiscal_Pagamento"].Select("Cartao_Sitef = 0  AND Forma_Pagamento_ID = " + dtrCaixaFechamentoDetalhes["Forma_Pagamento_ID"].ToString()).Length > 0)
                        {
                            dcmTotalInformadoValor += dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Fiscal"].DefaultDecimal();
                            stbPagamentos.Append(dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Fiscal"].ToString() + "\r\n");
                        }
                        else
                        {
                            decimal dcmValorPosInformado = dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Operadora_2"].DefaultDecimal() > 0 ?
                                                            dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Operadora_2"].DefaultDecimal() :
                                                            dtrCaixaFechamentoDetalhes["Caixa_Fechamento_Detalhes_Valor_Operadora_1"].DefaultDecimal();
                            dcmTotalInformadoValor += dcmValorPosInformado;
                            stbPagamentos.Append(dcmValorPosInformado.DefaultString() + "\r\n");
                        }

                    }
                }

                dcmDiferencaValor = dcmTotalInformadoValor - dcmTotalSistemaValor;
                intDiferencaVias = intTotalInformadoVias - intTotalSistemaVias;

                stbTotalizador.Append("\r\r\n");

                stbTotalizador.Append("Total Calculado Valor: " + dcmTotalSistemaValor.ToString("#,###,##0.00") + "\r\n");
                stbTotalizador.Append("Total Informado Valor: " + dcmTotalInformadoValor.ToString("#,###,##0.00") + "\r\n");
                stbTotalizador.Append("Diferença: " + dcmDiferencaValor.ToString("#,###,##0.00") + "\r\n");

                stbTotalizador.Append("\r\r\n");

                stbTotalizador.Append("Total Calculado Vias: " + intTotalSistemaVias + "\r\n");
                stbTotalizador.Append("Total Informado Vias: " + intTotalInformadoVias + "\r\n");
                stbTotalizador.Append("Diferença: " + intDiferencaVias + "\r\n");

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Comprovante Pacote e Serviço   "

        private string Processar_Impressao_Comprovante_Pacote_e_Servico()
        {
            try
            {
                string strComandasPacote = string.Empty;
                string strRomaneiosPacote = string.Empty;
                string strRomaneiosServico = string.Empty;
                bool blnGerarComprovantePacote = false;
                bool blnGerarComprovanteServico = false;
                bool blnControlaComprovantePacote = false;
                bool blnControlaComprovanteServico = false;
                foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Select(string.Empty, "Comanda_Interna_ID"))
                {

                    if ((Convert.ToInt32(dtrRomaneio["Enum_Romaneio_Tipo_ID"]) == (Int32)TipoRomaneio.Tecnica)
                            | (Convert.ToInt32(dtrRomaneio["Enum_Romaneio_Tipo_ID"]) == (Int32)TipoRomaneio.Auto_Servico)
                            | (Convert.ToInt32(dtrRomaneio["Enum_Romaneio_Tipo_ID"]) == (Int32)TipoRomaneio.Especial))
                    {
                        // Pesquisa nos itens
                        blnControlaComprovantePacote = false;
                        blnControlaComprovanteServico = false;
                        foreach (DataRow dtrRomaneio_IT in this.dtsRomaneioTemporario.Tables["Romaneio_IT"].Select("Romaneio_CT_ID =" + Convert.ToString(dtrRomaneio["Romaneio_Ct_ID"]) + " And Lojas_ID = " + Convert.ToString(dtrRomaneio["Lojas_ID"])))
                        {
                            if (((Convert.ToInt32(dtrRomaneio_IT["Enum_Objeto_Tipo_ID"]) == (Int32)Tipo_Objeto.Peca)
                                | (Convert.ToInt32(dtrRomaneio_IT["Enum_Objeto_Tipo_ID"]) == (Int32)Tipo_Objeto.Kit)
                                | (Convert.ToInt32(dtrRomaneio_IT["Enum_Objeto_Tipo_ID"]) == (Int32)Tipo_Objeto.Encomenda))
                                & (Convert.ToInt32(dtrRomaneio["Enum_Romaneio_Tipo_ID"]) != (Int32)TipoRomaneio.Auto_Servico))
                            {
                                // Se existir algum item do tipo Peça ou Kit gera comprovante do Pacote
                                if (blnControlaComprovantePacote == false)
                                {
                                    blnControlaComprovantePacote = true;
                                    blnGerarComprovantePacote = true;
                                    if ((Convert.ToInt32(dtrRomaneio["Comanda_Interna_ID"]) != 0))
                                    {
                                        strComandasPacote += Convert.ToString(dtrRomaneio["Comanda_Interna_ID"]) + ",";
                                    }
                                    else
                                    {
                                        strRomaneiosPacote += Convert.ToString(dtrRomaneio["Romaneio_Ct_ID"]) + ",";
                                    }

                                }
                            }
                            else if ((Convert.ToInt32(dtrRomaneio_IT["Enum_Objeto_Tipo_ID"]) == (Int32)Tipo_Objeto.Servico))
                            {
                                // Se existir algum item do tipo Serviço gera comprovante de Serviço
                                if (blnControlaComprovanteServico == false || strRomaneiosServico.Replace(",", string.Empty) != Convert.ToString(dtrRomaneio["Romaneio_Ct_ID"]))
                                {
                                    blnControlaComprovanteServico = true;
                                    blnGerarComprovanteServico = true;

                                    strRomaneiosServico += Convert.ToString(dtrRomaneio["Romaneio_Ct_ID"]) + ",";
                                }
                            }
                            if (blnControlaComprovantePacote == true & blnControlaComprovanteServico == true)
                                break;
                        }
                    }
                }

                string strComandasServico = string.Empty;
                if (this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows.Count != 0)
                {
                    if ((blnGerarComprovantePacote == true & blnGerarComprovanteServico == true))
                    {
                        string strRetorno = string.Empty;
                        strRetorno = this.Imprimir_Comprovante_Pacote(strComandasPacote, strRomaneiosPacote, Convert.ToInt32(this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_ID"]));
                        if (strRetorno != string.Empty)
                            return strRetorno;


                        return this.Imprimir_Comprovante_Servico(strComandasServico, strRomaneiosServico, Convert.ToInt32(this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_ID"]));
                    }
                    else if ((blnGerarComprovantePacote == true & blnGerarComprovanteServico == false))
                    {
                        return this.Imprimir_Comprovante_Pacote(strComandasPacote, strRomaneiosPacote, Convert.ToInt32(this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_ID"]));
                    }
                    else if ((blnGerarComprovantePacote == false & blnGerarComprovanteServico == true))
                    {
                        return this.Imprimir_Comprovante_Servico(strComandasServico, strRomaneiosServico, Convert.ToInt32(this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_ID"]));
                    }
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Imprimir_Comprovante_Pacote(string strComandasPacote, string strNumerosRomaneios, int intRomaneioGrupoID)
        {
            try
            {
                string strTextoCodigoBarra = new CaixaBUS().Montar_Comprovante_Senha(intRomaneioGrupoID, this.objTipoImpressoraFiscal);
                string strRetorno = this.Impressao_Comprovante_Pacote_e_Servico(this.Montar_Comprovante_Pacote_e_Servico(strComandasPacote, strNumerosRomaneios, Tipo_Comprovante.Pacote, intRomaneioGrupoID), intRomaneioGrupoID, strTextoCodigoBarra);

                if (strRetorno != string.Empty)
                {
                    return "Erro ao imprimir o Comprovante de Retirada do Pacote.";
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Imprimir_Comprovante_Servico(string strNumerosComandas, string strNumerosRomaneios, int intRomaneioGrupoID)
        {
            try
            {
                string strRetorno = this.Impressao_Comprovante_Pacote_e_Servico(this.Montar_Comprovante_Pacote_e_Servico(strNumerosComandas, strNumerosRomaneios, Tipo_Comprovante.Servico, intRomaneioGrupoID), intRomaneioGrupoID, string.Empty);

                if (strRetorno != string.Empty)
                {
                    return "Erro ao imprimir o Comprovante de Serviço.";
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Montar_Comprovante_Pacote_e_Servico(string strNumeroComandas, string strNumeroRomaneios, Tipo_Comprovante enuTipoComprovante, int intRomaneioGrupoID)
        {
            try
            {
                bool blnTipoComprovantePacote = false;

                if (enuTipoComprovante == Tipo_Comprovante.Pacote)
                {
                    blnTipoComprovantePacote = true;
                }

                string strNomeLoja = Root.Loja_Ativa.Nome;
                string strDocumento = this.strClienteID == Constantes_Caixa.ID_CONSUMIDOR_FINAL && this.blnUtilizaNFp ? this.strCpfCnpjNotaFiscalPaulista : this.strCpfCnpj;
                strDocumento = strDocumento == string.Empty ? "000.000.000-00" : strDocumento;

                return new CaixaBUS().Montar_Comprovante_Pacote_e_Servico(
                                                                    strNomeLoja,
                                                                    DateTime.Now,
                                                                    strNumeroComandas,
                                                                    strNumeroRomaneios,
                                                                    blnTipoComprovantePacote,
                                                                    intRomaneioGrupoID,
                                                                    strDocumento,
                                                                    this.dtoUsuario.Nome_Completo,
                                                                    this.dttCupomFiscal,
                                                                    this.dttCupomFiscalFechamento,
                                                                    false,
                                                                    this.objTipoImpressoraFiscal);

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Comprovante dos itens para o Cancelamento   "

        private void Imprimir_Comprovante_Itens_Cancelamento(DataRow[] colItens)
        {
            try
            {
                string strComprovante = new CaixaBUS().Montar_Comprovante_Itens_A_Cancelar(colItens);
                strComprovante = DivUtil.Remover_Caracteres_Especiais(strComprovante);

                this.Impressao_Comprovante_Nao_Fiscal(strComprovante);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Comprovante de Resta           "

        private string Processar_Impressao_Comprovante_Resta()
        {
            try
            {
                
                DataSet dtsResta = null;
                if (this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows.Count != 0)
                {
                    dtsResta = new Romaneio_Pre_VendaBUS().Imprimir_DataSet_Romaneio(0, 0, this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_ID"].DefaultInteger(), 
                                                                                    this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Lojas_ID"].DefaultInteger());
                }

                if (dtsResta == null || dtsResta.Tables["Venda_Tecnica_Dados_Impressao"].Rows.Count == 0)
                {
                    return string.Empty;
                }

                DataSet dtsLayout = new Parametros_SistemaBUS().Ver_DataSet_Parametro_Com_Loja("LAYOUT_ROMANEIO", this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Lojas_ID"].DefaultInteger());

                string strTipoImpressao = string.Empty;
                if (dtsLayout.Tables["Parametros_Sistema"].Rows.Count > 0)
                {
                    strTipoImpressao = dtsLayout.Tables["Parametros_Sistema"].Rows[0]["Parametros_Sistema_Valor"].DefaultString();
                }

                return this.Imprimir_Comprovante_De_Resta(dtsResta, strTipoImpressao);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Imprimir_Comprovante_De_Resta(DataSet dtsRomaneio, string strTipoImpressao)
        {
            try
            {
                if (this.Verifica_Cupom_Fiscal_Aberto() == true)
                {
                    return "Não foi possível realizar a impressão pois já existe uma impressão pendente!" + "\r" + "Para realizar esta impressão é preciso finalizar a impressão atual ou cancelar o cupom!";
                }

                string strComprovanteResta = new Impressao_Romaneio().Montar_Romaneio_Credito_Para_Impressao(dtsRomaneio, strTipoImpressao, false, false);

                string strRetorno = this.Impressao_Comprovante_Nao_Fiscal(strComprovanteResta);

                if (strRetorno != string.Empty)
                {
                    return "Erro ao imprimir o Comprovante de Resta.";
                }

                return string.Empty;

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Comprovante Liquid. Crédito    "

        private string Liberar_Credito_Em_Dinheiro()
        {
            try
            {
                string strRetornoImpressao = this.Imprimir_Comprovante_Credito_em_Dinheiro();

                if (string.IsNullOrEmpty(strRetornoImpressao))
                {
                    if (this.Confirmar_Venda(false))
                    {
                        this.Inicializar_Nova_Venda();
                    }
                }

                return strRetornoImpressao;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Imprimir_Comprovante_Credito_em_Dinheiro()
        {
            try
            {
                if ((this.dtoUsuarioAutenticar != null))
                {
                    if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.Gerar_Credito.ToString()) == true)
                    {
                        string strComprovanteCredito = this.Montar_Comprovante_Credito_em_Dinheiro(this.dtoUsuarioAutenticar);

                        string strRetorno = this.Impressao_Comprovante_Nao_Fiscal(strComprovanteCredito);
                        if (!string.IsNullOrEmpty(strRetorno))
                        {
                            return strRetorno;
                        }
                        return string.Empty;
                    }
                    else
                    {
                        this.enuSituacao = Operacao.Operacao_Inicial;
                        return "Sem permissão para a Liberação do Crédito em Dinheiro";
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Montar_Comprovante_Credito_em_Dinheiro(object objUsuarioAutorizacaoCaixa)
        {
            try
            {
                string strRomaneioCtID = this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Romaneio_Ct_ID"].ToString();
                string strNomeLoja = Root.Loja_Ativa.Nome;
                DateTime dtmDataLiberacao = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);
                string strUsuarioOperacao = this.dtoUsuario.Nome_Completo;
                string strUsuarioTroca = this.Selecionar_Nome_Usuario_Autorizacao(Convert.ToInt32(this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Usuario_Gerente_ID"]));
                string strUsuarioCredito = this.Selecionar_Nome_Usuario_Aprovacao_Credito();
                string strUsuarioLiberacao = ((UsuarioDO)objUsuarioAutorizacaoCaixa).Nome_Completo;

                return new CaixaBUS().Montar_Comprovante_Credito_em_Dinheiro(strRomaneioCtID, strNomeLoja, this.dcmTotalVenda, dtmDataLiberacao, strUsuarioOperacao, strUsuarioTroca, strUsuarioCredito, strUsuarioLiberacao, this.dtsRomaneioTemporario, this.dtsCliente);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private String Selecionar_Nome_Usuario_Autorizacao(int intUsuarioID)
        {
            try
            {
                UsuarioBUS busUsuario = new UsuarioBUS();
                if (intUsuarioID == 0)
                {
                    int intRomaneioPreVendaCT = this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Romaneio_Ct_ID"].DefaultInteger();
                    int intLojasID = this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Lojas_ID"].DefaultInteger();

                    DataTable dttResta = new Romaneio_Pre_VendaBUS().Consultar_DataTable_Romaneio_Pre_Venda_Gerente_ID(intRomaneioPreVendaCT, intLojasID);

                    if (dttResta.Rows.Count == 0)
                        return string.Empty;
   
                    return busUsuario.Selecionar_Nome_Usuario(dttResta.Rows[0]["Usuario_Gerente_ID"].DefaultInteger());
                }
                else
                {
                    return busUsuario.Selecionar_Nome_Usuario(intUsuarioID);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private String Selecionar_Nome_Usuario_Aprovacao_Credito()
        {
            try
            {
                int intRomaneioPreVendaCT = this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Romaneio_Ct_ID"].DefaultInteger();
                int intLojasID = this.dtsRomaneioTemporario.Tables["Romaneio_CT"].Rows[0]["Lojas_ID"].DefaultInteger();

                DataTable dttRomaneioCreditoAprovacao = new Romaneio_Credito_AprovacaoBUS().Selecionar_por_Romaneio_Pre_Venda(intRomaneioPreVendaCT, intLojasID);

                if (dttRomaneioCreditoAprovacao.Rows.Count > 0)
                {
                    return this.Selecionar_Nome_Usuario_Autorizacao(dttRomaneioCreditoAprovacao.Rows[0]["Usuario_Aprovacao_ID"].DefaultInteger());
                }
                else
                {
                    return String.Empty;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #endregion

        #region "   Procedimento Garantia       "

        private DataTable Consultar_Datatable_Peca_Da_Venda()
        {
            DataTable dttPecas = new DataTable();
            dttPecas.Columns.Add("Peca_ID");

            foreach (DataRow dtrPreVenda in this.dtsRomaneioTemporario.Tables["Romaneio_It"].Rows)
            {
                // se for romaneio de troca não incluir a peça para impressão de procedimentos de troca e somente peça
                if (dtrPreVenda["Enum_Romaneio_Tipo_ID"].DefaultInteger() != TipoRomaneio.Troca.DefaultInteger()
                    && dtrPreVenda["Enum_Objeto_Tipo_ID"].DefaultInteger() == Tipo_Objeto.Peca.DefaultInteger())
                {
                    DataRow dtrIncluir = dttPecas.NewRow();
                    dtrIncluir["Peca_ID"] = dtrPreVenda["Objeto_ID"];
                    dttPecas.Rows.Add(dtrIncluir);
                }
            }

            return dttPecas;
        }

        private void Processar_Impressao_Procedimento_Garantia(bool imprimeProcedimentoGarantia, int numeroRomaneioGrupo)
        {
            if (imprimeProcedimentoGarantia)
            {
                var dttpecas = this.Consultar_Datatable_Peca_Da_Venda();

                var procedimentoTrocaBUS = new Procedimento_TrocaBUS();

                var dttProcedimentosTroca = procedimentoTrocaBUS.Consultar_Procedimento_Troca_Por_Pecas(dttpecas);

                if (dttProcedimentosTroca.Rows.Count > 0) // só prosseguir com a impressão de procedimentos de garantia se houver peças com procedimento associado.
                {
                    Impressao_Romaneio objImpRomaneio = new Impressao_Romaneio();

                    var textoImpressao = objImpRomaneio.Formata_Texto_Impressao(dttProcedimentosTroca, numeroRomaneioGrupo, "Grupo");

                    if (textoImpressao != string.Empty)
                    {
                        objImpRomaneio.Imprimir_Procedimentos_De_Garantia(textoImpressao, RawPrinterHelper.GetDefaultSystemPrinter(), "Procedimentos");
                    }
                }
                else
                {
                    // caso não tenha cupom fiscal e for só auto-serviço não picotar no final pois já foi picotado.
                    if (this.blnCupomAberto == false || this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                    {
                        return;
                    }

                    // caso não tenha procedimento - pula linha e corta papel.
                    RawPrinterHelper.SendStringToPrinter(RawPrinterHelper.GetDefaultSystemPrinter(), "\xA\xA\xA\x1D\x56\x1");
                }
            }
        }

        #endregion

        #region "   Guiche              "

        private void Carregar_IP_e_Porta_Painel_Guiche()
        {
            try
            {
                ArrayList colPainel = new Painel_GuicheBUS().Consultar_DataObject(Root.Loja_Ativa.ID, Convert.ToInt32(Tipo_Painel_Guiche.Painel_Caixa));
                if (colPainel.Count > 0)
                {
                    this.strIPPainelGuiche = ((Painel_GuicheDO)colPainel[0]).Painel_Guiche_IP;
                    this.intPortaPainelGuiche = Convert.ToInt32(((Painel_GuicheDO)colPainel[0]).Painel_Guiche_Porta);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string Proximo_Cliente_Painel_Guiche()
        {
            try
            {
                if (!Root.Lojas_Parametros.Verificar_Loja_Por_Parametro_Opcional(Root.Loja_Ativa.ID, "Utiliza Painel Próximo Caixa", "Sim"))
                {
                    return "Esta loja não possui painel.";
                }

                TcpClient tcpClient = new TcpClient();

                try
                {
                    tcpClient.Connect(this.strIPPainelGuiche, this.intPortaPainelGuiche);
                }
                catch (Exception)
                {
                    return "Painel não disponível";
                }

                NetworkStream networkStream = tcpClient.GetStream();

                if ((networkStream.CanWrite) && (networkStream.CanRead))
                {
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(this.intNumeroGuiche.ToString());
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                }
                else
                {
                    tcpClient.Close();
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Impressora fiscal   "

        private void Inicilizar_Objetos_Comunicacao_Impressora_Fiscal()
        {
            try
            {
                CaixaBUS busCaixa = new CaixaBUS();
                this.objComunicacaoImpressoraFiscal = busCaixa.Verificar_Tipo_Comunicacao_Impressora_Fiscal();
                this.objTipoImpressoraFiscal = busCaixa.Verificar_Tipo_Impressora_Fiscal();

                string strMensagem = string.Empty;
                if (objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    busCaixa.Obter_DataObject_Sat_Arquivo(ref this.dtoSatCaixa);

                    this.blnUtilizaControladorSat = this.dtoSatCaixa == null;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Abrir_Porta_Impressora_Fiscal()
        {
            try
            {
                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Abertura_Porta_Impressora_Fiscal();

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    Root.Impressora_Fiscal_Disponivel = false;
                    this.blnPortaImpressoraFiscal = false;
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    return false;
                }

                if (this.Inicializar_Comunicacao_Sat() == false)
                {
                    return false;
                }

                Root.Impressora_Fiscal_Disponivel = true;
                this.blnPortaImpressoraFiscal = true;

                return true;
            }
            catch (Exception ex)
            {
                Root.Impressora_Fiscal_Disponivel = false;
                this.blnPortaImpressoraFiscal = false;

                if (ex.Message == "Não é possível carregar a DLL 'CONVECF.DLL': Não foi possível encontrar o módulo especificado. (Exceção de HRESULT: 0x8007007E)")
                {
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtMenu.Content = "Erro ao carregar o arquivo CONVECF.DLL.";
                    return false;
                }
                throw;
            }
        }

        private void Fechar_Porta_Impressora_Fiscal()
        {
            try
            {
                this.Interface_Fechamento_Porta_Impressora_Fiscal();

                if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT && this.objTipoProcessoSatFiscal.Verificar_Comunicao_Aberta())
                {
                    this.objTipoProcessoSatFiscal.Fechar_Comunicacao();
                }

                Root.Impressora_Fiscal_Disponivel = false;
                this.blnPortaImpressoraFiscal = false;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Não é possível carregar a DLL 'CONVECF.DLL': Não foi possível encontrar o módulo especificado. (Exceção de HRESULT: 0x8007007E)")
                {
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtMenu.Content = "Erro ao carregar o arquivo CONVECF.DLL.";
                    return;
                }

                throw;
            }

        }

        private bool Imprimir_Item_Orc_Cupom_Fiscal()
        {
            try
            {
                // Imprimir item no cupom fiscal
                if (this.blnCupomAberto == false)
                {
                    if (!this.Preparar_Abertura_Cupom())
                    {
                        // Excluir o item porque não pode imprimir o cupom 
                        this.Excluir_Item_Tab();
                        this.dtsConsultaPecaItens.Clear();
                        return false;
                    }
                }

                this.Preencher_DataTable_Item_Cupom();

                if (this.dttCupomFiscal.Rows.Count <= 0)
                {
                    this.dtsConsultaPecaItens.Clear();

                    return true;
                }

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Impressao_Cupom_Fiscal_Item(this.dttCupomFiscal);

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    for (int intContador = 0; intContador < TENTATIVAS_COMUNICACAO_IMPRESSORA; intContador++)
                    {
                        enuRetornoImpressoraFiscal = this.Impressao_Cupom_Fiscal_Item(this.dttCupomFiscal);

                        if (enuRetornoImpressoraFiscal == Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                        {
                            this.dtsConsultaPecaItens.Clear();

                            return true;
                        }
                    }

                    if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                    {
                        this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                        // Excluir o item porque não pode imprimir o cupom 
                        this.Excluir_Item_Tab();
                        this.dtsConsultaPecaItens.Clear();
                        return false;
                    }
                }

                this.dtsConsultaPecaItens.Clear();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Imprimir_Documento_Fiscal()
        {
            try
            {
                if (this.intTipoDocumento != TipoDocumento.Cupom.DefaultInteger())
                {
                    return true;
                }

                if (this.Imprimir_Fechamento_Cupom_Fiscal() == false)
                {
                    return false;
                }

                this.strCOO = this.Retornar_COO();
                this.strECF = this.Retornar_Numero_Caixa();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Imprimir_Fechamento_Cupom_Fiscal()
        {
            try
            {
                if (this.blnCupomAberto == false)
                {
                    return false;
                }

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Iniciar_Fechamento_Cupom_Fiscal(this.dcmDescontoComercial, this.dcmDescontoItemTotal);
                // Inicio o Fechamento do Cupom
                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    return false;
                }

                if (this.dttCupomFiscalFechamento.Rows.Count > 0)
                {
                    decimal dcmValorPago = 0;
                    decimal dcmValorCompra = 0;
                    decimal dcmValorTotalPago = 0;
                    decimal dcmValorTroco = 0;
                    string strFormaPagto = string.Empty;
                    string strOperadora = string.Empty;

                    foreach (DataRow dtrDados_Venda in this.dttCupomFiscalFechamento.Rows)
                    {
                        dcmValorCompra = Convert.ToDecimal(dtrDados_Venda["Valor_Venda"]).ToDecimalRound(2);

                        if (Convert.ToDecimal(dtrDados_Venda["Valor_FormaPagamento"]) > 0)
                        {
                            dcmValorPago = Convert.ToDecimal(dtrDados_Venda["Valor_FormaPagamento"]).ToDecimalRound(2);
                            strFormaPagto = Convert.ToString(dtrDados_Venda["Pagamento"]).PadRight(50, Convert.ToChar(" ")).Substring(0, 50);
                            dcmValorTotalPago += dtrDados_Venda["Valor_FormaPagamento"].DefaultDecimal();
                            dcmValorTroco = Convert.ToDecimal(dtrDados_Venda["Valor_Venda"]).ToDecimalRound(2) < 0 ? 0 : dcmValorTotalPago - Convert.ToDecimal(dtrDados_Venda["Valor_Venda"]).ToDecimalRound(2);
                            strOperadora = dtrDados_Venda["OperadoraCartao"].DefaultString();
                            enuRetornoImpressoraFiscal = this.Inserir_Forma_Pagamento(strFormaPagto.Trim(), dcmValorPago, dcmValorTroco, strOperadora);
                            if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                            {
                                this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                                return false;
                            }
                        }
                    }

                    string strMensagemFechamento = new CaixaBUS().Montar_Mensagem_Fechamento(dcmValorCompra, dcmValorPago);

                    enuRetornoImpressoraFiscal = this.Terminar_Fechamento_Cupom_Fiscal(strMensagemFechamento);
                    if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                    {
                        this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
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

        private void Preencher_DataTable_CupomFiscal_Fechamento()
        {
            try
            {
                foreach (DataRow dtrExclusao in this.dttCupomFiscal.Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                {
                    dtrExclusao.Delete();
                }

                foreach (DataRow dtrExclusao in this.dttCupomFiscalFechamento.Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                {
                    dtrExclusao.Delete();
                }

                decimal dcmValorRecebido = this.Calcular_Valor_Total_Pagamento();
                decimal dcmValorVenda = this.dcmTotalVenda;

                decimal dcmValorRomaneioCupom = 0;
                foreach (DataRow dtrRomaneio in this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select(" Enum_Objeto_Tipo_ID = " + Convert.ToInt32(Enumerados.Tipo_Objeto.Servico).ToString()))
                {
                    if (Convert.ToDecimal(dtrRomaneio["Romaneio_It_Preco_Pago"]) >= 0)
                    {
                        DataRow dtrCupomFiscal = this.dttCupomFiscal.NewRow();

                        dtrCupomFiscal["Cliente"] = this.txtNomeCliente.Text;
                        dtrCupomFiscal["Documento"] = this.strCpfCnpjNotaFiscalPaulista;

                        if (Convert.ToInt32(dtrRomaneio["Enum_Objeto_Tipo_ID"]) == (Int32)Tipo_Objeto.Servico)
                        {
                            dtrCupomFiscal["Codigo"] = Convert.ToString(dtrRomaneio["Codigo"]).PadRight(13, Convert.ToChar(" ")).Substring(0, 13) + ".";
                        }
                        else if (Convert.ToInt32(dtrRomaneio["Enum_Objeto_Tipo_ID"]) == (Int32)Tipo_Objeto.Encomenda)
                        {
                            dtrCupomFiscal["Codigo"] = Convert.ToString(dtrRomaneio["Codigo"]).PadRight(13, Convert.ToChar(" ")).Substring(0, 13) + ".";
                        }
                        else
                        {
                            dtrCupomFiscal["Codigo"] = dtrRomaneio["Codigo"];
                        }

                        dtrCupomFiscal["Tipo_Objeto"] = dtrRomaneio["Enum_Objeto_Tipo_ID"];

                        dtrCupomFiscal["Descricao"] = dtrRomaneio["Descricao"];
                        dtrCupomFiscal["Imposto"] = dtrRomaneio["Imposto"];
                        dtrCupomFiscal["Qtde"] = (Int32)dtrRomaneio["Romaneio_It_Qtde"];
                        dtrCupomFiscal["Preco_Unitario"] = Convert.ToDecimal(dtrRomaneio["Romaneio_It_Preco_Pago"]);

                        dtrCupomFiscal["IsCupomFiscal"] = false;
                        dtrCupomFiscal["IsRomaneio"] = true;
                        dcmValorRomaneioCupom = dcmValorRomaneioCupom + ((Int32)dtrRomaneio["Romaneio_It_Qtde"] * Convert.ToDecimal(dtrRomaneio["Romaneio_It_Preco_Pago"]));
                        this.dttCupomFiscal.Rows.Add(dtrCupomFiscal);
                    }
                }

                foreach (DataRow dtrPreVenda in this.dtsPreVendaTemporario.Tables["Romaneio_Pre_Venda_It"].Select(" Enum_Objeto_Tipo_ID = " + Convert.ToInt32(Enumerados.Tipo_Objeto.Servico).ToString()))
                {

                    DataRow dtrCupomFiscal = this.dttCupomFiscal.NewRow();

                    dtrCupomFiscal["Cliente"] = this.txtNomeCliente.Text;
                    dtrCupomFiscal["Documento"] = this.strCpfCnpjNotaFiscalPaulista;

                    dtrCupomFiscal["Codigo"] = dtrPreVenda["Codigo"];

                    dtrCupomFiscal["Tipo_Objeto"] = dtrPreVenda["Enum_Objeto_Tipo_ID"];

                    dtrCupomFiscal["Descricao"] = dtrPreVenda["Descricao"];
                    dtrCupomFiscal["Imposto"] = dtrPreVenda["Imposto"];
                    dtrCupomFiscal["Qtde"] = (Int32)dtrPreVenda["Romaneio_Pre_Venda_It_Qtde"];
                    dtrCupomFiscal["Preco_Unitario"] = Convert.ToDecimal(dtrPreVenda["Romaneio_Pre_Venda_It_Preco_Pago"]);

                    dtrCupomFiscal["IsCupomFiscal"] = false;
                    dtrCupomFiscal["IsRomaneio"] = true;
                    dcmValorRomaneioCupom = dcmValorRomaneioCupom + ((Int32)dtrPreVenda["Romaneio_Pre_Venda_It_Qtde"] * Convert.ToDecimal(dtrPreVenda["Romaneio_Pre_Venda_It_Preco_Pago"]));
                    this.dttCupomFiscal.Rows.Add(dtrCupomFiscal);
                }

                ///Calcular desconto comercial e cred-utilizado
                decimal dcmValorImpressoCupom = this.Calcular_Valor_Total_Item_Impresso();
                this.dcmDescontoComercial = 0;

                ///Calcula o valor do serviço caso houver
                decimal dcmTotalServicos = 0;
                string strResultadoSum = Convert.ToString(this.dttCupomFiscal.Compute("SUM(Total)", "Tipo_Objeto = " + Convert.ToString((int)Enumerados.Tipo_Objeto.Servico)));
                if (strResultadoSum != String.Empty)
                {
                    dcmTotalServicos = strResultadoSum.ToDecimal();
                }

                decimal dcmTotalDesconto = 0;
                ///Calcula o valor do desconto caso houver
                string strDescontoSum = Convert.ToString(this.dtsGridVenda.Tables["Venda_It"].Compute("SUM(Desconto)", string.Empty));
                if (strDescontoSum != String.Empty)
                {
                    dcmTotalDesconto = strDescontoSum.ToDecimal();
                }

                this.dcmDescontoItemTotal = dcmTotalDesconto;

                ///Calcula o valor do serviço caso houver
                decimal dcmTotalTroco = 0;
                string strTrocoSum = Convert.ToString(this.dtsCondicaoPagto.Tables["Romaneio_Pagamento_Venda_Liberada"].Compute("SUM(Troco)", string.Empty));
                if (strTrocoSum != String.Empty)
                {
                    dcmTotalTroco = strTrocoSum.ToDecimal();
                }

                decimal dcmContraVale = 0;
                decimal dcmCreditoUtilizado = 0;
                decimal dcmTotalItens = 0;

                if ((dcmValorImpressoCupom - dcmTotalDesconto + dcmTotalServicos) > dcmValorVenda)
                {

                    // Valor do crédito < Percentual definido por parametro, então sair o valor do credito como Desc Comercial, caso contrario sair como Cred-Utilizado
                    if (dcmValorVenda <= 0)
                    {   // Crédito é > que a venda, então imprimir valor da venda como Cred-Utilizado.Caso exista serviço então é calculado o valor do cupom menos o valor do serviço
                        dcmContraVale = dcmTotalServicos > 0 ? dcmValorImpressoCupom : (dcmValorImpressoCupom + dcmTotalServicos) - dcmTotalDesconto;
                    }
                    else if (((((dcmValorImpressoCupom + dcmTotalServicos) - dcmValorVenda) / (dcmValorImpressoCupom + dcmTotalServicos)) * 100) > this.dcmParamentroLojaPercentualDesconto)
                    { // Se Crédito > Perc do valor da venda, valor do crédito sair como Cred-Utilizado
                        dcmContraVale = ((dcmValorImpressoCupom + dcmTotalServicos) - dcmValorVenda) - dcmTotalDesconto;
                    }
                    else
                    { // valor do crédito sair como Desconto Comercial
                        if (dcmTotalServicos > 0)
                        {
                            dcmCreditoUtilizado = (dcmValorImpressoCupom + dcmTotalServicos) - dcmValorVenda;
                            dcmTotalItens = dcmValorImpressoCupom;
                            /// Se tiver serviços, então é feito o calculo para que o valor dos itens não ultrapasse o Credito Utilizado
                            if (dcmTotalItens <= dcmCreditoUtilizado)
                            {
                                dcmContraVale = dcmTotalItens;
                                dcmValorVenda = 0;
                                dcmValorRecebido = 0;
                            }
                            else
                            {
                                ///No caso do valor dos itens não ultrapasserem o cretido utilizado, então é feito o calculo para descontar o serviço 
                                dcmContraVale = dcmCreditoUtilizado;
                                dcmValorVenda = dcmValorVenda - dcmTotalServicos;
                                dcmValorRecebido = dcmValorRecebido - dcmTotalServicos;
                            }
                        }
                        else
                        {
                            this.dcmDescontoComercial = (dcmValorImpressoCupom - this.dcmDescontoItemTotal - dcmValorVenda);
                        }
                    }
                }

                ///Verifica se tem credito utilizado, caso contrário verifica se tem serviço
                if (dcmContraVale > 0)
                {
                    DataRow dtrCupomFiscalFechamento = this.dttCupomFiscalFechamento.NewRow();
                    dtrCupomFiscalFechamento["Valor_Recebido"] = dcmValorRecebido;
                    dtrCupomFiscalFechamento["Valor_Venda"] = dcmValorVenda;
                    dtrCupomFiscalFechamento["Pagamento"] = Formas_Pagamento_Descricao.FORMA_CREDITO_LOJA;
                    dtrCupomFiscalFechamento["Valor_FormaPagamento"] = dcmContraVale;
                    dtrCupomFiscalFechamento["NrParcelas"] = 1;
                    this.dttCupomFiscalFechamento.Rows.Add(dtrCupomFiscalFechamento);
                }
                else if (dcmTotalServicos > 0 && (this.dtsGridVenda.Tables["Venda_It"].Select("Tipo_Objeto = " + Convert.ToString((int)Enumerados.Tipo_Objeto.Peca) + " AND Preco_Unitario > 0").Length > 0))
                {
                    /// Caso tenha serviço e itens, então é feito o calculo para desprezar o valor do serviço 
                    /// e somente considerar o valor dos itens
                    dcmTotalItens = dcmValorVenda - dcmTotalServicos;
                    dcmValorVenda = dcmTotalItens;

                }

                decimal dcmTroco = 0;
                string strCondPag = string.Empty;
                if (this.objPagamentos.Items.Count > 0)
                {
                    dcmValorRecebido = this.Calcular_Valor_Total_Pagamento();

                    foreach (DataRow dtrPagamento in this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Select(string.Empty, string.Empty, DataViewRowState.CurrentRows))
                    {
                        if (dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Dia_Parcela"].ToString() == "0")
                        {
                            dcmTroco = 0;
                            if (Convert.ToBoolean(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Debito"]) == true)
                            {
                                strCondPag = this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT ? Formas_Pagamento_Descricao.FORMA_CARTAO_DEBITO : Formas_Pagamento_Descricao.FORMA_CARTAO_DEBITO_OPERADORA;
                            }
                            else if (Convert.ToBoolean(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cartao_Credito"]) == true)
                            {
                                strCondPag = this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT ? Formas_Pagamento_Descricao.FORMA_CARTAO_CREDITO : Formas_Pagamento_Descricao.FORMA_CARTAO_CREDITO_OPERADORA;
                            }
                            else if (Convert.ToBoolean(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Emite_Cheque"]) == true)
                            {
                                strCondPag = Formas_Pagamento_Descricao.FORMA_CHEQUE;
                            }
                            else
                            {
                                dcmTroco = dcmTotalTroco;
                                strCondPag = Formas_Pagamento_Descricao.FORMA_DINHEIRO;
                            }

                            ///Faz os calculos do credito utilizado quando tem serviço
                            decimal dcmValorVendaItem = 0;
                            dcmValorVendaItem = Convert.ToDecimal(dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Valor_Informado"]) + dcmTroco;

                            DataRow dtrCupomFiscalFechamento = this.dttCupomFiscalFechamento.NewRow();
                            dtrCupomFiscalFechamento["Item"] = dtrPagamento["Item"];
                            dtrCupomFiscalFechamento["Pagamento"] = strCondPag;
                            dtrCupomFiscalFechamento["Valor_Recebido"] = dcmValorRecebido;

                            dtrCupomFiscalFechamento["Valor_Venda"] = dcmValorVenda;
                            dtrCupomFiscalFechamento["Valor_CupomFiscal"] = (dcmValorImpressoCupom + dcmTotalServicos);
                            if (this.dtsPreVendaTemporario.Tables["Romaneio_Pagamento_Venda_Liberada"].Rows.Count > 1 && dcmTotalServicos > 0)
                            {
                                dtrCupomFiscalFechamento["Valor_FormaPagamento"] = this.Calcular_Percentual_Proporcional_Itens_Multiplo_Pagamento((dcmValorImpressoCupom + dcmTotalServicos - dcmContraVale), dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Valor_Informado"].ToDecimal(), (dcmValorImpressoCupom - dcmContraVale)).ToDecimalRound();
                            }
                            else
                            {
                                dtrCupomFiscalFechamento["Valor_FormaPagamento"] = dcmValorVendaItem.ToDecimalRound();
                            }
                            dtrCupomFiscalFechamento["NrParcelas"] = dtrPagamento["Romaneio_Pagamento_Venda_Liberada_Numero_de_Parcelas"];
                            dtrCupomFiscalFechamento["OperadoraCartao"] = dtrPagamento["OperadoraCartao"];
                            this.dttCupomFiscalFechamento.Rows.Add(dtrCupomFiscalFechamento);
                        }
                    }
                }

                if (dcmTotalServicos > 0)
                {
                    int intIndiceUltimoPagamento = 0;
                    Decimal dcmValorTotalPagamentos = 0;
                    foreach (DataRow dtrDadosVenda in this.dttCupomFiscalFechamento.Rows)
                    {
                        dcmValorTotalPagamentos = dcmValorTotalPagamentos + dtrDadosVenda["Valor_FormaPagamento"].ToDecimalRound();

                        intIndiceUltimoPagamento = intIndiceUltimoPagamento + 1;

                        if (intIndiceUltimoPagamento == this.dttCupomFiscalFechamento.Rows.Count)
                        {
                            if (dcmValorImpressoCupom != dcmValorTotalPagamentos)
                            {
                                dtrDadosVenda["Valor_FormaPagamento"] = dtrDadosVenda["Valor_FormaPagamento"].ToDecimalRound() + (dcmValorImpressoCupom - dcmValorTotalPagamentos - dcmDescontoItemTotal);
                            }
                        }
                    }
                }


                // Salvar desconto comercial e cred-utilizado
                this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_Valor_Desconto_Comercial"] = this.dcmDescontoComercial;
                this.dtsRomaneioTemporario.Tables["Romaneio_Grupo"].Rows[0]["Romaneio_Grupo_Valor_Credito_Utilizado"] = dcmContraVale;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private decimal Calcular_Percentual_Proporcional_Itens_Multiplo_Pagamento(decimal dcmValorRecebido, decimal dcmPagamentoValorInformado, decimal dcmValorVenda)
        {
            try
            {
                decimal dcmPercentualValorPago = 0;
                if (dcmValorRecebido != 0)
                {
                    dcmPercentualValorPago = dcmPagamentoValorInformado / dcmValorRecebido * 100;
                }
                return dcmValorVenda * dcmPercentualValorPago / 100;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Cancelar_Cupom_Fiscal()
        {
            try
            {
                if (this.dtoUsuarioAutenticar == null)
                {
                    return false;
                }

                if (Root.Permissao.Obter_Permissao_Do_Usuario(this.dtoUsuarioAutenticar, Root.Loja_Ativa.ID, this.Name, Acao_Formulario.CancelarCupom.ToString()) == false)
                {
                    this.txtMenu.Content = "Usuário não tem autorização para cancelar";
                    this.txtSenha.Clear();
                    this.txtSenha.Visibility = Visibility.Hidden;
                    this.txtCodigoItemFabricante.Focus();
                    this.enuSituacao = Operacao.Operacao_Inicial;
                    return false;
                }

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Cancelar_Cupom();

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preparar_Abertura_Cupom()
        {
            try
            {
                if (this.Verifica_Cupom_Fiscal_Aberto())
                {
                    this.Cancelar_Cupom();
                    this.dttCupomFiscal.Clear();
                    this.dttCupomFiscalFechamento.Clear();
                }

                // Abrir cupom para venda do orçamento
                this.Preencher_DataTable_Cabecalho_Cupom();

                // Pular para testar autoserviço sem impressora fiscal
                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Abrir_Cupom(this.dttCupomFiscal);

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    this.blnCupomAberto = false;
                    return false;
                }

                this.blnCupomAberto = true;
                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool Preparar_Alterar_Cupom()
        {
            try
            {
                if (!this.Verifica_Cupom_Fiscal_Aberto())
                {
                    this.blnCupomAberto = false;
                    return false;
                }

                this.Alterar_Dados_Cupom_Fiscal();

                if (!this.cpfCnpjNfpAlterado)
                {
                    this.blnCupomAberto = true;
                    return true;
                }

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Alterar_Cupom(this.dttCupomFiscal);

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    this.blnCupomAberto = false;
                    return false;
                }

                this.blnCupomAberto = true;
                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Atualizar_Data_Movimento_Impressora_Fiscal()
        {
            try
            {
                // Verificar se existe algum cupom aberto(Queda de Energia), caso exista, cancelar o cupom antes de identificar a data de movimentação.
                // Caso contrario, o cupom será liberado em dinheiro automaticamente pela impressora fiscal
                if (this.Verifica_Cupom_Fiscal_Aberto())
                {
                    this.blnCupomAberto = true;
                    this.blnPendenciaAtualizaDataMovimento = true;
                    return;
                }

                this.blnPendenciaAtualizaDataMovimento = false;

                string strArquivo = string.Empty;

                this.Retorna_Dados_Fiscais_Impressora(ref strArquivo);

                if (strArquivo == string.Empty)
                {
                    this.dtmDataMovimento = DateTime.Today;
                    return;
                }

                decimal dcmValorGTInicial = 0;
                decimal dcmValorGTFinal = 0;
                decimal dcmValorVendaLiquida = 0;

                char[] objSeparadores = new char[2];
                objSeparadores[0] = Convert.ToChar("\n");
                objSeparadores[1] = Convert.ToChar("=");

                string[] strDadosFiscais = strArquivo.Split(objSeparadores);

                for (int i = 0; i < strDadosFiscais.Length; i++)
                {
                    if (strDadosFiscais[i].Equals("DATAFISCAL"))
                    {
                        this.dtmDataMovimento = (strDadosFiscais[i + 1].Substring(0, 4) + "/" + strDadosFiscais[i + 1].Substring(4, 2) + "/" + strDadosFiscais[i + 1].Substring(6, 2)).ToDateTime();
                    }
                    else if (strDadosFiscais[i].Equals("GTI"))
                    {
                        dcmValorGTInicial = Convert.ToDecimal(strDadosFiscais[i + 1].ToDecimal() / 100);
                    }
                    else if (strDadosFiscais[i].Equals("GTF"))
                    {
                        dcmValorGTFinal = Convert.ToDecimal(strDadosFiscais[i + 1].ToDecimal() / 100);
                    }
                    else if (strDadosFiscais[i].Equals("VLIQUIDA"))
                    {
                        dcmValorVendaLiquida = Convert.ToDecimal(strDadosFiscais[i + 1].ToDecimal() / 100);
                    }
                }

                if (dcmValorGTInicial == dcmValorGTFinal && dcmValorVendaLiquida == 0)
                {
                    this.dtmDataMovimento = DateTime.Today;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Interface Impressao Fiscal  "

        private string Retornar_Numero_Caixa()
        {
            try
            {
                return this.objImpressaoFiscal.Retornar_Numero_Caixa();
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private bool Verifica_Cupom_Fiscal_Aberto()
        {
            try
            {
                return this.objImpressaoFiscal.Verificar_Cupom_Fiscal_Aberto() == Retorno_Impressora_Fiscal.Impressora_Fiscal_Cupom_Aberto;
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Interface_Cancelar_Item_Cupom_Fiscal(string strItem)
        {
            try
            {
                return this.objImpressaoFiscal.Cancelar_Item_Cupom_Fiscal(strItem);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Impressao_Leitura_X()
        {
            try
            {
                return this.objImpressaoFiscal.Leitura_X();
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private void Preencher_Data_Hora_Fiscal(ref StringBuilder stbData, ref StringBuilder stbHora)
        {
            try
            {
                this.objImpressaoFiscal.Preencher_Data_Hora_Impressora_Fiscal(ref stbData, ref stbHora);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private string Retornar_COO_Sitef()
        {
            try
            {
                string strCOO = string.Empty;

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal;
                enuRetornoImpressoraFiscal = this.objImpressaoFiscal.Preencher_Contador_Ordem_Operacao_Sitef(ref strCOO);

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    int intContador = 0;
                    while (intContador < TENTATIVAS_COMUNICACAO_IMPRESSORA && enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                    {
                        enuRetornoImpressoraFiscal = this.objImpressaoFiscal.Preencher_Contador_Ordem_Operacao_Sitef(ref strCOO);
                        intContador++;
                    }
                }
                return strCOO;
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private string Retornar_COO()
        {
            try
            {

                string strCOO = string.Empty;

                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal;
                enuRetornoImpressoraFiscal = this.objImpressaoFiscal.Preencher_Contador_Ordem_Operacao(ref strCOO);

                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    int intContador = 0;
                    while (intContador < TENTATIVAS_COMUNICACAO_IMPRESSORA && enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                    {
                        enuRetornoImpressoraFiscal = this.objImpressaoFiscal.Preencher_Contador_Ordem_Operacao(ref strCOO);
                        intContador++;
                    }
                }

                return strCOO;
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private string Impressao_Comprovante_Pacote_e_Servico(string strComprovante, int intRomaneioGrupoID, string strTextoCodigoBarra)
        {
            try
            {
                if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    strComprovante = DivUtil.Remover_Caracteres_Especiais(strComprovante);
                }

                return new CaixaBUS().Imprimir_Relatorio_Gerencial(this.objComunicacaoImpressoraFiscal, strComprovante, intRomaneioGrupoID.ToString(), false, false, this.objTipoImpressoraFiscal, this.objImpressaoFiscal, strTextoCodigoBarra);

            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private string Impressao_Comprovante_Sangria(string strComprovante, int intCaixaSangriaID)
        {
            try
            {
                return new CaixaBUS().Imprimir_Relatorio_Gerencial(this.objComunicacaoImpressoraFiscal, strComprovante, intCaixaSangriaID.ToString(), false, false, this.objTipoImpressoraFiscal, this.objImpressaoFiscal);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private string Impressao_Comprovante_Fechamento(string strCabecalho, string strPagamentos, string strTotalizador, int intCaixaFechamentoID)
        {
            try
            {
                return new CaixaBUS().Imprimir_Relatorio_Gerencial(this.objComunicacaoImpressoraFiscal, strCabecalho + strPagamentos + strTotalizador, intCaixaFechamentoID.ToString(), false, false, this.objTipoImpressoraFiscal, this.objImpressaoFiscal);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private string Impressao_Comprovante_Nao_Fiscal(string strComprovante)
        {
            try
            {
                return new CaixaBUS().Imprimir_Relatorio_Gerencial(this.objComunicacaoImpressoraFiscal, strComprovante, string.Empty, false, false, this.objTipoImpressoraFiscal, this.objImpressaoFiscal);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Abertura_Porta_Impressora_Fiscal()
        {
            try
            {
                return this.objImpressaoFiscal.Abrir_Porta_Impressora_Fiscal();
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Interface_Fechamento_Porta_Impressora_Fiscal()
        {
            try
            {
                return this.objImpressaoFiscal.Fechar_Porta_Impressora_Fiscal();
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Impressao_Cupom_Fiscal_Item(DataTable dttCupomItem)
        {
            try
            {
                return this.objImpressaoFiscal.Imprimir_Cupom_Fiscal_Item(dttCupomItem);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Cancelar_Cupom()
        {
            try
            {
                return this.objImpressaoFiscal.Cancelar_Cupom();
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Abrir_Cupom(DataTable dttCupomFiscal)
        {
            try
            {
                return this.objImpressaoFiscal.Abrir_Cupom(dttCupomFiscal);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Alterar_Cupom(DataTable dttCupomFiscal)
        {
            try
            {
                return this.objImpressaoFiscal.Alterar_Cupom(dttCupomFiscal);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Preencher_Identificacao_Caixa(string strCNPJ, string strAssinatura, string strNumeroCaixa)
        {
            try
            {
                return this.objImpressaoFiscal.Preencher_Identificacao_Caixa(strCNPJ, strAssinatura, strNumeroCaixa);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Preencher_Emitente_Caixa(LojasDO dtoLojas)
        {
            try
            {
                return this.objImpressaoFiscal.Preencher_Emitente_Caixa(dtoLojas);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Iniciar_Fechamento_Cupom_Fiscal(decimal dcmDescontoComercial, decimal dcmDescontoItem)
        {
            try
            {
                return this.objImpressaoFiscal.Iniciar_Fechamento_Cupom(dcmDescontoComercial, dcmDescontoItem);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Terminar_Fechamento_Cupom_Fiscal(string strMensagemFechamento)
        {
            try
            {
                return this.objImpressaoFiscal.Terminar_Fechamento_Cupom(strMensagemFechamento);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Gerar_XML_Venda(ref string strXmlVenda)
        {
            try
            {
                return this.objImpressaoFiscal.Gerar_XML_Venda(ref strXmlVenda);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Gerar_Leiaute_XML_Venda(string strXmlVenda, Boolean blnPicotarComprovante = true)
        {
            try
            {
                return this.objImpressaoFiscal.Gerar_Leiaute_XML_Venda(strXmlVenda, blnPicotarComprovante);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Inserir_Forma_Pagamento(string strFormaPagamento, decimal dcmValorPagamento, decimal dcmValorTroco, string strOperadora)
        {
            try
            {
                return this.objImpressaoFiscal.Inserir_Forma_Pagamento(strFormaPagamento, dcmValorPagamento, dcmValorTroco, strOperadora);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Retorna_Dados_Fiscais_Impressora(ref string strArquivo)
        {
            try
            {
                return this.objImpressaoFiscal.Retorno_Dados_Fiscais(ref strArquivo);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        private Retorno_Impressora_Fiscal Aplicar_Desconto_Item(int intNumeroItem, decimal dcmValorUnitario)
        {
            try
            {
                return this.objImpressaoFiscal.Aplicar_Desconto_Item(intNumeroItem, dcmValorUnitario);
            }
            catch (Exception)
            {
                this.blnErroImpressoraFiscal = true;
                throw;
            }
        }

        #endregion

        #region "   Horario de Verão            "

        private string Retorna_Mudanca_Horario_Verao()
        {
            try
            {
                string strMensagemCaixa = MENSAGEM_OPERACAO_CAIXA_DISPONIVEL;
                strMensagemCaixa = new CaixaBUS().Verificar_Horario_Verao(this.objComunicacaoImpressoraFiscal, this.objImpressaoFiscal);

                return strMensagemCaixa;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Monitor Sitef       "

        private void Inicializar_Monitor_Sitef()
        {
            try
            {
                int intTimerSitefLoja = Root.Parametros_Sistema.Retornar_Parametro_Sistema_Valor_Por_Tipo("TIMER_SITEF_LOJA", this.intLojaID).DefaultInteger();
                this.tmrMonitorSitef.Tick += new EventHandler(this.trmMonitor_Sitef_Elapsed);
                this.tmrMonitorSitef.Interval = new TimeSpan(0, 0, 0, 0, intTimerSitefLoja);
                this.tmrMonitorSitef.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   Relogio             "

        private void Inicializar_Relogio()
        {
            try
            {
                this.tmrRelogio = new DispatcherTimer();
                this.tmrRelogio.Tick += new EventHandler(this.tmrRelogio_Elapsed);
                this.tmrRelogio.Interval = new TimeSpan(0, 0, 1);
                this.tmrRelogio.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region "   SAT                 "

        private void Preencher_Identificacao_Caixa_Sat()
        {
            try
            {

                if (this.objTipoImpressoraFiscal == Caixa_Tipo_Impressora_Fiscal.SAT && Root.Loja_Ativa != null)
                {

                    LojasBUS busLoja = new LojasBUS();
                    LojasDO dtoLojasSoftwareHouse = busLoja.Selecionar(Loja.Tucuruvi.DefaultInteger());
                    LojasDO dtoLojas = busLoja.Selecionar(Root.Loja_Ativa.ID.DefaultInteger());

                    if (dtoLojas != null && dtoLojasSoftwareHouse != null)
                    {
                        string strAssinatura = this.blnUtilizaControladorSat ? Root.Lojas_Parametros.Retorna_Valor_Parametro_Descritivo(Root.Loja_Ativa.ID, "SAT Assinatura do Aplicativo Comercial").DefaultString() : dtoSatCaixa.Assinatura_Digital;

                        // Identificação
                        this.Preencher_Identificacao_Caixa(Utilitario.Remover_Caracteres_CNPJ_CPF(dtoLojasSoftwareHouse.PessoaJuridica.CNPJ), strAssinatura, this.intNumeroGuiche.DefaultString());

                        // Identificação (Teste)
                        if (Root.Desenvolvedor)
                        {
                            this.Preencher_Identificacao_Caixa("16716114000172", "SGR-SAT SISTEMA DE GESTAO E RETAGUARDA DO SAT", this.intNumeroGuiche.DefaultString());
                        }

                        // Emitente
                        this.Preencher_Emitente_Caixa(dtoLojas);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        private bool Inicializar_Venda_Sat()
        {
            try
            {
                // Venda apenas com serviço, não deve ser enviado para o SAT.
                if (this.blnCupomAberto == false)
                {
                    return true;
                }

                if (this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    return true;
                }

                string strXmlVenda = string.Empty;

                if (this.Gerar_Xml_Venda_Sat(ref strXmlVenda) == false)
                {
                    return false;
                }

                this.Preencher_DataObject_Sat_Venda(strXmlVenda);

                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Processar_Venda_Sat()
        {
            try
            {
                // Venda apenas com serviço, não deve ser enviado para o SAT.
                if (this.blnCupomAberto == false || this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    return true;
                }

                // Incluir SAT Venda
                SAT_VendaBUS busVendaSat = new SAT_VendaBUS();
                if (this.dtoCaixaSatVenda.SAT_Venda_ID == 0)
                {
                    busVendaSat.Incluir(this.dtoCaixaSatVenda);
                }

                if (this.objTipoProcessoSatFiscal.Verificar_Comunicao_Aberta() == false)
                {
                    this.Inicializar_Comunicacao_Sat(0);
                }

                if (this.blnEnvioVendaSatRealizada == false)
                {
                    this.Envio_Venda_Sat(0);
                }

                this.Retorno_Venda_Sat(0);

                if (this.dtoCaixaSatVenda.Enum_Status_ID == Caixa_Sat_Venda_Status.Processado_Falha.DefaultInteger())
                {
                    // Registrar a falha no Banco de Dados
                    busVendaSat.Alterar(this.dtoCaixaSatVenda);
                    return false;
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Inicializar_Comunicacao_Sat(int intQuantidadeTentativas)
        {
            // Tratamento de erro específico para quando o sistema perde a conexão. Ele irá realizar 5 tentativas até devolver o erro para a tela.
            try
            {
                this.objTipoProcessoSatFiscal.Iniciar_Comunicacao();
            }
            catch (CommunicationException)
            {
                intQuantidadeTentativas++;

                if (intQuantidadeTentativas > TENTATIVAS_COMUNICACAO_SAT)
                {
                    throw new McException(new Erro(new ErroItem(CODIGO_ERRO_COMUNICACAO_SAT)));
                }
                else
                {
                    this.Inicializar_Comunicacao_Sat(intQuantidadeTentativas);
                }
            }
            catch (Exception)
            {
                intQuantidadeTentativas++;

                if (intQuantidadeTentativas > TENTATIVAS_COMUNICACAO_SAT)
                {
                    throw new McException(new Erro(new ErroItem(CODIGO_ERRO_COMUNICACAO_SAT)));
                }
                else
                {
                    this.Inicializar_Comunicacao_Sat(intQuantidadeTentativas);
                }
            }
        }

        private void Envio_Venda_Sat(int intQuantidadeTentativas)
        {
            ///Tratamento de erro específico para quando o sistema perde a conexão. Ele irá realizar 5 tentativas até devolver o erro para a tela.
            try
            {
                this.dtoCaixaSatVenda.Data_Envio = new DBUtil().Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                if (this.objTipoProcessoSatFiscal.Verificar_Comunicao_Aberta() == false)
                {
                    this.Inicializar_Comunicacao_Sat(0);
                }

                this.objTipoProcessoSatFiscal.Enviar_Venda_XML(this.intLojaID, this.dtoCaixaSatVenda.SAT_Venda_ID, this.dtoCaixaSatVenda.XML);

                this.blnEnvioVendaSatRealizada = true;
            }
            catch (CommunicationException)
            {
                intQuantidadeTentativas++;

                if (intQuantidadeTentativas > TENTATIVAS_COMUNICACAO_SAT)
                {
                    throw new McException(new Erro(new ErroItem(CODIGO_ERRO_COMUNICACAO_SAT_ENVIO)));
                }
                else
                {
                    this.Envio_Venda_Sat(intQuantidadeTentativas);
                }
            }
            catch (Exception)
            {
                intQuantidadeTentativas++;

                if (intQuantidadeTentativas > TENTATIVAS_COMUNICACAO_SAT)
                {
                    throw new McException(new Erro(new ErroItem(CODIGO_ERRO_COMUNICACAO_SAT_ENVIO)));
                }
                else
                {
                    this.Envio_Venda_Sat(intQuantidadeTentativas);
                }
            }
        }

        private void Retorno_Venda_Sat(int intQuantidadeTentativasConsultaProtocolo)
        {
            // Tratamento de erro específico para quando o sistema perde a conexão. Ele irá realizar 5 tentativas até devolver o erro para a tela.
            try
            {
                if (this.objTipoProcessoSatFiscal.Verificar_Comunicao_Aberta() == false)
                {
                    this.Inicializar_Comunicacao_Sat(0);
                }

                this.objTipoProcessoSatFiscal.Retorno_Venda_XML(ref this.dtoCaixaSatVenda);

            }
            catch (CommunicationException)
            {
                intQuantidadeTentativasConsultaProtocolo++;

                if (intQuantidadeTentativasConsultaProtocolo > TENTATIVAS_COMUNICACAO_SAT)
                {
                    throw new McException(new Erro(new ErroItem(CODIGO_ERRO_COMUNICACAO_SAT_CONSULTA)));
                }
                else
                {
                    this.txtMenu.Content = string.Format("Comunicando com o SAT. ({0}/{1})",
                                                     intQuantidadeTentativasConsultaProtocolo.DefaultString(),
                                                     TENTATIVAS_COMUNICACAO_SAT);
                    Utilitario.Processar_Mensagens_Interface_WPF();

                    this.objTipoProcessoSatFiscal.Fechar_Comunicacao();

                    Thread.Sleep(TEMPO_AGUARDO_TENTATIVA_COMUNICACAO_SAT);

                    this.Retorno_Venda_Sat(intQuantidadeTentativasConsultaProtocolo);
                }
            }
            catch (Exception)
            {
                intQuantidadeTentativasConsultaProtocolo++;

                if (intQuantidadeTentativasConsultaProtocolo > TENTATIVAS_COMUNICACAO_SAT)
                {
                    throw new McException(new Erro(new ErroItem(CODIGO_ERRO_COMUNICACAO_SAT_CONSULTA)));
                }
                else
                {
                    this.txtMenu.Content = string.Format("Comunicando com o SAT. ({0}/{1})",
                                                     intQuantidadeTentativasConsultaProtocolo.DefaultString(),
                                                     TENTATIVAS_COMUNICACAO_SAT);
                    Utilitario.Processar_Mensagens_Interface_WPF();

                    this.objTipoProcessoSatFiscal.Fechar_Comunicacao();

                    Thread.Sleep(TEMPO_AGUARDO_TENTATIVA_COMUNICACAO_SAT);

                    this.Retorno_Venda_Sat(intQuantidadeTentativasConsultaProtocolo);
                }
            }
        }

        private bool Gerar_Xml_Venda_Sat(ref string strXML)
        {
            try
            {
                Retorno_Impressora_Fiscal enuRetornoImpressoraFiscal = this.Gerar_XML_Venda(ref strXML);
                if (enuRetornoImpressoraFiscal != Retorno_Impressora_Fiscal.Impressora_Fiscal_Comando_Efetuado)
                {
                    this.txtMenu.Content = enuRetornoImpressoraFiscal.ToDescription();
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Imprimir_Comprovante_Sat(Boolean blnPicotarComprovante = true)
        {
            try
            {
                // Venda apenas com serviço, não deve ser enviado para o SAT.
                if (this.blnCupomAberto == false || this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    return;
                }

                this.Gerar_Leiaute_XML_Venda(this.dtoCaixaSatVenda.XML, blnPicotarComprovante);

            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Validar_Sat()
        {
            try
            {
                CaixaBUS busCaixa = new CaixaBUS();

                if (objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT || this.blnUtilizaControladorSat)
                {
                    return true;
                }

                string strMensagem = string.Empty;

                if (!busCaixa.Verifica_Sat_Disponivel_Caixa(this.dtoSatCaixa, ref strMensagem))
                {
                    Root.Impressora_Fiscal_Disponivel = false;
                    this.blnPortaImpressoraFiscal = false;
                    this.txtMenu.Content = strMensagem;
                    return false;
                }

                if (!busCaixa.Validar_Aparelho_Sat_Fisico(this.dtoSatCaixa, this.Retornar_Consulta_Status_Operacional_Sat()))
                {
                    Root.Impressora_Fiscal_Disponivel = false;
                    this.blnPortaImpressoraFiscal = false;
                    this.txtMenu.Content = "Número Serie do Sat Inválido.";
                    return false;
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public bool Inicializar_Comunicacao_Sat()
        {
            try
            {
                if (this.objTipoImpressoraFiscal != Caixa_Tipo_Impressora_Fiscal.SAT)
                {
                    return true;
                }

                if (!this.blnUtilizaControladorSat && !this.blnPortaImpressoraFiscal)
                {
                    this.Inicilizar_Objetos_Comunicacao_Impressora_Fiscal();

                    this.objTipoProcessoSatFiscal = new Tipo_Processo_Sat_Fiscal_Factory(this.blnUtilizaControladorSat, 0, this.dtoSatCaixa);

                    if (!this.Validar_Sat())
                    {
                        return false;
                    }
                }

                if (this.objTipoProcessoSatFiscal.Verificar_Comunicao_Aberta() == false)
                {
                    this.Inicializar_Comunicacao_Sat(0);
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string Retornar_Consulta_Status_Operacional_Sat()
        {
            try
            {
                return objTipoProcessoSatFiscal.Retornar_Consulta_Status_Operacional_Sat(this.dtoSatCaixa.SAT_ID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Historico Processo      "

        private void Registrar_Historico_Processo(Historico_Operacao objEnumHistoricoOperacao)
        {
            try
            {
                if (blnParametroHistoricoProcesso)
                {
                    new Historico_ProcessoBUS().Processar_Inclusao_Historico_Processo(Root.Loja_Ativa.ID, Historico_Processo.Caixa.DefaultInteger(), objEnumHistoricoOperacao.DefaultInteger(), 0, 0);
                }
            }
            catch (Exception)
            {
                // Caso ocorra falha, o processo do caixa continua.
            }
        }

        #endregion

        #region "   Reciclagem              "

        private bool Verificar_Produto_Reciclavel_Por_Romaneio()
        {
            try
            {

                if (this.blnUtilizaProdutoReciclavel
                    && this.dtsRomaneioTemporario.Tables["Romaneio_It"].Select(string.Concat(" Produto_Reciclavel = true and Romaneio_CT_ID = ", this.dtsPreVendaEscolhido.Tables["Romaneio_Pre_Venda_CT"].Rows[0]["Romaneio_Pre_Venda_CT_ID"],
                                                                                            " AND (Romaneio_It_Qtde - Romaneio_Pre_Venda_It_Qtde_Reciclavel) > 0 AND  Romaneio_It_Preco_Pago > 0")).Length > 0)
                {
                    this.txtMenu.Content = "Reciclável na loja? (S/N)";
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Confirmar_Produto_Reciclavel_Romaneio;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    return true;
                }
                if (!this.blnIsFuncionario)
                    this.txtMenu.Content = string.Empty;
                this.enuSituacao = Operacao.Operacao_Inicial;
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Verificar_Produto_Reciclavel_Por_Auto_Servico()
        {
            try
            {
                if (this.blnUtilizaProdutoReciclavel && this.produtoReciclavel)
                {
                    this.txtMenu.Content = "Reciclável na loja? (S/N)";
                    this.txtCodigoItemFabricante.Text = string.Empty;
                    this.txtQuantidade.Text = string.Empty;
                    this.txtComando.Text = string.Empty;
                    this.enuSituacao = Operacao.Confirmar_Produto_Reciclavel_Auto_Servico;
                    this.Tratar_Campo_Comando_Limpar_e_Tornar_Visivel_e_Setar_Foco(false, true, true);
                    return true;
                }

                return false;
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
