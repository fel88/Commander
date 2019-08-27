//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System.Text;

namespace IsoLib.DiscUtils
{
    internal struct PathTableRecord
    {
        public uint LocationOfExtent;
        public ushort ParentDirectoryNumber;
        public string DirectoryIdentifier;

        

        internal int Write(bool byteSwap, Encoding enc, byte[] buffer, int offset)
        {
            int nameBytes = enc.GetByteCount(DirectoryIdentifier);

            buffer[offset + 0] = (byte)nameBytes;
            buffer[offset + 1] = 0; // ExtendedAttributeRecordLength;
            IsoUtilities.ToBytesFromUInt32(buffer, offset + 2, byteSwap ? Utilities.BitSwap(LocationOfExtent) : LocationOfExtent);
            IsoUtilities.ToBytesFromUInt16(buffer, offset + 6, byteSwap ? Utilities.BitSwap(ParentDirectoryNumber) : ParentDirectoryNumber);
            IsoUtilities.WriteString(buffer, offset + 8, nameBytes, false, DirectoryIdentifier, enc);
            if ((nameBytes & 1) == 1)
            {
                buffer[offset + 8 + nameBytes] = 0;
            }

            return (int)(8 + nameBytes + (((nameBytes & 0x1) == 1) ? 1 : 0));
        }
    }
}