using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Server.Utils
{
    /// <summary>
    ///     Simple wrapping stream which prevents reading more than the specified maximum length.
    ///     Also prevents seeking. Support sync and async reads.
    /// </summary>
    internal class MaxReadStream : RedirectingStream
    {
        private int _read;
        private readonly int _maxLength;

        public MaxReadStream(Stream innerStream, int maxLength)
            : base(innerStream)
        {
            _maxLength = maxLength;
        }

        private int Left
        {
            get { return _maxLength - _read; }
        }

        public override bool CanRead
        {
            get { return Left > 0; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var left = Left;

            if (left <= 0)
                return 0;

            if (count > left)
                count = left;

            var c = base.Read(buffer, offset, count);
            _read += c;

            return c;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            var left = Left;

            if (left <= 0)
            {
                var ar = new EmptyAsyncResult {AsyncState = state, AsyncWaitHandle = new ManualResetEvent(true)};

                callback(ar);
                return ar;
            }

            if (count > left)
                count = left;

            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            var left = Left;

            if (left <= 0)
                return 0;

            if (count > left)
                count = left;

            var c = await base.ReadAsync(buffer, offset, count, cancellationToken);

            _read += c;

            return c;
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            var buffer = new byte[bufferSize];

            int c;

            while ((c = await ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await destination.WriteAsync(buffer, 0, c, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult is EmptyAsyncResult)
                return 0;

            var c = base.EndRead(asyncResult);
            _read += c;

            return c;
        }

        public override int ReadByte()
        {
            if (Left > 0)
            {
                _read++;
                return base.ReadByte();
            }
            throw new EndOfStreamException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        private class EmptyAsyncResult : IAsyncResult
        {
            public object AsyncState { get; set; }
            public WaitHandle AsyncWaitHandle { get; set; }

            public bool CompletedSynchronously
            {
                get { return true; }
            }

            public bool IsCompleted
            {
                get { return true; }
            }
        }
    }
}