using System.IO;
using System.Text;

namespace IsoLib.DiscUtils
{
    internal class FileExtent : BuilderExtent
    {
        private BuildFileInfo _fileInfo;

        private Stream _readStream;

        public FileExtent(BuildFileInfo fileInfo, long start)
            : base(start, fileInfo.GetDataSize(Encoding.ASCII))
        {
            _fileInfo = fileInfo;
        }

        public override void Dispose()
        {
            if (_readStream != null)
            {
                _fileInfo.CloseStream(_readStream);
                _readStream = null;
            }
        }

        internal override void PrepareForRead()
        {
            _readStream = _fileInfo.OpenStream();
        }

        internal override int Read(long diskOffset, byte[] block, int offset, int count)
        {
            long relPos = diskOffset - Start;
            int totalRead = 0;

            // Don't arbitrarily set position, just in case stream implementation is
            // non-seeking, and we're doing sequential reads
            if (_readStream.Position != relPos)
            {
                _readStream.Position = relPos;
            }

            // Read up to EOF
            int numRead = _readStream.Read(block, offset, count);
            totalRead += numRead;
            while (numRead > 0 && totalRead < count)
            {
                numRead = _readStream.Read(block, offset + totalRead, count - totalRead);
                totalRead += numRead;
            }

            return totalRead;
        }

        internal override void DisposeReadState()
        {
            _fileInfo.CloseStream(_readStream);
            _readStream = null;
        }
    }
}