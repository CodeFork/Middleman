using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Server.Request;
using Switchboard.Server.Response;

namespace Switchboard.Server.Connection
{
    public class OutboundConnection : SwitchboardConnection
    {
        protected static readonly Encoding HeaderEncoding = Encoding.GetEncoding("us-ascii");
        protected TcpClient Connection;
        protected NetworkStream NetworkStream;

        public OutboundConnection(IPEndPoint endPoint)
        {
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

        public Task WriteRequestAsync(SwitchboardRequest request)
        {
            return WriteRequestAsync(request, CancellationToken.None);
        }

        public async Task WriteRequestAsync(SwitchboardRequest request, CancellationToken ct)
        {
            var writeStream = GetWriteStream();

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, HeaderEncoding) {NewLine = "\r\n"};

            sw.WriteLine("{0} {1} HTTP/1.{2}", request.Method, request.RequestUri, request.ProtocolVersion.Minor);

            for (var i = 0; i < request.Headers.Count; i++)
                sw.WriteLine("{0}: {1}", request.Headers.GetKey(i), request.Headers.Get(i));

            sw.WriteLine();
            sw.Flush();

            await writeStream.WriteAsync(ms.GetBuffer(), 0, (int) ms.Length, ct)
                .ConfigureAwait(false);

            if (request.RequestBody != null)
            {
                await request.RequestBody.CopyToAsync(writeStream)
                    .ConfigureAwait(false);
            }

            await writeStream.FlushAsync(ct)
                .ConfigureAwait(false);
        }

        protected virtual Stream GetWriteStream()
        {
            return NetworkStream;
        }

        protected virtual Stream GetReadStream()
        {
            return NetworkStream;
        }

        public Task<SwitchboardResponse> ReadResponseAsync()
        {
            var parser = new SwitchboardResponseParser();
            return parser.ParseAsync(GetReadStream());
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}