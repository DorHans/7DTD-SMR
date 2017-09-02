using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using _7DTD_SingleMapRenderer.Settings;

namespace _7DTD_SingleMapRenderer.Tools.FontTest
{
    /// <summary>
    /// Interaktionslogik für FontTestWindow.xaml
    /// </summary>
    public partial class FontTestWindow : Window
    {
        public float Fontsize { get; set; }
        public string SelectedGridColorName { get; set; }

        public FontTestWindow()
        {
            InitializeComponent();
            this.Fontsize = 20;
            this.SelectedGridColorName = "Red";
            this.DataContext = this;
        }


        private void renderFontTest(string imagefilename)
        {
            var gridColor = System.Drawing.Color.FromName(this.SelectedGridColorName);
            float fontSize = Fontsize;

            var allFontNames = new List<string>(AppSettings.GetAllInstalledFontNames());
            int sizeX = 1024;
            int sizeY = (allFontNames.Count + 1) * 30;

            using (Bitmap big_tile = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format16bppRgb555))
            using (Graphics g = Graphics.FromImage(big_tile))
            using (var drawBrush = new System.Drawing.SolidBrush(gridColor))
            {
                //g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White), 0, 0, sizeX, sizeY);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                var start = new PointF(20, 25);
                foreach (string fontname in allFontNames)
                {
                    var font = new System.Drawing.Font(fontname, fontSize);
                    string drawString = "r.0.-1\t0123456789\t" + fontname;
                    g.DrawString(drawString, font, drawBrush, start);
                    start.Y += 30;
                    font.Dispose();
                }
                big_tile.Save(imagefilename, ImageFormat.Png);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            saveFileDialog.FileName = "fontTest.png";
            saveFileDialog.DefaultExt = ".png";
            saveFileDialog.Filter = "Portable Network Graphics|*.png";

            var dlgresult = saveFileDialog.ShowDialog();
            if (dlgresult.Value == true)
            {
                string imagefilename = saveFileDialog.FileName;

                // Ausgabedatei prüfen, ob Ordner existiert und Dateiname mit .png endet
                string outputFolder = System.IO.Path.GetDirectoryName(imagefilename);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                if (!imagefilename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    imagefilename += ".png";

                renderFontTest(imagefilename);

                MessageBox.Show("done");
            }
        }
    }
}
