using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenDaysSaveManipulator.Data
{
    public class Prefab
    {
        public string Name { get; private set; }
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }

        public Prefab(string name, short x, short y, short z)
        {
            this.Name = name;
            this.SizeX = x;
            this.SizeY = y;
            this.SizeZ = z;
        }

        public static IEnumerable<Prefab> GetPrefabs(string path)
        {
            if (!Directory.Exists(path))
                yield break;

            var files = Directory.GetFiles(path, "*.tts", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string filename = Path.GetFileNameWithoutExtension(file);
                #region tried to skip all really small prefabs, like signs or zoning helpers
                // failed, because I hadn't find an attribute that distinguishes relevant and irrelevant prefabs
                //string path2 = Path.GetDirectoryName(file);
                //string xmlFilename = Path.Combine(path2, filename+".xml");

                //var doc = new System.Xml.XmlDocument();
                //doc.Load(xmlFilename);

                //var zoningNode = doc.SelectSingleNode("/prefab/property[@name='Zoning']");
                //var zoningValueAttribute = zoningNode?.Attributes["value"]?.Value;
                //if (zoningValueAttribute != null)
                //    if (zoningValueAttribute == "none")
                //        continue;
                #endregion

                using (var fs = new FileStream(file, FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    uint magic = br.ReadUInt32();
                    if (magic == 0x00737474)
                    {
                        uint version = br.ReadUInt32();

                        short x = br.ReadInt16();
                        short y = br.ReadInt16();
                        short z = br.ReadInt16();

                        var prefab = new Prefab(filename, x, y, z);
                        yield return prefab;
                    }
                }
            }
            yield break;
        }

    }
}
