using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Middleman.Server.Utils
{
    internal class ChunkedStream : Stream
    {
        private int _chunkHeaderPosition;
        private int _chunkLength;
        private int _chunkRead;
        private int _chunkTrailingCrLfPosition;
        private bool _done;
        private bool _inChunk;
        private bool _inChunkHeader = true;
        private bool _inChunkHeaderLength = true;
        private bool _inChunkTrailingCrLf;
        private readonly Stream _innerStream;

        public ChunkedStream(Stream innerStream)
        {
            _innerStream = innerStream;
        }

        private int ChunkLeft
        {
            get { return _chunkLength - _chunkRead; }
        }

        public override bool CanRead
        {
            get { return !_done; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            throw new NotImplementedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            if (_done)
                return 0;

            count = Math.Min(OptimizeCount(count), count);

            var read = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

            Execute(buffer, offset, read);

            return read;
        }

        private int OptimizeCount(int count)
        {
            if (!_inChunkHeader)
            {
                if (count > ChunkLeft + 2)
                    count = ChunkLeft + 2;
            }
            else
            {
                if (_chunkHeaderPosition == 0)
                    count = 3;
                else
                {
                    if (_inChunkHeaderLength)
                        count = 2;
                    else
                        count = _chunkLength + 3;
                }
            }
            return count;
        }

        private void Execute(byte[] buffer, int offset, int count)
        {
            for (var i = offset; i < offset + count; i++)
            {
                if (_done)
                    break;

                var b = buffer[i];

                if (_inChunkHeader)
                {
                    for (; i < offset + count; i++)
                    {
                        b = buffer[i];

                        if (_inChunkHeaderLength)
                        {
                            if (b == 13)
                                _inChunkHeaderLength = false;
                            else
                                _chunkLength = (_chunkLength << 4) + FromHex(b);

                            _chunkHeaderPosition++;
                        }
                        else
                        {
                            if (b != 10)
                                throw new FormatException("Malformed chunk header");

                            _inChunkHeader = false;
                            _inChunk = true;
                            _chunkHeaderPosition = 0;

                            break;
                        }
                    }
                }
                else if (_inChunkTrailingCrLf)
                {
                    if (_chunkTrailingCrLfPosition == 0 && b != 13 || _chunkTrailingCrLfPosition == 1 && b != 10)
                        throw new FormatException("Malformed chunk header");

                    if (_chunkTrailingCrLfPosition == 1)
                    {
                        _inChunkTrailingCrLf = false;
                        _chunkTrailingCrLfPosition = 0;

                        _inChunkHeader = true;
                        _inChunkHeaderLength = true;

                        if (_chunkLength == 0)
                            _done = true;

                        _chunkLength = 0;
                    }
                    else
                    {
                        _chunkTrailingCrLfPosition++;
                    }
                }
                else if (_inChunk)
                {
                    for (; i < offset + count; i++)
                    {
                        _chunkRead++;

                        if (_chunkRead == _chunkLength)
                        {
                            _inChunk = false;
                            _inChunkTrailingCrLf = true;
                            _chunkRead = 0;
                            break;
                        }
                    }
                }
            }
        }

        private int FromHex(byte b)
        {
            // 0-9
            if (b >= 48 && b <= 57)
                return b - 48;

            // A-F
            if (b >= 65 && b <= 70)
                return 10 + (b - 65);

            // a-f
            if (b >= 97 && b <= 102)
                return 10 + (b - 97);

            throw new FormatException("Not hex");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}