using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Middleman.Server.Request;
using Middleman.Server.Response;
using NLog;

namespace Middleman.Server.Connection
{
    public class OutboundConnection : MiddlemanConnection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected static readonly Encoding HeaderEncoding = Encoding.GetEncoding("us-ascii");
        protected TcpClient Connection;
        protected NetworkStream NetworkStream;

        public OutboundConnection(IPEndPoint endPoint)
        {
            Log.Info("Constructing new outbound connection to [{0}].", endPoint);

            RemoteEndPoint = endPoint;
            Connection = new TcpClient();
        }

        public IPEndPoint RemoteEndPoint { get; private set; }

        public override bool IsSecure
        {
            get { return false; }
        }

        public bool IsConnected
        {
            get { return Connection.Connected; }
        }

        public Task OpenAsync()
        {
            return OpenAsync(CancellationToken.None);
        }

        public virtual async Task OpenAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await Connection.ConnectAsync(RemoteEndPoint.Address, RemoteEndPoint.Port);

            NetworkStream = Connection.GetStream();
        }

        public Task WriteRequestAsync(MiddlemanRequest request)
        {
            return WriteRequestAsync(request, CancellationToken.None);
        }

        public async Task WriteRequestAsync(MiddlemanRequest request, CancellationToken ct)
        {
            var writeStream = GetWriteStream();

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, HeaderEncoding) {NewLine = "\r\n"};

            sw.WriteLine("{0} {1} HTTP/1.{2}", request.Method, request.RequestUri, request.ProtocolVersion.Minor);

            for (var i = 0; i < request.Headers.Count; i++)
                sw.WriteLine("{0}: {1}", request.Headers.GetKey(i), request.Headers.Get(i));

            sw.WriteLine();
            sw.Flush();

            byte[] reqHeaderBytes = ms.GetBuffer();
            string reqHeaders = HeaderEncoding.GetString(reqHeaderBytes);

            await writeStream.WriteAsync(reqHeaderBytes, 0, (int)ms.Length, ct).ConfigureAwait(false);

            string reqBody = "";

            if (request.RequestBody != null)
            {
                var rms = new MemoryStream();

                await request.RequestBody.CopyToAsync(rms).ConfigureAwait(false);
                rms.Position = 0;
                reqBody += HeaderEncoding.GetString(rms.ToArray(), 0, (int)rms.Length);
                rms.Position = 0;

                await rms.CopyToAsync(writeStream).ConfigureAwait(false);
            }

            Log.Info(reqHeaders.Trim() + Environment.NewLine + Environment.NewLine + reqBody.Trim());

            await writeStream.FlushAsync(ct).ConfigureAwait(false);
        }

        protected virtual Stream GetWriteStream()
        {
            return NetworkStream;
        }

        protected virtual Stream GetReadStream()
        {
            return NetworkStream;
        }

        public Task<MiddlemanResponse> ReadResponseAsync()
        {
            var parser = new MiddlemanResponseParser();
            return parser.ParseAsync(GetReadStream());
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}