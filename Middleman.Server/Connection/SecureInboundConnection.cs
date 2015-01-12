using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Middleman.Server.Connection
{
    public class SecureInboundConnection : InboundConnection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly X509Certificate _certificate;

        public SecureInboundConnection(TcpClient client, X509Certificate certificate)
            : base(client)
        {
            _certificate = certificate;
        }

        protected SslStream SslStream { get; private set; }

        public override bool IsSecure
        {
            get { return true; }
        }

        public override async Task OpenAsync(CancellationToken ct)
        {
            await base.OpenAsync(ct);

            SslStream = CreateSslStream(NetworkStream);

            Log.Info("Authenticating using certificate: {0}.", _certificate == null ? "<NULL>" : _certificate.Subject);

            await SslStream.AuthenticateAsServerAsync(_certificate);

            Log.Info("Authenticated.");
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