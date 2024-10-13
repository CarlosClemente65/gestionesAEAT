using GestionCertificadosDigitales;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using gestionesAEAT.Utilidades;
using System.Drawing;

namespace gestionesAEAT.Formularios
{
    public partial class frmSeleccion : Form
    {
        Utiles utilidad = new Utiles();

        //Crea un diccionario para saber el orden de cada columna
        private Dictionary<string, EstadoOrdenacion> estadosOrdenacion;

        //private GestionCertificados instanciaCertificado;
        public PropiedadesCertificados certificadoSeleccionado { get; set; }
        private List<PropiedadesCertificados> certificados;

        //Variables necesarias para mover el formulario
        private bool mouseDown;
        private Point startPoint;

        public frmSeleccion()
        {
            InitializeComponent();

            //Carga la relacion de certificados en el lineal del formulario
            certificados = Program.gestionCertificados.relacionCertificados();
            rellenarDGV();
            if (!string.IsNullOrEmpty(Program.tituloVentana))
            {
                Text = Program.tituloVentana;
            }
            Load += frmSeleccion_Load;
        }

        private void rellenarDGV()
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
            dgvCertificados.Columns["fechaEmision"].Width = 80;
            dgvCertificados.Columns["fechaEmision"].DisplayIndex = 2;
            dgvCertificados.Columns["fechaValidez"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaValidez"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["fechaValidez"].HeaderText = "Valido hasta";
            dgvCertificados.Columns["fechaValidez"].Width = 80;
            dgvCertificados.Columns["fechaValidez"].DisplayIndex = 3;
            dgvCertificados.Columns["nifRepresentante"].HeaderText = "NIF representante";
            dgvCertificados.Columns["nifRepresentante"].Width = 110;
            dgvCertificados.Columns["nifRepresentante"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifRepresentante"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifRepresentante"].DisplayIndex = 4;
            dgvCertificados.Columns["nombreRepresentante"].HeaderText = "Nombre representante";
            dgvCertificados.Columns["nombreRepresentante"].Width = 250;
            dgvCertificados.Columns["nombreRepresentante"].DisplayIndex = 5;
            dgvCertificados.Columns["nombreCertificado"].HeaderText = "Nombre certificado";
            dgvCertificados.Columns["nombreCertificado"].Width = 300;
            dgvCertificados.Columns["nombreCertificado"].DisplayIndex = 6;
            dgvCertificados.Columns["serieCertificado"].HeaderText = "Nº serie certificado";
            dgvCertificados.Columns["serieCertificado"].Width = 250;
            dgvCertificados.Columns["serieCertificado"].DisplayIndex = 7;
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
            if (certificados != null)
            {
                certificados = Program.gestionCertificados.filtrarCertificados(txtBusqueda.Text);
                rellenarDGV();
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            utilidad.SalirAplicacion("El usuario a cancelado la operacion");
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
                    Parametros.serieCertificado = celda.Value.ToString();
                }
            }
            this.Close();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            txtBusqueda.Text = string.Empty;
            txtBusqueda.Focus();
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

            //Se convierte el nombre de la columna al enum 'nombrePropiedades'
            if (Enum.TryParse(columna, out GestionarCertificados.nombresPropiedades CampoOrden))
            {
                //Carga en la variable los certificado ordenados
                certificados = Program.gestionCertificados.ordenarCertificados(CampoOrden, estado.Ascendente);
            }

            //Cambia el estado para la siguiente ordenacion hacerlo a la inversa
            estado.Ascendente = !estado.Ascendente;

            //Rellena el formulario con los certificado ya ordenados
            rellenarDGV();
        }

        private void panelMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                startPoint = new Point(e.X, e.Y);
                // Desactiva la captura de eventos del control
                Capture = false;
            }
        }

        private void panelMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Location = new Point(
                Location.X - startPoint.X + e.X,
                Location.Y - startPoint.Y + e.Y);

                Update();
            }
        }

        private void panelMouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            string mensaje = $"Proceso cancelado por el usuario.";
            utilidad.GrabarSalida(mensaje, Program.ficheroResultado);
            Environment.Exit(0);
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }

    public class EstadoOrdenacion
    {
        public string CampoOrden { get; set; }
        public bool Ascendente { get; set; }
    }
}
