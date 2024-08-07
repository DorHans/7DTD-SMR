﻿using System;
using System.IO;

namespace SevenDaysSaveManipulator.PlayerData
{
    [Serializable]
    public class Waypoint
    {
        public bool isTracked;
        public string icon;
        public string name;
        public Vector3i pos;
        public int ownerId; // pre Alpha 20 b238, reappeared in Release 1.0
        public int entityId;

        uint saveFileVersion;
        private string platformId; // Alpha 20 and cross platform with Epic Games EOS
        private string userId; // Alpha 20 and cross platform with Epic Games EOS

        public bool isHiddenOnCompass;
        public bool isAutoWaypoint;
        public bool usingLocalizationId;

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

            if (version >= 5)
            {
                reader.ReadByte(); // ??
                name = reader.ReadString();
                if (reader.ReadBoolean()) // cross platform with Epic Games EOS
                {
                    reader.ReadByte(); // ??
                    platformId = reader.ReadString();
                    userId = reader.ReadString();
                }
            }
            else // Alpha 21 and before
            {
                name = reader.ReadString();
            }

            isTracked = reader.ReadBoolean();

            if (version >= 3)
            {
                isHiddenOnCompass = reader.ReadBoolean();
            }

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
                        //reader.ReadByte();
                        platformId = reader.ReadString();
                        userId = reader.ReadString();
                    }
                }
                entityId = reader.ReadInt32();
                if (version >= 4)
                {
                    isAutoWaypoint = reader.ReadBoolean();
                    usingLocalizationId = reader.ReadBoolean();
                }
                if (version >= 5)
                {
                    ownerId = reader.ReadInt32();
                }
            }
        }

        internal void Write(BinaryWriter writer, uint version)
        {
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            writer.Write(icon);

            if (version >= 5)
            {
                writer.Write(true);
                writer.Write(name);
                writer.Write(true);
                writer.Write(true);
                writer.Write(platformId);
                writer.Write(userId);
            }
            else
            {
                writer.Write(name);
            }

            writer.Write(isTracked);

            if (version >= 3)
            {
                writer.Write(isHiddenOnCompass);
            }

            if (version >= 2)
            {
                if (saveFileVersion < 48) // pre Alpha 20 b238
                {
                    writer.Write(ownerId);
                }
                else // Alpha 20 and cross platform with Epic Games EOS
                {
                    if (version >= 5)
                    {
                        writer.Write(false);
                    }
                    else
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
                }
                writer.Write(entityId);
                if (version >= 4)
                {
                    writer.Write(isAutoWaypoint);
                    writer.Write(usingLocalizationId);
                }
                if (version >= 5)
                {
                   writer.Write(ownerId);
                }
            }
        }
    }
}