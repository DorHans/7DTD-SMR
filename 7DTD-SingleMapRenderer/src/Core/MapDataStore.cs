using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_SingleMapRenderer.Core
{
    public class MapDataStore
    {
        internal class DataRecord
        {
            private const int VALUELENGTH = 512;

            internal UInt32 Key { get; set; }
            internal byte[] Value { get; set; }
            internal DateTime Timestamp { get; set; }

            internal DataRecord()
            {
                this.Value = new byte[VALUELENGTH];
            }

            internal DataRecord(UInt32 key, byte[] value, DateTime timestamp)
            {
                this.Key = key;
                this.Value = value;
                this.Timestamp = timestamp;
            }

            internal DataRecord(UInt32 key, byte[] value)
            {
                this.Key = key;
                this.Value = value;
                this.Timestamp = DateTime.Now;
            }

            internal void Read(BinaryReader binReader)
            {
                this.Key = binReader.ReadUInt32();
                this.Value = binReader.ReadBytes(VALUELENGTH);
                long date = binReader.ReadInt64();
                this.Timestamp = DateTime.FromBinary(date);
            }

            internal void Write(BinaryWriter binWriter)
            {
                binWriter.Write(this.Key);
                binWriter.Write(this.Value);
                long date = this.Timestamp.ToBinary();
                binWriter.Write(date);
            }
        }

        string datafilename;
        Dictionary<UInt32, DataRecord> data;
        byte version = 1;
        DateTime lastTimestamp;

        public Dictionary<UInt32, byte[]> Data
        {
            get
            {
                var dict = new Dictionary<UInt32, byte[]>(this.data.Count);
                foreach (var item in this.data)
                {
                    dict.Add(item.Value.Key, item.Value.Value);
                }
                return dict;
            }
        }
        public DateTime LastTimestamp { get { return this.lastTimestamp; } }

        public MapDataStore(string filename)
        {
            this.data = new Dictionary<UInt32, DataRecord>();
            this.datafilename = filename;
        }

        public byte[] this[UInt32 index]
        {
            get
            {
                DataRecord rec;
                if (this.data.TryGetValue(index, out rec))
                    return rec.Value;
                return null;
            }
        }

        public void AddOrReplace(UInt32 key, byte[] value, DateTime timestamp)
        {
            DataRecord rec;
            if (this.data.TryGetValue(key, out rec))
            {
                // Replace, if older (I assume this is always true.)
                if (rec.Timestamp < timestamp)
                {
                    rec.Value = value;
                    rec.Timestamp = timestamp;
                }
            }
            else
            {
                // Add
                rec = new DataRecord(key, value, timestamp);
                this.data.Add(key, rec);
            }
        }

        public void Read()
        {
            if (!File.Exists(this.datafilename))
                return;

            using (var fs = new FileStream(this.datafilename, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var bin = new BinaryReader(fs))
            {
                byte ver = bin.ReadByte();

                long date = bin.ReadInt64();
                this.lastTimestamp = DateTime.FromBinary(date);

                while (fs.Position < fs.Length)
                {
                    DataRecord rec = new DataRecord();
                    rec.Read(bin);
                    this.data.Add(rec.Key, rec);
                }
            }
        }

        public void Write()
        {
            foreach (var rec in this.data.Values)
            {
                if (rec.Timestamp > this.lastTimestamp)
                    this.lastTimestamp = rec.Timestamp;
            }

            using (var fs = new FileStream(this.datafilename, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var bin = new BinaryWriter(fs))
            {
                bin.Write(this.version);

                long date = this.lastTimestamp.ToBinary();
                bin.Write(date);

                foreach (var rec in this.data.Values)
                {
                    rec.Write(bin);
                }
            }
        }

    }
}
