using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using _7DTD_SingleMapRenderer.CommandLine;
using _7DTD_SingleMapRenderer.Core;

namespace _7DTD_SingleMapRenderer.Settings
{
    [Serializable]
    [XmlRoot("Settings")]
    public class AppSettings : INotifyPropertyChanged
    {
        private string xmlFilename = "settings.xml";
        public string XMLFilename { get { return xmlFilename; } }

        private XmlSerializer serializer;

        private bool isDirty = false;
        public bool IsDirty { get { return isDirty; } }

        public static readonly AppSettings Default = new AppSettings();

        #region ********** Properties **********

        private string m_SaveFolderPath;
        public string SaveFolderPath
        {
            get { return m_SaveFolderPath; }
            set
            {
                if (m_SaveFolderPath != value)
                {
                    m_SaveFolderPath = value;
                    RaisePropertyChanged("SaveFolderPath");
                    isDirty = true;
                }
            }
        }

        private string m_GameRootPath;
        [Option("root", "gameroot", "Path to the game folder \"7 Days To Die\"", Default = @"C:\Program Files(x86)\Steam\SteamApps\common\7 Days To Die")]
        public string GameRootPath
        {
            get { return m_GameRootPath; }
            set
            {
                if (m_GameRootPath != value)
                {
                    m_GameRootPath = value;
                    RaisePropertyChanged("GameRootPath");
                    isDirty = true;
                }
            }
        }

        private string m_GeneratedWorldsFolderPath;
        [Option("worlds", "worlds", "Path to the folder with generated worlds", Default = "")]
        public string GeneratedWorldsFolderPath
        {
            get { return m_GeneratedWorldsFolderPath; }
            set
            {
                if (m_GeneratedWorldsFolderPath != value)
                {
                    m_GeneratedWorldsFolderPath = value;
                    RaisePropertyChanged("GeneratedWorldsFolderPath");
                    isDirty = true;
                }
            }
        }

        private string m_MapFilePath;
        [Option("m", "map", "Path to the map file", Default = "")]
        public string MapFilePath
        {
            get { return m_MapFilePath; }
            set
            {
                if (m_MapFilePath != value)
                {
                    m_MapFilePath = value;
                    RaisePropertyChanged("MapFilePath");
                    isDirty = true;
                }
            }
        }

        private string m_ImageFilePath;
        [Option("i", "image", "Path to the image file", Default = "")]
        public string ImageFilePath
        {
            get { return m_ImageFilePath; }
            set
            {
                if (m_ImageFilePath != value)
                {
                    m_ImageFilePath = value;
                    RaisePropertyChanged("ImageFilePath");
                    isDirty = true;
                }
            }
        }

        private string m_PoiFilePath;
        [Option("p", "poi", "Path to the POI file", Default = "")]
        public string PoiFilePath
        {
            get { return m_PoiFilePath; }
            set
            {
                if (m_PoiFilePath != value)
                {
                    m_PoiFilePath = value;
                    RaisePropertyChanged("PoiFilePath");
                    isDirty = true;
                }
            }
        }

        private TileSizes m_SelectedTileSize;
        [XmlElement("TileSize")]
        [Option("ts", "tilesize", "Size of each map tile (64, 32, 16, 8, 4, 2, 1)", Default = TileSizes.FullSize)]
        public TileSizes SelectedTileSize
        {
            get { return m_SelectedTileSize; }
            set
            {
                if (m_SelectedTileSize != value)
                {
                    m_SelectedTileSize = value;
                    RaisePropertyChanged("SelectedTileSize");
                    isDirty = true;
                }
            }
        }

        private bool m_RenderBackground;
        [XmlElement("DrawBackground")]
        [Option("bg", "background", "Draws background from in-game map", Default = false)]
        public bool RenderBackground
        {
            get { return m_RenderBackground; }
            set
            {
                if (m_RenderBackground != value)
                {
                    m_RenderBackground = value;
                    RaisePropertyChanged("RenderBackground");
                    isDirty = true;
                }
            }
        }

        private bool m_RenderBiomeMap;
        [XmlElement("RenderBiomeMap")]
        [Option("bm", "biomemap", "Draws the biome map as background. Has priority over switch \"background\".", Default = false)]
        public bool RenderBiomeMap
        {
            get { return m_RenderBiomeMap; }
            set
            {
                if (m_RenderBiomeMap != value)
                {
                    m_RenderBiomeMap = value;
                    RaisePropertyChanged("RenderBiomeMap");
                    isDirty = true;
                }
            }
        }

