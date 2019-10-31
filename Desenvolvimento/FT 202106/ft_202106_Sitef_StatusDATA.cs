using System;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;
using System.Text;
using Mercadocar.InfraEstrutura.BancoDados;
using Mercadocar.InfraEstrutura.Utilidades;
using Mercadocar.InfraEstrutura.Validadores;
using Mercadocar.InfraEstrutura.Erro;

namespace Mercadocar.ObjetosNegocio.Data
{

    public class Sitef_StatusDATA
    {

        #region "   CRUDE              "

        public void Incluir(DataRow dtrSitefStatus, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Geral(dtrSitefStatus, objErro, ref objTransaction);
                if (objErro.TemErro())
                    return;

                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("INSERT INTO Sitef_Status ");
                stbSql.AppendLine("(");
                stbSql.AppendLine("Lojas_ID, ");
                stbSql.AppendLine("Enum_Status_ID");
                stbSql.AppendLine(") ");
                stbSql.AppendLine("VALUES ");
                stbSql.AppendLine("(");
                stbSql.AppendLine("@Lojas_ID, ");
                stbSql.AppendLine("@Enum_Status_ID");
                stbSql.AppendLine("); ");
                stbSql.AppendLine("SELECT SCOPE_IDENTITY()");

                dtrSitefStatus["Sitef_Status_ID"] = Convert.ToString(SqlHelper.ExecuteScalar(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString(), this.Preencher_Parametros(dtrSitefStatus)));

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Alterar(DataRow dtrSitefStatus, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Geral(dtrSitefStatus, objErro, ref objTransaction);
                if (objErro.TemErro())
                    return;

                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("UPDATE Sitef_Status ");
                stbSql.AppendLine("SET ");
                stbSql.AppendLine("Lojas_ID = @Lojas_ID, ");
                stbSql.AppendLine("Enum_Status_ID = @Enum_Status_ID ");
                stbSql.AppendLine("WHERE Sitef_Status_ID = @Sitef_Status_ID ");

                SqlHelper.ExecuteNonQuery(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString(), this.Preencher_Parametros(dtrSitefStatus));

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Alterar_Por_Loja(DataRow dtrSitefStatus, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                this.Validar_Geral(dtrSitefStatus, objErro, ref objTransaction);
                if (objErro.TemErro())
                    return;

                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("UPDATE Sitef_Status ");
                stbSql.AppendLine("SET ");
                stbSql.AppendLine("Enum_Status_ID = @Enum_Status_ID ");
                stbSql.AppendLine("WHERE Lojas_ID = @Lojas_ID ");

                SqlHelper.ExecuteNonQuery(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString(), this.Preencher_Parametros(dtrSitefStatus));

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Excluir(String strSitefStatusID, ref TransactionManager objTransaction)
        {
            try
            {
                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("DELETE FROM Sitef_Status ");
                stbSql.AppendLine("WHERE Sitef_Status_ID = @Sitef_Status_ID  ");

                SqlParameter[] objParametro = new SqlParameter[1];

                objParametro[0] = new SqlParameter("@Sitef_Status_ID", SqlDbType.UniqueIdentifier);
                objParametro[0].Value = strSitefStatusID;

                SqlHelper.ExecuteNonQuery(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString(), objParametro);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataRow Selecionar(String strSitefStatusID, ref TransactionManager objTransaction)
        {
            try
            {
                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("SELECT ");
                stbSql.AppendLine("Sitef_Status_ID, ");
                stbSql.AppendLine("Lojas_ID, ");
                stbSql.AppendLine("Enum_Status_ID ");
                stbSql.AppendLine("FROM Sitef_Status (NOLOCK) ");
                stbSql.AppendLine("WHERE Sitef_Status_ID = @Sitef_Status_ID  ");

                SqlParameter[] objParametro = new SqlParameter[1];

                objParametro[0] = new SqlParameter("@Sitef_Status_ID", SqlDbType.UniqueIdentifier);
                objParametro[0].Value = strSitefStatusID;

                DataTable dttResultado = SqlHelper.ExecuteDataTable(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString(), objParametro);

                if (dttResultado.Rows.Count > 0)
                {
                    return dttResultado.Rows[0];
                }
                else
                {
                    DataRow dtrRetorno = null;
                    dtrRetorno = dttResultado.NewRow();

                    return dtrRetorno;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataRow Selecionar_Por_Loja(int intLojasID, ref TransactionManager objTransaction)
        {
            try
            {
                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("SELECT ");
                stbSql.AppendLine("Sitef_Status_ID, ");
                stbSql.AppendLine("Lojas_ID, ");
                stbSql.AppendLine("Enum_Status_ID ");
                stbSql.AppendLine("FROM Sitef_Status (NOLOCK) ");
                stbSql.AppendLine("WHERE Lojas_ID = @Lojas_ID  ");

                SqlParameter[] objParametro = new SqlParameter[1];

                objParametro[0] = new SqlParameter("@Lojas_ID", SqlDbType.Int);
                objParametro[0].Value = intLojasID;

                DataTable dttResultado = SqlHelper.ExecuteDataTable(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString(), objParametro);

                if (dttResultado.Rows.Count > 0)
                {
                    return dttResultado.Rows[0];
                }
                else
                {
                    DataRow dtrRetorno = null;
                    dtrRetorno = dttResultado.NewRow();

                    return dtrRetorno;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable Consultar_DataTable(ref TransactionManager objTransaction)
        {
            try
            {
                String strFiltro = String.Empty;
                SqlParameter[] objParam = null;
                DBUtil objDBUtil = new DBUtil();

                // if (Parametro_Campo1 != String.Empty)
                // {             //    strFiltro += "(Campo_Tabela Like '%' + @Campo_Tabela + '%') "
                //    objParam = objDBUtil.Preencher_Parametro_EmUmArray(objParam, "@Campo_Tabela", SqlDbType.VarChar, Parametro_Campo1)
                // }

                if (strFiltro != String.Empty)
                {
                    return this.Consultar_DataTable(objParam, " WHERE " + strFiltro, ref objTransaction);
                }
                else
                {
                    return this.Consultar_DataTable(null, String.Empty, ref objTransaction);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable Consultar_DataTable(SqlParameter[] objParametros, String strSqlWhere, ref TransactionManager objTransaction)
        {
            try
            {
                StringBuilder stbSql = new StringBuilder();
                stbSql.AppendLine("SELECT ");
                stbSql.AppendLine("Sitef_Status_ID, ");
                stbSql.AppendLine("Lojas_ID, ");
                stbSql.AppendLine("Enum_Status_ID ");
                stbSql.AppendLine("FROM Sitef_Status (NOLOCK) ");

                String strOrdem = String.Empty;

                if (objParametros == null)
                {
                    return SqlHelper.ExecuteDataTable(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString() + strOrdem);
                }
                else
                {
                    return SqlHelper.ExecuteDataTable(objTransaction.ObjetoDeAcessoDados, CommandType.Text, stbSql.ToString() + strSqlWhere + strOrdem, objParametros);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable Retornar_Estrutura_Tabela()
        {
            try
            {
                DataTable dttEstrutura = new DataTable("Sitef_Status");

                dttEstrutura.Columns.Add("Sitef_Status_ID", typeof(String));
                dttEstrutura.Columns.Add("Lojas_ID", typeof(Int32));
                dttEstrutura.Columns.Add("Enum_Status_ID", typeof(Int32));

                return dttEstrutura;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Métodos Privados   "

        private SqlParameter[] Preencher_Parametros(DataRow dtrSitefStatus)
        {
            try
            {
                SqlParameter[] objParametro = new SqlParameter[3];

                objParametro[0] = new SqlParameter("@Sitef_Status_ID", SqlDbType.UniqueIdentifier, 16);
                objParametro[0].Value = dtrSitefStatus["Sitef_Status_ID"];

                objParametro[1] = new SqlParameter("@Lojas_ID", SqlDbType.Int);
                objParametro[1].Value = dtrSitefStatus["Lojas_ID"];

                objParametro[2] = new SqlParameter("@Enum_Status_ID", SqlDbType.Int);
                objParametro[2].Value = dtrSitefStatus["Enum_Status_ID"];

                return objParametro;

            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region "   Validações         "

        private void Validar_Geral(DataRow dtrSitefStatus, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {

                this.Validar_Lojas_ID(Convert.ToString(dtrSitefStatus["Lojas_ID"]), objErro, ref objTransaction);
                this.Validar_Enum_Status_ID(Convert.ToString(dtrSitefStatus["Enum_Status_ID"]), objErro, ref objTransaction);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Lojas_ID(String strValorCampo, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                ValidaBanco objValidaBanco = new ValidaBanco();

                objValidaBanco.Valida_Chave_Extrangeira("Lojas_ID", "Lojas_ID", strValorCampo, "Lojas_IsAtivo", "Lojas_ID", "Lojas", this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro, ref objTransaction);

                if ((objErro.ObterErro("Lojas_ID") != null))
                    return;

                ValidaCampo objValidaCampo = new ValidaCampo();


                objValidaCampo.ValidaCampoObrigatorio("Lojas_ID", strValorCampo, "Lojas_ID", true, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro);
                objValidaCampo.ValidaSoNumeros("Lojas_ID", strValorCampo, "Lojas_ID", this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro);
                objValidaCampo.ValidaCapacidadeDeVariavelSemCasaDecimal("Lojas_ID", strValorCampo, "Lojas_ID", SqlDbType.Int, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Validar_Enum_Status_ID(String strValorCampo, Erro objErro, ref TransactionManager objTransaction)
        {
            try
            {
                ValidaBanco objValidaBanco = new ValidaBanco();

                objValidaBanco.Valida_Chave_Extrangeira("Enum_ID", "Enum_Status_ID", strValorCampo, "Enum_IsAtivo", "Validar campo status", "Enumerado", this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro, ref objTransaction);

                if ((objErro.ObterErro("Enum_Status_ID") != null))
                    return;

                ValidaCampo objValidaCampo = new ValidaCampo();

                objValidaCampo.ValidaCampoObrigatorio("Enum_Status_ID", strValorCampo, "Status Sitef", true, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro);
                objValidaCampo.ValidaSoNumeros("Enum_Status_ID", strValorCampo, "Status Sitef", this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro);
                objValidaCampo.ValidaCapacidadeDeVariavelSemCasaDecimal("Enum_Status_ID", strValorCampo, "Status Sitef", SqlDbType.Int, this.GetType().Name + "." + MethodBase.GetCurrentMethod().Name, objErro);


            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }

}