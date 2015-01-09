using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Switchboard.Server.Connection;
using Switchboard.Server.Handlers;

namespace Switchboard.Server.Server
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