﻿using System;
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
        public int ownerId; // pre Alpha 20 b238
        public int entityId;

        uint saveFileVersion;
        private string platformId; // Alpha 20 and cross platform with Epic Games EOS
        private string userId; // Alpha 20 and cross platform with Epic Games EOS

        public Waypoint()
        {
        }

        internal Waypoint(BinaryReader reader, uint version, uint saveFileVersion)
        {
            this.saveFileVersion = saveFileVersion;
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
                if (saveFileVersion < 48) // pre Alpha 20 b238
                {
                    ownerId = reader.ReadInt32();
                }
                else // Alpha 20 and cross platform with Epic Games EOS
                {
                    if (reader.ReadBoolean())
                    {
                        platformId = reader.ReadString();
                        userId = reader.ReadString();
                    }
                }
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
                if (saveFileVersion < 48) // pre Alpha 20 b238
                {
                    writer.Write(ownerId);
                }
                else // Alpha 20 and cross platform with Epic Games EOS
                {
                    if (String.IsNullOrEmpty(platformId))
                    {
                        writer.Write(false);
                    }
                    else
                    {
                        writer.Write(true);
                        writer.Write(true);
                        writer.Write(platformId);
                        writer.Write(userId);
                    }
                }
                writer.Write(entityId);
            }
        }
    }
}