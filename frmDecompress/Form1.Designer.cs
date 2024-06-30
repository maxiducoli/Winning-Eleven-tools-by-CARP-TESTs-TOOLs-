namespace frmDecompress
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pbImagen = new PictureBox();
            txtPaleta = new TextBox();
            label2 = new Label();
            label1 = new Label();
            txtGrafico = new TextBox();
            dgvGrafico = new DataGridView();
            colID = new DataGridViewTextBoxColumn();
            colVRAMx = new DataGridViewTextBoxColumn();
            colVRAMy = new DataGridViewTextBoxColumn();
            colAlto = new DataGridViewTextBoxColumn();
            colLargo = new DataGridViewTextBoxColumn();
            coloffset = new DataGridViewTextBoxColumn();
            colSize = new DataGridViewTextBoxColumn();
            dgvPaleta = new DataGridView();
            colIdPaleta = new DataGridViewTextBoxColumn();
            colVRAMxPaleta = new DataGridViewTextBoxColumn();
            colVRAMyPaleta = new DataGridViewTextBoxColumn();
            colLargoPaleta = new DataGridViewTextBoxColumn();
            colBytes = new DataGridViewTextBoxColumn();
            colAltoPaleta = new DataGridViewTextBoxColumn();
            colOffsetPaleta = new DataGridViewTextBoxColumn();
            btnCargarGrafico = new Button();
            btnPaleta = new Button();
            groupBox1 = new GroupBox();
            textBox1 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            lblLength = new Label();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            ((System.ComponentModel.ISupportInitialize)pbImagen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvGrafico).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPaleta).BeginInit();
            SuspendLayout();
            // 
            // pbImagen
            // 
            pbImagen.Location = new Point(667, 27);
            pbImagen.Name = "pbImagen";
            pbImagen.Size = new Size(128, 128);
            pbImagen.TabIndex = 36;
            pbImagen.TabStop = false;
            // 
            // txtPaleta
            // 
            txtPaleta.Location = new Point(12, 231);
            txtPaleta.Name = "txtPaleta";
            txtPaleta.Size = new Size(363, 23);
            txtPaleta.TabIndex = 34;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 213);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 24;
            label2.Text = "Paleta";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(45, 15);
            label1.TabIndex = 23;
            label1.Text = "Gráfico";
            // 
            // txtGrafico
            // 
            txtGrafico.Location = new Point(12, 27);
            txtGrafico.Name = "txtGrafico";
            txtGrafico.Size = new Size(363, 23);
            txtGrafico.TabIndex = 22;
            // 
            // dgvGrafico
            // 
            dgvGrafico.AllowUserToAddRows = false;
            dgvGrafico.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGrafico.Columns.AddRange(new DataGridViewColumn[] { colID, colVRAMx, colVRAMy, colAlto, colLargo, coloffset, colSize });
            dgvGrafico.Location = new Point(12, 56);
            dgvGrafico.Name = "dgvGrafico";
            dgvGrafico.RowHeadersVisible = false;
            dgvGrafico.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGrafico.Size = new Size(454, 150);
            dgvGrafico.TabIndex = 41;
            dgvGrafico.CellClick += dgvGrafico_CellClick;
            // 
            // colID
            // 
            colID.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colID.HeaderText = "ID";
            colID.Name = "colID";
            colID.Width = 43;
            // 
            // colVRAMx
            // 
            colVRAMx.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colVRAMx.HeaderText = "VRAM x";
            colVRAMx.Name = "colVRAMx";
            colVRAMx.Width = 74;
            // 
            // colVRAMy
            // 
            colVRAMy.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colVRAMy.HeaderText = "VRAM y";
            colVRAMy.Name = "colVRAMy";
            colVRAMy.Width = 74;
            // 
            // colAlto
            // 
            colAlto.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colAlto.HeaderText = "Alto";
            colAlto.Name = "colAlto";
            colAlto.Width = 54;
            // 
            // colLargo
            // 
            colLargo.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colLargo.HeaderText = "Largo";
            colLargo.Name = "colLargo";
            colLargo.Width = 62;
            // 
            // coloffset
            // 
            coloffset.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            coloffset.HeaderText = "Offset";
            coloffset.Name = "coloffset";
            coloffset.Width = 64;
            // 
            // colSize
            // 
            colSize.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSize.HeaderText = "Tamaño";
            colSize.Name = "colSize";
            // 
            // dgvPaleta
            // 
            dgvPaleta.AllowUserToAddRows = false;
            dgvPaleta.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPaleta.Columns.AddRange(new DataGridViewColumn[] { colIdPaleta, colVRAMxPaleta, colVRAMyPaleta, colLargoPaleta, colBytes, colAltoPaleta, colOffsetPaleta });
            dgvPaleta.Location = new Point(12, 260);
            dgvPaleta.Name = "dgvPaleta";
            dgvPaleta.RowHeadersVisible = false;
            dgvPaleta.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPaleta.Size = new Size(454, 172);
            dgvPaleta.TabIndex = 42;
            dgvPaleta.CellClick += dgvPaleta_CellClick;
            // 
            // colIdPaleta
            // 
            colIdPaleta.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colIdPaleta.HeaderText = "ID";
            colIdPaleta.Name = "colIdPaleta";
            colIdPaleta.Width = 43;
            // 
            // colVRAMxPaleta
            // 
            colVRAMxPaleta.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colVRAMxPaleta.HeaderText = "VRAM x";
            colVRAMxPaleta.Name = "colVRAMxPaleta";
            colVRAMxPaleta.Width = 74;
            // 
            // colVRAMyPaleta
            // 
            colVRAMyPaleta.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colVRAMyPaleta.HeaderText = "VRAM y";
            colVRAMyPaleta.Name = "colVRAMyPaleta";
            colVRAMyPaleta.Width = 74;
            // 
            // colLargoPaleta
            // 
            colLargoPaleta.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colLargoPaleta.HeaderText = "Largo";
            colLargoPaleta.Name = "colLargoPaleta";
            colLargoPaleta.Width = 62;
            // 
            // colBytes
            // 
            colBytes.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colBytes.HeaderText = "Bytes";
            colBytes.Name = "colBytes";
            colBytes.Width = 60;
            // 
            // colAltoPaleta
            // 
            colAltoPaleta.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            colAltoPaleta.HeaderText = "Colores";
            colAltoPaleta.Name = "colAltoPaleta";
            colAltoPaleta.Width = 72;
            // 
            // colOffsetPaleta
            // 
            colOffsetPaleta.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colOffsetPaleta.HeaderText = "Offset";
            colOffsetPaleta.Name = "colOffsetPaleta";
            // 
            // btnCargarGrafico
            // 
            btnCargarGrafico.Location = new Point(381, 27);
            btnCargarGrafico.Name = "btnCargarGrafico";
            btnCargarGrafico.Size = new Size(85, 23);
            btnCargarGrafico.TabIndex = 43;
            btnCargarGrafico.Text = "Abrir &Grafico";
            btnCargarGrafico.UseVisualStyleBackColor = true;
            btnCargarGrafico.Click += btnCargarGrafico_Click;
            // 
            // btnPaleta
            // 
            btnPaleta.Location = new Point(381, 230);
            btnPaleta.Name = "btnPaleta";
            btnPaleta.Size = new Size(85, 23);
            btnPaleta.TabIndex = 44;
            btnPaleta.Text = "Abrir &Paleta";
            btnPaleta.UseVisualStyleBackColor = true;
            btnPaleta.Click += btnPaleta_Click;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = SystemColors.ButtonFace;
            groupBox1.FlatStyle = FlatStyle.Flat;
            groupBox1.Location = new Point(472, 303);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(498, 129);
            groupBox1.TabIndex = 45;
            groupBox1.TabStop = false;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(472, 231);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(498, 23);
            textBox1.TabIndex = 46;
            // 
            // button1
            // 
            button1.Location = new Point(896, 161);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 47;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // button2
            // 
            button2.Location = new Point(472, 132);
            button2.Name = "button2";
            button2.Size = new Size(188, 23);
            button2.TabIndex = 48;
            button2.Text = "FInd FieCompressLength";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // lblLength
            // 
            lblLength.AutoSize = true;
            lblLength.Location = new Point(667, 165);
            lblLength.Name = "lblLength";
            lblLength.Size = new Size(47, 15);
            lblLength.TabIndex = 49;
            lblLength.Text = "Length:";
            // 
            // button3
            // 
            button3.Location = new Point(740, 161);
            button3.Name = "button3";
            button3.Size = new Size(104, 23);
            button3.TabIndex = 50;
            button3.Text = "Comprimir TIM";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(472, 161);
            button4.Name = "button4";
            button4.Size = new Size(188, 23);
            button4.TabIndex = 51;
            button4.Text = "Decompress DLL";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(472, 190);
            button5.Name = "button5";
            button5.Size = new Size(188, 23);
            button5.TabIndex = 52;
            button5.Text = "Compress DLL";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(982, 441);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(lblLength);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(groupBox1);
            Controls.Add(btnPaleta);
            Controls.Add(btnCargarGrafico);
            Controls.Add(dgvPaleta);
            Controls.Add(dgvGrafico);
            Controls.Add(pbImagen);
            Controls.Add(txtPaleta);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtGrafico);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pbImagen).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvGrafico).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPaleta).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pbImagen;
        private TextBox txtPaleta;
        private Label label2;
        private Label label1;
        private TextBox txtGrafico;
        private DataGridView dgvGrafico;
        private DataGridView dgvPaleta;
        private DataGridViewTextBoxColumn colID;
        private DataGridViewTextBoxColumn colVRAMx;
        private DataGridViewTextBoxColumn colVRAMy;
        private DataGridViewTextBoxColumn colAlto;
        private DataGridViewTextBoxColumn colLargo;
        private DataGridViewTextBoxColumn coloffset;
        private DataGridViewTextBoxColumn colSize;
        private DataGridViewTextBoxColumn colIdPaleta;
        private DataGridViewTextBoxColumn colVRAMxPaleta;
        private DataGridViewTextBoxColumn colVRAMyPaleta;
        private DataGridViewTextBoxColumn colLargoPaleta;
        private DataGridViewTextBoxColumn colBytes;
        private DataGridViewTextBoxColumn colAltoPaleta;
        private DataGridViewTextBoxColumn colOffsetPaleta;
        private Button btnCargarGrafico;
        private Button btnPaleta;
        private GroupBox groupBox1;
        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private Label lblLength;
        private Button button3;
        private Button button4;
        private Button button5;
    }
}
