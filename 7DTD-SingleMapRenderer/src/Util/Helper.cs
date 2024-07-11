using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _7DTD_SingleMapRenderer.Core;
using _7DTD_SingleMapRenderer.Settings;

namespace _7DTD_SingleMapRenderer.Util
{
    public static class Helper
    {

        public static void GetPois(AppSettings settings, out IEnumerable<POI> pois)
        {
            pois = POI.FromCsvFile(settings.PoiFilePath);

            string directory = System.IO.Path.GetDirectoryName(settings.MapFilePath);
            string ttpfilename = System.IO.Path.GetFileNameWithoutExtension(settings.MapFilePath) + ".ttp";
            ttpfilename = System.IO.Path.Combine(directory, ttpfilename);

            var waypoints = POI.FromTtpFile(ttpfilename);
            pois = pois.Concat(waypoints);
        }

        public static void GetPrefabPoisAndMapInfo(AppSettings settings, ref List<SevenDaysSaveManipulator.Data.Prefab> prefabs, out IEnumerable<PrefabPOI> prefabPois, out int height, out int width, out string worldFolderPath)
        {
            string directory = System.IO.Path.GetDirectoryName(settings.MapFilePath);
            prefabPois = null;
            height = 0;
            width = 0;
            worldFolderPath = GetWorldFolderPath(settings, directory);

            if (!String.IsNullOrEmpty(worldFolderPath))
            {
                if (settings.RenderPrefabMarker)
                {
                    if (prefabs == null)
                    {
                        var prefabFolder = System.IO.Path.Combine(settings.GameRootPath, "Data", "Prefabs");
                        if (Directory.Exists(prefabFolder))
                            prefabs = new List<SevenDaysSaveManipulator.Data.Prefab>(
                                SevenDaysSaveManipulator.Data.Prefab.GetPrefabs(prefabFolder)
                                );
                    }

                    string prefabFilename = System.IO.Path.Combine(worldFolderPath, "prefabs.xml");
                    prefabPois = PrefabPOI.FromPrefabFile(prefabFilename, prefabs);
                }

                if (settings.RenderBiomeMap)
                {
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
            }

        }

        public static string GetWorldFolderPath(AppSettings settings, string directory)
        {
            var paths = directory.Split('\\', '/').ToList();
            int firstIndex = paths.IndexOf("7DaysToDie");
            int indexOfMapName = paths.Count >= 3 ? paths.Count - 3 : firstIndex + 2;

            if (firstIndex > 0 && indexOfMapName < paths.Count)
            {
                string mapName = paths[indexOfMapName];

                string directory2 = Path.Combine(settings.GeneratedWorldsFolderPath, mapName);
                if (Directory.Exists(directory2))
                {
                    return directory2;
                }

                var list = new List<string>(paths.Take(firstIndex + 1));
                list.Add("GeneratedWorlds");
                list.Add(mapName);

                directory2 = String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), list);
                if (Directory.Exists(directory2))
                {
                    return directory2;
                }

                directory2 = System.IO.Path.Combine(settings.GameRootPath, "Data", "Worlds", paths[indexOfMapName]);
                if (Directory.Exists(directory2))
                {
                    return directory2;
                }
            }

            return null;
        }

    }
}
