using System;
using System.Collections.Generic;
using System.IO;

namespace SevenDaysSaveManipulator.PlayerData
{
    [Serializable]
    public class Vector3i
    {
        public float heading;
        public int x;
        public int y;
        public int z;
    }

    [Serializable]
    public class Vector3f
    {
        public float heading;
        public float x;
        public float y;
        public float z;
    }
}