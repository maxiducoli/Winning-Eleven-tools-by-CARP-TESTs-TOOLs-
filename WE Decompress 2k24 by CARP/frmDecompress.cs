using decompress;
namespace WE_Decompress_2k24_by_CARP
{
    public partial class frmDecompress : Form
    {
        public frmDecompress()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WeDecompress d = new WeDecompress();
            string rutaBIN, rutaTIM, rutaBMP, rutaNueva;;
                
            int offsetBIN, offsetPaleta, alto, ancho, bits = 0;
            try
            {
                rutaBMP = "T_NAME.BMP";
                rutaBIN = txtComprimido.Text;
                rutaTIM = txtPaleta.Text;
                offsetBIN = Convert.ToInt32(txtOffsetComprimido.Text);
                offsetPaleta = Convert.ToInt32(txtOffsetPaleta.Text);
                alto = Convert.ToInt32(txtAlto.Text);
                ancho = Convert.ToInt32(txtAncho.Text);
                bits = Convert.ToInt32(txtBits.Text);
                pbImagen.Height = alto;
                pbImagen.Width = ancho;
                d.CrearBMP(rutaBMP, rutaBIN, offsetBIN, rutaTIM, offsetPaleta, alto, ancho, bits, out rutaNueva);
                if (File.Exists(rutaBMP))
                {
                Image img = Image.FromFile(rutaBMP);
                    pbImagen.Image = img;
                    if (img != null) {img.Dispose();}
                }
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }
        }
    }
}
