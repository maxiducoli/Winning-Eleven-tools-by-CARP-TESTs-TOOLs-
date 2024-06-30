namespace WE_Decompress_2k24_by_CARP
{
    partial class frmDecompress
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
            button1 = new Button();
            txtComprimido = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label5 = new Label();
            label6 = new Label();
            txtOffsetComprimido = new TextBox();
            txtBits = new TextBox();
            label4 = new Label();
            label7 = new Label();
            txtAlto = new TextBox();
            txtAncho = new TextBox();
            txtPaleta = new TextBox();
            txtOffsetPaleta = new TextBox();
            pbImagen = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pbImagen).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(318, 364);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // txtComprimido
            // 
            txtComprimido.Location = new Point(12, 27);
            txtComprimido.Name = "txtComprimido";
            txtComprimido.Size = new Size(381, 23);
            txtComprimido.TabIndex = 1;
            txtComprimido.Text = "T_NAME.BIN";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 3;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 108);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 4;
            label2.Text = "label2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 70);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 5;
            label3.Text = "Offset";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(20, 163);
            label5.Name = "label5";
            label5.Size = new Size(37, 15);
            label5.TabIndex = 7;
            label5.Text = "offset";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(137, 163);
            label6.Name = "label6";
            label6.Size = new Size(29, 15);
            label6.TabIndex = 8;
            label6.Text = "BITS";
            // 
            // txtOffsetComprimido
            // 
            txtOffsetComprimido.Location = new Point(64, 62);
            txtOffsetComprimido.Name = "txtOffsetComprimido";
            txtOffsetComprimido.Size = new Size(67, 23);
            txtOffsetComprimido.TabIndex = 9;
            txtOffsetComprimido.Text = "4";
            // 
            // txtBits
            // 
            txtBits.Location = new Point(181, 155);
            txtBits.Name = "txtBits";
            txtBits.Size = new Size(65, 23);
            txtBits.TabIndex = 12;
            txtBits.Text = "4";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(22, 252);
            label4.Name = "label4";
            label4.Size = new Size(29, 15);
            label4.TabIndex = 14;
            label4.Text = "Alto";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(24, 320);
            label7.Name = "label7";
            label7.Size = new Size(42, 15);
            label7.TabIndex = 15;
            label7.Text = "Ancho";
            // 
            // txtAlto
            // 
            txtAlto.Location = new Point(24, 270);
            txtAlto.Name = "txtAlto";
            txtAlto.Size = new Size(36, 23);
            txtAlto.TabIndex = 16;
            txtAlto.Text = "128";
            // 
            // txtAncho
            // 
            txtAncho.Location = new Point(24, 338);
            txtAncho.Name = "txtAncho";
            txtAncho.Size = new Size(38, 23);
            txtAncho.TabIndex = 17;
            txtAncho.Text = "128";
            // 
            // txtPaleta
            // 
            txtPaleta.Location = new Point(12, 126);
            txtPaleta.Name = "txtPaleta";
            txtPaleta.Size = new Size(381, 23);
            txtPaleta.TabIndex = 18;
            txtPaleta.Text = "DAT2D.BIN";
            // 
            // txtOffsetPaleta
            // 
            txtOffsetPaleta.Location = new Point(64, 155);
            txtOffsetPaleta.Name = "txtOffsetPaleta";
            txtOffsetPaleta.Size = new Size(67, 23);
            txtOffsetPaleta.TabIndex = 19;
            txtOffsetPaleta.Text = "70276";
            // 
            // pbImagen
            // 
            pbImagen.Location = new Point(106, 217);
            pbImagen.Name = "pbImagen";
            pbImagen.Size = new Size(140, 144);
            pbImagen.TabIndex = 20;
            pbImagen.TabStop = false;
            // 
            // frmDecompress
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(416, 399);
            Controls.Add(pbImagen);
            Controls.Add(txtOffsetPaleta);
            Controls.Add(txtPaleta);
            Controls.Add(txtAncho);
            Controls.Add(txtAlto);
            Controls.Add(label7);
            Controls.Add(label4);
            Controls.Add(txtBits);
            Controls.Add(txtOffsetComprimido);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtComprimido);
            Controls.Add(button1);
            Name = "frmDecompress";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pbImagen).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox txtComprimido;
        private TextBox textBox2;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox txtOffsetComprimido;
        private TextBox textBox4;
        private TextBox textBox5;
        private TextBox txtBits;
        private Label label7;
        private TextBox txtAlto;
        private TextBox txtAncho;
        private TextBox txtPaleta;
        private TextBox txtOffsetPaleta;
        private PictureBox pbImagen;
    }
}
