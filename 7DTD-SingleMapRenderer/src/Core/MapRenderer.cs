using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using _7DTD_SingleMapRenderer.Services;
using _7DTD_SingleMapRenderer.Settings;

namespace _7DTD_SingleMapRenderer.Core
{
    /// <summary>
    /// Bietet Methoden an, um eine *.map-Datei in eine *.png zu konvertieren.
    /// Portiert von: https://github.com/nicolas-f/7DTD-leaflet/blob/master/map_reader.py
    /// erweitert um einige Funktionen
    /// </summary>
    public class MapRenderer
    {
        private AppSettings m_Settings;

        public MapRenderer(AppSettings settings)
        {
            this.m_Settings = settings;
        }

        /// <summary>
        /// Exportiert die gesamte Map als png-Datei.
        /// Die Pfade müssen korrekt sein, sonst gibt es eine Exception.
        /// </summary>
        /// <param name="mapfilename">Vollqualifizierter Pfad zur Map-Datei</param>
        /// <param name="pngfilename">Vollqualifizierter Pfad zur png-Datei</param>
        /// <param name="progress">ProgressService zur Anzeige des Fortschritts</param>
        public void RenderWholeMap(string mapfilename, string pngfilename, IEnumerable<POI> pois, IEnumerable<PrefabPOI> prefabPois, string worldFolderPath = "", int height = 0, int width = 0, IProgressService progress = null)
        {
            int tileSize = (int)m_Settings.SelectedTileSize;

            if (progress != null)
                progress.Report("Process started. Processing map file . . .");

            // die map-Datei einlesen
            Dictionary<UInt32, byte[]> tiles = null;
            if (m_Settings.UseDataStore)
                tiles = getAllTiles(mapfilename);
            else
                tiles = getAllTilesFromMapFile(mapfilename);
            if (tiles == null)
                throw new Exception("Couldn't load map file.");

            if (progress != null)
                progress.Report("Map file processed. Generating image . . .");

            // Größenabschätzung
            int maxX = int.MinValue, maxY = int.MinValue;
            int minX = int.MaxValue, minY = int.MaxValue;
            int sizeX, sizeY;
            if (height > 0 && width > 0)
            {
                sizeX = (int)(width * (tileSize / 16.0));
                sizeY = (int)(height * (tileSize / 16.0));

                minX = -sizeX / 2 / tileSize + 1;
                maxX = sizeX / 2 / tileSize - 2;

                minY = -sizeY / 2 / tileSize + 1;
                maxY = sizeY / 2 / tileSize - 2;
            }
            else
            {
                foreach (var key in tiles.Keys)
                {
                    int x = (Int16)(key & 0xFFFF);
                    int y = (Int16)((key >> 16));

                    if (x > maxX)
                        maxX = x;
                    if (y > maxY)
                        maxY = y;
                    if (x < minX)
                        minX = x;
                    if (y < minY)
                        minY = y;
                }

                // zu der Größe noch 3 Tiles addieren, jeweils eines am Rand und genau eines in der Mitte
                sizeX = (Math.Abs(maxX - minX) + 3) * tileSize;
                sizeY = (Math.Abs(maxY - minY) + 3) * tileSize;
            }


            // Tiles wieder zusammensetzen
            using (Bitmap big_tile = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format16bppRgb555))
            {
                if (m_Settings.RenderBiomeMap)
                {
                    if (progress != null)
                        progress.Report("Rendering biome map . . .");
                    renderBiomeMap(big_tile, worldFolderPath);
                }
                else if (m_Settings.RenderBackground)
                {
                    if (progress != null)
                        progress.Report("Rendering background . . .");
                    renderBackgroundTo(big_tile);
                }

                if (progress != null)
                    progress.Report("Rendering map tiles . . .");
                if (tileSize == 16)
                    renderTilesOptimized(big_tile, tiles, minX, maxX, minY, maxY);
                else
                    renderTiles(big_tile, tiles, maxY, minX);

                if (m_Settings.RenderGrid)
                {
                    if (progress != null)
                        progress.Report("Rendering grid . . .");
                    renderGridTo(big_tile, minX, maxY);

                    if (m_Settings.RenderRegionNumbers)
                        renderRegionNumbersTo(big_tile, minX, maxX, minY, maxY);
                }

                if (m_Settings.RenderWaypoints && pois != null)
                {
                    if (progress != null)
                        progress.Report("Rendering waypoints . . .");
                    renderPOIs(big_tile, pois, minX, maxX, minY, maxY);
                }

                if (m_Settings.RenderWaypoints && prefabPois != null)
                {
                    if (progress != null)
                        progress.Report("Rendering prefabs . . .");
                    renderPrefabPois(big_tile, prefabPois, minX, maxX, minY, maxY);
                }

                if (progress != null)
                    progress.Report("Encoding as PNG . . .");
                big_tile.Save(pngfilename, ImageFormat.Png);
            }

