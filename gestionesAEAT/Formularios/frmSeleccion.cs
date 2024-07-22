using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace gestionesAEAT.Formularios
{
    public partial class frmSeleccion : Form
    {
        private int columnaOrdenada = -1;
        private ListSortDirection ordenColumna = ListSortDirection.Ascending;
        private GestionCertificados instanciaCertificado;
        public ElementosCertificado certificadoSeleccionado { get; set; }
        private List<ElementosCertificado> certificados;


        public frmSeleccion(GestionCertificados instanciaCertificado)
        {
            InitializeComponent();
            this.instanciaCertificado = instanciaCertificado;
            certificadoSeleccionado = new ElementosCertificado();
            certificados = instanciaCertificado.relacionCertificados();
            rellenarDGV(certificados);
            this.Load += frmSeleccion_Load;
        }

        private void rellenarDGV(List<ElementosCertificado> certificados)
        {
            //Metodo para rellenar el listado de certificados con sus columnas (necesario para regrabar en el caso de ordenacion)
            //dgvCertificados.DataSource = null;
            //Padding padding = new Padding(2);
            dgvCertificados.DataSource = certificados;
            dgvCertificados.Columns["nifCertificado"].HeaderText = "NIF titular";
            dgvCertificados.Columns["nifCertificado"].Width = 80;
            dgvCertificados.Columns["nifCertificado"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifCertificado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgvCertificados.Columns["nifCertificado"].HeaderCell.Style.Padding = padding;
            dgvCertificados.Columns["titularCertificado"].HeaderText = "Titular del certificado";
            //dgvCertificados.Columns["titularCertificado"].HeaderCell.Style.Padding = padding;
            dgvCertificados.Columns["titularCertificado"].Width = 250;
            dgvCertificados.Columns["fechaEmision"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaEmision"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaEmision"].HeaderText = "Valido desde";
            dgvCertificados.Columns["fechaEmision"].Width = 85;
            dgvCertificados.Columns["fechaValidez"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaValidez"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaValidez"].HeaderText = "Valido hasta";
            dgvCertificados.Columns["fechaValidez"].Width = 85;
            dgvCertificados.Columns["nifRepresentante"].HeaderText = "NIF representante";
            dgvCertificados.Columns["nifRepresentante"].Width = 110;
            dgvCertificados.Columns["nifRepresentante"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifRepresentante"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nombreRepresentante"].HeaderText = "Nombre representante";
            dgvCertificados.Columns["nombreRepresentante"].Width = 250;
            dgvCertificados.Columns["serieCertificado"].HeaderText = "Nº serie certificado";
            dgvCertificados.Columns["serieCertificado"].Width = 250;

        }

        private void dgvCertificados_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        private void frmSeleccion_Load(object sender, EventArgs e)
        {
            txtBusqueda.Focus();
        }

        private void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            //gestionCertificados proceso = gestionCertificados.ObtenerInstancia();
            List<ElementosCertificado> certificados = instanciaCertificado.relacionCertificados();
            if (certificados != null)
            {
                certificados = instanciaCertificado.filtrarCertificados(txtBusqueda.Text);
                rellenarDGV(certificados);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnSeleccion_Click(object sender, EventArgs e)
        {
            int indice = dgvCertificados.SelectedRows[0].Index;
            if (indice >= 0 && indice < dgvCertificados.Rows.Count)
            {
                DataGridViewCell celda = dgvCertificados.Rows[indice].Cells["serieCertificado"];
                if (celda != null)
                {
                    certificadoSeleccionado.serieCertificado = celda.Value.ToString();
                }
            }
            this.Close();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            txtBusqueda.Text = string.Empty;
        }

        private void dgvCertificados_KeyPress(object sender, KeyPressEventArgs e)
        {
            btnSeleccion.PerformClick();
        }

        private void dgvCertificados_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            btnSeleccion.PerformClick();
        }
    }
}