        private bool m_RenderGrid;
        [XmlElement("DrawGrid")]
        [Option("g", "grid", "Draws grid", Default = false)]
        public bool RenderGrid
        {
            get { return m_RenderGrid; }
            set
            {
                if (m_RenderGrid != value)
                {
                    m_RenderGrid = value;
                    RaisePropertyChanged("RenderGrid");
                    isDirty = true;
                }
            }
        }

        private int m_GridSize;
        [Option("gs", "gridsize", "Size of a grid cell", Default = 512)]
        public int GridSize
        {
            get { return m_GridSize; }
            set
            {
                if (m_GridSize != value)
                {
                    m_GridSize = value;
                    RaisePropertyChanged("GridSize");
                    isDirty = true;
                }
            }
        }

        private string m_SelectedGridColor;
        [XmlElement("GridColor")]
        [Option("gc", "gridcolor", "Name of the grid color", Default = "Red")]
        public string SelectedGridColorName
        {
            get { return m_SelectedGridColor; }
            set
            {
                if (m_SelectedGridColor != value)
                {
                    m_SelectedGridColor = value;
                    RaisePropertyChanged("SelectedGridColorName");
                    isDirty = true;
                }
            }
        }

        private int m_AlphaValue;
        [XmlElement("GridAlpha")]
        [Option("ga", "gridalpha", "Alpha value of the grid color", Default = 255)]
        public int AlphaValue
        {
            get { return m_AlphaValue; }
            set
            {
                if (m_AlphaValue != value)
                {
                    if (value >= 255)
                        m_AlphaValue = 255;
                    else if (value <= 0)
                        m_AlphaValue = 0;
                    else
                        m_AlphaValue = value;

                    RaisePropertyChanged("AlphaValue");
                    isDirty = true;
                }
            }
        }

        private bool m_RenderRegionNumbers;
        [Option("rn", "region", "Draws region numbers", Default = false)]
        public bool RenderRegionNumbers
        {
            get { return m_RenderRegionNumbers; }
            set
            {
                if (m_RenderRegionNumbers != value)
                {
                    m_RenderRegionNumbers = value;
                    RaisePropertyChanged("RenderRegionNumbers");
                    isDirty = true;
                }
                if (value == true)
                {
                    this.GridSize = 512;
                }
            }
        }

        private string m_RegionFontName;
        [Option("rfn", "regfontname", "Name of font for region numbers", Default = "Calibri")]
        public string RegionFontName
        {
            get { return m_RegionFontName; }
            set
            {
                if (m_RegionFontName != value)
                {
                    if (isFontInstalled(value))
                    {
                        m_RegionFontName = value;
                        RaisePropertyChanged("RegionFontName");
                        isDirty = true;
                    }
                }
            }
        }

        private float m_RegionFontEmSize;
        [Option("rfs", "regfontsize", "Size of font for region numbers", Default = 20)]
        public float RegionFontEmSize
        {
            get { return m_RegionFontEmSize; }
            set
            {
                if (m_RegionFontEmSize != value)
                {
                    m_RegionFontEmSize = value;
                    RaisePropertyChanged("RegionFontEmSize");
                    isDirty = true;
                }
            }
        }

        private bool m_RenderWaypoints;
        [XmlElement("RenderWaypoints")]
        [Option("w", "waypoints", "Draws waypoints", Default = false)]
        public bool RenderWaypoints
        {
            get { return m_RenderWaypoints; }
            set
            {
                if (m_RenderWaypoints != value)
                {
                    m_RenderWaypoints = value;
                    RaisePropertyChanged("RenderWaypoints");
                    isDirty = true;
                }
            }
        }

        private bool m_RenderPrefabMarker;
        [XmlElement("RenderPrefabMarker")]
        [Option("pm", "prefabmarker", "Draws prefabs as waypoints. Requires option 'waypoints'.", Default = false)]
        public bool RenderPrefabMarker
        {
            get { return m_RenderPrefabMarker; }
            set
            {
                if (m_RenderPrefabMarker != value)
                {
                    m_RenderPrefabMarker = value;
                    RaisePropertyChanged("RenderPrefabMarker");
                    isDirty = true;
                }
            }
        }

