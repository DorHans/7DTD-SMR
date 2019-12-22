using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _7DTD_SingleMapRenderer.Core;
using _7DTD_SingleMapRenderer.Services;
using _7DTD_SingleMapRenderer.Settings;
using _7DTD_SingleMapRenderer.Util;

namespace _7DTD_SingleMapRenderer.Presentation.Windows
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IProgressService
    {
        #region ********** Felder und Properties **********

        private Microsoft.Win32.OpenFileDialog m_OpenFileDialog;
        private Microsoft.Win32.OpenFileDialog m_OpenPoiFileDialog;
        private Microsoft.Win32.SaveFileDialog m_SaveFileDialog;
        private Stopwatch m_Stopwatch;
        private SaveGame m_CustomMapFile;

        public bool IsDebugBuild { get; private set; }

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

        private AppSettings m_Settings;
        public AppSettings Settings
        {
            get { return m_Settings; }
            set
            {
                if (m_Settings != value)
                {
                    m_Settings = value;
                    RaisePropertyChanged("Settings");
                    m_Settings.RaiseAllPropertiesChanged();
                }
            }
        }

        private SaveGame m_SelectedSaveGame;
        public SaveGame SelectedSaveGame
        {
            get { return m_SelectedSaveGame; }
            set
            {
                if (m_SelectedSaveGame != value)
                {
                    m_SelectedSaveGame = value;
                    RaisePropertyChanged("SelectedSaveGame");
                    this.Settings.MapFilePath = value.MapfilePath;
                    this.Settings.PoiFilePath = value.PoiFilePath;
                    if (!String.IsNullOrEmpty(value.ImageFilePath))
                    {
                        this.Settings.ImageFilePath = value.ImageFilePath;
                        this.m_SaveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(value.ImageFilePath);
                        this.m_SaveFileDialog.FileName = value.ImageFilePath;
                    }
                }
            }
        }


        #endregion

        #region ********** Konstruktor und Initialisierung **********

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
#if DEBUG
            this.IsDebugBuild = true;
#else
            this.IsDebugBuild = false;
#endif
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.m_Stopwatch = new Stopwatch();
            this.Settings = new AppSettings();
            try
            {
                this.Settings.Load();
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ?
                    " " + ex.InnerException.Message :
                    String.Empty;
                this.StatusText = ex.Message + message;
            }

            this.m_OpenFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (this.Settings != null)
            {
                this.m_OpenFileDialog.InitialDirectory = this.Settings.SaveFolderPath;
                this.m_OpenFileDialog.FileName = this.Settings.MapFilePath;
            }
            this.m_OpenFileDialog.DefaultExt = ".map";
            this.m_OpenFileDialog.Filter = "Map files|*.map";

            this.m_OpenPoiFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (this.Settings != null)
            {
                this.m_OpenPoiFileDialog.InitialDirectory = this.Settings.SaveFolderPath;
                this.m_OpenPoiFileDialog.FileName = this.Settings.PoiFilePath;
            }
            this.m_OpenPoiFileDialog.DefaultExt = ".csv";
            this.m_OpenPoiFileDialog.Filter = "Poi files|*.csv";

            this.m_SaveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (this.Settings != null)
            {
                this.m_SaveFileDialog.InitialDirectory = this.Settings.SaveFolderPath;
                this.m_SaveFileDialog.FileName = this.Settings.ImageFilePath;
            }
            this.m_SaveFileDialog.DefaultExt = ".png";
            this.m_SaveFileDialog.Filter = "Portable Network Graphics|*.png";

            // TODO: überarbeiten .. Bedingungen sind nicht logisch
            try
            {
                if (this.Settings != null && Directory.Exists(this.Settings.SaveFolderPath))
                {
                    var savegames = this.Settings.SaveGames;
                    this.Settings.SaveGames = new List<SaveGame>();

                    // lade alle Spielstände
                    foreach (var folder in Directory.GetDirectories(this.Settings.SaveFolderPath))
                    {
                        string item = System.IO.Path.GetFileName(folder);
                        {
                            string[] dirs = Directory.GetDirectories(folder);
                            foreach (var dir in dirs)
                            {
                                var files = Directory.GetFiles(dir, "*.map", SearchOption.AllDirectories);
                                if (files.Length == 0)
                                    continue;

                                var name = item + " - " + System.IO.Path.GetFileName(dir);
                                SaveGame sg =
                                    savegames.FirstOrDefault(s => s.Name == name) ??
                                    new SaveGame(name, files[0]);
                                this.Settings.SaveGames.Add(sg);
                            }
                        }
                    }
                }
                // finde die zuletzt geladene Karte
                foreach (var sg in this.Settings.SaveGames)
                {
                    if (String.Equals(sg.MapfilePath, this.Settings.MapFilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        this.SelectedSaveGame = sg;
                        this.Settings.ImageFilePath = sg.ImageFilePath;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.StatusText = ex.GetType().ToString() + ": " + ex.Message;
            }

            // setze einen speziellen Custom-Wert
            if (this.SelectedSaveGame == null)
            {
                m_CustomMapFile = new SaveGame("Custom . . .", this.Settings.MapFilePath);
                m_CustomMapFile.ImageFilePath = this.Settings.ImageFilePath;
            }
            else
            {
                m_CustomMapFile = new SaveGame("Custom . . .", String.Empty);
            }
            if (!this.Settings.SaveGames.Any(s => s.MapfilePath == m_CustomMapFile.MapfilePath))
                this.Settings.SaveGames.Add(m_CustomMapFile);

            if (this.SelectedSaveGame == null)
                this.SelectedSaveGame = m_CustomMapFile;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.Settings != null)
                this.Settings.Save();
        }


        #endregion

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        #region ********** IProgressService **********
        void IProgressService.Report(string message)
        {
            this.StatusText = message;
        }
        void IProgressService.Report(string format, params object[] args)
        {
            this.StatusText = String.Format(format, args);
        }
        #endregion

        #region ********** Menü **********
        private void About_Clicked(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void Quit_Clicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void menuSettingsLoadDefault_Click(object sender, RoutedEventArgs e)
        {
            this.Settings.LoadDefaults();
            this.SelectedSaveGame = m_CustomMapFile;
            this.Settings.RaiseAllPropertiesChanged();
        }

        private void menuSettingsEdit_Click(object sender, RoutedEventArgs e)
        {
            var setwnd = new SettingsWindow(this.Settings);
            setwnd.Owner = this;
            var result = setwnd.ShowDialog();
            if (result == true)
            {
                setwnd.Settings.CopyTo(this.Settings);
            }
        }

        #endregion

        #region ********** Buttons **********

        private void buttonBrowseMapFile_Click(object sender, RoutedEventArgs e)
        {
            // Show load file dialog box
            var dlgresult = m_OpenFileDialog.ShowDialog();

            if (dlgresult.Value == true)
            {
                this.Settings.MapFilePath = this.m_OpenFileDialog.FileName;
                this.m_OpenFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(this.Settings.MapFilePath);
                this.m_CustomMapFile.MapfilePath = this.Settings.MapFilePath;
                this.SelectedSaveGame = this.m_CustomMapFile;
            }
        }

        private void buttonExportAsPng_Click(object sender, RoutedEventArgs e)
        {
            this.StatusText = "Starting export.";

            if (this.IsRunning)
            {
                this.StatusText = "Please wait until current process is completed.";
                return;
            }

            if (!System.IO.File.Exists(this.Settings.MapFilePath))
            {
                this.StatusText = "Please select a map file first.";
                return;
            }

            var dlgresult = m_SaveFileDialog.ShowDialog();
            if (dlgresult.Value == true)
            {
                string imagefilename = this.m_SaveFileDialog.FileName;
                this.Settings.ImageFilePath = imagefilename;
                if (this.SelectedSaveGame != null)
                    this.SelectedSaveGame.ImageFilePath = imagefilename;

                // Ausgabedatei prüfen, ob Ordner existiert und Dateiname mit .png endet
                string outputFolder = System.IO.Path.GetDirectoryName(imagefilename);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                if (!imagefilename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    imagefilename += ".png";


                this.m_Stopwatch.Restart();
                this.IsRunning = true;
                var bw = new BackgroundWorker();
                bw.DoWork += (s, a) =>
                {
                    var pois = POI.FromCsvFile(this.Settings.PoiFilePath);

                    //var waypoints = Util.GetPOIsFromSavegame(this.Settings.MapFilePath);
                    string directory = System.IO.Path.GetDirectoryName(this.Settings.MapFilePath);
                    string ttpfilename = System.IO.Path.GetFileNameWithoutExtension(this.Settings.MapFilePath) + ".ttp";
                    ttpfilename = System.IO.Path.Combine(directory, ttpfilename);
                    var waypoints = POI.FromTtpFile(ttpfilename);
                    pois = pois.Concat(waypoints);

                    var map = new MapRenderer();
                    map.TileSize = (int)this.Settings.SelectedTileSize;
                    map.RenderBackground = this.Settings.RenderBackground;
                    map.RenderGrid = this.Settings.RenderGrid;
                    map.GridSize = this.Settings.GridSize;
                    map.GridColor = System.Drawing.Color.FromArgb(this.Settings.AlphaValue,
                        System.Drawing.Color.FromName(this.Settings.SelectedGridColorName));
                    map.RenderRegionNumbers = this.Settings.RenderRegionNumbers;
                    map.RegionFontName = this.Settings.RegionFontName;
                    map.RegionFontEmSize = this.Settings.RegionFontEmSize;
                    map.RenderWaypoints = this.Settings.RenderWaypoints;
                    map.WaypointFontColor = System.Drawing.Color.FromName(this.Settings.SelectedWaypointFontColorName);
                    map.WaypointFontName = this.Settings.WaypointFontName;
                    map.WaypointFontEmSize = this.Settings.WaypointFontEmSize;
                    map.UseDataStore = this.Settings.UseDataStore;

                    map.RenderWholeMap(this.Settings.MapFilePath, imagefilename, pois, (IProgressService)this);
                };
                bw.RunWorkerCompleted += (s, a) =>
                {
                    this.m_Stopwatch.Stop();
                    this.IsRunning = false;
                    if (a.Error != null)
                    {
                        this.StatusText = a.Error.GetType().ToString() + ": " + a.Error.Message.ToString();
                    }
                    else
                    {
                        this.StatusText = String.Format("Done in {0}.", this.m_Stopwatch.Elapsed);
                    }
                };
                bw.RunWorkerAsync();

                this.m_SaveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(this.m_SaveFileDialog.FileName);
            }
            else
            {
                this.StatusText = "Export aborted.";
            }
        }

        private void buttonOpenExportFolder_Click(object sender, RoutedEventArgs e)
        {
            if (this.m_SaveFileDialog == null)
                return;

            string folder = this.m_SaveFileDialog.InitialDirectory;
            if (String.IsNullOrEmpty(folder))
                return;
            if (!System.IO.Directory.Exists(folder))
                return;

            string filename = this.m_SaveFileDialog.FileName;
            if (System.IO.File.Exists(filename))
            {
                ShellHelper.OpenFolderAndSelectFile(filename);
                return;
            }

            filename = System.IO.Path.Combine(folder, filename);
            if (System.IO.File.Exists(filename))
            {
                ShellHelper.OpenFolderAndSelectFile(filename);
                return;
            }

            Process.Start("explorer.exe", folder);
        }

        private void buttonBrowsePoiFile_Click(object sender, RoutedEventArgs e)
        {
            var dlgresult = m_OpenPoiFileDialog.ShowDialog();
            if (dlgresult.Value == true)
            {
                this.Settings.PoiFilePath = this.m_OpenPoiFileDialog.FileName;
                this.SelectedSaveGame.PoiFilePath = this.m_OpenPoiFileDialog.FileName;
                this.m_OpenPoiFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(this.Settings.PoiFilePath);
            }
        }

        #endregion

        #region ********** ExperimentalTools **********

        private void buttonToolsFonttest_Click(object sender, RoutedEventArgs e)
        {
            var childs = this.OwnedWindows.OfType<_7DTD_SingleMapRenderer.Tools.FontTest.FontTestWindow>();
            if (childs.Count() > 0)
            {
                var wnd = childs.First();
                if (wnd.WindowState == WindowState.Minimized)
                    wnd.WindowState = WindowState.Normal;
                wnd.Focus();
            }
            else
            {
                var wnd = new _7DTD_SingleMapRenderer.Tools.FontTest.FontTestWindow();
                wnd.Owner = this;
                wnd.Show();
            }
        }

        private void buttonXmlCsvConverter_Click(object sender, RoutedEventArgs e)
        {
            var childs = this.OwnedWindows.OfType<_7DTD_SingleMapRenderer.Tools.XmlConverter.XmlCsvConverterWindow>();
            if (childs.Count() > 0)
            {
                var wnd = childs.First();
                if (wnd.WindowState == WindowState.Minimized)
                    wnd.WindowState = WindowState.Normal;
                wnd.Focus();
            }
            else
            {
                var wnd = new _7DTD_SingleMapRenderer.Tools.XmlConverter.XmlCsvConverterWindow();
                wnd.Owner = this;
                wnd.Show();
            }
        }

        private void buttonItemAtlasSplitter_Click(object sender, RoutedEventArgs e)
        {
            var childs = this.OwnedWindows.OfType<_7DTD_SingleMapRenderer.Tools.ItemAtlas.ItemAtlasSplitter>();
            if (childs.Count() > 0)
            {
                var wnd = childs.First();
                if (wnd.WindowState == WindowState.Minimized)
                    wnd.WindowState = WindowState.Normal;
                wnd.Focus();
            }
            else
            {
                var wnd = new _7DTD_SingleMapRenderer.Tools.ItemAtlas.ItemAtlasSplitter();
                wnd.Owner = this;
                wnd.Show();
            }
        }

        private void menuRegionViewer_Click(object sender, RoutedEventArgs e)
        {
            var vm = new _7DTD_SingleMapRenderer.Tools.RegionViewer.RegionViewerViewModel(this.Settings.MapFilePath);
            var wnd = new _7DTD_SingleMapRenderer.Tools.RegionViewer.RegionViewer(vm);
            wnd.Owner = this;
            wnd.Show();
        }

        #endregion

    }
}
