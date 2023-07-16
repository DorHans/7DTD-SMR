using System;
using System.Collections.Generic;
using System.IO;

namespace SevenDaysSaveManipulator.PlayerData
{
    [Serializable]
    public class WaypointCollection
    {
        private const uint SUPPORTED_VERSION = 4;
        private uint version;

        private uint saveFileVersion;

        public List<Waypoint> waypoints;

        public WaypointCollection()
        {
        }

        internal WaypointCollection(BinaryReader reader, uint saveFileVersion)
        {
            this.saveFileVersion = saveFileVersion;
            Read(reader);
        }

        internal void Read(BinaryReader reader)
        {
            version = reader.ReadByte();
           // Utils.VerifyVersion("WaypointCollection", version, SUPPORTED_VERSION);

            waypoints = new List<Waypoint>();
            ushort waypointListLength = reader.ReadUInt16();
            for (short i = 0; i < waypointListLength; ++i)
            {
                waypoints.Add(new Waypoint(reader, version, saveFileVersion));
            }
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write((byte)version);

            writer.Write((ushort)waypoints.Count);
            foreach (Waypoint waypoint in waypoints)
            {
                waypoint.Write(writer, version);
            }
        }
    }
}