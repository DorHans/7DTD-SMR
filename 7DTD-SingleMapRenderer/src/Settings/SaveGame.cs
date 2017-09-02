using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_SingleMapRenderer.Settings
{
    [DebuggerDisplay("{Name}")]
    [Serializable]
    public class SaveGame
    {
        public string Name { get; set; }
        public string MapfilePath { get; set; }
        public string PoiFilePath { get; set; }

        public SaveGame()
        {
        }

        public SaveGame(string name, string mapfile)
        {
            this.Name = name;
            this.MapfilePath = mapfile;
        }
    }
}
