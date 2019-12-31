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

            var files = Directory.GetFiles(path, "*.tts");
            foreach (var file in files)
            {
                string filename = Path.GetFileNameWithoutExtension(file);
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
