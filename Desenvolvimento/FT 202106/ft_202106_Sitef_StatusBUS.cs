using System;
using System.Data;
using Mercadocar.InfraEstrutura.Erro;
using Mercadocar.InfraEstrutura.BancoDados;
using Mercadocar.ObjetosNegocio.Data;

namespace Mercadocar.RegrasNegocio
{

    public class Sitef_StatusBUS
    {

        #region "   CRUDE              "

        public void Incluir(DataRow dtrIncluir)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);
                objTransaction.BeginTransaction();

                this.Incluir(dtrIncluir, ref objTransaction);

                if (objTransaction != null)
                    objTransaction.Commit();
            }
            catch (Exception)
            {
                if (objTransaction != null)
                    objTransaction.RollBack();
                throw;
            }
        }

        public void Incluir(DataRow dtrIncluir, ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();
                Erro objErro = new Erro();

                datSitefStatus.Incluir(dtrIncluir, objErro, ref objTransaction);
                if (objErro.TemErro())
                {
                    throw new McException(objErro);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Alterar(DataRow dtrAlterar)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);
                objTransaction.BeginTransaction();

                this.Alterar(dtrAlterar, ref objTransaction);

                if (objTransaction != null)
                {
                    objTransaction.Commit();
                }

                return true;
            }
            catch (Exception)
            {
                if (objTransaction != null)
                    objTransaction.RollBack();
                throw;
            }
        }

        public bool Alterar(DataRow dtrAlterar, ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();
                Erro objErro = new Erro();

                datSitefStatus.Alterar(dtrAlterar, objErro, ref objTransaction);

                if (objErro.TemErro())
                {
                    throw new McException(objErro);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Alterar_Por_Loja(DataRow dtrAlterar)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);
                objTransaction.BeginTransaction();

                this.Alterar_Por_Loja(dtrAlterar, ref objTransaction);

                if (objTransaction != null)
                {
                    objTransaction.Commit();
                }

                return true;
            }
            catch (Exception)
            {
                if (objTransaction != null)
                    objTransaction.RollBack();
                throw;
            }
        }

        public bool Alterar_Por_Loja(DataRow dtrAlterar, ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();
                Erro objErro = new Erro();

                datSitefStatus.Alterar_Por_Loja(dtrAlterar, objErro, ref objTransaction);

                if (objErro.TemErro())
                {
                    throw new McException(objErro);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
      
        public bool Excluir(String strSitefStatusID)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);
                objTransaction.BeginTransaction();

                this.Excluir(strSitefStatusID, ref objTransaction);

                if (objTransaction != null)
                {
                    objTransaction.Commit();
                }
                return true;
            }
            catch (Exception)
            {
                if (objTransaction != null)
                    objTransaction.RollBack();
                throw;
            }
        }

        public bool Excluir(String strSitefStatusID, ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();

                datSitefStatus.Excluir(strSitefStatusID, ref objTransaction);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataRow Selecionar(String strSitefStatusID)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);

                return this.Selecionar(strSitefStatusID, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.CloseConnection();
                }
            }
        }

        public DataRow Selecionar(String strSitefStatusID, ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();

                return datSitefStatus.Selecionar(strSitefStatusID, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataRow Selecionar_Por_Loja(int intLojasID)
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);

                return this.Selecionar_Por_Loja(intLojasID, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.CloseConnection();
                }
            }
        }

        public DataRow Selecionar_Por_Loja(int intLojasID, ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();

                return datSitefStatus.Selecionar_Por_Loja(intLojasID, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable Consultar_DataTable()
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);

                return this.Consultar_DataTable(ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.CloseConnection();
                }
            }
        }

        public DataTable Consultar_DataTable(ref TransactionManager objTransaction)
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();

                return datSitefStatus.Consultar_DataTable(ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable Consultar_DataTable_Grid()
        {
            TransactionManager objTransaction = null;

            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidoDaLojaAtual);

                // TODO - Adaptar código comentado para a consulta que preenche telas do tipo GRID
                // Consultas.ComprasBUS busConsulta = new Consultas.ComprasBUS();
                // return busConsulta.Consultar_DataTable_Grid(objTransaction);
                return null;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.CloseConnection();
                }
            }
        }

        public DataTable Retornar_Estrutura_Tabela()
        {
            try
            {
                Sitef_StatusDATA datSitefStatus = new Sitef_StatusDATA();

                return datSitefStatus.Retornar_Estrutura_Tabela();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

    }

}