using _7DTD_SingleMapRenderer.Core;
using _7DTD_SingleMapRenderer.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace _7DTD_SingleMapRenderer.Tools.GenWorldsViewer
{
    public class GeneratedWorld : INotifyPropertyChanged
    {
        private readonly AppSettings settings;

        private string m_Name;
        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    onPropertyChanged("Name");
                }
            }
        }

        private string m_Fullpath;
        public string Fullpath
        {
            get { return m_Fullpath; }
            set
            {
                if (m_Fullpath != value)
                {
                    m_Fullpath = value;
                    onPropertyChanged("Fullpath");
                }
            }
        }

        private BitmapSource m_Preview;
        public BitmapSource Preview
        {
            get { return m_Preview; }
            set
            {
                if (m_Preview != value)
                {
                    m_Preview = value;
                    onPropertyChanged("Preview");
                }
            }
        }

        private Bitmap m_WholeMap;
        public Bitmap WholeMap
        {
            get { return m_WholeMap; }
            set
            {
                if (m_WholeMap != value)
                {
                    m_WholeMap = value;
                    onPropertyChanged("WholeMap");
                }
            }
        }


        public GeneratedWorld(AppSettings settings, string folderpath, List<SevenDaysSaveManipulator.Data.Prefab> prefabs)
        {
            this.Name = System.IO.Path.GetFileName(folderpath);
            this.Fullpath = folderpath;
            this.settings = settings;

            IEnumerable<PrefabPOI> prefabPois;
            int height, width;
            try
            {
                getMapInfo(folderpath, out height, out width);
                getPrefabPois(folderpath, prefabs, out prefabPois);

                this.WholeMap = renderGeneratedWorld(folderpath, height, width);
                this.Preview = renderPreview(512, 512);

                this.WholeMap.Save("Generated_Worlds\\" + this.Name + "_Preview.png");
            }
            catch (Exception ex)
            {
                this.Name += " - error while loading";
            }
        }

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void onPropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion


        private static void getMapInfo(string worldFolderPath, out int height, out int width)
        {
            height = 0;
            width = 0;

            // get map_info -> size
            string map_infoFilename = System.IO.Path.Combine(worldFolderPath, "map_info.xml");
            var doc = new System.Xml.XmlDocument();
            doc.Load(map_infoFilename);

            int scale = 1;
            var scaleNode = doc.SelectSingleNode("/MapInfo/property[@name='Scale']");
            var scaleValueAttribute = scaleNode?.Attributes["value"]?.Value;
            if (scaleValueAttribute != null)
                int.TryParse(scaleValueAttribute, out scale);

            var heightmapNode = doc.SelectSingleNode("/MapInfo/property[@name='HeightMapSize']");
            var valueAttribute = heightmapNode.Attributes["value"]?.Value;
            if (valueAttribute != null)
            {
                string[] vs = valueAttribute.Split(',');
                if (vs.Length >= 2)
                {
                    int.TryParse(vs[0], out width);  // x
                    int.TryParse(vs[1], out height); // z
                }
            }
            width *= scale;
            height *= scale;
        }

        private static void getPrefabPois(string worldFolderPath, List<SevenDaysSaveManipulator.Data.Prefab> prefabs, out IEnumerable<PrefabPOI> prefabPois)
        {
            prefabPois = null;

            string prefabFilename = System.IO.Path.Combine(worldFolderPath, "prefabs.xml");
            prefabPois = PrefabPOI.FromPrefabFile(prefabFilename, prefabs);
        }

        private Bitmap renderGeneratedWorld(string worldPath, int sizeX, int sizeY)
        {
            Bitmap big_tile = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            renderBiomeMap(big_tile, worldPath);

            return big_tile;
        }

        private BitmapSource renderPreview(int sizeX = 64, int sizeY = 64)
        {
            Bitmap big_tile = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            using (Graphics g = Graphics.FromImage(big_tile))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(this.WholeMap, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
            }

            return BmpImageFromBmp(big_tile);
        }

        private static BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private void renderBiomeMap(Bitmap big_tile, string worldFolderPath)
        {
            if (String.IsNullOrEmpty(worldFolderPath))
                return;

            using (Graphics g = Graphics.FromImage(big_tile))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                // biome map
                string biomeMap = Path.Combine(worldFolderPath, "biomes.png");
                if (File.Exists(biomeMap))
                {
                    using (Image img = Image.FromFile(biomeMap))
                    {
                        g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                    }
                }

                // splat 3 is a map of the roads
                string splat3 = Path.Combine(worldFolderPath, "splat3_processed.png");
                if (!File.Exists(splat3))
                    splat3 = Path.Combine(worldFolderPath, "splat3.png");
                if (File.Exists(splat3))
                {
                    renderPng(big_tile, g, splat3, 255);
                }

                // splat 4 is a map of available water
                string splat4 = Path.Combine(worldFolderPath, "splat4_processed.png");
                if (!File.Exists(splat4))
                    splat4 = Path.Combine(worldFolderPath, "splat4.png");
                if (File.Exists(splat4))
                {
                    renderPng(big_tile, g, splat4, 170, true); // ~ 67%
                }
                else // try tga
                {
                    splat4 = Path.Combine(worldFolderPath, "splat4_processed.tga");
                    if (File.Exists(splat4))
                    {
                        using (var fs = File.Open(splat4, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var bin = new BinaryReader(fs))
                        {
                            // Header
                            byte id = bin.ReadByte();
                            byte palType = bin.ReadByte();
                            byte pixelformat = bin.ReadByte();
                            short palStart = bin.ReadInt16();
                            short palLength = bin.ReadInt16();
                            byte palEntryLength = bin.ReadByte();
                            short zeroX = bin.ReadInt16();
                            short zeroY = bin.ReadInt16();
                            short width = bin.ReadInt16();
                            short height = bin.ReadInt16();
                            byte bitsPerPixel = bin.ReadByte();
                            byte attribute = bin.ReadByte();
                            // only support: 32 bit uncompressed
                            if (pixelformat == 2 && bitsPerPixel == 32)
                            {
                                using (Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                                {
                                    int totalBytes = width * height * 4;
                                    byte[] pixels = new byte[totalBytes];

                                    for (int i = 0; i < totalBytes; i += 4)
                                    {
                                        pixels[i + 0] = bin.ReadByte();
                                        pixels[i + 1] = bin.ReadByte();
                                        pixels[i + 2] = bin.ReadByte();
                                        pixels[i + 3] = bin.ReadByte();
                                        if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0)
                                        {
                                            pixels[i + 3] = 50;
                                        }
                                    }
                                    Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
                                    BitmapData bmpData = img.LockBits(rect, ImageLockMode.ReadWrite, img.PixelFormat);
                                    System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, totalBytes);

                                    img.UnlockBits(bmpData);
                                    g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                                }
                            }
                        }
                    }
                }

                // deadly radiation zones
                string radiation = Path.Combine(worldFolderPath, "radiation.png");
                if (File.Exists(radiation))
                {
                    renderPng(big_tile, g, radiation, 170); // ~ 67%
                }
            }
        }

        private static void renderPng(Bitmap big_tile, Graphics g, string pngFilepath, byte alphaValue = 255, bool switchChannels = false)
        {
            using (Bitmap img = (Bitmap)Bitmap.FromFile(pngFilepath))
            {
                int pixelformatsize = Image.GetPixelFormatSize(img.PixelFormat);
                if (pixelformatsize == 32)
                {
                    Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
                    BitmapData bmpData = img.LockBits(rect, ImageLockMode.ReadWrite, img.PixelFormat);

                    int totalBytes = img.Width * img.Height * 4;
                    byte[] pixels = new byte[totalBytes];
                    System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, totalBytes);

                    for (int i = 0; i < totalBytes; i += 4)
                    {
                        // pixel format 32bit ARGB -> [0]=B, [1]=G, [2]=R, [3]=A
                        if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                            pixels[i + 3] = 0; // transparent
                        else
                        {
                            pixels[i + 3] = alphaValue;

                            if (switchChannels)
                            {
                                // B <> G
                                pixels[i] ^= pixels[i + 1];
                                pixels[i + 1] ^= pixels[i];
                                pixels[i] ^= pixels[i + 1];

                                // G <> R
                                //pixels[i + 1] ^= pixels[i + 2];
                                //pixels[i + 2] ^= pixels[i + 1];
                                //pixels[i + 1] ^= pixels[i + 2];
                            }
                        }
                    }
                    System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, totalBytes);

                    img.UnlockBits(bmpData);
                    g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                }
            }
        }
    }
}
