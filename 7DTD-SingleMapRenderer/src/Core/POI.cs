using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _7DTD_SingleMapRenderer.Core
{
    [System.Diagnostics.DebuggerDisplay("{Latitude},{Longitude},{SymbolName},{Name}")]
    public class POI
    {
        /// <summary>
        /// Breitengrad (positiv für Nord, negativ für Süd)
        /// </summary>
        public int Latitude { get; set; }

        /// <summary>
        /// Längengrad (positiv für Ost, negativ für West)
        /// </summary>
        public int Longitude { get; set; }

        /// <summary>
        /// Name der Symboldatei (icon)
        /// </summary>
        public string SymbolName { get; set; }

        /// <summary>
        /// Name des POI oder Waypoints
        /// </summary>
        public string Name { get; set; }

        public POI()
        {

        }

        /// <summary>
        /// Erstellt eine neue Kartenmarkierung
        /// </summary>
        /// <param name="latitude">Z: Breitengrad (positiv für Nord, negativ für Süd)</param>
        /// <param name="longitude">X: Längengrad (positiv für Ost, negativ für West)</param>
        /// <param name="symbolname">Name der Symboldatei (icon)</param>
        /// <param name="name">Name des POI oder Waypoints</param>
        public POI(int latitude, int longitude, string symbolname, string name = null)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.SymbolName = symbolname;
            this.Name = name ?? String.Empty;
        }

        public Image GetImage()
        {
            Image img = null;
            // Versuche eine Datei zu laden ...
            string filename = String.Format("Resources\\png\\{0}.png", SymbolName);
            if (File.Exists(filename))
            {
                img = Image.FromFile(filename);
            }
            else
            {
                filename = filename.Replace('\\', '/');
                var sri = Application.GetResourceStream(new Uri("pack://application:,,,/7DTD-SingleMapRenderer;Component/" + filename));
                img = Image.FromStream(sri.Stream);
            }

            return img;
        }

        public static IEnumerable<POI> FromCsvFile(string csvFilename)
        {
            List<POI> pois = new List<POI>();

            if (String.IsNullOrEmpty(csvFilename) || !File.Exists(csvFilename))
                return pois;

            var lines = File.ReadAllLines(csvFilename);

            for (int i = 0, length = lines.Length; i < length; i++)
            {
                string line = lines[i];
                // Kommentare entfernen
                int commentIndex = line.IndexOf(';');
                if (commentIndex > 0)
                    line = line.Substring(0, commentIndex);

                // Zeile in Zellen aufsplitten
                var split = line.Split(',');
                if (split.Count() < 3)
                    continue;

                string s_lat = split[0].Trim();
                string s_lon = split[1].Trim();
                string s_sym = split[2].Trim();
                string s_nam = String.Empty;
                if (split.Count() >= 4)
                    s_nam = split[3].Trim(' ', '\t', '"');

                // versuche die Koordinaten zu parsen
                bool flag = true;
                int i_lat;
                int i_lon;
                flag &= int.TryParse(s_lat, out i_lat);
                flag &= int.TryParse(s_lon, out i_lon);
                if (!flag)
                    continue;

                // POI erzeugen
                var poi = new POI(i_lat, i_lon, s_sym, s_nam);
                pois.Add(poi);
            }

            return pois;
        }

        public static IEnumerable<POI> FromTtpFile(string ttpFilename)
        {
            // Format eines Waypoints
            // Int32 X, Int32 Y, Int32 Z    X=W/E, Y=0, Z=S/N
            // String icon (1 Byte Längenprefix, kein Nullterminator)
            // String name (1 Byte Längenprefix, kein Nullterminator)
            // Boolean tracked (belegt ein Byte)

            List<POI> pois = new List<POI>();

            if (String.IsNullOrEmpty(ttpFilename) || !File.Exists(ttpFilename))
                return pois;
            /*
            try
            {
                var playerfile = new PlayerDataFile(ttpFilename);

                //dropped backpack
                if (playerfile.droppedBackpackPosition.x.Get() != 0 && playerfile.droppedBackpackPosition.z.Get() != 0)
                {
                    var poi1 = new POI(playerfile.droppedBackpackPosition.z.Get(), playerfile.droppedBackpackPosition.x.Get(), "dropped_backpack", "");
                    pois.Add(poi1);
                }

                // spawn point -> bedroll
                if (playerfile.spawnPoints != null && playerfile.spawnPoints.Count > 0)
                {
                    int index = (playerfile.selectedSpawnPointKey.Get() > 0) ? (int)playerfile.selectedSpawnPointKey.Get() : 0;
                    var spawnpoint = playerfile.spawnPoints.ElementAt(index);
                    if (spawnpoint.x.Get() != 0 && spawnpoint.z.Get() != 0)
                    {
                        var poi2 = new POI(spawnpoint.z.Get(), spawnpoint.x.Get(), "spawnposition", "");
                        pois.Add(poi2);
                    }
                }

                // quick map marker
                if (playerfile.markerPosition.x.Get() != 0 && playerfile.markerPosition.z.Get() != 0)
                {
                    var poi3 = new POI(playerfile.markerPosition.z.Get(), playerfile.markerPosition.x.Get(), "quickmarker", "");
                    pois.Add(poi3);
                }

                // waypoints
                var waypoints = playerfile.waypoints.waypointList;
                foreach (var waypoint in waypoints)
                {
                    var poi = new POI(waypoint.pos.z.Get(), waypoint.pos.x.Get(), waypoint.icon.Get(), waypoint.name.Get());
                    pois.Add(poi);
                }
            }
            catch (Exception ex)
            {
                ;
            } // erstmal alle Fehler ignorieren und Map trotzdem rendern, bis ich ein besseres Verfahren zur Fehlermeldung habe (Logging)
            */
            return pois;
        }

    }
}
