/*
 * Copyright (c) 2008-2009 Markus Olsson
 * var mail = string.Join(".", new string[] {"j", "markus", "olsson"}) + string.Concat('@', "gmail.com");
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this 
 * software and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish, 
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Server.Utils
{
    /// <summary>
    ///     An implementation of a Stream that transparently redirects all
    ///     stream-related method calls to the supplied inner stream. Makes
    ///     it easy to implement the subset of stream functionality required
    ///     for your stream.
    /// </summary>
    internal abstract class RedirectingStream : Stream
    {
        protected readonly Stream InnerStream;

        protected RedirectingStream(Stream innerStream)
        {
            InnerStream = innerStream;
        }

        public override bool CanRead
        {
            get { return InnerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return InnerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return InnerStream.CanWrite; }
        }

        public override long Length
        {
            get { return InnerStream.Length; }
        }

        public override long Position
        {
            get { return InnerStream.Position; }
            set { InnerStream.Position = value; }
        }

        public override bool CanTimeout
        {
            get { return InnerStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return InnerStream.ReadTimeout; }
            set { InnerStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return InnerStream.WriteTimeout; }
            set { InnerStream.WriteTimeout = value; }
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return InnerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            return InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            return InnerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            InnerStream.Close();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            throw new NotSupportedException();
        }

        [Obsolete]
        protected override WaitHandle CreateWaitHandle()
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            InnerStream.Dispose();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return InnerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            InnerStream.EndWrite(asyncResult);
        }

        public override bool Equals(object obj)
        {
            return InnerStream.Equals(obj);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return InnerStream.FlushAsync(cancellationToken);
        }

        public override int GetHashCode()
        {
            return InnerStream.GetHashCode();
        }

        public override object InitializeLifetimeService()
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return InnerStream.ReadByte();
        }

        public override string ToString()
        {
            return InnerStream.ToString();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            InnerStream.WriteByte(value);
        }
    }
}