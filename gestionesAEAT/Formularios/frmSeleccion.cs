using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gestionesAEAT.Formularios
{
    public partial class frmSeleccion : Form
    {
        public frmSeleccion()
        {
            InitializeComponent();
            gestionCertificados proceso = gestionCertificados.ObtenerInstancia();
            List<certificadoInfo> certificados = proceso.listaCertificados();
            dgvCertificados.DataSource = certificados;
        }
    }
}
