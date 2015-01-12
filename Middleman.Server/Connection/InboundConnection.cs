using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Middleman.Server.Request;
using Middleman.Server.Response;
using Middleman.Server.Utils;
using NLog;

namespace Middleman.Server.Connection
{
    public class InboundConnection : MiddlemanConnection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static long _connectionCounter;
        protected static readonly Encoding HeaderEncoding = Encoding.GetEncoding("us-ascii");
        protected TcpClient Connection;
        public long ConnectionId;
        protected NetworkStream NetworkStream;

        [DebuggerHidden]
        public bool IsClientStillConnected()
        {
            bool connected = false;

            if (NetworkStream != null)
            {
                try
                {
                    NetworkStream.Write(null, 0, 0);
                    connected = true;
                }
                catch
                {
                    connected = false;
                }
            }

            return connected;
        }

        public InboundConnection(TcpClient connection)
        {
            Connection = connection;
            NetworkStream = connection.GetStream();
            ConnectionId = Interlocked.Increment(ref _connectionCounter);
            RemoteEndPoint = (IPEndPoint) connection.Client.RemoteEndPoint;
        }

        public override bool IsSecure
        {
            get { return false; }
        }

        public bool IsConnected
        {
            get
            {
                if (!Connection.Connected)
                    return false;

                try
                {
                    return !(Connection.Client.Poll(1, SelectMode.SelectRead) && Connection.Client.Available == 0);
                }
                catch (SocketException)
                {
                    return false;
                }
                //return connection.Connected; 
            }
        }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public virtual Task OpenAsync()
        {
            return OpenAsync(CancellationToken.None);
        }

        public virtual Task OpenAsync(CancellationToken ct)
        {
            return Task.FromResult(default(VoidTypeStruct));
        }

        protected virtual Stream GetReadStream()
        {
            return NetworkStream;
        }

        protected virtual Stream GetWriteStream()
        {
            return NetworkStream;
        }

        public Task<MiddlemanRequest> ReadRequestAsync()
        {
            return ReadRequestAsync(CancellationToken.None);
        }

        public Task<MiddlemanRequest> ReadRequestAsync(CancellationToken ct)
        {
            var requestParser = new MiddlemanRequestParser();

            return requestParser.ParseAsync(this, GetReadStream());
        }

        public async Task WriteResponseAsync(MiddlemanResponse response)
        {
            await WriteResponseAsync(response, CancellationToken.None)
                .ConfigureAwait(false);
        }

        public async Task WriteResponseAsync(MiddlemanResponse response, CancellationToken ct)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, HeaderEncoding) {NewLine = "\r\n"};

            sw.WriteLine("HTTP/{0} {1} {2}", response.ProtocolVersion, response.StatusCode, response.StatusDescription);

            for (var i = 0; i < response.Headers.Count; i++)
                sw.WriteLine("{0}: {1}", response.Headers.GetKey(i), response.Headers.Get(i));

            sw.WriteLine();
            sw.Flush();

            var writeStream = GetWriteStream();

            await writeStream.WriteAsync(ms.GetBuffer(), 0, (int) ms.Length, ct).ConfigureAwait(false);
            Log.Info("{0}: Wrote headers ({1}b)", RemoteEndPoint, ms.Length);

            if (response.ResponseBody != null && response.ResponseBody.CanRead)
            {
                var buffer = new byte[8192];
                int read;
                long written = 0;

                while (
                    (read = await response.ResponseBody.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) >
                    0)
                {
                    written += read;
                    Log.Info("{0}: Read {1:N0} bytes from response body", RemoteEndPoint, read);
                    await writeStream.WriteAsync(buffer, 0, read, ct).ConfigureAwait(false);
                    Log.Info("{0}: Wrote {1:N0} bytes to client", RemoteEndPoint, read);
                }

                Log.Info("{0}: Wrote response body ({1:N0} bytes) to client", RemoteEndPoint, written);
            }

            await writeStream.FlushAsync(ct).ConfigureAwait(false);
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}