using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using _7DTD_SingleMapRenderer.BaseClasses;

namespace _7DTD_SingleMapRenderer.Tools.RegionViewer
{
    public class RegionViewerViewModel : BaseViewModel
    {
        private int tileSize = 16;
        private string m_MapFilePath;
        private Dictionary<UInt32, byte[]> m_AllTiles;

        #region ********** Properties and Fields for Binding **********

        private ImageSource m_MapImage;

        public ImageSource MapImage
        {
            get { return m_MapImage; }
            set
            {
                if (m_MapImage != value)
                {
                    m_MapImage = value;
                    RaisePropertyChanged("MapImage");
                }
            }
        }

        private ObservableCollection<RegionFileViewModel> m_RegionFiles;

        public ObservableCollection<RegionFileViewModel> RegionFiles
        {
            get { return m_RegionFiles; }
            set
            {
                if (m_RegionFiles != value)
                {
                    m_RegionFiles = value;
                    RaisePropertyChanged("RegionFiles");
                }
            }
        }

        private RegionFileViewModel m_SelectedRegionFile;

        public RegionFileViewModel SelectedRegionFile
        {
            get { return m_SelectedRegionFile; }
            set
            {
                if (m_SelectedRegionFile != value)
                {
                    m_SelectedRegionFile = value;
                    RaisePropertyChanged("SelectedRegionFile");
                    this.MapImage = value.GetImageSource2();
                }
            }
        }

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

        #endregion

        public RegionViewerViewModel(string mapFilePath)
        {
            this.m_MapFilePath = mapFilePath;
            this.m_AllTiles = Core.MapRenderer.getAllTiles(mapFilePath);

            var list = getAllRegionfiles(m_AllTiles);
            list.Sort();
            this.m_RegionFiles = new ObservableCollection<RegionFileViewModel>(list);
        }

        private List<RegionFileViewModel> getAllRegionfiles(Dictionary<UInt32, byte[]> tiles)
        {
            var regions = new List<RegionFileViewModel>();

            int regionSize = 512 / 16;

            foreach (var tile in tiles)
            {
                var key = tile.Key;
                // get position of chunk
                int x = (Int16)(key & 0xFFFF);
                int y = (Int16)((key >> 16));

                // get region from chunk
                int regX = (int)Math.Floor((double)x / regionSize);
                int regY = (int)Math.Floor((double)y / regionSize);

                // find region
                var region = regions.FirstOrDefault(r => r.X == regX && r.Y == regY);
                if (region == null)
                {
                    region = new RegionFileViewModel(regX, regY);
                    regions.Add(region);
                }
                // add tile to region
                region.SetTile(key, tile.Value);
            }

            return regions;
        }
    }
}
