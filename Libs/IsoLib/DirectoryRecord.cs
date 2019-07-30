using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace isoViewer
{
    public class DirectoryRecord
    {
        public static List<DirectoryRecord> GetAllRecords(DirectoryRecord rec, List<DirectoryRecord> records = null)
        {
            if (records == null)
            {
                records = new List<DirectoryRecord>();
            }
            records.Add(rec);
            foreach (var directoryRecord in rec.Records)
            {
                GetAllRecords(directoryRecord, records);
            }

            return records;
        }

        public static void ReadRecursive(FileStream fs, DirectoryRecord rec, PVD pvdd)
        {
            rec.ReadRecords(fs, pvdd);

            foreach (var directoryRecord in rec.Records)
            {
                if (directoryRecord.LBA == rec.LBA) continue;
                if (rec.Parent != null && directoryRecord.LBA == rec.Parent.LBA) continue;
                if (directoryRecord.IsDirectory)
                {
                    ReadRecursive(fs, directoryRecord, pvdd);
                }
                else
                {
                    //directoryRecord.ReadFile(fs, pvdd);
                }
            }

        }


        public DirectoryRecord Parent;
        public uint DataLength;
        public uint LBA;

        public byte LengthName;
        public byte Flags;

        public bool IsDirectory
        {
            get { return (Flags & (2)) > 0; }
        }
        public bool IsFile
        {
            get { return !IsDirectory; }
        }


        public string Name;
        public bool Parse(FileStream fs, PVD pvd)
        {
            var len = fs.ReadByte();
            if (len == 0) return false;
            byte[] ll = new byte[len];
            ll[0] = (byte)len;
            fs.Read(ll, 1, len - 1);

            LBA = BitConverter.ToUInt32(ll, 2);
            DataLength = BitConverter.ToUInt32(ll, 10);
            LengthName = ll[32];
            Flags = ll[25];
            var nm = ll.Skip(33).Take(LengthName).ToArray();
            if (pvd.Type == 2)
            {
                Name = Encoding.BigEndianUnicode.GetString(nm);
            }
            else
            {
                Name = Encoding.UTF8.GetString(nm);
            }
            return true;
        }


        public List<DirectoryRecord> Records = new List<DirectoryRecord>();
        public void ReadRecords(FileStream fs, PVD pvd)
        {
            var address = LBA * pvd.LogicBlockSize;
            fs.Seek(address, SeekOrigin.Begin);


            int last = (int)DataLength;
            while (last > 0)
            {
                var rec = new DirectoryRecord();

                rec.Parent = this;
                var p0 = fs.Position;
                if (!rec.Parse(fs, pvd))
                {
                    break;
                }
                Records.Add(rec);
                var p1 = fs.Position;
                var len = p1 - p0;
                last -= (int)len;
            }

        }

        public byte[] Data;
        public void ReadFile(FileStream fs, PVD pvd)
        {
            var address = LBA * pvd.LogicBlockSize;
            fs.Seek(address, SeekOrigin.Begin);
            Data = new byte[DataLength];
            fs.Read(Data, 0, (int)DataLength);
        }
    }
}