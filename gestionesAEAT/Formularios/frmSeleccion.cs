﻿using System;
using System.CodeDom;
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
        private int columnaOrdenada = -1;
        private ListSortDirection ordenColumna = ListSortDirection.Ascending;

        public frmSeleccion()
        {
            InitializeComponent();
            dgvCertificados.CellFormatting += dgvCertificados_CellFormatting;
            gestionCertificados proceso = gestionCertificados.ObtenerInstancia();
            List<certificadoInfo> certificados = proceso.listaCertificados();
            rellenarDGV(certificados);
            this.Load += frmSeleccion_Load;

        }

        private void rellenarDGV(List<certificadoInfo> certificados)
        {
            //Metodo para rellenar el listado de certificados con sus columnas (necesario para regrabar en el caso de ordenacion)
            //dgvCertificados.DataSource = null;
            dgvCertificados.DataSource = certificados;
            dgvCertificados.Columns["nifCertificado"].HeaderText = "NIF titular";
            dgvCertificados.Columns["nifCertificado"].Width = 90;
            dgvCertificados.Columns["nifCertificado"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["titularCertificado"].HeaderText = "Titular del certificado";
            dgvCertificados.Columns["titularCertificado"].Width = 250;
            dgvCertificados.Columns["fechaCertificado"].HeaderText = "Fecha validez";
            dgvCertificados.Columns["fechaCertificado"].Width = 120;
            dgvCertificados.Columns["fechaCertificado"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nifRepresentante"].HeaderText = "NIF representante";
            dgvCertificados.Columns["nifRepresentante"].Width = 100;
            dgvCertificados.Columns["nifRepresentante"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCertificados.Columns["nombreRepresentante"].HeaderText = "Nombre representante";
            dgvCertificados.Columns["nombreRepresentante"].Width = 250;
            dgvCertificados.Columns["serieCertificado"].HeaderText = "Nº serie certificado";
            dgvCertificados.Columns["serieCertificado"].Width = 250;

        }

        private void dgvCertificados_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn columna = dgvCertificados.Columns[e.ColumnIndex];
            string nombreColumna = columna.DataPropertyName;

            // Obtener los datos del DataGridView
            List<certificadoInfo> certificados = dgvCertificados.DataSource as List<certificadoInfo>;
            if (certificados != null)
            {
                // Determinar la dirección de la ordenación
                ListSortDirection direccionOrdenacion = ListSortDirection.Ascending;
                if (columnaOrdenada == e.ColumnIndex)
                {
                    if (ordenColumna == ListSortDirection.Ascending)
                    {
                        direccionOrdenacion = ListSortDirection.Descending;
                    }
                }

                // Ordenar la lista
                gestionCertificados proceso = gestionCertificados.ObtenerInstancia();
                certificados = proceso.ordenarCertificados(certificados, nombreColumna, direccionOrdenacion == ListSortDirection.Ascending);

                // Actualizar el DataGridView con la lista ordenada
                rellenarDGV(certificados);

                columnaOrdenada = e.ColumnIndex;
                ordenColumna = direccionOrdenacion;
            }
        }

        private void frmSeleccion_Load(object sender, EventArgs e)
        {
            txtBusqueda.Focus();
        }

        private void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            gestionCertificados proceso = gestionCertificados.ObtenerInstancia();
            List<certificadoInfo> certificados = proceso.listaCertificados();
            if (certificados != null)
            {
                certificados = proceso.filtrarCertificados(txtBusqueda.Text);
                rellenarDGV(certificados);
            }
        }

        private void dgvCertificados_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //if (e.RowIndex >= -1 && e.ColumnIndex >= 0)
            //{
            //    //dgvCertificados.Columns[e.ColumnIndex].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    //if (dgvCertificados.Columns[e.ColumnIndex].Name == "nifCertificado")
            //    if (e.ColumnIndex == 0)
            //    {
            //        e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    }
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
