using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace _7DTD_SingleMapRenderer.Tools.ItemAtlas
{
    /// <summary>
    /// Interaktionslogik für ItemAtlasSplitter.xaml
    /// </summary>
    public partial class ItemAtlasSplitter : Window, INotifyPropertyChanged
    {
        private BackgroundWorker m_BackgroundWorker;
        private Stopwatch m_Stopwatch;

        private string m_StatusText;
        public string StatusText
        {
            get { return m_StatusText; }
            set
            {
                if (m_StatusText != value)
                {
                    m_StatusText = value;
                    RaisePropertyChanged("StatusText");
                }
            }
        }

        private bool m_IsRunning;
        public bool IsRunning
        {
            get { return m_IsRunning; }
            set
            {
                if (m_IsRunning != value)
                {
                    m_IsRunning = value;
                    RaisePropertyChanged("IsRunning");
                }
            }
        }


        public ItemAtlasSplitter()
        {
            InitializeComponent();
            this.m_Stopwatch = new Stopwatch();
            this.m_BackgroundWorker = new BackgroundWorker();
            this.m_BackgroundWorker.WorkerSupportsCancellation = true;
            this.m_BackgroundWorker.DoWork += m_BackgroundWorker_DoWork;
            this.m_BackgroundWorker.RunWorkerCompleted += m_BackgroundWorker_RunWorkerCompleted;
            this.DataContext = this;
        }


        void m_BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string imagefile = "Assets\\ItemIcons.png";
            string textfile = "Assets\\ItemIcons.txt";
            string foldername = System.IO.Path.Combine("Resources", "ItemIcons");
            System.Drawing.Size size;
            Dictionary<string, Rectangle> iconDescriptions = getIconDescriptions(textfile, out size);
            if (iconDescriptions == null || iconDescriptions.Count == 0)
            {
                StatusText = "No ItemIcons.txt found or file doesn't contain any valid lines.";
                return;
            }
            var iconCount = iconDescriptions.Count;

            if (!Directory.Exists(foldername))
                Directory.CreateDirectory(foldername);

            using (var fs = File.OpenRead(imagefile))
            using (Bitmap itemIcons = new Bitmap(fs))
            {
                int i = 0;
                foreach (var icondescription in iconDescriptions)
                {
                    StatusText = String.Format("Generating icon {0}/{1}: {2}", ++i, iconCount, icondescription.Key);

                    var name = System.IO.Path.Combine(foldername, icondescription.Key + ".png");
                    var rect = icondescription.Value;
                    var cropped = CropBitmap(itemIcons, rect.X, rect.Y, rect.Width, rect.Height);
                    cropped.Save(name);

                    if (this.m_BackgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }

        void m_BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.m_Stopwatch.Stop();
            this.IsRunning = false;
            if (e.Error != null)
            {
                this.StatusText = e.Error.GetType().ToString() + ": " + e.Error.Message.ToString();
            }
            else if (e.Cancelled)
            {
                this.StatusText = "Aborted by user.";
            }
            else
            {
                this.StatusText = String.Format("Done in {0}.", this.m_Stopwatch.Elapsed);
            }
        }

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.m_Stopwatch.Restart();
            this.IsRunning = true;
            this.m_BackgroundWorker.RunWorkerAsync();
        }

        private void ButtonAbort_Click(object sender, RoutedEventArgs e)
        {
            this.m_BackgroundWorker.CancelAsync();
        }

        private static Dictionary<string, Rectangle> getIconDescriptions(string textfile, out System.Drawing.Size size)
        {
            var icons = new Dictionary<string, Rectangle>();
            size = new System.Drawing.Size();
            /* Format:
             * erste Zeile -> Größe der einzelnen Icons
             * weitere Zeilen -> Name und Offset zum Icon
             * Die Werte in den Zeilen sind durch Tabstops getrennt
             * Beispiel einer ItemIcons.txt:
             * 116	80
             * stone	0	0
             * grass	117	0
             */
            using (var fs = File.OpenText(textfile))
            {
                string line = fs.ReadLine();
                string[] splitted = line.Split('\t');
                int num1 = 0, num2 = 0;
                bool flag = true;
                if (splitted.Length == 2)
                {
                    flag &= int.TryParse(splitted[0], out num1);
                    flag &= int.TryParse(splitted[1], out num2);
                }

                if (!flag || num1 == 0 || num2 == 0)
                    return null;

                size = new System.Drawing.Size(num1, num2);

                while (!String.IsNullOrEmpty(line))
                {
                    line = fs.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        break;

                    splitted = line.Split('\t');
                    if (splitted.Length != 3)
                        break;

                    flag = true;
                    flag &= int.TryParse(splitted[1], out num1);
                    flag &= int.TryParse(splitted[2], out num2);
                    if (!flag)
                        break;

                    icons[splitted[0]] = new Rectangle(num1, num2, size.Width, size.Height);
                }

            }

            return icons;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/13893517/extract-a-portion-of-an-image-in-net
        /// </summary>
        /// <param name="bitmap">Quelle</param>
        /// <param name="cropX">X-Position in der Quelle</param>
        /// <param name="cropY">Y-Position in der Quelle</param>
        /// <param name="cropWidth">Breite des Ziels</param>
        /// <param name="cropHeight">Höhe des Ziels</param>
        /// <returns>Image des Ziels</returns>
        private System.Drawing.Image CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight);
            System.Drawing.Image cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
