using System;
using System.IO;

namespace SevenDaysSaveManipulator.PlayerData
{
    [Serializable]
    public class Waypoint
    {
        public bool bTracked;
        public string icon;
        public string name;
        public Vector3i pos;
        public int ownerId;
        public int entityId;

        public Waypoint()
        {
        }

        internal Waypoint(BinaryReader reader, uint version)
        {
            Read(reader, version);
        }

        internal void Read(BinaryReader reader, uint version)
        {
            pos = new Vector3i
            {
                x = reader.ReadInt32(),
                y = reader.ReadInt32(),
                z = reader.ReadInt32()
            };

            icon = reader.ReadString();
            name = reader.ReadString();
            bTracked = reader.ReadBoolean();

            if (version >= 2)
            {
                ownerId = reader.ReadInt32();
                entityId = reader.ReadInt32();
            }
        }

        internal void Write(BinaryWriter writer, uint version)
        {
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            writer.Write(icon);
            writer.Write(name);
            writer.Write(bTracked);

            if (version >= 2)
            {
                writer.Write(ownerId);
                writer.Write(entityId);
            }
        }
    }
}