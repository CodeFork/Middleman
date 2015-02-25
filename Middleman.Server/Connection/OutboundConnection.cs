using System;
using System.IO;
using System.Linq;
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
    public class OutboundConnection : MiddlemanConnection, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected static readonly Encoding HeaderEncoding = Encoding.ASCII;//Encoding.GetEncoding("us-ascii");
        protected TcpClient Connection;
        protected NetworkStream NetworkStream;

        public OutboundConnection(IPEndPoint endPoint)
        {
            Log.Debug("Constructing new outbound connection to [{0}].", endPoint);

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

            var ms = new MemoryStream(0);
            var sw = new StreamWriter(ms, HeaderEncoding) { NewLine = "\r\n" };

            sw.WriteLine("{0} {1} HTTP/1.{2}", request.Method, request.RequestUri, request.ProtocolVersion.Minor);

            for (var i = 0; i < request.Headers.Count; i++)
            {
                var key = request.Headers.GetKey(i);
                var val = request.Headers.Get(i);
                sw.WriteLine("{0}: {1}", key, val);
            }

            sw.WriteLine();
            sw.Flush();

            var reqHeaderBytes = ms.GetBuffer();
            var reqHeaders = HeaderEncoding.GetString(reqHeaderBytes.Where(x => x != 0).ToArray());

            await writeStream.WriteAsync(reqHeaderBytes, 0, (int)ms.Length, ct).ConfigureAwait(false);

            var reqBody = "";

            if (request.RequestBody != null)
            {
                var rms = new MemoryStream();

                await request.RequestBody.CopyToAsync(rms).ConfigureAwait(false);
                rms.Position = 0;
                var bytes = rms.ToArray();
                reqBody += HeaderEncoding.GetString(bytes.Where(x => x != 0).ToArray(), 0, (int)rms.Length);
                rms.Position = 0;

                await rms.CopyToAsync(writeStream).ConfigureAwait(false);
            }

            Log.Info("FORWARDED REQUEST: " + Environment.NewLine + (reqHeaders + reqBody).Trim() + Environment.NewLine);

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
        private bool disposed = false;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (Connection != null)
                    {
                        Connection.Close();
                        Connection = null;
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;

            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~OutboundConnection()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

    }
}