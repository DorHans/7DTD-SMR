using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _7DTD_SingleMapRenderer.Core
{
    [System.Diagnostics.DebuggerDisplay("{Latitude},{Longitude},{Width}x{Height},{Name}")]
    public class PrefabPOI
    {
        private readonly Color penColor;

        /// <summary>
        /// Breitengrad (positiv für Nord, negativ für Süd)
        /// </summary>
        public int Latitude { get; set; }

        /// <summary>
        /// Längengrad (positiv für Ost, negativ für West)
        /// </summary>
        public int Longitude { get; set; }

        /// <summary>
        /// Name des POI oder Waypoints
        /// </summary>
        public string Name { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public PrefabPOI(int latitude, int longitude, int height, int width, string name = null)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Height = height;
            this.Width = width;
            this.Name = name ?? String.Empty;
            this.penColor = Color.FromArgb(200, Color.OrangeRed);
        }

        public Image GetImage(double scale = 1.0)
        {
            int width = (int)(Width * scale);
            if (width < 1)
                width = 1;

            int height = (int)(Height * scale);
            if (height < 1)
                height = 1;

            Bitmap img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int maxX = width - 1;
            int maxY = height - 1;
            using (Graphics g = Graphics.FromImage(img))
            using (var pen = new Pen(penColor, 1))
            {
                g.DrawLine(pen, 0, 0, maxX, 0); // top
                g.DrawLine(pen, 0, 0, 0, maxY); // left
                g.DrawLine(pen, maxX, 0, maxX, maxY); // right
                g.DrawLine(pen, 0, maxY, maxX, maxY); // bottom

                //g.DrawLine(pen, 0, 0, maxX, maxY);  // diagonal down
                //g.DrawLine(pen, 0, maxY, maxX, 0); // diagonal up
            }

            return img;
        }

        public static IEnumerable<PrefabPOI> FromPrefabFile(string prefabFilename, List<SevenDaysSaveManipulator.Data.Prefab> prefabs)
        {
            List<PrefabPOI> pois = new List<PrefabPOI>();

            if (String.IsNullOrEmpty(prefabFilename) || !File.Exists(prefabFilename))
                return pois;

            var doc = new XmlDocument();
            doc.Load(prefabFilename);

            foreach (XmlNode node in doc.SelectNodes("/prefabs/decoration"))
            {
                var nameAttr = node.Attributes["name"]?.Value;
                var posAttr = node.Attributes["position"]?.Value;
                var rotAttr = node.Attributes["rotation"]?.Value;

                if (nameAttr == null || posAttr == null)
                    continue;

                var positionVector = posAttr.Split(',');
                if (positionVector.Length < 3)
                    continue;

                int x, y, z;
                if (!int.TryParse(positionVector[0], out x))
                    continue;
                if (!int.TryParse(positionVector[1], out y))
                    continue;
                if (!int.TryParse(positionVector[2], out z))
                    continue;

                int rotation = 0;
                int.TryParse(rotAttr, out rotation);

                int height = 2, width = 2;
                if (prefabs != null)
                {
                    var prefab = prefabs.FirstOrDefault(p => p.Name == nameAttr);
                    if (prefab != null)
                    {
                        if (rotation % 2 == 0)
                        {
                            x += prefab.SizeX / 2;
                            z += prefab.SizeZ / 2;
                            height = prefab.SizeZ;
                            width = prefab.SizeX;
                        }
                        else
                        {
                            x += prefab.SizeZ / 2;
                            z += prefab.SizeX / 2;
                            height = prefab.SizeX;
                            width = prefab.SizeZ;
                        }
                    }
                }

                var poi = new PrefabPOI(z, x, height, width, nameAttr);
                pois.Add(poi);
                ;
            }

            return pois;
        }

    }
}
