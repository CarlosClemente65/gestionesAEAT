using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace gestionesAEAT.Formularios
{
    public partial class frmSeleccion : Form
    {
        //Crea un diccionario para saber el orden de cada columna
        private Dictionary<string, EstadoOrdenacion> estadosOrdenacion;

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
            dgvCertificados.DataSource = certificados;
            dgvCertificados.Columns["nifCertificado"].HeaderText = "NIF titular";
            dgvCertificados.Columns["nifCertificado"].Width = 80;
            dgvCertificados.Columns["nifCertificado"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifCertificado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifCertificado"].DisplayIndex = 0;
            dgvCertificados.Columns["titularCertificado"].HeaderText = "Titular del certificado";
            dgvCertificados.Columns["titularCertificado"].Width = 250;
            dgvCertificados.Columns["titularCertificado"].DisplayIndex = 1;
            dgvCertificados.Columns["fechaEmision"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaEmision"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaEmision"].HeaderText = "Valido desde";
            dgvCertificados.Columns["fechaEmision"].Width = 85;
            dgvCertificados.Columns["fechaEmision"].DisplayIndex = 2;
            dgvCertificados.Columns["fechaValidez"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaValidez"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaValidez"].HeaderText = "Valido hasta";
            dgvCertificados.Columns["fechaValidez"].Width = 85;
            dgvCertificados.Columns["fechaValidez"].DisplayIndex = 3;
            dgvCertificados.Columns["nifRepresentante"].HeaderText = "NIF representante";
            dgvCertificados.Columns["nifRepresentante"].Width = 110;
            dgvCertificados.Columns["nifRepresentante"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifRepresentante"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifRepresentante"].DisplayIndex = 4;
            dgvCertificados.Columns["nombreRepresentante"].HeaderText = "Nombre representante";
            dgvCertificados.Columns["nombreRepresentante"].Width = 250;
            dgvCertificados.Columns["nombreRepresentante"].DisplayIndex = 5;
            dgvCertificados.Columns["serieCertificado"].HeaderText = "Nº serie certificado";
            dgvCertificados.Columns["serieCertificado"].Width = 250;
            dgvCertificados.Columns["serieCertificado"].DisplayIndex = 7;
            dgvCertificados.Columns["datosRepresentante"].HeaderText = "Datos representante";
            dgvCertificados.Columns["datosRepresentante"].Width = 300;
            dgvCertificados.Columns["datosRepresentante"].DisplayIndex = 6;
            dgvCertificados.Columns["huellaCertificado"].HeaderText = "Huella certificado";
            dgvCertificados.Columns["huellaCertificado"].Width = 300;
            dgvCertificados.Columns["huellaCertificado"].DisplayIndex = 8;
        }



        private void frmSeleccion_Load(object sender, EventArgs e)
        {
            txtBusqueda.Focus();

            //Crea el diccionacion con las columnas y su ordenacion por defecto a true
            estadosOrdenacion = new Dictionary<string, EstadoOrdenacion>();
            foreach (DataGridViewColumn column in dgvCertificados.Columns)
            {
                estadosOrdenacion[column.Name] = new EstadoOrdenacion { CampoOrden = column.Name, Ascendente = true };
            }
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

        private void dgvCertificados_ColumnHeaderMouseClick_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            //Metodo para ordenar por la cabecera de la columna que se haya pulsado
            string columna = dgvCertificados.Columns[e.ColumnIndex].Name;

            //Lee el estado de ordenacion que tiene la columna
            EstadoOrdenacion estado = estadosOrdenacion[columna];

            //Carga en la variable los certificado ordenados
            var certificados = instanciaCertificado.ordenarCertificados(estado.CampoOrden, estado.Ascendente);

            //Cambia el estado para la siguiente ordenacion hacerlo a la inversa
            estado.Ascendente = !estado.Ascendente;

            //Rellena el formulario con los certificado ya ordenados
            rellenarDGV(certificados);
        }
    }

    public class EstadoOrdenacion
    {
        public string CampoOrden { get; set; }
        public bool Ascendente { get; set; }
    }
}
