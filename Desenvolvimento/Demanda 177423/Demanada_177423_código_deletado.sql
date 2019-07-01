public DataSet Preencher_DataSet_Pedido_Compra(DataTable dttPendenciaCompra, ref DataSet dtsPropriedadesPedido)
        {
            TransactionManager objTransaction = null;
            try
            {
                objTransaction = new TransactionManager(TransactionManager.OpcoesDeDataSource.ServidorCentral);

                return this.Preencher_DataSet_Pedido_Compra(dttPendenciaCompra, ref dtsPropriedadesPedido, ref objTransaction);
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

        public DataSet Preencher_DataSet_Pedido_Compra(DataTable dttPendenciaCompra, ref DataSet dtsPropriedadesPedido, ref TransactionManager objTransaction)
        {
            try
            {
                Pedido_CompraBUS busPedidoCompra = new Pedido_CompraBUS();
                return busPedidoCompra.Preencher_DataSet_Pedido_Compra(dttPendenciaCompra, ref dtsPropriedadesPedido, ref objTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        
        //Código sem chamada - mantindo para possível implementação futura (Demanda 177423)
        public DataSet Preencher_DataSet_Pedido_Compra(DataTable dttItens, ref DataSet dtsPropriedadesPedido, ref TransactionManager objTransaction)
        {
            try
            {
                dtsPropriedadesPedido = this.Consultar_Data_Set_Pedido_Compra(0, ref objTransaction);
                DataRow dtrCapaPedido = dtsPropriedadesPedido.Tables["Pedido_Compra_CT"].NewRow();
                dtrCapaPedido["Loja_Origem_ID"] = ((LojasDO)Root.Loja_Ativa_NEW).ID;
                dtrCapaPedido["Loja_Destino_ID"] = Enumerados.Loja.CD_Pirituba.ToInteger();
                dtrCapaPedido["Loja_Faturamento_ID"] = Constantes.Loja_Faturamento.LOJA_ID_FATURAMENTO;
                dtrCapaPedido["Fornecedor_ID"] = 0;
                dtrCapaPedido["Enum_Status_ID"] = Enumerados.Status_Pedido_Compra.Pendente_de_Autorizacao.ToInteger();
                dtrCapaPedido["Usuario_Geracao_ID"] = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID;
                dtrCapaPedido["Usuario_Comprador_ID"] = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID;
                dtrCapaPedido["Enum_Tipo_ID"] = Mercadocar.Enumerados.Tipo_Pedido.Pedido.ToInteger();
                dtrCapaPedido["Enum_Tipo_Origem_Extenso"] = Enumerados.Origem_Pedido_Compras.Pendencia_de_Compra.ToDescription();
                dtrCapaPedido["Enum_Tipo_Origem_ID"] = Enumerados.Origem_Pedido_Compras.Pendencia_de_Compra.ToInteger();
                dtrCapaPedido["Objeto_Origem_ID"] = 0;
                dtrCapaPedido["Pedido_Compra_CT_Saldo_Origem_ID"] = 0;
                dtrCapaPedido["Pedido_Compra_CT_Vendedor"] = string.Empty;
                dtrCapaPedido["Pedido_Compra_CT_Data_Geracao"] = new DateTime(1900, 1, 1);
                dtrCapaPedido["Pedido_Compra_CT_Data_Prevista"] = new DateTime(1900, 1, 1);
                dtrCapaPedido["Pedido_Compra_CT_Desconto"] = 0;
                dtrCapaPedido["Pedido_Compra_CT_IPI"] = 0;
                dtrCapaPedido["Pedido_Compra_CT_Substituicao"] = 0;
                dtrCapaPedido["Pedido_Compra_CT_Obs"] = string.Empty;
                dtrCapaPedido["Pedido_Compra_CT_Cobranca_Fornecedor"] = string.Empty;
                dtrCapaPedido["Pedido_Compra_CT_Considerar_IPI"] = 0;
                dtrCapaPedido["Forn_IsOptanteSimples"] = false;
                dtrCapaPedido["Pedido_Compra_CT_Remover_Impostos"] = 0;
                dtsPropriedadesPedido.Tables["Pedido_Compra_CT"].Rows.Add(dtrCapaPedido);

                int intSequencia = 1;
                foreach (DataRow dtrItem in dttItens.Rows)
                {
                    Pedido_Compra_ITDO dtoItem = new Pedido_Compra_ITDO();
                    Boolean blnInsere = false;

                    if (Convert.ToBoolean(dtrItem["Selecao_Item"]))
                    {
                        int intPedidoCompraITQuantidade = 0;
                        for (int intItem = 0; intItem < dtsPropriedadesPedido.Tables["Pedido_Compra_IT"].Rows.Count; intItem++)
                        {
                            if (dtrItem["Peca_ID"].ToInteger() == dtsPropriedadesPedido.Tables["Pedido_Compra_IT"].Rows[intItem]["Peca_ID"].ToInteger())
                            {
                                intPedidoCompraITQuantidade += dtrItem["Qtde_Solicitado"].ToInteger();
                                dtsPropriedadesPedido.Tables["Pedido_Compra_IT"].Rows[intItem]["Pedido_Compra_IT_Quantidade"] = intPedidoCompraITQuantidade;
                                blnInsere = true;
                            }
                        }
                        if (!blnInsere)
                        {
                            DataSet dtsResumoPecaPedidoCompra = this.Consultar_Resumo_Peca_Pedido_Compra_Propriedade(dtrItem["Peca_ID"].ToInteger(),
                                                                                                                     dtsPropriedadesPedido.Tables["Pedido_Compra_CT"].Rows[0]["Fornecedor_ID"].ToInteger(),
                                                                                                                     ref objTransaction, true);

                            DataRow[] dtrEmbalagemUtilizada = dtsResumoPecaPedidoCompra.Tables["Peca_Embalagem"].Select("Peca_Embalagem_ID = " + Convert.ToInt32(dtrItem["Peca_Embalagem_Compra_ID"]));

                            DataRow dtrItemPedido = null;

                            dtrItemPedido = dtsPropriedadesPedido.Tables["Pedido_Compra_IT"].NewRow();
                            dtrItemPedido["Pedido_Compra_IT_Sequencia"] = intSequencia;
                            dtrItemPedido["Cod_Mercadocar"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Cod_Mercadocar"].ToString();
                            dtrItemPedido["Pedido_Compra_IT_Quantidade"] = Convert.ToInt32(dtrItem["Qtde_Solicitado"]);
                            dtrItemPedido["Peca_Embalagem_Descricao"] = dtrEmbalagemUtilizada[0]["Peca_Embalagem_Descricao"].ToString();
                            dtrItemPedido["Pedido_Compra_IT_Custo_Compra"] = Convert.ToDecimal(dtrItem["Custo_Embalagem"]);
                            dtrItemPedido["Pedido_Compra_IT_Desconto"] = 0;
                            dtrItemPedido["Pedido_Compra_IT_Valor_Desconto"] = 0;
                            dtrItemPedido["Pedido_Compra_IT_ICMS"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_ICMS_Compra"].ToDecimal();
                            dtrItemPedido["Pedido_Compra_IT_Substituicao"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_ICMS_Substituicao_Tributaria"].ToDecimal();
                            dtrItemPedido["Pedido_Compra_IT_Imposto"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_Perc_IPI"].ToDecimal();
                            dtrItemPedido["Peca_Margem_Lucro"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_Margem_Lucro"].ToDecimal();
                            dtrItemPedido["Preco_Venda"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Preco_Venda"].ToDecimal();
                            dtrItemPedido["Parcialmente_Recebido"] = string.Empty;
                            dtrItemPedido["Pedido_Compra_IT_ID"] = 0;
                            dtrItemPedido["Pedido_Compra_CT_ID"] = 0;
                            dtrItemPedido["Peca_ID"] = dtrItem["Peca_ID"].ToInteger();
                            dtrItemPedido["Fabricante_CD"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Fabricante_CD"].ToString();
                            dtrItemPedido["Produto_CD"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Produto_CD"].ToString();
                            dtrItemPedido["Peca_CD"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_CD"].ToString();
                            dtrItemPedido["Peca_CdFabricante"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_CDFabricante"].ToString();
                            dtrItemPedido["Peca_Conv_ID"] = 0;
                            if (dtsResumoPecaPedidoCompra.Tables["Peca_Codigo_Fornecedor"].Rows.Count > 0)
                            {
                                dtrItemPedido["Peca_Codigo_Fornecedor_ID"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_Codigo_Fornecedor_ID"].ToInteger();
                            }
                            else
                            {
                                dtrItemPedido["Peca_Codigo_Fornecedor_ID"] = 0;
                            }
                            dtrItemPedido["Pedido_Compra_IT_Qtde_Recebida"] = 0;
                            dtrItemPedido["Peca_Embalagem_Quantidade"] = dtrEmbalagemUtilizada[0]["Peca_Embalagem_Quantidade"].ToInteger();
                            dtrItemPedido["Peca_Embalagem_Compra_ID"] = Convert.ToInt32(dtrEmbalagemUtilizada[0]["Peca_Embalagem_ID"]);
                            dtrItemPedido["Enum_Tipo_Embalagem"] = 0;
                            dtrItemPedido["Quantidade_Total"] = dtrItem["Qtde_Solicitado"].ToInteger() * dtrEmbalagemUtilizada[0]["Peca_Embalagem_Quantidade"].ToInteger();
                            dtrItemPedido["Pedido_Compra_IT_Custo_Unitario"] = Convert.ToDecimal(dtrItem["Custo_Embalagem"]);
                            dtrItemPedido["Pedido_Compra_IT_Custo_Efetivo"] = 0;
                            dtrItemPedido["Fabricante_NmFantasia"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Fabricante_NmFantasia"].ToString();
                            dtrItemPedido["Produto_DS"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Produto_DS"].ToString();
                            dtrItemPedido["Peca_DsTecnica"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_DSTecnica"].ToString();
                            dtrItemPedido["Quantidade_Recebida"] = 0;
                            dtrItemPedido["Estoque_Total"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Estoque_Total"].ToString();
                            dtrItemPedido["Venda_Media_Total"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Venda_Media_Total"].ToString();
                            dtrItemPedido["Fabricante_ID"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Fabricante_ID"].ToInteger();
                            dtrItemPedido["Produto_ID"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Produto_ID"].ToInteger();
                            dtrItemPedido["Usuario_Ultima_Alteracao_ID"] = ((UsuarioDO)Root.Funcionalidades.Usuario_Ativo).ID;
                            dtrItemPedido["Custo_Reposicao"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Custo_Reposicao"].ToDecimal();
                            dtrItemPedido["Solicitar_Distribuicao"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Solicitar_Distribuicao"];
                            dtrItemPedido["Ultimo_Custo"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Ultimo_Custo"].ToDecimal();
                            dtrItemPedido["Enum_Sigla"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Enum_Sigla_Embalagem"].ToString();
                            dtrItemPedido["Peca_QtMinimaVenda"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_QtMinimaVenda"].ToInteger();
                            dtrItemPedido["Comissao_ID"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Comissao_ID"].ToInteger();
                            dtrItemPedido["Peca_TVA"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_TVA"].ToDecimal();
                            dtrItemPedido["Pedido_Compra_IT_Remover_Impostos"] = 0;
                            dtrItemPedido["Pedido_Compra_IT_Considerar_IPI"] = 0;
                            dtrItemPedido["Peca_Qtde_Multipla_Compra"] = dtsResumoPecaPedidoCompra.Tables["Peca"].Rows[0]["Peca_Qtde_Multipla_Compra"].ToInteger();
                            dtrItemPedido["QTDE_Itens_No_Pre_Recebimento"] = 0;
                            dtrItemPedido["Valor_Total_Unitario"] = dtrItemPedido["Quantidade_Total"].ToInteger() * dtrItemPedido["Pedido_Compra_IT_Custo_Unitario"].ToDecimal();
                            dtrItemPedido["Valor_Total_Compra"] = this.Calcular_Valor_Total_Compra(dtrItemPedido);

                            dtsPropriedadesPedido.Tables["Pedido_Compra_IT"].Rows.Add(dtrItemPedido);
                            intSequencia += 1;

                            // Insere Peca_Codigo_Fornecedor        
                            if (dtsResumoPecaPedidoCompra.Tables["Peca_Codigo_Fornecedor"].Rows.Count > 0)
                            {
                                foreach (DataRow dtrPecaEmbalagem in dtsResumoPecaPedidoCompra.Tables["Peca_Codigo_Fornecedor"].Rows)
                                {
                                    dtsPropriedadesPedido.Tables["Peca_Codigo_Fornecedor"].ImportRow(dtrPecaEmbalagem);
                                }
                            }

                            // Insere as Embalagens da peça
                            if (dtsResumoPecaPedidoCompra.Tables["Peca_Embalagem"].Rows.Count > 0)
                            {
                                foreach (DataRow dtrPecaEmbalagem in dtsResumoPecaPedidoCompra.Tables["Peca_Embalagem"].Rows)
                                {
                                    dtsPropriedadesPedido.Tables["Peca_Embalagem"].ImportRow(dtrPecaEmbalagem);
                                }
                            }

                            // Insere as Pre Distribuicao da peça em branco
                            if (dtsResumoPecaPedidoCompra.Tables["Pre_Distribuicao"].Rows.Count > 0)
                            {
                                foreach (DataRow dtrItemPreDistribuicao in dtsResumoPecaPedidoCompra.Tables["Pre_Distribuicao"].Rows)
                                {
                                    dtsPropriedadesPedido.Tables["Pre_Distribuicao"].ImportRow(dtrItemPreDistribuicao);
                                }
                            }
                        }
                    }
                }
                dtsPropriedadesPedido.Tables.Add(dttItens);
                return dtsPropriedadesPedido;
            }
            catch (Exception)
            {
                throw;
            }
        }