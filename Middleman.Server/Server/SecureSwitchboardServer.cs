using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Middleman.Server.Connection;
using Middleman.Server.Handlers;

namespace Middleman.Server.Server
{
    public class SecureSwitchboardServer : SwitchboardServer
    {
        private readonly X509Certificate _certificate;

        public SecureSwitchboardServer(IPEndPoint endPoint, ISwitchboardRequestHandler handler,
            X509Certificate certificate)
            : base(endPoint, handler)
        {
            _certificate = certificate;
        }

        protected override Task<InboundConnection> CreateInboundConnection(TcpClient client)
        {
            return Task.FromResult<InboundConnection>(new SecureInboundConnection(client, _certificate));
        }
    }
}