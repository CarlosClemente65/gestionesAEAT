namespace gestionesAEAT.Formularios
{
    partial class frmSeleccion
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSeleccion));
            this.panelCertificados = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.dgvCertificados = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnBorrar = new System.Windows.Forms.Button();
            this.txtBusqueda = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnSeleccion = new System.Windows.Forms.Button();
            this.panelTitulo = new System.Windows.Forms.Panel();
            this.Titulo = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.panelCertificados.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCertificados)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.panelTitulo.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelCertificados
            // 
            this.panelCertificados.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCertificados.AutoScroll = true;
            this.panelCertificados.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(235)))), ((int)(((byte)(250)))));
            this.panelCertificados.Controls.Add(this.label3);
            this.panelCertificados.Controls.Add(this.dgvCertificados);
            this.panelCertificados.Controls.Add(this.label1);
            this.panelCertificados.Location = new System.Drawing.Point(0, 48);
            this.panelCertificados.Margin = new System.Windows.Forms.Padding(0);
            this.panelCertificados.Name = "panelCertificados";
            this.panelCertificados.Size = new System.Drawing.Size(897, 342);
            this.panelCertificados.TabIndex = 10;
            this.panelCertificados.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelMouseDown);
            this.panelCertificados.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelMouseMove);
            this.panelCertificados.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelMouseUp);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.label3.Location = new System.Drawing.Point(190, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "(doble clic para seleccionar)";
            // 
            // dgvCertificados
            // 
            this.dgvCertificados.AllowUserToAddRows = false;
            this.dgvCertificados.AllowUserToDeleteRows = false;
            this.dgvCertificados.AllowUserToOrderColumns = true;
            this.dgvCertificados.AllowUserToResizeRows = false;
            this.dgvCertificados.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCertificados.BackgroundColor = System.Drawing.Color.White;
            this.dgvCertificados.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvCertificados.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgvCertificados.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.Disable;
            dataGridViewCellStyle27.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle27.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle27.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle27.Padding = new System.Windows.Forms.Padding(4);
            dataGridViewCellStyle27.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(188)))));
            dataGridViewCellStyle27.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle27.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCertificados.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle27;
            this.dgvCertificados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle28.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle28.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle28.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle28.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle28.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(235)))), ((int)(((byte)(250)))));
            dataGridViewCellStyle28.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle28.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCertificados.DefaultCellStyle = dataGridViewCellStyle28;
            this.dgvCertificados.GridColor = System.Drawing.Color.SaddleBrown;
            this.dgvCertificados.Location = new System.Drawing.Point(16, 32);
            this.dgvCertificados.MultiSelect = false;
            this.dgvCertificados.Name = "dgvCertificados";
            this.dgvCertificados.ReadOnly = true;
            this.dgvCertificados.RowHeadersVisible = false;
            this.dgvCertificados.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCertificados.Size = new System.Drawing.Size(878, 309);
            this.dgvCertificados.StandardTab = true;
            this.dgvCertificados.TabIndex = 5;
            this.dgvCertificados.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCertificados_CellContentDoubleClick);
            this.dgvCertificados.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvCertificados_ColumnHeaderMouseClick_1);
            this.dgvCertificados.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dgvCertificados_KeyPress);
            this.dgvCertificados.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelMouseDown);
            this.dgvCertificados.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelMouseMove);
            this.dgvCertificados.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelMouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Relacion de certificados";
            // 
            // panelBotones
            // 
            this.panelBotones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBotones.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(235)))), ((int)(((byte)(250)))));
            this.panelBotones.Controls.Add(this.label4);
            this.panelBotones.Controls.Add(this.btnBorrar);
            this.panelBotones.Controls.Add(this.txtBusqueda);
            this.panelBotones.Controls.Add(this.label2);
            this.panelBotones.Controls.Add(this.btnCancelar);
            this.panelBotones.Controls.Add(this.btnSeleccion);
            this.panelBotones.Location = new System.Drawing.Point(0, 390);
            this.panelBotones.Margin = new System.Windows.Forms.Padding(0);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(5);
            this.panelBotones.Size = new System.Drawing.Size(894, 72);
            this.panelBotones.TabIndex = 1;
            this.panelBotones.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelMouseDown);
            this.panelBotones.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelMouseMove);
            this.panelBotones.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelMouseUp);
            // 
            // btnBorrar
            // 
            this.btnBorrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBorrar.BackColor = System.Drawing.Color.White;
            this.btnBorrar.BackgroundImage = global::gestionesAEAT.Properties.Resources.cancelar;
            this.btnBorrar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBorrar.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnBorrar.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlLight;
            this.btnBorrar.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.Control;
            this.btnBorrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBorrar.Location = new System.Drawing.Point(743, 34);
            this.btnBorrar.Name = "btnBorrar";
            this.btnBorrar.Size = new System.Drawing.Size(22, 22);
            this.btnBorrar.TabIndex = 2;
            this.btnBorrar.UseVisualStyleBackColor = false;
            this.btnBorrar.Click += new System.EventHandler(this.btnBorrar_Click);
            // 
            // txtBusqueda
            // 
            this.txtBusqueda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBusqueda.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBusqueda.Location = new System.Drawing.Point(124, 32);
            this.txtBusqueda.Name = "txtBusqueda";
            this.txtBusqueda.Size = new System.Drawing.Size(645, 26);
            this.txtBusqueda.TabIndex = 1;
            this.txtBusqueda.TextChanged += new System.EventHandler(this.txtBusqueda_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(124, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Buscar certificado";
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.Black;
            this.btnCancelar.Location = new System.Drawing.Point(777, 12);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(5);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(100, 48);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "Cancelar seleccion";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnSeleccion
            // 
            this.btnSeleccion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSeleccion.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSeleccion.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnSeleccion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnSeleccion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccion.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccion.ForeColor = System.Drawing.Color.Black;
            this.btnSeleccion.Location = new System.Drawing.Point(16, 10);
            this.btnSeleccion.Margin = new System.Windows.Forms.Padding(5);
            this.btnSeleccion.Name = "btnSeleccion";
            this.btnSeleccion.Size = new System.Drawing.Size(100, 48);
            this.btnSeleccion.TabIndex = 3;
            this.btnSeleccion.Text = "Seleccionar certificado";
            this.btnSeleccion.UseVisualStyleBackColor = false;
            this.btnSeleccion.Click += new System.EventHandler(this.btnSeleccion_Click);
            // 
            // panelTitulo
            // 
            this.panelTitulo.BackColor = System.Drawing.Color.DodgerBlue;
            this.panelTitulo.Controls.Add(this.button2);
            this.panelTitulo.Controls.Add(this.button4);
            this.panelTitulo.Controls.Add(this.Titulo);
            this.panelTitulo.Location = new System.Drawing.Point(0, 0);
            this.panelTitulo.Margin = new System.Windows.Forms.Padding(5);
            this.panelTitulo.Name = "panelTitulo";
            this.panelTitulo.Padding = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.panelTitulo.Size = new System.Drawing.Size(894, 48);
            this.panelTitulo.TabIndex = 15;
            this.panelTitulo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelMouseDown);
            this.panelTitulo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelMouseMove);
            this.panelTitulo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelMouseUp);
            // 
            // Titulo
            // 
            this.Titulo.AutoSize = true;
            this.Titulo.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Titulo.ForeColor = System.Drawing.Color.White;
            this.Titulo.Location = new System.Drawing.Point(12, 14);
            this.Titulo.Name = "Titulo";
            this.Titulo.Size = new System.Drawing.Size(189, 18);
            this.Titulo.TabIndex = 9;
            this.Titulo.Text = "Seleccionar certificado";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.BackColor = System.Drawing.Color.DodgerBlue;
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.PowderBlue;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ImageIndex = 2;
            this.button2.ImageList = this.imageList1;
            this.button2.Location = new System.Drawing.Point(847, 8);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(30, 30);
            this.button2.TabIndex = 14;
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.BackColor = System.Drawing.Color.DodgerBlue;
            this.button4.FlatAppearance.BorderSize = 0;
            this.button4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.PowderBlue;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.ImageIndex = 3;
            this.button4.ImageList = this.imageList1;
            this.button4.Location = new System.Drawing.Point(811, 8);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(30, 30);
            this.button4.TabIndex = 15;
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.btnMinimizar_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "buscar.png");
            this.imageList1.Images.SetKeyName(1, "cargar.png");
            this.imageList1.Images.SetKeyName(2, "cerrar.png");
            this.imageList1.Images.SetKeyName(3, "minimizar.png");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)), true);
            this.label4.Location = new System.Drawing.Point(239, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(234, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "(nombre o NIF del titular o representante)";
            // 
            // frmSeleccion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.PowderBlue;
            this.ClientSize = new System.Drawing.Size(894, 462);
            this.Controls.Add(this.panelTitulo);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.panelCertificados);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmSeleccion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Seleccion certificado";
            this.Load += new System.EventHandler(this.frmSeleccion_Load);
            this.panelCertificados.ResumeLayout(false);
            this.panelCertificados.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCertificados)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.panelBotones.PerformLayout();
            this.panelTitulo.ResumeLayout(false);
            this.panelTitulo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelCertificados;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.DataGridView dgvCertificados;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSeleccion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.TextBox txtBusqueda;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBorrar;
        private System.Windows.Forms.Panel panelTitulo;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label Titulo;
        private System.Windows.Forms.Label label4;
    }
}