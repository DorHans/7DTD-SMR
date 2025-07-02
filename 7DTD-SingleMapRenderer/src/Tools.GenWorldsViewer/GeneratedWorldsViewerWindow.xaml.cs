using _7DTD_SingleMapRenderer.Core;
using _7DTD_SingleMapRenderer.Settings;
using SevenDaysSaveManipulator.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using _7DTD_SingleMapRenderer.Properties;
using System.Collections.ObjectModel;

namespace _7DTD_SingleMapRenderer.Tools.GenWorldsViewer
{
    /// <summary>
    /// Interaktionslogik für GeneratedWorldsViewer.xaml
    /// </summary>
    public partial class GeneratedWorldsViewer : Window, INotifyPropertyChanged
    {
        private List<SevenDaysSaveManipulator.Data.Prefab> prefabs;

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

        private ObservableCollection<GeneratedWorld> m_Worlds;

        public ObservableCollection<GeneratedWorld> Worlds
        {
            get { return m_Worlds; }
            set
            {
                if (m_Worlds != value)
                {
                    m_Worlds = value;
                    RaisePropertyChanged("Worlds");
                }
            }
        }


        public GeneratedWorldsViewer()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Worlds = new ObservableCollection<GeneratedWorld>();
            this.Loaded += generatedWorldsViewer_Loaded;
        }

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private void generatedWorldsViewer_Loaded(object sender, RoutedEventArgs e)
        {
            this.Settings = new AppSettings();
            try
            {
                this.Settings.Load();

                var prefabFolder = System.IO.Path.Combine(this.Settings.GameRootPath, "Data", "Prefabs");
                if (Directory.Exists(prefabFolder))
                    this.prefabs = new List<SevenDaysSaveManipulator.Data.Prefab>(
                        SevenDaysSaveManipulator.Data.Prefab.GetPrefabs(prefabFolder)
                        );
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ?
                    " " + ex.InnerException.Message :
                    String.Empty;
                this.StatusText = ex.Message + message;
            }

            this.Worlds.Clear();
            var worldPaths = getAllGeneratedWorlds(Settings);
            foreach (var worldPath in worldPaths)
            {
                var world = new GeneratedWorld(Settings, worldPath, prefabs);
                this.Worlds.Add(world);

                //if (Worlds.Count > 2) break;
            }
        }

        private List<string> getAllGeneratedWorlds(AppSettings settings)
        {
            var list = new List<string>();

            list.AddRange(Directory.GetDirectories(settings.GeneratedWorldsFolderPath));

            string pregen_Worlds = System.IO.Path.Combine(settings.GameRootPath, "Data", "Worlds");
            list.AddRange(Directory.GetDirectories(pregen_Worlds));

            return list;
        }

    }
}
