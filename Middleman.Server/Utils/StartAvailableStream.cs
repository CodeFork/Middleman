using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Middleman.Server.Utils
{
    /// <summary>
    ///     Used internally by request/response parsers to merge a piece of a buffer and the
    ///     rest of the request/response stream.
    /// </summary>
    internal class StartAvailableStream : Stream
    {
        private readonly MemoryStream _buffer;
        private readonly Stream _stream;
        private bool _inStream;

        public StartAvailableStream(ArraySegment<byte> startBuffer, Stream continuationStream)
            : this(startBuffer.Array, startBuffer.Offset, startBuffer.Count, continuationStream)
        {
        }

        public StartAvailableStream(byte[] startBuffer, int offset, int count, Stream continuationStream)
        {
            _buffer = new MemoryStream(startBuffer, offset, count);
            _stream = continuationStream;
        }

        public override bool CanRead
        {
            get { return !_inStream || _stream.CanRead; }
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
            get { return _buffer.Length + _stream.Length; }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(default(VoidTypeStruct));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_inStream && _buffer.Position == _buffer.Length)
                _inStream = true;

            if (_inStream)
                return _stream.Read(buffer, offset, count);
            return _buffer.Read(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            if (!_inStream && _buffer.Position == _buffer.Length)
                _inStream = true;

            if (_inStream)
            {
                return _stream.BeginRead(buffer, offset, count, callback, state);
            }
            return _buffer.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (_inStream)
            {
                return _stream.EndRead(asyncResult);
            }
            return _buffer.EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (!_inStream && _buffer.Position == _buffer.Length)
                _inStream = true;

            if (_inStream)
                return _stream.ReadAsync(buffer, offset, count, cancellationToken);
            return _buffer.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}