            if (progress != null)
                progress.Report("Map saved.");
        }

        /// <summary>
        /// Schreibt alle Tiles in das Bitmap. Unterstützt mehrere Größen.
        /// </summary>
        private void renderTiles(Bitmap big_tile, Dictionary<UInt32, byte[]> tiles, int maxY, int minX)
        {
            int tileSize = (int)m_Settings.SelectedTileSize;
            using (Graphics g = Graphics.FromImage(big_tile))
            {
                foreach (var tile in tiles)
                {
                    int x = (Int16)(tile.Key & 0xFFFF);
                    int y = (Int16)((tile.Key >> 16));
                    byte[] tile_data = tile.Value;
                    var bitmapSrc = BitmapSource.Create(16, 16, 96D, 96D,
                        PixelFormats.Bgr555, BitmapPalettes.Halftone256,
                        tile_data, 32);
                    var bitmap = BitmapFromSource(bitmapSrc);
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    g.DrawImage(bitmap, (x - minX + 1) * tileSize, (maxY - y + 1) * tileSize, tileSize, tileSize);
                    bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// Schreibt alle Tiles in das Bitmap. Optimierte Methode. Unterstützt nur "Fullsize".
        /// Basiert auf: https://social.msdn.microsoft.com/Forums/vstudio/en-US/de9ee1c9-16d3-4422-a99f-e863041e4c1d/reading-raw-rgba-data-into-a-bitmap
        /// </summary>
        private void renderTilesOptimized(Bitmap big_tile, Dictionary<UInt32, byte[]> tiles, int minX, int maxX, int minY, int maxY)
        {
            int tileSize = (int)m_Settings.SelectedTileSize;
            int sizeX = big_tile.Width;
            int sizeY = big_tile.Height;
            Rectangle rect = new Rectangle(0, 0, sizeX, sizeY);
            BitmapData bmpData = big_tile.LockBits(rect, ImageLockMode.ReadWrite, big_tile.PixelFormat);
            IntPtr basePtr = bmpData.Scan0;
            IntPtr ptr = basePtr;

            foreach (var tile in tiles)
            {
                int x = (Int16)(tile.Key & 0xFFFF);
                int y = (Int16)((tile.Key >> 16));

                if (x > maxX + 1 || x < minX - 1)
                    continue;
                if (y > maxY + 1 || y < minY - 1)
                    continue;

                int offset = ((x - minX + 1) + (maxY - y + 1) * sizeX) * tileSize;
                ptr = basePtr + offset * 2;
                for (int j = 0; j < 16; j++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(tile.Value, 480 - j * 32, ptr, 32);
                    ptr = ptr + sizeX * 2;
                }
            }
            big_tile.UnlockBits(bmpData);
        }

        private void renderBackgroundTo(Bitmap big_tile)
        {
            var sri = Application.GetResourceStream(new Uri("pack://application:,,,/7DTD-SingleMapRenderer;Component/Resources/png/map_background.png"));
            var background = Image.FromStream(sri.Stream);
            using (var brush = new TextureBrush(background, WrapMode.Tile))
            {
                using (var g = Graphics.FromImage(big_tile))
                {
                    g.FillRectangle(brush, 0, 0, big_tile.Width, big_tile.Height);
                }
            }
        }

        private void renderBiomeMap(Bitmap big_tile, string worldFolderPath)
        {
            if (String.IsNullOrEmpty(worldFolderPath))
                return;

            using (Graphics g = Graphics.FromImage(big_tile))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                string biomeMap = Path.Combine(worldFolderPath, "biomes.png");
                if (File.Exists(biomeMap))
                {
                    using (Image img = Image.FromFile(biomeMap))
                    {
                        g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                    }
                }

                string splat3 = Path.Combine(worldFolderPath, "splat3_processed.png");
                if (!File.Exists(splat3))
                    splat3 = Path.Combine(worldFolderPath, "splat3.png");
                if (File.Exists(splat3))
                {
                    using (Bitmap img = (Bitmap)Bitmap.FromFile(splat3))
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
                                if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0)
                                    pixels[i + 3] = 255;
                            }
                            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, totalBytes);

                            img.UnlockBits(bmpData);
                            g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                        }
                    }
                }

                // splat 4 is a map of available water
                string splat4 = Path.Combine(worldFolderPath, "splat4_processed.png");
                if (!File.Exists(splat4))
                    splat4 = Path.Combine(worldFolderPath, "splat4.png");
                if (File.Exists(splat4))
                {
                    using (Bitmap img = (Bitmap)Bitmap.FromFile(splat4))
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
                                if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0)
                                {
                                    pixels[i + 3] = 50;
                                }
                            }
                            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, totalBytes);

                            img.UnlockBits(bmpData);
                            g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                        }
                    }
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

                string radiation = Path.Combine(worldFolderPath, "radiation.png");
                if (File.Exists(radiation))
                {
                    using (Bitmap img = (Bitmap)Bitmap.FromFile(radiation))
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
                                if (pixels[i] == 0 && pixels[i + 1] == 0 && pixels[i + 2] == 0)
                                    pixels[i + 3] = 0;
                                else
                                    pixels[i + 3] = 160;
                            }
                            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpData.Scan0, totalBytes);

                            img.UnlockBits(bmpData);
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g.DrawImage(img, 0.0f, 0.0f, big_tile.Width, big_tile.Height);
                        }
                    }
                }

            }
        }

        private void renderGridTo(Bitmap big_tile, int minX, int maxY)
        {
            int tileSize = (int)m_Settings.SelectedTileSize;
            int gridSize = m_Settings.GridSize;
            System.Drawing.Color gridColor = System.Drawing.Color.FromArgb(this.m_Settings.AlphaValue,
                        System.Drawing.Color.FromName(this.m_Settings.SelectedGridColorName));

            int sizeX = big_tile.Width;
            int sizeY = big_tile.Height;
            int tilegridsize = gridSize * tileSize / 16;
            using (Graphics g = Graphics.FromImage(big_tile))
            using (var pen = new System.Drawing.Pen(gridColor, 1))
            {
                // maxX - minX -> sizeX
                // maxY - minY -> sizeY
                // Mitte ausrechnen (Punkt mit den Koordinaten 0/0)
                // im Bild ist der Nullpunkt oben links
                // im Tile ist der Nullpunkt unten links

                //vertikale Linien
                int mitteX = Math.Abs(minX - 1) * tileSize;
                int offsetX = mitteX % tilegridsize;
                while (offsetX <= sizeX)
                {
                    g.DrawLine(pen, offsetX, 0, offsetX, sizeY);
                    offsetX += tilegridsize;
                }

                //horizontale Linien
                // -14 = -16 (aus tiles.Key) + 1 Rand + 1 Mitte
                int mitteY = Math.Abs(maxY + 2) * tileSize - 1; // minus 1 Pixel, damit das Grid ordentlich sitzt
                int offsetY = mitteY % tilegridsize;
                while (offsetY <= sizeY)
                {
                    g.DrawLine(pen, 0, offsetY, sizeX, offsetY);
                    offsetY += tilegridsize;
                }
                int circleSize = 10;
                var nullpunkt = new System.Drawing.Point((-minX + 1) * tileSize, (maxY + 2) * tileSize);
                g.DrawEllipse(pen, nullpunkt.X - circleSize / 2, nullpunkt.Y - circleSize / 2 - 1, circleSize, circleSize);
            }
        }

        private void renderRegionNumbersTo(Bitmap big_tile, int minX, int maxX, int minY, int maxY)
        {
            int tileSize = (int)m_Settings.SelectedTileSize;
            System.Drawing.Color gridColor = System.Drawing.Color.FromArgb(this.m_Settings.AlphaValue,
                        System.Drawing.Color.FromName(this.m_Settings.SelectedGridColorName));
            string regionFontName = m_Settings.RegionFontName;
            float regionFontEmSize = m_Settings.RegionFontEmSize;

            //int sizeX = big_tile.Width;
            //int sizeY = big_tile.Height;
            int regiongridsize = 512 * tileSize / 16;

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            using (Graphics g = Graphics.FromImage(big_tile))
            using (var drawFont = new System.Drawing.Font(regionFontName, regionFontEmSize))
            using (var drawBrush = new System.Drawing.SolidBrush(gridColor))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                var nullpunkt = new System.Drawing.Point((-minX + 1) * tileSize, (maxY + 2) * tileSize - 1);

                int regionSize = regiongridsize / 16;
                int regionXmin = (int)Math.Round((double)minX / regionSize);
                int regionXmax = (int)Math.Round((double)maxX / regionSize) - 1;
                int regionYmin = (int)Math.Round((double)minY / regionSize);
                int regionYmax = (int)Math.Round((double)maxY / regionSize) - 1;

                for (int rx = regionXmin; rx <= regionXmax; rx++)
                {
                    float x = nullpunkt.X + rx * regiongridsize + regiongridsize / 2;

                    for (int ry = regionYmin; ry <= regionYmax; ry++)
                    {
                        float y = nullpunkt.Y - ry * regiongridsize - regiongridsize / 2;

                        string drawString = String.Format("r.{0}.{1}", rx, ry);
                        g.DrawString(drawString, drawFont, drawBrush, x, y, sf);
                    }
                }
            }
        }

        private void renderPOIs(Bitmap big_tile, IEnumerable<POI> pois, int minX, int maxX, int minY, int maxY)
        {
            if (pois == null)
                throw new ArgumentNullException("pois");

            int tileSize = (int)m_Settings.SelectedTileSize;
            System.Drawing.Color waypointFontColor = System.Drawing.Color.FromName(this.m_Settings.SelectedWaypointFontColorName);
            string waypointFontName = this.m_Settings.WaypointFontName;
            float waypointFontEmSize = this.m_Settings.WaypointFontEmSize;

            int mitteX = (-minX + 1) * tileSize;
            int mitteY = (maxY + 2) * tileSize;

            // Symbole sollen nicht ganz so stark verkleinert werden
            int symbolsize = (int)Math.Ceiling(32 * Math.Sqrt(tileSize / 16.0));

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            using (Graphics g = Graphics.FromImage(big_tile))
            using (var drawFont = new System.Drawing.Font(waypointFontName, waypointFontEmSize))
            using (var drawBrush = new System.Drawing.SolidBrush(waypointFontColor))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                foreach (var poi in pois)
                {
                    // TODO: Ladefehler behandeln oder ignorieren?
                    Image img = poi.GetImage();
                    if (img != null)
                    {
                        int x = mitteX + poi.Longitude * tileSize / 16;
                        int y = mitteY - poi.Latitude * tileSize / 16;
                        g.DrawImage(img, x - symbolsize / 2, y - symbolsize / 2, symbolsize, symbolsize);

                        if (!String.IsNullOrWhiteSpace(poi.Name))
                            g.DrawString(poi.Name, drawFont, drawBrush, x, y + symbolsize, sf);
                    }
                }
            }
        }

        private void renderPrefabPois(Bitmap big_tile, IEnumerable<PrefabPOI> prefabPois, int minX, int maxX, int minY, int maxY)
        {
            if (prefabPois == null)
                throw new ArgumentNullException("prefabPois");

            int tileSize = (int)m_Settings.SelectedTileSize;
            System.Drawing.Color waypointFontColor = System.Drawing.Color.FromName(this.m_Settings.SelectedWaypointFontColorName);
            string waypointFontName = this.m_Settings.WaypointFontName;
            float waypointFontEmSize = this.m_Settings.WaypointFontEmSize;

            int mitteX = (-minX + 1) * tileSize;
            int mitteY = (maxY + 2) * tileSize;
            double scale = tileSize / 16.0;

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            using (Graphics g = Graphics.FromImage(big_tile))
            using (var drawFont = new System.Drawing.Font(waypointFontName, waypointFontEmSize))
            using (var drawBrush = new System.Drawing.SolidBrush(waypointFontColor))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                foreach (var poi in prefabPois)
                {
                    Image img = poi.GetImage(scale);
                    if (img != null)
                    {
                        int x = mitteX + (int)(poi.Longitude * scale);
                        int y = mitteY - (int)(poi.Latitude * scale);
                        int width = (int)(poi.Width * scale);
                        int height = (int)(poi.Height * scale);

                        g.DrawImage(img, x - width / 2, y - height / 2, width, height);

                        if (!String.IsNullOrWhiteSpace(poi.Name))
                        {
                            int textOffsetY = 0; // (int)waypointFontEmSize + height / 2;
                            g.DrawString(poi.Name, drawFont, drawBrush, x, y + textOffsetY, sf);
                        }
                    }
                }
            }
        }

        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          System.Windows.Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// Liest alle Tiles aus einer Map-Datei und liefert ein Dictionary mit Index und RawByteData.
        /// </summary>
        /// <param name="filename">Vollqualifizierter Pfad zur Map-Datei.</param>
        /// <returns>Auflistung mit Index und zugehörigen Rohdaten.</returns>
        private static Dictionary<UInt32, byte[]> getAllTilesFromMapFile(string filename)
        {
            Dictionary<UInt32, byte[]> tiles = null;

            // Tiles aus der Datei lesen
            using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var bin = new BinaryReader(fs))
            {
                // Header: map\0
                bin.ReadBytes(4);
                // Version: 02 00 00 00
                UInt32 version = bin.ReadUInt32();
                long tiles_pos = 524297;
                if (version == 3)
                {
                    int maxCountOfTiles = bin.ReadInt32(); // 524288
                    tiles_pos = maxCountOfTiles * 4 + 16; // index ist Int32 = 4 Bytes, Header ist 16 Bytes groß
                }
                else if (version == 2)
                    tiles_pos = 524300; // für Version 2
                else
                    fs.Seek(5, SeekOrigin.Begin); // alte Version

                UInt32 num = bin.ReadUInt32(); // Anzahl an Tiles in Map
                List<UInt32> tiles_index = new List<UInt32>((int)num);
                for (int i = 0; i < num; i++)
                {
                    tiles_index.Add(bin.ReadUInt32());
                }
                fs.Seek(tiles_pos, SeekOrigin.Begin);
                List<byte[]> tiles_data = new List<byte[]>((int)num);
                for (int i = 0; i < num; i++)
                {
                    var tile_data = bin.ReadBytes(512);
                    tiles_data.Add(tile_data);
                }
                tiles = new Dictionary<UInt32, byte[]>((int)num);
                for (int i = 0; i < num; i++)
                {
                    tiles.Add(tiles_index[i], tiles_data[i]);
                }
            }

            return tiles;
        }

        internal static Dictionary<UInt32, byte[]> getAllTiles(string filename)
        {
            string datastorefilename = filename + ".ds";

            var ds = new MapDataStore(datastorefilename);
            ds.Read();

            FileInfo fi = new FileInfo(filename);
            DateTime filetime = fi.LastWriteTimeUtc;

            if (filetime > ds.LastTimestamp)
            {
                var newData = getAllTilesFromMapFile(filename);
                foreach (var item in newData)
                {
                    ds.AddOrReplace(item.Key, item.Value, filetime);
                }
                ds.Write();
            }

            return ds.Data;
        }
    }
}
