using System;
using System.IO;

namespace IsoLib
{
    public class PVD
    {
        public uint DefaultSectorSize = 0x800;
        public uint LogicBlockSize;
        public DirectoryRecord RootDir;
        public long Address;
        public byte Type;
        public void Parse(FileStream fs)
        {
            Address = fs.Position;


            byte[] pvd = new byte[0x800];
            fs.Read(pvd, 0, 0x800);
            Type = pvd[0];
            var lbs = BitConverter.ToUInt16(pvd, 128);

            fs.Seek(Address + 156, SeekOrigin.Begin);
            RootDir = new DirectoryRecord();

            RootDir.Parse(fs, this);

            LogicBlockSize = lbs;
            fs.Seek(Address + 0x800, SeekOrigin.Begin);

        }

        public void Save(FileStream fs)
        {
            throw new NotImplementedException();
        }
    }
}