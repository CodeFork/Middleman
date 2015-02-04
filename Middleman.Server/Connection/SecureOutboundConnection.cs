using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Middleman.Server.Connection
{
    public class SecureOutboundConnection : OutboundConnection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public SecureOutboundConnection(string targetHost, IPEndPoint ep)
            : base(ep)
        {
            TargetHost = targetHost;
        }

        public string TargetHost { get; set; }
        protected SslStream SslStream { get; private set; }

        public override bool IsSecure
        {
            get { return true; }
        }

        public override async Task OpenAsync(CancellationToken ct)
        {
            await base.OpenAsync(ct);

            SslStream = CreateSslStream(NetworkStream);

            Log.Debug("Authenticating: {0}.", TargetHost ?? "<NULL>");

            await SslStream.AuthenticateAsClientAsync(TargetHost);

            Log.Debug("Authenticated.");
        }

        protected virtual SslStream CreateSslStream(Stream innerStream)
        {
            return new SslStream(NetworkStream, true);
        }

        protected override Stream GetWriteStream()
        {
            return SslStream;
        }

        protected override Stream GetReadStream()
        {
            return SslStream;
        }
    }
}