        private string m_WaypointFontName;
        [Option("wfn", "wayfontname", "Name of font for waypoint names", Default = "Calibri")]
        public string WaypointFontName
        {
            get { return m_WaypointFontName; }
            set
            {
                if (m_WaypointFontName != value)
                {
                    if (isFontInstalled(value))
                    {
                        m_WaypointFontName = value;
                        RaisePropertyChanged("WaypointFontName");
                        isDirty = true;
                    }
                }
            }
        }

        private float m_WaypointFontEmSize;
        [Option("wfs", "wayfontsize", "Size of font for waypoint names", Default = 14)]
        public float WaypointFontEmSize
        {
            get { return m_WaypointFontEmSize; }
            set
            {
                if (m_WaypointFontEmSize != value)
                {
                    m_WaypointFontEmSize = value;
                    RaisePropertyChanged("WaypointFontEmSize");
                    isDirty = true;
                }
            }
        }

        private string m_WaypointFontColor;
        [XmlElement("WaypointFontColor")]
        [Option("wfc", "wayfontcolor", "Name of the waypoint font color", Default = "WhiteSmoke")]
        public string SelectedWaypointFontColorName
        {
            get { return m_WaypointFontColor; }
            set
            {
                if (m_WaypointFontColor != value)
                {
                    m_WaypointFontColor = value;
                    RaisePropertyChanged("SelectedWaypointFontColorName");
                    isDirty = true;
                }
            }
        }

        private bool m_UseDataStore;
        [XmlElement("UseDataStore")]
        [Option("ds", "datastore", "Uses a DataStore for all map tiles", Default = false)]
        public bool UseDataStore
        {
            get { return m_UseDataStore; }
            set
            {
                if (m_UseDataStore != value)
                {
                    m_UseDataStore = value;
                    RaisePropertyChanged("UseDataStore");
                    isDirty = true;
                }
            }
        }

        private List<SaveGame> m_SaveGames;
        [XmlArray]
        public List<SaveGame> SaveGames
        {
            get { return m_SaveGames; }
            set
            {
                if (m_SaveGames != value)
                {
                    m_SaveGames = value;
                    RaisePropertyChanged("SaveGames");
                    isDirty = true;
                }
            }
        }

        #endregion


        public AppSettings()
        {
            this.serializer = new XmlSerializer(typeof(AppSettings));
            this.SaveGames = new List<SaveGame>();
            LoadDefaults();
        }

        public AppSettings(string filename)
            : this()
        {
            this.xmlFilename = filename;
        }

        public void LoadDefaults()
        {
            this.SaveFolderPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"7DaysToDie\Saves");

            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<OptionAttribute>();
                if (attr != null)
                {
                    prop.SetValue(this, Convert.ChangeType(attr.Default, prop.PropertyType));
                }
            }

            this.isDirty = true;
        }

        public void Load()
        {
            if (!File.Exists(this.XMLFilename))
                return;
            using (TextReader textReader = new StreamReader(this.XMLFilename))
            {
                object desirialized = this.serializer.Deserialize(textReader);

                if (desirialized is AppSettings)
                {
                    AppSettings result = (AppSettings)desirialized;
                    result.CopyTo(this);
                }
            }
            this.isDirty = false;
        }

        public void Save(bool force = false)
        {
            if (!isDirty && !force)
                return;

            using (TextWriter textWriter = new StreamWriter(this.XMLFilename))
                this.serializer.Serialize(textWriter, this);

            this.isDirty = false;
        }

        public void RaiseAllPropertiesChanged()
        {
            var props = typeof(AppSettings).GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanRead)
                    continue;
                RaisePropertyChanged(prop.Name);
            }
        }

        public void CopyTo(AppSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            // TODO: Listen behandeln
            var props = typeof(AppSettings).GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite)
                    continue;
                object value = prop.GetValue(this);
                prop.SetValue(settings, value);
            }
        }

        public static IEnumerable<string> GetAllInstalledFontNames()
        {
            var fontsCollection = new System.Drawing.Text.InstalledFontCollection();
            var fontFamilies = fontsCollection.Families;

            // ein bisschen ausführlicher programmiert -> alle Fontnamen im Debugging sehen
            var fonts = new List<string>();
            foreach (var font in fontFamilies)
            {
                fonts.Add(font.Name);
            }
            return fonts;
        }

        private static bool isFontInstalled(string fontname)
        {
            var installedFontCollection = new System.Drawing.Text.InstalledFontCollection();
            return installedFontCollection.Families.Any(ff => ff.Name == fontname);
        }

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

    }
}
