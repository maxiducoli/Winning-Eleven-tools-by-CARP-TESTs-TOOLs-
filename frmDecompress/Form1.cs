using decompress;
using Microsoft.VisualBasic;
using System.Collections;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TIMTools;

namespace frmDecompress
{

    public partial class Form1 : Form
    {
        public const byte BYTE = 0xFF;
        Label[]? cluts = null;
        int rowGrafico = -1;
        int rowPaleta = -1;
        int clutsIndex = -1;
        bool clickGrafico = false;
        bool clickPaleta = false;
        private const string DLL_PATH = "compressUtils.dll";//"compressUtils.dll";
        [DllImport(DLL_PATH,CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DeCompress(ref IntPtr BufDest, IntPtr BufSrc);
        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Compress(ref IntPtr BufDest, IntPtr BufSrc, ref uint SizeResult, uint SizeSrc);
        [DllImport(DLL_PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern long FindCompressedLength(IntPtr BufDest);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                //offsetBIN = Convert.ToInt32(txtOffsetComprimido.Text);

                //offsetPaleta = Convert.ToInt32(txtOffsetPaleta.Text);
                //alto = Convert.ToInt32(txtAlto.Text);
                //ancho = Convert.ToInt32(txtAncho.Text);
                //bits = Convert.ToInt32(txtBits.Text);

                //bytes = weDecompress.DescomprimirArchivoTIM(rutaBIN, offsetBIN);
                //bytePelette = tools.Palette(rutaPaleta, offsetPaleta, bits);

                //paleta = TIMtoBMP.ConvertTIMBytesToColors(bytePelette);

                //TIMtoBMP.CrearArchivoTIM("T_NAME_MIO.TIM", rutaBIN, offsetBIN, rutaPaleta, offsetPaleta, alto, ancho, 4);

                //Label[] colores = new Label[paleta.Length];

                //for (int i = 0; i < paleta.Length; i++)
                //{
                //    colores[i] = new Label();
                //    colores[i].BorderStyle = BorderStyle.FixedSingle;
                //    colores[i].BackColor = paleta[i];
                //    colores[i].AutoSize = false;
                //    colores[i].Size = new Size(25, 25);
                //    colores[i].Location = new Point(locationX * i + 5, 400);
                //    colores[i].Tag = paleta[i].ToString();
                //    this.Controls.Add(colores[i]);
                //}

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private void MostrarPaleta(string rutaPaleta, int offsetPaleta, int largoPaleta)
        {
            BytesToTIM bytesToTIM = new BytesToTIM();
            byte[] paleta;
            int indice = 0;
            Color[] colors = null;
            TIMtoBMP tIMtoBMP = new TIMtoBMP();
            try
            {
                // Largo de la paleta. Dato que viene desde la grilla
                int tipoPaleta = largoPaleta == 16 ? 4 : 8;

                // Array de bytes con los datos de la paleta TIM
                paleta = bytesToTIM.Palette(rutaPaleta, offsetPaleta, tipoPaleta);

                // Seteo el largo de los colores, según el tipo de imagen
                switch (tipoPaleta)
                {
                    case 4:
                        cluts = new Label[16];
                        break;
                    case 8:
                        cluts = new Label[256];
                        break;
                }

                // Convierto el array de bytes en TIM a array de colores
                colors = tIMtoBMP.ConvertTIMBytesToColors(paleta);
                CrearPaletaColores(colors);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        private void EscribirTIM(string pathTIM, int largo, int alto, int palX, int palY, int VRAMx, int VRAMy, byte[] datos, byte[] paleta)
        {
            BytesToTIM bytesToTIM = new BytesToTIM();
            try
            {
                bytesToTIM.CreateTIM(pathTIM, largo, alto, palX, palY, VRAMx, VRAMy, datos, paleta);
            }
            catch (Exception ex)
            {

                throw new IOException(ex.Message);
            }
        }

        private void CargarGraficos()
        {
            WeDecompress weDecompress = new WeDecompress();
            List<byte[]> bytes = new List<byte[]>();
            int indiceGrafico = 0;
            byte[] datos = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivo BIN|*.BIN";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtGrafico.Text = ofd.FileName;
                }
                if (File.Exists(ofd.FileName))
                {
                    dgvGrafico.Rows.Clear();
                    bytes = weDecompress.ListadoDeDatos(ofd.FileName);
                    int alto = 0, largo = 0;
                    foreach (var item in bytes)
                    {
                        if ((item[0] == 10 && item[1] == 0) || (item[0] == 0))
                        {
                            indiceGrafico = dgvGrafico.Rows.Add();
                            dgvGrafico.Rows[indiceGrafico].Cells["colID"].Value = indiceGrafico + 1;
                            byte[] b = new byte[2];
                            b[0] = item[2];
                            b[1] = item[3];
                            Int16 vRam = BitConverter.ToInt16(b, 0);
                            dgvGrafico.Rows[indiceGrafico].Cells["colVRAMx"].Value = vRam;
                            b[0] = item[4];
                            b[1] = item[5];
                            vRam = BitConverter.ToInt16(b, 0);
                            dgvGrafico.Rows[indiceGrafico].Cells["colVRAMy"].Value = vRam;
                            b[0] = item[8];
                            b[1] = item[9];
                            vRam = BitConverter.ToInt16(b, 0);
                            alto = vRam;
                            dgvGrafico.Rows[indiceGrafico].Cells["colAlto"].Value = vRam;
                            b[0] = item[6];
                            b[1] = item[7];
                            vRam = BitConverter.ToInt16(b, 0);
                            largo = vRam;
                            dgvGrafico.Rows[indiceGrafico].Cells["colLargo"].Value = vRam * 2;
                            byte[] offset = new byte[4];
                            offset[0] = item[12];
                            offset[1] = item[13];
                            offset[2] = item[14];
                            offset[3] = item[15];
                            int offsetComprimido = weDecompress.Puntero(offset);
                            dgvGrafico.Rows[indiceGrafico].Cells["coloffset"].Value = offsetComprimido;

                        }
                    }
                    BemaniLZ bemaniLZ = new BemaniLZ();
                    foreach (DataGridViewRow row in dgvGrafico.Rows)
                    {
                        int offset = Convert.ToInt32(row.Cells["coloffset"].Value);
                        int i = row.Index;
                        using (FileStream fs = new FileStream(txtGrafico.Text, FileMode.Open, FileAccess.Read))
                        {
                            datos = new byte[fs.Length - offset];
                            fs.Position = offset;
                            fs.Read(datos, 0, datos.Length);
                            long l = bemaniLZ.FindCompressedLength_1(datos);
                            dgvGrafico.Rows[i].Cells["colSize"].Value = l;
                        }
                    }
                }
            }
        }

        private void CargarPaleta()
        {
            WeDecompress weDecompress = new WeDecompress();
            List<byte[]> bytes = new List<byte[]>();
            int indicePaleta = 0;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Archivo BIN|*.BIN";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPaleta.Text = ofd.FileName;
                }
                if (File.Exists(ofd.FileName))
                {
                    dgvPaleta.Rows.Clear();
                    bytes = weDecompress.ListadoDeDatos(ofd.FileName);
                    foreach (var item in bytes)
                    {
                        if (item[0] == 09)
                        {
                            indicePaleta = dgvPaleta.Rows.Add();

                            dgvPaleta.Rows[indicePaleta].Cells["colIdPaleta"].Value = indicePaleta + 1;

                            byte[] b = new byte[2];
                            b[0] = item[2];
                            b[1] = item[3];
                            Int16 vRam = BitConverter.ToInt16(b, 0);
                            dgvPaleta.Rows[indicePaleta].Cells["colVRAMxPaleta"].Value = vRam;

                            b[0] = item[4];
                            b[1] = item[5];
                            vRam = BitConverter.ToInt16(b, 0);
                            dgvPaleta.Rows[indicePaleta].Cells["colVRAMyPaleta"].Value = vRam;

                            b[0] = item[6];
                            b[1] = item[7];
                            vRam = BitConverter.ToInt16(b, 0);
                            dgvPaleta.Rows[indicePaleta].Cells["colAltoPaleta"].Value = vRam;
                            dgvPaleta.Rows[indicePaleta].Cells["colBytes"].Value = vRam * 2;

                            b[0] = item[8];
                            b[1] = item[9];
                            vRam = BitConverter.ToInt16(b, 0);
                            dgvPaleta.Rows[indicePaleta].Cells["colLargoPaleta"].Value = vRam;

                            byte[] offset = new byte[4];
                            offset[0] = item[12];
                            offset[1] = item[13];
                            offset[2] = item[14];
                            offset[3] = item[15];
                            int offsetComprimido = weDecompress.Puntero(offset);
                            dgvPaleta.Rows[indicePaleta].Cells["colOffsetPaleta"].Value = Convert.ToString(offsetComprimido);

                        }
                    }
                }
            }

        }

        // Carga el BIN con los gráficos
        private void btnCargarGrafico_Click(object sender, EventArgs e)
        {
            CargarGraficos();
        }

        // Carga el BIN con la paleta
        private void btnPaleta_Click(object sender, EventArgs e)
        {
            CargarPaleta();
        }

        // Crea la paleta de coloes
        private void CrearPaletaColores(Color[] colores)
        {
            int cantidadColores = 0;
            int nuevaLinea = 0;
            int labelTop = 0;
            int labelLeft = 0;
            Point point = dgvPaleta.Location;
            cluts = null;
            try
            {

                int controlsCoount = groupBox1.Controls.Count;

                if (controlsCoount > 0)
                {

                    for (int i = controlsCoount - 1; i >= 0; i--)
                    {
                        groupBox1.Controls.Remove(groupBox1.Controls[i]);
                    }
                }
                cluts = new Label[colores.Length];
                foreach (Color color in colores)
                {
                    cluts[cantidadColores] = new Label();
                    cluts[cantidadColores].Name = "LABEL" + cantidadColores.ToString();
                    cluts[cantidadColores].Text = "";
                    cluts[cantidadColores].Size = new Size(15, 15);
                    cluts[cantidadColores].BorderStyle = BorderStyle.FixedSingle;
                    cluts[cantidadColores].Top = (labelTop * cluts[cantidadColores].Height) + 3;
                    cluts[cantidadColores].Left = (labelLeft * cluts[cantidadColores].Width) + 3;
                    cluts[cantidadColores].BackColor = color;
                    cluts[cantidadColores].Tag = cantidadColores.ToString();
                    cluts[cantidadColores].Click += new EventHandler(LabelClick);
                    groupBox1.Controls.Add(cluts[cantidadColores]);
                    cantidadColores++;
                    labelLeft++;
                    if (labelLeft == 32)
                    {
                        labelLeft = 0;
                        labelTop++;
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        // Click para recuperar datos de gráficos
        private void dgvGrafico_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            BytesToTIM bytesToTIM = new BytesToTIM();
            WeDecompress weDecompress = new WeDecompress();
            BmpHelper bmpHelper = new BmpHelper();
            TIMtoBMP tIMtoBMP = new TIMtoBMP();
            int offsetGrafico = 0;
            int offsetPaleta = 0;
            int offsetPaletaRaw = 0;
            int alto, ancho, palx, paly, vramX, vramY, largoPaleta;
            byte[] raw = new byte[300000];
            byte[] paleta = null;
            byte[] bufferTIM = null;
            string timPath = string.Empty;
            int tam = 0;
            try
            {
                clickGrafico = true;
                rowGrafico = e.RowIndex;
                // No hacemos nada hasta no tener seleccionadas las dos grillas
                if (!clickGrafico || !clickPaleta)
                    return;
                // Cargamos variables
                offsetGrafico = Convert.ToInt32(dgvGrafico.Rows[rowGrafico].Cells["coloffset"].Value);

                if (rowGrafico + 1 <= dgvGrafico.RowCount - 1)
                    offsetPaletaRaw = Convert.ToInt32(dgvGrafico.Rows[rowGrafico + 1].Cells["colSize"].Value);

                offsetPaleta = Convert.ToInt32(dgvPaleta.Rows[rowPaleta].Cells["colOffsetPaleta"].Value);

                largoPaleta = Convert.ToInt32(dgvPaleta.Rows[rowPaleta].Cells["colBytes"].Value);
                // Ancho de la imagen
                ancho = Convert.ToInt32(dgvGrafico.Rows[rowGrafico].Cells["colLargo"].Value);
                // Alto de la imagen
                alto = Convert.ToInt32(dgvGrafico.Rows[rowGrafico].Cells["colAlto"].Value);

                // VRAM de la paleta
                palx = Convert.ToInt32(dgvPaleta.Rows[rowPaleta].Cells["colVRAMxPaleta"].Value);
                paly = Convert.ToInt32(dgvPaleta.Rows[rowPaleta].Cells["colVRAMyPaleta"].Value);

                // VRAM de la imagen
                vramX = Convert.ToInt32(dgvGrafico.Rows[rowGrafico].Cells["colVRAMx"].Value);
                vramY = Convert.ToInt32(dgvGrafico.Rows[rowGrafico].Cells["colVRAMy"].Value);

                tam = Convert.ToInt32(dgvGrafico.Rows[rowGrafico].Cells["colSize"].Value);
                // Descomprimimos el RAW
                bool esCero = false;
                if (tam > 0)
                {
                    byte[] buffer;
                    using (FileStream fs = new FileStream(txtGrafico.Text, FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = offsetGrafico;
                        buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, (int)fs.Length);
                        fs.Close();
                    }
                    // weDecompress.DeCompress(ref raw, buffer);
                    List<byte> ar = new List<byte>();
                    BemaniLZ bemaniLZ = new BemaniLZ();
                    int decompressionSize = 0;

                    bool descomprimido = CallDecompress(ref raw, buffer);

                    if (!descomprimido)
                        return;
                    //raw = weDecompress.DescomprimirArchivoTIM(txtGrafico.Text, offsetGrafico);
                }
                else
                {
                    esCero = true;
                    tam = offsetPaletaRaw;
                    byte[] buffer = new byte[tam];

                    using (FileStream fs = new FileStream(txtGrafico.Text, FileMode.Open, FileAccess.Read))
                    {
                        fs.Position = offsetGrafico;
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                    }
                    raw = buffer;
                }

                // Obtenemos la paleta
                paleta = bytesToTIM.Palette(txtPaleta.Text, offsetPaleta, largoPaleta);

                timPath = Path.GetFileNameWithoutExtension(txtGrafico.Text) + "_GRAF_" + (rowGrafico + 1).ToString() + "_PAL_" + (rowPaleta + 1).ToString() + ".TIM";
                string bmpPath = Path.GetFileNameWithoutExtension(txtGrafico.Text) + "_GRAF_" + (rowGrafico + 1).ToString() + "_PAL_" + (rowPaleta + 1).ToString() + ".BMP";
                //byte[] b = File.ReadAllBytes("0000_COMPRIMIDO.BIN");
                //CrearTIM(timPath, ancho, alto, palx, paly, vramX, vramY, b, paleta);
                CrearTIM(timPath, ancho, alto, palx, paly, vramX, vramY, raw, paleta);

                string rutaNueva = string.Empty;
                int bits = largoPaleta == 32 ? 4 : 8;

                //if (File.Exists(timPath))
                //{
                //    int offsetTIM = 20 + largoPaleta;
                //    long largoArchivo = ObtenerTamanoArchivo(timPath);

                //    using (FileStream fs = new FileStream(timPath,FileMode.Open,FileAccess.Read))
                //    {
                //        fs.Position = offsetTIM;
                //        bufferTIM = new byte[largoArchivo - offsetTIM];
                //        fs.Read(bufferTIM, 0,bufferTIM.Length);
                //    }
                //}

                //raw = bufferTIM;

                Bitmap bmp = null;
                if (!esCero)
                {
                    Color[] colores = weDecompress.ConvertTimPaletteToColorArray(paleta, bits);
                    bmp = tIMtoBMP.CreateBitmapFromRawData(raw, colores, ancho, alto, bits);
                    //bmp = tIMtoBMP.CreateBitmapFromRawData(raw, colores, ancho, alto, bits);
                    if (bmp != null)
                    {
                        bmp.Save(bmpPath, ImageFormat.Bmp);
                        bmp = null;
                    }

                    //weDecompress.CrearBMPs("T_NAME.BMP", raw, offsetGrafico, txtPaleta.Text, offsetPaleta, ancho,alto, bits);
                    //weDecompress.CrearBMP("T_NAME.BMP", txtGrafico.Text, offsetGrafico, txtPaleta.Text, offsetPaleta, ancho,alto, bits, out rutaNueva);
                }
                else
                {
                    Color[] colores = weDecompress.ConvertTimPaletteToColorArray(paleta, bits);
                    bmp = tIMtoBMP.CreateBitmapFromRawData(raw, colores, ancho, alto, bits);
                }
                if (bmp != null)
                {
                    bmp.Save(bmpPath, ImageFormat.Bmp);
                    bmp = null;
                }

                //bmpHelper.CrearBMP(bmpPath, raw, ancho, alto, colores, bits);
                //weDecompress.CrearBMPs("T_NAME.BMP", raw, offsetGrafico, txtPaleta.Text, offsetPaleta, ancho,alto, bits);


                if (File.Exists(bmpPath))
                {
                    pbImagen.Image = Image.FromFile(bmpPath);
                }


                if (File.Exists(timPath))
                {
                    clsWECompressCARP clsWECompressCARP = new clsWECompressCARP();

                    clsWECompressCARP.SaveFile(timPath, Path.Combine(Path.GetDirectoryName(timPath), Path.ChangeExtension(Path.GetFileName(timPath), ".BIN")));
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public long ObtenerTamanoArchivo(string rutaArchivo)
        {
            // Verificar si el archivo existe
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException($"El archivo no existe: {rutaArchivo}");
            }

            // Crear un objeto FileInfo
            FileInfo fileInfo = new FileInfo(rutaArchivo);

            // Obtener el tamaño del archivo en bytes
            long tamanoBytes = fileInfo.Length;

            return tamanoBytes;
        }

        // Click para recuperar datos de paleta
        private void dgvPaleta_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int offsetPaleta = 0;
            int cantidadColores = 0;
            string rutaPaleta = txtPaleta.Text;
            try
            {
                clickPaleta = true;
                rowPaleta = e.RowIndex;
                // No hacemos nada hasta no tener seleccionadas las dos grillas
                if (!clickGrafico || !clickPaleta)
                    return;

                offsetPaleta = Convert.ToInt32(dgvPaleta.Rows[rowPaleta].Cells["colOffsetPaleta"].Value);
                cantidadColores = Convert.ToInt32(dgvPaleta.Rows[rowPaleta].Cells["colAltoPaleta"].Value);
                MostrarPaleta(rutaPaleta, offsetPaleta, cantidadColores);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        // Click de los Labels
        private void LabelClick(object sender, EventArgs e)
        {
            try
            {
                if (sender is Label)
                {
                    clutsIndex = Convert.ToInt32(((Label)sender).Tag);
                    MessageBox.Show(Convert.ToString(clutsIndex));
                }

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        private void CrearTIM(string pathTIM, int ancho, int alto, int palX, int palY, int vramX, int vramY, byte[] raw, byte[] paleta)
        {
            //string tempPath =Path.Combine(Path.GetTempPath() , pathTIM);
            string tempPath = pathTIM;

            BytesToTIM bytesToTIM = new BytesToTIM();
            try
            {
                textBox1.Text = tempPath;
                bytesToTIM.CreateTIM(tempPath, ancho, alto, palX, palY, vramX, vramY, raw, paleta);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            finally
            {
                bytesToTIM = null;
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //byte[] salida = null;
            //CWECompress cWECompress = new CWECompress();
            //ulong tam = 0;
            //ulong tamInicial = 0;
            //using (FileStream fs = new FileStream("T_NAME_GRAF_1_PAL_63.TIM", FileMode.Open, FileAccess.Read))
            //{
            //    fs.Position = 64;
            //    tamInicial = (ulong)fs.Length;
            //    buffer = new byte[(int)tamInicial];
            //    fs.Read(buffer, 0, (int)tamInicial);
            //}
            //cWECompress.Compress(ref salida, buffer, ref tam, tamInicial);

            byte[] buffer = null;
            const int CABEZAL = 64;
            //const int CABEZAL = 0;
            using (FileStream fs = new FileStream("00 - T_NAME.TIM", FileMode.Open, FileAccess.Read))
            //using (FileStream fls = new FileStream("ID_00001.bin", FileMode.Open, FileAccess.Read))
            {
                //fls.Position = 0;
                fs.Position = CABEZAL;
                //buffer = new byte[fls.Length];
                buffer = new byte[fs.Length - CABEZAL];
                fs.Read(buffer, 0, buffer.Length);
                //}
                //byte[] buffer = File.ReadAllBytes("ID_00001.bin");
                BemaniLZ bemaniLZ = new BemaniLZ();
                //long size = bemaniLZ.FindCompressedLength_1(buffer);
                byte[] data = null;
                int ds = 0;
                //File.WriteAllBytes("00_NUEVO_TIM", data);
                data = bemaniLZ.Compress(buffer);
                //bemaniLZ.DeCompress(ref data ,buffer);

                using (FileStream fls = new FileStream("00000100_COMPRIMIDO.BIN", FileMode.Create, FileAccess.Write))
                {
                    fls.Seek(0, SeekOrigin.Begin);
                    fls.Write(data, 0, data.Length);
                }

            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            byte[] buffer;
            //const int CABEZAL = 64;
            const int CABEZAL = 4;
            using (FileStream fls = new FileStream("T_NAME.BIN", FileMode.Open, FileAccess.Read))
            //using (FileStream fls = new FileStream("ID_00001.bin", FileMode.Open, FileAccess.Read))
            {
                //fls.Position = 0;
                fls.Position = CABEZAL;
                //buffer = new byte[fls.Length];
                buffer = new byte[fls.Length - CABEZAL];
                fls.Read(buffer, 0, buffer.Length);
                //buffer = File.ReadAllBytes("T_NAME_GRAF_1_PAL_63.TIM");
                //BemaniLZ bemaniLZ = new BemaniLZ();
                ///byte[] data = null;
                long ds = 0;
                //File.WriteAllBytes("00_NUEVO_TIM", data);
                //data =  bemaniLZ.DeCompress(buffer,ds);
                ds = CallFindCompressedLength(buffer);
                lblLength.Text += $" {Convert.ToString(ds)}";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WECompressByCARP compressByCARP = new WECompressByCARP();
            compressByCARP.SaveFile("TIM_BAT.tim", "TIM_BAT.BIN");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] buffer = null;
            //const int CABEZAL = 64;
            //const int CABEZAL = 0;
            //using (FileStream fs = new FileStream("00 - T_NAME.TIM", FileMode.Open, FileAccess.Read))

            try
            {
                using (FileStream fls = new FileStream("ID_00001.bin", FileMode.Open, FileAccess.Read))
                {
                    fls.Position = 0;
                    //fs.Position = CABEZAL;
                    buffer = new byte[fls.Length];
                    //buffer = new byte[fs.Length - CABEZAL];
                    fls.Read(buffer, 0, buffer.Length);
                    //}
                    //byte[] buffer = File.ReadAllBytes("ID_00001.bin");

                    //long size = bemaniLZ.FindCompressedLength_1(buffer);
                }
                byte[] data = new byte[200000];
                int ds = 0;

                bool descomprimdo = CallDecompress(ref data, buffer);

                if (descomprimdo)
                    MessageBox.Show("Al fin.");
                //File.WriteAllBytes("00_NUEVO_TIM", data);

                // CallDecompress(data, buffer);


                using (FileStream fls = new FileStream("00000100_COMPRIMIDO.BIN", FileMode.Create, FileAccess.Write))
                {
                    fls.Seek(0, SeekOrigin.Begin);
                    fls.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            finally
            {
                buffer = null;

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private static bool CallDecompress(ref byte[] data, byte[] buffer)
        {
            
            // Asumiendo que tienes tus datos de entrada y salida preparados
            byte[] sourceData = buffer; // Datos de origen (BufSrc)
            byte[] destinationData = data; // Buffer de destino (BufDest)
            bool result = false;
            // Usar IntPtr para manejar punteros en C#
            IntPtr ptrSrc = Marshal.AllocHGlobal(sourceData.Length);
            Marshal.Copy(sourceData, 0, ptrSrc, sourceData.Length);

            IntPtr ptrDest = Marshal.AllocHGlobal(destinationData.Length);

            try
            {
                if (DeCompress(ref ptrDest, ptrSrc))
               
                {
                    Marshal.Copy(ptrDest, destinationData, 0, destinationData.Length);
                    // destinationData ahora contiene los datos descomprimidos
                  //byte[]  data = new byte[];
                    Array.Copy(destinationData, 0, data, 0, data.Length);
                    result = true;
                }
                else
                {
                    Console.WriteLine("La descompresión falló.");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrSrc);
                Marshal.FreeHGlobal(ptrDest);
            }
            return result;
        }
        private static long CallFindCompressedLength(byte[] data)
        {
            // Asumiendo que tienes tus datos de entrada y salida preparados
            byte[] sourceData = data; // Datos de origen (BufSrc)
            long result = -1;
            // Usar IntPtr para manejar punteros en C#
            IntPtr ptrSrc = Marshal.AllocHGlobal(sourceData.Length);
            Marshal.Copy(sourceData, 0, ptrSrc, sourceData.Length);

            try
            {
                long dataLength = FindCompressedLength(ptrSrc);
                if (dataLength > 0)
                {
                    // Marshal.Copy(ptrSrc, destinationData, 0, destinationData.Length);
                    // destinationData ahora contiene los datos descomprimidos
                    //data = new byte[dataLength];
                    //Array.Copy(destinationData, 0, data, 0, data.Length);
                    result = dataLength;
                }
                else
                {
                    Console.WriteLine("La lectura de datos falló.");
                }
            }
            finally
            {
               Marshal.FreeHGlobal(ptrSrc);
               //Marshal.FreeHGlobal(ptrDest);
            }
            return result;
        }
        private static bool CallCompress(ref byte[] data, byte[] buffer, ref uint dataSize, uint bufferSize)
        {
            // Asumiendo que tienes tus datos de entrada y salida preparados
            IntPtr ptrSrc = Marshal.AllocHGlobal(buffer.Length);
            IntPtr ptrDest = Marshal.AllocHGlobal(data.Length);
            bool result = false;
            try
            {
                // Copia los datos de origen en el puntero
                Marshal.Copy(buffer, 0, ptrSrc, buffer.Length);

                // Tamaños de los datos
                uint sizeResult = 0;
                 result = Compress(ref ptrDest, ptrSrc, ref sizeResult, bufferSize);

                if (result)
                {
                    // Ajusta el tamaño del arreglo de datos comprimidos
                    byte[] compressedData = new byte[sizeResult];
                    Marshal.Copy(ptrDest, compressedData, 0, (int)sizeResult);

                    // Actualiza el arreglo de salida y el tamaño de los datos
                    data = compressedData;
                    dataSize = sizeResult;
                }
                else
                {
                    Console.WriteLine("La compresión falló.");
                }

                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptrSrc);
                Marshal.FreeHGlobal(ptrDest);
            }
            return result;
        }
            private void button5_Click(object sender, EventArgs e)
        {
            byte[] buffer = null;
            const int CABEZAL = 64;
            //const int CABEZAL = 0;
            //using (FileStream fs = new FileStream("00 - T_NAME.TIM", FileMode.Open, FileAccess.Read))

            try
            {
                using (FileStream fs = new FileStream("Custom.tim", FileMode.Open, FileAccess.Read))
                {
                    //fls.Position = 0;
                    fs.Position = CABEZAL;
                    //buffer = new byte[fs.Length];
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    //}
                    //byte[] buffer = File.ReadAllBytes("ID_00001.bin");

                    //long size = bemaniLZ.FindCompressedLength_1(buffer);
                }
                byte[] data = new byte[200000];
                uint ds = 0;
                uint length = (uint)buffer.Length;
                //int dataSize = FindCompressedLength(buffer)

                bool descomprimdo = CallCompress(ref data, buffer,ref ds,length);

                if (descomprimdo)
                    MessageBox.Show("Al fin.");
                //File.WriteAllBytes("00_NUEVO_TIM", data);

                // CallDecompress(data, buffer);


                using (FileStream fls = new FileStream("00000100_COMPRIMIDO.BIN", FileMode.Create, FileAccess.Write))
                {
                    fls.Seek(0, SeekOrigin.Begin);
                    fls.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            finally
            {
                buffer = null;

            }
        }
    }
}