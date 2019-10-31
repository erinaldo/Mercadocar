using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mercadocar.InfraEstrutura;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.InfraEstrutura.Servidor;
using Mercadocar.Enumerados;
using Mercadocar.Sitef;
using Mercadocar.Constantes;
using Mercadocar.RegrasNegocio;

namespace MC_WS_Monitor_Sitef
{
    public partial class MC_WS_Monitor_Sitef : ServiceBase
    {
        #region "   Declarações        "

        private System.Timers.Timer tmrTemporizador = new System.Timers.Timer();

        private int intLojasID = 0;

        #endregion

        #region "   Construtor         "

        public MC_WS_Monitor_Sitef()
        {
            this.InitializeComponent();
        }

        #endregion

        #region "   Eventos            "

        private void Executar_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Thread thrProcessamento = new Thread(this.Executar_Thread_Principal);
                thrProcessamento.Start();
            }
            catch (Exception ex)
            {
                Log.Erro("LOG_WS_MONITOR_SITEF", 0, ex);
            }
        }

        #endregion

        #region "   Métodos Herança    "

        protected override void OnStart(string[] strArgs)
        {


            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            ///Executar ao iniciar o serviço 
            Root.IniciarSistema(Root.FrontEnd.Configurador, Root.AcessoDoExecutavel.ServidorCentral);

            ///Gravar inicio no log
            Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço iniciado.");

            ///Adiciona o handler para o evento do timer (contador)
            this.tmrTemporizador.Elapsed += this.Executar_Timer_Elapsed;

            ///Configurar o timer para a cada start executa o método apenas uma vez
            this.tmrTemporizador.AutoReset = false;

            ///Inicia a primeira contagem, chamando direto a thread principal.
            Thread thrProcessamento = new Thread(this.Executar_Thread_Principal);
            thrProcessamento.Start();
        }

        protected override void OnStop()
        {
            try
            {
                // Gravar no log
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço encerrado.");
            }
            catch (Exception ex)
            {
                Log.Erro("LOG_WS_MONITOR_SITEF", 0, ex);
            }
        }

        #endregion

        #region "   Métodos Privados   "

        private void Executar_Thread_Principal()
        {
            try
            {
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Inicio da execução da thread.");

                // Localiza a loja
                if (this.Identificar_Loja_Ativa())
                {
                    // Inicia a rotina do Monitor Sitef
                    this.Processar_Monitor_Sitef();
                }

                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Fim da execução da thread principal.");

                // Se a thread for executada normalmente mantém o intervalo determinado
                this.tmrTemporizador.Interval = Root.Parametros_Sistema.Retornar_Parametro_Sistema_Valor_Por_Tipo("TIMER_MONITOR_SITEF", this.intLojasID).DefaultInteger();

            }
            catch (Exception ex)
            {
                Log.Erro("LOG_WS_MONITOR_SITEF", 0, ex);
                ///Se der erro na execução, reinicia o job em apenas 1 segundo
                this.tmrTemporizador.Interval = Constantes_Sitef.SITEF_MONITOR_TEMPO_INTERVALO_TIMER_ERRO;
            }
            finally
            {
                ///Ao fim da execução da rotina, reinicia o timer contador para executar novamente após N milisegundos
                this.tmrTemporizador.Start();
            }
        }

        private bool Identificar_Loja_Ativa()
        {
            try
            {
                // Identifica o IP Local
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Verifica IP Local.");
                Rede objRede = new Rede();
                string strEnderecoIPLocal = objRede.Obter_Endereco_IP();

                // Identifica a Loja com o ip da tabela conexão
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Identifica Loja.");
                DataTable dttConexao = Root.Servidor.Consultar_DataTable();
                DataRow[] dtrConexaoLocal = dttConexao.Select("Servidor_Endereco = '" + strEnderecoIPLocal + "' AND Enum_Servidor_Tipo_ID = " + Servidor.Tipo_Servidor.Banco_De_Dados.DefaultInteger());
                if (dtrConexaoLocal.Length == 0)
                {
                    return false;
                }

                this.intLojasID = dtrConexaoLocal[0]["Lojas_ID"].DefaultInteger();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Processar_Monitor_Sitef()
        {
            try
            {
                // Obter o endereço de Ip Primario
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Obtem o link do IP Primario.");
                string strEnderecoIPrimario = Root.Servidor.Retornar_Endereco_Servidor(this.intLojasID, Servidor.Tipo_Servidor.Sitef, Constantes_Sitef.SITEF_MONITOR_DESCRICAO_SERVIDOR_SITEF_PRIMARIO);

                // Obter o endereço de Ip Secundario
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Obtem o link do IP Secundario.");
                string strEnderecoIPSecundario = Root.Servidor.Retornar_Endereco_Servidor(this.intLojasID, Servidor.Tipo_Servidor.Sitef, Constantes_Sitef.SITEF_MONITOR_DESCRICAO_SERVIDOR_SITEF_SECUNDARIO);

                // Obter o endereço de IP de conexão
                Log.Info("LOG_WS_MONITOR_SITEF", 0, "Obtem paramentro ip do servidor de conexao.");
                string strEnderecoConexaoIP = Root.Parametros_Sistema.Retornar_Parametro_Sistema_Valor_Por_Tipo("IP_SITEF_SERVIDOR_CONEXAO", this.intLojasID);

                bool blnFalhaSitef = false;
                Rede objRede = new Rede();
                // Servidor Loja
                if (strEnderecoConexaoIP != null && strEnderecoConexaoIP != string.Empty)
                {
                    // Verificar se o link está ativo
                    Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço loja: Verifica link ativo.");
                    if (objRede.Ping(strEnderecoConexaoIP, Constantes_Sitef.SITEF_MONITOR_PING_TIMEOUT) == false)
                    {
                        // Em caso de falha registra na tabela Sitef_Log_Loja
                        Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço loja: Registra falha no link.");
                        this.Registrar_Sitef_Log_Loja(this.Preencher_DataRow_Sitef_Log_Loja(this.intLojasID, Status_Sitef.Link_OffLine));
                        blnFalhaSitef = true;
                    }
                }
                else
                {
                    // Servidor Conexao
                    // Verificar se o link está ativo
                    Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Verifica o link do IP Primario.");

                    if (objRede.Ping(strEnderecoIPrimario, Constantes_Sitef.SITEF_MONITOR_PING_TIMEOUT) == false)
                    {
                        // Em caso de falha registra na tabela Sitef_Log_Servico
                        Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Registra falha no link Primario.");
                        this.Registrar_Sitef_Log_Servico(this.Preencher_DataRow_Sitef_Log_Servico(strEnderecoIPrimario, Status_Sitef.Link_OffLine));
                    }

                    // Verifica se o Sitef está ativo
                    Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Verifica monitor sitef online.");
                    string strMensgem = string.Empty;
                    if (this.Monitor_Sitef(strEnderecoIPrimario, ref strMensgem) == false)
                    {
                        // Em caso de falha registra na tabela Sitef_Log_Servico
                        Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Falha na conexão com Sitef. " + strMensgem);
                        this.Registrar_Sitef_Log_Servico(this.Preencher_DataRow_Sitef_Log_Servico(strEnderecoIPrimario, Status_Sitef.OffLine));
                    }
                }

                if (blnFalhaSitef)
                {
                    // FALHA NO LINK: Atualiza a tabela Sitef_Status
                    Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço loja: Atualiza tabela Sitef_Status, falha no link. ");
                    this.Registrar_Sitef_Status(this.Preencher_DataRow_Sitef_Status(this.intLojasID, Status_Sitef.OffLine));
                }
                else
                {
                    // Verifica se existe registro de log na tabela Sitef_Log_Servico no servidor Primario e Secundario
                    DBUtil objUtil = new DBUtil();
                    DateTime dtmSitefLogServicoData = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                    int intTimerIdentificaFalha = Root.Parametros_Sistema.Retornar_Parametro_Sistema_Valor_Por_Tipo("TIMER_MONITOR_SITEF_IDENTIFICA_FALHA").DefaultInteger();

                    double dblIdentificaFalhaMinutos = TimeSpan.FromMilliseconds(intTimerIdentificaFalha).TotalMinutes;

                    DateTime dtmDataIdentificaFalha = new DateTime(dtmSitefLogServicoData.Year, dtmSitefLogServicoData.Month, dtmSitefLogServicoData.Day, dtmSitefLogServicoData.Hour, (dtmSitefLogServicoData.Minute - dblIdentificaFalhaMinutos).DefaultInteger(), dtmSitefLogServicoData.Second);
                    Sitef_Log_ServicoBUS busSitefLogServico = new Sitef_Log_ServicoBUS();
                    if (busSitefLogServico.Selecionar_Por_Servidor_Data(strEnderecoIPrimario, dtmDataIdentificaFalha).Rows.Count > 0
                        && busSitefLogServico.Selecionar_Por_Servidor_Data(strEnderecoIPSecundario, dtmDataIdentificaFalha).Rows.Count > 0)
                    {
                        // FALHA: Atualiza a tabela Sitef_Status, falha no servidor primario
                        Log.Info("LOG_WS_MONITOR_SITEF", 0, "Serviço conexao: Atualiza tabela Sitef_Status, falha servidor primario ou secundario.");
                        this.Registrar_Sitef_Status(this.Preencher_DataRow_Sitef_Status(this.intLojasID, Status_Sitef.OffLine));
                    }
                    else
                    {
                        // Atualiza a tabela Sitef_Status para ONLINE
                        Log.Info("LOG_WS_MONITOR_SITEF", 0, "Atualiza tabela Sitef_Status, sitef online.");
                        this.Registrar_Sitef_Status(this.Preencher_DataRow_Sitef_Status(this.intLojasID, Status_Sitef.Online));
                    }

                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool Monitor_Sitef(string strServidor, ref string strMensagem)
        {
            try
            {
                SitefDO dtoSitefMonitor = new SitefDO();

                return dtoSitefMonitor.Monitor_Sitef(strServidor, ref strMensagem);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Preencher_DataRow_Sitef_Log_Loja(int intParamLojasID, Status_Sitef enuStatusSitef)
        {
            try
            {
                Sitef_Log_LojaBUS busSitefLogLoja = new Sitef_Log_LojaBUS();
                DataTable dttSitefLogLoja = busSitefLogLoja.Retornar_Estrutura_Tabela();

                DataRow dtrSitefLogLoja = dttSitefLogLoja.NewRow();

                dtrSitefLogLoja["Lojas_ID"] = intParamLojasID;
                dtrSitefLogLoja["Enum_Status_ID"] = enuStatusSitef;

                DBUtil objUtil = new DBUtil();
                DateTime dtmSitefLogLojaData = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                dtrSitefLogLoja["Sitef_Log_Loja_Data"] = dtmSitefLogLojaData;
                dtrSitefLogLoja["Sitef_Log_Loja_Observacao"] = string.Empty;

                dttSitefLogLoja.Rows.Add(dtrSitefLogLoja);

                return dttSitefLogLoja;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private DataTable Preencher_DataRow_Sitef_Log_Servico(string strEnderecoIPrimario, Status_Sitef enuStatusSitef)
        {

            try
            {
                Sitef_Log_ServicoBUS busSitefLogServico = new Sitef_Log_ServicoBUS();
                DataTable dttSitefLogServico = busSitefLogServico.Retornar_Estrutura_Tabela();

                DataRow dtrSitefLogServico = dttSitefLogServico.NewRow();

                dtrSitefLogServico["Sitef_Log_Servico_IP_Servidor"] = strEnderecoIPrimario;
                dtrSitefLogServico["Enum_Status_ID"] = enuStatusSitef;
                DBUtil objUtil = new DBUtil();
                DateTime dtmSitefLogServicoData = objUtil.Obter_Data_do_Servidor(true, TipoServidor.LojaAtual);

                dtrSitefLogServico["Sitef_Log_Servico_Data"] = dtmSitefLogServicoData;
                dtrSitefLogServico["Sitef_Log_Servico_Observacao"] = string.Empty;

                dttSitefLogServico.Rows.Add(dtrSitefLogServico);

                return dttSitefLogServico;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private DataTable Preencher_DataRow_Sitef_Status(int intParamLojasID, Status_Sitef enuStatusSitef)
        {

            try
            {

                Sitef_StatusBUS busSitefStatus = new Sitef_StatusBUS();
                DataTable dttSitefStatus = busSitefStatus.Retornar_Estrutura_Tabela();

                DataRow dtrSitefStatus = dttSitefStatus.NewRow();
                dtrSitefStatus["Lojas_ID"] = intParamLojasID;
                dtrSitefStatus["Enum_Status_ID"] = enuStatusSitef;

                dttSitefStatus.Rows.Add(dtrSitefStatus);

                return dttSitefStatus;
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void Registrar_Sitef_Log_Loja(DataTable dttSitefLogLoja)
        {
            try
            {
                if (dttSitefLogLoja.Rows.Count == 0)
                {
                    return;
                }

                Sitef_Log_LojaBUS busSitefLogLoja = new Sitef_Log_LojaBUS();

                busSitefLogLoja.Incluir(dttSitefLogLoja.Rows[0]);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Registrar_Sitef_Log_Servico(DataTable dttSitefLogServico)
        {
            try
            {
                if (dttSitefLogServico.Rows.Count == 0)
                {
                    return;
                }

                Sitef_Log_ServicoBUS busSitefLogServico = new Sitef_Log_ServicoBUS();
                busSitefLogServico.Incluir(dttSitefLogServico.Rows[0]);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Registrar_Sitef_Status(DataTable dttSitefStatus)
        {
            try
            {
                if (dttSitefStatus.Rows.Count == 0)
                {
                    return;
                }

                Sitef_StatusBUS busSitefStatus = new Sitef_StatusBUS();
                busSitefStatus.Alterar_Por_Loja(dttSitefStatus.Rows[0]);
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

    }
}
