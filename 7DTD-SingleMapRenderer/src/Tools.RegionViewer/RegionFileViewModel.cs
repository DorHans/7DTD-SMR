using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using _7DTD_SingleMapRenderer.BaseClasses;

namespace _7DTD_SingleMapRenderer.Tools.RegionViewer
{
    public class RegionFileViewModel : BaseViewModel, IComparable<RegionFileViewModel>
    {
        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        private int m_X;

        public int X
        {
            get { return m_X; }
            set
            {
                if (m_X != value)
                {
                    m_X = value;
                    RaisePropertyChanged("X");
                }
            }
        }

        private int m_Y;

        public int Y
        {
            get { return m_Y; }
            set
            {
                if (m_Y != value)
                {
                    m_Y = value;
                    RaisePropertyChanged("Y");
                }
            }
        }

        private Dictionary<UInt32, byte[]> m_Tiles;
        private ImageSource m_Image;

        private ObservableCollection<ChunkViewModel> m_Chunks;

        public ObservableCollection<ChunkViewModel> Chunks
        {
            get { return m_Chunks; }
            set
            {
                if (m_Chunks != value)
                {
                    m_Chunks = value;
                    RaisePropertyChanged("Chunks");
                }
            }
        }

        public RegionFileViewModel(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.m_Name = String.Format("r.{0}.{1}", x, y);

            this.m_Tiles = new Dictionary<UInt32, byte[]>();

            var list = new List<ChunkViewModel>();
            // offset for the chunks
            int offx = x * 32;
            int offy = y * 32;
            // chunks are displayed in a UniformGrid -> order of creation is important
            // create all chunks in the first row, then switch to next row
            for (int j = 31; j >= 0; j--) // y - row -> top row has the highest index
                for (int i = 0; i < 32; i++) // x - column
                {
                    var chunk = new ChunkViewModel(this, i + offx, j + offy);
                    list.Add(chunk);
                }
            this.m_Chunks = new ObservableCollection<ChunkViewModel>(list);
        }

        public void SetTile(UInt32 key, byte[] value)
        {
            this.m_Tiles[key] = value;
        }

        public ImageSource GetImageSource()
        {
            int sizeX = 512;
            int sizeY = 512;
            int maxY = 31;
            int minX = 0;
            int offset = 0;
            int tileSize = 16;

            // Tiles wieder zusammensetzen
            using (Bitmap big_tile = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format16bppRgb555))
            {
                Rectangle rect = new Rectangle(0, 0, sizeX, sizeY);
                BitmapData bmpData = big_tile.LockBits(rect, ImageLockMode.ReadWrite, big_tile.PixelFormat);
                IntPtr basePtr = bmpData.Scan0;
                IntPtr ptr = basePtr;

                foreach (var tile in m_Tiles)
                {
                    int x = (Int16)(tile.Key & 0xFFFF);
                    int y = (Int16)((tile.Key >> 16));

                    x = x < 0 ? x % 32 + 32 : x % 32;
                    y = y < 0 ? y % 32 + 32 : y % 32;

                    int off = ((x % 32 - minX + offset) + (maxY - y % 32 + offset) * sizeX) * tileSize;
                    ptr = basePtr + off * 2;
                    for (int j = 0; j < 16; j++)
                    {
                        System.Runtime.InteropServices.Marshal.Copy(tile.Value, 480 - j * 32, ptr, 32);
                        ptr = ptr + sizeX * 2;
                    }
                }
                big_tile.UnlockBits(bmpData);

                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              big_tile.GetHbitmap(),
                              IntPtr.Zero,
                              System.Windows.Int32Rect.Empty,
                              BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public ImageSource GetImageSource2()
        {
            if (m_Image != null)
                return m_Image;

            int sizeX = 512;
            int sizeY = 512;
            int maxY = 31;
            int minX = 0;
            int offset = 0;
            int tileSize = 16;
            int ptr = 0;

            byte[] memory = new byte[sizeX * sizeY * 2];

            foreach (var tile in m_Tiles)
            {
                int x = (Int16)(tile.Key & 0xFFFF);
                int y = (Int16)((tile.Key >> 16));

                x = x < 0 ? x % 32 + 32 : x % 32;
                y = y < 0 ? y % 32 + 32 : y % 32;

                int off = ((x % 32 - minX + offset) + (maxY - y % 32 + offset) * sizeX) * tileSize;
                ptr = off * 2;
                for (int j = 0; j < 16; j++)
                {
                    Array.Copy(tile.Value, 480 - j * 32, memory, ptr, 32);
                    ptr = ptr + sizeX * 2;
                }
            }

            // Stride = Width * BytesPerPixel
            BitmapSource img = BitmapImage.Create(sizeX, sizeY, 96.0, 96.0, PixelFormats.Bgr555, null, memory, 1024);
            m_Image = img;

            return img;
        }

        #region IComparable<RegionFileViewModel> Member

        /// <summary>
        /// Vergleicht das aktuelle Objekt mit einem anderen Objekt desselben Typs.
        /// </summary>
        /// <param name="other">Ein Objekt, das mit diesem Objekt verglichen werden soll.</param>
        /// <returns>Ein Wert, der die relative Reihenfolge der verglichenen Objekte angibt. 
        /// Der Rückgabewert hat folgende Bedeutung:
        /// Kleiner als 0        : Dieses Objekt ist kleiner als der other-Parameter.
        /// Zero                 : Dieses Objekt ist gleich other.
        /// Größer als  0 (null) : Dieses Objekt ist größer als other.</returns>
        public int CompareTo(RegionFileViewModel other)
        {
            int diff = this.X - other.X;
            if (diff == 0)
            {
                diff = this.Y - other.Y;
            }
            return diff;
        }

        #endregion
    }
}
