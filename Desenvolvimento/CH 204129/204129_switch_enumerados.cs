
switch ((Enumerados.Tipo_Documento_Venda)this.cboTipoDocumento.SelectedValue.DefaultInteger())
{
    case 0:
        dtmDataInicial = this.dtpDataInicial.Value;
        dtmDataFinal = this.dtpDataFinal.Value;
        break;

    case Enumerados.Tipo_Documento_Venda.Romaneio:
        if (this.txtNumeroDocumento.TextLength > 9)
        {
            MessageBox.Show("Informe o número do Romaneio corretamente.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            this.txtNumeroDocumento.Focus();
            return;
        }
        strNumeroDocumento = this.txtNumeroDocumento.Text;
        intTipoDocumento = Enumerados.Tipo_Documento_Venda.Romaneio.DefaultInteger();
        break;

    case Enumerados.Tipo_Documento_Venda.Grupo_da_Venda:
        if (this.txtNumeroDocumento.TextLength > 9)
        {
            MessageBox.Show("Informe o número do Grupo da Venda corretamente.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            this.txtNumeroDocumento.Focus();
            return;
        }
        strNumeroDocumento = this.txtNumeroDocumento.Text;
        intTipoDocumento = Enumerados.Tipo_Documento_Venda.Grupo_da_Venda.DefaultInteger();
        break;

    default:
        strNumeroDocumento = this.txtNumeroDocumento.Text;
        intTipoDocumento = this.cboTipoDocumento.SelectedValue.DefaultInteger();
        break;
}